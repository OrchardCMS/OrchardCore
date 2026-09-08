# Users

Orchard Core ships with a complete membership system, provided by the [Users module](../../reference/modules/Users/README.md), which is enabled by default in the CMS recipes.

Users are managed in the admin under `Security → Users`. From there you can:

- Create, edit, disable, or delete users.
- Assign one or several [roles](roles.md) to a user.
- Reset a user's password or edit their email and phone number.

The Users module also provides optional features you can enable individually, among others:

- **Users Registration**: lets visitors create their own account (see [Registration](registration.md)).
- **Reset Password**: lets users reset a forgotten password by email.
- **Users Change Email**: lets users change their own email address.
- **Two-Factor Authentication methods** (email or authenticator app), covered in [Login](login.md).
- **Custom User Settings**: extend the user profile with your own settings, defined like content types ([documentation](../../reference/modules/Users/CustomUserSettings/README.md)).

See the [Users module documentation](../../reference/modules/Users/README.md) for the full feature list, the custom authentication paths, and the recipe steps to configure these features from code.
