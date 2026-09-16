# Placements (`OrchardCore.Placements`)

The placements module allows editors to add custom placement logic.

## Management API

The [Placements API](../../api/placements/README.md) exposes stored shape rules, validation and
filter discovery with generated `pomi placements` commands and MCP tools. It shares the admin
editor's manager and selected storage provider.

## General concepts

Custom placements can be provided by themes and modules using a `placement.json` file.

The placements module provides a way to define placements from the admin site.

### Placement precedence

Placements defined in placements module override placements defined by theme and modules.

### Placements storage

Placements defined with this module are stored in the database by default.

You can also choose to store placements in a file by enabling 'Placements file storage' (`OrchardCore.Placements.FileStorage`) feature.

### Shape placements format

Placements are defined by shape name.
For each shape you can define a set of placements rules.
Placement rules are a JSON array, similar to a `placement.json` file entry, as defined in the [Placement documentation](../Placement/README.md#format).

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/YR8QzyAEgo4" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
