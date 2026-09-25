import { getTranslations, setTranslations } from "../helpers/localizations";

// OrchardCore.Localization's LocalizationSettings.Edit.cshtml: the site's supported-culture list
// editor - a plain radio-select table (pick the default culture) plus an add/remove list, no
// vue-multiselect or drag involved (structurally unrelated to every other "OptionsEditor"-named
// file migrated in Task 3 - see that task's investigation notes distinguishing this "Family B"
// widget from the unified options-table-editor.ts "Family A" widgets).
declare const Vue: {
    createApp(options: Record<string, unknown>): { mount(selector: string | Element): void };
};

export interface CultureOption {
    Name: string;
    DisplayName: string;
    Supported: boolean;
}

export interface CultureSettingsEditorConfig {
    element: HTMLElement;
    supportedCultures: string[];
    allCultures: CultureOption[];
    defaultCulture: string;
    selectedCulture: string;
    invariantCultureDisplayName: string;
    hiddenSupportedCulturesInputId: string;
    hiddenSupportedCulturesInputName: string;
    hiddenDefaultCultureInputId: string;
    hiddenDefaultCultureInputName: string;
    translations: Record<string, string>;
}

interface RootInstance {
    supportedCultures: string[];
    allCultures: CultureOption[];
    selectedCulture: string;
}

const initCultureSettingsEditor = (config: CultureSettingsEditorConfig): void => {
    setTranslations(config.translations);
    const t = getTranslations();

    Vue.createApp({
        data() {
            return {
                t,
                supportedCultures: config.supportedCultures,
                allCultures: config.allCultures,
                defaultCulture: config.defaultCulture,
                selectedCulture: config.selectedCulture,
            };
        },
        methods: {
            add(this: RootInstance) {
                if (this.supportedCultures.includes(this.selectedCulture)) {
                    return;
                }
                this.supportedCultures.push(this.selectedCulture);

                const culture = this.allCultures.find((c) => c.Name === this.selectedCulture);
                if (culture) {
                    culture.Supported = true;
                }

                const nextUnsupported = this.allCultures.find((c) => !c.Supported);
                if (nextUnsupported) {
                    this.selectedCulture = nextUnsupported.Name;
                }
            },
            remove(this: RootInstance, index: number) {
                const removedCultureName = this.supportedCultures[index];
                const removedCulture = this.allCultures.find((c) => c.Name === removedCultureName);
                if (removedCulture) {
                    removedCulture.Supported = false;
                }

                this.supportedCultures.splice(index, 1);

                const nextUnsupported = this.allCultures.find((c) => !c.Supported);
                if (nextUnsupported) {
                    this.selectedCulture = nextUnsupported.Name;
                }
            },
            getSupportedCultures(this: RootInstance) {
                return JSON.stringify(this.supportedCultures);
            },
        },
        template: `
            <table class="table border-bottom">
                <thead>
                    <tr>
                        <th>{{ t.Culture }}</th>
                        <th>{{ t.DefaultCulture }}</th>
                        <th>&nbsp;</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="(culture, index) in supportedCultures" :key="index">
                        <td>
                            <span v-if="culture !== ''">{{ culture }}</span>
                            <span v-if="culture === ''">${config.invariantCultureDisplayName}</span>
                            <input type="hidden" class="form-control" :value="culture" />
                        </td>
                        <td>
                            <div class="form-check ms-5">
                                <input type="radio" class="form-check-input" :id="'customRadio_' + index" :value="culture" v-model="defaultCulture">
                                <label class="form-check-label" :title="t.SetAsDefault" v-bind:for="'customRadio_' + index"></label>
                            </div>
                        </td>
                        <td class="text-end">
                            <a v-on:click="remove(index)" :title="t.RemoveCulture" href="javascript:void(0)" class="btn btn-secondary btn-sm" style="cursor:pointer">
                                <i class="fa-solid fa-trash" aria-hidden="true"></i>
                            </a>
                        </td>
                    </tr>
                </tbody>
            </table>

            <div class="row">
                <div class="col">
                    <select v-model="selectedCulture" class="form-select col">
                        <template v-for="(culture, index) in allCultures" :key="index">
                            <option v-if="!culture.Supported" :value="culture.Name">
                                {{ culture.Name }} ({{ culture.DisplayName }})
                            </option>
                        </template>
                    </select>
                </div>
                <div class="col">
                    <a v-on:click="add()" href="javascript:void(0)" class="btn btn-success"><i class="fa-solid fa-plus small" aria-hidden="true"></i> {{ t.AddCulture }}</a>
                </div>
            </div>

            <input class="form-control" id="${config.hiddenSupportedCulturesInputId}" name="${config.hiddenSupportedCulturesInputName}" type="hidden" :value="getSupportedCultures()" />
            <input class="form-control" id="${config.hiddenDefaultCultureInputId}" name="${config.hiddenDefaultCultureInputName}" type="hidden" :value="defaultCulture" />
        `,
    }).mount(config.element);
};

export default initCultureSettingsEditor;
