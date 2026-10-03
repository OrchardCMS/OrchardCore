# Archive Later (`OrchardCore.ArchiveLater`)

The `OrchardCore.ArchiveLater` module adds the ability to schedule a published content item to be unpublished (archived) automatically at a future date and time.

## Usage

1. Enable the `Archive Later` feature.
2. Attach the **Archive Later Part** to any content type that should support scheduled archiving (in the content type definition).
3. When editing a published item of that type, set the scheduled archive date and time in the editor. The value is entered in the site/local time zone and stored as UTC.
4. To cancel a scheduled archive, clear the date or use the cancel option in the editor before the scheduled time.

## How it works

The part stores a single value, `ScheduledArchiveUtc`, on the content item via the `ArchiveLaterPart`.

A background task (`Content Items Archiver`) runs every minute. It finds published items whose scheduled archive time has passed, clears their schedule, and unpublishes them. Because it relies on a background task, the scheduled archiving only runs while the tenant is active.

!!! note
    Archiving here means the item is **unpublished** — it is removed from the published frontend while its latest draft is preserved.

See also [Publish Later](../PublishLater/README.md) for scheduling the opposite operation.
