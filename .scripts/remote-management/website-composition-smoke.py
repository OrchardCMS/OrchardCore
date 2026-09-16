#!/usr/bin/env python3
"""Compose a new tenant using its provisioned application context, without login."""
import json
import os
from pathlib import Path
import secrets
import ssl
import stat
import subprocess
import sys
import tempfile
import urllib.parse
import urllib.request

state_path = Path(sys.argv[1])
state = json.loads(state_path.read_text())
assert urllib.parse.urlparse(state['url']).hostname in ('127.0.0.1', 'localhost', '::1')
root = Path(tempfile.mkdtemp(prefix='website-composition-', dir=state_path.parent))
os.chmod(root, 0o700)
wrapper = Path(__file__).with_name('pomi-fixture.py')
password = secrets.token_urlsafe(32) + 'aA1!'
name = 'Compose' + secrets.token_hex(4)
ssl_context = ssl.create_default_context(cafile=state['certificatePath']) if state.get('certificatePath') else None
calls = []


def pomi(*args, body=None, parent=False, context=None):
    assert 'login' not in args
    calls.append(args)
    env = {**os.environ, 'OC_FIXTURE_CONFIG_HOME': str(root / 'contexts'), 'OC_COMPOSITION_PASSWORD': password}
    if parent:
        env.pop('OC_FIXTURE_HUMAN', None)
    else:
        env['OC_FIXTURE_HUMAN'] = '1'  # Stored client credentials only; no fixture client override.
    command = [sys.executable, str(wrapper), str(state_path)]
    if context:
        command += ['--context', context]
    command += [*args, '--output', 'json']
    if body is not None:
        command.append('--stdin')
    result = subprocess.run(command, input=json.dumps(body) if body is not None else None,
        env=env, text=True, capture_output=True, timeout=180)
    assert password not in result.stdout + result.stderr, 'Administrator password leaked into command output'
    assert result.returncode == 0, (args[:4], result.stderr[:800])
    return json.loads(result.stdout) if result.stdout.strip() else None


def handoff(tenant):
    path = root / (tenant['tenantId'] + '-administrator.json')
    descriptor = os.open(path, os.O_WRONLY | os.O_CREAT | os.O_EXCL, 0o600)
    with os.fdopen(descriptor, 'w') as output:
        json.dump({'url': tenant['primaryUrl'], 'username': 'admin', 'password': password}, output)
    assert stat.S_IMODE(path.stat().st_mode) == 0o600


pomi('context', 'add', name + '-parent', state['url'], '--current', parent=True)
pomi('api', 'refresh', '--force', parent=True)
before = pomi('context', 'list', parent=True)
common = ['--site-name', 'Composition verification', '--user-name', 'admin', '--email', 'admin@example.test',
    '--database-provider', 'Sqlite', '--password-env', 'OC_COMPOSITION_PASSWORD']
plain = pomi('tenants', 'install', name + 'Plain', '--request-url-prefix', (name + 'Plain').lower(),
    '--recipe-name', 'Blank', *common, parent=True)
assert plain['state'] == 'Running' and not plain.get('context')
assert pomi('context', 'list', parent=True) == before
handoff(plain)
managed = pomi('tenants', 'install', name, '--request-url-prefix', name.lower(), '--recipe-name', 'Blog',
    '--enable-remote-management', *common, parent=True)
assert managed['state'] == 'Running' and managed['context'] and managed['clientId'].startswith('pomi-')
assert 'clientCredentials' not in managed and 'clientSecret' not in managed
assert pomi('context', 'list', parent=True)['currentContext'] == before['currentContext']
handoff(managed)
context = managed['context']
for feature in ['OrchardCore.Layers', 'OrchardCore.Placements', 'OrchardCore.Shortcodes.Templates', 'OrchardCore.Liquid', 'OrchardCore.Html', 'OrchardCore.Autoroute']:
    pomi('features', 'enable', feature, context=context)
pomi('themes', 'set-current', 'TheTheme', context=context)
pomi('api', 'refresh', '--force', context=context)
assert 'Content' in pomi('layers', 'widgets', 'zones', context=context)
for suffix, parts, stereotype in [('Page', ['TitlePart', 'HtmlBodyPart', 'LiquidPart', 'AutoroutePart'], None),
        ('Widget', ['TitlePart', 'HtmlBodyPart'], 'Widget')]:
    settings = {'draftable': True, 'versionable': True, 'creatable': True}
    if stereotype:
        settings['stereotype'] = stereotype
    pomi('content', 'types', 'create', context=context, body={'name': name + suffix, 'displayName': name + suffix,
        'settings': {'ContentTypeSettings': settings}, 'parts': [{'name': part, 'partName': part} for part in parts]})
shortcode = name.lower()
shortcode_marker, page_marker, widget_marker = [kind + '-' + secrets.token_hex(5) for kind in ('shortcode', 'page', 'widget')]
pomi('shortcodes', 'templates', 'create', context=context, body={'name': shortcode, 'content': '<strong>{{ Content }}</strong>'})
page = pomi('content', 'items', 'save', context=context, body={'ContentType': name + 'Page',
    'TitlePart': {'Title': 'Composition page'}, 'HtmlBodyPart': {'Html': '<p>' + page_marker + '</p>'},
    'LiquidPart': {'Liquid': '{{ "[' + shortcode + ']' + shortcode_marker + '[/' + shortcode + ']" | shortcode | raw }}'},
    'AutoroutePart': {'Path': 'composition'}})
widget = pomi('content', 'items', 'save', context=context, body={'ContentType': name + 'Widget',
    'TitlePart': {'Title': 'Composition widget'}, 'HtmlBodyPart': {'Html': '<p>' + widget_marker + '</p>'}})
layer = {'name': name, 'conditions': [{'name': 'BooleanCondition', 'properties': {'value': True}}]}
pomi('layers', 'create', context=context, body=layer)
pomi('layers', 'widgets', 'update', widget['ContentItemId'], context=context,
    body={'layer': name, 'zone': 'Content', 'position': 1, 'renderTitle': False})
url = managed['primaryUrl'].rstrip('/') + '/composition'


def render():
    with urllib.request.urlopen(url, context=ssl_context, timeout=30) as response:
        return response.read().decode()


html = render()
assert page_marker in html and widget_marker in html and '<strong>' + shortcode_marker + '</strong>' in html
pomi('placements', 'create', context=context, body={'shapeType': 'HtmlBodyPart',
    'nodes': [{'place': '-', 'contentType': name + 'Page'}]})
html = render()
assert page_marker not in html and widget_marker in html and shortcode_marker in html
layer['conditions'][0]['properties']['value'] = False
pomi('layers', 'update', name, context=context, body=layer)
assert widget_marker not in render()
layer['conditions'][0]['properties']['value'] = True
pomi('layers', 'update', name, context=context, body=layer)
pomi('placements', 'update', 'HtmlBodyPart', context=context, body={'shapeType': 'HtmlBodyPart', 'nodes': []})
html = render()
assert page_marker in html and widget_marker in html and shortcode_marker in html
assert pomi('context', 'list', parent=True)['currentContext'] == before['currentContext']
assert not any('login' in args for args in calls)
print('Composition passed: optional setup, private administrator files, automatic application context, content, widget, layer, placement, shortcode and rendered output; no login.')
print('Disposable verification state:', root)
