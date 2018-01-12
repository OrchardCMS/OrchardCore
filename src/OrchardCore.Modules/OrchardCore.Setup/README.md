# Setup (OrchardCore.Setup)

## Recipe Parameters

During setup, all recipes have access to the setup screen values using these parameters:

| Parameter | Description |
| --- | --- |
| `SiteName` | The name of the site |
| `AdminUsername` | The username of the super user |
| `AdminEmail` | The email of the super user |
| `AdminPassword` | The password of the super user |
| `DatabaseProvider` | The database provider |
| `DatabaseConnectionString` | The connection string |
| `DatabaseTablePrefix` | The database table prefix |

These parameters can be used in the recipe using a scripted value like `[js: parameters('AdminUsername')]`.

## GraphQL

You can use a GraphQL endpoint to initiate setup.

## Mutation

### Create Tenant


mutation CreateTenant { 
    createTenant(
        SiteName: "",
        DatabaseProvider: "",
        UserName: "",
        Email: "",
        Password: "",
        PasswordConfirmation: "",
        RecipeName: ""
    ) { executionId } }

### createSite
Create a tenant

#### Input Fields

##### siteName (`!String`)
The name of the site

##### databaseProvider (`!String`)
The database the tenant will use, eg. Sqlite

##### userName (`!String`)
The user name of the site admin

##### email (`!String`)
The email address of the site admin

##### password (`!String`)
password to log in with

##### recipeName (`!String`)
The recipe to run to load the site, eg. `Blog` or `Agency`

The recipe names are defined in the recipe files marked `*.recipe.json`.

#### Return Fields

##### executionId (`!String`)
The unique id for the execution of creating a site