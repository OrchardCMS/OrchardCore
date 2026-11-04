#!/usr/bin/env python3
"""Exercise embedded CMS installation against published packages, without a tenant fixture."""
import argparse
import json
import os
from pathlib import Path
import secrets
import signal
import socket
import subprocess
import tempfile
import time
import urllib.error
import urllib.request
import urllib.parse
import xml.etree.ElementTree as ET

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('pomi', type=Path, help='Native CLI from the published build to test')
parser.add_argument('--source', help='Explicit NuGet feed for preview dependencies')
args = parser.parse_args()
pomi = str(args.pomi.resolve())
if args.source and Path(args.source).is_dir():
    args.source = str(Path(args.source).resolve())
source_args = ['--source', args.source] if args.source else []


def free_port():
    with socket.socket() as listener:
        listener.bind(('127.0.0.1', 0))
        return listener.getsockname()[1]


def responds(url):
    try:
        with urllib.request.urlopen(url, timeout=1) as response:
            return response.status == 200
    except (OSError, urllib.error.URLError):
        return False


with tempfile.TemporaryDirectory(prefix='pomi-install-smoke-') as scratch:
    root = Path(scratch)
    # Preview packages may be supplied by an inherited config without --source.
    inherited = root / 'inherited-packages'
    inherited.mkdir()
    config = ET.Element('configuration')
    sources = ET.SubElement(config, 'packageSources')
    ET.SubElement(sources, 'clear')
    ET.SubElement(sources, 'add', key='nuget.org', value='https://api.nuget.org/v3/index.json')
    ET.SubElement(sources, 'add', key='InheritedOnly', value=str(inherited))
    if args.source:
        ET.SubElement(sources, 'add', key='Preview', value=args.source)
    ET.ElementTree(config).write(root / 'NuGet.Config', encoding='utf-8', xml_declaration=True)
    env = os.environ.copy()
    env['OC_CONFIG_HOME'] = str(root / 'config')
    password = 'Aa1!' + secrets.token_hex(20)
    env['OC_INSTALL_PASSWORD'] = password
    # A new site must not inherit an unrelated tenant's configuration.
    env['OrchardCore__OrchardCore_AutoSetup__Tenants__1__ShellName'] = 'Unexpected'
    no_sdk = env.copy()
    no_sdk['PATH'] = str(root / 'empty-path')
    diagnostics = subprocess.run([pomi, 'doctor', '--output', 'json'], env=no_sdk, capture_output=True, text=True, check=True, timeout=15)
    assert json.loads(diagnostics.stdout)['localInstallWarning'], diagnostics.stdout
    assert not diagnostics.stderr, diagnostics.stderr
    missing = subprocess.run([pomi, 'install', str(root / 'missing-sdk'), '--site-name', 'Missing SDK', '--email', 'admin@example.com', '--password-env', 'OC_INSTALL_PASSWORD'], env=no_sdk, capture_output=True, text=True, timeout=15)
    assert missing.returncode != 0 and '.NET' in missing.stderr, missing
    assert not (root / 'missing-sdk').exists()

    site = root / 'CMS with spaces'
    base = [pomi, 'install', str(site), '--site-name', 'Embedded CMS', '--email', 'admin@example.com', '--recipe-name', 'SaaS', '--output', 'json']
    completed = subprocess.run([*base, '--password-env', 'OC_INSTALL_PASSWORD'], env=env, capture_output=True, text=True, timeout=600)
    assert completed.returncode == 0, completed.stderr
    assert 'Now listening on:' not in completed.stderr, completed.stderr
    assert '127.0.0.1' not in completed.stderr, completed.stderr
    assert 'Failed to determine the https port' not in completed.stderr, completed.stderr
    assert 'Build succeeded.' not in completed.stderr, completed.stderr
    result = json.loads(completed.stdout)
    selected = urllib.parse.urlsplit(result['listenUrl'])
    assert selected.scheme == 'https' and selected.hostname == 'localhost' and selected.port > 0, result
    assert json.loads((site / 'appsettings.json').read_text())['Urls'] == result['listenUrl']
    for profile in json.loads((site / 'Properties/launchSettings.json').read_text())['profiles'].values():
        if profile.get('commandName') == 'Project':
            assert profile['applicationUrl'] == result['listenUrl'], profile
    # Explicit occupied ports fail before SDK discovery, password input, or file creation.
    with socket.socket() as busy:
        busy.bind(('127.0.0.1', 0))
        busy.listen()
        busy_url = f"http://127.0.0.1:{busy.getsockname()[1]}"
        blocked_path = root / 'busy-site'
        blocked = subprocess.run([pomi, 'install', str(blocked_path), '--site-name', 'Busy',
                                  '--email', 'admin@example.com', '--urls', busy_url],
                                 env=no_sdk, capture_output=True, text=True, timeout=15)
        assert blocked.returncode != 0 and 'already in use' in blocked.stderr, blocked
        assert not blocked_path.exists()

    assert ET.parse(site / 'NuGet.Config').find('./packageSources/clear') is None
    restored_sources = json.loads((site / 'obj/project.assets.json').read_text())['project']['restore']['sources']
    assert str(inherited) in restored_sources, restored_sources
    if args.source:
        assert args.source in restored_sources, restored_sources
    # A subsequent ordinary restore must see the same sources as installation.
    restore = subprocess.run(['dotnet', 'restore', '--force-evaluate', '--disable-build-servers'], cwd=site,
                             env=env, capture_output=True, text=True, timeout=120)
    assert restore.returncode == 0, restore.stderr + restore.stdout
    assert restored_sources == json.loads((site / 'obj/project.assets.json').read_text())['project']['restore']['sources']
    assert result['tenantState'] == 'Running' and result['tenant'] == 'Default', result
    assert json.loads((site / 'App_Data/tenants.json').read_text())['Default']['State'] == 'Running'
    if os.name != 'nt':
        assert (site / 'App_Data').stat().st_mode & 0o777 == 0o700
    assert not (site / 'App_Data/Sites/Unexpected').exists()
    for path in site.rglob('*'):
        if path.is_file() and path.stat().st_size < 20_000_000:
            assert password.encode() not in path.read_bytes(), f'Plaintext password in {path.relative_to(site)}'
    assert password not in completed.stdout + completed.stderr
    refused = subprocess.run([*base, '--password-env', 'OC_INSTALL_PASSWORD'], env=env, capture_output=True, text=True, timeout=15)
    assert refused.returncode != 0 and 'overwrite' in refused.stderr, refused

    # A setup failure must be reported without claiming a ready site.
    failure = subprocess.run([pomi, 'install', str(root / 'bad-recipe'), '--site-name', 'Invalid recipe', '--email', 'admin@example.com', '--recipe-name', 'RecipeThatDoesNotExist', '--clear-sources', '--password-env', 'OC_INSTALL_PASSWORD', '--output', 'json', *source_args], env=env, capture_output=True, text=True, timeout=600)
    assert failure.returncode != 0 and not failure.stdout.strip(), failure
    assert password not in failure.stderr
    assert 'Installation diagnostics:' in failure.stderr, failure.stderr
    assert 'The AutoSetup failed installing the site' in failure.stderr, failure.stderr
    assert (root / 'bad-recipe/Program.cs').exists()
    assert ET.parse(root / 'bad-recipe/NuGet.Config').find('./packageSources/clear') is not None
    cleared_sources = json.loads((root / 'bad-recipe/obj/project.assets.json').read_text())['project']['restore']['sources']
    assert str(inherited) not in cleared_sources, cleared_sources
    assert 'https://api.nuget.org/v3/index.json' in cleared_sources
    if args.source:
        assert args.source in cleared_sources, cleared_sources

    # Check --run in the foreground, stdin input, path prefix, and cancellation.
    port = free_port()
    run_url = f'http://127.0.0.1:{port}'
    second_port = free_port()
    while second_port == port:
        second_port = free_port()
    second_url = f'http://127.0.0.1:{second_port}'
    output_path = root / 'run.json'
    error_path = root / 'run.log'
    with output_path.open('w') as output, error_path.open('w') as errors:
        process = subprocess.Popen([pomi, 'install', str(root / 'running-site'), '--site-name', 'Running CMS', '--email', 'admin@example.com', '--recipe-name', 'SaaS', '--request-url-prefix', 'news', '--enable-remote-management', '--password-stdin', '--run', '--urls', run_url + ';' + second_url, '--output', 'json', *source_args], env=env, stdin=subprocess.PIPE, stdout=output, stderr=errors, text=True, start_new_session=os.name != 'nt', creationflags=subprocess.CREATE_NEW_PROCESS_GROUP if os.name == 'nt' else 0)
        try:
            process.stdin.write(password + '\n')
            process.stdin.close()
            deadline = time.monotonic() + 600
            while not (responds(run_url + '/news/') and responds(second_url + '/news/')):
                assert process.poll() is None, error_path.read_text()
                assert time.monotonic() < deadline, error_path.read_text()
                time.sleep(0.25)
            installed = json.loads(output_path.read_text())
            assert installed['url'].rstrip('/') == run_url + '/news'
            context = installed['context']
            assert context
            # First use obtains a client-credentials token without a browser or device login.
            features = subprocess.run([pomi, '--context', context, 'api', 'invoke', 'GET',
                                       'api/features', '--output', 'json'], env=env,
                                      capture_output=True, text=True, timeout=30)
            assert features.returncode == 0, features.stderr
            tenant = subprocess.run([pomi, '--context', context, 'tenants', 'install', 'Managed',
                                     '--site-name', 'Managed', '--user-name', 'admin',
                                     '--email', 'admin@example.com', '--recipe-name', 'Blank',
                                     '--request-url-prefix', 'managed', '--password-env', 'OC_INSTALL_PASSWORD',
                                     '--enable-remote-management', '--output', 'json'], env=env,
                                    capture_output=True, text=True, timeout=120)
            assert tenant.returncode == 0, tenant.stderr
            managed = json.loads(tenant.stdout)
            assert 'clientCredentials' not in managed, managed
            child_context = managed['context']
            assert child_context != context
            child_features = subprocess.run([pomi, '--context', child_context, 'api', 'invoke', 'GET',
                                             'api/features', '--output', 'json'], env=env,
                                            capture_output=True, text=True, timeout=30)
            assert child_features.returncode == 0, child_features.stderr
            assert password not in tenant.stdout + tenant.stderr

            run_logs = error_path.read_text()
            assert run_logs.count('Now listening on:') == 2, run_logs
            assert run_logs.count('Application started.') == 1, run_logs
            assert f'Now listening on: {run_url}' in run_logs, run_logs
            assert f'Now listening on: {second_url}' in run_logs, run_logs
            if os.name == 'nt':
                process.send_signal(signal.CTRL_BREAK_EVENT)
            else:
                process.send_signal(signal.SIGINT)
            process.wait(timeout=20)
            deadline = time.monotonic() + 5
            while responds(run_url + '/news/') and time.monotonic() < deadline:
                time.sleep(0.25)
            assert not responds(run_url + '/news/'), 'Foreground server survived CLI cancellation'
            assert not responds(second_url + '/news/'), 'Second listener survived CLI cancellation'
            assert password not in output_path.read_text() + error_path.read_text()
        finally:
            if os.name != 'nt':
                try:
                    os.killpg(process.pid, signal.SIGKILL)
                except ProcessLookupError:
                    pass
            elif process.poll() is None:
                subprocess.run(['taskkill', '/PID', str(process.pid), '/T', '/F'], check=False, capture_output=True)
            process.wait()

    print(json.dumps({'version': result['packageVersion'], 'embeddedTemplate': True, 'autoSetup': True,
                      'foregroundRun': True, 'applicationContexts': True, 'tenantProvisioning': True, 'cancellationStopsServer': True, 'missingSdkWarning': True,
                      'existingFilesProtected': True, 'setupFailureReported': True, 'noPlaintextAdminPassword': True}))
