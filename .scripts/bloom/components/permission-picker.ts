import { getTranslations, setTranslations } from "../helpers/localizations";

// Shared by OrchardCore.Menu's MenuItemPermissionPart.Edit.cshtml and OrchardCore.AdminMenu's
// LinkAdminNode/PlaceholderAdminNode.Fields.TreeEdit.cshtml - both views ship byte-identical
// X-Templates and Vue instances (menu-permission-picker.js / admin-menu-permission-picker.js),
// differing only in the CustomEvent name they dispatch afterward. One shared component replaces
// both copies, per the plan's Task 5 step 4.
//
// Follows this repo's established plain-.ts bloom-component shape (Vue.createApp + template
// string, Options API) - see options-table-editor.ts, content-type-picker.ts,
// translation-editor.ts - rather than a .vue SFC (no SFC compilation exists anywhere in this
// repo's shared bloom/ workspace). Display text goes through the IJSLocalizer /
// getTranslations()-setTranslations() pattern (see
// src/docs/reference/modules/Localize/javascript-localization.md) - each consuming module
// registers its own IJSLocalizer under the shared "permission-picker" translation group.
declare const Vue: {
    createApp(options: Record<string, unknown>): { mount(selector: string | Element): unknown };
};

// Loaded globally as the "vue-multiselect" UMD resource (vuejs:3 dependency chain, already
// vendored - see ResourceManagementOptionsConfiguration.cs). Its webpack UMD wrapper assigns
// itself to window["vue-multiselect"] (a hyphenated bracket key), NOT window.VueMultiselect -
// that global name was specific to the old Vue 2 build (vue-multiselect@2.1.6) this replaces;
// the component itself is on the `.default` property either way.
declare global {
    interface Window {
        ["vue-multiselect"]: { default: Record<string, unknown> };
    }
}

export interface PermissionPickerItem {
    name: string;
    displayText: string;
}

export interface PermissionPickerConfig {
    element: HTMLElement;
    selectedItems: PermissionPickerItem[];
    allItems: PermissionPickerItem[];
    hiddenInputId: string;
    hiddenInputName: string;
    // Rendered as a <span class="hint"> right after the multiselect, matching each consumer's
    // original X-Template position exactly (inside the Vue-owned markup, not a view-level
    // sibling) - passed in rather than hard-coded since each consumer's hint text differs.
    hintText: string;
    // Dispatched on document.body once mounted, carrying { vm: <app root instance> } - preserves
    // the "hook for other scripts" integration point each original file exposed under its own
    // event name (menu-permission-picker-created / admin-menu-permission-picker-created).
    createdEventName: string;
    // Raw translations payload from Orchard.GetJSLocalizations("permission-picker"), seeded into
    // the shared store via setTranslations() before this component reads any of it.
    translations: Record<string, string>;
}

interface RootInstance {
    arrayOfItems: PermissionPickerItem[];
    remove(item: PermissionPickerItem): void;
    onSelect(selectedOption: PermissionPickerItem): void;
}

const initPermissionPicker = (config: PermissionPickerConfig): void => {
    setTranslations(config.translations);
    const t = getTranslations();

    const app = Vue.createApp({
        components: { "vue-multiselect": window["vue-multiselect"].default },
        data() {
            return {
                t,
                value: null as PermissionPickerItem | null,
                arrayOfItems: config.selectedItems,
                options: config.allItems,
                hintText: config.hintText,
            };
        },
        computed: {
            selectedNames(this: { arrayOfItems: PermissionPickerItem[] }) {
                return this.arrayOfItems.map((item) => item.name).join(",");
            },
        },
        methods: {
            onSelect(this: RootInstance, selectedOption: PermissionPickerItem) {
                if (this.arrayOfItems.some((item) => item.name === selectedOption.name)) {
                    return;
                }
                this.arrayOfItems.push(selectedOption);
            },
            remove(this: RootInstance, item: PermissionPickerItem) {
                this.arrayOfItems.splice(this.arrayOfItems.indexOf(item), 1);
            },
        },
        template: `
            <ul class="mb-1 list-group w-xl-50 permission-picker-default__list" v-show="arrayOfItems.length">
                <li v-for="(item, i) in arrayOfItems"
                    class="list-group-item permission-picker-default__list-item d-flex align-items-start justify-content-between"
                    :key="item.name">
                    <div class="align-items-center align-self-center"><span>{{ item.displayText }}</span></div>
                    <div class="btn-group btn-group-sm align-items-center" role="group">
                        <button v-on:click="remove(item)" type="button" class="btn btn-secondary permission-picker-default__list-item__delete"><i class="fa-solid fa-trash fa-sm" aria-hidden="true"></i></button>
                    </div>
                </li>
            </ul>
            <div>
                <div class="w-xl-50">
                    <input id="${config.hiddenInputId}" name="${config.hiddenInputName}" type="hidden" :value="selectedNames" />
                    <vue-multiselect v-model="value"
                                     :options="options"
                                     track-by="name"
                                     label="displayText"
                                     :placeholder="t.TypeToSearch"
                                     v-on:select="onSelect"
                                     :searchable="true"
                                     :close-on-select="true"
                                     :reset-after="true"
                                     :show-labels="true"
                                     :hide-selected="false"
                                     :select-label="t.Select"
                                     :deselect-label="t.Remove">
                        <template v-slot:option="props">
                            <div v-cloak><span>{{ props.option.displayText }}</span></div>
                        </template>
                        <template v-slot:noResult>
                            {{ t.NoResultFound }}
                        </template>
                    </vue-multiselect>
                    <span class="hint">{{ hintText }}</span>
                </div>
            </div>
        `,
    });

    const vm = app.mount(config.element);

    document.body.dispatchEvent(new CustomEvent(config.createdEventName, { detail: { vm } }));
};

export default initPermissionPicker;
