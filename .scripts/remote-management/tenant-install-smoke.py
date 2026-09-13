#!/usr/bin/env python3
"""Verify dynamic tenant installation with a disposable Default-tenant fixture."""
import json
import os
from pathlib import Path
import secrets
import subprocess
import sys
import urllib.parse

state_path = Path(sys.argv[1])
state = json.loads(state_path.read_text())
assert urllib.parse.urlparse(state['url']).hostname in ('127.0.0.1', 'localhost', '::1')
wrapper = Path(__file__).with_name('pomi-fixture.py')
password = secrets.token_urlsafe(32) + 'aA1!'
env = {**os.environ, 'OC_INSTALL_TEST_PASSWORD': password}


def pomi(*args, stdin=None, status=None, output='json', client=None, stored=False):
    command = [sys.executable, str(wrapper), str(state_path), *args, '--output', output]
    run_env = {**env, **({'OC_FIXTURE_CLIENT_ID': client} if client else {})}
    if stored:
        run_env['OC_FIXTURE_HUMAN'] = '1'  # Use stored application credentials, without fixture credential overrides.
    result = subprocess.run(command, input=stdin, text=True, capture_output=True, timeout=180, env=run_env)
    assert password not in result.stdout + result.stderr, 'Password leaked into command output'
    if status is not None:
        assert result.returncode != 0, args
        error = json.loads(result.stderr or result.stdout)['error']
        assert error['status'] == status, (args, error)
        return error
    assert result.returncode == 0, (args, result.stderr)
    return json.loads(result.stdout) if output == 'json' and result.stdout.strip() else result.stdout


pomi('context', 'add', 'tenant-install-smoke', state['url'], '--current')
pomi('api', 'refresh', '--force')
help_result = subprocess.run([sys.executable, str(wrapper), str(state_path), 'tenants', 'install', '--help'],
                             text=True, capture_output=True, env=env, timeout=60)
assert help_result.returncode == 0 and '--password-env' in help_result.stdout and '--connection-string-env' in help_result.stdout
assert '--create-context' not in help_result.stdout
before = pomi('context', 'list')
prefix = 'Install' + secrets.token_hex(4)
for mode in ('env', 'stdin'):
    name = prefix + mode
    args = ['tenants', 'install', name, '--request-url-prefix', name.lower(), '--recipe-name', 'Blog' if mode == 'env' else 'Blank',
            '--site-name', 'Installed by CLI', '--user-name', 'admin',
            '--email', 'admin@example.com', '--site-time-zone', 'Europe/Paris']
    args += ['--password-env', 'OC_INSTALL_TEST_PASSWORD'] if mode == 'env' else ['--password-stdin']
    if mode == 'env':
        human = pomi(*args, output='human')
        assert 'Tenant created and initialized successfully.' in human, human
        assert 'tenants enable-remote-management ' + name in human, human
        assert state['url'].rstrip('/') + '/' + name.lower() in human, human
        installed = pomi('tenants', 'show', name)
    else:
        installed = pomi(*args, stdin=password)
    assert installed['state'] == 'Running' and installed['setupUrl'] is None, installed
    assert installed['databaseProvider'] == 'Sqlite', installed
    assert installed['primaryUrl'].rstrip('/') == state['url'].rstrip('/') + '/' + name.lower(), installed
    pomi(*args, stdin=password if mode == 'stdin' else None, status=409)
    assert pomi('tenants', 'show', name)['tenantId'] == installed['tenantId']

# Creation failure must not leave a shell; setup failure must preserve it with recovery details.
bad = prefix + 'Invalid'
args = ['tenants', 'install', bad, '--request-url-prefix', 'invalid/path', '--recipe-name', 'Blank',
        '--database-provider', 'Sqlite', '--site-name', 'Test', '--user-name', 'admin',
        '--email', 'admin@example.com', '--password-env', 'OC_INSTALL_TEST_PASSWORD']
pomi(*args, status=400)
pomi('tenants', 'show', bad, status=404)
args[args.index('invalid/path')] = bad.lower()
args[args.index('Blank')] = 'MissingRecipe'
error = pomi(*args, status=400)
assert error['details']['stage'] == 'setup' and error['details']['tenantName'] == bad, error
assert pomi('tenants', 'show', bad)['state'] == 'Uninitialized'
pomi(*args, status=409)
# The uninitialized tenant can be repaired using the existing commands.
pomi('tenants', 'update', bad, '--stdin', stdin=json.dumps({'requestUrlPrefix': bad.lower(), 'databaseProvider': 'Sqlite', 'recipeName': 'Blank'}))
human = pomi('tenants', 'setup', bad, '--site-name', 'Recovered', '--user-name', 'admin', '--email', 'admin@example.com',
           '--password-env', 'OC_INSTALL_TEST_PASSWORD', output='human')
assert 'successfully' in human
assert pomi('context', 'list') == before, 'Installation unexpectedly changed local contexts'

managed_name = prefix + 'Managed'
managed_args = ['tenants', 'install', managed_name, '--request-url-prefix', managed_name.lower(),
                '--recipe-name', 'Blank', '--site-name', 'Managed', '--user-name', 'admin',
                '--email', 'admin@example.com', '--password-env', 'OC_INSTALL_TEST_PASSWORD',
                '--enable-remote-management']
pomi('api', 'invoke', 'POST', f'api/tenants/{managed_name}:install', '--stdin',
     stdin=json.dumps({'siteName': 'Managed', 'userName': 'admin', 'email': 'admin@example.com',
                       'password': password, 'recipeName': 'Blank', 'requestUrlPrefix': managed_name.lower(),
                       'enableRemoteManagement': True}), client='cli-discovery', status=403)
pomi('tenants', 'show', managed_name, status=404)
managed = pomi(*managed_args)
assert 'clientCredentials' not in managed, managed
managed_context = managed['context']
assert managed_context and managed['clientId'].startswith('pomi-'), managed
pomi('--context', managed_context, 'api', 'invoke', 'GET', 'api/features', stored=True)
assert pomi('context', 'list')['currentContext'] == before['currentContext']
# Existing tenants can opt in later, using a separate application each time.
enabled = pomi('tenants', 'enable-remote-management', name, '--provision-client')
assert 'clientCredentials' not in enabled, enabled
pomi('--context', enabled['context'], 'api', 'invoke', 'GET', 'api/features', stored=True)
again = pomi('tenants', 'enable-remote-management', name, '--provision-client')
assert again['context'] != enabled['context'] and again['clientId'] != enabled['clientId']
pomi('--context', enabled['context'], 'api', 'invoke', 'GET', 'api/features', stored=True)
print('Tenant install smoke passed: dynamic discovery, secret inputs, running tenant URLs, duplicate rejection, validation, partial failure/recovery, optional provisioning, application authentication, unchanged existing contexts.')
