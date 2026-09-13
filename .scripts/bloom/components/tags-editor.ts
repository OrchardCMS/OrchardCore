import { getTranslations, setTranslations } from "../helpers/localizations";

// OrchardCore.Taxonomies' TaxonomyField-Tags.Edit.cshtml: a static-options taggable
// vue-multiselect with an inline "create new term" flow (POSTs to a Create Tag endpoint and adds
// the new term into both the full term list and the current selection). Structurally distinct
// from every other consumer migrated so far - closest relative is
// multitextfield-picker.ts (static options, taggable) but this one also needs the async tag
// creation, an isLeaf filter, and an authorization-gated "open" (freeform tagging) flag.
declare const Vue: {
    createApp(options: Record<string, unknown>): { mount(selector: string | Element): void };
};

declare global {
    interface Window {
        ["vue-multiselect"]: { default: Record<string, unknown> };
    }
}

export interface TagTerm {
    contentItemId: string;
    displayText: string;
    selected: boolean;
    isLeaf?: boolean;
}

export interface TagsEditorConfig {
    element: HTMLElement;
    allTagTerms: TagTerm[];
    open: boolean;
    leavesOnly: boolean;
    // Mirrors TaxonomyFieldSettings.Unique: true restricts selection to a single term
    // (multiple=false, closeOnSelect=true), false allows any number of terms.
    unique: boolean;
    taxonomyContentItemId: string;
    createTagUrl: string;
    createTagErrorMessage: string;
    hiddenInputId: string;
    hiddenInputName: string;
    translations: Record<string, string>;
    placeholder?: string;
}

interface RootInstance {
    allTagTerms: TagTerm[];
    selectedTagTerms: TagTerm[];
    taxonomyContentItemId: string;
    createTagUrl: string;
    createTagErrorMessage: string;
}

const initTagsEditor = (config: TagsEditorConfig): void => {
    setTranslations(config.translations);
    const t = getTranslations();

    // Mounts onto a dedicated inner element (class "tags-editor-mount"), not config.element
    // itself: Vue 3's mount() replaces the mounted node's content, and observeAndInit's shared
    // MutationObserver tracks config.element by identity to decide whether it has already been
    // initialized. Mounting directly onto config.element risks the same re-mount-loop hazard
    // documented on options-table-editor.ts's mountTarget and the original Vue 2 file's
    // ".tags-vue-root" convention - this is that same convention, renamed for the shared
    // bloom-component family.
    const mountTarget = config.element.querySelector<HTMLElement>(".tags-editor-mount") ?? config.element;

    let selectableTagTerms = config.allTagTerms;
    if (config.leavesOnly) {
        selectableTagTerms = selectableTagTerms.filter((term) => term.isLeaf);
        // Self-heal when the leaves-only setting is toggled on after some non-leaf terms were
        // already selected.
        config.allTagTerms.forEach((term) => {
            if (!selectableTagTerms.includes(term)) {
                term.selected = false;
            }
        });
    }

    const selectedTagTerms = config.allTagTerms.filter((term) => term.selected);

    Vue.createApp({
        components: { "vue-multiselect": window["vue-multiselect"].default },
        data() {
            return {
                t,
                allTagTerms: config.allTagTerms,
                selectableTagTerms,
                selectedTagTerms,
                taxonomyContentItemId: config.taxonomyContentItemId,
                createTagUrl: config.createTagUrl,
                createTagErrorMessage: config.createTagErrorMessage,
                placeholder: config.placeholder || t.TypeToSearch,
            };
        },
        computed: {
            isDisabled(this: { selectableTagTerms: TagTerm[] }) {
                return !config.open && this.selectableTagTerms.length === 0;
            },
            selectedTagTermsIds(this: { selectedTagTerms: TagTerm[] }) {
                return this.selectedTagTerms.map((term) => term.contentItemId);
            },
        },
        methods: {
            createTagTerm(this: RootInstance, newTagTerm: string) {
                const tokenInput = document.querySelector<HTMLInputElement>("input[name='__RequestVerificationToken']");
                fetch(this.createTagUrl, {
                    method: "POST",
                    headers: { "Content-Type": "application/x-www-form-urlencoded" },
                    body: new URLSearchParams({
                        __RequestVerificationToken: tokenInput?.value ?? "",
                        taxonomyContentItemId: this.taxonomyContentItemId,
                        displayText: newTagTerm,
                    }),
                })
                    .then((response) => {
                        if (!response.ok) {
                            throw new Error("Request failed");
                        }
                        return response.json();
                    })
                    .then((data: { contentItemId: string; displayText: string }) => {
                        const tagTerm: TagTerm = {
                            contentItemId: data.contentItemId,
                            displayText: data.displayText,
                            selected: true,
                        };
                        this.allTagTerms.push(tagTerm);
                        this.selectedTagTerms.push(tagTerm);
                    })
                    .catch(() => {
                        alert(this.createTagErrorMessage);
                    });
            },
            onSelect(this: RootInstance, selectedTagTerm: TagTerm) {
                const term = this.allTagTerms.find((t) => t.contentItemId === selectedTagTerm.contentItemId);
                if (term) {
                    term.selected = true;
                }
                document.dispatchEvent(new CustomEvent("contentpreview:render"));
            },
            onRemove(this: RootInstance, removedTagTerm: TagTerm) {
                const term = this.allTagTerms.find((t) => t.contentItemId === removedTagTerm.contentItemId);
                if (term) {
                    term.selected = false;
                }
                document.dispatchEvent(new CustomEvent("contentpreview:render"));
            },
        },
        template: `
            <input id="${config.hiddenInputId}" name="${config.hiddenInputName}" type="hidden" :value="selectedTagTermsIds.join(',')" />
            <vue-multiselect v-model="selectedTagTerms"
                        :placeholder="placeholder"
                        :select-label="t.Select"
                        :deselect-label="t.Remove"
                        :options="selectableTagTerms"
                        :multiple="${!config.unique}"
                        :show-labels="false"
                        :close-on-select="${config.unique}"
                        :disabled="isDisabled"
                        track-by="contentItemId"
                        label="displayText"
                        v-on:select="onSelect"
                        v-on:remove="onRemove"
                        :taggable="${config.open}"
                        tag-position="bottom"
                        :tag-placeholder="t.PressEnterToCreateTag"
                        v-on:tag="createTagTerm">
                <template v-slot:noResult>
                    {{ t.NoTagsFound }}
                </template>
                <template v-slot:noOptions>
                    {{ t.NoTagsFound }}
                </template>
            </vue-multiselect>
        `,
    }).mount(mountTarget);
};

export default initTagsEditor;
