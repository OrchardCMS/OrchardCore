# Home Route (`OrchardCore.HomeRoute`)

The `OrchardCore.HomeRoute` module defines the route that the homepage (the URL `/`) maps to. It registers a dynamic route on `/` that is resolved at request time from the configured home route, so any controller action or content item can act as the site's homepage.

## Setting the homepage

The home route is stored in the site settings (`HomeRoute`). It is most often set indirectly:

- **Promote a content item to the homepage.** Content items expose a *Set as homepage* option (through the `Contents` feature), which points the home route at that item.
- **Point to a controller action.** Modules and themes can set the home route to their own route values.

Remote clients should not submit the transient
`AutoroutePart.SetHomepage` property. Use the permission-gated command instead:

```bash
pomi settings set-home-content <published-content-item-id>
```

See the [Home Route management API](../../api/home-route/README.md).

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

## Updating the route from code

Inject `IHomeRouteService` from `OrchardCore.Settings` and call
`UpdateAsync(Action<RouteValueDictionary>)`. The callback receives a copy of the
current route. Clear it to replace the route, or modify selected values to retain
the others. The service saves through `ISiteService` and refreshes cached settings
only when the complete route changes; its return value indicates whether it saved.

The Home Route API and the Autoroute publication handler use this service. The
API replaces the route with its fixed content-display values. Autoroute retains
its configured global route values and custom content and contained-path keys.
Callers remain responsible for authorization and selecting valid content.
