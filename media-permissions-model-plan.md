# Media permissions model — cleanup plan

Status: Phases 0 and 1 done. **Further implementation is on hold** pending
`media-permissions-analysis.md`, which examines the whole permission set and supersedes this plan's
Phase 2 onward — those phases should be re-derived from its §5 ordering. Phase 1 also left a
regression on the write side (analysis F6) that must be closed before this branch is pushed.
Branch: `skrypt/media-permissions-model`, branched off `skrypt/media-auth-cost-fix` to keep these
behaviour changes out of the pushed cost-fix branch. Unpushed.

## 1. Why

The media permission set has grown into something that cannot be reasoned about from the Roles
editor. Three separate problems compound:

1. **One identifier does two unrelated jobs.** `ManageMediaFolder` is both a grantable
   "everything, everywhere" permission and the name used for the resource-based question *"may this
   user act on this path?"* at 23 call sites. `ViewMediaContent` has the same split.
2. **The implication tree reads backwards, and its base is misnamed.** `ManageMediaContent`
   ("Manage Media") sounds broad but is the weakest permission in the tree — `ManageOwnMediaContent`
   implies it. Granting "own media only" therefore also grants the generic "Manage Media" flag. The
   name describes managing content, but the permission is an entry gate: it is checked resourcelessly
   before anything else and grants nothing by itself. It is the media equivalent of
   `AdminPermissions.AccessAdminPanel`.
3. **A plausible, wanted configuration is impossible.** "Let this role open the Media Library and work
   only in its own folder" has no working permission combination (§3.1).

The names are the visible symptom; the question/grant conflation is the cause.

## 2. Current behaviour, for reference

### 2.1 Grantable permissions

"Granted by" is the `ImpliedBy` closure — holding any of those satisfies a check for that row.

| Claim name | Feature | Granted by | Actually gates |
| --- | --- | --- | --- |
| `ManageMediaFolder` | Media | — | Nothing directly; root of both trees, so it confers every other media permission including the view ones. Also the path question. |
| `ManageMediaContent` | Media | `ManageOwnMediaContent`, `ManageOthersMediaContent`, `ManageAttachedMediaFieldsFolder`, `ManageMediaFolder` | "May use the Media Library at all" — checked resourceless by every endpoint, the admin page, the menu, the TUS hook. |
| `ManageOwnMediaContent` | Media | `ManageOthersMediaContent`, `ManageMediaFolder` | Write to `_users` and `_users/{own}/**`; only reached via the path mapping. |
| `ManageOthersMediaContent` | Media | `ManageMediaFolder` | Write to other users' `_users` folders; only via the path mapping. |
| `ManageAttachedMediaFieldsFolder` | Media | `ManageMediaFolder` | Write to `mediafields/**`; only via the path mapping. |
| `ManageMediaProfiles` | Media | — | MediaProfiles controller + menu. |
| `ViewMediaOptions` | Media | — | Configuration → Media Options. |
| `ManageMediaApiSettings` | Media | — | Media API settings driver. |
| `ManageAssetCache` | Media Cache | — | Asset cache admin. |
| `ViewMediaContent` | Secure Media | `ManageMediaFolder` | Read every folder. Also the path question. |
| `ViewRootMediaContent` | Secure Media | `ViewMediaContent`, `ManageMediaFolder`, and every `ViewMediaContent_{folder}` **on the provider's instance only** | Read the media root. |
| `ViewOthersMediaContent` | Secure Media | `ManageMediaFolder` | Read other users' `_users` folders and their `mediafields/temp`. |
| `ViewOwnMediaContent` | Secure Media | `ViewOthersMediaContent`, `ManageMediaFolder` | Read `_users`, own folder, own `mediafields/temp`. |
| `ViewMediaContent_{folder}` | Secure Media, generated per first-level folder | `ViewMediaContent`, `ManageMediaFolder` | Read that folder and below. |

### 2.2 The path questions

`ManageMediaFolder` + path → `ManageMediaFolderAuthorizationHandler` maps to:

