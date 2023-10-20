# Diagnostics (`OrchardCore.Diagnostics`)

## Purpose

Enables you to present HTTP errors in a personalized style and offers a means to modify the response of particular HTML errors. The default setting utilizes the following templates.

 - BadRequest
 - Forbidden
 - NotFound
 - Unauthorized
 - HttpStatusCode

### Example

To alter the presentation of a particular HTTP status code, you can easily create a template within your theme that corresponds to the HTTP status code. For instance, if you wish to customize the 404 error page, you can achieve this by creating a template in your theme named `NotFound.cshtml` or `NotFound.liquid` as outlined below.

```
<h1>@T["The page could not be found."]</h1>
```

We utilize a template named `HttpStatusCode` to structure the default output of the undefined error.
