# Create a Liquid Widget

A Widget is a reusable content type which can be used by multiple features, such as `Layers` and `Flows` to add widgets to your content.

A Liquid Widget allows the editor to use the liquid templating language to write content, consisting of html, css, and javascript.

!!! warning
    Do not create this content type if, for security reasons, you do not want your editors to be able to craft javascript.

## What you will build

You will add a Liquid Widget to the Content Type Definitions and create a template.

## What you will need

- A working Orchard Core CMS website.

## Create the Liquid Widget

- Navigate to the Orchard Core CMS admin area <https://localhost:5001/admin>

- Through the admin menu select _Content -> Content Definition -> Content Types_

- Select `Create new type` and enter the Display Name `Liquid Widget`. The technical name will auto populate, removing the spaces. Then select `Create`

- The Add content parts view will display next, and from there, select `Liquid`, and save your new content type.

- When the content type has been saved the view will return to the definition of your new Liquid Widget.

- Remove the `Creatable` and `Listable` flags as these are for content types that will be displayed in the contents admin list. We are building a widget which will be used by the `Layers` and `Flows` feature.

- Add `Widget` to the `Stereotype`, and select `Save`.

Your Widget can now be used from within the `Layers` module, by navigating to _Design -> Widgets_ or from a `Page` content type.

Read on to learn how to customize the template.

## Create a Liquid Widget template

By default this widget will use the standard Widget template.

The standard template contains wrapper divs and you may wish to customize how this widget is rendered with your own html or css classes.

``` html
<div class="widget widget-liquid-widget widget-align-justify widget-size-100">
    <div class="widget-body">
        <!-- The content of your widget is rendered here. -->
    </div>
</div>
```

To customize this template 

- Navigate to _Design -> Templates_

- Select `Add template` and name your template `Widget__LiquidWidget`

- Create a template with the following content

``` liquid
{{ Model.Content.LiquidPart | shape_render }}
```

This template will only render the `LiquidPart`, and will override the default widget template.

You could also choose to render the widget sizing classes.

``` liquid
<article class="{{ Model.Classes | join: " " }}">
    {{ Model.Content.LiquidPart | shape_render }}
</article>
```

A Razor template would be named `Widget-LiquidWidget.cshtml`

``` html
<article class="@String.Join(" ", Model.Classes.ToArray())">
    @await DisplayAsync(Model.Content.LiquidPart)
</article>
```

## Notes

You can use this technique to build more complex Widgets, which could contain multiple fields, or parts.

## Summary

You just learned how to add create a Liquid Widget and a template.
