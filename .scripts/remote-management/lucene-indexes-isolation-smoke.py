#!/usr/bin/env python3
"""Verify index identity, provider resource and bearer-token isolation between tenants."""
import json
import os
from pathlib import Path
import secrets
import ssl
import stat
import subprocess
import sys
import tempfile
import urllib.parse
import urllib.request
import urllib.error

state_path = Path(sys.argv[1])
state = json.loads(state_path.read_text())
assert urllib.parse.urlparse(state['url']).hostname in ('127.0.0.1', 'localhost', '::1')
root = Path(tempfile.mkdtemp(prefix='lucene-index-isolation-', dir=state_path.parent))
os.chmod(root, 0o700)
wrapper = Path(__file__).with_name('pomi-fixture.py')
password = secrets.token_urlsafe(32) + 'aA1!'
name = 'IndexTenant' + secrets.token_hex(4)
ssl_context = ssl.create_default_context(cafile=state['certificatePath']) if state.get('certificatePath') else None
calls = []


def pomi(*args, body=None, parent=False, context=None, status=None):
    assert 'login' not in args
    calls.append(args)
    env = {**os.environ, 'OC_FIXTURE_CONFIG_HOME': str(root / 'contexts'), 'OC_COMPOSITION_PASSWORD': password}
    if parent:
        env.pop('OC_FIXTURE_HUMAN', None)
    else:
        env['OC_FIXTURE_HUMAN'] = '1'  # Stored client credentials only; no fixture client override.
    command = [sys.executable, str(wrapper), str(state_path)]
    if context:
        command += ['--context', context]
    command += [*args, '--output', 'json']
    if body is not None:
        command.append('--stdin')
    result = subprocess.run(command, input=json.dumps(body) if body is not None else None,
        env=env, text=True, capture_output=True, timeout=180)
    assert password not in result.stdout + result.stderr, 'Administrator password leaked into command output'
    if status is not None:
        assert result.returncode != 0, 'Expected a denied or unavailable feature'
        assert json.loads(result.stderr or result.stdout)['error']['status'] == status, 'Unexpected error status'
        return
    assert result.returncode == 0, (args[:4], 'CLI request failed')
    return json.loads(result.stdout) if result.stdout.strip() else None


def handoff(tenant):
    path = root / (tenant['tenantId'] + '-administrator.json')
    descriptor = os.open(path, os.O_WRONLY | os.O_CREAT | os.O_EXCL, 0o600)
    with os.fdopen(descriptor, 'w') as output:
        json.dump({'url': tenant['primaryUrl'], 'username': 'admin', 'password': password}, output)
    assert stat.S_IMODE(path.stat().st_mode) == 0o600


pomi('context', 'add', name + '-parent', state['url'], '--current', parent=True)
pomi('features', 'enable', 'OrchardCore.Lucene', '--api-force', 'true', parent=True)
pomi('api', 'refresh', '--force', parent=True)
child = pomi('tenants', 'install', name, '--request-url-prefix', name.lower(), '--recipe-name', 'Blank',
    '--site-name', 'Lucene isolation verification', '--user-name', 'admin', '--email', 'admin@example.test',
    '--database-provider', 'Sqlite', '--password-env', 'OC_COMPOSITION_PASSWORD', '--enable-remote-management', parent=True)
assert child['state'] == 'Running' and child['context']
handoff(child)
context = child['context']
parent_index = None
child_index = None
content_type = name + 'Article'
definition = {'name': name + 'Index', 'indexName': name.lower() + 'index', 'indexedContentTypes': [content_type]}
try:
    pomi('features', 'enable', 'OrchardCore.Lucene', '--api-force', 'true', context=context)
    pomi('api', 'refresh', '--force', context=context)
    for parent in [True, False]:
        pomi('content', 'types', 'create', body={'name': content_type, 'displayName': content_type,
            'settings': {'ContentTypeSettings': {'creatable': True}},
            'parts': [{'name': 'TitlePart', 'partName': 'TitlePart', 'settings': {}}]},
            parent=parent, context=None if parent else context)
    parent_index = pomi('indexes', 'lucene', 'create', body=definition, parent=True)
    child_index = pomi('indexes', 'lucene', 'create', body=definition, context=context)
    assert parent_index['id'] != child_index['id']
    assert parent_index['definition'] == child_index['definition']
    pomi('indexes', 'lucene', 'show', parent_index['id'], context=context, status=404)
    pomi('indexes', 'lucene', 'show', child_index['id'], parent=True, status=404)
    pomi('indexes', 'lucene', 'update', parent_index['id'], body=definition, context=context, status=404)
    pomi('indexes', 'lucene', 'delete', parent_index['id'], '--force', context=context)
    assert pomi('indexes', 'lucene', 'show', parent_index['id'], parent=True) == parent_index
    changed = {**child_index['definition'], 'storeSourceData': True}
    assert pomi('indexes', 'lucene', 'update', child_index['id'], body=changed, context=context)['definition'] == changed
    assert pomi('indexes', 'lucene', 'show', parent_index['id'], parent=True) == parent_index
    token_body = urllib.parse.urlencode({'grant_type': 'client_credentials', 'client_id': state['OC_CLIENT_ID'],
        'client_secret': state['OC_CLIENT_SECRET'], 'scope': 'orchardcore.management'}).encode()
    with urllib.request.urlopen(urllib.request.Request(state['url'] + 'connect/token', data=token_body,
        headers={'Content-Type': 'application/x-www-form-urlencoded'}), context=ssl_context, timeout=30) as response:
        parent_token = json.load(response)['access_token']
    child_url = child['primaryUrl'].rstrip('/') + '/api/indexes'
    assert urllib.parse.urlparse(child_url).hostname in ('127.0.0.1', 'localhost', '::1')
    try:
        response = urllib.request.urlopen(urllib.request.Request(child_url,
            headers={'Authorization': 'Bearer ' + parent_token}), context=ssl_context, timeout=30)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        assert response.status == 401, 'Parent bearer token crossed the tenant boundary'
    pomi('indexes', 'lucene', 'delete', child_index['id'], '--force', context=context)
    child_index = None
    assert pomi('indexes', 'lucene', 'show', parent_index['id'], parent=True) == parent_index
    print('PASS: same-named index resources, foreign IDs, independent mutation/deletion and parent-token isolation', flush=True)
finally:
    if child_index:
        pomi('indexes', 'lucene', 'delete', child_index['id'], '--force', context=context)
    if parent_index:
        pomi('indexes', 'lucene', 'delete', parent_index['id'], '--force', parent=True)
    pomi('content', 'types', 'delete', content_type, '--force', parent=True)
    pomi('tenants', 'stop', name, '--force', parent=True)
    pomi('tenants', 'delete', name, '--force', parent=True)
assert all('login' not in args for args in calls)
print('PASS: automated tenant provisioning used the saved client-credentials context and private administrator handoff')

print('Private administrator handoff: ' + str(root / (child['tenantId'] + '-administrator.json')))
