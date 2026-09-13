#!/usr/bin/env python3
"""Verify dynamic version commands against a disposable start-fixture.py tenant."""
import json
import os
from pathlib import Path
import secrets
import subprocess
import sys
import urllib.parse
import urllib.request
import urllib.error

state_path = Path(sys.argv[1])
state = json.loads(state_path.read_text())
assert urllib.parse.urlparse(state['url']).hostname in ('127.0.0.1', 'localhost', '::1')
wrapper = Path(__file__).with_name('pomi-fixture.py')


def pomi(*args, body=None, status=None, failure=False):
    command = [sys.executable, str(wrapper), str(state_path), *args, '--output', 'json']
    if body is not None:
        command += ['--stdin']
    result = subprocess.run(command, input=json.dumps(body) if body is not None else None,
                            text=True, capture_output=True, timeout=90)
    if failure or status:
        assert result.returncode != 0, args
        if status:
            assert json.loads(result.stderr or result.stdout)['error']['status'] == status, (args, result.stderr)
        return
    assert result.returncode == 0, (args, result.stderr)
    return json.loads(result.stdout) if result.stdout.strip() else None


pomi('context', 'add', 'content-versions-smoke', state['url'], '--current')
pomi('api', 'refresh', '--force')
name = 'VersionSmoke' + secrets.token_hex(4)
pomi('content', 'types', 'create', body={
    'name': name, 'displayName': name,
    'settings': {'ContentTypeSettings': {'draftable': True, 'versionable': True, 'creatable': True, 'listable': True}},
    'parts': [{'name': 'TitlePart', 'partName': 'TitlePart', 'settings': {}}],
})
first = pomi('content', 'items', 'save', body={'ContentType': name, 'TitlePart': {'Title': 'Original version'}})
item_id, first_id = first['ContentItemId'], first['ContentItemVersionId']
second = pomi('content', 'items', 'update', item_id, body={'ContentType': name, 'TitlePart': {'Title': 'Current publication'}})
second_id = second['ContentItemVersionId']
assert first_id != second_id
listing = pomi('content', 'versions', 'list', item_id, '--take', '1')
assert listing['totalCount'] == 2 and listing['items'][0]['ContentItemVersionId'] == second_id
archived = pomi('content', 'versions', 'show', first_id)
assert not archived['Latest'] and not archived['Published']
assert archived['TitlePart']['Title'] == 'Original version'
rendered = pomi('content', 'versions', 'render', first_id, '--display-type', 'Detail')
assert rendered['contentItemVersionId'] == first_id and rendered['html'].strip()
restored = pomi('content', 'versions', 'restore', first_id)
assert restored['ContentItemVersionId'] not in (first_id, second_id)
assert restored['Latest'] and not restored['Published']
assert restored['TitlePart']['Title'] == 'Original version'
assert pomi('content', 'items', 'show', item_id)['ContentItemVersionId'] == second_id
pomi('content', 'versions', 'restore', first_id, status=409)
replaced = pomi('content', 'versions', 'restore', first_id, '--replace-draft', 'true')
assert replaced['ContentItemVersionId'] != restored['ContentItemVersionId']
assert pomi('content', 'versions', 'show', first_id) == archived
pomi('content', 'versions', 'delete', second_id, '--force', status=409)
pomi('content', 'versions', 'delete', replaced['ContentItemVersionId'], '--force', status=409)
pomi('content', 'versions', 'delete', first_id, failure=True)  # Confirmation required before sending.
assert pomi('content', 'versions', 'show', first_id) == archived
pomi('content', 'versions', 'delete', first_id, '--force')
pomi('content', 'versions', 'show', first_id, status=404)
pomi('content', 'versions', 'delete', first_id, '--force')
assert pomi('content', 'items', 'show', item_id)['ContentItemVersionId'] == second_id
for route in (f'api/content/{item_id}/versions', f'api/content/versions/{second_id}', f'api/content/versions/{second_id}/render'):
    try:
        urllib.request.urlopen(state['url'] + route, timeout=15)
        raise AssertionError('Anonymous access unexpectedly succeeded: ' + route)
    except urllib.error.HTTPError as error:
        assert error.code == 401, (route, error.code)
pomi('content', 'items', 'delete', item_id, '--force')
pomi('content', 'types', 'delete', name, '--force')
print('Content versions CLI smoke passed: five dynamic operations, paging, restore, protected versions, confirmation, retries, and anonymous denial.')
