#!/usr/bin/env python3
"""Verify audit search and task administration through Pomi/MCP and tenant isolation."""
import json
import os
from pathlib import Path
import secrets
import subprocess
import sys
import tempfile
import time
import urllib.error
import urllib.parse
import urllib.request

state_path = Path(sys.argv[1])
state = json.loads(state_path.read_text())
base = state['url']
assert urllib.parse.urlparse(base).hostname in ('127.0.0.1', 'localhost', '::1')
wrapper = Path(__file__).with_name('pomi-fixture.py')
name = 'AuditSmoke' + secrets.token_hex(4)
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


with tempfile.TemporaryDirectory(prefix='audit-task-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    for feature in ['OrchardCore.AuditTrail', 'OrchardCore.Users.AuditTrail', 'OrchardCore.BackgroundTasks', 'OrchardCore.RemoteManagement.Mcp']:
        request('api/features/' + feature + ':enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    for path in ['api/audit-trail/events', 'api/background-tasks']:
        request(path, client=None, status=401)
        request(path, client='cli-discovery', status=403)
    request('api/audit-trail/events', client='cli-audit-trail')
    request('api/background-tasks', client='cli-background-tasks')
    request('api/audit-trail/events', client='cli-background-tasks', status=403)
    request('api/background-tasks', client='cli-audit-trail', status=403)
    user = request('api/users', 'POST', {'userName': name, 'email': name + '@example.test',
        'password': secrets.token_urlsafe(32) + 'aA1!'}, status=201)
    user_id = user['userId']
    task_name = 'OrchardCore.AuditTrail.Services.AuditTrailBackgroundTask'
    before = pomi('background-tasks', 'show', task_name)
    secondary_name = None
    try:
        query = 'id:' + user_id + ' category:User event:Created'
        page = pomi('audit-trail', 'events', 'list', '--q', query, '--page-size', '1')
        assert page['totalCount'] >= 1 and len(page['items']) == 1
        event = page['items'][0]
        assert event['category'] == 'User' and event['name'] == 'Created' and event['correlationId'] == user_id
        shown = pomi('audit-trail', 'events', 'show', event['eventId'])
        assert shown == event
        assert not {'properties', 'clientIpAddress', 'snapshot'} & shown.keys()
        assert tool('audit-trail_events_show', {'query': {'eventId': event['eventId']}}) == shown
        assert pomi('audit-trail', 'events', 'list', '--q', query, '--page', '2', '--page-size', '1')['items'] == []
        pomi('audit-trail', 'events', 'list', '--page-size', '201', status=400)
        pomi('audit-trail', 'events', 'show', 'missing-event', status=404)
        assert any(value['name'] == task_name for value in pomi('background-tasks', 'list'))
        pomi('background-tasks', 'disable', task_name)
        pomi('background-tasks', 'disable', task_name)
        disabled = pomi('background-tasks', 'show', task_name)
        assert not disabled['enabled']
        for invalid in [{'schedule': 'invalid'}, {'lockTimeout': -1}, {'lockExpiration': -1}]:
            configuration = {**disabled['configuration'], **invalid}
            assert not pomi('background-tasks', 'validate', body=configuration)['isValid']
            pomi('background-tasks', 'update', task_name, body=configuration, status=400)
            assert pomi('background-tasks', 'show', task_name) == disabled
        desired = {**disabled['configuration'], 'schedule': '51 23 31 12 *', 'description': name}
        assert pomi('background-tasks', 'validate', body=desired)['isValid']
        changed = pomi('background-tasks', 'update', task_name, body=desired)
        assert changed['configuration'] == desired and not changed['enabled']
        assert pomi('background-tasks', 'update', task_name, body=desired) == changed
        assert tool('background-tasks_show', {'query': {'name': task_name}}) == changed
        tool('background-tasks_update', {'query': {'name': task_name}, 'body': {**desired, 'description': name + ' MCP'}})
        pomi('background-tasks', 'enable', task_name)
        assert pomi('background-tasks', 'show', task_name)['enabled']

        # A second tenant on this same host must have independent audit and task state.
        secondary_name = name + 'Tenant'
        installed = request('api/tenants/' + secondary_name + ':install', 'POST', {
            'siteName': secondary_name, 'userName': 'admin', 'email': 'admin@example.test',
            'password': secrets.token_urlsafe(32) + 'aA1!', 'recipeName': 'Blank',
            'requestUrlPrefix': secondary_name.lower(), 'enableRemoteManagement': True}, status=201)
        secondary_base = installed['primaryUrl'].rstrip('/') + '/'
        credentials = installed['clientCredentials']
        data = urllib.parse.urlencode({'grant_type': 'client_credentials', 'client_id': credentials['clientId'],
            'client_secret': credentials['clientSecret'], 'scope': 'orchardcore.management'}).encode()
        with urllib.request.urlopen(urllib.request.Request(secondary_base + 'connect/token', data=data,
                headers={'Content-Type': 'application/x-www-form-urlencoded'}), timeout=60) as response:
            secondary_token = json.load(response)['access_token']
        def secondary(path, method='GET', body=None, status=200):
            url = urllib.parse.urljoin(secondary_base, path)
            assert urllib.parse.urlparse(url).netloc == urllib.parse.urlparse(base).netloc
            req = urllib.request.Request(url, method=method,
                headers={'Authorization': 'Bearer ' + secondary_token, 'Content-Type': 'application/json'},
                data=json.dumps(body).encode() if body is not None else None)
            try:
                response = urllib.request.urlopen(req, timeout=60)
            except urllib.error.HTTPError as error:
                response = error
            with response:
                assert response.status == status, ('secondary', method, path, response.status)
                text = response.read().decode()
                return json.loads(text) if text else None
        for feature in ['OrchardCore.AuditTrail', 'OrchardCore.BackgroundTasks']:
            secondary('api/features/' + feature + ':enable?force=true', 'POST')
        secondary('api/audit-trail/events/by-id?eventId=' + event['eventId'], status=404)
        assert secondary('api/audit-trail/events?q=' + urllib.parse.quote(query))['totalCount'] == 0
        secondary_task = secondary('api/background-tasks/by-name?name=' + task_name)
        assert secondary_task['configuration']['description'] != name + ' MCP'
        secondary('api/background-tasks/disable?name=' + task_name, 'POST', status=204)
        assert pomi('background-tasks', 'show', task_name)['enabled']
        print('PASS: real audit events/search/pagination, Pomi/MCP, cron validation, status/configuration retries, permission separation and two-tenant isolation', flush=True)
    finally:
        pomi('background-tasks', 'update', task_name, body=before['configuration'])
        pomi('background-tasks', 'enable' if before['enabled'] else 'disable', task_name)
        request('api/users/' + user_id, 'DELETE', status=(200, 204))
        if secondary_name:
            request('api/tenants/' + secondary_name + ':stop', 'POST', status=(200, 204))
