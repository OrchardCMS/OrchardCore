import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { initRemotePicker } from "@orchardcore/bloom/components/remote-picker";
import { initSelectPicker } from "@orchardcore/bloom/components/select-picker";

// The content picker, localization set picker and user picker fields, and the content picker of the template
// inspector: a list of the selected items above a bootstrap-select searching them on the server.
observeAndInit("[data-remote-picker]", (element) => initRemotePicker(element));

// The Picker editor of the multi text field: a bootstrap-select of the options of the field.
observeAndInit("select[data-multi-text-field-picker]", (element) => {
    initSelectPicker(element as HTMLSelectElement);
});
