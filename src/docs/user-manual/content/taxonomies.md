---
title: Taxonomies
description: Create taxonomies such as categories and tags, organize their terms in a tree, let editors pick terms on content items, and filter the content list by term.
technical_manual:
  - reference/modules/Taxonomies/README.md
  - topics/content-relationships/README.md
---

# Taxonomies

A **taxonomy** is a list of words used to classify content: *Categories* (*News*, *Events*, *Tutorials*), *Tags* (*space*, *earth*, *exploration*), *Regions*, *Departments*. Each word is a **term**, and terms can have child terms to form a tree (*Europe > France > Paris*). Editors then pick terms on their content items with a **Taxonomy Field**, and each term gets a page on the site that lists the content items classified with it.

| | |
| --- | --- |
| **Menu** | **Content > Content Items** (taxonomies are content items of the **Taxonomy** type), **Settings > Taxonomy Filters** |
| **Permission** | *Manage taxonomies* (add, edit, delete terms; settings) and the usual content permissions on the **Taxonomy** type |
| **Feature** | **Taxonomies** (`OrchardCore.Taxonomies`), **Taxonomies Contents List Filters** (`OrchardCore.Taxonomies.ContentsAdminList`) |


A site set up with the **Blog** recipe already has two taxonomies, **Tags** and **Categories**, used by the **Tags** and **Category** fields of blog posts. On that site, **Content > Taxonomy** lists them.

## Prepare a term content type

Each term is a small content item. Before you create a taxonomy, you need a content type for its terms (the Blog recipe has **Tag** and **Category**).

1. Open **Design > Content Definition > Content Types** and click **Create new type**, for example *Region*.
2. Add the **Title** part, and the **Autoroute** part if each term should have its own page on the site.
3. On the type, untick **Creatable** and **Listable**: terms are created only inside their taxonomy.
4. Click **Save**.

To give each term a page, open the **Autoroute** part settings on the term type and tick **Manage contained item routes**. See [Content types](content-types.md) and [URLs and homepage](urls-and-homepage.md).

## Create a taxonomy

1. Open **Content > Content Items**, click **New** (or the create button) and choose **Taxonomy**.
2. Type the **Title**, for example *Regions*.
3. Pick the **Term Content Type**, for example **Region**. This field is required. Only content types without a stereotype are offered.
4. Tick **Route contained items** if the terms should have their own pages (the term type also needs **Autoroute**).
5. Click **Publish**.

The taxonomy also has an **Alias** and a **Permalink**, both generated from the title when you leave them empty.

!!! note
    You can add terms only after the taxonomy is saved once. Until then the **Add ...** button doesn't show.

## Add terms

1. Open the taxonomy's editor (**Content > Content Items**, then **Edit** on the taxonomy).
2. Click **Add ...** (for example **Add Region**) above the list of terms.
3. Fill in the term's editor (**New ...**), at least its **Title**, and click **Publish**.
4. You are back in the taxonomy editor. Click **Publish** on the taxonomy to make the new terms live.

To add a child term, click **Add** on the row of the parent term instead.

!!! warning "Publish the taxonomy after changing terms"
    The **Taxonomy** type is draftable: adding, editing or deleting a term changes a draft of the taxonomy. The Taxonomy fields in editors already offer the draft terms, but the site keeps showing the published taxonomy until you click **Publish** on it.

## Edit, move and delete terms

Each term row has three buttons:

- **Edit** opens the term's editor.
- **Add** adds a child term under it.
- **Delete** removes the term and its child terms, after you confirm.

To reorder terms or change their parent, drag a term up, down, or to the right under another term in the tree. Then click **Publish** (or **Save Draft**) on the taxonomy. If you leave the page without saving, the screen warns you that you reordered the taxonomy.

## Add a Taxonomy field to a content type

1. Open **Design > Content Definition > Content Types** and click **Edit** on the type, for example **Article**.
2. Click **Add Field**, type a **Display Name** (for example *Region*), pick **Taxonomy Field** and click **Save & Configure**.
3. Fill in the settings below and click **Save**.

| Setting | What it does |
| --- | --- |
| **Placeholder** | The text shown in an empty tags box. |
| **Hint** | Help text shown under the field in the editor. |
| **Taxonomy** | The taxonomy whose terms editors pick from. |
| **Required** | At least one term must be selected. |
| **Unique** | Only one term can be selected (radio buttons instead of check boxes). |
| **Leaves only** | Only terms without child terms can be selected. |
| **What type of editor should be used?** | **Standard** shows the whole tree with a check box (or radio button) per term. **Tags** shows a search box where editors type to find terms. |
| **Open** | Only for the **Tags** editor. Editors can create a tag by typing it and pressing Enter. Only the tag's title is set. |
| **What type of display mode should be used?** | **Standard** or **Tags** (the selected terms shown as tags on the site). |

The Blog recipe uses the **Tags** editor for the **Tags** field and **Standard** with **Unique** and **Leaves only** for the **Category** field.

## Pick terms on a content item

- With the **Standard** editor, tick the terms you want (or pick one radio button when only one is allowed). Grayed-out terms are parents that can't be picked because only leaves are allowed.
- With the **Tags** editor, start typing in the box (*Type to search*) and pick a term from the list. Click a tag's remove button to take it off. When the field is **Open**, type a new word and press Enter to create it (*Press enter to create a tag*).

A tag created this way is added at the top level of the taxonomy, and the taxonomy is published for you so the new tag can be used straight away. Creating tags needs the *Manage taxonomies* permission.

If the field shows *The taxonomy could not be found. Please check the settings for this field.*, the taxonomy picked in the field settings was deleted. Ask your site builder to choose another one.

## Term pages on the site

When the taxonomy has **Route contained items** ticked and the term type has an **Autoroute** part with **Manage contained item routes**, each term has its own address, for example `/categories/news`. The term's page lists the published content items classified with that term, a page at a time.

## Filter the content list by term

With the **Taxonomies Contents List Filters** feature on, you can filter **Content > Content Items** by term.

1. Open **Settings > Taxonomy Filters**.
2. Under **Taxonomies**, tick the taxonomies to offer as filters, for example **Tags** and **Categories**.
3. Click **Save**.

The content items list now has one drop-down per taxonomy. Pick a term to show the items classified with it, **Show all** to show every item that has any term of that taxonomy, or **Clear filter** to remove the filter. When you filter the list by a content type, only the taxonomies used by a field of that type are offered.

!!! example "See it in practice"
    [Build a blog](../use-cases/build-a-blog.md) walks through this step by step.
