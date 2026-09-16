#!/usr/bin/env python3
"""Check native confirmation flags independently of an API's force parameter."""
import hashlib
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import threading
import urllib.parse

binary = str(Path(sys.argv[1]).resolve())
requests = []


class Handler(BaseHTTPRequestHandler):
    def log_message(self, *_):
        pass

    def do_POST(self):
        body = self.rfile.read(int(self.headers.get('Content-Length', 0)))
        requests.append((self.path, json.loads(body) if body else None))
        self.send_response(200)
        self.send_header('Content-Type', 'application/json')
        self.end_headers()
        self.wfile.write(b'{"success":true}')


server = ThreadingHTTPServer(('127.0.0.1', 0), Handler)
thread = threading.Thread(target=server.serve_forever, daemon=True)
thread.start()
tenant = f'http://127.0.0.1:{server.server_port}/'
try:
    with tempfile.TemporaryDirectory(prefix='pomi-force-confirmation-') as directory:
        env = os.environ.copy()
        env['OC_CONFIG_HOME'] = directory
        for name in ('OC_CLIENT_ID', 'OC_CLIENT_SECRET'):
            env.pop(name, None)
        config = Path(directory)
        (config / 'contexts.json').write_text(json.dumps({
            'currentContext': 'test', 'contexts': [{'name': 'test', 'tenantUrl': tenant}],
        }))
        cache = config / 'cache' / hashlib.sha256(tenant.encode()).hexdigest()[:16]
        cache.mkdir(parents=True)

        def invoke(*arguments, code=0, body_parameter=False):
            operation = {
                'operationId': 'DisableFeature',
                'x-oc-cli': {'commandGroup': ['features'], 'verb': 'disable', 'requiresConfirmation': True},
            }
            if body_parameter:
                operation['requestBody'] = {'required': True, 'content': {'application/json': {'schema': {
                    'type': 'object', 'properties': {'force': {'type': 'boolean'}}, 'required': ['force'],
                }}}}
            else:
                operation['parameters'] = [{'name': 'force', 'in': 'query', 'schema': {'type': 'boolean'}}]
            (cache / 'openapi.json').write_text(json.dumps({
                'expiresAt': '9999-01-01T00:00:00Z',
                'content': json.dumps({'paths': {'/api/features/Example:disable': {'post': operation}}}),
            }))
            result = subprocess.run([binary, *arguments, '--output', 'json'], env=env,
                                    capture_output=True, text=True, timeout=20)
            assert result.returncode == code, (arguments, result.returncode, result.stderr)
            return result.stdout + result.stderr

        help_text = invoke('features', 'disable', '--help')
        assert '--force' in help_text and '--api-force' in help_text and '--yes' not in help_text
        assert '--force' in invoke('features', 'disable', code=1)
        invoke('features', 'disable', '--api-force', 'true', code=1)
        invoke('features', 'disable', '--yes', code=1)
        assert not requests, requests  # Neither an API parameter nor the removed flag confirms.
        invoke('features', 'disable', '--force')
        assert 'force' not in urllib.parse.parse_qs(urllib.parse.urlparse(requests[-1][0]).query), requests
        invoke('features', 'disable', '--force', '--api-force', 'true')
        assert urllib.parse.parse_qs(urllib.parse.urlparse(requests[-1][0]).query)['force'] == ['true']
        invoke('features', 'disable', '--force', '--api-force', 'false')
        assert urllib.parse.parse_qs(urllib.parse.urlparse(requests[-1][0]).query)['force'] == ['false']
        before = len(requests)
        assert '--api-force' in invoke('features', 'disable', '--force', code=1, body_parameter=True)
        assert len(requests) == before
        invoke('features', 'disable', '--force', '--api-force', 'false', body_parameter=True)
        assert requests[-1][1] == {'force': False}
        invoke('context', 'delete', 'test', code=1)
        invoke('context', 'delete', 'test', '--force')
        assert not json.loads((config / 'contexts.json').read_text())['contexts']
        print('Verified --force confirmation, removed --yes, distinct API force query/body values, required option errors, and context deletion.')
finally:
    server.shutdown()
    server.server_close()
    thread.join(timeout=5)
