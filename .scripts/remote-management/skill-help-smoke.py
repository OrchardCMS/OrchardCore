#!/usr/bin/env python3
"""Check CLI skill shell examples against cached help without executing examples.

Refresh an isolated context against start-fixture.py first. The fixture must
enable all features used by the skills. This checks command paths and option
names, not payloads, argument values, permissions, or workflow outcomes.
"""
import argparse
import json
import os
from pathlib import Path
import re
import shlex
import subprocess

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('binary', type=Path, help='Native Pomi executable')
parser.add_argument('config_home', type=Path, help='Isolated configuration with refreshed metadata')
parser.add_argument('--skills-root', type=Path, default=Path(__file__).resolve().parents[2] / '.agents/skills/pomi/skills')
args = parser.parse_args()
env = {key: value for key, value in os.environ.items() if not key.startswith('OC_')}
env.update(OC_CONFIG_HOME=str(args.config_home.resolve()), NO_COLOR='1', TERM='dumb')
cache = {}


def help_for(path):
    if path not in cache:
        # Only command names discovered in help reach this subprocess. No shell,
        # example arguments, credentials, redirects, or mutations are executed.
        result = subprocess.run([str(args.binary.resolve()), *path, '--help'],
                                env=env, text=True, capture_output=True, timeout=30)
        if result.returncode:
            raise ValueError(f'Help failed for {" ".join(path) or "root"} (exit {result.returncode})')
        sections = {}
        section = ''
        for line in result.stdout.splitlines():
            if line and not line.startswith(' '):
                section = line.rstrip(':')
                sections[section] = []
            else:
                sections.setdefault(section, []).append(line)
        if not {'Usage', 'Options'} <= sections.keys():
            raise ValueError('Unrecognized help layout; update the checker')
        commands = [match.group(1) for line in sections.get('Commands', [])
                    if (match := re.match(r'^  ([\w-]+)(?:\s|$)', line))]
        options = {}
        for line in sections['Options']:
            if not re.match(r'^  -', line):
                continue
            syntax = re.split(r'\s{2,}', line.strip())[0]
            for option in re.findall(r'(?<![\w-])--?[\w?-]+', syntax):
                options[option] = '<' in syntax
        cache[path] = commands, options
    return cache[path]


failures = []
examples = 0
files = sorted(args.skills_root.glob('orchardcore-cli*/**/*.md'))
for file in files:
    content = file.read_text()
    for block in re.finditer(r'```(?:bash|sh)\n(.*?)```', content, re.S):
        line_number = content.count('\n', 0, block.start()) + 2
        joined = ''
        for offset, line in enumerate(block.group(1).splitlines()):
            if not joined:
                start_line = line_number + offset
            joined += line.rstrip('\\') + ' '
            if line.endswith('\\'):
                continue
            example = joined.strip()
            joined = ''
            if not example.startswith('pomi '):
                continue
            examples += 1
            path = ()
            positionals = []
            try:
                tokens = shlex.split(example)[1:]
                while tokens:
                    token = tokens.pop(0)
                    if token in ('>', '>>', '|'):
                        break
                    children, options = help_for(path)
                    if token.startswith('-'):
                        key = token.split('=')[0]
                        if key not in options:
                            raise ValueError(f'Unknown option {key}')
                        if options[key] and '=' not in token:
                            tokens.pop(0)
                    elif token in children and not positionals:
                        path += (token,)
                    elif children:
                        raise ValueError(f'Unknown command {token}')
                    else:
                        positionals.append(token)
            except (ValueError, IndexError) as error:
                failures.append({'file': str(file.relative_to(args.skills_root)),
                                 'line': start_line, 'command': ' '.join(path),
                                 'error': str(error) or 'Missing option value'})

if not examples:
    failures.append({'error': 'No shell examples found'})
print(json.dumps({'files': len(files), 'examples': examples, 'helpPages': len(cache),
                  'failures': failures}, indent=2))
raise SystemExit(bool(failures))
