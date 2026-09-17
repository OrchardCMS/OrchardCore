import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";
import initCultureSettingsEditor, { CultureOption } from "@orchardcore/bloom/components/culture-settings-editor";

const wrapper = document.querySelector<HTMLElement>(".localization-settings-wrapper");

if (wrapper) {
    const supportedCultures = getDatasetJson<string[]>(wrapper, "supportedCultures") ?? [];
    const allCultures = getDatasetJson<CultureOption[]>(wrapper, "allCultures") ?? [];
    const translations = getDatasetJson<Record<string, string>>(wrapper, "translations");

    if (translations) {
        initCultureSettingsEditor({
            element: wrapper,
            supportedCultures,
            allCultures,
            defaultCulture: wrapper.dataset.defaultCulture ?? "",
            selectedCulture: wrapper.dataset.selectedCulture ?? "",
            invariantCultureDisplayName: wrapper.dataset.invariantCultureDisplayName ?? "",
            hiddenSupportedCulturesInputId: wrapper.dataset.supportedCulturesInputId ?? "",
            hiddenSupportedCulturesInputName: wrapper.dataset.supportedCulturesInputName ?? "",
            hiddenDefaultCultureInputId: wrapper.dataset.defaultCultureInputId ?? "",
            hiddenDefaultCultureInputName: wrapper.dataset.defaultCultureInputName ?? "",
            translations,
        });
    }
}
