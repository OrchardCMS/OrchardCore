# Media permissions — analysis

Purpose: understand the media permission set well enough to decide what to rename, what to add, and
what to remove, before any further implementation. Companion to `media-permissions-model-plan.md`,
whose Phase 2 onward should be re-derived from the conclusions here.

Everything below is read off the code on `skrypt/media-permissions-model`, which carries the Phase 0
and Phase 1 changes on top of `skrypt/media-auth-cost-fix`.

## 1. The list mixes three unrelated things

The Roles editor shows one flat list, which is the first reason it cannot be reasoned about. There are
actually three groups:

**A. Admin surfaces** — ordinary feature permissions, unrelated to media paths. No implications, no
handlers, nothing subtle:

`ManageMediaProfiles`, `ViewMediaOptions`, `ManageMediaApiSettings`, `ManageAssetCache`

**B. The entry gate** — one permission, checked with no resource before anything else:

`ManageMediaContent`

**C. The path model** — everything else. This is where the complexity lives:

`ManageMediaFolder`, `ManageOwnMediaContent`, `ManageOthersMediaContent`,
`ManageAttachedMediaFieldsFolder`, `ViewMediaContent`, `ViewRootMediaContent`,
`ViewOwnMediaContent`, `ViewOthersMediaContent`, `ViewMediaContent_{folder}`

Group A is fine and should be left alone. Group B needs a name. Group C is the subject of the rest of
this document.

## 2. The path model is a matrix, sampled unevenly

Two handlers discriminate a set of **scopes**, and callers ask about one of two **operations** (read a
path, or act on it). The permission set is an uneven sample of that grid.

| Scope | Read requires | Write requires |
| --- | --- | --- |
| Everything | `ViewMediaContent` | `ManageMediaFolder` |
| Media root — its own files | `ViewRootMediaContent` | entry gate **+** the root read |
| Media root — traversal only | root read, **or** any folder / own / others read | n/a |
| A named first-level folder, and below | `ViewMediaContent_{folder}` | entry gate **+** that folder's read |
| `_users` (the shared parent) | `ViewOwnMediaContent` | `ManageOwnMediaContent` **+** the read |
| Own user folder | `ViewOwnMediaContent` | `ManageOwnMediaContent` **+** the read |
| Another user's folder | `ViewOthersMediaContent` | `ManageOthersMediaContent` **+** the read |
| `mediafields/` and below | no permission of its own | `ManageAttachedMediaFieldsFolder` **+** the read |
| `mediafields/temp/{own}` | `ViewOwnMediaContent` | `ManageAttachedMediaFieldsFolder` **+** the read |
| `mediafields/{contentItemId}` | Contents `ViewContent` on that item | `ManageAttachedMediaFieldsFolder` **+** that |

"Write requires X + the read" is not editorial shorthand: `ManageMediaFolderAuthorizationHandler`
literally checks its mapped manage permission and then, with Secure Media on, asks the read question
for the same path.

Read that table column by column and the shape of the problem appears. **The read column carries all
the granularity. The write column has four permissions for ten scopes, and three of them are the same
"manage" permission repeated.**

## 3. Findings

### F1 — The Manage axis has no folder granularity at all

`SecureMediaPermissions` builds dynamic per-folder permissions from exactly one template:

```csharp
private static readonly ReadOnlyDictionary<string, Permission> _permissionTemplates = new(new Dictionary<string, Permission>()
{
    { MediaPermissions.ViewMedia.Name, _viewMediaTemplate },
});
```

There is no `ManageMediaContent_{folder}` anywhere in the module. For any path that is not under
`_users` or `mediafields`, the manage handler maps to plain `ManageMediaContent` — the entry gate,
which every role that can open the library already holds.

So "let this role write to folder Alpha only" cannot be expressed on the write axis. It happens to
work, but only as a side effect of F2.

### F2 — Read-only folder access is not expressible

Because write access to a named folder is *the entry gate plus that folder's read permission*, and
the entry gate is held by everyone who can open the library, granting `ViewMediaContent_Alpha`
also grants **write** access to Alpha.

