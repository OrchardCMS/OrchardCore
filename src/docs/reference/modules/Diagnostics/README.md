# Diagnostics (`OrchardCore.Diagnostics`)

## Purpose

Enables you to present HTTP errors in a personalized style and offers a means to modify the response of particular HTML errors. By default, we utilizes the following templates.

| Template | Description |
| --------- | ----------- |
| `BadRequest` | Generates the `400` HTTP error page. You can adjust its appearance by modifying the `BadRequest.cshtml` or `BadRequest.liquid` views. |
| `Forbidden` | Generates the `403` HTTP error page. You can adjust its appearance by modifying the `Forbidden.cshtml` or `Forbidden.liquid` views. |
| `NotFound` | Generates the `404` HTTP error page. You can adjust its appearance by modifying the `NotFound.cshtml` or `NotFound.liquid` views. |
| `Unauthorized` | Generates the `401` HTTP error page. You can adjust its appearance by modifying the `Unauthorized.cshtml` or `Unauthorized.liquid` views. |
| `HttpStatusCode` | Fallback template which generates the HTTP error page when no explicit template defined (ex, `MethodNotAllowed.cshtml`). You can adjust its appearance by modifying the `HttpStatusCode.cshtml` or `HttpStatusCode.liquid` views. |


### Example

To alter the presentation of a particular HTTP status code, you can easily create a template within your theme that corresponds to the HTTP status code. For instance, if you wish to customize the 404 error page, you can achieve this by creating a template in your theme named `NotFound.cshtml` or `NotFound.liquid` as outlined below.

```
<h1>@T["The page could not be found."]</h1>
```

We utilize a template named `HttpStatusCode` to structure the default output of the undefined error.

## Utilizing the Templates Feature for Customizing Error Pages

You can utilize the [Templates](../Templates/README.md) feature to modify the appearance of your error pages.

To illustrate, if you want to alter the view of the `403 (Forbidden)` page using the Template feature, create a new Template named `Forbidden` and insert your customized HTML as follows:

```
<h2 class="text-danger">{{ "You do not have access permission to this page." | t }}</h2>
```
