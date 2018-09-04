# WebHooks (OrchardCore.WebHooks)

Webhooks provide a way for external services to receive information about certain events as soon as they happen in your Orchard site. They allow you to extend and integrate your site with other applications around the web.

When the specified events happen we’ll send a POST request to each of the URLs you have configured. This allows external sites to be notified in near real-time when events such as a new blog post are published within your site.

# Supported Events
Right now webhooks can be registered for the following events:

| Model          | Event                      | Trigger                                 |
|----------------|----------------------------|-----------------------------------------|
| Content        | content.created            | When any content item is created.       |
|                | content.updated            | When any content item is updated.       |
|                | content.removed            | When any content item is removed.       |
|                | content.published          | When any content item is published.     |
|                | content.unpublished        | When any content item is unpublished.   |
| {Content Type} | {content type}.created     | When any {content type} is created.     |
|                | {content type}.updated     | When any {content type} is updated.     |
|                | {content type}.removed     | When any {content type} is removed.     |
|                | {content type}.published   | When any {content type} is published.   |
|                | {content type}.unpublished | When any {content type} is unpublished. |

# Payloads
By default the payload is the JSON representation of the content item object including all parts and fields. To customize the default payload you can provide a Liquid template that defines the exact format the service URL your webhook calls is expecting.

For example if integrating with Twilio to send an SMS when content is published, you can specify the JSON payload that their REST API expects.
```liquid
{
    "From": "+12345678910",
    "To": "234234234",
    "Body": "{{ User.Identity.Name }} published a new {{ Model.ContentItem.ContentType }} at: {{Site.BaseUrl}}{{ Model.ContentItem | display_url }}"
}
```
# Headers
### Default Headers
The webhook is sent with a few default headers that help identify and verify the webhook. You can also add custom headers yourself through the UI.

| Header              | Description                                                   |
|---------------------|---------------------------------------------------------------|
| X-Orchard-Id        | The Id of the webhook object that triggered the notification. |
| X-Orchard-Event     | The event name that triggered the notification.               |
| X-Orchard-Tenant    | The tenant name.                                              |
| X-Orchard-Signature | The SHA 256 signature of the request body.                    |

The `X-Orchard-Signature` header can be used to validate the authenticity of the webhook notification. The key for the signature is the `Secret` field specified when creating the webhook.

### Form URL Encoded
Sometimes remote services expect a media type of `application/x-www-form-urlencoded` instead of the more common JSON formatted request of `application/json`.

In these cases you can configure your webhook request in the `Headers` section with a form url encoded content type.

For example the above Twilio JSON payload would be sent as below when using the `application/x-www-form-urlencoded` content type.
```
From=%2B12345678910&To=234234234&Body=admin+published+a+new+Menu+at%3A+http%3A%2F%2Fexample.org%2FContents%2FContentItems%2F4jgk1vbk85ases2jpsvh9mmvnf
```