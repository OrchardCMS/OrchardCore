# Authentication and contexts

Apply the [shared operating rules](shared-rules.md). Read only the flow needed
for the target tenant; existing authenticated contexts do not need a new login.

- [Automatically provisioned application contexts](#automatically-provisioned-application-contexts)
- [Connect, browser login, and device authorization](#connect-to-an-existing-tenant)
- [Manage saved contexts](#manage-contexts)
- [Client credentials configuration](#client-credentials-configuration)

## Automatically provisioned application contexts

For automated website or tenant creation, use `--enable-remote-management` on
`pomi install` or `pomi tenants install`. For an existing running tenant, use
`pomi --context "$HOST_CONTEXT" tenants enable-remote-management <tenant> --provision-client`.
These flows enable CLI/OpenID dependencies, register a dedicated administrative
application, and return a unique `context`. Select that name explicitly and run
`pomi --context "$CONTEXT" api refresh --output json` once the server is running.
Pomi discovers the authority, obtains an application token, and renews it when
needed. No `context add`, `pomi login`, or device approval is required.

Do not supply a host's `OC_CLIENT_ID`/`OC_CLIENT_SECRET` to a child command:
explicit environment credentials take precedence over the saved credentials.
Keep the administrator username/password in the separate
[private handoff file](setup-password.md#credential-handoff) for the user.
The application represents itself, not that administrator, and retains its own
access even if the user's password changes. Provisioned applications have the
tenant administrator role; use a separately configured limited application for
jobs that only need narrower permissions.

Pomi stores application secrets in Windows Credential Manager on Windows or
owner-only plaintext credential files on macOS/Linux, following the existing
credential-store policy. Do not copy them into site files or the human handoff.
`pomi logout` removes the local stored application credentials; it does not delete
the server application. Delete the application in OpenID administration to stop
future token issuance when revocation is authorized. Reprovisioning creates another
application and context, not a token refresh or secret rotation.

If provisioning flags are missing, check live help and matching CLI/server versions.
If setup partly succeeds, inspect the tenant before retrying and retain the user
credential file. Do not silently switch an unattended task to interactive login.
For a setup-only request without additional remote management, omit the install
flag; ordinary recipe behavior and administrator-account creation still apply.

## Connect to an existing tenant

Use this manual flow when human sign-in is wanted and no provisioned application
context applies. Existing valid contexts need no new login.

First run `pomi context list --output json`. Reuse an existing context only for
an intentionally selected existing site. When adding one, follow the
[unique context naming rules](shared-rules.md#unique-context-names-after-setup),
set `CONTEXT` to an unused name and `SITE_URL` to the exact tenant URL. Do not
copy `production`, `default`, or another example name without checking it.

```bash
pomi context add "$CONTEXT" "$SITE_URL" --current
pomi --context "$CONTEXT" login
pomi --context "$CONTEXT" doctor
pomi --context "$CONTEXT" api refresh
pomi --context "$CONTEXT" --help
```

For agent-driven device login, prefer separate commands so each process returns
one JSON result and the user can approve between tool calls:

```bash
pomi --context "$CONTEXT" login device start --output json
pomi login device show <session-id> --qr always --output json
pomi login device wait <session-id> --output json
```

`start` returns immediately with `sessionId`, `verificationUri`, `userCode`,
`expiresAt`, and optional `qrCode: { mediaType: "image/png", base64: "..." }`.
Present the URL/code or decoded image to the human. The `authorization_pending`
status and exit code 0 mean a request was created, not a successful login.
`show` redisplays it offline; the example opts into a base64 PNG with `--qr always`.
`wait` polls until approval/denial/expiry and saves credentials on success. Use the local session ID, never the displayed user code
or a private device code. Do not read pending session files or expose their contents.
Resume an interrupted wait using the same ID before expiry; only one waiter is
allowed. The original context remains bound even if the current context changes.
Keep the same local CLI configuration; context identity changes require a fresh start.

Use `pomi login --grant device` for a combined interactive flow. It prints a URL/code;
QR output is disabled by default. Add `--qr auto` to render a QR code in compatible
terminals or `--qr always` to force ANSI/Unicode QR output.
With JSON output and either opt-in QR mode, stdout first emits an
`authorization_pending` record with `verificationUri`, `userCode`, `expiresAt`,
and `qrCode: { mediaType: "image/png", base64: "..." }`. Present this image or URL
while the process runs. Consume a stream of JSON values: the normal login result
follows after approval. The pending record is not a successful login.
The human can open the URL or scan an enabled QR code on a phone that can reach the tenant; keep the
CLI waiting while they verify the matching code and approve the request.
`pomi login --no-browser`
prints the browser-flow URL for opening on the same computer. Browser login uses
authorization code with PKCE. On Windows, human tokens are encrypted by Windows
Credential Manager. On macOS, Linux, and other Unix-like systems, they are
plaintext owner-only files under `~/.orchardcore/credentials` (`0700`
directory, `0600` files).

For multiple tenants, start one device flow per context and complete each code
as the intended tenant-local user:

```bash
pomi --context news login --grant device
pomi --context marketing login --grant device
pomi --context commerce login --grant device
```

Do not reuse tokens across contexts. Device authorization can automate the CLI
side, but a user must approve each code. For browser login, enter credentials
only in the tenant's HTTPS login page or a trusted password manager. Never pass
browser passwords through CLI arguments, chat, logs, screenshots, or browser
automation scripts.

For automation, use a dedicated confidential OpenID application with minimal
permissions:

```bash
export OC_CLIENT_ID=orchard-automation
export OC_CLIENT_SECRET='<injected-by-secret-store>'
pomi --context "$CONTEXT" content items list
```

Do not use the public `orchardcore-cli` client for client credentials.

## Manage contexts

```bash
pomi context list --output json
pomi context use news
pomi --context "$CONTEXT" content items list
pomi logout news
pomi context delete news --force
pomi context clear --force
```

For cache freshness and offline help, see [shared discovery rules](shared-rules.md#cache-and-compatibility).

## Client credentials configuration

The environment-variable example above assumes a confidential application is
already configured in this tenant. For its roles, scopes, grants, and secret
setup, read the official [authentication API reference](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/api/authentication/README.md)
and [Remote Management reference](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/RemoteManagement/README.md).
Do not create applications or grant additional permissions unless the requested
automation requires and authorizes that configuration.
