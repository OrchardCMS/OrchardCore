---
title: Security
description: Set the browser security headers, CORS policies, reCAPTCHA, rate limits, reverse proxy and HTTPS options, and learn what response compression does.
technical_manual:
  - reference/modules/Security/README.md
  - topics/security/security-settings.md
  - reference/modules/Cors/README.md
  - reference/modules/ReCaptcha/README.md
  - reference/modules/RateLimits/README.md
  - reference/modules/ReverseProxy/README.md
  - reference/modules/Https/README.md
  - reference/modules/ResponseCompression/README.md
---

# Security

These screens protect your site and its visitors: they tell browsers what a page may load, decide which other websites may call your site, stop robots on your sign-in and forms, slow down visitors who send too many requests, and force secure connections. Most of them are technical, and a wrong value can make the site unreachable or break pages, so change them with your developer or IT team.

| Screen | Menu | Permission | Feature |
| --- | --- | --- | --- |
| Security headers | **Settings > Security > Security Headers** | *Manage Security Headers Settings* | **Security** (`OrchardCore.Security`) |
| CORS | **Settings > Security > Cross-Origin Resource Sharing** | *Managing Cors Settings* | **CORS Configuration** (`OrchardCore.Cors`) |
| reCAPTCHA | **Settings > Security > reCaptcha** | *Manage ReCaptcha Settings* | **ReCaptcha** (`OrchardCore.ReCaptcha`), **ReCaptcha Users** (`OrchardCore.ReCaptcha.Users`) |
| Rate limits | **Tools > Rate Limits** | *Manage Rate Limits* | **Rate Limits** (`OrchardCore.RateLimits`) |
| Reverse proxy | **Settings > Reverse Proxy** | *Manage Reverse Proxy Settings* | **Reverse Proxy Configuration** (`OrchardCore.ReverseProxy`) |
| HTTPS | **Settings > Security > HTTPS** | *Manage HTTPS* | **HTTPS** (`OrchardCore.Https`) |
| Response compression | No screen | | **Response Compression** (`OrchardCore.ResponseCompression`) |

The Administrator role has all these permissions by default.


