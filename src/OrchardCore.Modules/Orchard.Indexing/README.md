# Indexing (Orchard.Indexing)

The Indexing module provides services to index content items. It does so by storing an append-only log of 
content item entries, and providing a service to query this list with a cursor-like interface. And entry can 
either be an `Update` or a `Deletion` task. This list of tasks can also be seen as an event store for content items.

Other modules can then store their own cursor location for this list, and check for updates and deletions
of content items then do a custom operation based on these changes.