#!/usr/bin/env python3
"""Exercise shared content contracts on a disposable loopback Orchard tenant."""
import copy
from datetime import datetime, timedelta, timezone
import json
import os
from pathlib import Path
import secrets
import subprocess
import sys
import tempfile
import urllib.parse

state_path = Path(sys.argv[1])
state = json.loads(state_path.read_text())
assert urllib.parse.urlparse(state['url']).hostname in ('127.0.0.1', 'localhost', '::1')
wrapper = Path(__file__).with_name('pomi-fixture.py')
prefix = 'Baseline' + secrets.token_hex(4)
created_items, created_types = [], []
report = []


def pomi(*args, body=None):
    env = os.environ.copy()
    env['OC_FIXTURE_CONFIG_HOME'] = config_home
    command = [sys.executable, str(wrapper), str(state_path), *args, '--output', 'json']
    if body is not None:
        command.append('--stdin')
    result = subprocess.run(command, input=json.dumps(body) if body is not None else None,
                            capture_output=True, text=True, env=env, timeout=90)
    if result.returncode:
        raise RuntimeError(f"{' '.join(args[:3])}: {result.returncode}: {result.stderr[:1200]}")
    return json.loads(result.stdout) if result.stdout.strip() else None


def define(suffix, parts, stereotype=None):
    name = prefix + suffix
    settings = {'draftable': True, 'versionable': True, 'creatable': True, 'listable': True}
    if stereotype:
        settings['stereotype'] = stereotype
    pomi('content', 'types', 'create', body={'name': name, 'displayName': name,
         'settings': {'ContentTypeSettings': settings},
         'parts': [{'name': p, 'partName': p, 'settings': {}} for p in parts]})
    created_types.append(name)
    schema = pomi('content', 'items', 'schema', name)
    assert schema
    return name


def create(payload):
    assert pomi('content', 'items', 'validate', body=payload)['isValid']
    item = pomi('content', 'items', 'create-draft', body=payload)
    created_items.append(item['ContentItemId'])
    return item


def read(item):
    return pomi('content', 'items', 'show', item['ContentItemId'], '--version', 'latest')


def update(item, payload):
    assert pomi('content', 'items', 'validate-update', item['ContentItemId'], body=payload)['isValid']
    pomi('content', 'items', 'update-draft', item['ContentItemId'], body=payload)
    return read(item)


def record(name):
    report.append(name)
    print(name + ': passed', flush=True)


