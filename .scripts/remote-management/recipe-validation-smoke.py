#!/usr/bin/env python3
"""Verify that validating a recipe or a deployment package reports its problems and changes nothing."""
import atexit
import hashlib
import io
import json
import os
from pathlib import Path
import secrets
import sqlite3
import ssl
import subprocess
import sys
import tempfile
import time
import urllib.error
import urllib.parse
import urllib.request
import zipfile

state_path = Path(sys.argv[1])
state = json.loads(state_path.read_text())
base = state['url']
assert urllib.parse.urlparse(base).hostname in ('127.0.0.1', 'localhost', '::1')
context = ssl.create_default_context(cafile=state.get('certificatePath'))
urllib.request.install_opener(urllib.request.build_opener(urllib.request.HTTPSHandler(context=context)))
root = Path(state['root'])
wrapper = Path(__file__).with_name('pomi-fixture.py')
marker = 'validation' + secrets.token_hex(6)
tokens = {}


def token(client):
    if client not in tokens:
        body = urllib.parse.urlencode(dict(grant_type='client_credentials', client_id=client,
            client_secret=state['OC_CLIENT_SECRET'], scope='orchardcore.management')).encode()
        with urllib.request.urlopen(urllib.request.Request(base + 'connect/token', data=body,
                headers={'Content-Type': 'application/x-www-form-urlencoded'}), timeout=30) as response:
            tokens[client] = json.load(response)['access_token']
    return tokens[client]


def request(path, method='GET', body=None, client='cli-fixture', status=200):
    url = urllib.parse.urljoin(base, path)
    assert urllib.parse.urlparse(url).netloc == urllib.parse.urlparse(base).netloc
    headers = {'Content-Type': 'application/octet-stream' if isinstance(body, bytes) else 'application/json',
        'Accept': 'application/json, text/event-stream'}
    if client:
        headers['Authorization'] = 'Bearer ' + token(client)
    data = body if isinstance(body, bytes) else json.dumps(body).encode() if body is not None else None
    try:
        response = urllib.request.urlopen(urllib.request.Request(url, method=method, headers=headers, data=data), timeout=120)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        text = response.read().decode()
        assert response.status == status, (method, path, client, response.status, status, text[:400])
        if response.headers.get('Content-Type', '').startswith('text/event-stream'):
            return next(json.loads(line[6:]) for line in text.splitlines()
                if line.startswith('data: ') and json.loads(line[6:]).get('id') == 1)
        return json.loads(text) if text and 'json' in response.headers.get('Content-Type', '') else None


def pomi(*args, human=False):
    env = os.environ.copy()
    env.update(OC_FIXTURE_CONFIG_HOME=config_home)
    result = subprocess.run([sys.executable, str(wrapper), str(state_path), *args, '--output', 'human' if human else 'json'],
        capture_output=True, text=True, env=env, timeout=120, stdin=subprocess.DEVNULL)
    assert result.returncode == 0, (args, result.stdout[:400], result.stderr[:400])
    return result.stdout if human else json.loads(result.stdout)


def tool(name, arguments):
    result = request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/call',
        'params': {'name': name, 'arguments': arguments}})['result']
    content = json.loads(result['content'][0]['text'])
    assert content['statusCode'] == 200, (name, content['statusCode'])
    return content['body']


def zip_package(*entries):
    output = io.BytesIO()
    with zipfile.ZipFile(output, 'w') as archive:
        for name, content in entries:
            archive.writestr(name, content)
    return output.getvalue()


def upload(package, file_name='package.zip', status=200):
    result = request('api/deployment/artifacts?fileName=' + file_name, 'POST', package, status=status)
    return result['id'] if status == 200 else None


def codes(result, severity='error'):
    return sorted((finding['stepIndex'], finding['stepName'], finding['code'])
        for finding in result['findings'] if finding['severity'] == severity)


