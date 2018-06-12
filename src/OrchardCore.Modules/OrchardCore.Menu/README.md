# Menu (OrchardCore.Menu)

## Templates

Example of alternate or template for a `Menu`:

```liquid
<nav>
    <ul class="nav navbar-nav {{ Model.Classes | join: " " }}">
        {% for item in Model.Items %}
            {{ item | shape_render }}
        {% endfor %}
    </ul>
</nav>
```

```razor
@{
    TagBuilder tag = Tag(Model, "ul");
    tag.AddCssClass("navbar-nav mr-auto");

    foreach (var item in Model.Items)
    {
        tag.InnerHtml.AppendHtml(await DisplayAsync(item));
    }
}

@tag
```

Example of alternate or template for a `MenuItem`:

```liquid
<li class="nav-item{% if Model.HasItems %} dropdown{% endif %}">
    {% shape_clear_alternates Model %}
    {% shape_type Model "MenuItemLink" %}
    {{ Model | shape_render }}
    {% if Model.HasItems %}
    <div class="dropdown-menu">
        {% for item in Model.Items %}
        {{ item | shape_render }}
        {% endfor %}
    </div>
    {% endif %}
</li>
```

```razor
@{
    int level = (int)Model.Level;
    TagBuilder tag = Tag(Model, "li");

    if ((bool)Model.HasItems)
    {
        tag.AddCssClass("dropdown");
    }

    // Morphing the shape to keep Model untouched
    Model.Metadata.Alternates.Clear();
    Model.Metadata.Type = "MenuItemLink";

    tag.InnerHtml.AppendHtml(await DisplayAsync(Model));

    if ((bool)(Model.HasItems))
    {
        TagBuilder parentTag = Tag(Model, "ul");
        parentTag.AddCssClass("dropdown-menu");

        foreach (var item in Model.Items)
        {
            item.Level = level + 1;
            item.ParentTag = parentTag;
            parentTag.InnerHtml.AppendHtml(await DisplayAsync(item));
        }

        tag.InnerHtml.AppendHtml(parentTag);
    }
}

@tag
```

Example of alternate or template for a `MenuItemLink-LinkMenuItem`:

```liquid
{% assign link = Model.ContentItem.Content.LinkMenuItemPart %}

{% if Model.HasItems %}
    <a href="{{ link.Url | href }}" class="nav-link dropdown-toggle">{{ link.Name }}<b class="caret"></b></a>
{% else %}
    <a href="{{ link.Url | href }}" class="nav-link">{{ link.Name }}</a>
{% endif %}
```

```razor
@using OrchardCore.ContentManagement

@{
    ContentItem contentItem = Model.ContentItem;
    var linkMenuItemPart = contentItem.Content["LinkMenuItemPart"];
    int level = (int)Model.Level;
}

@if (level > 0)
{
    <a class="dropdown-item" href="@Url.Content((string)linkMenuItemPart.Url)">@linkMenuItemPart.Name</a>
}
else if ((bool)(Model.HasItems))
{
    <a class="nav-link dropdown-toggle" data-toggle="dropdown" href="@Url.Content((string)linkMenuItemPart.Url)">@linkMenuItemPart.Name<b class="caret"></b></a>
}
else
{
    <a class="nav-link" href="@Url.Content((string)linkMenuItemPart.Url)">@linkMenuItemPart.Name</a>
}
```

## CREDITS

### nestedSortable jQuery plugin

https://github.com/ilikenwf/nestedSortable  
License: MIT
