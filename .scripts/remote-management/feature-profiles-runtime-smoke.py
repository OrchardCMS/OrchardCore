#!/usr/bin/env python3
"""Verify profile assignment and fresh-request rule enforcement on a provisioned child tenant."""
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

state_path = Path(sys.argv[1])
state = json.loads(state_path.read_text())
assert urllib.parse.urlparse(state['url']).hostname in ('127.0.0.1', 'localhost', '::1')
root = Path(tempfile.mkdtemp(prefix='feature-profile-runtime-', dir=state_path.parent))
os.chmod(root, 0o700)
wrapper = Path(__file__).with_name('pomi-fixture.py')
password = secrets.token_urlsafe(32) + 'aA1!'
name = 'ProfileTenant' + secrets.token_hex(4)
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
pomi('features', 'enable', 'OrchardCore.Tenants.FeatureProfiles', parent=True)
pomi('api', 'refresh', '--force', parent=True)
profile_id = name + 'Profile'
definition = {'id': profile_id, 'name': profile_id,
              'featureRules': [{'rule': 'Exclude', 'expression': 'OrchardCore.Markdown'}]}
pomi('tenants', 'feature-profiles', 'create', body=definition, parent=True)
child = pomi('tenants', 'install', name, '--request-url-prefix', name.lower(), '--recipe-name', 'Blank',
    '--site-name', 'Feature profile runtime verification', '--user-name', 'admin', '--email', 'admin@example.test',
    '--database-provider', 'Sqlite', '--password-env', 'OC_COMPOSITION_PASSWORD', '--enable-remote-management', parent=True)
assert child['state'] == 'Running' and child['context']
handoff(child)
context = child['context']
pomi('api', 'refresh', '--force', context=context)
tenant = pomi('tenants', 'show', name, parent=True)
fields = ['requestUrlHost', 'requestUrlPrefix', 'category', 'description', 'databaseProvider',
          'connectionString', 'tablePrefix', 'schema', 'recipeName']
assignment = {key: tenant.get(key) for key in fields}
assignment['featureProfiles'] = [profile_id]
try:
    updated = pomi('tenants', 'update', name, body=assignment, parent=True)
    assert updated['featureProfiles'] == [profile_id]
    # Each command is a new request/process. No tenant reload is requested between rule changes.
    pomi('features', 'show', 'OrchardCore.Markdown', context=context, status=404)
    pomi('features', 'enable', 'OrchardCore.Markdown', context=context, status=404)
    allowed = {**definition, 'featureRules': [*definition['featureRules'],
        {'rule': 'Include', 'expression': 'OrchardCore.Markdown'}]}
    pomi('tenants', 'feature-profiles', 'update', profile_id, body=allowed, parent=True)
    assert pomi('features', 'show', 'OrchardCore.Markdown', context=context)['id'] == 'OrchardCore.Markdown'
    assert pomi('features', 'enable', 'OrchardCore.Markdown', context=context)['isEnabled']
    assert not pomi('features', 'disable', 'OrchardCore.Markdown', '--force', context=context)['isEnabled']
    reversed_rules = {**allowed, 'featureRules': list(reversed(allowed['featureRules']))}
    pomi('tenants', 'feature-profiles', 'update', profile_id, body=reversed_rules, parent=True)
    pomi('features', 'show', 'OrchardCore.Markdown', context=context, status=404)
    dependencies = {**definition, 'featureRules': [{'rule': 'Exclude', 'expression': 'OrchardCore.Shortcodes'}]}
    pomi('tenants', 'feature-profiles', 'update', profile_id, body=dependencies, parent=True)
    pomi('features', 'enable', 'OrchardCore.Markdown', context=context, status=404)
    pomi('tenants', 'feature-profiles', 'delete', profile_id, '--force', parent=True)
    # Missing profiles preserve the existing allow behavior, without rewriting assignment.
    assert pomi('tenants', 'show', name, parent=True)['featureProfiles'] == [profile_id]
    assert pomi('features', 'show', 'OrchardCore.Markdown', context=context)['id'] == 'OrchardCore.Markdown'
finally:
    assignment['featureProfiles'] = []
    pomi('tenants', 'update', name, body=assignment, parent=True)
    pomi('tenants', 'feature-profiles', 'delete', profile_id, '--force', parent=True)
assert all('login' not in args for args in calls)
print('PASS: provisioned context without login, private administrator handoff, profile assignment, rule order, dependencies and fresh-request updates')
print('Private administrator handoff: ' + str(root / (child['tenantId'] + '-administrator.json')))
