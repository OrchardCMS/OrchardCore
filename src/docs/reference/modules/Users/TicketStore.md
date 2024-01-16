# Users Authentication Ticket Store

Out of the box Orchard Core stores the authentication tickets inside a cookie that resides on the client/browser. For some scenarios, requirement is to store lot of permissions that result in larger cookie on request - that could result in falied request due to exceeding header limit. Use this feature to reduce the cookie size and store the authentication tickets server side.

Enabling Users Authentication Ticket Store feature, stores users authentication tickets on server in memory cache. If [distributed cache](../Redis/README.md) feature is enabled it will store authentication tickets on distributed cache.
