# Flows (`OrchardCore.Flows`)

The Flows module provides methods to display content items directly within another content item. This is achieved with the Flow Part and the Bag Part.

A good example of this would be a page with an FAQ section in it. A FAQ content type might have a question and an answer field, and the content editor can add new FAQs directly when editing the page.

## Empty Flows and Bags

Flows and Bags that do not contain any content items will be displayed with a different shape name. For empty Flows, the shape name is `FlowPart_Empty`; for empty Bags, it's `BagPart_Empty`.

This allows you to conditionally show or hide empty Flows or Bags. For example, to hide a Flow part that has no items, you can add this to your placement file:

```json
  "FlowPart_Empty": [
    {
      "place": "-"
    }
  ]
```

And if you'd like to use the same template for Flow parts that have items and Flow parts that have no items, you could add this:

```json
  "FlowPart_Empty": [
    {
      "shape": "FlowPart"
    }
  ]
```

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/ufEhMXYZPy4" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
