#!/usr/bin/env python3
"""Verify tenant rate-limit management through Pomi, MCP, and actual request enforcement."""
import json
import hashlib
from datetime import datetime, timedelta, timezone
import os
from pathlib import Path
import secrets
import subprocess
import sys
import tempfile
import time
import urllib.error
import urllib.parse
import urllib.request

state_path = Path(sys.argv[1])
state = json.loads(state_path.read_text())
base = state['url']
assert urllib.parse.urlparse(base).hostname in ('127.0.0.1', 'localhost', '::1')
wrapper = Path(__file__).with_name('pomi-fixture.py')
name = 'RateLimitSmoke' + secrets.token_hex(4)
tokens = {}
token_expirations = {}
use_stored_credentials = False


def token(client):
    if client not in tokens:
        body = urllib.parse.urlencode(dict(grant_type='client_credentials', client_id=client,
            client_secret=state['OC_CLIENT_SECRET'], scope='orchardcore.management')).encode()
        with urllib.request.urlopen(urllib.request.Request(base + 'connect/token', data=body,
                headers={'Content-Type': 'application/x-www-form-urlencoded'}), timeout=30) as response:
            payload = json.load(response)
            tokens[client] = payload['access_token']
            token_expirations[client] = (datetime.now(timezone.utc) + timedelta(seconds=payload['expires_in'])).isoformat()
    return tokens[client]


def request(path, method='GET', body=None, client='cli-fixture', status=200, raw=False):
    url = urllib.parse.urljoin(base, path)
    assert urllib.parse.urlparse(url).netloc == urllib.parse.urlparse(base).netloc
    headers = {'Content-Type': 'application/json', 'Accept': 'application/json, text/event-stream'}
    if client:
        headers['Authorization'] = 'Bearer ' + token(client)
    req = urllib.request.Request(url, method=method, headers=headers,
        data=json.dumps(body).encode() if body is not None else None)
    try:
        response = urllib.request.urlopen(req, timeout=60)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        text = response.read().decode()
        assert (response.status in status if isinstance(status, tuple) else response.status == status), (method, path, response.status, status)
        if raw:
            return text, response.headers
        if response.headers.get('Content-Type', '').startswith('text/event-stream'):
            return next(json.loads(line[6:]) for line in text.splitlines()
                if line.startswith('data: ') and json.loads(line[6:]).get('id') == 1)
        return json.loads(text) if text and 'json' in response.headers.get('Content-Type', '') else None


def pomi(*args, body=None, client='cli-fixture', status=None, failure=False):
    env = os.environ.copy()
    env.update(OC_FIXTURE_CONFIG_HOME=config_home, OC_FIXTURE_CLIENT_ID=client)
    if use_stored_credentials:
        env['OC_FIXTURE_HUMAN'] = '1'
    selected_state = state_path
    command = [sys.executable, str(wrapper), str(selected_state), *args, '--output', 'json']
    if body is not None:
        command.append('--stdin')
    result = subprocess.run(command, input=json.dumps(body) if body is not None else None,
        capture_output=True, text=True, env=env, timeout=90)
    if failure:
        assert result.returncode != 0, args
        return
    if status:
        assert result.returncode != 0, args
        assert json.loads(result.stderr or result.stdout)['error']['status'] == status, 'unexpected CLI error status'
        return
    if result.returncode != 0:
        try:
            error = json.loads(result.stderr or result.stdout).get('error', {})
            print('CLI failure:', args[:3], error.get('code'), error.get('status'), error.get('message'), flush=True)
        except (ValueError, AttributeError):
            message = result.stderr or result.stdout
            for value in [state.get('OC_CLIENT_SECRET', ''), *tokens.values()]:
                if value:
                    message = message.replace(value, '[redacted]')
            print('CLI diagnostic:', message[:1600], flush=True)
    assert result.returncode == 0, (args[:3], 'CLI request failed')
    return json.loads(result.stdout) if result.stdout.strip() else None


def tool(name, arguments):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': name, 'arguments': arguments}})['result']
    content = json.loads(result['content'][0]['text'])
    assert content['statusCode'] == 200, (name, content['statusCode'])
    return json.loads(content['body']) if content['body'] else None