| Resolved path | Requires |
| --- | --- |
| root `""` | `ManageMediaContent` |
| `mediafields`, `mediafields/**` | `ManageAttachedMediaFieldsFolder` |
| `_users`, `_users/{own}`, `_users/{own}/**` | `ManageOwnMediaContent` |
| `_users/{other}`, `_users/{other}/**` | `ManageOthersMediaContent` |
| anything else | `ManageMediaContent` |

With Secure Media on, it *additionally* requires the `ViewMediaContent` question on the same path.

`ViewMediaContent` + path → `ViewMediaFolderAuthorizationHandler` maps to:

| Resolved path | Requires |
| --- | --- |
| root container `""` | `ViewRootMediaContent` (provider instance, so any folder grant passes) |
| file directly in root | `ViewRootMediaContent`, strict |
| `mediafields` alone | denied |
| `mediafields/temp/{own}/**` | `ViewOwnMediaContent` |
| `mediafields/temp/{other}/**` | `ViewOthersMediaContent` |
| `mediafields/{contentItemId}/**` | Contents `ViewContent` on that item |
| `_users` | `ViewOwnMediaContent` |
| `_users/{own}/**` | `ViewOwnMediaContent` |
| `_users/{other}/**` | `ViewOthersMediaContent` |
| `{folder}/**` | `ViewMediaContent_{folder}` |

### 2.3 Ownership

Ownership means exactly one thing: `_users/{NameIdentifier}`, from
`DefaultUserAssetFolderNameProvider`. There is no role-level ownership and no ownership of folders
outside `_users`. Any requirement phrased as "folders owned by this role" needs a new concept — see
§6.1.

## 3. Confirmed gaps

### 3.1 "Own folders only" cannot be configured

A role holding only `ManageOwnMediaContent` + `ViewOwnMediaContent` (what Author plus the
Authenticated stereotype gives) cannot open the Media Library. Every endpoint gates on the
`ManageMediaFolder` question for the root, which nests the `ViewMediaContent` question on `""`, and
root traversal is satisfied only by `ViewRootMediaContent`, `ViewMediaContent`, `ManageMediaFolder`,
or a dynamic folder permission. `ViewOwnMediaContent` implies none of them.

Was covered by `NoFolderPermissionDoesNotGrantRootViewPermission`, which asserted the denial. Phase 1
reverses that expectation; the tests are now `OwnMediaPermissionGrantsRootTraversal` and
`OwnMediaPermissionDoesNotGrantRootFiles`, with `NoMediaPermissionDoesNotGrantRootViewPermission`
keeping the no-permissions case pinned.

Creating a folder inside one's own folder *would* authorize correctly
(`ManageOwnMediaContent` + `ViewOwnMediaContent`), and `IsSpecialFolder` only blocks creating
directly inside `_users` itself. So this is purely an entry-point problem.

### 3.2 Root traversal and root content were the same grant

Fixed in Phase 0.

### 3.3 The two `ViewRootMediaContent` instances disagreed

Fixed and shipped in commit `eafb4e7bd8`; the handler now asks the provider for its instance.

## 4. Phases

### Phase 0 — done (`82dc2e4173`)

Behavioural fixes that stand on their own and do not depend on the rename:

- `ViewMediaFolderAuthorizationHandler`: track `isRootFile` so a file lying in the root requires
  `ViewRootMediaContent` strictly, while the bare root container still accepts any folder grant.
- `MediaEndpointHelpers.CanListRootFilesAsync`, used by `GetDirectoryContentEndpoint` and
  `GetMediaItemsEndpoint`, so root files are not listed to callers that only traverse. No-op when
  Secure Media is off.
- `SecureMediaPermissions`: drop the Anonymous stereotype, so enabling Secure Media no longer grants
  the public every folder. **Breaking** — see §5.
- `AdminController.Index`: gate on the root question as well as `ManageMediaContent`, and return
  `NotFound()` instead of `Forbid()`, matching the middleware's hide-don't-tell behaviour.
- `AdminMenu`: gate the Media Library entry on the same question so it disappears instead of leading
  to a 404.
