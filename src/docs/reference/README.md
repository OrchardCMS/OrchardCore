# Reference of Built-in Modules

This is a comprehensive reference for the modules and their features available in Orchard Core. Under each of these pages, you'll find an overview of the given module with documentation on how to use and extend them.

Check out the reference pages for each module under the following categories in the menu:

- CMS Modules: Modules that contain content management features that all interact with content items in some way. E.g. the base content management module, the media management module, or modules related to search and indexing are here.
- Core Modules: Modules that usually provide some fundamental features for a web app which is not connected to content management, or framework-like functionality that you can build on. These modules include e.g. user and role management, or the UI localization and background task management infrastructure.

The above distinction is also present in the Orchard Core source code with solution folders. However, this is just a logical categorization and technically all the modules are alike.

## Overview of all Orchard Core features

Here's a categorized overview of all built-in Orchard Core features at a glance. Click on the links to open the corresponding documentation pages.

### Admin

- [Admin](modules/Admin/README.md)
- [Admin Dashboard](modules/AdminDashboard/README.md)
- [Admin Menu](modules/AdminMenu/README.md)
- [Custom Settings](modules/CustomSettings/README.md)

### App security

- [Audit Trail](modules/AuditTrail/README.md)
- [CORS](modules/Cors/README.md)
- [ReCaptcha](modules/ReCaptcha/README.md)
- [Sanitizer](core/Sanitizer/README.md)
- [Security](modules/Security/README.md) 

### Authentication and User Management

- Authentication:
  - [Microsoft](modules/Microsoft.Authentication/README.md)
  - [Facebook](modules/Facebook/README.md)
  - [Twitter](modules/Twitter/README.md)
  - [GitHub](modules/GitHub/README.md)
  - [Google](modules/Google/README.md)
- Users: 
  - [Overview](modules/Users/README.md)
  - [Custom User Settings](modules/Users/CustomUserSettings/README.md)
- [OpenId](modules/OpenId/README.md)
- [Roles](modules/Roles/README.md)

### Content

- [Contents](modules/Contents/README.md)
- [Content Types](modules/ContentTypes/README.md)
- Content Parts: 
  - [Add Parts to your Content](modules/ContentParts/README.md)
  - [Title](modules/Title/README.md)
  - [Autoroute](modules/Autoroute/README.md)
  - [Alias](modules/Alias/README.md)
  - [Html](modules/Html/README.md)
  - [Markdown](modules/Markdown/README.md)
  - [List](modules/Lists/README.md)
  - [Flow](modules/Flow/README.md) 
  - [Bag](modules/Flow/BagPart.md)
  - [Publish Later](modules/PublishLater/README.md)
  - [SEO Meta](modules/Seo/README.md)
- [Content Fields](modules/ContentFields/README.md)
- [Content Preview](modules/ContentPreview/README.md)
- [Taxonomies](modules/Taxonomies/README.md)
- [Feeds](modules/Feeds/README.md)
- [Forms](modules/Forms/README.md)
- Media:
  - [Media](modules/Media/README.md)
  - [Media Slugify](modules/Media.Slugify/README.md)
  - [Media Amazon S3](modules/Media.AmazonS3/README.md)
  - [Media Azure](modules/Media.Azure/README.md)
- [XML-RPC](modules/XmlRpc/README.md)
- [Shortcodes](modules/Shortcodes/README.md)

### Design

- [Layers](modules/Layers/README.md)
- [Widgets](modules/Widgets/README.md)
- [Templates](modules/Templates/README.md)
- [Placements](modules/Placements/README.md)
- [Themes](modules/Themes/README.md)
- [Liquid](modules/Liquid/README.md)
- [Resources](modules/Resources/README.md)
- [Rules](modules/Rules/README.md)
- [Placement](core/Placement/README.md)

### Extensibility

- [Auto Setup](modules/AutoSetup/README.md)
- [GraphQL](modules/Apis.GraphQL/README.md)
- [GraphQL queries](core/Apis.GraphQL.Abstractions/README.md)
- [Scripting](modules/Scripting/README.md)
- [Modules](core/Modules/README.md)
- [Features](modules/Features/README.md)
- [Data](core/Data/README.md)
- [Dynamic Cache](modules/DynamicCache/README.md)
- [Razor Helpers](core/Razor/README.md)
- [Recipes](modules/Recipes/README.md)
- [Setup](modules/Setup/README.md)
- [Shells](core/Shells/README.md)
- [Workflows](modules/Workflows/README.md)
- [Background Tasks](modules/BackgroundTasks/README.md)

### Hosting and Operations

- [Configuration](core/Configuration/README.md)
- [Key Vault (Azure)](core/KeyVault.Azure/README.md)
- [DataProtection (Azure Storage)](modules/DataProtection.Azure/README.md)
- [Reverse Proxy](modules/ReverseProxy/README.md)
- [Tenants](modules/Tenants/README.md)
- [Health Check](modules/HealthChecks/README.md)
- [HTTPS](modules/Https/README.md)
- [Logging Serilog](core/Logging.Serilog/README.md)
- [Mini Profiler](modules/MiniProfiler/README.md)
- [Response Compression](modules/ResponseCompression/README.md)
- [Email](modules/Email/README.md)
- [Redis](modules/Redis/README.md)
- [Deployment](modules/Deployment/README.md)
- [Remote Deployment](modules/Deployment.Remote/README.md)

### Localization

- [Content Localization](modules/ContentLocalization/README.md)
- [Localization infrastructure](modules/Localize/README.md)

### Navigation

- [Menu](modules/Menu/README.md)
- [Navigation](modules/Navigation/README.md)
- [Home Route](modules/HomeRoute/README.md)
- [Sitemaps](modules/Sitemaps/README.md)

### Search, Indexing, Querying

- [SQL](modules/SQLIndexing/README.md)
- [Lucene](modules/Lucene/README.md)
- [Elasticsearch](modules/Elasticsearch/README.md)
- [Queries](modules/Queries/README.md)
- [Indexing](modules/Indexing/README.md)