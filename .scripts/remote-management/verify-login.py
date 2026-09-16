#!/usr/bin/env python3
"""After a manual device/browser login, verify reuse, refresh and logout on a fixture."""
import json
import os
from pathlib import Path
import subprocess
import sys
import urllib.error
import urllib.parse
import urllib.request

state_path = Path(sys.argv[1]).resolve()
state = json.loads(state_path.read_text())
assert urllib.parse.urlparse(state['url']).hostname in ('localhost','127.0.0.1','::1')
config = Path(state['OC_CONFIG_HOME'])
files = list((config/'credentials').glob('*.json'))
assert len(files) == 1, 'Sign into exactly one fixture context with OC_FIXTURE_HUMAN=1 first'
credential = files[0]
env = os.environ.copy()
env['OC_FIXTURE_HUMAN'] = '1'
wrapper = Path(__file__).with_name('pomi-fixture.py')
def pomi(*args):
    result = subprocess.run([sys.executable,str(wrapper),str(state_path),*args,'--output','json'],env=env,text=True,capture_output=True)
    assert result.returncode == 0, f'{args[0]} failed; inspect fixture authentication without printing tokens'
    return json.loads(result.stdout)
pomi('content','items','list','--take','1')
original = json.loads(credential.read_text())
assert original.get('refreshToken'), 'Login did not return a refresh token'
original['expiresAt'] = '2000-01-01T00:00:00+00:00'
credential.write_text(json.dumps(original))
pomi('content','items','list','--take','1')
refreshed = json.loads(credential.read_text())
assert refreshed['expiresAt'] != original['expiresAt']
assert refreshed['refreshToken'] != original['refreshToken'], 'Expected refresh-token rotation'
if os.name != 'nt':
    assert credential.stat().st_mode & 0o777 == 0o600
    assert credential.parent.stat().st_mode & 0o777 == 0o700
logout = pomi('logout')
assert logout['removed'] and logout['revoked']
assert not credential.exists()
body = urllib.parse.urlencode({'grant_type':'refresh_token','client_id':'orchardcore-cli','refresh_token':refreshed['refreshToken']}).encode()
request = urllib.request.Request(state['url']+'connect/token',data=body,headers={'Content-Type':'application/x-www-form-urlencoded'})
try:
    urllib.request.urlopen(request,timeout=30)
    raise AssertionError('The revoked refresh token was accepted')
except urllib.error.HTTPError as error:
    assert error.code == 400
    assert json.load(error)['error'] == 'invalid_grant'
report = {'separateProcessReuse':True,'refreshRotation':True,'ownerOnlyFilePermissions':os.name!='nt','logoutRevocation':True}
(Path(state['root'])/'login-verification.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report))
