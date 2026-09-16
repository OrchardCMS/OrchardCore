#!/usr/bin/env python3
"""Verify owned artifact upload/download through OAuth, Pomi and MCP metadata."""
import json
import hashlib
import io
import zipfile
import os
from pathlib import Path
import secrets
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


with tempfile.TemporaryDirectory(prefix='artifact-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    for feature in ['OrchardCore.Deployment', 'OrchardCore.RemoteManagement.Mcp']:
        request('api/features/' + feature + ':enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    data = io.BytesIO()
    with zipfile.ZipFile(data, 'w') as archive:
        archive.writestr('Recipe.json', '{"steps":[]}')
        archive.writestr('nested/payload.bin', bytes(range(256)))
    payload = data.getvalue()
    package = Path(config_home) / 'package.zip'
    package.write_bytes(payload)
    artifact = None
    try:
        for client in [None, 'cli-discovery', 'cli-denied']:
            request('api/deployment/artifacts?fileName=package.zip', 'POST', payload,
                client=client, status=401 if client is None else 403)
        request('api/deployment/artifacts?fileName=bad.json', 'POST', b'invalid', status=400)
        artifact = pomi('deployment', 'artifacts', 'upload', 'package.zip', '--file', str(package))
        assert artifact['sha256'] == hashlib.sha256(payload).hexdigest()
        assert artifact['length'] == len(payload)
        assert 'owner' not in artifact
        route = 'api/deployment/artifacts/' + artifact['id']
        assert pomi('deployment', 'artifacts', 'show', artifact['id'])['id'] == artifact['id']
        request(route, client='cli-discovery', status=404)
        downloaded, headers = request(route + '/content', raw=True)
        assert downloaded == payload
        output = Path(config_home) / 'download.zip'
        pomi('deployment', 'artifacts', 'download', artifact['id'], '--output-file', str(output))
        assert output.read_bytes() == payload
        pomi('deployment', 'artifacts', 'download', artifact['id'], '--output-file', str(output), failure=True)
        assert output.read_bytes() == payload
        catalog = request('mcp', 'POST', {'jsonrpc':'2.0', 'id':1, 'method':'tools/list'})['result']['tools']
        names = {item['name'] for item in catalog}
        assert 'deployment_artifacts_upload' not in names
        assert 'deployment_artifacts_download' not in names
        assert tool('deployment_artifacts_show', {'path': {'id':artifact['id']}})['id'] == artifact['id']
        assert pomi('deployment', 'artifacts', 'delete', artifact['id'], '--force')['changed']
        artifact = None
        print('Artifact OAuth, Pomi upload/download, ownership and MCP metadata checks passed.', flush=True)
    finally:
        if artifact:
            request('api/deployment/artifacts/' + artifact['id'], 'DELETE')
