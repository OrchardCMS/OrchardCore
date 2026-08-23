import { getTranslations, setTranslations } from "@orchardcore/bloom/helpers/localizations";

// OrchardCore.Cors' Views/Admin/Index.cshtml: a CORS policy list/editor. Single consumer (unlike
// the shared bloom/components/* widgets elsewhere in this migration), so this stays a module-own
// .ts entry - matches the precedent set by Flows' content-type-picker.ts.
declare const Vue: {
    createApp(options: Record<string, unknown>): { mount(selector: string | Element): void };
};

export interface CorsPolicy {
    name: string;
    originalName?: string;
    allowedOrigins: string[];
    allowAnyOrigin: boolean;
    allowedMethods: string[];
    allowAnyMethod: boolean;
    allowedHeaders: string[];
    allowAnyHeader: boolean;
    allowCredentials: boolean;
    isDefaultPolicy: boolean;
    exposedHeaders: string[];
}

interface OptionsListInstance {
    options: string[];
    newOption: string;
}

interface CorsAppInstance {
    selectedPolicy: CorsPolicy | null;
    policies: CorsPolicy[];
}

// The list-editor used for each of a policy's string-array fields (allowed origins/methods/
// headers/exposed headers) - mutates the array reference passed down from the parent's
// selectedPolicy rather than emitting an update event, matching the original Vue 2 component's
// documented intentional prop-mutation (the parent never re-passes a new array).
const optionsListComponent = {
    props: {
        options: { type: Array, required: true },
        optionType: { type: String, required: true },
        title: { type: String, required: true },
        subTitle: { type: String, default: "" },
    },
    data() {
        return { t: getTranslations(), newOption: "" };
    },
    methods: {
        addOption(this: OptionsListInstance, value: string) {
            if (value !== null && value !== "") {
                const noDuplicates = this.options.map((o) => o.toLowerCase()).indexOf(value.toLowerCase()) < 0;
                if (noDuplicates) {
                    // eslint-disable-next-line vue/no-mutating-props -- see file header.
                    this.options.push(value);
                }
            }
            this.newOption = "";
        },
        deleteOption(this: OptionsListInstance, value: string) {
            // eslint-disable-next-line vue/no-mutating-props -- see file header.
            this.options.splice(this.options.indexOf(value), 1);
        },
    },
    template: `
        <div class="ocat-wrapper">
            <label class="ocat-label">{{ title }}</label>
            <div class="ocat-end">
                <span class="hint" v-if="subTitle">{{ subTitle }}</span>
                <div class="input-group input-group-sm mb-2 w-50">
                    <input type="text" class="form-control" v-model="newOption">
                    <button class="btn btn-primary btn-sm" type="button" v-on:click="addOption(newOption)">{{ t.Add }} {{ optionType }}</button>
                </div>
                <ul class="list-group mt-3" v-if="options.length > 0">
                    <li class="list-group-item d-flex" v-for="option in options" :key="option">
                        <div class="align-self-center me-auto">{{ option }}</div>
                        <div class="align-self-center">
                            <button type="button" class="btn btn-danger btn-sm" v-on:click="deleteOption(option)">{{ t.Delete }}</button>
                        </div>
                    </li>
                </ul>
            </div>
        </div>
    `,
};

