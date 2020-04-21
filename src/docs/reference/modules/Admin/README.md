# Admin (OrchardCore.Admin)

The Admin module provides an admin dashboard for your site.

## Custom Admin prefix

If you want to specify another prefix in the urls to access the admin section, you can change it by using this option in the appsettings.json:

``` json
  "OrchardCore": {
    "OrchardCore_Admin": {
      "AdminUrlPrefix": "YourCustomAdminUrl"
	  }
	}
```