with tempfile.TemporaryDirectory(prefix='shared-content-cli-', dir=state_path.parent) as config_home:
    pomi('context', 'list')
    pomi('context', 'add', prefix, state['url'], '--current')
    for feature in ['Menu', 'Lists', 'Taxonomies', 'Flows', 'PublishLater', 'ArchiveLater']:
        pomi('features', 'enable', 'OrchardCore.' + feature, '--api-force', 'true')
    pomi('api', 'refresh', '--force')
    try:
        # Inspect shipped menu schemas and preserve stable embedded identities on reorder.
        for name in ['Menu', 'LinkMenuItem']:
            pomi('content', 'types', 'show', name)
            pomi('content', 'items', 'schema', name)
        def link(label, children=None):
            item = {'ContentItemId': secrets.token_hex(13), 'ContentType': 'LinkMenuItem',
                    'DisplayText': label, 'LinkMenuItemPart': {'Url': '~/'+label.lower(), 'Target': ''}}
            if children:
                item['MenuItemsListPart'] = {'MenuItems': children}
            return item
        menu_payload = {'ContentType': 'Menu', 'TitlePart': {'Title': prefix},
                        'AliasPart': {'Alias': prefix.lower()},
                        'MenuItemsListPart': {'MenuItems': [link('Home'), link('Products', [link('Cloud')])]}}
        menu = create(menu_payload)
        menu_payload['MenuItemsListPart']['MenuItems'].reverse()
        saved = update(menu, menu_payload)
        assert saved['MenuItemsListPart'] == menu_payload['MenuItemsListPart']
        record('Nested menu identities and reorder round-trip')

        child_type = define('Child', ['TitlePart'])
        list_type = define('List', ['TitlePart', 'ListPart'])
        container = create({'ContentType': list_type, 'TitlePart': {'Title': prefix}})
        child_payload = {'ContentType': child_type, 'TitlePart': {'Title': 'Child'},
                         'ContainedPart': {'ListContentItemId': container['ContentItemId'],
                                           'ListContentType': list_type, 'Order': 0}}
        child = create(child_payload)
        child_payload['ContainedPart']['Order'] = 2
        saved = update(child, child_payload)
        assert saved['ContainedPart'] == child_payload['ContainedPart']
        record('List membership and order round-trip')

        leaf_type = define('Leaf', ['TitlePart'], 'Widget')
        section_type = define('Section', ['TitlePart', 'BagPart'], 'Widget')
        page_type = define('Page', ['TitlePart', 'FlowPart'])
        leaf = {'ContentType': leaf_type, 'ContentItemId': secrets.token_hex(13), 'TitlePart': {'Title': 'Leaf'}}
        section = {'ContentType': section_type, 'ContentItemId': secrets.token_hex(13),
                   'TitlePart': {'Title': 'Section'}, 'BagPart': {'ContentItems': [leaf]}}
        page_payload = {'ContentType': page_type, 'TitlePart': {'Title': 'Page'}, 'FlowPart': {'Widgets': [section]}}
        page = create(page_payload)
        page_payload['FlowPart']['Widgets'][0]['BagPart']['ContentItems'][0]['TitlePart']['Title'] = 'Updated leaf'
        saved = update(page, page_payload)
        assert saved['FlowPart'] == page_payload['FlowPart']
        record('Nested Flow and Bag identity/update round-trip')

        term_type = define('Term', ['TitlePart'])
        pomi('content', 'items', 'schema', 'Taxonomy')
        taxonomy = create({'ContentType': 'Taxonomy', 'TitlePart': {'Title': prefix},
                           'TaxonomyPart': {'TermContentType': term_type, 'Terms': []}})
        tax_id = taxonomy['ContentItemId']
        term = {'ContentType': term_type, 'ContentItemId': secrets.token_hex(13), 'TitlePart': {'Title': 'Parent'},
                'TermPart': {'TaxonomyContentItemId': tax_id}}
        nested_term = copy.deepcopy(term)
        nested_term['ContentItemId'] = secrets.token_hex(13)
        nested_term['TitlePart']['Title'] = 'Child'
        term['Terms'] = [nested_term]
        taxonomy_payload = {'ContentType': 'Taxonomy', 'TitlePart': {'Title': prefix},
                            'TaxonomyPart': {'TermContentType': term_type, 'Terms': [term]}}
        update(taxonomy, taxonomy_payload)
        assert read(taxonomy)['TaxonomyPart']['Terms'][0]['Terms'][0]['ContentItemId'] == nested_term['ContentItemId']
        record('Taxonomy hierarchy and embedded identity round-trip')

        scheduled_type = define('Scheduled', ['TitlePart', 'PublishLaterPart', 'ArchiveLaterPart'])
        future = (datetime.now(timezone.utc) + timedelta(days=1)).isoformat()
        scheduled_payload = {'ContentType': scheduled_type, 'TitlePart': {'Title': prefix},
                             'PublishLaterPart': {'ScheduledPublishUtc': future},
                             'ArchiveLaterPart': {'ScheduledArchiveUtc': future}}
        scheduled = create(scheduled_payload)
        saved = read(scheduled)
        assert saved['PublishLaterPart']['ScheduledPublishUtc'] and saved['ArchiveLaterPart']['ScheduledArchiveUtc']
        scheduled_payload['PublishLaterPart']['ScheduledPublishUtc'] = None
        scheduled_payload['ArchiveLaterPart']['ScheduledArchiveUtc'] = None
        saved = update(scheduled, scheduled_payload)
        assert saved['PublishLaterPart']['ScheduledPublishUtc'] is None
        assert saved['ArchiveLaterPart']['ScheduledArchiveUtc'] is None
        record('Publish/archive schedule set and clear round-trip')
        schemas = pomi('content', 'settings', 'list')
        (state_path.parent / 'shared-content-settings-schemas.json').write_text(json.dumps(schemas, indent=2)+'\n')
    finally:
        for item_id in reversed(created_items):
            pomi('content', 'items', 'delete', item_id, '--force')
        for name in reversed(created_types):
            pomi('content', 'types', 'delete', name, '--force')

(state_path.parent / 'shared-content-smoke.json').write_text(json.dumps(report, indent=2)+'\n')
print('Shared content round-trip smoke passed. This does not prove editor-side permission, rendering or scheduled-execution parity.')
