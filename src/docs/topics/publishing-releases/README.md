# Publishing a new Orchard Core release

These notes are primarily for Orchard's core contributors to guide how to prepare a new release.

## Versioning

We follow [Semantic Versioning 2.0.0](https://semver.org/). Note that this allows only bug fixes in patch versions (e.g. `1.2.x`), new features in minor versions (e.g. `1.x.0`), and breaking changes only in major versions (`x.0.0`).

## Documentation versioning

The documentation site uses [mike](https://github.com/jimporter/mike) together with the [mkdocs-material versioning feature](https://squidfunk.github.io/mkdocs-material/setup/setting-up-versioning/) to provide a version selector dropdown in the header, allowing users to switch between documentation versions.

Documentation versions use the `major.minor` format (e.g. `2.2`, `3.0`). The `main` branch always represents the next upcoming version and is aliased as `latest`.

### How it works

mike deploys each documentation version to a separate subdirectory on the `gh-pages` branch, and generates a `versions.json` file at the root that the mkdocs-material theme reads to render the version selector.

### URL structure

Each version is served under its own path:

| Version | URL |
|---------|-----|
| Latest (3.0) | `https://docs.orchardcore.net/3.0/` |
| 2.2 | `https://docs.orchardcore.net/2.2/` |
| 1.0 | `https://docs.orchardcore.net/1.0/` |

Individual pages follow the pattern `https://docs.orchardcore.net/<version>/<page-path>/`. For example, the Content Types reference for version 2.2 would be available at `https://docs.orchardcore.net/2.2/reference/modules/ContentTypes/`.

When a user visits the root URL without specifying a version, they are redirected to the version aliased as `latest`.

### Publishing a new documentation version

When a new minor or major release is created (e.g. `2.2.0`, `3.0.0`), follow these steps to add a new documentation version:

1. Ensure you have the documentation dependencies installed:

    ```bash
    pip install -r src/docs/requirements.txt
    ```

2. Deploy the new version using mike. Replace `<version>` with the `major.minor` version number (e.g. `2.2`):

    ```bash
    mike deploy <version>
    ```

3. If this is the latest stable release, update the `latest` alias to point to it:

    ```bash
    mike deploy --update-aliases <version> latest
    ```

4. Verify the deployed versions:

    ```bash
    mike list
    ```

5. Test locally to confirm the version selector works:

    ```bash
    mike serve
    ```

    Navigate to `http://localhost:8000` and verify the version selector dropdown appears in the header and all versions are listed.

!!! note
    Patch releases (e.g. `2.2.1`) do not require a new documentation version. The documentation for a patch release should be updated in the existing `major.minor` version.

### Setting the default version

To set which version users are redirected to when visiting the root URL:

```bash
mike set-default latest
```

## Release checklist

[Create a release issue](https://github.com/OrchardCMS/OrchardCore/issues/new/choose) and follow its checklist, ticking everything as you progress.
