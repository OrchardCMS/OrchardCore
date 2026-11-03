# Content Preview (`OrchardCore.ContentPreview`)

The Content Preview module adds a live preview to the content editor. It renders the current
form values in a separate window without saving or publishing the content item.

## Enable Content Preview

Enable the **Content Preview** feature from **Configuration** > **Features**. The feature depends on the Contents module.

The **Preview** button appears in the action area of a content item editor when the current user
is authorized to preview that item. Placement rules can hide the button for specific content
types; Orchard Core does this for menu items and dashboard widgets by default.

## Choose the preview workflow

Orchard Core provides two related preview workflows:

| Workflow | Where to start | Content that is rendered |
| --- | --- | --- |
| Preview a saved draft | Select **Preview Draft** for an item in the content items list | The latest version already stored by the Contents module |
| Live preview | Select **Preview** while creating or editing an item | The current, unsaved values in the editor form |

Live preview opens a separate window. Keep the content editor open while using it. Changes from
Supported editor controls update the preview after a short delay.

## Render the frontend page

By default, live preview builds the content item's `Detail` display shape and places it in a
minimal HTML layout owned by the Content Preview module. This fallback is useful for shape
content, but it doesn't load the active frontend theme's layout, resource zones, styles, or
scripts.

To preview the complete frontend page:

1. Go to **Content** > **Content Definition** > **Content Types**.
2. Edit the content type.
3. Add `PreviewPart`.
4. Set **Pattern** to a Liquid expression that returns the tenant-relative path of the frontend
   route that displays the item.

For a routable content type, the following pattern commonly uses its display URL:

```liquid
{{ ContentItem | display_url }}
```

The pattern has access to `ContentItem`. It must resolve to a local path, such as
`/blog/my-post`, not an absolute URL on another host. If the result doesn't start with `/`, the
module adds it.

When a preview path is configured, Orchard Core runs that frontend route through the tenant's
normal request pipeline. The route must:

- Resolve the content item through Orchard Core's content management APIs.
- Render a complete response through the intended frontend theme and layout.
- Be reachable in the same tenant as the content editor.

The preview draft is available from the request's content manager session. A route hosted by a
separate application or one that bypasses Orchard Core's content manager can't load it.

## How live preview works

