import { getTranslations, setTranslations } from "../helpers/localizations";

// Shared by 3 near-identical Vue 2 "draggable selected-list + async-search multiselect" widgets
// that existed as separate copy-pasted files before this migration:
//   - OrchardCore.ContentFields' ContentPickerField.Edit.cshtml (badge: hasPublished/"Not
//     published"; only consumer with per-item clickable links via editUrl/viewUrl)
//   - OrchardCore.ContentFields' LocalizationSetContentPickerField.Edit.cshtml (badge:
//     hasPublished/"Not published"; no clickable links)
//   - OrchardCore.ContentFields' UserPickerField.Edit.cshtml (badge: isEnabled/"Not enabled"; no
//     clickable links; own debounce copy in the original Vue 2 file, now shared)
//
// Follows this repo's established plain-.ts bloom-component shape (Vue.createApp + template
// string, Options API) - see options-table-editor.ts, content-type-picker.ts,
// translation-editor.ts - rather than a .vue SFC.
//
// Display text goes through the IJSLocalizer / getTranslations()-setTranslations() pattern (see
// src/docs/reference/modules/Localize/javascript-localization.md) under the
// "content-fields-multiselect-picker" translation group (ContentFieldsJSLocalizer).
declare const Vue: {
    createApp(options: Record<string, unknown>): { mount(selector: string | Element): void };
};

declare global {
    interface Window {
        ["vue-multiselect"]: { default: Record<string, unknown> };
    }
}

// Loaded globally as the "vue-draggable" UMD resource - the Vue 3 build of vuedraggable. The UMD
// wrapper's trailing `})["default"]` unwraps webpack's `__webpack_exports__["default"]` one level
// before assigning to `root["vuedraggable"]`, so `window.vuedraggable` IS the component itself,
// not an ES-module-shaped `{ default: ... }` namespace object - confirmed by runtime inspection
// (see select-part-editor.ts's comment for the full story; `vuedraggable.default` reads as
// `undefined`, silently registering `draggable` as an unresolved component, so Vue renders the
// literal non-reactive `<draggable>` tag instead of the real one and drag/rows/click become
// permanent no-ops).
declare const vuedraggable: Record<string, unknown>;

export interface MultiselectPickerItem {
    id: string;
    displayText: string;
    isClickable?: boolean;
    [key: string]: unknown;
}

export interface MultiselectPickerClickableLinks {
    editUrl: string;
    viewUrl: string;
}

export interface MultiselectPickerConfig {
    element: HTMLElement;
    selectedItems: MultiselectPickerItem[];
    searchUrl: string;
    multiple: boolean;
    hiddenInputId: string;
    hiddenInputName: string;
    // Item property that flags an item as in an unpublished/disabled state, and the translation
    // key for the badge text shown next to such items (e.g. "hasPublished"/"NotPublished" for
    // content pickers, "isEnabled"/"NotEnabled" for the user picker).
    statusField: string;
    statusLabelKey: string;
    // Only ContentPickerField renders each selected item's displayText as a link (to an edit or
    // view route depending on item.isClickable); the other two consumers render plain text.
    clickableLinks?: MultiselectPickerClickableLinks;
    // Dispatched on document.body once mounted, carrying { vm: <app root instance> } - preserves
    // each original file's "hook for other scripts" integration point under its own event name.
    createdEventName: string;
    translations: Record<string, string>;
    // Overrides the "Type to search" placeholder text - only UserPickerField exposes a
    // per-field Placeholder setting that falls back to the localized default when blank.
    placeholder?: string;
}

interface RootInstance {
    arrayOfItems: MultiselectPickerItem[];
    options: MultiselectPickerItem[];
    isLoading: boolean;
    searchBoxContainer: Element | null;
    multiple: boolean;
    remove(item: MultiselectPickerItem): void;
    onSelect(selectedOption: MultiselectPickerItem): void;
    asyncFind(query?: string): void;
}

const debounce = <TArgs extends unknown[]>(fn: (...args: TArgs) => void, wait: number): ((...args: TArgs) => void) => {
    let timeout: ReturnType<typeof setTimeout> | null = null;
    return (...args: TArgs) => {
        if (timeout) {
            clearTimeout(timeout);
        }
        timeout = setTimeout(() => fn(...args), wait);
    };
};

