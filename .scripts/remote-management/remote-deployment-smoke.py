#!/usr/bin/env python3
"""Verify deployment plan and step management through HTTP, Pomi and MCP."""
import json
import os
from pathlib import Path
import secrets
import ssl
import subprocess
import sys
import tempfile
import urllib.error
import urllib.parse
import urllib.request

state_path = Path(sys.argv[1])
state = json.loads(state_path.read_text())
base = state['url']
context = ssl.create_default_context(cafile=state.get('certificatePath'))
urllib.request.install_opener(urllib.request.build_opener(urllib.request.HTTPSHandler(context=context)))
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
    env.update(OC_FIXTURE_CONFIG_HOME=str(Path(config_home) / state_path.parent.name), OC_FIXTURE_CLIENT_ID=client)
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


def select(path):
    global state_path, state, base, tokens
    state_path = Path(path)
    state = json.loads(state_path.read_text())
    base = state['url']
    assert urllib.parse.urlparse(base).hostname in ('localhost', '127.0.0.1', '::1')
    tokens = {}
    context = ssl.create_default_context(cafile=state.get('certificatePath'))
    urllib.request.install_opener(urllib.request.build_opener(urllib.request.HTTPSHandler(context=context)))


source_path, target_path = sys.argv[1:3]
key = secrets.token_urlsafe(32)
with tempfile.TemporaryDirectory(prefix='remote-deployment-cli-', dir=state_path.parent) as config_home:
    select(target_path)
    request('api/features/OrchardCore.Deployment.Remote:enable?force=true', 'POST')
    request('api/features/OrchardCore.RemoteManagement.Mcp:enable?force=true', 'POST')
    pomi('context', 'add', name + 'Target', base, '--current')
    pomi('api', 'refresh', '--force')
    client_body = {'clientName': name, 'apiKey': key}
    client = pomi('deployment', 'remote-clients', 'create', body=client_body)
    assert client['changed'] and client['resource']['hasApiKey']
    assert key not in json.dumps(client)
    client_id = client['resource']['id']
    assert not pomi('deployment', 'remote-clients', 'create', body=client_body)['changed']
    assert not pomi('deployment', 'remote-clients', 'update', client_id, body={'clientName': name})['changed']
    pomi('deployment', 'remote-clients', 'create', body={'clientName': name, 'apiKey': 'different'}, status=409)
    target_url = base + 'OrchardCore.Deployment.Remote/ImportRemoteInstance/Import'
    for denied in [None, 'cli-discovery', 'cli-denied']:
        request('api/deployment/remote-clients', client=denied, status=401 if denied is None else 403)
    select(source_path)
    request('api/features/OrchardCore.Deployment.Remote:enable?force=true', 'POST')
    request('api/features/OrchardCore.RemoteManagement.Mcp:enable?force=true', 'POST')
    pomi('context', 'add', name + 'Source', base, '--current')
    pomi('api', 'refresh', '--force')
    assert all(value['id'] != client_id for value in pomi('deployment', 'remote-clients', 'list'))
    body = {'name': name, 'url': target_url, 'clientName': name, 'apiKey': key}
    instance = pomi('deployment', 'remote-instances', 'create', body=body)
    instance_id = instance['resource']['id']
    assert instance['changed'] and key not in json.dumps(instance)
    assert not pomi('deployment', 'remote-instances', 'create', body=body)['changed']
    assert not pomi('deployment', 'remote-instances', 'update', instance_id, body={k:v for k,v in body.items() if k != 'apiKey'})['changed']
    pomi('deployment', 'remote-instances', 'update', instance_id, body={**body, 'url': 'http://example.org/import'}, status=400)
    assert pomi('deployment', 'remote-instances', 'show', instance_id)['url'] == target_url
    assert any(value['id'] == instance_id for value in tool('deployment_targets_list', {}))
    assert key not in json.dumps(pomi('deployment', 'remote-instances', 'list'))
    plan = pomi('deployment', 'plans', 'create', body={'name': name})['plan']
    pomi('deployment', 'plans', 'steps', 'add', str(plan['id']), body={'id': 'settings', 'type': 'JsonRecipeDeploymentStep',
        'values': {'json': json.dumps({'name': 'settings', 'SiteName': name})}})
    request('api/deployment/remote-instances', client='cli-remote-instances')
    request('api/deployment/remote-instances', client='cli-remote-export', status=403)
    request('api/deployment/targets', client='cli-remote-export')
    request('api/deployment/targets/' + instance_id + '/send', 'POST', {'planId': plan['id']}, client='cli-remote-export-no-data', status=403)
    result = pomi('deployment', 'targets', 'send', instance_id, '--force', body={'planId': plan['id']})
    assert result['succeeded']
    select(target_path)
    assert request('api/settings')['siteName'] == name
    select(source_path)
    for denied in [None, 'cli-discovery', 'cli-denied']:
        request('api/deployment/targets', client=denied, status=401 if denied is None else 403)
    assert pomi('deployment', 'remote-instances', 'delete', instance_id, '--force')['changed']
    assert not pomi('deployment', 'remote-instances', 'delete', instance_id, '--force')['changed']
    select(target_path)
    assert pomi('deployment', 'remote-clients', 'delete', client_id, '--force')['changed']
    assert not pomi('deployment', 'remote-clients', 'delete', client_id, '--force')['changed']
    print('PASS: remote client/instance CRUD, redaction, key preservation, HTTP/Pomi/MCP, tenant isolation and actual remote recipe delivery', flush=True)