- Handler tests extended for the root-file/container split.
- Release note in `src/docs/releases/4.0.0.md` for the Anonymous change (§6.2) — it is breaking, so it
  ships with the change rather than waiting for Phase 5.

Shipped as one commit, with the release note.

### Phase 1 — done (`afbda01801`) — make "own folders only" work

Smallest change that closes §3.1. Root *traversal* should succeed when the user can view anything
beneath the root, not only when they can view the root's own contents.

- `ViewMediaFolderAuthorizationHandler`, root container branch: succeed if any of the root permission,
  a dynamic folder permission, `ViewOwnMediaContent`, or `ViewOthersMediaContent` is granted.
- Verify the resulting listing shows `_users` and the user's own folder and nothing else — the folder
  filter in `GetDirectoryFoldersAsync` already handles this, and Phase 0 stops root files leaking.
- Tests: own-media-only role opens the root; sees `_users`; does not see other first-level folders,
  root files, or other users' folders; can create a folder inside its own folder.

No renames, no new types. Independent of Phases 2-4.

### Phase 2 — next: separate the question from the grant

No behaviour change; this is the structural fix.

- Introduce a dedicated requirement, e.g. `MediaPathRequirement(string path, MediaAccess access)` with
  `access` in `{ Read, Write }`, plus a small `IMediaPathAuthorizationService` wrapper so call sites
  read as `AuthorizeAsync(user, MediaAccess.Write, path)` rather than borrowing a permission name.
- Re-target the two handlers at the new requirement. Their internal path→permission mapping (§2.2) is
  unchanged and stays the single source of truth.
- Migrate the 23 resource-passing call sites. Mechanical, but touches every endpoint, `MediaHub`,
  `SecureMediaMiddleware`, `AdminController`, `AdminMenu`, and the TUS hooks in `Startup`.
- Keep `ManageMediaFolder` / `ViewMediaContent` working as resource-based checks for one release,
  marked obsolete, so external modules do not break.
- The `(object)` cast trap disappears with the dedicated API — worth calling out in the release notes.

### Phase 3 — rename the grant

Only after Phase 2, when the name has one meaning left.

Permission names are persisted as claim values, so every rename here keeps the old name as an
`ImpliedBy` alias permission. A bare rename silently revokes access — the alias is mandatory, not
optional.

- `ManageMediaFolder` → `ManageAllMedia` ("Manage all media"). It is not about a folder and does not
  manage one; as a grant it means everything, everywhere.
- `ManageMediaContent` → `AccessMediaLibrary` ("Access the Media Library"). It gates the admin page,
  the menu entry, every API endpoint, and the TUS hook, and confers no access on its own — the same
  role `AccessAdminPanel` plays in `AdminFilter`. Naming it after that precedent also stops it reading
  as the broadest permission when it is the weakest.
- Consider `ViewMediaContent` → `ViewAllMedia` for symmetry.
- Update the recipes and the permission tables in the docs.

Note the ordering constraint: renaming `ManageMediaContent` does not depend on Phase 2, since it is
never used as a path question — it could ship earlier if the rest of Phase 3 stalls.

### Phase 4 — revisit the implication tree

Optional, and the most disruptive; may be deferred indefinitely.

The inversion in §1.2 is confusing but currently *load-bearest* at `ManageMediaContent`, which acts as
"may use the library". Splitting that into an explicit `UseMediaLibrary` permission would let the
Manage tree run in the intuitive direction. Needs a migration for every existing role.

### Phase 5 — documentation

- Rewrite the permission table in `src/docs/reference/modules/Media/README.md` to match §2.1, and add
  the path-mapping table from §2.2 — the current table documents neither the folder→root implication
  nor the mapping.
- Add a "common configurations" section with the exact permission set for: own folders only; a single
  shared folder; public read-only media; full media administration.
- Document the Anonymous default change and what to grant to restore public media.
- Note that the Media Library page 404s rather than 403s, and that the menu entry hides itself.

## 5. Compatibility