Withholding the entry gate is not an escape: every endpoint's first check is `ManageMediaContent`
with no resource, so a role without it cannot open the library at all.

There is therefore no way to say "this role may look at Alpha but not change it". For a permission
whose label is literally *"View media content in folder 'Alpha'"*, that is the most surprising thing
in the whole model.

### F3 — Manage does not imply View, so manage grants alone are inert

The two trees meet only at `ManageMediaFolder`, the top of both:

```csharp
ViewOthersMedia = new("ViewOthersMediaContent", "…", [ManageMediaFolder]);
ViewOwnMedia    = new("ViewOwnMediaContent",    "…", [ViewOthersMedia]);
```

`ManageOwnMediaContent` is not in `ViewOwnMediaContent`'s granting set. Since the manage handler
requires the read question to pass, `ManageOwnMediaContent` on its own grants **nothing** — it is a
checkbox that does nothing until its view counterpart is also ticked. Same for
`ManageOthersMediaContent` and `ManageAttachedMediaFieldsFolder`.

Every "own folders only" recipe therefore needs two checkboxes where one would do.

### F4 — The inversion blocks the obvious fix for F3

The natural repair is "manage implies view". It works for the own/others pairs, which describe the
same scope on both axes. It cannot work for the pair that matters most: `ViewMediaContent` must not be
implied by `ManageMediaContent`, because `ManageMediaContent` is the **weakest** permission in the
tree — `ManageOwnMediaContent` implies it — so that link would turn every own-media role into
view-every-folder and destroy folder scoping entirely.

The entry gate being at the bottom of the manage tree means nothing can ever be hung off it. This is
the structural reason the model cannot be tidied by adding implications alone.

### F5 — Two identifiers are both a grant and a question