const policyDetailsComponent = {
    components: { "options-list": optionsListComponent },
    props: { policy: { type: Object, required: true } },
    data() {
        return { t: getTranslations() };
    },
    template: `
        <div>
            <h3>{{ policy.name }}</h3>
            <div class="card mb-2">
                <div class="card-body">
                    <h5 class="card-title">{{ t.Details }}
                        <span class="hint dashed">{{ t.ProvidePolicyDetails }}</span>
                    </h5>
                    <div class="ocat-wrapper">
                        <label class="ocat-label">{{ t.PolicyName }}</label>
                        <div class="ocat-end">
                        <input v-model="policy.name" type="text" class="form-control" />
                        <span class="hint">{{ t.PolicyNameHint }}</span>
                        </div>
                    </div>
                    <div class="mb-3">
                        <div>
                            <div class="form-check">
                                <input class="form-check-input" id="set-default-policy" v-model="policy.isDefaultPolicy" type="checkbox">
                                <label class="form-check-label" for="set-default-policy">{{ t.SetAsDefaultPolicy }}</label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="card mb-2">
                <div class="card-body">
                    <h5 class="card-title">{{ t.Credentials }}
                        <span class="hint dashed">{{ t.ConfigureCredentialsBehavior }}</span>
                    </h5>
                    <div class="mb-3">
                        <div>
                            <div class="form-check">
                                <input class="form-check-input" id="allow-credentials" v-model="policy.allowCredentials" type="checkbox" />
                                <label class="form-check-label" for="allow-credentials">{{ t.AllowCredentials }}</label>
                                <span class="hint dashed">{{ t.AllowCredentialsHint }}</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="card mb-2">
                <div class="card-body">
                    <h5 class="card-title">{{ t.Origins }}
                        <span class="hint dashed">{{ t.ConfigureOriginsBehavior }}</span>
                    </h5>
                    <div class="">
                        <div class="mb-3">
                            <div>
                                <div class="form-check mb-2">
                                    <input class="form-check-input" id="allowed-origins" v-model="policy.allowAnyOrigin" type="checkbox" />
                                    <label class="form-check-label" for="allowed-origins">{{ t.AllowAnyOrigin }}</label>
                                    <span class="hint dashed">{{ t.AllowAnyOriginHint }}</span>
                                </div>
                            </div>
                        </div>
                        <options-list v-bind:options="policy.allowedOrigins" v-bind:option-type="t.Origin" v-bind:title="t.AllowedOrigins" sub-title="" />
                    </div>
                </div>
            </div>
            <div class="card mb-2">
                <div class="card-body">
                    <h5 class="card-title">{{ t.Headers }}
                        <span class="hint dashed">{{ t.AllowHeadersHint }}</span>
                    </h5>
                    <div class="">
                        <div class="mb-3">
                            <div>
                                <div class="form-check mb-2">
                                    <input class="form-check-input" id="allowed-headers" v-model="policy.allowAnyHeader" type="checkbox" />
                                    <label class="form-check-label" for="allowed-headers">{{ t.AllowAnyHeader }}</label>
                                    <span class="hint dashed">{{ t.AllowAnyHeaderHint }}</span>
                                </div>
                            </div>
                        </div>
                        <options-list v-bind:options="policy.allowedHeaders" v-bind:option-type="t.Header" v-bind:title="t.AllowedHeaders" sub-title="" />
                    </div>
                </div>
            </div>
            <div class="card mb-2">
                <div class="card-body">
                    <h5 class="card-title">{{ t.Methods }}
                        <span class="hint dashed">{{ t.ConfigureMethodsBehavior }}</span>
                    </h5>
                    <div class="">
                        <div class="mb-3">
                            <div>
                                <div class="form-check mb-2">
                                    <input class="form-check-input" id="allowed-methods" v-model="policy.allowAnyMethod" type="checkbox" />
                                    <label class="form-check-label" for="allowed-methods">{{ t.AllowAnyMethod }}</label>
                                    <span class="hint dashed">{{ t.AllowAnyMethodHint }}</span>
                                </div>
                            </div>
                        </div>
                        <options-list v-bind:options="policy.allowedMethods" v-bind:option-type="t.Method" v-bind:title="t.AllowedMethods" sub-title="" />
                    </div>
                </div>
            </div>
            <div class="card mb-3">
                <div class="card-body">
                    <h5 class="card-title">{{ t.ExposedHeaders }}
                        <span class="hint dashed">{{ t.ConfigureExposedHeaders }}</span>
                    </h5>
                    <div>
                        <span class="hint">{{ t.ExposedHeadersHint }}</span>
                        <options-list v-bind:options="policy.exposedHeaders" v-bind:option-type="t.Header" v-bind:title="t.ExposedHeaders" sub-title="" />
                    </div>
                </div>
            </div>
            <div class="mb-3">
                <div>
                    <button type="button" class="btn btn-primary" v-on:click="$emit('ok', policy, $event)">{{ t.Save }}</button>
                    <button type="button" class="btn btn-secondary" v-on:click="$emit('back')">{{ t.Cancel }}</button>
                </div>
            </div>
        </div>
    `,
};

const searchBox = () => {
    const searchBoxElement = document.getElementById("search-box");
    if (!searchBoxElement) {
        return;
    }

    // On Enter, edit the item if there is a single visible one.
    searchBoxElement.addEventListener("keydown", (e) => {
        if (e.key === "Enter") {
            const visible = Array.from(document.querySelectorAll<HTMLElement>("#corsAdmin > ul > li")).filter(
                (li) => li.style.display !== "none",
            );
            if (visible.length === 1) {
                const editLink = visible[0].querySelector<HTMLAnchorElement>(".edit");
                if (editLink) {
                    window.location.href = editLink.href;
                }
            }
            e.preventDefault();
        }
    });

    // On each keypress filter the list.
    searchBoxElement.addEventListener("keyup", (e) => {
        const search = (searchBoxElement as HTMLInputElement).value.toLowerCase();
        const elementsToFilter = document.querySelectorAll<HTMLElement>("[data-filter-value]");
        const listAlert = document.getElementById("list-alert");

        if (e.key === "Escape" || search === "") {
            (searchBoxElement as HTMLInputElement).value = "";
            elementsToFilter.forEach((el) => (el.style.display = ""));
            listAlert?.classList.add("d-none");
        } else {
            let visibleCount = 0;
            elementsToFilter.forEach((el) => {
                const text = (el.dataset.filterValue ?? "").toLowerCase();
                const found = text.indexOf(search) > -1;
                el.style.display = found ? "" : "none";
                if (found) {
                    visibleCount++;
                }
            });
            listAlert?.classList.toggle("d-none", visibleCount !== 0);
        }
    });
};

