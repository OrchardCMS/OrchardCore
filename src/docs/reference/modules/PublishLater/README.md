# Publish Later (`OrchardCore.PublishLater`)

The Publish Later module schedules a draft content item to be published at a specific date and time.

## Enable and configure the module

1. Enable the **Publish Later** feature.
2. In **Content** → **Content Definition** → **Content Types**, edit the content type that should support scheduling.
3. Add the **Publish Later Part** to the type and save the definition.

The part is attachable and can be added to any content type. It stores the schedule on the content item as `PublishLaterPart.ScheduledPublishUtc`.

## Schedule publication

When editing an item that has the part:

1. Enter a date and time in the **Publish Later** control in the editor actions.
2. Select **Publish Later**.

This saves the latest version as a draft with its schedule; it does not publish the item immediately. For an item that already has a published version, that version remains live until the scheduled draft is published.

The content item list also shows the scheduled publication time for items with an active schedule.

To change the schedule, enter another value and save the draft. To remove it, clear the date and save, or select **Cancel Publish Later**.

## Permissions

The scheduling controls are available only when the current user is authorized for the standard `PublishContent` permission on the content item. Orchard Core automatically applies owner-specific and content-type-specific permission variants where appropriate. The module does not define a separate scheduling permission.

## Date, time, and time zones

The editor accepts a local date and time and converts it to UTC before storing it in `ScheduledPublishUtc`. When the editor displays an existing schedule, it converts the stored UTC value back to the local time zone for the current request.

The local time zone is selected through `ILocalClock`. A configured user time zone takes precedence when the **User Time Zone** feature is enabled; otherwise, the site time zone is used. If neither provides a time zone, Orchard Core falls back to the server's system time zone.

!!! warning
    Local times skipped or repeated by a daylight-saving transition are converted strictly. Choose an unambiguous local time; otherwise, the conversion fails and the schedule is not saved.

The editor does not require the value to be in the future. A time already in the past becomes eligible the next time the background task runs.

## Background publication

The **Scheduled Content Items Publisher** background task is enabled by default and uses the cron schedule `* * * * *`, which checks for due items every minute. Each run:

1. Queries scheduled versions that are both the latest version and unpublished.
2. Selects schedules earlier than the current UTC time.
3. Clears `ScheduledPublishUtc`.
4. Publishes each content item through `IContentManager.PublishAsync`.

Publication can therefore happen after, but not before, the requested time. The exact delay depends on the task schedule, host availability, and processing time.

Because the task uses the normal content publication API, content handlers, workflows, indexing, notifications, and other publication integrations run as they do for an editor-triggered publication.

Enable the [Background Tasks](../BackgroundTasks/README.md) feature to manage the task from the admin. Disabling the task, stopping the application, or leaving a tenant unavailable keeps due items as drafts until a later run can process them.

!!! note
    The task processes all due items without a defined ordering or batch limit. If publishing an item throws an exception, the background-task infrastructure logs the error and the current run stops before later items are processed. A subsequent run queries eligible items again.

## Programmatic scheduling

The module does not expose a separate HTTP API. Custom modules can schedule a draft through the regular content APIs by setting `PublishLaterPart.ScheduledPublishUtc` to a UTC value and saving the draft:

```csharp
draft.Alter<PublishLaterPart>(part =>
    part.ScheduledPublishUtc = scheduledUtc);

await contentManager.SaveDraftAsync(draft);
```

The content type must contain `PublishLaterPart`, `draft` must be the latest unpublished version, and `scheduledUtc` must represent UTC. Set the property to `null` to cancel the schedule.

Code that accepts a local value can use `ILocalClock.ConvertToUtcAsync` before assigning the property. Custom endpoints must perform their own authorization; the `PublishContent` check belongs to the built-in display driver, not to `PublishLaterPart` itself.

The built-in display driver exposes the `PublishLaterPart_Edit` editor shape in the `Actions` zone and the `PublishLaterPart_SummaryAdmin` shape in the admin summary `Meta` zone. Use placement or shape overrides to customize their rendering.

## Edge behavior

- Only the latest unpublished version is indexed for scheduled publication. Manually publishing that version before its scheduled time removes it from the pending set.
- Removing `PublishLaterPart` from a content type prevents stale part data from remaining indexed after the item is next created or updated.
- The task compares against the current UTC time. Changing a user's or site's time zone later changes how the stored instant is displayed, not the instant itself.
- Publication can still be canceled by normal content handlers. The task uses the boolean-returning `PublishAsync` API but does not add separate handling when a handler cancels publication.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/E7UH8R14EUA" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