`ManageMediaFolder` and `ViewMediaContent` each mean one thing when granted to a role ("everything,
everywhere") and something unrelated when passed to `AuthorizeAsync` with a path resource ("may this
user act on / read this path?"). 23 call sites use the second meaning. The answer is always delegated
to a different permission, so the name in the call is never the name that decides.

`ManageMediaFolder` additionally describes neither meaning: it is not about a folder, and as a grant
it is not scoped to one.

### F6 — Traversal and content are split on the read side only

Phase 0 separated "reach the root in order to descend" from "read what is stored in the root". The
write side still asks the single old question, so **the same call answers "may I open the library?"
and "may I write into the root?"**

Live consequence on this branch: after Phase 1 an own-media-only role passes the root question, so
`UploadMediaEndpoint` accepts a root destination. Such a role can upload into the media root while
being unable to see anything there. `CreateFolderEndpoint` is unaffected — a new root folder resolves
to `ViewMediaContent_{name}`, which the role lacks. `Move`, `Copy`, and `Delete` need the same tracing.

This regression is in `afbda01801`, which is **not pushed**. Nothing to fix in the wild, but it should
be closed before the branch goes out.

### F7 — The special folders are asymmetric

`mediafields` has a dedicated manage permission (`ManageAttachedMediaFieldsFolder`) and no view
counterpart: reads under it are governed by `ViewOwnMediaContent`/`ViewOthersMediaContent` for
`temp/{user}`, and by the Contents `ViewContent` permission of the owning content item otherwise.
Delegating to content permissions is the right call — it is the only place in the model where media
access follows the thing the media belongs to — but it means the two axes do not describe the same
scopes, so no symmetric renaming scheme can cover both.

### F8 — `_users` itself counts as "own"

The shared parent `_users` maps to `ViewOwnMediaContent`/`ManageOwnMediaContent`, not to "others".
The code says why: *"We need to allow the `_Users` folder for own media access too. If someone uploads
into this folder, we are screwed."* It works, but "own" covering a folder that is nobody's is a wart
worth naming.

### F9 — Naming assessment

| Current | Verdict | Proposed |
| --- | --- | --- |
| `ManageMediaFolder` | Wrong on both counts: not a folder, not scoped. Doubles as a question (F5). | `ManageAllMedia`, question dropped |
| `ManageMediaContent` | Describes managing content; is actually an entry gate granting nothing. Weakest despite the broadest-sounding label. | `AccessMediaLibrary`, after `AdminPermissions.AccessAdminPanel` |
| `ViewMediaContent` | Accurate as a grant; doubles as a question (F5). | `ViewAllMedia`, question dropped |
| `ViewRootMediaContent` | Accurate, and now precise since Phase 0 narrowed it to the root's own files. | keep |
| `ManageOwnMediaContent` / `ViewOwnMediaContent` | Accurate. Needs the F3 implication, not a rename. | keep |
| `ManageOthersMediaContent` / `ViewOthersMediaContent` | Accurate. Same. | keep |
| `ManageAttachedMediaFieldsFolder` | Says folder, means the files under it; and it is the odd one with no `Content` suffix beyond `ManageMediaFolder`. | `ManageAttachedMediaFiles` |
| `ViewMediaContent_{folder}` | Accurate, but misleading in effect until F2 is fixed — it grants write too. | keep name, fix behaviour |
| — | Missing entirely (F1). | `ManageMediaContent_{folder}` |

The `Content` suffix is applied inconsistently across the set; worth settling one way in whatever
rename lands.

## 4. What a coherent model looks like

Both options keep group A untouched and rename the entry gate to `AccessMediaLibrary`.

### Option A — repair in place

- Add the `Manage* → View*` implications for own and others (F3), accepting that the folder pair
  cannot be linked (F4).
- Add a `ManageMediaContent_{folder}` dynamic template (F1) and have the manage handler map named
  folders to it, which also fixes F2.
- Rename per F9.

Smallest diff, and every fix is independently shippable. Leaves two parallel trees whose relationship
still has to be explained rather than being evident, and leaves F4 as a permanent caveat.

### Option B — recast on the two axes

- One entry permission: `AccessMediaLibrary`.
- Scope permissions in read/write pairs, generated from one template set over the scopes in §2, so
  each cell of the matrix is a real permission and the empty cells become visible rather than implied.
- The path question becomes a requirement type — `MediaPathRequirement(path, Read|Write)` — not a
  permission. This is the plan's Phase 2, and it is what makes F6 fixable: traversal is a read-only
  concession that cannot license writes.
- Implications run one way only: write of a scope implies read of the same scope; a broader scope
  implies the narrower ones. `ManageAllMedia` sits at the top of both as it does now.

Coherent, explainable in a sentence, and the matrix in §2 becomes the documentation. Costs a migration
for every existing role and a rewrite of the two handlers.

## 5. Recommendation

Option B's *structure*, reached by Option A's *increments*, in this order:

1. **Close F6** — the write side must not accept a traversal-only grant. Needed before the branch is
   pushed; it is a regression, not a phase.
2. **F3** — add the two safe implications. Two lines, removes the inert-checkbox trap.
3. **Phase 2, `MediaPathRequirement`** — the read/write axis, no behaviour change. Everything else
   depends on having an operation axis to talk about.
4. **F1 + F2** — per-folder write permission, so read-only folder access becomes expressible. This is
   the largest functional gap and the one users will notice.
5. **Renames per F9**, with `ImpliedBy` aliases.
6. **F4** — decide whether to lift the entry gate out of the manage tree, which is what would let the
   implications run in the intuitive direction throughout.

Docs come last and should be generated from §2 rather than written from scratch.

## 6. Open questions

1. **F2 is a behaviour change with a security direction.** Fixing it *removes* write access that roles
   currently have — anyone holding a folder view permission today can write to that folder. Is that a
   fix to ship in 4.0.0 with a release note, or does it need a transitional grant?
2. **§6.1 of the plan, unchanged:** what "owned by this role" means. Nothing here settles it, but the
   matrix makes the shape of the answer clearer: role-scoped ownership is one more *scope*, so under
   Option B it is a row in the table rather than a new mechanism.
3. **How many scopes deserve to be user-visible?** Ten rows in §2 is a lot of checkboxes per folder if
   read/write are split. Some scopes (`_users` as a parent, `mediafields/temp`) may be better as
   internal rules than as grants.
4. **The `Content` suffix** — settle it, so the set reads consistently after the renames.