const initMultiselectPicker = (config: MultiselectPickerConfig): void => {
    setTranslations(config.translations);
    const t = getTranslations();

    const debouncedSearch = debounce((vm: RootInstance, query?: string) => {
        vm.isLoading = true;
        let url = config.searchUrl;
        if (query) {
            url += `&query=${encodeURIComponent(query)}`;
        }
        fetch(url)
            .then((res) => res.json())
            .then((json) => {
                vm.options = json;
                vm.isLoading = false;
            });
    }, 250);

    const app = Vue.createApp({
        components: {
            draggable: vuedraggable,
            "vue-multiselect": window["vue-multiselect"].default,
        },
        data() {
            return {
                t,
                value: null as MultiselectPickerItem | null,
                arrayOfItems: config.selectedItems,
                options: [] as MultiselectPickerItem[],
                isLoading: false,
                searchBoxContainer: null as Element | null,
                placeholder: config.placeholder || t.TypeToSearch,
            };
        },
        computed: {
            selectedIds(this: { arrayOfItems: MultiselectPickerItem[] }) {
                return this.arrayOfItems.map((item) => item.id).join(",");
            },
            isDisabled(this: { arrayOfItems: MultiselectPickerItem[] }) {
                return this.arrayOfItems.length > 0 && !config.multiple;
            },
        },
        watch: {
            selectedIds() {
                // Delay lets the hidden <input> pick up the new value before the form submits.
                setTimeout(() => document.dispatchEvent(new CustomEvent("contentpreview:render")), 100);
            },
        },
        created(this: RootInstance) {
            this.asyncFind();
        },
        mounted(this: RootInstance & { $el: Element }) {
            // Stored so onSelect/remove can hide/show it without a full re-render; matches the
            // original Vue 2 code's identical mounted-hook comment/reasoning.
            this.searchBoxContainer = this.$el.lastElementChild;
            if (this.searchBoxContainer instanceof HTMLElement) {
                this.searchBoxContainer.style.display = config.multiple || this.arrayOfItems.length === 0 ? "block" : "none";
            }
        },
        methods: {
            asyncFind(this: RootInstance, query?: string) {
                debouncedSearch(this, query);
            },
            onSelect(this: RootInstance, selectedOption: MultiselectPickerItem) {
                if (this.arrayOfItems.some((item) => item.id === selectedOption.id)) {
                    return;
                }
                this.arrayOfItems.push(selectedOption);
                if (this.searchBoxContainer instanceof HTMLElement) {
                    this.searchBoxContainer.style.display = config.multiple ? "block" : "none";
                }
            },
            url(this: RootInstance, item: MultiselectPickerItem) {
                if (!config.clickableLinks) {
                    return "";
                }
                const base = item.isClickable ? config.clickableLinks.editUrl : config.clickableLinks.viewUrl;
                return base.replace("contentItemId", item.id);
            },
            remove(this: RootInstance, item: MultiselectPickerItem) {
                this.arrayOfItems.splice(this.arrayOfItems.indexOf(item), 1);
                if (this.searchBoxContainer instanceof HTMLElement) {
                    this.searchBoxContainer.style.display = "block";
                }
            },
        },
        template: `
            <ul class="mb-1 list-group w-100 content-picker-default__list" v-show="arrayOfItems.length" v-cloak>
                <draggable v-model="arrayOfItems" item-key="id" handle=".cursor-move">
                    <template #item="{ element: item }">
                        <li class="cursor-move list-group-item content-picker-default__list-item d-flex align-items-start justify-content-between">
                            <div class="align-items-center align-self-center">
                                <a v-if="${config.clickableLinks ? "item.isClickable" : "false"}" :href="url(item)" target="_blank">
                                    <span>{{ item.displayText }}</span> <span v-show="!item['${config.statusField}']" class="text-muted small">({{ t.${config.statusLabelKey} }})</span>
                                </a>
                                <span v-else>
                                    <span>{{ item.displayText }}</span> <span v-show="!item['${config.statusField}']" class="text-muted small">({{ t.${config.statusLabelKey} }})</span>
                                </span>
                            </div>
                            <div class="btn-group btn-group-sm align-items-center" role="group">
                                <button v-on:click="remove(item)" type="button" class="btn btn-secondary content-picker-default__list-item__delete"><i class="fa-solid fa-trash fa-sm" aria-hidden="true"></i></button>
                            </div>
                        </li>
                    </template>
                </draggable>
            </ul>
            <div class="w-100">
                <input id="${config.hiddenInputId}" name="${config.hiddenInputName}" type="hidden" :value="selectedIds" />
                <vue-multiselect v-model="value" :options="options" track-by="id"
                                 label="displayText" :placeholder="placeholder"
                                 v-on:search-change="asyncFind" v-on:select="onSelect"
                                 :searchable="true" :close-on-select="true" :reset-after="true"
                                 :show-labels="true" :hide-selected="multiple"
                                 :disabled="isDisabled"
                                 :select-label="t.Select" :deselect-label="t.Remove">
                    <template v-slot:option="props">
                        <div v-cloak><span>{{ props.option.displayText }}</span><span class="small ms-2" v-show="!props.option['${config.statusField}']">({{ t.${config.statusLabelKey} }})</span></div>
                    </template>
                    <template v-slot:noResult>
                        {{ t.NoResultFound }}
                    </template>
                </vue-multiselect>
            </div>
        `,
    });

    const vm = app.mount(config.element);

    document.body.dispatchEvent(new CustomEvent(config.createdEventName, { detail: { vm } }));
};

export default initMultiselectPicker;
