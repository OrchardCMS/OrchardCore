import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import initIconPickerTriggers from "@orchardcore/bloom/components/icon-picker-trigger";
import { initSelectPicker } from "@orchardcore/bloom/components/select-picker";

observeAndInit("select[data-permission-picker]", (element) => {
    initSelectPicker(element as HTMLSelectElement);
});

initIconPickerTriggers();
