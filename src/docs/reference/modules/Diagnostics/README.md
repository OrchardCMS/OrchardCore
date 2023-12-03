# Diagnostics (`OrchardCore.Diagnostics`)

## Purpose

Enables you to present HTTP errors in a personalized style and offers a means to modify the response of particular HTML errors. By default, we utilizes the following templates.

| Template | Description |
| --------- | ----------- |
| `HttpError__BadRequest` | Generates the `400` HTTP error page. You can adjust its appearance by modifying the `HttpError-BadRequest.cshtml` or `HttpError-BadRequest.liquid` views. |
| `HttpError__Forbidden` | Generates the `403` HTTP error page. You can adjust its appearance by modifying the `HttpError-Forbidden.cshtml` or `HttpError-Forbidden.liquid` views. |
| `HttpError__NotFound` | Generates the `404` HTTP error page. You can adjust its appearance by modifying the `HttpError-NotFound.cshtml` or `HttpError-NotFound.liquid` views. |
| `HttpError__Unauthorized` | Generates the `401` HTTP error page. You can adjust its appearance by modifying the `HttpError-Unauthorized.cshtml` or `HttpError-Unauthorized.liquid` views. |
| `HttpError__HttpStatusCode` | Fallback template which generates the HTTP error page when no explicit template defined (ex, `HttpError-MethodNotAllowed.cshtml`). You can adjust its appearance by modifying the `HttpError.cshtml` or `HttpError.liquid` views. |

### Example

To alter the presentation of a particular HTTP status code, you can easily create a template within your theme that corresponds to the HTTP status code. For instance, if you wish to customize the 404 error page, you can achieve this by creating a template in your theme named `HttpError-NotFound.cshtml` or `HttpError-NotFound.liquid` as outlined below.

```
<h1>@T["The page could not be found."]</h1>
```

We utilize a template named `HttpError` to structure the default output of the undefined error.

## Utilizing the Templates Feature for Customizing Error Pages

You can utilize the [Templates](../Templates/README.md) feature to modify the appearance of your error pages.

To illustrate, if you want to alter the view of the `403 (Forbidden)` page using the Template feature, create a new Template named `HttpError__Forbidden` and insert your customized HTML as follows:

```
<h2 class="text-danger">{{ "You do not have access permission to this page." | t }}</h2>
```
