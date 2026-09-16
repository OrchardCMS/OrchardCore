#!/usr/bin/env python3
"""Verify deployment plan and step management through HTTP, Pomi and MCP."""
import json
import io
import time
import zipfile
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


def completed(operation, client='cli-fixture', expected='succeeded'):
    deadline = time.monotonic() + 180
    while time.monotonic() < deadline:
        value = request('api/deployment/operations/' + operation['id'], client=client)
        if value['state'] in ('succeeded', 'failed', 'uncertain'):
            assert value['state'] == expected, (value['state'], value.get('errorCode'))
            return value
        time.sleep(2)
    raise AssertionError('Queued content export timed out')


with tempfile.TemporaryDirectory(prefix='content-export-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    for feature in ['OrchardCore.Deployment', 'OrchardCore.Contents.Deployment.Download',
            'OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget', 'OrchardCore.RemoteManagement.Mcp']:
        request('api/features/' + feature + ':enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    pomi('content', 'types', 'create', body={'name': name, 'displayName': name,
        'settings': {'ContentTypeSettings': {'draftable': True, 'versionable': True, 'creatable': True}},
        'parts': [{'name': 'TitlePart', 'partName': 'TitlePart'}]})
    item = pomi('content', 'items', 'save', body={'ContentType': name, 'TitlePart': {'Title': 'Published probe'}})
    item_id = item['ContentItemId']
    pomi('content', 'items', 'update-draft', item_id, body={'TitlePart': {'Title': 'Latest probe'}})
    assert pomi('content', 'export', item_id)['TitlePart']['Title'] == 'Published probe'
    assert pomi('content', 'export', item_id, '--latest', 'true')['TitlePart']['Title'] == 'Latest probe'
    assert tool('content_export', {'path': {'contentItemId': item_id}, 'query': {'latest': True}})['TitlePart']['Title'] == 'Latest probe'
    for denied in [None, 'cli-discovery', 'cli-content-export-no-edit']:
        request('api/content/' + item_id + '/export', client=denied, status=401 if denied is None else 403)
    request('api/content/missing-export-probe/export', status=404)
    plan = pomi('deployment', 'plans', 'create', body={'name': name})['plan']
    body = {'id': 'selected', 'type': 'ExportContentToDeploymentTargetDeploymentStep', 'values': {'contentItemIds': [item_id], 'latest': True}}
    assert pomi('deployment', 'plans', 'steps', 'add', str(plan['id']), body=body)['changed']
    assert not pomi('deployment', 'plans', 'steps', 'add', str(plan['id']), body=body)['changed']
    pomi('deployment', 'plans', 'steps', 'update', str(plan['id']), 'selected', body={'values': {'contentItemIds': [item_id, 'missing']}}, status=400)
    denied = request('api/deployment/operations/export', 'POST', {'planId': plan['id'], 'requestId': name + 'Denied'}, client='cli-content-export-no-edit', status=202)
    completed(denied, client='cli-content-export-no-edit', expected='failed')
    operation = request('api/deployment/operations/export', 'POST', {'planId': plan['id'], 'requestId': name}, status=202)
    result = completed(operation)
    path = Path(config_home) / 'selected.zip'
    pomi('deployment', 'artifacts', 'download', result['artifactId'], '--output-file', str(path))
    with zipfile.ZipFile(path) as package:
        recipe = json.loads(package.read('Recipe.json'))
    exported = next(step for step in recipe['steps'] if step['name'] == 'Content')['data']
    assert len(exported) == 1 and exported[0]['ContentItemId'] == item_id
    assert exported[0]['TitlePart']['Title'] == 'Latest probe' and 'Id' not in exported[0]
    if len(sys.argv) > 2:
        state_path = Path(sys.argv[2])
        state = json.loads(state_path.read_text())
        base = state['url']
        assert urllib.parse.urlparse(base).hostname in ('localhost', '127.0.0.1', '::1')
        tokens = {}
        context = ssl.create_default_context(cafile=state.get('certificatePath'))
        urllib.request.install_opener(urllib.request.build_opener(urllib.request.HTTPSHandler(context=context)))
        request('api/features/OrchardCore.Contents.Deployment.Download:enable?force=true', 'POST')
        request('api/content/' + item_id + '/export?latest=true', status=404)
        request('api/deployment/artifacts/' + result['artifactId'], status=404)
    print('PASS: published/latest JSON export through HTTP/Pomi/MCP, shared permissions, explicit selection and queued latest-version archive', flush=True)
