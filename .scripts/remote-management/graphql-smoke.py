#!/usr/bin/env python3
"""Exercise the real Orchard GraphQL endpoint through pomi on a disposable fixture."""
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import urllib.parse

state_path = Path(sys.argv[1])
state = json.loads(state_path.read_text())
assert urllib.parse.urlparse(state['url']).hostname in ('127.0.0.1', 'localhost', '::1')
wrapper = Path(__file__).with_name('pomi-fixture.py')


def pomi(*args, client=None, expected=0, input=None):
    env = os.environ.copy()
    env['OC_FIXTURE_CONFIG_HOME'] = config_home
    if client:
        env['OC_FIXTURE_CLIENT_ID'] = client
    result = subprocess.run([sys.executable, str(wrapper), str(state_path), *args, '--output', 'json'],
                            input=input, text=True, capture_output=True, env=env, timeout=90)
    assert result.returncode == expected, (args, result.returncode, result.stderr)
    return json.loads(result.stdout) if result.stdout.strip() else None


# Other smoke tests may already have populated the fixture's shared OpenAPI cache.
# Use a fresh context store to prove GraphQL has no dependency on that cache.
with tempfile.TemporaryDirectory(prefix='graphql-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', 'graphql-smoke', state['url'], '--current')
    # Deliberately do not run api refresh: GraphQL must work with no OpenAPI cache.
    result = pomi('graphql', 'execute', '--query', '{ __typename siteCultures { culture default } }')
    assert result['data']['__typename'] and result['data']['siteCultures']
    schema = pomi('graphql', 'schema')['data']['__schema']
    assert schema['queryType']['name']
    assert any(t['name'] == 'SiteCulture' for t in schema['types'])
    single = pomi('graphql', 'schema', '--type', 'SiteCulture')['data']['__type']
    assert {'culture', 'default'} <= {field['name'] for field in single['fields']}
    query = 'query Read($name: String!) { __type(name: $name) { name } }'
    result = pomi('graphql', 'execute', '--stdin', '--variables', '{"name":"SiteCulture"}', '--operation-name', 'Read', input=query)
    assert result['data']['__type']['name'] == 'SiteCulture'
    assert pomi('graphql', 'execute', '--query', '{ __typename }', client='cli-graphql-reader')['data']['__typename']
    # Field errors are returned intact; a successful HTTP status alone is not sufficient.
    errors = pomi('graphql', 'execute', '--query', '{ nonexistentField }', expected=4)
    assert errors['errors']
    pomi('graphql', 'execute', '--query', '{ __typename }', client='cli-denied', expected=4)
    assert not list((Path(config_home) / 'cache').rglob('openapi.json'))
    print('Orchard GraphQL smoke passed: native queries, full/type introspection, variables/stdin, errors, and permission checks.')
