#!/usr/bin/env python3
"""Validate and stage one complete Orchard Core build before package publication."""
import argparse
import json
from pathlib import Path
import shutil
import xml.etree.ElementTree as ET
import zipfile

RIDS = ('linux-x64', 'linux-arm64', 'win-x64', 'win-arm64', 'osx-x64', 'osx-arm64')
THEMES = {name.lower(): name for name in (
    'SafeMode', 'TheAdmin', 'TheAgencyTheme', 'TheBlogTheme',
    'TheComingSoonTheme', 'TheSaaSTheme', 'TheTheme')}
REQUIRED = {
    'orchardcore.application.cms.targets', 'orchardcore.application.mvc.targets',
    'orchardcore.module.targets', 'orchardcore.theme.targets',
    'orchardcore.projecttemplates', 'orchardcore.remotemanagement', 'orchardcore.cli',
    *('orchardcore.cli.' + rid for rid in RIDS),
    *THEMES,
}


def is_build_package(package_id):
    key = package_id.lower()
    return key == 'orchardcore' or key.startswith('orchardcore.') or key in THEMES


def prepare(source, destination, version):
    # Translation packs are separately published dependencies, not projects in this build.
    central = Path(__file__).resolve().parents[2] / 'Directory.Packages.props'
    external = {item.attrib['Include'].lower(): item.attrib['Version']
                for item in ET.parse(central).findall('.//{*}PackageVersion')
                if item.attrib.get('Include', '').lower().startswith('orchardcore.')}
    packages = {}
    dependencies = []
    for path in sorted(source.rglob('*.nupkg')):
        with zipfile.ZipFile(path) as archive:
            nuspecs = [name for name in archive.namelist() if name.endswith('.nuspec')]
            if len(nuspecs) != 1:
                raise ValueError(f'{path.name}: expected one nuspec')
            metadata = ET.fromstring(archive.read(nuspecs[0])).find('./{*}metadata')
            package_id = metadata.findtext('{*}id')
            package_version = metadata.findtext('{*}version')
            if not is_build_package(package_id):
                raise ValueError(f'Unexpected package ID: {package_id}')
            if package_version != version:
                raise ValueError(f'{package_id}: expected version {version}, got {package_version}')
            key = package_id.lower()
            if key in packages:
                # Every native job independently tests the same RID-neutral installer.
                if key != 'orchardcore.cli':
                    raise ValueError(f'Duplicate package: {package_id}')
                continue
            packages[key] = path
            for dependency in metadata.findall('.//{*}dependency'):
                if is_build_package(dependency.attrib['id']):
                    dependencies.append((package_id, dependency.attrib['id'].lower(), dependency.attrib.get('version')))
            if key == 'orchardcore.projecttemplates':
                configs = [name for name in archive.namelist() if name.endswith('/.template.config/template.json')]
                if len(configs) != 5:
                    raise ValueError(f'Expected all five project templates, got {len(configs)}')
                for name in configs:
                    config = json.loads(archive.read(name))
                    if config['symbols']['OrchardVersion']['defaultValue'] != version:
                        raise ValueError(f'{name}: template uses a different Orchard version')
    missing = REQUIRED - packages.keys()
    if missing:
        raise ValueError(f'Missing required packages: {", ".join(sorted(missing))}')
    for owner, dependency, constraint in dependencies:
        expected = version if dependency in packages else external.get(dependency)
        if expected is None:
            raise ValueError(f'{owner}: missing build dependency {dependency}')
        if constraint not in (expected, f'[{expected}]', f'[{expected}, )'):
            raise ValueError(f'{owner}: {dependency} has inconsistent version {constraint}')
    destination.mkdir(parents=True, exist_ok=False)
    # Publish the tool pointer last so it cannot reference unpublished native packages.
    ordered = sorted(packages, key=lambda key: (key == 'orchardcore.cli', key))
    staged = []
    for key in ordered:
        source_path = packages[key]
        target = destination / source_path.name
        shutil.copy2(source_path, target)
        symbols = source_path.with_suffix('.snupkg')
        if symbols.exists():
            shutil.copy2(symbols, destination / symbols.name)
        staged.append(str(target))
    (destination / 'publish-order.txt').write_text('\n'.join(staged) + '\n')
    (destination / 'packages.json').write_text(json.dumps({'version': version, 'packages': ordered}, indent=2))
    print(f'Validated {len(packages)} OrchardCore packages at {version}, including templates and all six native tools.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('source', type=Path)
    parser.add_argument('destination', type=Path, help='New directory for the verified package set')
    parser.add_argument('version')
    args = parser.parse_args()
    prepare(args.source, args.destination, args.version)