def assert_contract(result):
    assert 'guarantee' in result['notice'], result['notice']
    assert result['valid'] == (result['errorCount'] == 0), result
    assert result['errorCount'] == sum(finding['severity'] == 'error' for finding in result['findings']), result
    assert result['warningCount'] == sum(finding['severity'] == 'warning' for finding in result['findings']), result


def digest_files():
    """Hash everything the tenant has on disk but its database, logs and the disposable credentials."""
    data = root / 'App_Data'
    digest = {}
    for path in sorted(data.rglob('*')):
        relative = str(path.relative_to(data))
        if not path.is_file() or path.suffix in ('.db', '.db-wal', '.db-shm', '.log') or '/logs/' in '/' + relative:
            continue
        digest[relative] = hashlib.sha256(path.read_bytes()).hexdigest()
    return digest


def digest_database():
    """Hash the rows of each table of the tenant's database, read from a separate read-only connection."""
    digest = {}
    files = sorted((root / 'App_Data').rglob('*.db'))
    assert files, 'The fixture has no SQLite database.'
    for file in files:
        connection = sqlite3.connect(f'file:{file}?mode=ro', uri=True, timeout=30)
        try:
            tables = [row[0] for row in connection.execute("SELECT name FROM sqlite_master WHERE type = 'table'")]
            for table in tables:
                # Signing in stores a token, and Pomi signs in during the test.
                if table in ('sqlite_sequence', 'OpenId_OpenIdTokenIndex'):
                    continue
                rows = sorted(repr(row) for row in connection.execute(f'SELECT * FROM "{table}"'))
                if table == 'OpenId_Document':
                    rows = [row for row in rows if 'OpenIdToken' not in row]
                digest[f'{file.name}/{table}'] = (len(rows), hashlib.sha256('\n'.join(rows).encode()).hexdigest())
        finally:
            connection.close()
    return digest


def tenant_state():
    temporary = root / 'Temp'
    assert temporary.is_dir(), 'Start this fixture with start-fixture.py, which keeps its temporary files in its folder.'
    return {
        'temporary': sorted(str(path.relative_to(temporary)) for path in temporary.rglob('*')),
        'features': sorted(feature['id'] for feature in request('api/features?take=200')['items'] if feature['isEnabled']),
        'settings': {section['name']: request('api/settings/sections/' + section['name']) for section in request('api/settings/sections')},
        'content': request('api/content?take=1')['totalCount'],
        'files': digest_files(),
        'database': digest_database(),
    }


def assert_unchanged(before, after):
    differences = [key for key in before if before[key] != after[key]]
    details = {}
    for key in differences:
        if isinstance(before[key], dict):
            details[key] = sorted(name for name in {*before[key], *after[key]} if before[key].get(name) != after[key].get(name))
    assert not differences, ('The validation changed the tenant.', details or differences)


def completed(operation):
    deadline = time.monotonic() + 240
    while time.monotonic() < deadline:
        current = request('api/deployment/operations/' + operation['id'])
        if current['state'] in ('succeeded', 'failed', 'uncertain'):
            return current
        time.sleep(3)
    raise AssertionError('The deployment worker did not finish before the test deadline.')


# The API lists the recipes of an extension, which are harvested once until the shell is released.
recipes_folder = root / 'Areas/OrchardCore.Recipes/Recipes'
recipes_folder.mkdir(parents=True, exist_ok=True)
(recipes_folder / f'{marker}-valid.recipe.json').write_text(json.dumps({
    'name': marker + '-valid', 'displayName': 'Valid validation recipe', 'steps': [
        {'name': 'Feature', 'enable': ['OrchardCore.Deployment'], 'disable': []},
        {'name': 'settings', 'SiteName': '[js:parameters("site")]'},
    ]}))
