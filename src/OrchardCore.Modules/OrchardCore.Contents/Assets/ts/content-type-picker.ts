import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { onEditorUnmounting } from "@orchardcore/bloom/helpers/editorLifecycle";

// The global of the bootstrap-select resource (@crestapps/bootstrap-select).
interface Selectpicker {
    destroy(): void;
}

declare global {
    interface Window {
        Selectpicker?: {
            getOrCreateInstance(element: Element): Selectpicker;
        };
    }
}

// Turns the content type pickers (the "Picker" view of the SelectContentTypes view component) into a searchable
// multi-select that shows the selected types as tags, including the pickers injected after the page loaded, for
// example in the workflow designer. The select fires "change" itself when the selection changes.
observeAndInit("select[data-content-type-picker]", (select) => {
    if (!window.Selectpicker) {
        return;
    }

    const picker = window.Selectpicker.getOrCreateInstance(select);

    onEditorUnmounting(select, () => picker.destroy());
});
