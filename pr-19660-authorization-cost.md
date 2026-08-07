# PR #19660 — measured cost of per-folder authorization

Numbers for the performance / DoS concern raised on
[#19660](https://github.com/OrchardCMS/OrchardCore/pull/19660), and for the "can folder permissions be
cached?" question.

**Method.** A reproducible xUnit test counts every `IMediaFileStore` call made while authorizing a media
folder, using an in-memory counting store:

```
test/OrchardCore.Tests/Modules/OrchardCore.Media/MediaFolderAuthorizationCostTests.cs

dotnet test test/OrchardCore.Tests/OrchardCore.Tests.csproj \
  -- --filter-class "OrchardCore.Tests.Modules.OrchardCore.Media.MediaFolderAuthorizationCostTests"
```

7 tests, all passing on `main` @ `5c60562e93`.

---

## 1. Measured — cost of **one** `AuthorizeAsync(user, ManageMediaFolder, path)`

| Configuration | File-store round-trips |
| --- | --- |
| Secure Media **disabled** | **0** |
| Secure Media **enabled**, user holds folder-scoped `ViewMediaContent_{folder}` | **1** (`GetDirectoryInfoAsync`) |
| Secure Media **enabled**, user holds global `ViewMedia` | **0** — short-circuits |

Why: [`ManageMediaFolderAuthorizationHandler`](src/OrchardCore.Modules/OrchardCore.Media/Services/ManageMediaFolderAuthorizationHandler.cs#L92-L97)
adds a `ViewMedia` check for the path when `IsSecureMediaEnabled()`, and
[`ViewMediaFolderAuthorizationHandler`](src/OrchardCore.Modules/OrchardCore.Media/Services/ViewMediaFolderAuthorizationHandler.cs#L76-L89)
stats the directory to distinguish "new directory" from "file in root". On Azure Blob or S3 that stat is
a **network round-trip**, not a local `stat`.

Also measured: **no memoization exists** — authorizing the same path `F + 1` times costs exactly
`(F + 1) ×` the single-check cost.

## 2. Measured — the redundancy that makes caching possible

`ViewMediaFolderAuthorizationHandler` derives the permission from the path **up to the first separator**.
Verified by test: `photos`, `photos/2026` and `photos/2026/january` all resolve to the same
`ViewMediaContent_photos` decision — and each still pays its own round-trip.

**Consequence:** when listing a **non-root** directory, all `F + 1` checks share one first-tier folder, so
they compute **one distinct decision** `F + 1` times. A first-tier-keyed cache — or simply hoisting the
decision — collapses that to **1**.

The **root listing is the exception**: its children are distinct first-tier folders, so `F` distinct
decisions genuinely are required there.

## 3. Derived — cost per directory listing after #19660

The endpoint already costs `3 + F` storage operations (1 `GetDirectoryInfoAsync` + 1 `GetDirectoriesAsync`
+ 1 `GetFilesAsync` + `F` `HasChildren` probes, see
[MediaEndpointHelpers.cs:94-98](src/OrchardCore.Modules/OrchardCore.Media/Endpoints/Api/MediaEndpointHelpers.cs#L94-L98)).
#19660 adds `F + 1` authorization checks, each costing 1 round-trip in the Secure Media + folder-scoped
case:

| Subfolders `F` | Today | After #19660 | Change |
| --- | --- | --- | --- |
| 10 | 13 | 24 | **+85 %** |
| 25 | 28 | 54 | +93 % |
| 50 | 53 | 104 | +96 % |

Asymptotically the listing cost **doubles** (`3 + F` → `2F + 4`).

## 4. Derived — interaction with real-time updates

Every `MediaChanged` event makes each connected client re-run this listing
([SignalR.ts:34](src/OrchardCore.Modules/OrchardCore.Media/Assets/media-gallery/src/services/SignalR.ts#L34)
calls `loadDirectoryFiles(..., true)` unconditionally, even on a cache hit).

**#19660 helps here**: switching from `Clients.All` to per-folder groups means only clients *viewing that
folder* react, instead of every connected admin. That is a large win.

But the per-client cost roughly doubles at the same time. With `N` clients viewing the folder, `F = 10`:

| | Today (`Clients.All`, N = all clients) | After #19660 (N = viewers of that folder) |
| --- | --- | --- |
| One uploaded file, N = 50 | 650 | 1,200 |
| 200-file bulk upload, N = 50 | 130,000 | 240,000 |

So for a folder that several editors are watching, #19660 is net-positive only if it removes more clients
from the fan-out than it adds cost per remaining client. For the worst case — a shared folder several
editors have open during a bulk upload — it is a regression.

## 5. What the data supports

Three mitigations, in order of value. **Only the third needs a cache.**

1. **Skip the stat when the caller already knows it's a directory.** The `GetDirectoryInfoAsync` exists
   only to disambiguate new-directory vs file-in-root — a question the listing code has already answered,
   since it is enumerating known directories. An overload taking that hint removes the round-trip
   entirely: **1 → 0** per check, no semantic change. Highest value, lowest risk.
2. **Short-circuit on the global permission.** Measured at 0 round-trips (§1, row 3), so administrators
   are already unaffected. Making this explicit and early keeps the cost confined to exactly the
   folder-scoped editors Secure Media exists for.
3. **Cache by first-tier folder, not by full path.** §2 shows the decision is first-tier-only, so a
   non-root listing needs **one** decision rather than `F + 1`. Key on
   `(user + roles version, first-tier folder)` with a short absolute expiry plus an `ISignal` token —
   the pattern [`SecureMediaPermissions`](src/OrchardCore.Modules/OrchardCore.Media/SecureMediaPermissions.cs#L55-L66)
   already uses (5 minutes + `_signal.GetToken(...)`).

   **Exclude the two zones that genuinely cannot be cached**: `mediafields/…`, whose decision depends on
   per-content-item `ViewContent` permissions that change independently, and `_Users/…`, which is
   caller-relative. That exclusion is, I believe, the substance of the "caching isn't possible"
   objection — it holds *for those zones*, not for the general case.

Per-request memoization is a safe backstop regardless: within one request the decision is deterministic,
so repeated checks of the same path across endpoint and helper layers cost once.

## 6. Caveats — read before quoting these numbers

- **§1 and §2 are measured. §3 and §4 are derived**, by applying the measured per-check cost to the
  `F + 1` pattern read from the PR. The PR's code is not on the branch these tests run on, so the
  multiplier is not executed.
- The counting store is in-memory: it counts **round-trips, not latency**. The impact depends entirely on
  the backing store — negligible on a local filesystem, a network call each on Azure Blob or S3.
- Permissions are granted through a stub handler rather than the full role/claims pipeline. That affects
  only *whether* access is granted; the number of file-store calls is decided by the media handlers under
  test.
- `F` is the subfolder count of the directory being listed, not the total media library size.