(recipes_folder / f'{marker}-invalid.recipe.json').write_text(json.dumps({
    'name': marker + '-invalid', 'displayName': 'Invalid validation recipe', 'steps': [
        {'name': 'settings', 'SiteName': marker},
        {'name': 'NoSuchStep' + marker},
        {'name': 'Feature', 'enable': ['No.Such.Feature']},
        {'name': 'Themes', 'site': 'NoSuchTheme'},
        {'name': 'Recipes', 'Values': [{'name': 'NoSuchRecipe'}]},
        {'name': 'settings', 'SiteName': "[file:text('missing.txt')]"},
    ]}))
(recipes_folder / f'{marker}-pending.recipe.json').write_text(json.dumps({
    'name': marker + '-pending', 'displayName': 'Pending handler validation recipe', 'steps': [
        {'name': 'Feature', 'enable': ['OrchardCore.Facebook']},
        {'name': 'NoSuchStep' + marker},
    ]}))
(recipes_folder / f'{marker}-no-steps.recipe.json').write_text(json.dumps({'name': marker + '-no-steps', 'displayName': 'No steps validation recipe'}))
(recipes_folder / f'{marker}-malformed.recipe.json').write_text('{"name":"' + marker + '-malformed",')
atexit.register(lambda: [path.unlink() for path in recipes_folder.glob(marker + '*')])

for feature in ['OrchardCore.Deployment', 'OrchardCore.RemoteManagement.Mcp', 'OrchardCore.Media', 'OrchardCore.Themes']:
    request('api/features/' + feature + ':enable?force=true', 'POST')
# A feature that was already enabled doesn't release the shell, which keeps the harvested recipes.
if not request(f'api/recipes?take=200&search={marker}')['items']:
    request('api/features/OrchardCore.RemoteManagement.Mcp:disable?force=true', 'POST')
    request('api/features/OrchardCore.RemoteManagement.Mcp:enable?force=true', 'POST')
recipes = {recipe['name']: recipe['id'] for recipe in request(f'api/recipes?take=200&search={marker}')['items']}
# A file that isn't valid JSON isn't harvested, so it can't be found to be validated.
assert sorted(recipes) == sorted(marker + suffix for suffix in ('-valid', '-invalid', '-pending', '-no-steps')), sorted(recipes)

valid_id, invalid_id, pending_id, no_steps_id = (recipes[marker + suffix] for suffix in ('-valid', '-invalid', '-pending', '-no-steps'))

