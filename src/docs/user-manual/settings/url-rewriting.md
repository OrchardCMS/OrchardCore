---
title: URL rewriting
description: Send visitors from old addresses to new ones with redirect rules, serve a page under another address with rewrite rules, and set the order of the rules.
technical_manual:
  - reference/modules/UrlRewriting/README.md
---

# URL rewriting

URL rewriting rules change what happens when a visitor asks for an address. Use them when you move or rename pages, so old links and search results keep working, or to show a page under a shorter or friendlier address.

There are two kinds of rules:

- A **Redirect** rule sends the visitor's browser to another address. The address bar changes. Use it for moved pages: search engines then update their links.
- A **Rewrite** rule serves another address's content without telling the browser. The address bar doesn't change.

| | |
| --- | --- |
| **Menu** | **Tools > URL Rewriting** |
| **Permission** | *Manage URLs rewriting rules* (the Administrator role has it by default) |
| **Feature** | **URL Rewriting** (`OrchardCore.UrlRewriting`) |


!!! tip "Changing the address of a single content item?"
    To give a page a new address, change its permalink instead; see [URLs and the home page](../content/urls-and-homepage.md). Use URL rewriting for addresses that no longer belong to a content item, or for whole groups of addresses.

## How patterns work

Rules match addresses with **regular expressions**, a small pattern language. The address being matched is the path, starting with `/`, without the domain. A few building blocks are enough for most rules:

| Pattern | Matches |
| --- | --- |
| `^/about-us$` | Exactly `/about-us`. `^` means "starts here" and `$` means "ends here". |
| `^/blog/(.*)$` | Every address that starts with `/blog/`. The part in brackets is captured. |
| `$1`, `$2` | Used in the **Substitution URL Pattern**: the text captured by the first, second, ... pair of brackets. |

For example, a rule with the **Match URL Pattern** `^/img/(.*)$` and the **Substitution URL Pattern** `/media/$1` sends `/img/logo.png` to `/media/logo.png`.

## Add a redirect rule

1. Open **Tools > URL Rewriting** and click **Add Rule**.
2. In **Available Rule Types**, click **Add** next to **Redirect**.
3. Fill in the form (see the table below).
4. Click **Save**.

You see *Rule created successfully.* and the site reloads so the rule applies.

| Field | What it does |
| --- | --- |
| **Display Name** | Required. A name that helps you find the rule in the list. |
| **Match URL Pattern** | Required. The regular expression an address must match, for example `^/old-page$`. It must be a valid pattern. |
| **Case Insensitive** | Matches the pattern whatever the case of the letters, so `/Old-Page` matches too. |
| **Query String Policy** | **Append Query String from Original Request** keeps the part of the address after `?`, for example `?page=2`. **Exclude Query String from Original Request** drops it. |
| **Redirect Type** | **Found (302)** and **Temporary Redirect (307)** for a temporary move. **Moved Permanently (301)** and **Permanent Redirect (308)** for a permanent move: browsers and search engines remember it. |
| **The Substitution URL Pattern** | Required. The address to send visitors to, such as `/new-page` or a full address on another site, with `$1`, `$2` for the captured parts. |

!!! warning "Permanent redirects stick"
    Browsers remember a permanent redirect (301 or 308) and may keep following it even after you delete the rule. Use **Found (302)** while you try a rule, and switch to a permanent type once it is right.

## Add a rewrite rule

1. Open **Tools > URL Rewriting** and click **Add Rule**.
2. In **Available Rule Types**, click **Add** next to **Rewrite**.
3. Fill in the form.
4. Click **Save**.

The fields are the same as for a redirect rule, without **Redirect Type**, and with one more:

| Field | What it does |
| --- | --- |
| **Stop Evaluation of Subsequent Rules** | When this rule matches, the rules below it in the list are ignored. |

## Set the order of the rules

The site checks the rules from the top of the list to the bottom, so the order matters when several rules could match the same address. Put the most specific rules first.

1. Open **Tools > URL Rewriting**.
2. Drag a rule by its handle (*Drag to reorder*) to its new place.

The new order is saved as soon as you drop the rule. If it fails, you see *Unable to sort the list. Try refreshing your page and try again.*

## Edit or delete rules

The list shows each rule's name, its **Source** (**Redirect** or **Rewrite**), its author and when it was last modified. Type in **Search** to find a rule by name.

- Click **Edit** to change a rule, then **Save**. You see *Rule updated successfully.*
- Click **Delete** and confirm to remove a rule. You see *Rule deleted successfully.*
- To delete several rules, check them, then pick **Delete** in the **Actions** menu above the list.

Saving or deleting a rule reloads the site.

## Check that a rule works

There is no test tool on the screen. After you save a rule, open a private browser window and type an address the rule should match. A redirect rule changes the address in the address bar; a rewrite rule shows the other page's content under the same address. If nothing happens, check the pattern and the order of the rules.
