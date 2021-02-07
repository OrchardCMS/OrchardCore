# Placements (`OrchardCore.Placements`)

The placements module allows editors to add custom placement logic.

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
Placements rules is a JSON array, similar to a `placement.json` file entry, as defined in the [Placement documentation](../../core/Placement/README.md#format).
