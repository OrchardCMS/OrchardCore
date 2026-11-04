#!/usr/bin/env python3
"""Verify opt-in device QR output and PNG delivery before user authorization."""
import base64
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
import json
import os
from pathlib import Path
import queue
import struct
import subprocess
import sys
import tempfile
import threading
import zlib

binary = str(Path(sys.argv[1]).resolve())
challenge_read = threading.Event()
token_requested = threading.Event()
requests = []
approved = False


class Handler(BaseHTTPRequestHandler):
    def log_message(self, *_):
        pass

    def respond(self, status, value):
        self.send_response(status)
        self.send_header('Content-Type', 'application/json')
        self.end_headers()
        self.wfile.write(json.dumps(value).encode())

    def do_GET(self):
        requests.append(self.path)
        if self.path == '/blog/.well-known/openid-configuration':
            self.respond(200, {'issuer': tenant, 'token_endpoint': tenant + 'connect/token',
                               'device_authorization_endpoint': tenant + 'connect/device'})
        elif self.path == '/blog/api/management/manifest':
            assert self.headers.get('Authorization') == 'Bearer private-token'
            self.respond(200, {'protocolMajorVersion': 1, 'protocolMinorVersion': 0,
                               'managementManifestUrl': tenant + 'api/management/manifest',
                               'openApiUrl': tenant + 'openapi.json',
                               'authentication': {'authority': tenant, 'clientId': 'orchardcore-cli',
                                                  'grantTypes': ['urn:ietf:params:oauth:grant-type:device_code'], 'scopes': []}})
        else:
            assert self.path == '/blog/openapi.json'
            assert self.headers.get('Authorization') == 'Bearer private-token'
            self.respond(200, {'openapi': '3.0.0', 'paths': {}})

    def do_POST(self):
        requests.append(self.path)
        self.rfile.read(int(self.headers.get('Content-Length', 0)))
        if self.path == '/blog/connect/device':
            self.respond(200, {'device_code': 'private-device-code', 'user_code': '1234-5678',
                               'verification_uri': tenant + 'connect/verify',
                               'verification_uri_complete': verification_url,
                               'expires_in': 60, 'interval': 1})
        else:
            assert self.path == '/blog/connect/token'
            token_requested.set()
            challenge_read.wait(10)
            if approved:
                self.respond(200, {'access_token': 'private-token', 'expires_in': 3600})
            else:
                self.respond(400, {'error': 'access_denied'})


