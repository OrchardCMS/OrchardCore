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

```csharp
@foreach(var termId in Model.TermContentItemIds)
{
    @await OrchardCore.GetTaxonomyTermAsync(Model.TaxonomyContentItemId, termId);
}
```

### GetInheritedTermsAsync

Returns the list of terms including their parents.

```csharp
@foreach(var termId in Model.TermContentItemIds)
{
    <div>
    @foreach(var parent in await OrchardCore.GetInheritedTermsAsync(Model.TaxonomyContentItemId, termId))
    {
        @parent
    }
    </div>
}
```

### QueryCategorizedContentItemsAsync

Provides a way to query content items that are categorized with specific terms.

## Liquid Tags

### taxonomy_terms

The `taxonomy_terms` filter loads the specified term content items.

#### taxonomy_terms Example 

The following example lists all the terms related to the **Colors** field on the **BlogPost**
content type, then renders them.

```liquid
{% assign colors = Model.ContentItem.Content.BlogPost.Colors | taxonomy_terms %}
{% for c in colors %}
  {{ c }}
{% endfor %}
```

The `taxonomy_terms` also accepts term content item ids as input, as long as the first
argument is a taxonomy content item id.

#### Example

The following example displays all the colors and their hierarchy

```liquid
{% assign taxonomyId = Model.ContentItem.Content.BlogPost.Colors.TaxonomyContentItemId %}

{% for colorId in Model.ContentItem.Content.BlogPost.Colors.TermContentItemIds %}
  <div>
    {% assign parentColors = colorId | inherited_terms: taxonomyId %}
    {% for c in  parentColors %}
      {{ c }}
    {% endfor %}
  </div>
{% endfor %}
```

### inherited_terms

The `inherited_terms` filter loads all the parents of a given term. The input must be
a term content item or content item id. The first argument must be the taxonomy content 
item or content item id.

## Taxonomy Index

The `TaxonomyIndex` SQL table contains a list of all content items that are associated 
with a Taxonomy field. Each record corresponds to a selected term for a field.

| Column | Type | Description |
| --------- | ---- |------------ |
| TaxonomyContentItemId | `string` | The content item id of the Taxonomy |
| ContentItemId | `string` | The content item id of the categorized content |
| ContentType | `string` | The content type of the categorized content |
| ContentPart | `string` | The content part containing the field |
| ContentField | `string` | The name of the field in the content part |
| TermContentItemId | `string` | The content item id of the categorized Term |

For instance if a field has two selected terms, there will be two records with all identical column values except for the `TermContentItemId`.
