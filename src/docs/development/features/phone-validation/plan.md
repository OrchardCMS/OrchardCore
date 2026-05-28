# Phone Validation Enhancement Plan

**Issue:** [OrchardCMS/OrchardCore#19052](https://github.com/OrchardCMS/OrchardCore/issues/19052)
**Branch:** `skrypt/#19052`
**Date:** 2026-03-28

---

## Problem Statement

The phone number field in the admin user views accepts arbitrary text with no client-side guidance.
Validation only fires server-side after form submission, and the error message ("Please provide a valid phone number") gives no formatting guidance beyond "must include a country code."
Users are confused about what constitutes a valid phone number.

---

## Goals

1. **Client-side as-you-type formatting** so the user sees their input normalized in real-time.
2. **Country code picker** (dropdown with flag + dial code) so users don't need to memorize country codes.
3. **Enhanced server-side validation** that returns richer error messages (e.g., "too short", "too long", detected country).
4. **Consistent UX** across all phone input surfaces: admin user edit/create, SMS authenticator setup.
5. **Extensibility**: keep `IPhoneFormatValidator` as the seam for custom validation.

---

## Current State

| Component | File | Notes |
|-----------|------|-------|
| Validator interface | `OrchardCore.Sms.Abstractions/IPhoneFormatValidator.cs` | `bool IsValid(string)` only |
| Default validator | `OrchardCore.Sms.Core/DefaultPhoneFormatValidator.cs` | Uses `libphonenumber-csharp` v9.0.26, `PhoneNumberUtil.Parse` + `IsValidNumber` |
| Admin phone view | `OrchardCore.Users/Views/UserPhoneNumber.Edit.cshtml` | `<input type="tel">` with no JS |
| SMS auth phone view | `OrchardCore.Users/Views/SmsAuthenticator/Index.cshtml` | `<input type="text">` with no JS |
| Display driver | `OrchardCore.Users/Drivers/UserInformationDisplayDriver.cs` | Server-side validation via `IPhoneFormatValidator` |
| SMS auth controller | `OrchardCore.Users/Controllers/SmsAuthenticatorController.cs` | Same validation pattern |
| View model | `OrchardCore.Users/ViewModels/EditUserPhoneNumberViewModel.cs` | No annotations |
| Resource manifest | `OrchardCore.Users/UserOptionsConfiguration.cs` | Registers `password-generator` and `qrcode` scripts |
| Asset config | `OrchardCore.Users/Assets.json` | Minification pipeline |
| NPM deps | `OrchardCore.Users/Assets/package.json` | Only `qrcodejs` currently |

---

## Implementation Plan

### Phase 1: Enhance the `IPhoneFormatValidator` Interface and Implementation

**Goal:** Return richer validation results instead of a bare `bool`.

#### 1.1 Add `PhoneValidationResult` model

**File:** `OrchardCore.Sms.Abstractions/PhoneValidationResult.cs` (new)

```csharp
namespace OrchardCore.Sms;

public class PhoneValidationResult
{
    public bool IsValid { get; set; }
    public string FormattedNumber { get; set; }   // E.164 format (e.g., "+14155552671")
    public string NationalNumber { get; set; }     // National format (e.g., "(415) 555-2671")
    public string CountryCode { get; set; }        // ISO 3166-1 alpha-2 (e.g., "US")
    public int DialCode { get; set; }              // Country calling code (e.g., 1)
    public string ErrorMessage { get; set; }       // Localized reason if invalid
}
```

#### 1.2 Extend `IPhoneFormatValidator`

**File:** `OrchardCore.Sms.Abstractions/IPhoneFormatValidator.cs`

Add a new method while keeping the existing `IsValid` for backward compatibility:

```csharp
public interface IPhoneFormatValidator
{
    bool IsValid(string phoneNumber);
    PhoneValidationResult Validate(string phoneNumber, string defaultRegion = null);
}
```

#### 1.3 Enhance `DefaultPhoneFormatValidator`

**File:** `OrchardCore.Sms.Core/DefaultPhoneFormatValidator.cs`

- Implement `Validate()` using `PhoneNumberUtil`:
  - `Parse(phoneNumber, defaultRegion)` to support regional numbers without `+` prefix.
  - `GetRegionCodeForNumber()` to detect country.
  - `Format(phone, PhoneNumberFormat.E164)` for normalized storage.
  - `Format(phone, PhoneNumberFormat.NATIONAL)` for display.
  - Return specific error reasons: missing country code, too short, too long, invalid pattern.
- Keep `IsValid()` delegating to `Validate().IsValid`.

#### 1.4 Add `AsYouTypeFormatter` Endpoint (DEFERRED)

> **Blocked on:** OpenId module merge. This endpoint requires proper API auth infrastructure.
> Until then, client-side formatting is handled entirely by `intl-tel-input`'s built-in utils (no server round-trips needed for as-you-type formatting).

**File:** `OrchardCore.Users/Endpoints/PhoneNumber/FormatPhoneNumber.cs` (new, post-OpenId)

Create a minimal API endpoint for server-driven formatting:

- **Route:** `POST /api/phone/format`
- **Input:** `{ "phoneNumber": "415555", "regionCode": "US" }`
- **Output:** `{ "formatted": "(415) 555-", "isValid": false, "e164": null }`
- Uses `AsYouTypeFormatter` from `libphonenumber-csharp`:
  ```csharp
  var formatter = phoneNumberUtil.GetAsYouTypeFormatter(regionCode);
  foreach (char digit in phoneNumber)
      result = formatter.InputDigit(digit);
  ```
- Authorized via OpenId; rate-limited.

---

### Phase 2: Client-Side Phone Input with Country Picker (Vue + Vite)

**Goal:** Add a country-aware phone input with live formatting, built as a Vue 3 component bundled with Vite.

#### 2.1 Add NPM dependencies

**File:** `OrchardCore.Users/Assets/package.json`

```json
{
  "dependencies": {
    "qrcodejs": "^1.0.0",
    "intl-tel-input": "^25.3.1",
    "vue": "^3.5.26"
  }
}
```

**Why `intl-tel-input`:** Most widely used phone input library (12M+ weekly npm downloads), provides a country dropdown with flags, auto-detects country from number, integrates with `libphonenumber` conventions, is accessible, and works well with Bootstrap.

#### 2.2 Create Vue phone input component

**File:** `OrchardCore.Users/Assets/js/components/PhoneInput.vue` (new)

Single-file Vue 3 component (Composition API + `<script setup>`):

```vue
<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue';
import intlTelInput from 'intl-tel-input';

const props = defineProps<{
  modelValue: string;
  disabled: boolean;
  defaultRegion: string;
  confirmed: boolean;
}>();

const emit = defineEmits<{
  'update:modelValue': [value: string];
}>();

// Initialize intl-tel-input on the <input> ref
// Format as-you-type using intl-tel-input's built-in utils (client-only, no API call)
// Write E.164 to hidden field on change
// Show inline validation state (green check / red X)
</script>
```

Responsibilities:
- Wraps `intl-tel-input` in a Vue component with `v-model` support.
- Configure with:
  - `initialCountry: "auto"` (fall back to browser locale or `defaultRegion` prop).
  - `separateDialCode: true` to show dial code outside the input.
  - Uses `intl-tel-input`'s built-in `utils.js` for client-side as-you-type formatting (no server round-trips).
- On input change: emit the full international number (E.164) via `v-model`.
- Show inline validation state (green check / red X) as the user types.
- Respect `disabled` prop when `AllowEditing` is false.
- Display verified/unverified icon based on `confirmed` prop.

#### 2.3 Create Vite entry point

**File:** `OrchardCore.Users/Assets/js/phone-input.ts` (new)

```typescript
import { createApp } from 'vue';
import PhoneInput from './components/PhoneInput.vue';

// Mount on all elements with [data-phone-input]
document.querySelectorAll('[data-phone-input]').forEach((el) => {
  const app = createApp(PhoneInput, {
    modelValue: el.dataset.phoneValue ?? '',
    disabled: el.dataset.phoneDisabled === 'true',
    defaultRegion: el.dataset.phoneRegion ?? '',
    confirmed: el.dataset.phoneConfirmed === 'true',
  });
  app.mount(el);
});
```

#### 2.4 Add Vite build configuration

**File:** `OrchardCore.Users/Assets.json` — add a Vite action entry:

```json
{
  "action": "vite",
  "name": "users-phone-input",
  "source": "Assets/js/phone-input.ts",
  "tags": ["admin", "dashboard", "js"]
}
```

This uses the existing `.scripts/assets-manager/vite.mjs` build runner with the `@vitejs/plugin-vue` plugin already configured at the repo root. The Vite plugin generates both `phone-input.js` and `phone-input.min.js` following OrchardCore naming conventions.

#### 2.5 Register resources

**File:** `OrchardCore.Users/UserOptionsConfiguration.cs`

Add resource definitions:

```csharp
_manifest
    .DefineScript("phone-input")
    .SetUrl("~/OrchardCore.Users/Scripts/phone-input.min.js",
            "~/OrchardCore.Users/Scripts/phone-input.js")
    .SetDependencies("vuejs:3")
    .SetVersion("1.0.0");

_manifest
    .DefineStyle("phone-input")
    .SetUrl("~/OrchardCore.Users/Styles/phone-input.min.css",
            "~/OrchardCore.Users/Styles/phone-input.css")
    .SetVersion("1.0.0");
```

---

### Phase 3: Update Views

#### 3.1 Admin user phone number edit view

**File:** `OrchardCore.Users/Views/UserPhoneNumber.Edit.cshtml`

Replace the current plain `<input type="tel">` with a Vue mount point:

```html
<div class="mb-3" asp-validation-class-for="PhoneNumber">
    <label asp-for="PhoneNumber" class="form-label">@T["Phone Number"]</label>
    <div data-phone-input
         data-phone-value="@Model.PhoneNumber"
         data-phone-disabled="@(!Model.AllowEditing)"
         data-phone-region="@Model.DefaultRegion"
         data-phone-confirmed="@Model.PhoneNumberConfirmed">
    </div>
    <input type="hidden" asp-for="PhoneNumber" data-phone-e164 />
    <span asp-validation-for="PhoneNumber" class="text-danger"></span>
</div>

<style asp-name="phone-input"></style>
<script asp-name="phone-input" at="Foot"></script>
```

Key changes:
- Vue component mounts on the `[data-phone-input]` div, renders the full input + country picker + icons.
- Hidden input `data-phone-e164` holds the E.164 value for form submission, synced by Vue.
- Remove the hardcoded country code hint text (the dropdown replaces it).
- Include the `phone-input` script and style resources.

#### 3.2 SMS authenticator phone input view

**File:** `OrchardCore.Users/Views/SmsAuthenticator/Index.cshtml`

Apply the same `data-phone-input` pattern. Change `type="text"` to `type="tel"`.

---

### Phase 4: Update View Model and Server-Side Handling

#### 4.1 Extend view model

**File:** `OrchardCore.Users/ViewModels/EditUserPhoneNumberViewModel.cs`

Add an optional `RegionCode` property so the server knows which region the user selected:

```csharp
public class EditUserPhoneNumberViewModel
{
    public string PhoneNumber { get; set; }
    public string RegionCode { get; set; }

    [BindNever]
    public bool PhoneNumberConfirmed { get; set; }
    [BindNever]
    public bool AllowEditing { get; set; }
}
```

#### 4.2 Update display driver validation

**File:** `OrchardCore.Users/Drivers/UserInformationDisplayDriver.cs`

Replace:
```csharp
if (!string.IsNullOrEmpty(phoneNumberModel.PhoneNumber)
    && !_phoneFormatValidator.IsValid(phoneNumberModel.PhoneNumber))
{
    context.Updater.ModelState.AddModelError(..., S["Please provide a valid phone number."]);
}
```

With:
```csharp
if (!string.IsNullOrEmpty(phoneNumberModel.PhoneNumber))
{
    var result = _phoneFormatValidator.Validate(
        phoneNumberModel.PhoneNumber,
        phoneNumberModel.RegionCode);
    if (!result.IsValid)
    {
        context.Updater.ModelState.AddModelError(..., S[result.ErrorMessage]);
    }
    else
    {
        // Store the normalized E.164 format.
        user.PhoneNumber = result.FormattedNumber;
    }
}
```

This gives users specific error messages and ensures consistent E.164 storage.

#### 4.3 Update SMS authenticator controller

**File:** `OrchardCore.Users/Controllers/SmsAuthenticatorController.cs`

Apply the same `Validate()` pattern in `IndexPost()`.

---

### Phase 5: Settings and Configuration

#### 5.1 Add default region setting

**File:** `OrchardCore.Users.Core/Models/LoginSettings.cs`

Add a `DefaultPhoneRegion` property:

```csharp
public string DefaultPhoneRegion { get; set; }  // ISO 3166-1 alpha-2, e.g., "US"
```

#### 5.2 Expose setting in admin UI

**File:** `OrchardCore.Users/Drivers/LoginSettingsDisplayDriver.cs`

Add a region code dropdown (populated from `libphonenumber-csharp`'s supported regions) to the login settings form.

#### 5.3 Pass default region to views

Update the `EditAsync` method in `UserInformationDisplayDriver` to pass the default region to the view model, so the JS can set the initial country in the dropdown.

---

## File Change Summary

| Action | File | Description |
|--------|------|-------------|
| NEW | `OrchardCore.Sms.Abstractions/PhoneValidationResult.cs` | Rich validation result model |
| EDIT | `OrchardCore.Sms.Abstractions/IPhoneFormatValidator.cs` | Add `Validate()` method |
| EDIT | `OrchardCore.Sms.Core/DefaultPhoneFormatValidator.cs` | Implement `Validate()` with `AsYouTypeFormatter` |
| EDIT | `OrchardCore.Users/Assets/package.json` | Add `intl-tel-input` + `vue` dependencies |
| NEW | `OrchardCore.Users/Assets/js/components/PhoneInput.vue` | Vue 3 phone input component |
| NEW | `OrchardCore.Users/Assets/js/phone-input.ts` | Vite entry point, mounts Vue component |
| EDIT | `OrchardCore.Users/Assets.json` | Add Vite build action for phone-input |
| EDIT | `OrchardCore.Users/UserOptionsConfiguration.cs` | Register phone-input script/style resources |
| EDIT | `OrchardCore.Users/Views/UserPhoneNumber.Edit.cshtml` | Vue mount point + country picker |
| EDIT | `OrchardCore.Users/Views/SmsAuthenticator/Index.cshtml` | Same phone input upgrade |
| EDIT | `OrchardCore.Users/ViewModels/EditUserPhoneNumberViewModel.cs` | Add `RegionCode` property |
| EDIT | `OrchardCore.Users/Drivers/UserInformationDisplayDriver.cs` | Use `Validate()` for richer errors + E.164 normalization |
| EDIT | `OrchardCore.Users/Controllers/SmsAuthenticatorController.cs` | Same validation upgrade |
| EDIT | `OrchardCore.Users.Core/Models/LoginSettings.cs` | Add `DefaultPhoneRegion` setting |
| EDIT | `OrchardCore.Users/Drivers/LoginSettingsDisplayDriver.cs` | Expose region setting in admin |
| DEFERRED | `OrchardCore.Users/Endpoints/PhoneNumber/FormatPhoneNumber.cs` | API endpoint — blocked on OpenId merge |

---

## Risks and Mitigations

| Risk | Mitigation |
|------|------------|
| `intl-tel-input` adds ~200KB of assets (flags sprite + utils) | Vite tree-shaking; load only when phone input is on page via lazy `<script>` |
| Breaking change to `IPhoneFormatValidator` | Add `Validate()` as a new method; keep `IsValid()` intact. Provide default implementation. |
| API endpoint adds attack surface | Deferred until OpenId module is merged for proper auth infrastructure |
| Existing stored phone numbers not in E.164 | Migration not needed immediately — validation only applies on edit. Document that existing numbers may display differently. |
| Vue bundle size overhead | Vite builds a scoped module; Vue 3 is already registered as a shared resource in OrchardCore.Resources — no duplication |

---

## Implementation Order

1. **Phase 1** (backend) — Enhance `IPhoneFormatValidator` and `DefaultPhoneFormatValidator`
2. **Phase 2** (frontend) — Vue 3 + Vite component with `intl-tel-input`, register resources
3. **Phase 3** (views) — Update Razor views with Vue mount points
4. **Phase 4** (integration) — Wire up view models, display driver, controller
5. **Phase 5** (settings) — Add default region configuration
6. **Phase 1.4** (deferred) — API endpoint after OpenId module merge

Phases 1-5 can ship now. Phase 1.4 is deferred and tracked separately.
Each phase is independently testable and can be submitted as a separate PR if preferred.

---

## Testing Checklist

- [ ] Admin create user: phone input shows country dropdown, formats as you type
- [ ] Admin edit user: phone input pre-selects correct country from existing E.164 number
- [ ] Admin edit user: disabled phone input when `AllowChangingPhoneNumber` is false
- [ ] SMS authenticator setup: phone input works with country picker
- [ ] Server-side validation: specific error messages for too-short, too-long, invalid
- [ ] E.164 normalization: phone stored in E.164 format after save
- [ ] Backward compatibility: `IsValid()` still works for custom implementations
- [ ] No JS fallback: form still works without JavaScript (plain tel input, server validation)
- [ ] Default region setting: configurable in admin, used as initial country in dropdown
- [ ] Verified/unverified icon still displays correctly