| Change | Impact |
| --- | --- |
| Anonymous stereotype removed | Confirmed, see §6.2. Only affects *newly* created/updated roles, since stereotypes apply at role creation. Fresh Secure Media enablement 404s all public media until `ViewMediaContent` is granted to Anonymous. Release note ships with Phase 0. |
| Root files no longer visible to folder-only roles | Intended tightening. A role that relied on it must be granted `ViewRootMediaContent`. |
| `Admin/Media` 404 instead of 403 | Anything asserting 403 on that route changes; check the functional tests. |
| Phase 2 call-site migration | Old permission-with-resource checks kept working for one release. |
| Phase 3 renames | Safe only with the `ImpliedBy` aliases. Without them, every role holding `ManageMediaFolder` loses all media access, and every role holding `ManageMediaContent` loses the Media Library entirely. Recipes and deployment plans that set permissions by name also need the aliases to keep importing. |

## 6. Open decisions

### 6.1 What does "owned by this role" mean?

The stated goal is a role that may create and manage only folders it owns. Today ownership is
`_users/{userId}` and nothing else. Options:

- **(a) Per-user only, as now.** Own-folders-only means "work inside `_users/{yourId}`". Phase 1 alone
  delivers this. No new concepts. The folders live under `_users`, which is not where most people want
  their content.
- **(b) Role-scoped asset folder.** Extend `IUserAssetFolderNameProvider` (or add a sibling) so a
  role maps to a folder, e.g. `_roles/{roleName}`, authorized like `_users`. Moderate change,
  no per-folder ACL storage.
- **(c) Real folder ownership.** Persist an owner per folder and authorize against it. Most flexible,
  by far the largest change — new storage, migration, UI, and it partly duplicates the dynamic
  first-level folder permissions.

Recommendation: (a) now via Phase 1, and treat (b) as a separate feature if the `_users` location is
the actual objection. (c) only with a concrete requirement that (b) cannot meet.

**Still undecided.** Phase 1 does not depend on the outcome: per-user own-folder traversal is required
under all three options, since (b) and (c) both add locations to reach rather than removing `_users`.
So Phase 1 can proceed while this is settled — it is only the *sufficiency* of Phase 1 as the complete
answer to "own folders only" that hangs on this.

### 6.2 Settled: the Anonymous default change stays

Secure Media no longer grants `ViewMediaContent` to the Anonymous role. It is the right default for a
feature whose purpose is restricting media, and it removes the trap where an Anonymous grant silently
overrides every folder-scoped restriction on signed-in roles — anonymous claims are evaluated for
authenticated users too.

Because it is breaking, it ships with a release note in `src/docs/releases/4.0.0.md` as part of
Phase 0, stating that a fresh Secure Media enablement serves 404 for public media until
`ViewMediaContent` (or a folder permission) is granted to Anonymous, and that existing roles keep the
claim they already hold since stereotypes only apply at role creation.

### 6.3 How far to take Phase 4?

Cheap option: leave the tree alone and document it. Expensive option: introduce `UseMediaLibrary` and
migrate. Recommend documenting now, deciding later once Phases 1-3 have settled.

### 6.4 Settled: `ManageMediaContent` stays the entry gate, under a better name

It keeps its job — the resourceless ticket checked in ~15 places — and is renamed to
`AccessMediaLibrary` in Phase 3. What remains open is only whether Phase 4 additionally moves it out
of the Manage implication tree, so that `ManageOwnMediaContent` no longer implies it.

## 7. Test plan

- Unit, per phase, in `ViewMediaFolderAuthorizationHandlerTests` and
  `ManageMediaFolderAuthorizationHandlerTests`: every row of the §2.2 mapping tables, asserted from
  the permission claims a role would actually hold.
- A test that the Roles editor's effective calculation and the runtime agree for
  `ViewRootMediaContent` — the divergence that started this work. `eafb4e7bd8` covers the handler
  side; the provider side is still only covered indirectly.
- Functional: own-media-only user opens the Media Library, sees only their folder, creates a subfolder,
  and gets 404 on a root file URL.
- Functional: role with no media permissions gets 404 on `Admin/Media` and no menu entry.
- Phase 2 is a refactor: the existing suites must pass unchanged, which is the point.
