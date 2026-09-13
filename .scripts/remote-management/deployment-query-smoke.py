#!/usr/bin/env python3
"""Verify query-based content deployment configuration and export through Pomi and MCP."""
import json
import os
from pathlib import Path
import secrets
import subprocess
import sys
import tempfile
import time
import zipfile
import urllib.error
import urllib.parse
import urllib.request

state_path = Path(sys.argv[1])
state = json.loads(state_path.read_text())
base = state['url']
assert urllib.parse.urlparse(base).hostname in ('127.0.0.1', 'localhost', '::1')
wrapper = Path(__file__).with_name('pomi-fixture.py')
name = 'DeploymentSmoke' + secrets.token_hex(4)
tokens = {}


def token(client):
    if client not in tokens:
        body = urllib.parse.urlencode(dict(grant_type='client_credentials', client_id=client,
            client_secret=state['OC_CLIENT_SECRET'], scope='orchardcore.management')).encode()
        with urllib.request.urlopen(urllib.request.Request(base + 'connect/token', data=body,
                headers={'Content-Type': 'application/x-www-form-urlencoded'}), timeout=30) as response:
            tokens[client] = json.load(response)['access_token']
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


with tempfile.TemporaryDirectory(prefix='query-deployment-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    for feature in ['OrchardCore.Deployment', 'OrchardCore.RemoteManagement.Mcp', 'OrchardCore.Queries.Sql']:
        request('api/features/' + feature + ':enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    pomi('content', 'types', 'create', body={'name': name, 'displayName': name,
        'settings': {'ContentTypeSettings': {'creatable': True, 'draftable': True}},
        'parts': [{'name': 'TitlePart', 'partName': 'TitlePart'}]})
    item = pomi('content', 'items', 'create-draft', body={'ContentType': name, 'TitlePart': {'Title': name}})
    pomi('content', 'items', 'publish', item['ContentItemId'])
    request('api/queries', 'POST', {'name': name, 'source': 'Sql', 'returnContentItems': True,
        'properties': {'SqlQueryMetadata': {'Template': "SELECT DocumentId FROM ContentItemIndex WHERE ContentType='" + name + "' AND Published = 1 LIMIT @limit:1"}}}, status=201)
    plan = pomi('deployment', 'plans', 'create', body={'name': name})
    plan_id = str(plan['plan']['id'])
    try:
        step_type = 'QueryBasedContentDeploymentStep'
        assert any(value['type'] == step_type and value['canConfigure']
            for value in pomi('deployment', 'step-types', 'list'))
        schema = pomi('deployment', 'step-types', 'schema', step_type)
        assert 'queryParameters' in schema['properties']
        pomi('deployment', 'plans', 'steps', 'add', plan_id, body={'id': 'query', 'type': step_type,
            'values': {'queryName': name, 'queryParameters': '{"limit":1}'}})
        before = pomi('deployment', 'plans', 'steps', 'show', plan_id, 'query')
        for invalid in [{'queryName': name + 'Missing'}, {'queryParameters': 'null'},
                        {'queryParameters': '[]'}, {'queryParameters': '{'}]:
            pomi('deployment', 'plans', 'steps', 'update', plan_id, 'query', body={'values': invalid}, status=400)
            assert pomi('deployment', 'plans', 'steps', 'show', plan_id, 'query') == before
        tool('deployment_plans_steps_update', {'query': {'planId': int(plan_id), 'stepId': 'query'},
            'body': {'values': {'queryParameters': None}}})
        assert pomi('deployment', 'plans', 'steps', 'show', plan_id, 'query')['values']['queryParameters'] is None
        operation = pomi('deployment', 'operations', 'export', body={'requestId': name, 'planId': int(plan_id)})
        deadline = time.monotonic() + 240
        while time.monotonic() < deadline:
            status = pomi('deployment', 'operations', 'show', operation['id'])
            if status['state'] == 'succeeded':
                break
            assert status['state'] in ('pending', 'running'), status['state']
            time.sleep(3)
        else:
            raise AssertionError('query export timed out')
        package = Path(config_home) / 'query.zip'
        pomi('deployment', 'artifacts', 'download', status['artifactId'], '--output-file', str(package))
        with zipfile.ZipFile(package) as archive:
            recipe = json.loads(archive.read('Recipe.json'))
        exported = next(value for value in recipe['steps'] if value['name'] == 'content')['data']
        assert len(exported) == 1 and exported[0]['ContentItemId'] == item['ContentItemId']
        assert exported[0]['TitlePart']['Title'] == name
        print('PASS: Pomi/MCP query adapter, invalid-patch preservation, parameter clearing and queued export through the existing query source', flush=True)
    finally:
        pomi('deployment', 'plans', 'delete', plan_id, '--force')
        request('api/queries/named/' + name, 'DELETE', status=(200, 204))
