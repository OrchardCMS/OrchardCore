import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { initSelectPicker } from "@orchardcore/bloom/components/select-picker";

observeAndInit("select[data-permission-picker]", (element) => {
    initSelectPicker(element as HTMLSelectElement);
});
