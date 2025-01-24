# Orchard Core Permissions

Orchard Core provides a comprehensive set of permissions to manage various aspects of the system. Below is a list of permissions along with their descriptions. Note that security critical permissions allow users to elevate their permissions and should be assigned with caution.


| Permission Group | Permission | Description |
|------------------|------------|-------------|
| Admin (OrchardCore.Admin) | AccessAdminPanel | Access admin panel |
| | ManageAdminSettings | Manage Admin Settings |
| Admin Menu (OrchardCore.AdminMenu) | ManageAdminMenu | Manage the admin menu |
| | ViewAdminMenu_[MenuName] | View Admin Menu - [MenuName] |
| | ViewAdminMenuAll | View Admin Menu - View All |
| Amazon Media Storage (OrchardCore.Media.AmazonS3) | ViewAmazonS3MediaOptions | View Amazon S3 Media Options |
| Audit Trail (OrchardCore.AuditTrail) | ManageAuditTrailSettings | Manage audit trail settings |
| | ViewAuditTrail | View audit trail |
| Autoroute (OrchardCore.Autoroute) | SetHomepage | Set homepage |
| Azure Media ImageSharp Image Cache (OrchardCore.Media.Azure.ImageSharpImageCache) | ViewAzureMediaOptions | View Azure Media Options |
| Background Tasks (OrchardCore.BackgroundTasks) | ManageBackgroundTasks | Manage background tasks |
| Permissions for each Content Type | Clone_[ContentType] | Clone [ContentType] by others |
| | CloneOwn_[ContentType] | Clone own [ContentType] |
| | DeleteOwn_[ContentType] | Delete [ContentType] |
| | Delete_[ContentType] | Delete [ContentType] for others |
| | EditOwn_[ContentType] | Edit [ContentType] |
| | Edit_[ContentType] | Edit [ContentType] for others |
| | EditContentOwner_[ContentType] | Edit the owner of a [ContentType] content item |
| | ListContent_[ContentType] | List [ContentType] content items |
| | Preview_[ContentType] | Preview [ContentType] by others |
| | PreviewOwn_[ContentType] | Preview own [ContentType] |
| | PublishOwn_[ContentType] | Publish or unpublish [ContentType] |
| | Publish_[ContentType] | Publish or unpublish [ContentType] for others |
| | View_[ContentType] | View [ContentType] by others |
| | ViewOwn_[ContentType] | View own [ContentType] |
| Content Localization (OrchardCore.ContentLocalization) | LocalizeContent | Localize content for others |
| | LocalizeOwnContent | Localize own content |
| | ManageContentCulturePicker | Manage ContentCulturePicker settings |
| Content Types (OrchardCore.ContentTypes) | EditContentTypes (security critical) | Edit content types |
| | ViewContentTypes | View content types |
| Contents (OrchardCore.Contents) | AccessContentApi | Access content via the api |
| | ApiViewContent | Access view content endpoints |
| | CloneContent | Clone content |
| | CloneOwnContent | Clone own content |
| | DeleteContent | Delete content for others |
| | DeleteOwnContent | Delete own content |
| | EditContent | Edit content for others |
| | EditOwnContent | Edit own content |
| | EditContentOwner | Edit the owner of a content item |
| | ListContent | List content items |
| | PreviewContent | Preview content |
| | PreviewOwnContent | Preview own content |
| | PublishContent | Publish or unpublish content for others |
| | PublishOwnContent | Publish or unpublish own content |
| | ViewContent | View all content |
| | ViewOwnContent | View own content |
| Custom Settings (OrchardCore.CustomSettings) | ManageCustomSettings_[CustomSettingsType] | Manage Custom Settings |
| Deployment (OrchardCore.Deployment) | Export | Export Data |
| | Import (security critical) | Import Data |
| | ManageDeploymentPlan | Manage deployment plans |
| Elasticsearch (OrchardCore.Search.Elasticsearch) | ManageElasticIndexes | Manage Elasticsearch Indexes |
| | QueryElasticsearchApi | Query Elasticsearch Api |
| Email (OrchardCore.Email) | ManageEmailSettings | Manage Email Settings |
| Features (OrchardCore.Features) | ManageFeatures | Manage Features |
| GitHub Authentication (OrchardCore.GitHub.Authentication) | ManageGitHubAuthentication | Manage GitHub Authentication settings |
| Google Tag Manager (OrchardCore.Google.TagManager) | ManageGoogleAnalytics | Manage Google Analytics settings |
| | ManageGoogleAuthentication | Manage Google Authentication settings |
| | ManageGoogleTagManager | Manage Google Tag Manager settings |
| GraphQL (OrchardCore.Apis.GraphQL) | ExecuteGraphQLMutations | Execute GraphQL Mutations |
| | ExecuteGraphQL | Execute GraphQL |
| HTTPS (OrchardCore.Https) | ManageHttps | Manage HTTPS |
| Layers (OrchardCore.Layers) | ManageLayers | Manage layers |
| Localization (OrchardCore.Localization) | ManageCultures | Manage supported culture |
| Media (OrchardCore.Media) | ManageMediaFolder | Manage All Media Folders |
| | ManageAttachedMediaFieldsFolder | Manage Attached Media Fields Folder |
| | ManageMediaContent | Manage Media |
| | ManageOthersMediaContent | Manage Media For Others |
| | ManageMediaProfiles | Manage Media Profiles |
| | ManageOwnMediaContent | Manage Own Media |
| | ViewMediaOptions | View Media Options |
| Media Cache (OrchardCore.Media.Cache) | ManageAssetCache | Manage Asset Cache Folder |
| Menu (OrchardCore.Menu) | ManageMenu | Manage menus |
| Meta Core Components (OrchardCore.Facebook) | ManageFacebookApp | View and edit the Facebook app |
| Meta Pixel (OrchardCore.Facebook.Pixel) | ManageFacebookPixel | Manage Facebook Pixel settings |
| Microsoft Entra ID (Azure Active Directory) Authentication (OrchardCore.Microsoft.Authentication.AzureAD) | ManageMicrosoftAuthentication | Manage Microsoft Authentication settings |
| Notifications (OrchardCore.Notifications) | ManageNotifications | Manage notifications |
| OpenID Connect Core Services (OrchardCore.OpenId) | ManageClientSettings | View and edit the OpenID Connect client settings |
| | ManageServerSettings | View and edit the OpenID Connect server settings |
| | ManageValidationSettings | View and edit the OpenID Connect validation settings |
| | ManageApplications | View, add, edit and remove the OpenID Connect applications |
| | ManageScopes | View, add, edit and remove the OpenID Connect scopes |
| Placements (OrchardCore.Placements) | ManagePlacements | Manage placements |
| Queries (OrchardCore.Queries) | ExecuteApiAll | Execute Api - All queries |
| | ExecuteApi_RecentBlogPosts | Execute Api - RecentBlogPosts |
| | ManageQueries | Manage queries |
| ReCaptcha (OrchardCore.ReCaptcha) | ManageReCaptchaSettings | Manage ReCaptcha Settings |
| Recipes (OrchardCore.Recipes) | ManageRecipes (security critical) | Manage Recipes |
| Remote Deployment (OrchardCore.Deployment.Remote) | ExportRemoteInstances | Export to remote instances |
| | ManageRemoteClients | Manage remote clients |
| | ManageRemoteInstances | Manage remote instances |
| Reverse Proxy Configuration (OrchardCore.ReverseProxy) | ManageReverseProxySettings | Manage Reverse Proxy Settings |
| Roles (OrchardCore.Roles) | ManageRoles (security critical) | Managing Roles |
| Search (OrchardCore.Search) | ManageSearchSettings | Manage Search Settings |
| | QuerySearchIndex | Query any index |
| Secure Media (OrchardCore.Media.Security) | ViewMediaContent | View media content in all folders |
| | ViewRootMediaContent | View media content in the root folder |
| | ViewOthersMediaContent | View others media content |
| | ViewOwnMediaContent | View own media content |
| | ViewMediaContent_[FolderName] | View media content in folder '[FolderName]' |
| Security (OrchardCore.Security) | ManageSecurityHeadersSettings | Manage Security Headers Settings |
| SEO (OrchardCore.Seo) | ManageSeoSettings | Manage SEO related settings |
| Settings (OrchardCore.Settings) | ManageSettings | Manage settings |
| Shortcode Templates (OrchardCore.Shortcodes.Templates) | ManageShortcodeTemplates (security critical) | Manage shortcode templates |
| Sitemaps (OrchardCore.Sitemaps) | ManageSitemaps | Manage sitemaps |
| SMS (OrchardCore.Sms) | ManageSmsSettings | Manage SMS Settings |
| SQL Queries (OrchardCore.Queries.Sql) | ManageSqlQueries | Manage SQL Queries |
| Taxonomies (OrchardCore.Taxonomies) | ManageTaxonomy | Manage taxonomies |
| Templates (OrchardCore.Templates) | ManageAdminTemplates (security critical) | Manage admin templates |
| | ManageTemplates (security critical) | Manage templates |
| Themes (OrchardCore.Themes) | ApplyTheme | Apply a Theme |
| Two-Factor Authentication Services (OrchardCore.Users.2FA) | DisableTwoFactorAuthenticationForUsers (security critical) | Disable two-factor authentication for any user |
| URL Rewriting (OrchardCore.UrlRewriting) | ManageUrlRewritingRules | Manage URLs rewriting rules |
| Users (OrchardCore.Users) | AssignRoleToUsers (security critical) | Assign any role to users |
| | AssignRoleToUsers_[RoleName] (security critical) | Assign [RoleName] role to users |
| | DeleteUsers (security critical) | Delete any user |
| | DeleteUsersInRole_[RoleName] (security critical) | Delete users in [RoleName] role |
| | EditUsers (security critical) | Edit any user |
| | ManageOwnUserInformation | Edit own user information |
| | EditUsersInRole_[RoleName] (security critical) | Edit users in [RoleName] role |
| | ListUsers | List all users |
| | ListUsersInRole_[RoleName] | List users in [RoleName] role |
| | ManageUsers (security critical) | Manage security settings and all users |
| | ManageUsersInRole_[RoleName] (security critical) | Manage users in [RoleName] role |
| | View Users | View user profiles |
| Workflows (OrchardCore.Workflows) | ExecuteWorkflows (security critical) | Execute workflows |
| | ManageWorkflowSettings | Manage workflow settings |
| | ManageWorkflows (security critical) | Manage workflows |
| X (Twitter) Integration (OrchardCore.Twitter) | ManageTwitterSignin | Manage Sign in with X (Twitter) settings |
| | ManageTwitter | Manage X (Twitter) settings |

### Writing New Permissions

When creating new permissions in Orchard Core, it is crucial to ensure that each permission name is unique across the system. This helps maintain clarity and prevents conflicts that could arise from duplicate permission names. 

#### Steps to Create a New Permission:
1. **Define the Permission**: Clearly define the purpose and scope of the new permission.
2. **Check for Uniqueness**: Before finalizing the permission name, check it against the list of existing permissions to ensure it is unique. This can be done by reviewing the current permissions documentation or querying the system.
3. **Implement the Permission**: Once the name is confirmed to be unique, proceed with implementing the permission in the codebase.

**Note**: Always document new permissions thoroughly, including their descriptions and any security implications, especially if they are security critical permissions that allow users to elevate their permissions.

By following these steps, you can help maintain a well-organized and secure permissions system in Orchard Core.
