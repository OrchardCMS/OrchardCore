#!/usr/bin/env python3
"""Regression checks for the package publication boundary."""
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest
import zipfile

spec = importlib.util.spec_from_file_location('prepare_packages', Path(__file__).with_name('prepare-packages.py'))
publisher = importlib.util.module_from_spec(spec)
spec.loader.exec_module(publisher)
VERSION = '4.0.0'


class PreparePackagesTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.source = self.root / 'input'
        self.source.mkdir()
        self.output = self.root / 'output'
        for key in publisher.REQUIRED:
            self.package(publisher.THEMES.get(key, 'OrchardCore.' + key.removeprefix('orchardcore.')))

    def package(self, package_id, version=VERSION, template_version=VERSION, dependency=None, folder='', dependency_version='0.0.1'):
        path = self.source / folder / f'{package_id}.{version}.nupkg'
        path.parent.mkdir(parents=True, exist_ok=True)
        dependencies = '' if dependency is None else f'<dependencies><dependency id="{dependency}" version="{dependency_version}" /></dependencies>'
        with zipfile.ZipFile(path, 'w') as archive:
            archive.writestr(package_id + '.nuspec', f'<package><metadata><id>{package_id}</id><version>{version}</version>{dependencies}</metadata></package>')
            if package_id.lower() == 'orchardcore.projecttemplates':
                for i in range(5):
                    archive.writestr(f'content/template{i}/.template.config/template.json', json.dumps({
                        'symbols': {'OrchardVersion': {'defaultValue': template_version}},
                    }))
        return path

    def test_complete_set_deduplicates_pointer_and_publishes_it_last(self):
        self.package('OrchardCore.cli', folder='another-native-runner')
        publisher.prepare(self.source, self.output, VERSION)
        ordered = (self.output / 'publish-order.txt').read_text().splitlines()
        self.assertEqual(len(publisher.REQUIRED), len(ordered))
        self.assertEqual('OrchardCore.cli.' + VERSION + '.nupkg', Path(ordered[-1]).name)

    def test_missing_platform_stops_before_staging(self):
        next(self.source.glob('*cli.win-arm64.*')).unlink()
        with self.assertRaisesRegex(ValueError, 'Missing required packages'):
            publisher.prepare(self.source, self.output, VERSION)
        self.assertFalse(self.output.exists())

    def test_foreign_package_stops_before_staging(self):
        self.package('Unexpected.Package')
        with self.assertRaisesRegex(ValueError, 'Unexpected package ID'):
            publisher.prepare(self.source, self.output, VERSION)
        self.assertFalse(self.output.exists())

    def test_original_theme_ids_and_dependencies_are_preserved(self):
        self.package('OrchardCore.Extra', dependency='TheBlogTheme', dependency_version=VERSION)
        publisher.prepare(self.source, self.output, VERSION)
        self.assertTrue((self.output / f'TheBlogTheme.{VERSION}.nupkg').exists())
        packages = json.loads((self.output / 'packages.json').read_text())['packages']
        self.assertIn('theblogtheme', packages)
        self.assertNotIn('orchardcore.themes.theblogtheme', packages)

    def test_renamed_theme_cannot_replace_original_package(self):
        next(self.source.glob('TheBlogTheme.*')).unlink()
        self.package('OrchardCore.Themes.TheBlogTheme')
        with self.assertRaisesRegex(ValueError, 'Missing required packages: theblogtheme'):
            publisher.prepare(self.source, self.output, VERSION)
        self.assertFalse(self.output.exists())

    def test_inconsistent_theme_dependency_is_rejected(self):
        self.package('OrchardCore.Extra', dependency='TheBlogTheme')
        with self.assertRaisesRegex(ValueError, 'inconsistent version'):
            publisher.prepare(self.source, self.output, VERSION)

    def test_mixed_versions_are_rejected(self):
        self.package('OrchardCore.Extra', version='4.0.0-cli.122')
        with self.assertRaisesRegex(ValueError, 'expected version'):
            publisher.prepare(self.source, self.output, VERSION)

    def test_stale_template_version_is_rejected(self):
        self.package('OrchardCore.projecttemplates', template_version='4.0.0-preview')
        with self.assertRaisesRegex(ValueError, 'template uses a different Orchard version'):
            publisher.prepare(self.source, self.output, VERSION)

    def test_separately_published_translation_dependency_is_allowed_at_pinned_version(self):
        self.package('OrchardCore.Extra', dependency='OrchardCore.Translations.All', dependency_version='3.0.0')
        publisher.prepare(self.source, self.output, VERSION)

    def test_unexpected_translation_version_is_rejected(self):
        self.package('OrchardCore.Extra', dependency='OrchardCore.Translations.All')
        with self.assertRaisesRegex(ValueError, 'inconsistent version'):
            publisher.prepare(self.source, self.output, VERSION)

    def test_inconsistent_build_dependency_is_rejected(self):
        self.package('OrchardCore.Extra', dependency='OrchardCore.Module.Targets')
        with self.assertRaisesRegex(ValueError, 'inconsistent version'):
            publisher.prepare(self.source, self.output, VERSION)


if __name__ == '__main__':
    unittest.main()
