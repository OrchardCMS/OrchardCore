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

## Feature Profiles

This feature allows the `Default` tenant to create Feature Profiles which can restrict the features available to a tenant with Feature Rules.

### Creating a feature profile

1. Enable the _Tenant Feature Profiles_ feature on the `Default` tenant.
2. Go to the _Configuration -> Tenant Feature Profiles_ menu.
3. Select _Add Feature Profile_.
4. Add a _Name_ and a set of _Rules_.

#### Rule Configuration

Rules are a JSON array of Rule Expressions. 

A rule consists of the Rule Name, and an Expression, which supports simple matching, i.e. characters, or the `*` or `?` modifier.

By default the available rules are `Exclude` and `Include`


Consider the following

``` json
[
  {
    "Rule": "Exclude",
    "Expression": "OrchardCore.AdminTemplates"
  },
  {
    "Rule": "Exclude",
    "Expression": "TheAgencyTheme"
  }
]
```

In this rule we exclude the `OrchardCore.AdminTemplates` and `TheAgencyTheme` features

or we can use wild card matching

``` json
[
  {
    "Rule": "Exclude",
    "Expression": "MyModules.Custom.*"
  },
  {
    "Rule": "Include",
    "Expression": "MyModules.Custom.IncludedModule"
  }
]
```

In this example we exclude all Features starting with `MyModules.Custom.`, and then specifically include `MyModules.Custom.IncludedModule`

Rules are processed in the order they are supplied, so reversing the order of the above will cause the last rule, i.e. the `Exclude` rule to be applied, negating the `Include` rule.

### Selecting a feature profile

1. Create a Feature Profile.
2. Go to the _Configuration -> Tenants_ menu.
3. Edit the tenant.
4 Select a feature profile from the dropdown.

The _Feature Profile_ drop down will only be available if at least one Feature Profile has been configured.

A feature profile can also be set when creating a tenant via the web API.
