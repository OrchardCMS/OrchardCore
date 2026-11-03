import { getTranslations, setTranslations } from "../helpers/localizations";

// OrchardCore.ContentFields' MultiTextField-Picker.Edit.cshtml: a static-options (no async
// search), taggable vue-multiselect - structurally distinct from the async-search family covered
// by multiselect-picker.ts (no draggable list, no server round-trip, options are supplied
// up-front from the field's settings).
declare const Vue: {
    createApp(options: Record<string, unknown>): { mount(selector: string | Element): void };
};

declare global {
    interface Window {
        ["vue-multiselect"]: { default: Record<string, unknown> };
    }
}

export interface MultiTextFieldPickerOption {
    value: string;
    name: string;
}

export interface MultiTextFieldPickerConfig {
    element: HTMLElement;
    selectedValues: MultiTextFieldPickerOption[];
    options: MultiTextFieldPickerOption[];
    valuesInputName: string;
    translations: Record<string, string>;
}

const initMultiTextFieldPicker = (config: MultiTextFieldPickerConfig): void => {
    setTranslations(config.translations);
    const t = getTranslations();

    const app = Vue.createApp({
        components: { "vue-multiselect": window["vue-multiselect"].default },
        data() {
            return {
                t,
                value: config.selectedValues,
                options: config.options,
            };
        },
        template: `
            <input v-for="v in value" :key="v.value" :name="'${config.valuesInputName}'" :value="v.value" type="hidden" />
            <vue-multiselect v-model="value"
                             :placeholder="t.TypeToSearch"
                             :select-label="t.Select"
                             :deselect-label="t.Remove"
                             track-by="value"
                             label="name"
                             :options="options"
                             :multiple="true"
                             :show-labels="false"
                             :close-on-select="false"
                             :taggable="true"
                             tag-position="bottom">
                <template v-slot:noOptions>
                    {{ t.NoValuesFound }}
                </template>
            </vue-multiselect>
        `,
    });

    app.mount(config.element);
};

export default initMultiTextFieldPicker;
