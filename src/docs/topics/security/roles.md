# Roles and Permissions

Roles group [permissions](../../reference/modules/Security/Permissions.md) and are managed under `Security → Roles`. Each module contributes its own permissions (manage content, manage media, etc.), and the role editor displays them all as a grid of checkboxes.

Orchard Core defines a set of [predefined roles](../../reference/modules/Roles/README.md#predefined-roles): `Administrator`, `Editor`, `Moderator`, `Author`, `Contributor`, and the two special roles `Anonymous` (unauthenticated visitors) and `Authenticated` (any logged-in user).

Roles are assigned to [users](users.md) from the user editor, and can also be configured from a recipe: see the [Roles module documentation](../../reference/modules/Roles/README.md).
