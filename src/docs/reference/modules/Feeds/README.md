# Feeds (`OrchardCore.Feeds`)

The `OrchardCore.Feeds` module provides an extensible infrastructure for exposing content as syndication feeds, such as RSS. It does not expose any feed on its own: other modules plug into it to describe **what** to syndicate, while this module handles **how** the feed document is built and served.

## How it works

A feed request is served by the `Feed` controller and resolved through two extension points:

- `IFeedBuilderProvider` — selects the output format. The built-in `rss` builder is provided by `OrchardCore.Feeds.Core`. The desired format is passed with the `format` query value (e.g. `format=rss`).
- `IFeedQueryProvider` — selects the items to include, based on the request (for example, the items of a given list).

The highest-priority matching builder and query are used. If neither matches, the controller returns `404`.

## Feeds for lists

The [Lists](../Lists/README.md) module is the most common consumer. When the `Feeds` and `Lists` features are enabled, a List content item can expose an RSS feed of its contained items.

- The feed is enabled per list through the **RSS** settings on the list (`FeedMetadata`), which let you disable the feed (`DisableRssFeed`), set the number of items, or point to an external proxy URL (`FeedProxyUrl`).
- When enabled, an auto-discovery `<link rel="alternate" type="application/rss+xml">` is added to the page `<head>` so browsers and feed readers can find the feed.

## Providing your own feed

To syndicate something other than lists, implement `IFeedQueryProvider` (to produce the items) and, if you need a non-RSS format, `IFeedBuilderProvider`, then register them in your module's `Startup`. Call `services.AddFeeds()` to bring in the core feed services.
