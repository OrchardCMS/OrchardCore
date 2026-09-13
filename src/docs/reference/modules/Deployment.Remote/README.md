# Remote Deployment (`OrchardCore.Deployment.Remote`)

The Remote Deployment module adds remote Orchard Core sites as execution targets for [deployment plans](../Deployment/README.md). When a plan is executed, the source site generates a deployment package, sends it to the destination site, and the destination immediately imports and executes its recipe.

## Prerequisites

- Enable the **Deployment** and **Remote Deployment** features on both sites.
- Ensure the source site can send HTTPS requests to the destination site.
- Enable every feature required by the exported recipe steps on the destination site.
- Grant the appropriate deployment permissions only to trusted administrators.

!!! warning
    A remote deployment executes a recipe on the destination site without prompting for confirmation. Use a dedicated, strong API key, transmit it only over HTTPS, and configure clients only for trusted source sites.

## Configure the destination site

The destination site authorizes incoming packages through a remote client:

1. In the destination site's admin, go to **Tools** > **Deployments** > **Remote Clients**.
2. Select **Add Remote Client**.
3. Enter a unique client name.
4. Generate a strong API key, store a copy securely, and enter it in the **API Key** field.
5. Save the client.

The destination protects the remote client's API key using ASP.NET Core Data Protection. If Data Protection keys are lost or changed, update the client with a new API key and update the matching remote instance on the source site.

## Configure the source site

Add the destination as a remote instance:

1. In the source site's admin, go to **Tools** > **Deployments** > **Remote Instances**.
2. Select **Add Remote Instance**.
3. Enter a descriptive name.
4. Set **URL** to the destination tenant's remote import endpoint:

    ```text
    https://example.com/OrchardCore.Deployment.Remote/ImportRemoteInstance/Import
    ```

    Include the tenant URL prefix when the destination is not the default tenant.

5. Enter the client name and API key created on the destination site.
6. Save the remote instance.

!!! warning
    The source site's remote instance document contains the API key needed to call the destination. Restrict access to the tenant database, backups, and the **Manage remote instances** permission.

## Deploy a plan

1. On the source site, go to **Tools** > **Deployments** > **Plans**.
2. Open a deployment plan.
3. Select **Execute**.
4. Select the configured remote instance.

The source builds a `.zip` package and sends it as a multipart HTTP `POST` request. The destination validates the client name and API key, extracts the package, and executes `Recipe.json`. A success notification on the source confirms that the destination returned an HTTP `200 OK` response.

If the deployment fails, verify:

- The destination URL includes the correct tenant prefix and endpoint path.
- The client name and API key match on both sites.
- The destination has the features required by every recipe step.
- Reverse proxies allow multipart `POST` requests and packages of the required size.
- The destination application logs do not contain recipe execution or file-upload validation errors.

## Permissions

The module defines permissions for managing remote instances, managing remote clients, and exporting to remote instances. Executing a deployment plan also requires the Deployment module's **Export Data** permission. Administrators receive these permissions by default.

Use separate role assignments when operators should be allowed to deploy but should not be allowed to create or reveal remote credentials.

## Security considerations

- Always use HTTPS because the client name and API key are sent with the package.
- Use a different API key for each source and destination pairing.
- Rotate API keys periodically and immediately after suspected disclosure.
- Remove remote clients and instances that are no longer used.
- Treat exported packages as sensitive because they can contain content, configuration, and files selected by the deployment plan.
- Review the destination site's upload limits and file-validation configuration before transferring large packages.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/2c5pbXuJJb0" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## OAuth, Pomi and MCP management

Enable Remote Deployment and remote management in the selected tenant, then refresh
Pomi discovery. Setup with `--enable-remote-management` already provides a client
credentials context; automation does not need an interactive login.

```sh
pomi deployment remote-clients list
pomi deployment remote-clients create --body-file private-client.json
pomi deployment remote-clients show <id>
pomi deployment remote-clients update <id> --body-file private-client.json
pomi deployment remote-clients delete <id> --force
pomi deployment remote-instances list
pomi deployment remote-instances create --body-file private-destination.json
pomi deployment remote-instances show <id>
pomi deployment remote-instances update <id> --body-file private-destination.json
pomi deployment remote-instances delete <id> --force
pomi deployment targets list
pomi deployment targets send <id> --plan-id <plan-id> --force
```

Client bodies contain `clientName` and `apiKey`. Destination bodies contain `name`,
`url`, `clientName` and `apiKey`. All non-key fields are required on update; an
omitted or null key preserves its stored value. Empty keys are rejected; delete the
client to revoke it. Creation retries with identical names and configuration return
`changed: false`; conflicting configuration returns 409. Equivalent updates and
repeated deletes are unchanged. IDs belong to the selected tenant.

Keys are write-only through these APIs. Readback returns `hasApiKey` and never
returns plaintext or protected keys. Supply keys using stdin or private body files,
not command arguments. Store the same strong generated key in the destination's
client and source's instance. The destination protects its client key; the source
retains its existing database storage semantics described above. These credentials
secure the existing import protocol; OAuth secures configuration and send commands.

All operations require `AccessRemoteManagement`. Client and instance administration
also require `ManageRemoteClients` or `ManageRemoteInstances`, respectively. Target
listing requires `ExportRemoteInstances`; sending additionally requires `Export`
and the permissions enforced by the selected export sources. Target listing reveals
only the destination identity and display name. All management mutations require
HTTPS. Destination URLs must use HTTPS without user information or fragments; HTTP
is accepted only for loopback development. Admin and API validation share these rules.

A send builds the existing plan archive and submits it once through the same sender
as the admin action. Redirects are disabled, so deployment credentials cannot follow
a redirect to another location. The HTTP request has a two-minute timeout. A remote
error, timeout or disconnected request can occur after import has started: inspect
the destination before sending again. Sending is not idempotent and has no automatic
retry. The existing receiver now stages uploads through the shared bounded package
validator and returns an error status when recipe execution fails, instead of reporting
HTTP 200. Imports can partially commit before failure.

The JSON management operations are also available through the tenant MCP catalog.
The binary package continues to travel directly from the source site to the configured
destination; it is not embedded into tool results.
