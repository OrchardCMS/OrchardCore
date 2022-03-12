# Audit Trail (`OrchardCore.AuditTrail`)

The Audit Trail module provides an immutable (for users, even administrators but not for developers), auditable log of certain changes and events in the system. This includes e.g. creation or deletion of content items, and events like user login failures. For content items, previous versions and deleted items can be restored, and changes can be tracked.

## General Concepts

Audit Trail events are created if a certain supported event happened in the system. While there are several built-in event handlers for the most important events, this is extensible and module authors can provide their own ones.

Once you've enabled the module, you will see a new menu item in the admin UI called *Audit Trail*. The list there shows you the logged Audit Trail events.

## Using the Audit Trail event list

The Audit Trail event list provides you with filtering and pagination to be able to find the Audit Trail events that you are looking for easily. By default, you can filter by the following criteria:

- A given date range,
- categories (e.g. *All categories*, *Content*, *User*).

Also, you can sort entries by various parameters.

Events provide various details on their line:

- The category and type of event (e.g. *Content* and *Published*).
- The time when the event was recorded.
- The user initiating the event.
- An event-specific quick summary, if any. E.g. events of the *Content* category show you the version and title of the content item. If you click on the *Version X* link you can see the read-only editor of the content item filled with the values that the content item has at that version. If you click on the display text of the content item, you can edit the latest version of the content item.
- Event-specific buttons, if any. E.g. events of the *Content* category will display a *View* button that you can use to view the content item at the given version, and a *Restore* button that you can use to restore the content item to the version even if the item was deleted (the restored item will be created as a draft version that you then need to publish).
- A *Details* link. Here you can get more general information about the given event, as well as event handlers can provide custom information. E.g. if you check out the *Detail* view of a *Content* event you can also see the textual differences between the current version of the content item and the previous version under the *Diff* tab. The values of the current version will be shown in green, and the values of the previous version will be shown in red.

## Audit Trail Settings 

If you navigate to Configuration -> Settings -> Audit Trail, you will see various configuration options, depending on the enabled Audit Trail features:

- A list that contains every event that you can record using the Audit Trail module. Here you can enable or disable the recording of the given events.
- You can enable client IP address logging. When you enable this, the client IP address will be recorded in Audit Trail events. Note that depending on the legislation your site operates in, you need to take special care to collect and store such Personal Identifiable Information.
- The *Trimming* settings are about configuring how long you would like to keep the Audit Trail events in the database. You can disable trimming if you would like to keep the events indefinitely.
- Further tabs can be added. E.g. the *Content* tab allows you to select items of which content types you want to record Audit Trail events for.

## Audit Trail Part

You can attach the `AuditTrailPart` content part to your content type. This will allow content editors to enter a comment to be saved into the Audit Trail event when saving a content item. This will then be visible in the Audit Trail event list.

## Recording Custom Events

Orchard Core is built to be extended, and the Audit Trail module is no different. So, when creating your own module, you can log your events with Audit Trail too. Check out the source of the `OrchardCore.Users` or `OrchardCore.Contents` modules for examples.

## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/ko0jEgQtXYc" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/t28rnjYtlJc" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## CREDITS

### jsdiff

<https://github.com/kpdecker/jsdiff>  
Copyright (c) 2009-2015, Kevin Decker, <kpdecker@gmail.com>  
License: Software License Agreement (BSD License)