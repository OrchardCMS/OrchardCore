# Built-in Recipe Steps Reference

Each step is a JSON object in the `steps` array with a `name` and step-specific fields. Steps run in order.

## `feature`

Enable or disable features (modules/themes).

```json
{
  "name": "feature",
  "enable": [ "OrchardCore.Contents", "YourTheme" ],
  "disable": []
}
```

Enable your own theme's feature here, or its layout won't apply.

## `themes`

Set active site and admin themes.

```json
{
  "name": "themes",
  "admin": "TheAdmin",
  "site": "YourTheme"
}
```

## `settings`

Site-wide settings (home route, culture, time zone, and feature settings).

```json
{
  "name": "settings",
  "HomeRoute": {
    "Action": "Display",
    "Controller": "Item",
    "Area": "OrchardCore.Contents",
    "ContentItemId": "[js:variables('homeContentItemId')]"
  },
  "LayerSettings": { "Zones": [ "Content", "Footer" ] }
}
```

## `Roles`

Create roles and assign permissions.

```json
{
  "name": "Roles",
  "Roles": [
    { "Name": "Editor", "Description": "Edits content.", "Permissions": [ "PublishContent", "EditContent" ] }
  ]
}
```

## `ContentDefinition`

Define or update content types and parts.

```json
{
  "name": "ContentDefinition",
  "ContentTypes": [
    {
      "Name": "Article",
      "DisplayName": "Article",
      "Settings": { "ContentTypeSettings": { "Creatable": true, "Listable": true } },
      "ContentTypePartDefinitionRecords": [
        { "PartName": "TitlePart", "Name": "TitlePart" },
        { "PartName": "Article", "Name": "Article" }
      ]
    }
  ],
  "ContentParts": [
    { "Name": "Article", "Settings": { "ContentPartSettings": { "Attachable": true } } }
  ]
}
```

## `content`

Import content items (pages, posts, menus, widgets).

```json
{
  "name": "content",
  "Data": [
    {
      "ContentItemId": "[js:variables('homeContentItemId')]",
      "ContentType": "Page",
      "DisplayText": "Home",
      "Latest": true,
      "Published": true,
      "TitlePart": { "Title": "Home" },
      "AutoroutePart": { "Path": "home", "SetHomepage": true }
    }
  ]
}
```

The easiest way to author `content` data: build it in the admin, then export via Deployment and copy the JSON.

## `media`

Upload files into the Media library.

```json
{
  "name": "media",
  "Files": [
    { "TargetPath": "logo.jpg", "SourcePath": "../wwwroot/img/logo.jpg" }
  ]
}
```

## `layers`

Define layers for conditional widget placement.

```json
{
  "name": "layers",
  "Layers": [
    { "Name": "Always", "Rule": "true", "Description": "Shown on every page." },
    { "Name": "Homepage", "Rule": "isHomepage()", "Description": "Homepage only." }
  ]
}
```

## `queries`

Register reusable Lucene/SQL/Elasticsearch queries.

```json
{
  "name": "queries",
  "Queries": [
    {
      "Source": "Lucene",
      "Name": "RecentPosts",
      "Index": "Search",
      "Template": "[file:text('Snippets/recentPosts.json')]",
      "ReturnContentItems": true
    }
  ]
}
```

## `Templates`

Define Liquid templates (shape overrides).

```json
{
  "name": "Templates",
  "Templates": {
    "Content__LandingPage": {
      "Description": "Landing page layout",
      "Content": "[file:text('Snippets/landingpage.liquid')]"
    }
  }
}
```

## `AdminMenu`

Add custom admin menu trees.

```json
{
  "name": "AdminMenu",
  "data": [
    { "Id": "[js:uuid()]", "Name": "Tools", "Enabled": true, "MenuItems": [] }
  ]
}
```

## `WorkflowType`

Define workflows automating content/user events.

```json
{
  "name": "WorkflowType",
  "data": [
    { "WorkflowTypeId": "[js:variables('workflowTypeId')]", "Name": "User Registration", "IsEnabled": true }
  ]
}
```

## `custom-settings`

Set values for a content-item-backed custom settings type.

```json
{
  "name": "custom-settings",
  "MySiteSettings": {
    "ContentType": "MySiteSettings",
    "MySettingsPart": { "SomeTextField": { "Text": "Hello World" } }
  }
}
```

## `deployment`

Define deployment plans (export/import content and config).

```json
{
  "name": "deployment",
  "Plans": [
    {
      "Name": "ExportSite",
      "Steps": [
        { "Type": "AllContentDeploymentStep", "Step": { "Id": "[js:uuid()]", "Name": "AllContent" } }
      ]
    }
  ]
}
```

## `recipes`

Run other recipes from this one (modular composition).

```json
{
  "name": "recipes",
  "Values": [
    { "executionid": "YourSite", "name": "YourSite.Content" }
  ]
}
```

`name` matches the target recipe's `name` header property. `executionid` is any identifier distinguishing this execution.

## Search index steps

Each search provider has create/reset/rebuild steps. Lucene example (Elasticsearch and Azure AI Search mirror this with `elasticsearch-*` / `azureai-*` names):

```json
{ "name": "lucene-index", "Indices": [ { "Search": { "AnalyzerName": "standardanalyzer", "IndexLatest": false, "IndexedContentTypes": [ "Article" ] } } ] }
```

```json
{ "name": "lucene-index-reset", "includeAll": true }
```

```json
{ "name": "lucene-index-rebuild", "Indices": [ "Search" ] }
```

Create indices **before** the `content` step so imported items are indexed automatically.
