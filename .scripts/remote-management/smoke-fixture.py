#!/usr/bin/env python3
"""Exercise real CLI mutations on a disposable start-fixture.py tenant."""
import json
import os
from pathlib import Path
import secrets
import subprocess
import sys
import urllib.parse
import urllib.request

fixture_path = Path(sys.argv[1]).resolve()
state = json.loads(fixture_path.read_text())
assert urllib.parse.urlparse(state['url']).hostname in ('127.0.0.1', 'localhost', '::1')
wrapper = Path(__file__).with_name('pomi-fixture.py')
steps = []
prefix = 'Smoke' + secrets.token_hex(4)

def pomi(*args, body=None, expected=0, env=None):
    command = [sys.executable, str(wrapper), str(fixture_path), *args, '--output', 'json']
    if body is not None:
        command += ['--stdin']
    result = subprocess.run(command, input=json.dumps(body) if body is not None else None,
                            text=True, capture_output=True, env=env)
    # Only command names/status are recorded, never bodies or token material.
    steps.append({'command': ' '.join(args[:3]), 'exitCode': result.returncode})
    if result.returncode != expected:
        raise RuntimeError(f"{' '.join(args[:3])}: exit {result.returncode}; {result.stderr[:600]}")
    return json.loads(result.stdout) if result.stdout.strip() else None

pomi('context', 'add', 'smoke', state['url'], '--current')
pomi('api', 'refresh', '--force')
pomi('api', 'compatibility')
part = {'name':prefix+'Details','settings':{'ContentPartSettings':{'attachable':True,'reusable':True}},
        'fields':[{'name':'Summary','fieldName':'TextField','settings':{}}]}
pomi('content','parts','create',body=part)
ctype = {'name':prefix,'displayName':prefix+' article','settings':{'ContentTypeSettings':{'draftable':True,'versionable':True,'creatable':True,'listable':True}},
         'parts':[{'name':'TitlePart','partName':'TitlePart','settings':{}},{'name':prefix+'Details','partName':prefix+'Details','settings':{}}]}
pomi('content','types','create',body=ctype)
pomi('content','items','schema',prefix)
payload = {'ContentType':prefix,'TitlePart':{'Title':'CLI smoke draft'},prefix+'Details':{'Summary':{'Text':'Round trip'}}}
assert pomi('content','items','validate',body=payload)['isValid']
item = pomi('content','items','create-draft',body=payload)
item_id = item['ContentItemId']
assert not pomi('content','items','show',item_id,'--version','draft')['Published']
assert pomi('content','items','validate-update',item_id,body=payload)['isValid']
assert pomi('content','items','list','--content-type',prefix,'--status','published')['totalCount'] == 0
# Destructive input must fail before sending a mutation.
pomi('content','items','delete',item_id,expected=1)
assert pomi('content','items','show',item_id,'--version','draft')['ContentItemId'] == item_id
pomi('content','items','delete',item_id,'--force')
pomi('content','types','delete',prefix,'--force')
pomi('content','parts','delete',prefix+'Details','--force')
role = pomi('roles','create',body={'roleName':prefix,'permissionNames':[]})
role_id = role['roleId']
assert pomi('roles','show',role_id)['roleName'] == prefix
env = os.environ.copy()
env['OC_SMOKE_PASSWORD'] = secrets.token_urlsafe(32) + 'aA1!'
user = pomi('users','create','--user-name',prefix,'--email',prefix+'@example.test',
          '--is-enabled','true','--role-names',json.dumps([prefix]),'--password-env','OC_SMOKE_PASSWORD',env=env)
assert 'password' not in json.dumps(user).lower()
user_id = user['userId']
assert pomi('users','show',user_id)['roleNames'] == [prefix]
pomi('users','disable',user_id,'--force')
pomi('users','delete',user_id,'--force')
pomi('roles','delete',role_id,'--force')
# Custom CSS uses the same Media store and permissions as the admin library.
css = Path(state['root']) / (prefix + '.css')
css.write_text('/* CLI smoke */\nbody { color: #123; }\n')
remote = prefix + '/style-v1.css'
assert '.css' in pomi('media','constraints','show')['allowedFileExtensions']
pomi('media','folders','create','--name',prefix)
asset = pomi('media','files','upload','style-v1.css','--path',prefix,'--file',str(css))
assert asset['filePath'] == remote
assert pomi('media','files','show',remote)['size'] == css.stat().st_size
with urllib.request.urlopen(urllib.parse.urljoin(state['url'], asset['url'])) as response:
    assert response.read() == css.read_bytes()
pomi('media','files','delete',remote,expected=1)
pomi('media','files','delete',remote,'--force')
pomi('media','files','show',remote,expected=4)
assert pomi('media','files','list','--path',prefix)['totalCount'] == 0
pomi('media','folders','delete',prefix,'--force')
report = {'passed':True,'steps':steps,'prefix':prefix}
(Path(state['root']) / 'smoke.json').write_text(json.dumps(report,indent=2))
print(json.dumps({'passed':True,'commands':len(steps)}))
