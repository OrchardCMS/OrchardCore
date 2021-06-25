# Tenants (`OrchardCore.Tenants`)

The `Tenants` module allows to manage tenants from the admin.

## Static File Provider Feature

This feature registers a file provider for each tenant in order to serve custom files per tenant, even if they have the same names.

Once enabled on a tenant, a folder `wwwroot` is created in the `App_Data\Sites\[Tenant]` folder. Any file that is placed in this folder will be served under the same domain and prefix as the tenant.

Any static file that is placed in the content root folder of the website will be served
first.

## robots.txt for Tenants

Static File Provider allows you to setup `robots.txt` per tenant. 
To create `robots.txt` for each tenant, Place `robots.txt` under `App_Data\Sites\[Tenant]\wwwroot` folder 

E.g.

`App_Data\Sites\Tenant1\wwwroot\robots.txt`

`App_Data\Sites\Tenant2\wwwroot\robots.txt`
 