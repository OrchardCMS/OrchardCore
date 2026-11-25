# Users Authentication Ticket Store

Out of the box Orchard Core stores the authentication tickets inside a cookie that resides on the client/browser. For some scenarios, requirement is to store lot of permissions that result in larger cookie on request - that could result in failed request due to exceeding header limit. Use this feature to reduce the cookie size and store the authentication tickets server side.

Enabling Users Authentication Ticket Store feature, stores users authentication tickets on server in memory cache. If [distributed cache](../Redis/README.md) feature is enabled it will store authentication tickets on distributed cache.

## Video

This video reacts to registrations, sign-ins and role changes with a workflow.

<!-- video:workflows-user-events -->
<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/5OSh78XTbI4" title="Workflows: React to Registrations, Sign-Ins and Role Changes" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