The editor and preview windows communicate on the same origin
[`BroadcastChannel`](https://developer.mozilla.org/docs/Web/API/BroadcastChannel) identified by
the content item ID.

For each update, the module:

1. Serializes the complete editor form.
2. Creates a temporary content item and runs the editor update and validation pipeline.
3. Marks the temporary item as published for rendering, without saving or publishing it.
4. Stores the item in `IDistributedCache` under an opaque token.
5. Sends an authorized display URL containing that token to the preview window.
6. Loads the response into a staging iframe and swaps it into view after the response finishes.

Updates are debounced for 500 milliseconds. A newer update cancels an in-flight draft request,
and the same cache token is reused while the editor remains connected. The two-iframe swap keeps
the previous response visible while the next one loads and preserves the vertical scroll
position.

The cache entry has a five-minute sliding expiration. The module doesn't create a
Content Preview-specific cookie. The normal tenant authentication mechanism, which can use a
cookie, still applies to every draft and display request.

## Support live updates in an editor

Live updates aren't based on a fixed list of field types. An editor participates by using one of
the module's event hooks:

| Editor behavior | Integration |
| --- | --- |
| Text-like control | Add the `content-preview-text` CSS class. Input changes trigger an update. |
| Select, checkbox, toggle, or similar control | Add the `content-preview-select` CSS class. Change events trigger an update. |
| Rich, structured, or JavaScript editor | Trigger `contentpreview:render` on `document` when its value changes. |

For example:

```html
<input asp-for="Value" class="form-control content-preview-text" />
```

```javascript
$(document).trigger("contentpreview:render");
```

Many built-in editors already use these hooks. A custom or third-party editor that updates only
its own JavaScript state and doesn't trigger one of them isn't live-preview aware. Ensure that
the editor also writes its value to a successful form control before triggering the event,
because the module submits the serialized editor form.

## Customize the preview URL

`PreviewPart` supplies a `PreviewAspect` from its Liquid pattern. A custom content handler can
provide the same aspect when the path must be calculated in code:

```csharp
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

public sealed class ProductPreviewHandler : ContentHandlerBase
{
    public override Task GetContentItemAspectAsync(ContentItemAspectContext context)
    {
        return context.ForAsync<PreviewAspect>(aspect =>
        {
            aspect.PreviewUrl = $"/products/{context.ContentItem.ContentItemId}";

            return Task.CompletedTask;
        });
    }
}
```

Register the handler with the tenant's services:

```csharp
services.AddScoped<IContentHandler, ProductPreviewHandler>();
```

Only set `PreviewUrl` to a tenant-relative frontend path. The Content Preview middleware
re-executes the request pipeline for that path after restoring the temporary item.

## Detect preview requests

Draft creation and display requests expose `ContentPreviewFeature` through ASP.NET Core request
features:

```csharp
var isPreview = httpContext.Features
    .Get<ContentPreviewFeature>()?
    .Previewing == true;
```

Use this signal when an editor, handler, or service would otherwise produce a persistent side
effect while processing the temporary item. Orchard Core uses it to avoid indexing preview
items and moving attached media files during preview.

## Content lifecycle and validation

A live preview item exists only in the draft request, distributed cache, and display request. It
isn't saved as a content item version and doesn't change the published item. Saving, publishing,
unpublishing, or discarding the item remains an explicit editor action.

The module runs the normal content editor update pipeline against the temporary item. If editor
validation fails, the draft endpoint returns a validation response and the preview window keeps
the last successfully rendered version.

Custom update handlers still run during preview. Avoid writes, queued work, external calls, or
other irreversible effects when `ContentPreviewFeature` is active.

## Permissions and security

The preview button uses resource authorization for `Preview content`, including applicable
owner and content type permission variations. The live draft and display endpoints also require
the `Preview content` permission. Assign that permission to roles that must use live preview.

The cache token is an opaque identifier, not an authorization credential. Knowing it doesn't
bypass the permission check or tenant authentication. Even so, don't share preview URLs: they
identify temporary, potentially unpublished content and can appear in browser history or
request logs.

In a multi-node deployment, configure `IDistributedCache` so every node can read the same cache
entries. Otherwise, a draft created on one node can return `404 Not Found` when its display
request reaches another node.

## Limitations

- Live preview requires browser support for `BroadcastChannel`.
- The editor and preview window must remain on the same origin.
- A custom editor needs one of the live-update hooks described above.
- Full frontend layout and resources require a valid preview path from `PreviewPart` or a custom
  `PreviewAspect` provider.
- Preview paths can target only routes in the current Orchard Core tenant, not an external
  headless frontend.
- Temporary drafts expire after five minutes without a successful cache access.
- Preview renders editor input; it doesn't simulate a future publish or unpublish schedule.

## Troubleshoot live preview

### The Preview button is missing

Confirm that the Content Preview feature is enabled, the user has permission to preview the
item, and placement rules don't hide the `ContentPreview_Button` shape for the content type.

### The window opens but doesn't update

Keep the editor window open. Confirm that the browser supports `BroadcastChannel` and that the
custom editor uses `content-preview-text`, `content-preview-select`, or triggers
`contentpreview:render`.

Also confirm that the value is included when the editor form is serialized. Disabled controls
and values held only in JavaScript aren't submitted.

### The preview is unstyled or misses the site layout

The module is using its minimal fallback layout. Attach `PreviewPart` to the content type and set its pattern to a frontend route that renders the item through the active theme.

### The preview returns 404

The token can be missing or expired, or a different application node might not have the cache
entry. Reopen the preview to create a current draft. For multi-node deployments, verify the shared distributed cache configuration.
Also verify that the `PreviewPart` pattern resolves to an existing tenant route and that the route loads the item through Orchard Core's content manager.

### The preview shows an older value

The latest editor values didn't pass validation or the editor didn't raise a live-update event.
Correct validation errors and verify the editor integration. The window continues to display the last successful response until another draft renders.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/NDUjn5_KdEM" title="Content Preview demonstration" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Credits

### JavaScript Cookie

<https://github.com/js-cookie/js-cookie>

Copyright 2006, 2015 Klaus Hartl and Fagner Brack. Released under the MIT license.
