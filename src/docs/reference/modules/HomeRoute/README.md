# Home Route (`OrchardCore.HomeRoute`)

The `OrchardCore.HomeRoute` module defines the route that the homepage (the URL `/`) maps to. It registers a dynamic route on `/` that is resolved at request time from the configured home route, so any controller action or content item can act as the site's homepage.

## Setting the homepage

The home route is stored in the site settings (`HomeRoute`). It is most often set indirectly:

- **Promote a content item to the homepage.** Content items expose a *Set as homepage* option (through the `Contents` feature), which points the home route at that item.
- **Point to a controller action.** Modules and themes can set the home route to their own route values.

## Setting it from a recipe

The home route is part of the site settings, so it can be set with the `settings` recipe step using the `HomeRoute` key:

```json
{
  "steps": [
    {
      "name": "settings",
      "HomeRoute": {
        "Area": "OrchardCore.Contents",
        "Controller": "Item",
        "Action": "Display",
        "ContentItemId": "4xn8...your-content-item-id"
      }
    }
  ]
}
```

See [Settings](../Settings/README.md) for the site settings infrastructure.
