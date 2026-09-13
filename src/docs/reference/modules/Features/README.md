# Features (`OrchardCore.Features`)

The `OrchardCore.Features` module lets the administrator of a site manage the installed modules and enable or disable the features they provide.

## API reference

See the [Feature management API](../../api/features/README.md) for OpenAPI operations that list, enable, and disable features.

## Managing features

Go to **Configuration** > **Features** in the admin (requires the `Manage features` permission). From there you can:

- Enable a feature, which also enables any feature it depends on.
- Disable a feature, which also disables features that depend on it.
- See each feature's category, description and dependencies.

Some features are marked as *always enabled* by their module and cannot be turned off.

## Permissions

| Permission         | Description                          |
|--------------------|--------------------------------------|
| `Manage features`  | Allows enabling and disabling features. |

Granted to the `Administrator` role by default.

## Enabling features with a recipe

Features can be enabled or disabled from a recipe using the `Feature` step. List the feature ids to turn on or off:

```json
{
  "steps": [
    {
      "name": "Feature",
      "enable": [
        "OrchardCore.Contents",
        "OrchardCore.Lists"
      ],
      "disable": [
        "OrchardCore.HomeRoute"
      ]
    }
  ]
}
```

This is the recommended way to compose a site's enabled feature set as part of a setup or deployment recipe.

## Deployment step configuration through Pomi

The `AllFeaturesDeploymentStep` factory exposes the Boolean `ignoreDisabledFeatures`
option through the live deployment step schema.
Use `pomi deployment step-types schema <factory>` to inspect the contract and
`pomi deployment plans steps add <planId> --body-file step.json` to add a step with
`id`, `type` and `values`. Updates contain only `values`; omitted options retain
their existing setting. Unknown properties, nulls and non-Boolean values are
rejected before the persisted step changes. This requires the Deployment feature
and its plan-management permission.
