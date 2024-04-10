# Contributing to the Orchard Core documentation

First of all, thank you for thinking about contributing to the docs! This is especially valuable while you're still new to Orchard because your experiences and revelations can help other newcomers a lot. Be sure to check out [the general contribution docs](README.md) first.

The [Orchard Core documentation site](https://docs.orchardcore.net/) is built with [MkDocs](https://www.mkdocs.org/) and served from [Read the docs](https://readthedocs.org/projects/orchardcore/).

!!! info
    Are you looking for information on contributing code? Head over to [here](contributing-code.md) instead.

## Editing documentation pages

First, clone the repository using the command `git clone https://github.com/OrchardCMS/OrchardCore.git` and checkout the `main` branch. You can find the documentation files under the `src/docs` folder.

If you open the Orchard Core solution (`OrchardCore.sln` in the root) in Visual Studio or another IDE then you'll be able to browse the files in the `OrchardCore.Docs` project under the `docs` solution folder. If you use a Markdown editor (IDEs usually have built-in support for it, including Visual Studio) then you'll see all the Markdown formatting and embedded images in a WYSIWYG manner, and links will work too.

- To embed YouTube videos, be sure to tick "Enable privacy-enhanced mode." when generating the embed code (this will create a code that references youtube-nocookie.com).
- If you rename or move a documentation page, external links to it will break. Please add a redirect from the old URL in the `mkdocs.yml` file, under the `redirect_maps` section.

## Adding docs for a new extension or topic

Do the following if you're adding docs for a newly developed extension, or if you add some other completely new topic (in which case adapt it to the section you add the topic to):

1. Add a folder under `reference/modules` with the same name as the module's project without `OrchardCore.`. E.g. for `OrchardCore.AuditTrail` use `AuditTrail`.
2. Add a `README.md` file to it, following the format of existing such files. There, add an overview of the extension's features, configuration, embed demo videos.
3. Link to the `README.md` file from `reference/modules/README.md`.
4. Link to the `README.md` file from the `mkdocs.yml` file in the repository root.
5. If the module contains content parts, also link the `README.md` file from `reference/modules/ContentParts/README.md`.

## Running the documentation site locally

With MkDocs you can get the full docs.orchardcore.net experience locally too. If you are looking to contribute substantial amount of docs then please do run the site locally to make sure what you write will actually look like it should.

1. Follow the [MkDocs installation guide](https://www.mkdocs.org/#installation) to install Python. once you have Python installed you won't need to install MkDocs by hand, we'll do that in a next step. If you're on Windows be sure to add the Python `Scripts` folder to the `PATH` as noted there, otherwise none of the `mkdocs` commands will be found. You may need to add your user's Scripts folder (something like `C:\Users\<user name>\AppData\Roaming\Python\Python39\Scripts`) to the `PATH` too.
2. Open a command line in the root of your clone of the repository.
3. Run `pip3 install -r src/docs/requirements.txt` to install dependencies.
4. Run `mkdocs serve` to start the site. You'll then be able to browse it under <http://127.0.0.1:8000>. If you use Visual Studio under Windows, we recommend NOT running the command from a Windows PowerShell prompt opened from Visual Studio's Solution Explorer. Instead, open CMD or a PowerShell 7+ window; for some reason, the MkDocs build is really slow under Windows PowerShell.

## Submitting a pull request and gathering feedback

This happens in the same way as for code contributions, [see there](contributing-code.md).