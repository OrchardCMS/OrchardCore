# Orchard.DisplayManagement

## Placement files

Any extension can provide contain an optional `placement.json` file providing custom placement logic.

### Format

A `placement.json` file contains an object whose properties are shape names. Each of these properties is an array of 
placement rules.

In the following example, we describe the placement for the `TextField` and `Parts_Contents_Publish` shapes.

```csharp
{
  "TextField": [ ... ],
  "Parts_Contents_Publish" : [ ... ]
}
```

A placement rule contains two sets of data:
- Filters
  - Defines what specific shapes are targetted.
- Placement information
  - The placement information to apply when the filter is matched.

Currently you can filter shapes by:
- Their original type, which is the property name of the placement rule, like 'TextField'.
- `display-type` (Optional): The display type, like `Summary` and `Detail` for the most common ones.
- `differentiator` (Optional): The differentiator which is used to distinguish shape types that are reused for multiple elements, like field names.

Placement information consists of:
- `place` (Optional): The actual location of the shape in the rendered zone.
- `alternates` (Optional): An array of alternate shape types to add to the current shape's metadata.
- `wrappers` (Optional): An array of shape types to use as wrappers for the current shape.
- `shape` (Optional): A subtitution shape type.


```json
{
  "TextField": [ 
    {
		"display-type": "Detail",
		"differentiator": "Article.MyTextField",

		"place": "Content",
		"alternates": [ "TextField_Title" ],
		"wrappers": [ "TextField_Title" ],
		"shape": "AnotherShape"
	}
  ],
}
```

### Placing Fields

Fields have a custom differentiator as their shape is used in many places. It is built using the Part it's contained
in, and the name of the field. For instance, if a field named `MyField` would be added to an `Article` content type,
its differentiator would be `Article.MyField`. If a field named `City` was added to an `Address` part then its differentiator would
be `Address.City`.