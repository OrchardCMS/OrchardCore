#!/usr/bin/env python3
"""Run pomi against credentials from start-fixture.py without displaying them."""
import json
import os
from pathlib import Path
import subprocess
import sys

state = json.loads(Path(sys.argv[1]).read_text())
env = os.environ.copy()
env.update({key: value for key, value in state.items() if key.startswith('OC_')})
env['OC_CONFIG_HOME'] = os.environ.get('OC_FIXTURE_CONFIG_HOME', state['OC_CONFIG_HOME'])
env['OC_CLIENT_ID'] = os.environ.get('OC_FIXTURE_CLIENT_ID', state['OC_CLIENT_ID'])
if os.environ.get('OC_FIXTURE_HUMAN') == '1':
    env.pop('OC_CLIENT_ID', None)
    env.pop('OC_CLIENT_SECRET', None)
repo = Path(__file__).resolve().parents[2]
binary = os.environ.get('OC_FIXTURE_BINARY')
command = [binary] if binary else ['dotnet', str(repo / 'src/OrchardCore.Cli/bin/Debug/net10.0/pomi.dll')]
result = subprocess.run(command + sys.argv[2:], env=env)
raise SystemExit(result.returncode)