server = ThreadingHTTPServer(('127.0.0.1', 0), Handler)
tenant = f'http://127.0.0.1:{server.server_port}/blog/'
verification_url = tenant + 'connect/verify?user_code=1234-5678'
threading.Thread(target=server.serve_forever, daemon=True).start()
try:
    with tempfile.TemporaryDirectory(prefix='pomi-device-qr-') as directory:
        root = Path(directory)
        (root / 'contexts.json').write_text(json.dumps({'currentContext': 'test', 'contexts': [{
            'name': 'test', 'tenantUrl': tenant, 'authority': tenant, 'clientId': 'orchardcore-cli',
            'grantTypes': ['urn:ietf:params:oauth:grant-type:device_code']}]}))
        env = dict(os.environ, OC_CONFIG_HOME=directory, NO_COLOR='1', TERM='dumb')
        env.pop('OC_CLIENT_ID', None)
        env.pop('OC_CLIENT_SECRET', None)
        command = [binary, 'login', '--grant', 'device']
        challenge_read.set()
        for options in ([], ['--qr', 'never'], ['--output', 'human']):
            result = subprocess.run(command + options, env=env, capture_output=True, text=True, timeout=15)
            assert result.returncode == 1 and not result.stdout.strip(), result
            assert verification_url in result.stderr and '\x1b' not in result.stderr, result.stderr

        for options in (['--qr', 'always', '--output', 'json'], ['--qr', 'auto']):
            challenge_read.clear()
            process = subprocess.Popen(command + options, env=env, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
            try:
                lines = queue.Queue()
                threading.Thread(target=lambda: lines.put(process.stdout.readline()), daemon=True).start()
                pending_text = lines.get(timeout=8)
                pending = json.loads(pending_text)
                assert process.poll() is None  # Still waiting for the user's decision.
                assert pending['status'] == 'authorization_pending'
                assert pending['verificationUri'] == verification_url and pending['userCode'] == '1234-5678'
                assert 'private-device-code' not in pending_text and 'access_token' not in pending_text
                assert pending['qrCode']['mediaType'] == 'image/png'
                png = base64.b64decode(pending['qrCode']['base64'], validate=True)
                assert png[:8] == b'\x89PNG\r\n\x1a\n'
                width, height = struct.unpack('>II', png[16:24])
                assert 100 < width == height < 4096
                offset = 8
                compressed = bytearray()
                while offset < len(png):
                    length = struct.unpack('>I', png[offset:offset + 4])[0]
                    chunk = png[offset + 4:offset + 8 + length]
                    assert zlib.crc32(chunk) == struct.unpack('>I', png[offset + 8 + length:offset + 12 + length])[0]
                    if chunk[:4] == b'IDAT':
                        compressed.extend(chunk[4:])
                    offset += length + 12
                assert zlib.decompress(compressed)
                challenge_read.set()
                stdout, stderr = process.communicate(timeout=10)
                assert process.returncode == 1 and not stdout.strip()
                assert 'denied' in stderr and '\x1b' not in stderr
            finally:
                if process.poll() is None:
                    process.kill()
                    process.wait()
        def invoke(*args, expected=0):
            result = subprocess.run([binary, 'login', 'device', *args, '--output', 'json'], env=env,
                                    capture_output=True, text=True, timeout=15)
            assert result.returncode == expected, (args, result.returncode, result.stderr)
            assert 'private-device-code' not in result.stdout + result.stderr
            assert 'private-token' not in result.stdout + result.stderr
            return result

        before = len(requests)
        session = json.loads(invoke('start').stdout)
        assert requests[before:] == ['/blog/.well-known/openid-configuration', '/blog/connect/device']
        assert 'qrCode' not in session
        session_id = session['sessionId']
        state_file = root / 'device-logins' / (session_id + '.json')
        assert state_file.exists()
        if os.name != 'nt':
            assert state_file.stat().st_mode & 0o777 == 0o600
            assert state_file.parent.stat().st_mode & 0o777 == 0o700
        before = len(requests)
        shown = json.loads(invoke('show', session_id, '--qr', 'always').stdout)
        assert shown['verificationUri'] == session['verificationUri']
        assert shown['qrCode']['mediaType'] == 'image/png'
        assert len(requests) == before  # Redisplay needs no network.
        approved = True
        challenge_read.clear()
        token_requested.clear()
        waiter = subprocess.Popen([binary, 'login', 'device', 'wait', session_id, '--output', 'json'],
                                  env=env, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        try:
            assert token_requested.wait(8)
            before = len(requests)
            assert 'already waiting' in invoke('wait', session_id, expected=1).stderr
            assert json.loads(invoke('show', session_id).stdout)['sessionId'] == session_id
            assert len(requests) == before
            # Changing the current context while waiting must not retarget login or
            # have the concurrent configuration update overwritten on completion.
            config = json.loads((root / 'contexts.json').read_text())
            config['contexts'].append({'name': 'other', 'tenantUrl': 'https://other.example/'})
            config['currentContext'] = 'other'
            (root / 'contexts.json').write_text(json.dumps(config))
            challenge_read.set()
            stdout, stderr = waiter.communicate(timeout=15)
            assert waiter.returncode == 0, stderr
            assert json.loads(stdout)['context'] == 'test'
            assert json.loads((root / 'contexts.json').read_text())['currentContext'] == 'other'
            assert not state_file.exists()
        finally:
            challenge_read.set()
            if waiter.poll() is None:
                waiter.kill()
                waiter.wait()
        before = len(requests)
        assert 'not found' in invoke('wait', session_id, expected=1).stderr
        assert len(requests) == before
        approved = False
        denied = json.loads(invoke('start', '--context', 'test', '--qr', 'always').stdout)
        assert denied['qrCode']['mediaType'] == 'image/png'
        assert 'denied' in invoke('wait', denied['sessionId'], expected=1).stderr
        assert not (root / 'device-logins' / (denied['sessionId'] + '.json')).exists()
        expired = json.loads(invoke('start', '--context', 'test').stdout)
        expired_path = root / 'device-logins' / (expired['sessionId'] + '.json')
        state = json.loads(expired_path.read_text())
        state['expiresAt'] = '2000-01-01T00:00:00Z'
        expired_path.write_text(json.dumps(state))
        before = len(requests)
        assert 'expired' in invoke('wait', expired['sessionId'], expected=1).stderr
        assert not expired_path.exists() and len(requests) == before
        # Windows stores the successful login outside this temporary directory.
        logout = subprocess.run([binary, 'logout', 'test', '--output', 'none'], env=env,
                                capture_output=True, text=True, timeout=15)
        assert logout.returncode == 0, logout.stderr
        print('Native device login smoke passed: opt-in PNG, start/show/wait, offline redisplay, concurrent wait exclusion, context binding, success, denial, and expiry.')
finally:
    challenge_read.set()
    server.shutdown()
    server.server_close()
