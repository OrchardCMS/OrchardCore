#!/usr/bin/env python3
"""Verify and install NativeAOT tool packages from an isolated local source."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import struct
import subprocess
import tempfile
import xml.etree.ElementTree as ET
import zipfile

PACKAGE_ID = 'OrchardCore.Cli'
RIDS = {'linux-x64', 'linux-arm64', 'win-x64', 'win-arm64', 'osx-arm64', 'osx-x64'}
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('directory', type=Path)
parser.add_argument('rid', choices=sorted(RIDS))
parser.add_argument('version')
args = parser.parse_args()
directory = args.directory.resolve()
executable_name = 'pomi.exe' if args.rid.startswith('win-') else 'pomi'
pointer = directory / f'{PACKAGE_ID}.{args.version}.nupkg'
implementation = directory / f'{PACKAGE_ID}.{args.rid}.{args.version}.nupkg'


def has_clr_header(binary):
    """Distinguish managed PE assemblies from Windows native support DLLs."""
    # https://learn.microsoft.com/windows/win32/debug/pe-format
    assert binary[:2] == b'MZ', 'Expected a Windows PE image'
    pe = struct.unpack_from('<I', binary, 0x3c)[0]
    assert binary[pe:pe + 4] == b'PE\0\0', 'Invalid PE signature'
    optional_size = struct.unpack_from('<H', binary, pe + 20)[0]
    optional = pe + 24
    magic = struct.unpack_from('<H', binary, optional)[0]
    assert magic in (0x10b, 0x20b), 'Expected PE32 or PE32+'
    directories = 96 if magic == 0x10b else 112
    count = struct.unpack_from('<I', binary, optional + directories - 4)[0]
    if count <= 14:
        return False
    assert optional_size >= directories + 15 * 8, 'Truncated PE data directories'
    clr_address, clr_size = struct.unpack_from('<II', binary, optional + directories + 14 * 8)
    return clr_address != 0 or clr_size != 0


def inspect_package(path, package_id, package_type):
    with zipfile.ZipFile(path) as package:
        names = package.namelist()
        assert not any(Path(name).name in {'oc', 'oc.exe', 'oc.dll', 'oc.cmd'} for name in names), 'Package contains the retired CLI executable; clean stale build outputs before packing'
        nuspec = ET.fromstring(package.read(next(name for name in names if name.endswith('.nuspec'))))
        assert nuspec.find('.//{*}id').text == package_id
        assert nuspec.find('.//{*}version').text == args.version
        assert nuspec.find('.//{*}packageType').get('name') == package_type
        assert 'README.md' in names and 'LICENSE' in names
        settings_path = next(name for name in names if name.endswith('/DotnetToolSettings.xml'))
        settings = ET.fromstring(package.read(settings_path))
        assert settings.get('Version') == '2'
        command = settings.find('./Commands/Command')
        assert command.get('Name') == 'pomi'
        if package_type == 'DotnetTool':
            platforms = settings.findall('./RuntimeIdentifierPackages/RuntimeIdentifierPackage')
            assert {platform.get('RuntimeIdentifier') for platform in platforms} == RIDS
            for platform in platforms:
                assert platform.get('Id') == f"{PACKAGE_ID}.{platform.get('RuntimeIdentifier')}"
            assert not any(name.endswith(('/pomi', '/pomi.exe', '.dll')) for name in names)
            return None

        assert command.get('Runner') == 'executable', ET.tostring(settings)
        assert command.get('EntryPoint') == executable_name
        root = str(Path(settings_path).parent).replace('\\', '/')
        assert f'{root}/QRCoder.LICENSE.txt' in names
        assert not any(name.endswith(('.runtimeconfig.json', '.deps.json')) for name in names)
        for name in names:
            if name.lower().endswith('.dll'):
                assert not has_clr_header(package.read(name)), f'Managed assembly in native package: {name}'
        binary = package.read(f'{root}/{executable_name}')
        expected_magic = b'MZ' if args.rid.startswith('win-') else b'\x7fELF' if args.rid.startswith('linux-') else b'\xcf\xfa\xed\xfe'
        assert binary.startswith(expected_magic), 'Package must contain a native executable'
        if args.rid.startswith('win-'):
            assert not has_clr_header(binary), 'Tool entry point must be native'
        return hashlib.sha256(binary).hexdigest()


inspect_package(pointer, PACKAGE_ID, 'DotnetTool')
binary_hash = inspect_package(implementation, f'{PACKAGE_ID}.{args.rid}', 'DotnetToolRidPackage')
with tempfile.TemporaryDirectory(prefix='pomi-tool-smoke-') as scratch:
    scratch = Path(scratch)
    env = os.environ.copy()
    env['DOTNET_CLI_HOME'] = str(scratch / 'dotnet-home')
    env['NUGET_PACKAGES'] = str(scratch / 'packages')
    env['OC_CONFIG_HOME'] = str(scratch / 'config')
    env['DOTNET_CLI_TELEMETRY_OPTOUT'] = '1'
    env['DOTNET_NOLOGO'] = '1'
    tool_path = scratch / 'bin'
    subprocess.run(['dotnet', 'tool', 'install', PACKAGE_ID, '--tool-path', str(tool_path),
                    '--source', str(directory), '--source', 'https://api.nuget.org/v3/index.json',
                    '--version', args.version],
                   env=env, check=True, timeout=120)
    # The SDK links the executable on Unix and writes a pomi.cmd launcher on Windows.
    installed = tool_path / ('pomi.cmd' if os.name == 'nt' else 'pomi')
    assert installed.is_file(), list(tool_path.iterdir())
    # Run the installed command without dotnet on PATH or an available DOTNET_ROOT.
    native_env = env.copy()
    native_env['PATH'] = str(tool_path)
    for variable in ('DOTNET_ROOT', 'DOTNET_ROOT_ARM64', 'DOTNET_ROOT_X64', 'DOTNET_ROOT(x86)'):
        native_env[variable] = str(scratch / 'no-runtime')
    for command in ([], ['--version'], ['doctor', '--output', 'json']):
        result = subprocess.run([str(installed), *command], env=native_env, shell=os.name == 'nt',
                                capture_output=True, text=True, check=True, timeout=10)
        assert not result.stderr, result.stderr
        if not command:
            assert 'Pomi: Orchard Core command-line interface' in result.stdout
        elif command[0] == '--version':
            assert result.stdout.strip().split('+')[0] == args.version, result.stdout
        else:
            assert json.loads(result.stdout)['runtimeIdentifier'] == args.rid
    subprocess.run(['dotnet', 'tool', 'uninstall', PACKAGE_ID, '--tool-path', str(tool_path)],
                   env=env, check=True, timeout=60)
    assert not installed.exists()

report = {'rid': args.rid, 'version': args.version, 'nativeBinarySha256': binary_hash,
          'packages': {path.name: hashlib.sha256(path.read_bytes()).hexdigest()
                       for path in (pointer, implementation)},
          'localSourceInstall': True, 'executionWithoutDotnetOnPath': True, 'uninstall': True}
(directory / 'tool-verification.json').write_text(json.dumps(report, indent=2) + '\n')
(directory / 'INSTALL.md').write_text(
    f'# Install pomi for {args.rid}\n\n'
    'With .NET SDK 10 or later, open a terminal in this extracted directory.\n'
    'Keep both .nupkg files together, then run:\n\n'
    f'```sh\ndotnet tool install --global {PACKAGE_ID} --add-source . --version {args.version}\n'
    'pomi --version\noc\n```\n\n'
    'To upgrade an existing installation, replace `install` with `update`.\n'
    'The installed executable runs natively without a separate .NET runtime.\n')
print(json.dumps(report, indent=2))
