# Setup administrator passwords

Apply the [shared operating rules](shared-rules.md). This applies to local
`pomi install`, `pomi tenants install`, and `pomi tenants setup`.

The standard ASP.NET Core Identity setup policy requires:

- At least **6 characters** and at least **1 distinct character**.
- An ASCII uppercase letter (`A-Z`).
- An ASCII lowercase letter (`a-z`).
- An ASCII digit (`0-9`).
- A non-alphanumeric character, such as `!` or `-`.

Pomi checks this baseline before creating local project files or sending the
tenant install/setup request. All password input sources use the same check.
The tenant API also validates the management host's configured Identity password
requirements before creating a tenant or starting its setup recipe. An invalid
password produces a validation error without echoing its value.

## Generate a compliant password

Do not use a plain random hex/base64/URL-safe token as the setup password: it
may lack a required character group. The following Bash command uses Python 3's
cryptographic random source, explicitly includes all four groups, fills to
**24 characters**, and shuffles their positions. It always satisfies the standard
policy above. The generated password is captured in an environment variable;
it is not printed to the terminal or placed in a command argument.

```bash
OC_SITE_PASSWORD="$(python3 - <<'PYGEN'
import secrets
import string

groups = (string.ascii_uppercase, string.ascii_lowercase, string.digits, "!@#$%^&*()-_=+")
password = [secrets.choice(group) for group in groups]
alphabet = "".join(groups)
password.extend(secrets.choice(alphabet) for _ in range(24 - len(password)))
secrets.SystemRandom().shuffle(password)
print("".join(password))
PYGEN
)" || exit 1
export OC_SITE_PASSWORD
```

Save the [private handoff file](#credential-handoff) before setup. Then use the
password in the same shell session with `--password-env OC_SITE_PASSWORD`, for
example, with `HOST_CONTEXT` set to the existing host context selected from
`pomi context list --output json`:

```bash
pomi --context "$HOST_CONTEXT" tenants install Blog \
  --request-url-prefix blog --recipe-name Blog --site-name "My Blog" \
  --email admin@example.com --user-name admin --password-env OC_SITE_PASSWORD \
  --enable-remote-management --output json
```

## Credential handoff

For every new site or tenant, save the administrator username, email, and password
in a persistent private file **before setup** so the user can retrieve them.
This is required even with `--enable-remote-management`: Pomi authenticates as a
separate application, while the administrator account keeps the supplied password
for human sign-in. No password reset is needed because application credentials
were provisioned. Keep application client secrets and tokens in Pomi's credential
store, separate from this user credential file. Use the user's secure
location or a unique directory under `~/.config/pomi/site-credentials/` on
macOS/Linux (`%LOCALAPPDATA%\Pomi\site-credentials\` on Windows), outside the
site and repository. Create directories with mode `0700` and files with `0600`
(or private user ACLs on Windows); use exclusive creation, no symlink following
or overwriting, and verify permissions. Include the site/tenant name. The final URL
may be unknown before local setup selects a port: record it as pending, then update
this task's private record with the actual URL and returned Pomi context after
success, preserving its permissions. Include the human admin sign-in URL when
verified. The nonsecret installation JSON is not the credential handoff file.

A handoff record containing multiple fields is not a `--password-file` input:
that option expects only the raw password. Supply the password through the
existing environment variable or stdin, or a separate private raw-secret file.
Report only the handoff file's absolute path, never its contents. Preserve the record on
failure. A user-selected secret manager can hold an additional copy. After the command and secure handoff, remove
the temporary variable with `unset OC_SITE_PASSWORD`. Keep shell tracing
(`set -x`) disabled while generating and passing secrets. Do not replace a
user-supplied password silently; report unmet requirements and let the user
choose another password or authorize generation.

Custom hosts or recipe-enabled modules can enforce longer passwords, more
unique characters, or custom validators. This generator guarantees the standard
policy, not arbitrary custom rules. Use a password meeting the known custom
requirements when supplied. The CLI baseline check cannot predict policy
changes introduced by a custom setup recipe. Setup can also fail for unrelated
reasons after making partial progress: inspect the tenant and its errors before
retrying, and never automatically delete it or its database.
