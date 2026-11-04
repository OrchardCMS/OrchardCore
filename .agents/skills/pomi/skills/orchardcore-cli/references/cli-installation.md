# Install or update Pomi

Apply the [shared operating rules](shared-rules.md). The NuGet package ID is
`OrchardCore.Cli`; the installed command is `pomi`. Installing the agent plugin
does not install the executable.

Use the same commands for both a first installation and an update:

```bash
dotnet tool install --global OrchardCore.Cli --prerelease
pomi --version
```

The installer selects the latest available version, including prereleases,
from the user's registered NuGet sources. Required feeds must already be
configured there. No separate package search, version selection, or update
command is needed. Report the version returned by `pomi --version`.

Installation requires the .NET 10 SDK or later. Running the installed native
executable needs no separate .NET runtime. If `pomi` is not found after
installation, add the tool directory reported by the installer to `PATH`
(normally `$HOME/.dotnet/tools` on macOS/Linux).

Continue with [authentication and contexts](authentication.md), or
[local CMS installation](installation.md) to create a new site. The CLI embeds
its matching site template; a newer CLI does not update an existing server.