site_marker = marker + '-applied'
media_name = marker + '.svg'
applied_steps = [
    {'name': 'settings', 'SiteName': site_marker},
    {'name': 'Feature', 'enable': ['OrchardCore.Sitemaps']},
    {'name': 'ContentDefinition', 'ContentTypes': [{'Name': 'ValidationProbe', 'DisplayName': 'Validation Probe',
        'Settings': {}, 'ContentTypePartDefinitionRecords': []}]},
    {'name': 'media', 'Files': [{'SourcePath': 'payload.svg', 'TargetPath': media_name}]},
]
payload = '<svg xmlns="http://www.w3.org/2000/svg"></svg>'
with tempfile.TemporaryDirectory(prefix='recipe-validation-') as config_home:
    pomi('context', 'add', 'validation', base, '--current')
    pomi('api', 'refresh', '--force')

    # Every package would change the tenant if it were applied.
    valid_package = upload(zip_package(('Recipe.json', json.dumps({'steps': applied_steps})), ('payload.svg', payload)))
    json_package = upload(json.dumps({'steps': [{'name': 'settings', 'SiteName': site_marker}]}).encode(), 'Recipe.json')
    invalid_package = upload(zip_package(('Recipe.json', json.dumps({'steps': [
        {'name': 'settings', 'SiteName': site_marker},
        {'name': 'NoSuchStep' + marker},
        {'name': 'Feature', 'enable': ['No.Such.Feature']},
        {'name': 'media', 'Files': [{'SourcePath': '../../../host.log', 'TargetPath': media_name},
                                    {'SourcePath': 'payload.svg', 'TargetPath': '../' + media_name}]},
        {'name': 'settings', 'SiteName': "[file:text('../../../fixture.json')]"},
    ]})), ('payload.svg', payload)))

    # An unsafe archive is refused with the same checks before it becomes an artifact.
    upload(zip_package(('../Recipe.json', '{"steps":[]}')), status=400)
    upload(zip_package(('payload.svg', payload)), status=400)
    upload(b'{"steps":', 'Recipe.json', status=400)

    # Issue the tokens now, as they are stored in the database.
    for client in ['cli-fixture', 'cli-denied', 'cli-discovery']:
        token(client)
    before = tenant_state()
    assert site_marker not in json.dumps(before['settings']), 'The fixture already has the marker.'
    print(f'Compared state: {len(before["features"])} enabled features, {len(before["files"])} files, {len(before["database"])} tables.', flush=True)

    # Recipes.
    result = request(f'api/recipes/{valid_id}:validate', 'POST')
    assert_contract(result)
    assert result['valid'] and result['stepCount'] == 2, result
    assert codes(result, 'info') == [(1, 'settings', 'scripts-not-evaluated')], result

    result = request(f'api/recipes/{invalid_id}:validate', 'POST')
    assert_contract(result)
    assert not result['valid'] and result['stepCount'] == 6, result
    assert codes(result) == sorted([
        (1, 'NoSuchStep' + marker, 'unknown-step'),
        (2, 'Feature', 'feature-not-found'),
        (3, 'Themes', 'theme-not-found'),
        (4, 'Recipes', 'recipe-not-found'),
        (5, 'settings', 'file-not-found'),
    ]), result

    # A feature that an earlier step enables may provide the handler, which can't be proven without enabling it.
    result = request(f'api/recipes/{pending_id}:validate', 'POST')
    assert_contract(result)
    assert result['valid'] and codes(result, 'warning') == [(1, 'NoSuchStep' + marker, 'step-handler-unverified')], result

    result = request(f'api/recipes/{no_steps_id}:validate', 'POST')
    assert not result['valid'] and codes(result) == [(None, None, 'recipe-no-steps')], result

    # Deployment packages.
    result = request('api/deployment/operations:validate', 'POST', {'artifactId': valid_package})
    assert_contract(result)
    assert result['valid'] and result['stepCount'] == len(applied_steps) and not codes(result), result

    result = request('api/deployment/operations:validate', 'POST', {'artifactId': json_package})
    assert result['valid'] and result['stepCount'] == 1, result

    result = request('api/deployment/operations:validate', 'POST', {'artifactId': invalid_package})
    assert_contract(result)
    assert not result['valid'], result
    assert codes(result) == sorted([
        (1, 'NoSuchStep' + marker, 'unknown-step'),
        (2, 'Feature', 'feature-not-found'),
        (3, 'media', 'file-not-found'),
        (3, 'media', 'media-target-invalid'),
        (4, 'settings', 'file-not-found'),
    ]), result
    print('HTTP findings match.', flush=True)

    # The same permissions as the execution, and nothing to learn without them.
    for path, body in [(f'api/recipes/{invalid_id}:validate', None), ('api/recipes/unknown:validate', None),
                       ('api/deployment/operations:validate', {'artifactId': invalid_package}),
                       ('api/deployment/operations:validate', {'artifactId': secrets.token_hex(16)})]:
        request(path, 'POST', body, client=None, status=401)
        request(path, 'POST', body, client='cli-denied', status=403)
        request(path, 'POST', body, client='cli-discovery', status=403)
    request('api/recipes/unknown:validate', 'POST', status=404)
    request('api/deployment/operations:validate', 'POST', {'artifactId': secrets.token_hex(16)}, status=404)
    request('api/deployment/operations:validate', 'POST', {'artifactId': 'not-an-id'}, status=404)
    request('api/deployment/operations:validate', 'POST', {}, status=400)
    print('Permissions match.', flush=True)

    # Pomi.
    result = pomi('recipes', 'validate', invalid_id)
    assert not result['valid'] and (1, 'NoSuchStep' + marker, 'unknown-step') in codes(result), result
    assert pomi('recipes', 'validate', valid_id)['valid']
    result = pomi('deployment', 'operations', 'validate', '--artifact-id', invalid_package)
    assert not result['valid'], result
    assert pomi('deployment', 'operations', 'validate', '--artifact-id', valid_package)['valid']
    output = pomi('recipes', 'validate', valid_id, human=True)
    assert 'guarantee' in output and 'successfully' not in output, output
    output = pomi('recipes', 'validate', invalid_id, human=True)
    assert 'unsuccessful result' in output and 'unknown-step' in output, output
    print('Pomi commands match.', flush=True)

    # OpenAPI and MCP.
    openapi = request(urllib.parse.urlparse(request('api/management/manifest')['openApiUrl']).path)
    for route, group in [('/api/recipes/{recipeId}:validate', ['recipes']), ('/api/deployment/operations:validate', ['deployment', 'operations'])]:
        extension = openapi['paths'][route]['post']['x-oc-cli']
        assert extension['commandGroup'] == group and extension['verb'] == 'validate', extension
        assert extension['readOnly'] is True and extension['requiresConfirmation'] is False, extension
    tools = {item['name']: item for item in request('mcp', 'POST', {'jsonrpc': '2.0', 'id': 1, 'method': 'tools/list'})['result']['tools']}
    for name in ['recipes_validate', 'deployment_operations_validate']:
        annotations = tools[name]['annotations']
        assert annotations['readOnlyHint'] is True and annotations['destructiveHint'] is False and annotations['idempotentHint'] is True, annotations
    assert not tool('recipes_validate', {'path': {'recipeId': invalid_id}})['valid']
    assert tool('recipes_validate', {'path': {'recipeId': valid_id}})['valid']
    assert not tool('deployment_operations_validate', {'body': {'artifactId': invalid_package}})['valid']
    assert tool('deployment_operations_validate', {'body': {'artifactId': valid_package}})['valid']
    print('MCP tools match.', flush=True)

    after = tenant_state()
    assert_unchanged(before, after)
    print('The tenant has not changed.', flush=True)

    # What a valid result can't prove: this one fails on its data, and the steps before it stay applied.
    failing = upload(zip_package(('Recipe.json', json.dumps({'steps': [
        {'name': 'media', 'Files': [{'Base64': 'not base64', 'TargetPath': media_name}]},
    ]}))))
    result = request('api/deployment/operations:validate', 'POST', {'artifactId': failing})
    assert result['valid'], result
    operation = request('api/deployment/operations:import', 'POST', {'requestId': marker + '-failing', 'artifactId': failing}, status=202)
    current = completed(operation)
    assert current['state'] == 'failed', current
    print('A valid package can fail on its data.', flush=True)

    # What an invalid result prevents: the import skips the unknown step, and reports a success.
    skipped = upload(zip_package(('Recipe.json', json.dumps({'steps': [{'name': 'NoSuchStep' + marker}]}))))
    result = request('api/deployment/operations:validate', 'POST', {'artifactId': skipped})
    assert not result['valid'], result
    operation = request('api/deployment/operations:import', 'POST', {'requestId': marker + '-skipped', 'artifactId': skipped}, status=202)
    current = completed(operation)
    assert current['state'] == 'succeeded', current
    print('An import silently skips the unknown step the validation reports.', flush=True)

    # The comparison isn't blind: importing the package that was only validated above changes what it looks at.
    current_state = tenant_state()
    operation = request('api/deployment/operations:import', 'POST', {'requestId': marker + '-applied', 'artifactId': valid_package}, status=202)
    current = completed(operation)
    assert current['state'] == 'succeeded', current
    applied = tenant_state()
    assert applied['database'] != current_state['database'] and applied['files'] != current_state['files'], 'The import changed nothing.'
    assert 'OrchardCore.Sitemaps' in applied['features'], 'The import changed no feature.'
    print('Importing the validated package changes the compared state.', flush=True)

print('All recipe validation checks passed', flush=True)
