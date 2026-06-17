# playwright-cli Reference (for OrchardCore testing)

`playwright-cli` is the command-line wrapper used to drive a browser for
OrchardCore functional testing. This page documents the tool-specific gotchas
that matter when testing OrchardCore, especially the **password-field
limitation** that affects the setup wizard and the login form.

## Browser must be installed

`playwright-cli` needs a browser engine installed. On macOS (and any machine
without Chrome) use **webkit**:

```bash
playwright-cli --browser webkit install
```

Then pass `--browser webkit` on the first command that opens a page (the choice
sticks for the session):

```bash
playwright-cli --browser webkit open "http://localhost:$PORT/Login"
```

If you don't specify a browser and Chrome isn't installed, commands fail.

## `open` navigates and discards page/form state

`playwright-cli open <url>` performs a fresh navigation. Anything you typed into
a form is lost. To re-inspect the current page **without** navigating, use
`snapshot` (it re-reads the live DOM and returns fresh element refs):

```bash
playwright-cli snapshot
```

Element refs (e.g. `e16`) come from the latest `snapshot`/`open` output. After a
click that re-renders part of the page, take a new `snapshot` to get fresh refs.

## `eval` is a single expression

`playwright-cli eval '<expr>'` wraps your text as `() => (<expr>)`. Consequences:

- Only a **single expression** works. Multi-statement JavaScript using `;` or
  `var`/`let` declarations silently does nothing.
- To run multiple statements, wrap them in an **arrow-IIFE** that is itself a
  single expression: `((x)=>{ ...; return ...; })(arg)`.
- When the expression returns a non-function value (e.g. a number), the tool may
  print a **benign** error like
  `Error: ... result is not a function. ... 'result' is 10`. The side effects
  (DOM writes, dispatched events) still happen — verify with a follow-up `eval`
  that reads `.value`.

Read a value:

```bash
playwright-cli eval 'document.getElementById("Password").value'
```

## Password fields cannot be filled with `fill`/`type`/`.value`

**This is the key OrchardCore gotcha.** The password inputs on the setup wizard
(`Password`, `PasswordConfirmation`) and the login form
(`LoginForm.Password`) are standard `<input type="password">` elements wrapped in
a Bootstrap `.input-group` with a show/hide toggle button.

Observed with `--browser webkit`:

- `playwright-cli fill <ref> <text>` on these password inputs is a **silent
  no-op** — it prints no output and the value stays empty. (The same `fill`
  works fine on plain inputs like Username, Email, Site Name.)
- Clicking the field then `type`, and a plain `.value =` assignment, **also
  leave the field empty**.
- This is **not** caused by `type="password"` — toggling the field to
  `type="text"` first does not help. It is a `playwright-cli` + webkit
  interaction with these input-group inputs, not an OrchardCore defect (the
  inputs are standard, accessible HTML).

### Working approach: native setter + events

Set the value through the native `HTMLInputElement.prototype.value` setter and
dispatch `input` + `change` so any listeners (password-strength meter, form
validation) react correctly:

```bash
playwright-cli eval '((p)=>{var s=Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype,"value").set; s.call(p,"Password1!"); p.dispatchEvent(new Event("input",{bubbles:true})); p.dispatchEvent(new Event("change",{bubbles:true})); return p.value.length})(document.querySelector("input[type=\"password\"]"))'
```

Replace the selector to target the right field:

- Setup wizard password: `document.getElementById("Password")`
- Setup wizard confirmation: `document.getElementById("PasswordConfirmation")`
- Login password: `document.querySelector("input[name=\"LoginForm.Password\"]")`

Expect the benign `result is not a function` message (the IIFE returns a number).
Verify it worked:

```bash
playwright-cli eval 'document.getElementById("Password").value'   # -> "Password1!"
```

> Prefer the AutoSetup path (`references/autosetup.md`) for provisioning so you
> avoid the wizard password fields entirely. You still need this workaround for
> the **login** password field when signing in through the browser.

## Cleaning up the browser session

```bash
playwright-cli session-stop-all
```
