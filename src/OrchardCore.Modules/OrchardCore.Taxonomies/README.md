# Taxonomies (OrchardCore.Taxonomies)

This modules provides a Taxonomy content type that is used to define managed vocabularies (categories) of any type.
Taxonomy content items are made of terms organized as a hierarchy. Using the Taxonomy Field allows any content item
to be associated with one or many terms of a taxonomy.

## Shapes

### TaxonomyField

This shape is rendered when a `TaxonomyField` is attached to a content part.
The shape base class is of type `DisplayTaxonomyFieldViewModel`.

The following properties are available on the `TaxonomyField` class.

| Property | Type | Description |
| --------- | ---- |------------ |
| `TaxonomyContentItemId` | `string` | The Content Item id of the taxonomy associated with the field. |
| `TermContentItemIds` | `string[]` | The list of Content Item ids of the terms selected for this field. |

### DisplayTaxonomyFieldViewModel

This class is used when displaying a field.

The following properties are available on the `DisplayTaxonomyFieldViewModel` class.

| Property | Type | Description |
| --------- | ---- |------------ |
| `Field` | `TaxonomyField` | The `TaxonomyField` instance|
| `Part` | `ContentPart` | The part this field attached to |
| `PartFieldDefinition` | `ContentPartFieldDefinition` | The part field definition |

## Orchard Helpers

### GetTaxonomyTermAsync

Returns a the term from its content item id and taxonomy.

```
@foreach(var termId in Model.Field.TermContentItemIds)
{
    @await OrchardCore.GetTaxonomyTermAsync(Model.Field.TaxonomyContentItemId, termId);
}
```

### GetInheritedTermsAsync

Returns the list of terms including their parents.

```
@foreach(var termId in Model.Field.TermContentItemIds)
{
    <div>
    @foreach(var parent in await OrchardCore.GetInheritedTermsAsync(Model.Field.TaxonomyContentItemId, termId))
    {
        @parent
    }    
    </div>
}
```

### QueryCategorizedContentItemsAsync

Provides a way to query content items that are categorized with specific terms.

## Taxonomy Index

The `TaxonomyIndex` SQL table containes a list of all content items that are associated 
with a Taxonomy field. Each record corresponds to a selected term for a field.

| Column | Type | Description |
| --------- | ---- |------------ |
| TaxonomyContentItemId | `string` | The content item id of the Taxonomy |
| ContentItemId | `string` | The content item id of the categorized content |
| ContentType | `string` | The content type of the categorized content |
| ContentPart | `string` | The content part containing the field |
| ContentField | `string` | The name of the field in the content part |
| TermContentItemId | `string` | The content item id of the categorized Term |

For instance if a field has two selected terms, there will be two records with all 
identical column values except for the `TermContentItemId`.