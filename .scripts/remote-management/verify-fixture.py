#!/usr/bin/env python3
"""Verify discovery and authorization on every CLI-projected route in a local fixture."""
import json
from pathlib import Path
import re
import sys
import urllib.error
import urllib.parse
import urllib.request

state = json.loads(Path(sys.argv[1]).read_text())
url = state['url']
assert urllib.parse.urlparse(url).hostname in ('127.0.0.1', 'localhost', '::1'), 'Only loopback fixtures are supported'

def request(path, method='GET', token=None, data=None, content_type='application/json'):
    headers = {'Content-Type': content_type}
    if token:
        headers['Authorization'] = 'Bearer ' + token
    req = urllib.request.Request(url + path.lstrip('/'), data=data, method=method, headers=headers)
    try:
        with urllib.request.urlopen(req, timeout=30) as response:
            return response.status, response.read()
    except urllib.error.HTTPError as error:
        return error.code, error.read()

def token(client):
    data = urllib.parse.urlencode(dict(grant_type='client_credentials', client_id=client, client_secret=state['OC_CLIENT_SECRET'], scope='orchardcore.management')).encode()
    req = urllib.request.Request(url+'connect/token', data=data, headers={'Content-Type':'application/x-www-form-urlencoded'})
    with urllib.request.urlopen(req) as response:
        return json.load(response)['access_token']

admin = token('cli-fixture')
denied = token('cli-denied')
discovery = token('cli-discovery')
status, payload = request('api/management/manifest', token=admin)
assert status == 200, status
manifest = json.loads(payload)
openapi_path = urllib.parse.urlparse(manifest['openApiUrl']).path
status, payload = request(openapi_path, token=admin)
assert status == 200, status
schema = json.loads(payload)
(Path(state['root']) / 'openapi.json').write_text(json.dumps(schema, indent=2))
failures = []
commands = []
# These routes resolve a resource before checking its resource-specific permission.
# Unknown resources disclose no data; repeated user deletion converges to 204.
missing_resource_status = {
    'queries execute': 404, 'users show': 404, 'users update': 404,
    'users delete': 204, 'users disable': 404, 'custom-settings show': 404,
    'custom-settings update': 404, 'custom-settings schema': 404,
    'custom-settings list': 200,  # Filters out settings the identity cannot access.
    'settings sections list': 200,  # Filters providers by their read permission.
    'settings sections show': 404, 'settings sections update': 404,
    'settings sections schema': 404,
    'content localizations list': 404, 'content localizations create': 404,
}
ids = set()
capabilities = {capability['id'] for capability in manifest['capabilities']}
assert 'static-files' not in capabilities
assert not any('/static-files' in path for path in schema['paths'])

for path, item in schema['paths'].items():
    for method, operation in item.items():
        if not isinstance(operation, dict) or 'x-oc-cli' not in operation:
            continue
        meta = operation['x-oc-cli']
        if meta.get('capability') and meta['capability'] not in capabilities:
            failures.append({'operation':operation.get('operationId'),'error':'capability absent from manifest'})
        command = ' '.join(meta['commandGroup'] + [meta['verb']])
        if command in commands or not operation.get('operationId') or operation.get('operationId') in ids:
            failures.append({'command':command, 'error':'duplicate command or missing/duplicate operation ID'})
        commands.append(command)
        ids.add(operation.get('operationId'))
        target = re.sub(r'\{[^}]+\}', 'MissingReviewResource', path)
        body = b'{}' if method.upper() in ('POST','PUT','PATCH') else None
        if command == 'content localizations create':
            body = b'{"culture":"en"}'  # Reach the missing-resource check with a valid-shaped request.
        for identity, bearer, expected in [('anonymous',None,401),('denied',denied,403),('discovery',discovery,403)]:
            content_type = next(iter(operation.get('requestBody', {}).get('content', {'application/json':{}})))
            status, _ = request(target, method.upper(), bearer, body, content_type)
            if identity == 'discovery':
                expected = missing_resource_status.get(command, 403)
            if operation.get('operationId') == 'ApiGetMediaLocalizations':
                expected = 200  # Public, non-sensitive gallery UI labels, deliberately anonymous.
            if status != expected:
                failures.append({'command':command, 'identity':identity, 'status':status,'expected':expected})
# Exercise real resources as well as absent IDs: no permission to inspect/change admin.
status, payload = request('api/users', token=admin)
assert status == 200
user_id = json.loads(payload)['items'][0]['userId']
for method, suffix in [('GET',''),('PUT',''),('DELETE',''),('POST',':disable')]:
    status, _ = request('api/users/' + user_id + suffix, method, discovery, b'{}' if method in ('PUT','POST') else None)
    if status != 403:
        failures.append({'resource':'existing user','method':method,'status':status,'expected':403})
status, _ = request('api/media/folders', 'POST', admin, b'{}')
if status != 400:
    failures.append({'resource':'empty folder name','status':status,'expected':400})
report = {'commands': len(commands), 'authorizationRequests':len(commands)*3+4, 'failures':failures, 'commandPaths':sorted(commands)}
(Path(state['root']) / 'verification.json').write_text(json.dumps(report, indent=2))
print(json.dumps({key:value for key,value in report.items() if key != 'commandPaths'}, indent=2))
raise SystemExit(bool(failures))
