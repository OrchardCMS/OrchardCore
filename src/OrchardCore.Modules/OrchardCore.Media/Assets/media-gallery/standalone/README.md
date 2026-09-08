# Media gallery — standalone external app

Run the Media gallery SPA on **its own origin** (static host / CDN / dev server) against a remote
OrchardCore tenant, authenticating with OAuth2 authorization-code + PKCE. This is the standalone
counterpart to the embedded gallery Orchard serves via `asp-name="media"`; both build from the same
`src/`, differing only in the entry (`src/standalone.ts` vs `src/main.ts`) and how config is supplied.

See the design rationale in the repo-root `media-gallery-standalone-app-design.md`.

## Build

```bash
# from Assets/media-gallery
yarn build:standalone      # or: npx vite build --config vite.standalone.config.ts
```

Output lands in `dist-standalone/`:

```
dist-standalone/
  index.html               # app host
  callback.html            # interactive-login redirect callback
  silent-callback.html     # silent-renew iframe callback
  assets/                   # hashed JS/CSS
```

Serve `dist-standalone/` as static files from the app origin.

### Run locally (dev)

```bash
# from Assets/media-gallery
cp standalone/config.example.json standalone/config.json   # then edit orchardBaseUrl (gitignored)
yarn dev:standalone                                         # Vite dev server at http://localhost:5173
```

The dev server serves `standalone/` (so `config.json` lives there). To preview a production build
instead: `yarn build:standalone` then `npx vite preview --config vite.standalone.config.ts` (put a
`config.json` in `dist-standalone/`).

⚠️ The app origin here is `http://localhost:5173`, so for auth + API calls to work the Orchard tenant
must register `http://localhost:5173/callback.html` and `.../silent-callback.html` as redirect URIs
and allow `http://localhost:5173` in the CORS policy (edit the origins in the MediaApiStandalone
recipe, or the OpenID/CORS admin screens, before running).

## Configure

Place a `config.json` next to `index.html` (copy `config.example.json`):

| field | required | meaning |
|---|---|---|
| `orchardBaseUrl` | yes | Remote Orchard tenant origin+base, incl. any tenant prefix, trailing slash (`https://cms.example.com/` or `https://cms.example.com/team-a/`). Issuer, API, SignalR hub, and media files resolve here. |
| `appBaseUrl` | no | This app's own origin+base; defaults to the page origin+path. The OIDC redirect URIs live here. |
| `oidcClientId` | no | Defaults to `media_gallery`. |
| `oidcScope` | no | Defaults to `openid email profile roles` (the `roles` scope is required — Media permission checks read it). |
| `signalrEnabled` | no | Real-time updates. Requires SignalR CORS on the Orchard origin. Defaults to false. |

UI labels are fetched from the remote tenant at startup (the anonymous `api/media/localizations`
endpoint returns the same `media-gallery` JS localizations the embedded admin renders, for the
server's culture) — no local translations file needed.

## Orchard-side setup

1. Apply the **MediaApiPkce** recipe first (bearer Media API + OpenID server/validation).
2. Apply a standalone recipe:
   - **Local dev** (app at `http://localhost:5173`): run **MediaApiStandalone — localhost dev**
     (`media-api-standalone-localhost.recipe.json`) — localhost origins are already baked in, nothing
     to edit.
   - **Real deployment**: run **MediaApiStandalone** (`media-api-standalone.recipe.json`) and replace
     the placeholder origins first.
   Either recipe: enables **OrchardCore.Cors** with a **default** policy allowing the app origin for
   the Media API, `/connect/token`, and `/hubs/media` (bearer-only → `AllowCredentials` false); adds
   the app-origin `callback.html` + `silent-callback.html` redirect URIs to the `media_gallery`
   client; and ends with a **`ReloadTenant`** step so the CORS policy takes effect immediately (CORS
   options are cached at startup, so without the reload a recipe-applied policy stays dormant until a
   restart).
3. Ensure signed-in users have roles (carried into the token via the `roles` scope).

CORS origins are **origins only** — scheme+host+port, no path, no trailing slash. The policy must be
the **default** policy, or the global `UseCors()` won't apply it. If you configure CORS via the admin
screen (Configuration → Settings → CORS) instead of a recipe, saving there reloads the tenant for you.

## Manual end-to-end verification

Full verification needs the app origin and the Orchard origin to differ (that is the whole point), so
it can't run in a single-origin unit test. Checklist against a real deployment:

- [ ] Loading the app when **not** signed into Orchard redirects to `/connect/authorize` and back to
      `callback.html`, then lands in the gallery (interactive flow).
- [ ] `GET /api/media/*` calls carry `Authorization: Bearer …` and return 200 (not 401/CORS error).
- [ ] Thumbnails and downloads load from the **Orchard** origin (check the network tab — no 404s
      against the app origin; this exercises `resolveMediaUrl`).
- [ ] Upload (XHR and, if enabled, tus) succeeds cross-origin.
- [ ] With `signalrEnabled: true`, the MediaHub negotiate + WebSocket connect cross-origin and a media
      change in another tab refreshes the gallery.
- [ ] Leaving the tab idle past the token lifetime silently renews via `silent-callback.html` when
      the IdP session is still alive; when it isn't (or third-party-cookie blocking defeats the
      silent iframe), the app automatically restarts the interactive login instead of dying on 401s.
- [ ] Behind **Azure SignalR**: confirm the `roles` claim survives the negotiate → client-token round
      trip (the `ManageMedia` check in `MediaHub.OnConnectedAsync` fails closed otherwise). See the
      Scalability section of `media-api-bearer-pkce-plan.md`.

## Notes / current limitations

- **Self-contained styling** — no Bootstrap. The gallery themes itself via app-owned `--mg-*` design
  tokens (`src/assets/css/file.css`); when embedded they inherit the admin's live Bootstrap theme, and
  standalone they fall back to built-in light/dark defaults. The standalone host follows the OS
  light/dark preference (`[data-bs-theme]` set from `prefers-color-scheme`), and `standalone.css`
  supplies the page shell (box model, typography, body colors).
- Cross-origin **downloads**: `resolveMediaUrl` targets the Orchard origin, but browser `<a download>`
  is ignored cross-origin and `fetch`+blob downloads need CORS on the media file path — verify against
  your storage/CDN and widen CORS if needed.
