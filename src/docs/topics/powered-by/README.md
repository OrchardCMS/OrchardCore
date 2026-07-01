# Configure Powered-By Middleware

Orchard Core adds `PoweredByMiddleware` into the pipeline, which adds a `X-Powered-By` HTTP header to all responses by default. You can configure it using the `OrchardCore_PoweredBy` section in the `appsettings.json` file:

```json
{
  "OrchardCore": {
	"OrchardCore_PoweredBy": {
	  "Enabled": true,
	  "HeaderName": "X-Powered-By",
	  "HeaderValue": "OrchardCore"
	}
  }
}
```

!!! note
    The `X-Powered-By` HTTP header is added by default and can be disabled by setting the `Enabled` property to `false`.
