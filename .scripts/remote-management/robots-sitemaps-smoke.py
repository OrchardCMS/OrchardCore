#!/usr/bin/env python3
"""Verify audit search and task administration through Pomi/MCP and tenant isolation."""
import json
import xml.etree.ElementTree as ET
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
name = 'SitemapSmoke' + secrets.token_hex(4)
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


with tempfile.TemporaryDirectory(prefix='sitemaps-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'add', name, base, '--current')
    for feature in ['OrchardCore.Seo', 'OrchardCore.Sitemaps', 'OrchardCore.Autoroute', 'OrchardCore.RemoteManagement.Mcp']:
        request('api/features/' + feature + ':enable?force=true', 'POST')
    pomi('api', 'refresh', '--force')
    for path in ['api/sitemaps', 'api/settings/sections/robots']:
        request(path, client=None, status=401)
        request(path, client='cli-discovery', status=403)
    request('api/sitemaps', client='cli-sitemaps')
    request('api/sitemaps', client='cli-seo', status=403)
    request('api/settings/sections/robots', client='cli-seo')
    request('api/settings/sections/robots', client='cli-sitemaps', status=403)
    before = pomi('settings', 'sections', 'show', 'robots')['values']
    sitemap_id = index_id = secondary_name = None
    try:
        desired_robots = {'allowAllAgents': True, 'disallowAdmin': True, 'additionalRules': 'Disallow: /' + name}
        pomi('settings', 'sections', 'update', 'robots', body=desired_robots)
        assert not pomi('settings', 'sections', 'update', 'robots', body=desired_robots)['changed']
        robots, _ = request('robots.txt', client=None, raw=True)
        assert 'User-agent: *' in robots and 'Disallow: /' + name in robots
        assert tool('settings_sections_show', {'path': {'name': 'robots'}})['values'] == desired_robots
        definition = {'name': name, 'path': name.lower() + '.xml', 'enabled': True, 'kind': 'Sitemap', 'containedSitemapIds': []}
        sitemap = pomi('sitemaps', 'create', body=definition)
        sitemap_id = sitemap['id']
        assert pomi('sitemaps', 'show', sitemap_id) == sitemap
        assert 'CustomPathSitemapSource' in pomi('sitemaps', 'sources', 'types')
        schema = pomi('sitemaps', 'sources', 'schema', 'CustomPathSitemapSource')
        assert 'path' in schema['properties'] and 'id' not in schema['properties']
        source = pomi('sitemaps', 'sources', 'create', sitemap_id, body={'type': 'CustomPathSitemapSource',
            'configuration': {'path': '/' + name + '-first', 'priority': 5, 'changeFrequency': 'Daily'}})
        source_id = source['id']
        def locations(path):
            xml, _ = request(path, client=None, raw=True)
            return [node.text for node in ET.fromstring(xml).iter('{http://www.sitemaps.org/schemas/sitemap/0.9}loc')]
        assert any(url.endswith('/' + name + '-first') for url in locations(definition['path']))
        changed_source = {'type': 'CustomPathSitemapSource', 'configuration': {'path': '/' + name + '-second', 'priority': 8, 'changeFrequency': 'Weekly'}}
        updated = pomi('sitemaps', 'sources', 'update', sitemap_id, source_id, body=changed_source)
        assert pomi('sitemaps', 'sources', 'update', sitemap_id, source_id, body=changed_source) == updated
        assert any(url.endswith('/' + name + '-second') for url in locations(definition['path']))
        assert not any(url.endswith('/' + name + '-first') for url in locations(definition['path']))
        pomi('sitemaps', 'sources', 'update', sitemap_id, source_id,
            body={'type': 'CustomPathSitemapSource', 'configuration': {'path': '/bad path'}}, status=400)
        pomi('content', 'types', 'create', body={'name': name + 'Page', 'displayName': name + 'Page',
            'settings': {'ContentTypeSettings': {'draftable': True, 'versionable': True}},
            'parts': [{'name': part, 'partName': part} for part in ['TitlePart', 'AutoroutePart']]})
        page = pomi('content', 'items', 'save', body={'ContentType': name + 'Page', 'TitlePart': {'Title': name},
            'AutoroutePart': {'Path': name.lower() + '-page'}})
        pomi('content', 'items', 'publish', page['ContentItemId'])
        content_source = {'type': 'ContentTypesSitemapSource', 'configuration': {'indexAll': False,
            'contentTypes': [{'contentTypeName': name + 'Page', 'priority': 5, 'changeFrequency': 'Daily'}]}}
        pomi('sitemaps', 'sources', 'create', sitemap_id, body=content_source)
        assert any(url.endswith('/' + name.lower() + '-page') for url in locations(definition['path']))
        index_definition = {'name': name + 'Index', 'path': name.lower() + '-index.xml', 'kind': 'SitemapIndex',
            'enabled': True, 'containedSitemapIds': [sitemap_id]}
        index = pomi('sitemaps', 'create', body=index_definition)
        index_id = index['id']
        assert any(url.endswith('/' + definition['path']) for url in locations(index_definition['path']))
        # A cached index must reflect a changed child path, status and deletion.
        definition['path'] = name.lower() + '-renamed.xml'
        pomi('sitemaps', 'update', sitemap_id, body=definition)
        assert any(url.endswith('/' + definition['path']) for url in locations(index_definition['path']))
        pomi('sitemaps', 'disable', sitemap_id)
        assert locations(index_definition['path']) == []
        request(definition['path'], client=None, status=404)
        pomi('sitemaps', 'enable', sitemap_id)
        assert locations(index_definition['path'])
        pomi('settings', 'update', body={'baseUrl': base.rstrip('/')})
        pomi('settings', 'sections', 'update', 'sitemaps-robots', body={'includeSitemaps': True})
        robots, _ = request('robots.txt', client=None, raw=True)
        assert 'Sitemap: ' + base + index_definition['path'] in robots
        pomi('settings', 'sections', 'update', 'sitemaps-robots', body={'includeSitemaps': False})
        robots, _ = request('robots.txt', client=None, raw=True)
        assert 'Sitemap: ' not in robots
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
        for feature in ['OrchardCore.Seo', 'OrchardCore.Sitemaps']:
            secondary('api/features/' + feature + ':enable?force=true', 'POST')
        secondary('api/sitemaps/' + sitemap_id, status=404)
        independent = secondary('api/settings/sections/robots')['values']
        assert independent['additionalRules'] != desired_robots['additionalRules']
        secondary('api/settings/sections/robots', 'PUT', {'additionalRules': 'Disallow: /secondary-only'})
        robots, _ = request('robots.txt', client=None, raw=True)
        assert 'secondary-only' not in robots
        pomi('sitemaps', 'delete', sitemap_id, '--force')
        assert locations(index_definition['path']) == []
        print('PASS: robots public output, sitemap/source CRUD, XML regeneration and index child path/status/deletion invalidation, Pomi/MCP and permissions.', flush=True)
    finally:
        pomi('settings', 'sections', 'update', 'robots', body=before)
        if index_id:
            pomi('sitemaps', 'delete', index_id, '--force')
        if sitemap_id:
            pomi('sitemaps', 'delete', sitemap_id, '--force')
        if secondary_name:
            request('api/tenants/' + secondary_name + ':stop', 'POST', status=(200, 204))
