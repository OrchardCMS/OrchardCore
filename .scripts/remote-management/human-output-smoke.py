#!/usr/bin/env python3
"""Exercise native CLI output against a disposable loopback API, without credentials."""
import argparse
import hashlib
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
import json
import os
import select
from pathlib import Path
import subprocess
import tempfile
import threading

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('pomi', type=Path)
args = parser.parse_args()
pomi = str(args.pomi.resolve())


class Handler(BaseHTTPRequestHandler):
    def log_message(self, *_):
        pass

    def do_POST(self):
        self.rfile.read(int(self.headers.get('Content-Length', 0)))
        self.send_response(response_status)
        self.send_header('Content-Type', 'application/json')
        self.end_headers()
        self.wfile.write(json.dumps(result).encode())

    def do_DELETE(self):
        self.send_response(204)
        self.end_headers()


server = ThreadingHTTPServer(('127.0.0.1', 0), Handler)
thread = threading.Thread(target=server.serve_forever, daemon=True)
thread.start()
tenant = f'http://127.0.0.1:{server.server_port}/'
setup_url = tenant + 'Demo/setup?token=' + 'sample-token-' * 32
result = {'name': 'Demo', 'state': 'Uninitialized', 'setupUrl': setup_url,
          'primaryUrl': tenant + 'Demo/', 'canDelete': False, 'featureProfiles': []}
response_status = 201
try:
    with tempfile.TemporaryDirectory(prefix='pomi-human-output-') as directory:
        config = Path(directory)
        env = os.environ.copy()
        env['OC_CONFIG_HOME'] = directory
        env.pop('OC_CLIENT_ID', None)
        env.pop('OC_CLIENT_SECRET', None)
        (config / 'contexts.json').write_text(json.dumps({
            'currentContext': 'test', 'contexts': [{'name': 'test', 'tenantUrl': tenant}, {'name': 'other', 'tenantUrl': tenant}],
        }))
        cache = config / 'cache' / hashlib.sha256(tenant.encode()).hexdigest()[:16]
        cache.mkdir(parents=True)

        def invoke(*options, verb='create', terminal=False):
            # Mutations invalidate metadata; reseed this isolated fixture for each call.
            (cache / 'openapi.json').write_text(json.dumps({
                'expiresAt': '9999-01-01T00:00:00Z',
                'content': json.dumps({'paths': {'/api/tenants': {'post': {
                    'operationId': 'CreateTenant',
                    'x-oc-cli': {'commandGroup': ['tenants'], 'verb': verb if verb != 'delete' else 'create'},
                }, 'delete': {
                    'operationId': 'DeleteTenant',
                    'x-oc-cli': {'commandGroup': ['tenants'], 'verb': 'delete'},
                }}}}),
            }))
            command = [pomi, 'tenants', verb, *options]
            if terminal:
                master, slave = os.openpty()
                try:
                    subprocess.run(command, env=env, stdout=slave, stderr=subprocess.PIPE,
                                   check=True, timeout=15)
                    # Drain before closing the slave: macOS can discard unread bytes on close.
                    chunks = []
                    while select.select([master], [], [], 0)[0]:
                        chunk = os.read(master, 4096)
                        if not chunk:
                            break
                        chunks.append(chunk)
                    # .NET may initialize xterm keypad mode when opening the console.
                    return b''.join(chunks).decode().replace('\r\n', '\n').removeprefix('\x1b[?1h\x1b=')
                finally:
                    os.close(slave)
                    os.close(master)
            return subprocess.run(command, env=env,
                                  capture_output=True, text=True, check=True, timeout=15).stdout

        assert json.loads(invoke()) == result  # Default auto follows stdout redirection.
        human = invoke('--output', 'human')
        if os.name != 'nt':
            env['TERM'] = 'xterm-256color'
            env.pop('NO_COLOR', None)
            terminal_human = invoke(terminal=True)
            assert '\x1b[36mNext:' in terminal_human and '\x1b[0m' in terminal_human, repr(terminal_human)
            assert terminal_human.replace('\x1b[36m', '').replace('\x1b[0m', '') == human, (repr(terminal_human), repr(human))
            assert '\x1b' not in human  # Redirected human output remains plain.
            env['NO_COLOR'] = '1'
            assert invoke(terminal=True) == human
            env.pop('NO_COLOR')
            env['TERM'] = 'dumb'
            assert invoke(terminal=True) == human
            env['TERM'] = 'xterm-256color'
            assert json.loads(invoke('--output', 'json', terminal=True)) == result
        assert human.startswith("Tenant 'Demo' created successfully."), human
        assert 'Setup URL: ' + setup_url in human, human
        assert ' | ' not in human and 'Can delete' not in human, human
        assert 'pomi tenants setup Demo' in human, human
        assert '--context' not in human, human
        assert invoke('--context', 'test', '--output', 'human') == human
        other = invoke('--context', 'other', '--output', 'human')
        assert 'pomi --context=other tenants setup Demo' in other, other
        assert "--email '<admin-email>'" in human, human
        assert '--password' not in human, human
        assert json.loads(invoke('--output', 'json')) == result
        assert json.loads(invoke('--output', 'auto')) == result  # Explicit auto follows redirection.
        assert setup_url in invoke('--output', 'table')
        assert not invoke('--output', 'none')
        assert invoke('--output', 'human') == human
        assert invoke('--output', 'human', verb='delete').strip() == 'Tenant removed successfully.'
        response_status = 202
        result = {'operationId': 'job-42', 'status': 'pending'}
        pending = invoke('--output', 'human')
        assert pending.startswith('Request accepted.') and 'job-42' in pending, pending
        response_status = 200
        result = {'name': 'Demo', 'state': 'Running', 'primaryUrl': tenant + 'Demo/'}
        running = invoke('--output', 'human')
        assert 'pomi tenants enable-remote-management Demo' in running, running
        setup = invoke('--output', 'human', verb='setup')
        assert "Tenant 'Demo' set up successfully." in setup, setup
        assert 'pomi tenants enable-remote-management Demo' in setup, setup
        result = {'name': 'Demo', 'state': 'Running', 'url': tenant + 'Demo/'}
        enabled = invoke('--output', 'human', verb='enable-remote-management')
        assert "Tenant 'Demo' configured for remote management successfully." in enabled, enabled
        assert f'pomi context add Demo {tenant}Demo/ --current' in enabled, enabled
        result = {'success': False, 'message': 'The request could not be completed'}
        failed = invoke('--output', 'human')
        assert 'unsuccessful result' in failed and result['message'] in failed, failed
        response_status = 400
        try:
            invoke()
            raise AssertionError('HTTP failure reported success')
        except subprocess.CalledProcessError as error:
            assert not error.stdout.strip() and 'api_error' in error.stderr, error
        # The spelling is deliberately --output; --format is not an alias.
        help_result = subprocess.run([pomi, '--help'], env=env, capture_output=True, text=True, check=True)
        assert '--output' in help_result.stdout and '--format' not in help_result.stdout
        print(human.strip())
        print('Verified default auto output, human messages, complete URLs, and explicit JSON/table/none/auto modes.')
finally:
    server.shutdown()
    server.server_close()
    thread.join(timeout=5)
