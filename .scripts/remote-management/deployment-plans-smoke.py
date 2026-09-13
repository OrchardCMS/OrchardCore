#!/usr/bin/env python3
"""Verify deployment plan and step management through HTTP, Pomi and MCP."""
import json
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
    for feature in ['OrchardCore.Deployment', 'OrchardCore.RemoteManagement.Mcp',
                    'OrchardCore.Features', 'OrchardCore.Templates', 'OrchardCore.AdminTemplates',
                    'OrchardCore.ContentTypes', 'OrchardCore.Media']:
        request('api/features/' + feature + ':enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    route = 'api/deployment/plans'
    for client in [None, 'cli-discovery', 'cli-denied']:
        request(route, client=client, status=401 if client is None else 403)
    types = pomi('deployment', 'step-types', 'list')
    assert any(value['type'] == 'CustomFileDeploymentStep' and value['canConfigure'] for value in types)
    schema = pomi('deployment', 'step-types', 'schema', 'CustomFileDeploymentStep')
    assert schema['properties']['fileContent']['writeOnly']
    tools = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/list', 'params': {}})['result']['tools']
    names = {value['name'] for value in tools}
    assert {'deployment_plans_create', 'deployment_plans_steps_add', 'deployment_plans_steps_order'} <= names
    plan = pomi('deployment', 'plans', 'create', body={'name': name})
    assert plan['changed']
    plan_id = str(plan['plan']['id'])
    by_id = route + '/by-id?id=' + plan_id
    steps_route = route + '/steps?planId=' + plan_id
    try:
        assert not pomi('deployment', 'plans', 'create', body={'name': name})['changed']
        values = {'id': 'file', 'type': 'CustomFileDeploymentStep',
            'values': {'fileName': 'readme.txt', 'fileContent': 'private sample'}}
        added = pomi('deployment', 'plans', 'steps', 'add', plan_id, body=values)
        assert added['changed'] and 'private sample' not in json.dumps(added)
        assert not pomi('deployment', 'plans', 'steps', 'add', plan_id, body=values)['changed']
        pomi('deployment', 'plans', 'steps', 'update', plan_id, 'file',
            body={'values': {'fileName': '../escape.txt'}}, status=400)
        added = tool('deployment_plans_steps_add', {'query': {'planId': int(plan_id)},
            'body': {'id': 'recipe', 'type': 'RecipeFileDeploymentStep', 'values': {'recipeName': name}}})
        assert added['changed']
        assert tool('deployment_plans_steps_order', {'query': {'planId': int(plan_id)},
            'body': {'stepIds': ['recipe', 'file']}})['changed']
        listed = request(steps_route)
        assert [step['id'] for step in listed] == ['recipe', 'file']
        assert listed[1]['values']['fileName'] == 'readme.txt'
        assert 'fileContent' not in listed[1]['values']
        assert pomi('deployment', 'plans', 'update', plan_id, body={'name': name + 'Renamed'})['changed']
        assert pomi('deployment', 'plans', 'show', plan_id)['stepCount'] == 2
        assert request(route + '?search=' + name)['totalCount'] == 1
        configurations = {
            'AllFeaturesDeploymentStep': {'ignoreDisabledFeatures': True},
            'AllTemplatesDeploymentStep': {'exportAsFiles': True},
            'AllAdminTemplatesDeploymentStep': {'exportAsFiles': True},
            'ContentDefinitionDeploymentStep': {'includeAll': True},
            'ReplaceContentDefinitionDeploymentStep': {'includeAll': True},
            'DeleteContentDefinitionDeploymentStep': {'contentTypes': ['DestinationOnly']},
            'MediaDeploymentStep': {'includeAll': True},
        }
        for index, (step_type, configuration) in enumerate(configurations.items()):
            assert any(value['type'] == step_type and value['canConfigure'] for value in types), step_type
            schema = pomi('deployment', 'step-types', 'schema', step_type)
            assert not schema['additionalProperties']
            step_id = 'adapter' + str(index)
            added = pomi('deployment', 'plans', 'steps', 'add', plan_id,
                body={'id': step_id, 'type': step_type, 'values': configuration})
            assert added['changed']
            before = next(value for value in request(steps_route) if value['id'] == step_id)
            for key, value in configuration.items():
                assert before['values'][key] == value
            pomi('deployment', 'plans', 'steps', 'update', plan_id, step_id,
                body={'values': {'unknown': True}}, status=400)
            assert next(value for value in request(steps_route) if value['id'] == step_id) == before
        assert len(tool('deployment_plans_steps_list', {'query': {'planId': int(plan_id)}})) == 9
        print('PASS: seven feature-owned adapters discovered and configured through Pomi, MCP readback, invalid patches preserved', flush=True)
        assert pomi('deployment', 'plans', 'steps', 'delete', plan_id, 'recipe', '--force')['changed']
        assert not pomi('deployment', 'plans', 'steps', 'delete', plan_id, 'recipe', '--force')['changed']
        print('PASS: HTTP permissions, CLI discovery/CRUD/retries, MCP add/order, safe readback and invalid-patch preservation', flush=True)
    finally:
        request(by_id, 'DELETE')
    assert not pomi('deployment', 'plans', 'delete', plan_id, '--force')['changed']
    request(by_id, status=404)
    print('PASS: deployment cleanup and repeatable delete', flush=True)
