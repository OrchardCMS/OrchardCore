# Indexing (`OrchardCore.Indexing`)

The `Indexing` module provides services to index content items. It does so by storing an append-only log of
content item entries, and providing a service to query this list with a cursor-like interface. An entry can
be either an `Update` or a `Deletion` task. This list of tasks can also be seen as an event store for content items.

Other modules can then store their own cursor location for this list, and check for updates and deletions
of content items and do custom operations based on these changes.

## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/6jJH9ntqi_A" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/IYKEeYxeNck" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
