import { getTranslations, setTranslations } from "../helpers/localizations";

// OrchardCore.Shortcodes' Create.cshtml / Edit.cshtml Categories field: a static-options taggable
// vue-multiselect, structurally close to multitextfield-picker.ts but adds its own `addCategory`
// (push a freshly-typed tag into both the options list and the current selection) and exposes a
// `getSelectedCategories()` method the surrounding form reads via a hidden input's
// v-bind:value - preserved here as-is since the original Vue 2 markup calls it directly in the
// view rather than through a v-model binding.
declare const Vue: {
    createApp(options: Record<string, unknown>): { mount(selector: string | Element): void };
};

declare global {
    interface Window {
        ["vue-multiselect"]: { default: Record<string, unknown> };
    }
}

export interface ShortcodeCategoriesConfig {
    element: HTMLElement;
    allCategories: string[];
    selectedCategories: string[];
    hiddenInputId: string;
    hiddenInputName: string;
    translations: Record<string, string>;
}

interface RootInstance {
    options: string[];
    value: string[];
}

const initShortcodeCategoriesEditor = (config: ShortcodeCategoriesConfig): void => {
    setTranslations(config.translations);
    const t = getTranslations();

    // Mounts onto a dedicated inner element (class "shortcode-categories-mount"), not
    // config.element itself: config.element is the shared ocat-wrapper div that also holds the
    // sibling, non-Vue-owned <label> - see options-table-editor.ts's mountTarget for the same
    // reasoning/hazard (Vue 3's mount() with an explicit template replaces the target's entire
    // content, unlike Vue 2's el-based in-place DOM compilation the original file relied on).
    const mountTarget = config.element.querySelector<HTMLElement>(".shortcode-categories-mount") ?? config.element;

    Vue.createApp({
        components: { "vue-multiselect": window["vue-multiselect"].default },
        data() {
            return {
                t,
                value: config.selectedCategories,
                options: config.allCategories,
            };
        },
        methods: {
            getSelectedCategories(this: RootInstance) {
                return JSON.stringify(this.value);
            },
            addCategory(this: RootInstance, category: string) {
                this.options.push(category);
                this.value.push(category);
            },
        },
        template: `
            <input id="${config.hiddenInputId}" name="${config.hiddenInputName}" class="form-control" type="hidden" v-bind:value="getSelectedCategories()" />
            <vue-multiselect v-model="value"
                             :placeholder="t.TypeToSearch"
                             :select-label="t.Select"
                             :deselect-label="t.Remove"
                             :options="options"
                             :multiple="true"
                             :show-labels="false"
                             :close-on-select="false"
                             :clear-on-select="false"
                             :taggable="true"
                             tag-position="bottom"
                             :tag-placeholder="t.PressEnterToAddCategory"
                             v-on:tag="addCategory">
                <template v-slot:noResult>
                    {{ t.NoCategoriesFound }}
                </template>
                <template v-slot:noOptions>
                    {{ t.NoCategoriesFound }}
                </template>
            </vue-multiselect>
        `,
    }).mount(mountTarget);
};

export default initShortcodeCategoriesEditor;
