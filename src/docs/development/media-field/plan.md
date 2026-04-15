# Media Field Picker Upload Plan

## Background

Issue [#11940](https://github.com/OrchardCMS/OrchardCore/issues/11940) requests that when a user uploads a file from the media picker opened by a media field, the newly uploaded file should:

- be selected automatically
- appear at the top of the file list
- be inserted into the field when the user clicks `Ok`, unless the user explicitly deselects it

This request makes sense for the picker-backed media fields, but not for the Attached Media Field.

## Recommendation

Apply this behavior only to the picker-backed media fields:

- Standard Media Field
- Gallery Media Field

Do not change the Attached Media Field.

The Attached Media Field already uploads directly into the field state and does not require a second selection step after upload. Changing it to match issue #11940 would not solve an existing problem and would risk regressing a better workflow.

## Current Behavior

### Attached Media Field

The Attached Media Field uploads files directly and appends them to the field model immediately. There is no separate picker confirmation step.

### Standard and Gallery Media Fields

The Standard and Gallery Media Fields open `MediaPickerModal`, which embeds the Vue 3 media app as a picker.

The current picker flow is:

1. open the picker modal
2. upload a file inside the embedded media app
3. manually select the uploaded file in the browser UI
4. click `Ok`
5. receive the selected files back in the field component

This is the gap described in issue #11940.

## Scope

### In Scope

- Auto-select files uploaded during the current picker session
- Surface newly uploaded files at the top of the list while the picker is open
- Preserve the user's ability to deselect uploaded files before clicking `Ok`
- Keep behavior consistent across Standard and Gallery Media Fields

### Out of Scope

- Any workflow changes to Attached Media Field
- Global sorting changes for the standalone Media library outside picker mode
- Changes to persisted media ordering in the library

## Implementation Plan

### 1. Add picker-session upload tracking in the media app

Introduce picker-session state in the media app to track uploaded files for the current modal session.

The state should support:

- identifying files uploaded while the picker is open
- marking those files as selected by default
- distinguishing picker-only temporary ordering from the normal library sort order

This should be implemented in the shared picker/media-app integration layer instead of inside individual media field components.

### 2. Extend the picker contract

The picker integration currently exposes selection count changes and a method to read selected files. Extend this contract so picker mode can react to upload success, rather than forcing `MediaPickerModal` to infer uploaded items from generic list state.

Preferred design:

- add a picker-mode upload callback or picker-mode selection helper in the media app mounting API
- keep the API specific to picker mode so standalone Media App behavior is unchanged

### 3. Auto-select uploaded files on upload success

When an upload succeeds inside picker mode:

- resolve the uploaded file metadata returned by the upload flow
- add the uploaded file to picker-session selection if it is not already selected
- update the selection count immediately so the `Ok` button becomes enabled without further user action

Behavior rules:

- uploaded files are selected by default
- the user may still deselect them manually before confirming
- duplicate selections must not be introduced

### 4. Show newly uploaded files at the top in picker mode only

Do not change the global Media App sorting behavior.

Instead, when the app is mounted as a picker, render session-uploaded files before the normally filtered and sorted result set.

Recommended behavior:

- first render files uploaded in the current picker session
- then render the remaining files using the existing sort/filter logic
- preserve stable ordering among session-uploaded files, newest first

This keeps the requested UX local to the picker and avoids surprising users in the standalone media library.

### 5. Keep field components thin

`MediaFieldBasic.vue` and `MediaFieldGallery.vue` should continue to receive selected files through `MediaPickerModal` and should not implement upload-specific picker logic themselves.

This keeps the behavior centralized and reduces the chance that Basic and Gallery diverge again.

## Files Likely To Change

- `src/OrchardCore.Modules/OrchardCore.Media/Assets/media-app/src/main.ts`
- `src/OrchardCore.Modules/OrchardCore.Media/Assets/media-app/src/services/Globals.ts`
- `src/OrchardCore.Modules/OrchardCore.Media/Assets/media-app/src/services/UppyFileUpload.ts`
- `src/OrchardCore.Modules/OrchardCore.Media/Assets/media-app/src/composables/useFileListFiltering.ts`
- `src/OrchardCore.Modules/OrchardCore.Media/Assets/media-field/src/components/MediaPickerModal.vue`
- tests for both `media-app` and `media-field`

## Testing Plan

### Media App Tests

Add or update tests to cover:

- uploaded files are flagged as picker-session uploads
- uploaded files are auto-selected in picker mode
- selected count changes immediately after upload success
- session-uploaded files are rendered before the normal list ordering
- standalone media-app mode remains unchanged

### Media Field Tests

Add or update tests to cover:

- `MediaPickerModal` emits uploaded files through `select` without manual reselection
- `Ok` becomes enabled after upload success in picker mode
- Standard Media Field receives the uploaded selection correctly
- Gallery Media Field receives the uploaded selection correctly
- Attached Media Field behavior is unchanged

### Manual Verification

Verify the following scenarios in the admin UI:

1. Open Standard Media Field picker, upload a file, click `Ok`, confirm insertion without manual selection.
2. Open Gallery Media Field picker, upload multiple files, confirm all are preselected and inserted.
3. Deselect one of the newly uploaded files before clicking `Ok`, confirm only the remaining selected files are inserted.
4. Confirm the Attached Media Field still inserts uploaded files directly with no picker changes.
5. Confirm standalone Media library sorting is unchanged outside picker mode.

## Risks and Constraints

- The current media app sorting pipeline is generic, so adding picker-specific ordering must not leak into standalone mode.
- Upload success handling differs between XHR and TUS flows, so both paths must participate in the same picker-session behavior.
- Selection updates must avoid duplicate entries and must remain consistent with the existing `getSelectedFiles()` contract.
- The implementation should not require the media field components to understand media-app internal state.

## Acceptance Criteria

- In Standard and Gallery Media Fields, files uploaded during the current picker session are selected automatically.
- In Standard and Gallery Media Fields, newly uploaded files are shown first in the picker.
- Users can still deselect uploaded files before clicking `Ok`.
- Clicking `Ok` inserts the currently selected files without requiring a manual reselection step after upload.
- Attached Media Field behavior remains unchanged.
- Standalone Media App behavior remains unchanged.

## Proposed Delivery Order

1. Extend picker/media-app session state and contract.
2. Implement auto-selection on upload success for both XHR and TUS.
3. Implement picker-only uploaded-first ordering.
4. Add automated tests.
5. Perform manual admin verification.