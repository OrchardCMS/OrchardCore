#!/usr/bin/env python3
"""Verify automatic API cache refresh after external feature changes on a disposable fixture."""
import json
import os
from pathlib import Path
import subprocess
import sys
import urllib.error
import urllib.parse
import urllib.request

state_path = Path(sys.argv[1])
state = json.loads(state_path.read_text())
assert urllib.parse.urlparse(state['url']).hostname in ('127.0.0.1', 'localhost', '::1')
wrapper = Path(__file__).with_name('pomi-fixture.py')
header = 'OrchardCore-Api-Revision'


def request(path, method='GET', token=None, data=None):
    headers = {'Authorization': 'Bearer ' + token} if token else {}
    req = urllib.request.Request(state['url'] + path, method=method, data=data, headers=headers)
    try:
        with urllib.request.urlopen(req, timeout=60) as response:
            return response.status, response.headers, response.read()
    except urllib.error.HTTPError as error:
        return error.code, error.headers, error.read()


def token(client):
    body = urllib.parse.urlencode({'grant_type': 'client_credentials', 'client_id': client,
                                  'client_secret': state['OC_CLIENT_SECRET'], 'scope': 'orchardcore.management'}).encode()
    status, _, payload = request('connect/token', 'POST', data=body)
    assert status == 200, ('token status', status)
    return json.loads(payload)['access_token']


def pomi(*args):
    result = subprocess.run([sys.executable, str(wrapper), str(state_path), *args, '--output', 'json'],
                            text=True, capture_output=True, env=os.environ.copy(), timeout=90)
    assert result.returncode == 0, (args, result.stderr, result.stdout)
    return json.loads(result.stdout) if result.stdout.strip() else None


def cache():
    files = list(Path(os.environ.get('OC_FIXTURE_CONFIG_HOME', state['OC_CONFIG_HOME'])).rglob('*.json'))
    records = [json.loads(path.read_text()) for path in files]
    return next(record for record in records if record.get('url', '').endswith('/openapi/v1.json') and 'content' in record)


admin = token('cli-fixture')
for auth, expected in [(None, 401), (token('cli-denied'), 403), (admin, 200)]:
    status, headers, body = request('api/management/manifest', 'HEAD', auth)
    assert status == expected, status
    assert not body
    assert headers['Cache-Control'] == 'no-store'
    assert len(headers[header]) == 64
status, headers, _ = request('api/not-an-endpoint', token=admin)
assert status == 404 and len(headers[header]) == 64
pomi('context', 'add', 'revision-smoke', state['url'], '--current')
pomi('api', 'refresh', '--force')
before = cache()
assert 'templates' in before['content']
pomi('features', 'list')
assert cache()['fetchedAt'] == before['fetchedAt'], 'Unchanged revision downloaded OpenAPI again'

# Change the feature directly: this cannot invalidate the CLI's files itself.
feature = 'OrchardCore.Templates'
status, _, body = request('api/features/' + feature + ':disable', 'POST', admin)
assert status == 200, (status, body.decode())
try:
    assert cache()['expiresAt'] == before['expiresAt']
    pomi('features', 'list')
    disabled = cache()
    assert disabled['apiRevision'] != before['apiRevision']
    assert '/api/templates' not in json.loads(disabled['content'])['paths']
finally:
    status, _, body = request('api/features/' + feature + ':enable?force=true', 'POST', admin)
    assert status == 200, (status, body.decode())

# A newly restored command is absent from the current cache, but must parse and execute.
pomi('templates', 'list')
restored = cache()
assert restored['apiRevision'] == before['apiRevision'], 'Equivalent feature sets should have the same revision'
assert '/api/templates' in json.loads(restored['content'])['paths']
print('API revision smoke passed: HEAD authorization, error headers, unchanged cache reuse, external disable/enable, and automatic new command discovery.')
