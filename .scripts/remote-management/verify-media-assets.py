#!/usr/bin/env python3
"""Verify Media asset policy and retired static routes on a disposable loopback tenant."""
import base64
import json
from pathlib import Path
import secrets
import sys
import urllib.error
import urllib.parse
import urllib.request

state = json.loads(Path(sys.argv[1]).read_text())
url = state['url']
assert urllib.parse.urlparse(url).hostname in ('127.0.0.1', 'localhost', '::1')
checks = []


def token(client):
    body = urllib.parse.urlencode(dict(grant_type='client_credentials', client_id=client,
        client_secret=state['OC_CLIENT_SECRET'], scope='orchardcore.management')).encode()
    with urllib.request.urlopen(url + 'connect/token', data=body, timeout=30) as response:
        return json.load(response)['access_token']


def request(path, bearer, method='GET', body=None, expected=200):
    headers = {'Authorization': 'Bearer ' + bearer}
    if isinstance(body, dict):
        body = json.dumps(body).encode()
        headers['Content-Type'] = 'application/json'
    elif body is not None:
        headers['Content-Type'] = 'application/octet-stream'
    req = urllib.request.Request(url + path, method=method, data=body, headers=headers)
    try:
        with urllib.request.urlopen(req, timeout=30) as response:
            status, payload = response.status, response.read()
    except urllib.error.HTTPError as error:
        status, payload = error.code, error.read()
    checks.append({'method': method, 'path': path.split('?')[0], 'status': status})
    assert status == expected, f'{method} {path}: expected {expected}, got {status}: {payload[:400]!r}'
    return json.loads(payload) if status == 200 and payload else None


regular = token('cli-media')
restricted = token('cli-media-restricted')
no_folder = token('cli-media-no-folder')
admin = token('cli-fixture')
for bearer, permitted in [(regular, False), (restricted, True)]:
    extensions = request('api/media/constraints', bearer)['allowedFileExtensions']
    assert '.png' in extensions
    for extension in ('.css', '.js', '.svg'):
        assert (extension in extensions) == permitted, (extension, extensions)
# The default policy lets Media users manage ordinary folders. Own-media access
# must still reject another user's protected folder.
request('api/media/constraints', no_folder)
manifest = request('api/management/manifest', admin)
assert 'static-files' not in {item['id'] for item in manifest['capabilities']}
# Limited clients need to read the document before pomi can discover Media commands.
openapi_path = urllib.parse.urlparse(manifest['openApiUrl']).path.lstrip('/')
for bearer in (regular, restricted):
    schema = request(openapi_path, bearer)
    assert '/api/media/files/content' in schema['paths']
    assert not any('/static-files' in path for path in schema['paths'])

for method, path in [('GET', 'api/static-files'), ('GET', 'api/static-files/file?path=probe.css'),
                     ('PUT', 'api/static-files/content?path=probe.css'), ('DELETE', 'api/static-files/file?path=probe.css')]:
    request(path, admin, method, b'/* not written */' if method == 'PUT' else None, expected=404)

folder = 'assets-' + secrets.token_hex(6)
protected_folder = '_Users/OtherUser-' + secrets.token_hex(6)
request('api/media/folders', restricted, 'POST', {'name': folder})
try:
    assets = {
        'site-v1.css': b'/* Media policy test */\nbody { color: #123; }\n',
        'site-v1.js': b'/* Media policy test; no executable code */\n',
        'site-v1.svg': b'<svg xmlns="http://www.w3.org/2000/svg" width="1" height="1"><rect width="1" height="1"/></svg>',
        'pixel.png': base64.b64decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII='),
    }
    request('api/media/files/content?' + urllib.parse.urlencode({'path': protected_folder, 'fileName': 'probe.png'}),
            admin, 'PUT', assets['pixel.png'])
    request('api/media/file?path=' + protected_folder + '/probe.png', no_folder, expected=403)
    for name, content in assets.items():
        endpoint = 'api/media/files/content?' + urllib.parse.urlencode({'path': folder, 'fileName': name})
        if not name.endswith('.png'):
            request(endpoint, regular, 'PUT', content, expected=400)
            request('api/media/file?path=' + folder + '/' + name, restricted, expected=404)
        protected_endpoint = 'api/media/files/content?' + urllib.parse.urlencode(
            {'path': protected_folder, 'fileName': name})
        request(protected_endpoint, no_folder, 'PUT', content, expected=403)
        uploaded = request(endpoint, regular if name.endswith('.png') else restricted, 'PUT', content)
        assert uploaded['filePath'] == folder + '/' + name
        request(endpoint, restricted, 'PUT', content, expected=400)  # No implicit overwrite.
        with urllib.request.urlopen(urllib.parse.urljoin(url, uploaded['url']), timeout=30) as response:
            actual = response.read()
            if name.endswith(('.css', '.js', '.png')):
                assert actual == content
            assert response.status == 200
    request('api/media/files/content?' + urllib.parse.urlencode({'path': folder, 'fileName': 'blocked.exe'}),
            restricted, 'PUT', b'not an executable', expected=400)
    # Copy/rename cannot bypass the destination extension policy.
    for verb in ('copy', 'move'):
        body = {'oldPath': folder + '/pixel.png', 'newPath': folder + '/' + verb + '.js'}
        request('api/media/files:' + verb, regular, 'POST', body, expected=400)
        request('api/media/files:' + verb, restricted, 'POST', body)
    # An independent Media identity sees assets created through the upload API.
    entries = request('api/media/files?path=' + folder, admin)['items']
    assert {item['name'] for item in entries} == {'site-v1.css', 'site-v1.js', 'site-v1.svg', 'copy.js', 'move.js'}
finally:
    request('api/media/folder?path=' + protected_folder, admin, 'DELETE')
    request('api/media/folder?path=' + folder, restricted, 'DELETE')
request('api/media/file?path=' + folder + '/site-v1.css', restricted, expected=404)
report = {'passed': True, 'requests': len(checks), 'checks': checks}
(Path(state['root']) / 'media-assets.json').write_text(json.dumps(report, indent=2))
print(json.dumps({'passed': True, 'requests': len(checks)}))