const corsSettingsElement = document.getElementById("corsSettings") as HTMLInputElement | null;
const corsFormElement = document.getElementById("corsForm") as HTMLFormElement | null;
const corsAdminElement = document.getElementById("corsAdmin");
const translationsElement = document.querySelector<HTMLElement>("[data-cors-translations]");

if (corsSettingsElement && corsFormElement && corsAdminElement && translationsElement) {
    const translations = JSON.parse(translationsElement.dataset.corsTranslations ?? "{}") as Record<string, string>;
    setTranslations(translations);

    Vue.createApp({
        components: { "policy-details": policyDetailsComponent, "options-list": optionsListComponent },
        data() {
            return {
                t: getTranslations(),
                selectedPolicy: null as CorsPolicy | null,
                policies: JSON.parse(corsSettingsElement.value || "[]") as CorsPolicy[],
            };
        },
        updated() {
            searchBox();
        },
        mounted() {
            searchBox();
        },
        methods: {
            newPolicy(this: CorsAppInstance) {
                this.selectedPolicy = {
                    name: "New policy",
                    allowedOrigins: [],
                    allowAnyOrigin: false,
                    allowedMethods: [],
                    allowAnyMethod: true,
                    allowedHeaders: [],
                    allowAnyHeader: true,
                    allowCredentials: true,
                    isDefaultPolicy: false,
                    exposedHeaders: [],
                };
            },
            editPolicy(this: CorsAppInstance, policy: CorsPolicy) {
                this.selectedPolicy = { ...policy, originalName: policy.name };
            },
            deletePolicy(this: CorsAppInstance & { save(): void }, policy: CorsPolicy, event: Event) {
                this.selectedPolicy = null;
                const policyToRemove = this.policies.find((item) => item.name === policy.name);
                if (policyToRemove) {
                    this.policies.splice(this.policies.indexOf(policyToRemove), 1);
                }
                event.stopPropagation();
                this.save();
            },
            updatePolicy(this: CorsAppInstance & { save(): void; back(): void }, policy: CorsPolicy) {
                if (policy.isDefaultPolicy) {
                    this.policies.forEach((p) => (p.isDefaultPolicy = false));
                }

                if (policy.originalName) {
                    const policyIndex = this.policies.findIndex((oldPolicy) => oldPolicy.name === policy.originalName);
                    this.policies[policyIndex] = policy;
                } else {
                    this.policies.push(policy);
                }

                this.save();
                this.back();
            },
            save(this: CorsAppInstance) {
                corsSettingsElement.value = JSON.stringify(this.policies);
                corsFormElement.submit();
            },
            back(this: CorsAppInstance) {
                this.selectedPolicy = null;
            },
        },
        template: `
            <div v-if="!selectedPolicy">
                <div class="card mb-3 text-bg-theme position-sticky action-bar">
                    <div class="card-body">
                        <div class="row gx-3">
                            <div class="col">
                                <div class="has-search">
                                    <i class="fa-solid fa-search form-control-feedback" aria-hidden="true"></i>
                                    <input id="search-box" class="form-control" :placeholder="t.Search" type="search" autofocus />
                                </div>
                            </div>
                            <div class="col-auto">
                                <button class="btn btn-secondary" type="button" v-on:click="newPolicy">{{ t.AddPolicy }}</button>
                            </div>
                        </div>
                    </div>
                </div>
                <ul class="list-group mb-2" v-cloak v-if="policies.length > 0">
                    <li class="list-group-item" v-for="policy in policies" :key="policy.name" :data-filter-value="policy.name">
                        <div class="d-flex">
                            <div class="align-self-center me-auto">{{ policy.name }}</div>
                            <div class="align-self-center">
                                <span v-if="policy.isDefaultPolicy" class="badge ta-badge">{{ t.DefaultPolicy }}</span>
                                <button class="btn btn-primary btn-sm" type="button" v-on:click="editPolicy(policy)">{{ t.Edit }}</button>
                                <button class="delete btn btn-danger btn-sm" type="button" v-on:click="deletePolicy(policy, $event)">{{ t.Delete }}</button>
                            </div>
                        </div>
                    </li>
                </ul>
                <div id="list-alert" class="alert alert-info" role="alert" v-if="policies.length == 0">
                    {{ t.NothingHere }}
                </div>
            </div>
            <policy-details v-if="selectedPolicy" v-bind:policy="selectedPolicy" v-on:ok="updatePolicy" v-on:back="back" />
            <div id="list-alert" class="alert alert-info d-none" role="alert">
                {{ t.NothingHereSearch }}
            </div>
        `,
    }).mount(corsAdminElement);
}