with tempfile.TemporaryDirectory(prefix='rate-limits-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    # Seed only this private fixture's application-token cache in the same format
    # produced by install provisioning. This avoids fresh token requests for each
    # Pomi process while preserving the built-in 10/minute token endpoint limiter.
    context = next(value for value in json.loads((Path(config_home) / 'contexts.json').read_text())['contexts'] if value['name'] == name)
    identity = '\n'.join([context['name'].upper(), context['tenantUrl'], context.get('authority', ''), context['clientId']])
    key = hashlib.sha256(identity.encode()).hexdigest().upper()
    directory = Path(config_home) / 'credentials'
    directory.mkdir(mode=0o700, exist_ok=True)
    credential_path = directory / (hashlib.sha256(key.encode()).hexdigest() + '.json')
    access_token = token('cli-fixture')
    discovery = request('.well-known/openid-configuration', client=None)
    with os.fdopen(os.open(credential_path, os.O_CREAT | os.O_EXCL | os.O_WRONLY, 0o600), 'w') as stream:
        json.dump({'clientId': 'cli-fixture', 'clientSecret': state['OC_CLIENT_SECRET'],
                   'accessToken': access_token, 'expiresAt': token_expirations['cli-fixture'],
                   'issuer': discovery['issuer'], 'tokenType': 'Bearer'}, stream)
    use_stored_credentials = True
    for feature in ['OrchardCore.RateLimits', 'OrchardCore.RemoteManagement.Mcp']:
        request('api/features/' + feature + ':enable?force=true', 'POST')
    # The seeded 150/minute global limit includes discovery requests. Isolate the
    # selected endpoint policy from that independent fixture limit, then restore it.
    globals_enabled = [value['policyId'] for value in request('api/rate-limits/policies?take=200')
        if value['isEnabled'] and value['definition']['scope'] == 'Global']
    for value in globals_enabled:
        request('api/rate-limits/policies/disable?policyId=' + value, 'POST', status=204)
    policy_id = None
    try:
        pomi('api', 'refresh', '--force')
        types = pomi('rate-limits', 'limiter-types', 'list')
        assert {value['source'] for value in types} == {'FixedWindow', 'SlidingWindow', 'Concurrency', 'TokenBucket'}
        for client, status in [(None, 401), ('cli-denied', 403), ('cli-discovery', 403)]:
            request('api/rate-limits/policies', client=client, status=status)
        request('api/rate-limits/policies', client='cli-rate-limits')
        path = '/__rate_smoke_' + name
        definition = {'name': name, 'description': 'Disposable rate limit', 'scope': 'Endpoint', 'path': path}
        policy = pomi('rate-limits', 'policies', 'create', body=definition)
        policy_id = policy['policyId']
        assert not policy['isEnabled']
        assert pomi('rate-limits', 'policies', 'create', body=definition)['policyId'] == policy_id
        configurations = {
            'FixedWindow': {'permitLimit': 1, 'windowSeconds': 60, 'queueLimit': 0},
            'SlidingWindow': {'permitLimit': 10, 'windowSeconds': 60, 'segmentsPerWindow': 4, 'queueLimit': 0},
            'Concurrency': {'permitLimit': 10, 'queueLimit': 0, 'queueProcessingOrder': 'OldestFirst'},
            'TokenBucket': {'tokenLimit': 10, 'tokensPerPeriod': 10, 'replenishmentPeriodSeconds': 60,
                            'queueLimit': 0, 'queueProcessingOrder': 'NewestFirst'},
        }
        for source, values in configurations.items():
            body = {'id': source, 'source': source, 'values': values}
            added = pomi('rate-limits', 'policies', 'limiters', 'add', policy_id, body=body)
            assert added['values'] == values
            assert pomi('rate-limits', 'policies', 'limiters', 'add', policy_id, body=body) == added
            pomi('rate-limits', 'policies', 'limiters', 'update', policy_id, source,
                 body={**body, 'values': {**values, 'queueLimit': -1}}, status=400)
            assert pomi('rate-limits', 'policies', 'limiters', 'show', policy_id, source) == added
        assert len(tool('rate-limits_policies_limiters_list', {'query': {'policyId': policy_id}})) == 4
        pomi('rate-limits', 'policies', 'enable', policy_id)
        pomi('rate-limits', 'policies', 'enable', policy_id)
        assert pomi('rate-limits', 'policies', 'show', policy_id)['isEnabled']
        request(path, client=None, status=404)
        request(path, client=None, status=429)
        pomi('rate-limits', 'policies', 'update', policy_id, body={**definition, 'path': '/changed'}, status=409)
        pomi('rate-limits', 'policies', 'limiters', 'delete', policy_id, 'FixedWindow', '--force', status=409)
        pomi('rate-limits', 'policies', 'disable', policy_id)
        request(path, client=None, status=404)
        values = {**configurations['FixedWindow'], 'permitLimit': 2}
        result = tool('rate-limits_policies_limiters_update', {'query': {'policyId': policy_id, 'limiterId': 'FixedWindow'},
            'body': {'id': 'FixedWindow', 'source': 'FixedWindow', 'values': values}})
        assert result['values']['permitLimit'] == 2
        for source in configurations:
            pomi('rate-limits', 'policies', 'limiters', 'delete', policy_id, source, '--force')
        assert pomi('rate-limits', 'policies', 'limiters', 'list', policy_id) == []
        print('PASS: four limiter contracts, Pomi/MCP, permission separation, retries, rejected active edits, actual 429 and disable recovery', flush=True)
    finally:
        try:
            if policy_id:
                pomi('rate-limits', 'policies', 'delete', policy_id, '--force')
                pomi('rate-limits', 'policies', 'delete', policy_id, '--force')
        finally:
            for value in globals_enabled:
                request('api/rate-limits/policies/enable?policyId=' + value, 'POST', status=204)
