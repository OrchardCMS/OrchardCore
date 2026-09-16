#!/usr/bin/env python3
"""Verify native GraphQL transport, output, and errors without credentials or OpenAPI."""
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import threading

binary = str(Path(sys.argv[1]).resolve())
requests = []
response_body = {'data': {'name': 'Français'}}
response_status = 200


class Handler(BaseHTTPRequestHandler):
    def log_message(self, *_):
        pass

    def do_GET(self):
        requests.append(('GET', self.path, None))
        self.send_error(500, 'Unexpected discovery or redirect')

    def do_POST(self):
        body = json.loads(self.rfile.read(int(self.headers.get('Content-Length', 0))))
        requests.append(('POST', self.path, body))
        self.send_response(response_status)
        self.send_header('Content-Type', 'application/graphql-response+json')
        if response_status == 302:
            self.send_header('Location', '/outside-tenant')
        self.end_headers()
        self.wfile.write(json.dumps(response_body).encode())


server = ThreadingHTTPServer(('127.0.0.1', 0), Handler)
threading.Thread(target=server.serve_forever, daemon=True).start()
try:
    with tempfile.TemporaryDirectory(prefix='pomi-graphql-') as directory:
        root = Path(directory)
        tenant = f'http://127.0.0.1:{server.server_port}/blog/'
        (root / 'contexts.json').write_text(json.dumps({'currentContext': 'test', 'contexts': [{'name': 'test', 'tenantUrl': tenant}]}))
        env = os.environ.copy()
        env.update(OC_CONFIG_HOME=directory)
        env.pop('OC_CLIENT_ID', None)
        env.pop('OC_CLIENT_SECRET', None)

        def invoke(*args, input=None, expected=0):
            result = subprocess.run([binary, 'graphql', *args], input=input, env=env, capture_output=True, text=True, timeout=20)
            assert result.returncode == expected, (args, result.returncode, result.stderr)
            return result

        query = 'query Read($name: String!) { __type(name: $name) { name } }'
        assert json.loads(invoke('execute', '--query', query, '--variables', '{"name":"String"}', '--operation-name', 'Read').stdout) == response_body
        assert requests[-1] == ('POST', '/blog/api/graphql', {'query': query, 'variables': {'name': 'String'}, 'operationName': 'Read'})
        (root / 'read.graphql').write_text(query, encoding='utf-8')
        (root / 'vars.json').write_text('{"name":"Français"}', encoding='utf-8')
        invoke('execute', '--file', str(root / 'read.graphql'), '--variables-file', str(root / 'vars.json'))
        assert requests[-1][2]['variables']['name'] == 'Français'
        invoke('execute', '--stdin', input=query)
        assert requests[-1][2]['query'] == query
        invoke('execute', '--named-query', 'RecentPosts')
        assert requests[-1][2] == {'namedQuery': 'RecentPosts'}
        invoke('execute', '--query', 'mutation { customMutation }')
        assert requests[-1][2]['query'].startswith('mutation')
        invoke('schema', '--type', 'Article', '--endpoint', 'custom/graphql')
        assert requests[-1][1] == '/blog/custom/graphql'
        assert requests[-1][2]['variables'] == {'name': 'Article'}
        assert '__type(name: $name)' in requests[-1][2]['query']
        invoke('schema')
        assert '__schema' in requests[-1][2]['query']
        for status in (200, 400, 401):
            response_status = status
            response_body = {'data': {'name': 'Partial'}, 'errors': [{'message': 'Field denied', 'path': ['secret']}]}
            before = len(requests)
            result = invoke('execute', '--query', query, expected=4)
            assert json.loads(result.stdout) == response_body
            assert 'GraphQL returned errors' in result.stderr
            assert len(requests) == before + 1  # Never retry a potentially committed mutation.
        human = invoke('execute', '--query', query, '--output', 'human', expected=4)
        assert 'Partial' in human.stdout and 'Field denied' not in human.stdout
        assert 'An error occurred.' in human.stderr and 'Field denied' in human.stderr
        assert '"errors"' not in human.stderr
        response_body = {'data': None, 'errors': [{'message': 'Field denied'}]}
        human = invoke('execute', '--query', query, '--output', 'human', expected=4)
        assert not human.stdout.strip() and 'Field denied' in human.stderr
        response_status = 302
        response_body = {}
        before = len(requests)
        assert not invoke('execute', '--query', query, expected=4).stdout.strip()
        assert len(requests) == before + 1  # Do not follow redirects with a bearer token.
        before = len(requests)
        invoke('execute', '--query', query, '--endpoint', '../outside', expected=1)
        invoke('execute', '--query', query, '--variables', '[]', expected=1)
        invoke('execute', '--query', query, '--stdin', expected=1)
        assert len(requests) == before
        assert all(method == 'POST' for method, _, _ in requests), requests
        assert not (root / 'cache').exists() or not list((root / 'cache').rglob('openapi.json'))
        print('Native GraphQL smoke passed: direct transport, variables, introspection, partial results, errors, and no retries/redirects.')
finally:
    server.shutdown()
    server.server_close()
