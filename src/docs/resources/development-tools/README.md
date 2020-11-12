# Development Tools

What tools do we recommend to build your app with Orchard Core, or work on Orchard itself? In the end, this is up to your personal preference since as long as you can edit source files and build the app you can use any tool on any platform that .NET Core supports. Below are some tips to get you going for the general editing experience as well as for other useful tools.

## Editors and IDEs

- Visual Studio: The go-to IDE for .NET developers on Windows. Feature-rich and also has a free version. Download the latest Visual Studio (any edition) from <https://www.visualstudio.com/downloads/>.
  - Optionally install the [Lombiq Orchard Visual Studio Extension](https://marketplace.visualstudio.com/items?itemName=LombiqVisualStudioExtension.LombiqOrchardVisualStudioExtension) to add some useful utilities to your Visual Studio such as an error log watcher or a dependency injector.
  - Optionally install the [code snippets from the Orchard Dojo Library](https://orcharddojo.net/orchard-resources/CoreLibrary/Utilities/VisualStudioSnippets/) to quickly generate code in some common scenarios during module and theme development.
  - There are some further recommended extensions and other tips on using Visual Studio in the [Orchard Dojo Library](https://orcharddojo.net/orchard-resources/CoreLibrary/DevelopmentGuidelines/DevelopmentEnvironment).
- Visual Studio Code: Free and cross-platform editor that you can get from <https://code.visualstudio.com/>.
- JetBrains Rider: Feature-rich cross-platform IDE with a 30-day free trial that you can get from <https://www.jetbrains.com/rider/download/>.

## Utilities

- [DB Browser for SQLite](https://sqlitebrowser.org/) is a free and open-source tool to browse the SQLite database files created by Orchard. You can use it to open the `yessql.db` files under the tenant folders in `App_Data/Sites` and e.g. browse the tables, run queries, display the JSON documents in a nicely formatted way.
- [smtp4dev](https://github.com/rnwood/smtp4dev) is a small SMTP server that you can run locally to test sending emails. Just install it via `dotnet` and configure Orchard to use it as an SMTP server. It even features a web interface where you can browse processed emails.