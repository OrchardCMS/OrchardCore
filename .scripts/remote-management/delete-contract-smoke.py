#!/usr/bin/env python3
"""Verify that deleting a missing resource returns a 404 problem on every management API."""
import json
from pathlib import Path
import secrets
import sys
import urllib.error
import urllib.parse
import urllib.request

state = json.loads(Path(sys.argv[1]).read_text())
base = state['url']

features = ['OrchardCore.ContentTypes', 'OrchardCore.DataLocalization', 'OrchardCore.Deployment', 'OrchardCore.Deployment.Remote',
    'OrchardCore.Layers', 'OrchardCore.Localization', 'OrchardCore.Search.Lucene', 'OrchardCore.Media', 'OrchardCore.Placements', 'OrchardCore.Queries',
    'OrchardCore.RateLimits', 'OrchardCore.Roles', 'OrchardCore.Shortcodes.Templates', 'OrchardCore.Sitemaps',
    'OrchardCore.Templates', 'OrchardCore.Tenants.FeatureProfiles', 'OrchardCore.UrlRewriting', 'OrchardCore.Workflows']
missing = 'missing' + secrets.token_hex(6)
routes = [
    'api/content/' + missing,
    'api/content/versions/' + missing,
    'api/content-definition/parts/' + missing,
    'api/content-definition/parts/' + missing + '/fields/' + missing,
    'api/content-definition/types/' + missing,
    'api/deployment/artifacts/' + secrets.token_hex(16),
    'api/deployment/plans/by-id?id=999999',
    'api/deployment/plans/steps/by-id?planId=999999&stepId=' + missing,
    'api/deployment/remote-clients/' + missing,
    'api/deployment/remote-instances/' + missing,
    'api/indexes/lucene/by-id?id=' + missing,
    'api/layers/by-name?name=' + missing,
    'api/localization/cultures/not-a-culture',
    'api/localization/translations?culture=fr&context=' + missing + '&key=' + missing,
    'api/media/profiles/by-name?name=' + missing,
    'api/openid/applications/by-client-id?clientId=' + missing,
    'api/openid/scopes/by-name?name=' + missing,
    'api/placements/by-shape?shapeType=' + missing,
    'api/queries/named/' + missing,
    'api/rate-limits/policies/by-id?policyId=' + missing,
    'api/rate-limits/policies/limiters/by-id?policyId=' + missing + '&limiterId=' + missing,
    'api/roles/' + missing,
    'api/shortcode-templates/by-name?name=' + missing,
    'api/sitemaps/' + missing,
    'api/sitemaps/' + missing + '/sources/' + missing,
    'api/templates/' + missing,
    'api/tenants/' + missing,
    'api/tenants/feature-profiles/by-id?id=' + missing,
    'api/url-rewriting/rules/' + missing,
    'api/users/' + missing,
    'api/workflows/instances/' + missing,
    'api/workflows/types/' + missing,
]


def token():
    data = urllib.parse.urlencode({'grant_type': 'client_credentials', 'client_id': state['OC_CLIENT_ID'],
        'client_secret': state['OC_CLIENT_SECRET'], 'scope': 'orchardcore.management'}).encode()
    with urllib.request.urlopen(base + 'connect/token', data=data, timeout=30) as response:
        return json.load(response)['access_token']


def send(route, method, bearer):
    request = urllib.request.Request(base + route, method=method, headers={'Authorization': 'Bearer ' + bearer, 'Accept': 'application/json'})
    try:
        response = urllib.request.urlopen(request, timeout=60)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        return response.status, response.headers.get('Content-Type', ''), response.read().decode(errors='replace')


bearer = token()
for feature in features:
    status, _, text = send('api/features/' + feature + ':enable?force=true', 'POST', bearer)
    assert status in (200, 204), (feature, status, text[:300])

failures = []
for route in routes:
    status, content_type, text = send(route, 'DELETE', bearer)
    # A route that doesn't exist also returns 404, without a problem.
    if status != 404 or not content_type.startswith('application/problem+json'):
        failures.append((route, status, content_type, text[:200]))

assert not failures, failures
print(f'PASS: {len(routes)} delete routes return a 404 problem for a missing resource')
