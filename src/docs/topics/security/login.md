# Login

The login screen is available at `/Login` by default (the path is [configurable](../../reference/modules/Users/README.md#custom-paths)). Its behavior is configured under `Security → Settings → User Login`:

- Render the login page with the site theme instead of the admin theme.
- Disable local (username/password) login entirely to force authentication through an external provider.
- Control the **Remember me** option and whether authentication cookies persist across browser sessions. **Allow Remember me** determines whether visitors can choose persistence on the login form. When it is disabled, **Use persistent authentication cookies by default** determines whether all local and external sign-ins use persistent cookies. When it is enabled, the latter setting supplies the default value of the **Remember me** checkbox.
- Require and configure two-factor authentication, optionally only for specific roles.

Related options live in their own places:

- **External providers**: enable and configure [Microsoft](../../reference/modules/Microsoft.Authentication/README.md), [Facebook](../../reference/modules/Facebook/README.md), [Twitter](../../reference/modules/X/README.md), [GitHub](../../reference/modules/GitHub/README.md), [Google](../../reference/modules/Google/README.md), or any [OpenID Connect](../../reference/modules/OpenId/README.md) provider.
- **Password strength rules**: see [Change password configuration](../../guides/password-configuration/README.md).
- **Account lockout after failed attempts**: see [Change lockout configuration](../../guides/lockout-configuration/README.md).

To let visitors create their own account, see [Registration](registration.md).