!!! warning "Saving reloads the site"
    The security headers, CORS, reverse proxy and HTTPS screens reload the site when you save. Rate limits reload it when you enable or disable a policy, or delete an enabled one. The site may answer slowly for a few seconds. See [How settings changes apply](general.md#how-settings-changes-apply).

## Set the security headers

Security headers are instructions the site sends to the browser with every page. They tell the browser which scripts, images and frames a page may load, which device features (camera, microphone, location) it may use, and how much of the page address to share when a visitor follows a link to another site.

1. Open **Settings > Security > Security Headers**. The screen has three tabs.
2. Change the values on the tabs you need.
3. Click **Save**.

If your IT team set these headers in the site configuration, a message says *The current settings are coming from configuration sources, saving the settings will affect the AdminSettings not the configuration.* In that case, what you save here isn't what the site uses.

### Content Security Policy tab

This tab controls where a page may load content from.

| Field | What it does |
| --- | --- |
| **Enable Sandbox** | Restricts the page the way a sandboxed frame is restricted. Then type the allowed exceptions in **Sandbox** below. |
| **Upgrade Insecure Requests** | Tells the browser to load every `http://` resource of the page over `https://` instead. |
| **Base URI**, **Child Source**, **Connect Source**, **Default Source**, **Font Source**, **Form Action**, **Frame Ancestors**, **Frame Source**, **Image Source**, **Manifest Source**, **Media Source**, **Object Source**, **Script Source**, **Style Source**, **Sandbox** | Each box lists where that kind of content may come from, separated by spaces, for example `'self' https://cdn.example.com`. Leave a box empty to send no rule for it. **Default Source** applies to every kind you leave empty. **Frame Ancestors** decides which sites may show your pages in a frame. |

!!! warning
    A Content Security Policy that is too strict blocks scripts, styles, fonts or embedded videos, and pages look broken or stop working, including the admin. Test the site after every change.

### Permissions Policy tab

This tab decides which browser features pages may use: **Accelerometer**, **Ambient Light Sensor**, **Autoplay**, **Battery**, **Camera**, **Display Capture**, **Document Domain**, **Encrypted Media**, **Full Screen**, **Game Pad**, **Geolocation**, **Gyroscope**, **Layout Animations**, **Legacy ImageFormat**, **Magnetometer**, **Microphone**, **Midi**, **Oversized Images**, **Payment**, **Picture In Picture**, **Public Key Retrieval**, **Speaker Selection**, **Screen Wake Lock**, **Synchronous XML Http Request**, **Unoptimized Images**, **Unsized Media**, **USB**, **Web Share** and **Web XR**.

For each feature, pick one column:

| Column | What it does |
| --- | --- |
| **None** | No page may use the feature. |
| **Any** | Any page, including pages of other sites shown in a frame, may use it. |
| **Self** | Only your own pages may use it. A box appears where you can add other allowed sites, separated by spaces, for example `https://www.domain.com https://www.another.domain.com`. |

### Referrer Policy tab

When a visitor follows a link from your site to another site, the browser may tell that site which page the visitor came from. **Referrer Policy** decides how much it shares: **No referrer** (the default: nothing), **No referrer when downgrade**, **Origin** (only your domain), **Origin when cross origin**, **Same origin**, **Strict origin**, **Strict origin when cross origin**, or **Unsafe URL** (the full address, always).

The site also always tells browsers not to guess the type of the files it sends, which protects against some attacks. There is no setting for it.

## Let other websites call your site (CORS)

Browsers stop a web page on one website from reading data from another website, unless the second website allows it. Cross-Origin Resource Sharing (CORS) policies are those permissions. You need one when another website or app of yours, such as a mobile web app or a separate front end, calls your site's APIs from the browser.

### Add a CORS policy

1. Open **Settings > Security > Cross-Origin Resource Sharing**.
2. Click **Add a policy**.
3. Fill in the form (see the table below). A new policy starts with **Allow credentials**, **Allow any header** and **Allow any method** checked.
4. To add an origin, header or method, type it in the box under **Allowed origins**, **Allowed headers**, **Allowed methods** or **Exposed headers**, and click **Add Origin**, **Add Header** or **Add Method**. Click **Delete** next to an entry to remove it.
5. Click **Save** at the bottom of the form.

The policy is saved right away, the site reloads, and you see *The CORS settings have updated successfully.*

| Field | What it does |
| --- | --- |
| **Policy name** | The name of the policy. |
| **Set as default policy** | Makes this the policy the site applies. Only one policy can be the default; checking it clears it on the others. If no policy is marked as default, the first one in the list is used. |
| **Allow credentials** | Lets the other site send cookies and sign-in information with its requests. Not allowed together with **Allow any origin**. |
| **Allow any origin** | Accepts requests from any website, and ignores the list of **Allowed origins**. |
| **Allowed origins** | The websites allowed to call your site, such as `https://app.example.com`. |
| **Allow any header** / **Allowed headers** | Accepts any request header, or only the listed ones. |
| **Allow any method** / **Allowed methods** | Accepts any method, or only the listed ones, such as `GET` or `POST`. |
| **Exposed headers** | The response headers the other website may read. |

!!! warning
    If a policy has both **Allow any origin** and **Allow credentials**, the screen warns you and lists the policy. Browsers reject this combination, and it would let any website act on behalf of your signed-in visitors. Fix the policy.

### Edit or delete a CORS policy

The screen lists the policies, with a **Default Policy** badge on the default one. Type in **Search** to filter them.

- Click **Edit** to change a policy, then **Save**. Click **Cancel** to go back without saving.
- Click **Delete** to remove a policy. It is removed and saved immediately, without a confirmation.

## Protect sign-in and forms with reCAPTCHA

reCAPTCHA is a free Google service that asks visitors to prove they are not robots. It stops robots from guessing passwords, creating fake accounts and filling in your forms.

1. Create a reCAPTCHA site on Google's reCAPTCHA website (the link is on the screen). Google gives you a site key and a secret key.
2. Open **Settings > Security > reCaptcha**.
3. Type the **Site Key** and the **Secret Key**.
4. Click **Save**.

| Field | What it does |
| --- | --- |
| **Site Key** | The public key that shows the reCAPTCHA box on your pages. |
| **Secret Key** | The private key the site uses to check the visitor's answer with Google. |

reCAPTCHA works only when both keys are filled in. Then:

- With the **ReCaptcha Users** feature on, the reCAPTCHA box appears on the login, registration, forgot password and reset password pages. People who sign in with an external account, such as Google or Microsoft, don't see it. See [Login and registration](../users/login-and-registration.md).
- With the **Forms** feature on, you can add a **ReCaptcha** widget to a form. See [Forms](../forms-and-automation/forms.md). If the keys are missing, the widget shows *ReCaptcha settings are not configured.*
- With **Workflows** on, the **Validate ReCaptcha** task checks the answer, with the outcomes **Done**, **Valid** and **Invalid**. See [Workflow activities](../forms-and-automation/workflow-activities.md#validation).

## Limit how many requests visitors can send

Rate limits stop a single visitor, or a robot, from overloading the site with requests. When a visitor sends too many, the site answers with the *Too many requests* error page (status 429) instead of the page.

Each signed-in user has their own allowance. Anonymous visitors are counted by their IP address, so visitors who share one address, such as everyone in an office behind one gateway, share one allowance.

### Read the list of policies

Open **Tools > Rate Limits**. The **Rate Limit Policies** list shows your policies, with their status (**Enabled** or **Disabled**), author and target.

- When the feature is first turned on, it adds a **Default Global Policy**: it applies to every request and allows 1000 requests per minute for each user or IP address. Raise it if many visitors share an IP address.
- Under **Read-only Policies**, **Route limits** and **Group limits** list the limits set by features, such as the limits on the login, registration and password pages. You can't change them here.

### Add a policy

New policies start disabled, so you can prepare them without affecting the site.

1. Open **Tools > Rate Limits** and click **Add Policy**.
2. Type a **Policy name** (required, unique) and, optionally, a **Description**.
3. Pick the **Policy type**:
    - **Global**: applies to every request to the site.
    - **Endpoint**: applies to requests whose address starts with the **Request path** you type, which must start with `/`, for example `/api/`.
    - **Group**: applies to the pages a developer put in the **Rate-limit group** you pick.
4. Click **Save and Continue**. The policy opens in **Edit Rate Limit Policy**.
5. Click **Add Limiter**. In **Available Limiter Types**, click **Add** next to a limiter type, fill in its settings (see below), and click **Add Limiter**. You can add several limiters; a request must pass all of them. A policy without limiters limits nothing.
6. Click **Save**.
7. Go back to **Tools > Rate Limits**, and in the policy's row click **Actions > Enable**. Confirm. The site reloads and the policy applies.

### Limiter types

| Limiter | What it does | Settings |
| --- | --- | --- |
| **Fixed window** | Allows a set number of requests during each time period, then resets the count all at once. | **Permit limit** (requests per window), **Window seconds** (length of the window), **Queue limit** |
| **Sliding window** | Spreads the limit across a rolling time period, so the allowance recovers gradually. | **Permit limit**, **Window seconds**, **Segments per window** (more segments recover more gradually), **Queue limit** |
| **Token bucket** | Allows short bursts, while steadily refilling capacity. | **Token limit** (largest burst), **Tokens per period**, **Replenishment seconds** (how often tokens are added), **Queue limit**, **Queue processing order** |
| **Concurrency** | Limits how many requests can run at the same time, useful for slow, expensive pages. | **Permit limit** (requests at the same time), **Queue limit**, **Queue processing order** |

**Queue limit** is how many extra requests wait for room instead of being rejected; 0 rejects them immediately. **Queue processing order** is **Oldest first (first come, first served)** or **Newest first (latest request first)**. Every limit, window and period must be greater than zero.

### Change, disable, clone or delete a policy

While a policy is enabled, you can still change its **Policy name** and **Description**, but its **Policy type**, target and limiters are locked: *This policy is enabled and cannot be modified. To modify it, disable it first or create another policy.*

- **Edit**: opens the policy. Change the fields and the limiters (click **Edit** or **Delete** next to a limiter), then **Save**. Saving a disabled policy doesn't reload the site.
- **Actions > Disable** / **Enable**: turns the policy off or on, after a confirmation. The site reloads.
- **Clone**: makes a disabled copy of the policy, with its limiters, that you can change and enable.
- **Delete**: removes the policy, after a confirmation. Deleting an enabled policy reloads the site.

To act on several policies, check them (or **Select All**), then pick **Enable**, **Disable** or **Delete** in the **Actions** menu above the list. Type in **Search policies** to filter the list.

!!! warning
    A limit that is too low blocks real visitors and editors, including you. Start with a generous limit.

## Set up the reverse proxy options

Many sites run behind another server, such as a load balancer, a CDN or a hosting proxy, that receives the visitors' requests and passes them on. The site then sees the proxy's address instead of the visitor's. These options tell the site to trust the information the proxy adds about the original request.

1. Open **Settings > Reverse Proxy**.
2. Check the options your IT team asks for.
3. Click **Save**.

| Option | What it does |
| --- | --- |
| **Enables the forwarding of the HTTP header X-Forwarded-For** | Uses the visitor's real IP address passed on by the proxy. The **Current request value** under it shows the address the site sees now. |
| **Enables the forwarding of the HTTP header X-Forwarded-Proto** | Uses the original protocol (`http` or `https`) passed on by the proxy. |
| **Enables the forwarding of the HTTP header X-Forwarded-Host** | Uses the original host name passed on by the proxy. |

Turn these on only when the site really runs behind a proxy you trust; your IT team knows. If the values come from the site configuration, the screen says so.

## Require HTTPS

The **HTTPS** feature makes sure visitors use a secure connection (`https://`). It is off by default; turn it on in **Tools > Features**.

1. Open the site with `https://` in the address bar. On a plain `http://` connection, the screen says *For safety, changing HTTPS-only settings over HTTP has been prevented.* and the fields can't be changed.
2. Open **Settings > Security > HTTPS**.
3. Set the options (see the table below).
4. Click **Save**.

| Field | What it does |
| --- | --- |
| **Require HTTPS** | Sends every visitor who uses `http://` to the `https://` address. |
| **HSTS mode** | HTTP Strict Transport Security tells browsers to always use `https://` for your site, even if someone types `http://`. **Disabled**, **Enabled**, or **From environment — enabled in Production, disabled otherwise**. |
| **Require HTTPS Permanent** | Uses a permanent redirect (308) instead of a temporary one (307) when sending visitors to `https://`. |
| **SSL Port** | The port of the secure address. Leave it empty to let the site find it. |

!!! warning "Use HSTS with care"
    After a browser receives the HSTS instruction, it refuses to connect over `http://` for a long time. If your HTTPS certificate later stops working, visitors can't reach the site. Turn HSTS on only when HTTPS is set up for good.

## Response compression

The **Response Compression** feature compresses pages before sending them, so they download faster, also over HTTPS. It has no settings: when it is on, it works for every page. Your hosting may already compress pages, in which case your IT team may leave it off.
