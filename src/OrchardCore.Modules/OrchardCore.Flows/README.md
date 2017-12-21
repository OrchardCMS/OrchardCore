# Flows (OrchardCore.Flows)

The Flows module provides methods to display content items directly within another content item. This is achieved with the Flow Part and the Bag Part.

A good example of this would be a page with a FAQ section in it. A FAQ content type might have a question and an answer field, and the content editor can add new FAQs directly when editing the page.

### Empty Flows and Bags

Flows and Bags that do not contain any content items will be hidden by default. This can be overridden in the `placement.json` file. One thing to note is that the shape names for empty Flows and Bags are different to those that are used when the Flows and Bags contain content items. For empty Flows, the shape name name is `FlowPart_NoItems`; for empty Bags, it's `BagPart_NoItems`.

For example, to display a shape when a Flow part has no items, you can add this to your placement file:

```
  "FlowPart_NoItems": [
    {
      "place": "Content:5"
    }
  ]
```
And if you'd like to use the same template for Flow parts that have items and Flow parts that have no items, you could add this:

```
  "FlowPart_NoItems": [
    {
      "place": "Content:5",
      "shape": "FlowPart" 
    }
  ]
```