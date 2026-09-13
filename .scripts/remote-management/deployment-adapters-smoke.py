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


with tempfile.TemporaryDirectory(prefix='deployment-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    features = ['Deployment', 'RemoteManagement.Mcp', 'CustomSettings', 'Users.CustomUserSettings',
        'AdminMenu', 'DataLocalization', 'Layers', 'Media', 'Placements', 'Queries', 'Roles',
        'Search', 'Shortcodes', 'Sitemaps', 'Tenants.FeatureProfiles', 'Themes', 'Workflows', 'Lucene']
    for feature in features:
        request('api/features/OrchardCore.' + feature + ':enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    empty = ['AdminMenu', 'AllDataTranslations', 'AllLayers', 'AllMediaProfiles', 'OpenIdServer',
        'OpenIdValidation', 'Placements', 'AllQueries', 'AllRoles', 'SearchSettings',
        'AllShortcodeTemplates', 'AllSitemaps', 'AllFeatureProfiles', 'Themes', 'AllWorkflowType', 'AllUsers']
    configurations = {value + 'DeploymentStep': {} for value in empty}
    configurations.update({
        'CustomSettingsDeploymentStep': {'includeAll': True},
        'CustomUserSettingsDeploymentStep': {'includeAll': True},
        'TranslationsDeploymentStep': {'includeAll': True},
        'SiteSettingsDeploymentStep': {'settings': ['SiteName', 'TimeZoneId']},
    })
    for value in ['IndexProfile', 'RebuildIndex', 'ResetIndex', 'LuceneIndex', 'LuceneIndexRebuild', 'LuceneIndexReset']:
        configurations[value + 'DeploymentStep'] = {'includeAll': True}
    types = {value['type']: value for value in pomi('deployment', 'step-types', 'list')}
    for kind in configurations:
        assert kind in types and types[kind]['canConfigure'], kind
        assert pomi('deployment', 'step-types', 'schema', kind)['additionalProperties'] is False, kind
    plan = pomi('deployment', 'plans', 'create', body={'name': name})['plan']
    plan_id = str(plan['id'])
    for i, (kind, values) in enumerate(configurations.items()):
        body = {'id': 'step' + str(i), 'type': kind, 'values': values}
        assert pomi('deployment', 'plans', 'steps', 'add', plan_id, body=body)['changed'], kind
        assert not pomi('deployment', 'plans', 'steps', 'add', plan_id, body=body)['changed'], kind
        pomi('deployment', 'plans', 'steps', 'update', plan_id, body['id'],
            body={'values': {'hiddenValue': 'rejected'}}, status=400)
        if kind in ('IndexProfileDeploymentStep', 'RebuildIndexDeploymentStep', 'ResetIndexDeploymentStep'):
            pomi('deployment', 'plans', 'steps', 'update', plan_id, body['id'],
                body={'values': {'includeAll': False, 'indexNames': []}}, status=400)
    listed = tool('deployment_plans_steps_list', {'query': {'planId': plan['id']}})
    assert len(listed) == len(configurations)
    for client in [None, 'cli-discovery', 'cli-denied']:
        request('api/deployment/step-types', client=client, status=401 if client is None else 403)
    print('PASS:', len(configurations), 'deployment contracts through HTTP, Pomi and MCP; retries, invalid patches and denied access', flush=True)
