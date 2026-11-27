# Users Authentication Ticket Store

Out of the box Orchard Core stores the authentication tickets inside a cookie that resides on the client/browser. For some scenarios, requirement is to store lot of permissions that result in larger cookie on request - that could result in failed request due to exceeding header limit. Use this feature to reduce the cookie size and store the authentication tickets server side.

Enabling Users Authentication Ticket Store feature, stores users authentication tickets on server in memory cache. If [distributed cache](../Redis/README.md) feature is enabled it will store authentication tickets on distributed cache.

## Videos

This video reacts to registrations, sign-ins and role changes with a workflow.

<!-- video:workflows-user-events -->
<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/5OSh78XTbI4" title="Workflows: React to Registrations, Sign-Ins and Role Changes" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

This video creates, edits, disables and deletes user accounts.

<!-- video:users-manage -->
<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/zuhO5OFcxmA" title="Users: Create, Edit, Disable and Delete Accounts" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

This video sets the sign-in options and the lockout of user accounts.

<!-- video:users-login-settings -->
<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/e5b3QRTfdO0" title="User Login Settings: Sign-In Options and Lockout" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

This video keeps sign-in tickets on the server with the authentication ticket store.

<!-- video:users-ticket-store -->
<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/X1TKmwKmo3g" title="Authentication Ticket Store: Keep Sign-In Tickets on the Server" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
