# Tenants (`OrchardCore.Tenants`)

The `Tenants` module allows to manage tenants.

## Static File Provider Feature

This feature registers a file provider for each tenant in order to serve custom files per tenant, even if they have the same names.

Once enabled on a tenant, a folder `wwwroot` is created in the `App_Data` folder. Any file that is placed in this folder will be served under the same domain and prefix as the tenant.

Any static file that is placed in the content root folder of the website will be served
first.
