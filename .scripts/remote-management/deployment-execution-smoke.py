#!/usr/bin/env python3
"""Verify owned artifact upload/download through OAuth, Pomi and MCP metadata."""
import json
import time
import hashlib
import io
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
    headers = {'Content-Type': 'application/octet-stream' if isinstance(body, bytes) else 'application/json', 'Accept': 'application/json, text/event-stream'}
    if client:
        headers['Authorization'] = 'Bearer ' + token(client)
    req = urllib.request.Request(url, method=method, headers=headers,
        data=body if isinstance(body, bytes) else json.dumps(body).encode() if body is not None else None)
    try:
        response = urllib.request.urlopen(req, timeout=60)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        payload = response.read()
        text = payload.decode() if not raw else None
        assert (response.status in status if isinstance(status, tuple) else response.status == status), (method, path, response.status, status)
        if raw:
            return payload, response.headers
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


def select(path):
    global state_path, state, base, tokens
    state_path = Path(path)
    state = json.loads(state_path.read_text())
    base = state['url']
    assert urllib.parse.urlparse(base).hostname in ('127.0.0.1', 'localhost', '::1')
    tokens = {}
    context = ssl.create_default_context(cafile=state.get("certificatePath"))
    urllib.request.install_opener(urllib.request.build_opener(urllib.request.HTTPSHandler(context=context)))


def completed(operation):
    deadline = time.monotonic() + 240
    previous = None
    while time.monotonic() < deadline:
        current = request('api/deployment/operations/' + operation['id'])
        if current['state'] != previous:
            print('Operation', current['kind'], current['state'], flush=True)
            previous = current['state']
        if current['state'] in ('succeeded', 'failed', 'uncertain'):
            assert current['state'] == 'succeeded', (current['state'], current.get('errorCode'))
            return current
        time.sleep(3)
    raise AssertionError('Deployment worker did not finish before test deadline')


source_path, target_path = sys.argv[1:3]
with tempfile.TemporaryDirectory(prefix='deployment-roundtrip-') as config_home:
    select(source_path)
    pomi('context', 'add', 'source', base, '--current')
    for feature in ['OrchardCore.Deployment', 'OrchardCore.RemoteManagement.Mcp']:
        request('api/features/' + feature + ':enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    plan = pomi('deployment', 'plans', 'create', body={'name':name})['plan']
    plan_id = str(plan['id'])
    marker = 'roundtrip-' + secrets.token_hex(8)
    item_id = secrets.token_hex(13)
    media_name = marker + '.svg'
    file_payload = '<svg xmlns="http://www.w3.org/2000/svg"><title>' + marker + '</title></svg>'
    recipe_steps = [
        {'name':'settings', 'SiteName':marker},
        {'name':'ContentDefinition', 'ContentTypes':[{'Name':'DeploymentProbe', 'DisplayName':'Deployment Probe',
            'Settings':{}, 'ContentTypePartDefinitionRecords':[]}]},
        {'name':'Content', 'Data':[{'ContentItemId':item_id, 'ContentType':'DeploymentProbe',
            'DisplayText':marker, 'Published':True, 'Latest':True}]},
        {'name':'media', 'Files':[{'SourcePath':'payload.svg', 'TargetPath':media_name}]},
    ]
    pomi('deployment', 'plans', 'steps', 'add', plan_id,
        body={'id':'file', 'type':'CustomFileDeploymentStep', 'values':{'fileName':'payload.svg', 'fileContent':file_payload}})
    for index, step in enumerate(recipe_steps):
        pomi('deployment', 'plans', 'steps', 'add', plan_id,
            body={'id':'recipe'+str(index), 'type':'JsonRecipeDeploymentStep', 'values':{'json':json.dumps(step)}})
    export_request = {'requestId':marker, 'planId':plan['id']}
    operation = pomi('deployment', 'operations', 'export', body=export_request)
    assert pomi('deployment', 'operations', 'export', body=export_request)['id'] == operation['id']
    exported = completed(operation)
    package = Path(config_home) / 'export.zip'
    pomi('deployment', 'artifacts', 'download', exported['artifactId'], '--output-file', str(package))
    with zipfile.ZipFile(package) as archive:
        assert archive.read('payload.svg').decode() == file_payload
        assert json.loads(archive.read('Recipe.json'))['steps'][0]['SiteName'] == marker
    select(target_path)
    pomi('context', 'add', 'target', base, '--current')
    request('api/features/OrchardCore.Deployment:enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    request('api/deployment/operations/' + exported['id'], status=404)
    request('api/deployment/artifacts/' + exported['artifactId'], status=404)
    uploaded = pomi('deployment', 'artifacts', 'upload', 'export.zip', '--file', str(package))
    imported = pomi('deployment', 'operations', 'import', '--force', body={'requestId':marker, 'artifactId':uploaded['id']})
    completed(imported)
    assert request('api/settings')['siteName'] == marker
    item = request('api/content/' + item_id)
    assert item['DisplayText'] == marker
    data, _ = request('media/' + media_name, raw=True)
    assert data.decode() == file_payload
    assert pomi('deployment', 'operations', 'import', '--force', body={'requestId':marker, 'artifactId':uploaded['id']})['id'] == imported['id']
    print('PASS: cross-tenant queued export/import, retries, settings, content and file bytes', flush=True)
