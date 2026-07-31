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
