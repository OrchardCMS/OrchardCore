import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import {
    initReverseToggle,
    initCheckboxLink,
    initCheckboxCheckedLink,
    initCheckboxUncheckedLink,
} from "@orchardcore/bloom/components/checkbox-relations";

observeAndInit("[data-reversetoggle]", initReverseToggle);
observeAndInit("[data-checkbox]", initCheckboxLink);
observeAndInit("[data-checkboxchecked]", initCheckboxCheckedLink);
observeAndInit("[data-checkboxunchecked]", initCheckboxUncheckedLink);
