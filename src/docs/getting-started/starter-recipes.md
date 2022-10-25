# Starter Recipes and Themes included with Orchard Core

Orchard Core is available for use via two different NuGet meta packages.

- `OrchardCore.Application.Cms.Core.Targets`
- `OrchardCore.Application.Cms.Targets`

The first package `OrchardCore.Application.Cms.Core.Targets` is intended for use when 

- Developing a Decoupled Web Site
- Developing a Headless Web Site
- Developing a Themed Web Site from scratch

The `Core.Targets` package contains the minimum you need to setup an Orchard Core installation.
It contains `TheAdmin` theme, and two recipes to base your installation on, but no front end themes.

!!! tip
    Any features that are not enabled by the selected setup recipe can be enabled after setup,
    through the _Configuration -> Features_ menu.

The second package `OrchardCore.Application.Cms.Targets` contains all of the above plus

- Setup recipes for the Themes
- Multiple CMS Starter Themes

Recipes in Orchard Core help you get your site setup by enabling features,
and / or creating content types, and content for your site.

Orchard Core Themes can contain Razor or Liquid views, and by default use 
Orchard Core Display Management techniques to render content.

## OrchardCore.Application.Cms.Core.Targets

### Blank Recipe

The Blank recipe enables content management features, but does not set a current theme.
You can use this recipe when starting Orchard Core in Decoupled Mode,
or when building your own theme.

Alternatively you can start with another recipe,
and change the active theme after setup.

#### Blank Recipe Contents 

- Content management features
- Activates `TheAdmin` theme

### Headless Recipe

The Headless recipe is intended to get you started when using Orchard Core
as an API, and Content Management System, with Administrator access to the host.

#### Headless Recipe Contents

- Content management features
- Secure GraphQL API support
- OpenID authentication features
- Activates `TheAdmin` theme and set Admin as the home route

!!! tip
    You will want to review the default security configuration to be certain
    it suits your requirements.

## Headless Recipe Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/dbABI1wECPg" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## OrchardCore.Application.Cms.Targets

### TheBlogTheme and Blog Recipe

The Blog recipe sets up a range of content types, and widgets, the initial content,
and sets the current theme to the TheBlogTheme.

TheBlogTheme is based on the [Start Bootstrap Clean Blog Theme](https://startbootstrap.com/themes/clean-blog/)

#### Blog Recipe Contents

- Content management features
- Blog related Content Types, and Widgets
- A blog, and a first blog post, based on the `ListPart`
- Liquid templates, in the TheBlogTheme source code
- Bootstrap

### Blog - Lucene Query Recipe

The Lucene Query recipe is an optional recipe in the TheBlogTheme. 
This recipe runs the Blog Lucene Search recipe and as an example,
the recipe replaces the RecentBlogPosts SQL query with a Lucene query.

#### Blog Lucene Query Recipe Contents

- Runs the Blog Lucene Search recipe
- Replace RecentBlogPosts SQL query with Lucene

### Blog - Lucene Search Recipe

The Lucene Search recipe is an optional recipe in the TheBlogTheme. 
This recipe enables the Lucene feature and creates Search setting, Lucene indices and permissions.

#### Blog Lucene Search Recipe Contents

- Enables Lucene feature
- Setup Lucene indices 
- Create the search settings
- Search index permission

### TheAgencyTheme and Agency Recipe

The Agency recipe sets up a range of content types, and widgets, the initial content,
and sets the current theme to TheAgencyTheme.

TheAgencyTheme is based on the [Start Bootstrap Agency Theme](https://startbootstrap.com/themes/agency/)

#### Agency Recipe Contents

- Content management features
- Agency related Content types, and widgets
- A LandingPage, based on the `BagPart`
- Liquid templates, in TheAgencyTheme source code, and Templates feature
- Bootstrap

### ComingSoon Recipe and TheComingSoonTheme

This recipe sets up a range of Content Types, and Widgets, and the initial content of TheComingSoonTheme.
It also includes Email, Recaptcha, Forms, Workflows and User Registration Forms.

TheComingSoon theme is based on the [Start Bootstrap Coming Soon Theme](https://startbootstrap.com/themes/coming-soon/)

#### ComingSoon Recipe Contents

- Content management features
- A Coming Soon landing page, using the the `FlowPart`, and form `Widgets`
- Liquid layout template, in TheComingSoon Source Code
- Liquid content templates stored in the database with the Templates features
- Bootstrap

### SaaS Recipe with TheTheme

The Saas recipe includes a Software as a Service multi tenancy configuration.

It configures the site to use TheTheme, and you are then able to create Tenants 
using any of the other recipes.

#### Saas Recipe Contents

- Multi-tenancy feature
- Razor home page and Layout with bootstrap and jQuery

## Creating your own recipe

You can create your own recipes for deployment of your Orchard Core websites.

See the [Recipes](../reference/modules/Recipes/README.md) document for more information.
