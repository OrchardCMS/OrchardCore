# Bag (`OrchardCore.Flows`)

The BagPart is part of the Flows module, and is a content part that can contain multiple types of content items directly within it.

They are stored in the database as one single document, which makes them very powerful.

The BagPart shares a lot of similarities with the FlowPart in design, the difference is that with the BagPart you are able to specify exactly which content types can be contained within it.

This is done through the Settings for the BagPart.

BagParts may also be added to a Content Type Definition as a NamedPart. This allows one container item to have multiple BagParts.

An example of this can be found in TheAgencyTheme where four Named BagParts are used.

- Services
- Portfolio
- About
- Team

## Templating in a decoupled manner.

When templating in a decoupled manner the content items are accessed directly through the name of the BagPart.

=== "Liquid"

    ``` liquid
    {% for service in Model.ContentItem.Content.Services.ContentItems %}
        <h4 class="service-heading">{{ service.DisplayText }}</h4>
        <p class="text-muted">{{ service.HtmlBodyPart.Html | raw }}</p>
    {% endfor %}
    ```

=== "Razor"

    ``` html
    @foreach (var item in Model.ContentItem.Content.Services.ContentItems)
    {
        <h4 class="service-heading">@item.DisplayText</h4>
        <p class="text-muted">@Html.Raw(item.Content.HtmlBodyPart.Html)</p>
    }
    ```

In this example Services is a Named BagPart.

## Templating with Display Management

When templating with Liquid the `shape_build_display` filter is used on the contained items to build
the display shapes for the content items, then the `shape_render` filter is used to render these shapes.

When templating with Razor the `IContentItemDisplayManager` is used on the contained items to call `BuildDisplayAsync`
to build the display shapes for the  content items, then `DisplayAsync` is used to render these shapes.

=== "Liquid"

    ``` liquid
    <section class="flow">
        {% for item in Model.ContentItems %}
            {{ item | shape_build_display: "Detail" | shape_render }}
        {% endfor %}
    </section>
    ```

=== "Razor"

    ``` html
    @using OrchardCore.Flows.ViewModels

    @model BagPartViewModel
    @inject OrchardCore.ContentManagement.Display.IContentItemDisplayManager ContentItemDisplayManager

    <section class="flow">
        @foreach (var item in Model.BagPart.ContentItems)
        {
            var itemContent = await ContentItemDisplayManager.BuildDisplayAsync(item, Model.BuildPartDisplayContext.Updater, Model.Settings.DisplayType ?? "Detail", Model.BuildPartDisplayContext.GroupId);

            @await DisplayAsync(itemContent)
        }
    </section>
    ```

## Template Alternates

The BagPart supports standard alternates and for a Named BagPart, you can include the name of the part, in the alternate.

`MyBag-BagPart.liquid`

`MyBag-MyNamedBagPart.liquid`

In the first example we have an alternate specifying the Content Type `MyBag` and the `BagPart`.

In the second example we have an alternate for the Content Type `MyBag`, and the Named BagPart `MyNamedBagPart`

The templates and alternate names for the Content Items contained in a BagPart are the same as standard Content Items.

!!! note
    More alternates are available. You can examine these using the `ConsoleLog` Razor Helper or `console_log` liquid filter.

## Placement Differentiator

The name of a BagPart is used as the differentiator in `placement.json`

```json
  "BagPart": [
    {
      "differentiator": "MyNamedBagPart"
    }
  ]
```