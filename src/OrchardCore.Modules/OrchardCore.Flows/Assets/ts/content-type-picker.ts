import evalScripts from "@orchardcore/bloom/helpers/evalScripts";

declare const bootstrap: typeof import("bootstrap");

// Loaded globally as a UMD script (asp-name="vuejs" version="2"), not the npm "vue"
// package referenced elsewhere in this repo (v3, a different API) - typed narrowly to
// just what this file uses rather than pulling in a mismatched version's types.
declare const Vue: new (options: Record<string, unknown>) => VueInstance;

interface ContentTypePickerContentType {
    name: string;
    displayName: string;
    description?: string;
    category?: string;
    thumbnail?: string;
}

interface ContentTypePickerConfig {
    targetId: string;
    htmlFieldPrefix: string;
    prefixesName: string;
    contentTypesName: string;
    contentItemsName: string;
    parentContentType: string;
    partName: string;
    contentTypes: ContentTypePickerContentType[];
    flowmetadata?: boolean;
    cardCollectionType?: string;
    insertBefore?: Element;
    modalTitle?: string;
    pathBase?: string;
    onContentTypeSelected: (contentType: ContentTypePickerContentType, config: ContentTypePickerConfig) => void;
}

interface VueInstance {
    contentTypes: ContentTypePickerContentType[];
    categories: string[];
    searchFilter: string;
    selectedCategory: string;
    pathBase: string;
    currentConfig: ContentTypePickerConfig | null;
    hasError: boolean;
    readonly filteredContentTypes: ContentTypePickerContentType[];
    configure(config: ContentTypePickerConfig): void;
    selectCategory(category: string): void;
    selectContentType(contentType: ContentTypePickerContentType): void;
    getThumbnailUrl(contentType: ContentTypePickerContentType): string | null;
}

declare global {
    interface Window {
        initializeContentTypePickerApplication: (pathBase?: string) => void;
        showContentTypePicker: (config: ContentTypePickerConfig) => void;
        contentTypePickerSelectContentType: (contentType: ContentTypePickerContentType, config: ContentTypePickerConfig) => void;
    }
}

let contentTypePickerApp: VueInstance | undefined;
let contentTypePickerInitialized = false;
let sharedContentTypePickerModal: InstanceType<typeof bootstrap.Modal> | undefined;

window.initializeContentTypePickerApplication = function initializeContentTypePickerApplication(pathBase) {
    if (contentTypePickerInitialized) {
        return;
    }

    // Check if the Vue app element exists in the DOM.
    const appElement = document.getElementById("contentTypePickerApp");
    if (!appElement) {
        // Element not found - don't set initialized flag so it can be retried.
        return;
    }

    contentTypePickerInitialized = true;

    contentTypePickerApp = new Vue({
        el: "#contentTypePickerApp",
        data() {
            return {
                contentTypes: [] as ContentTypePickerContentType[],
                categories: [] as string[],
                searchFilter: "",
                selectedCategory: "All",
                pathBase: pathBase || "",
                currentConfig: null as ContentTypePickerConfig | null,
                hasError: false,
            };
        },
        computed: {
            filteredContentTypes(this: VueInstance): ContentTypePickerContentType[] {
                return this.contentTypes.filter((contentType) => {
                    // Filter by category.
                    // Items without a category are only shown when "All" is selected.
                    if (this.selectedCategory !== "All") {
                        if (!contentType.category || contentType.category !== this.selectedCategory) {
                            return false;
                        }
                    }
                    // Filter by search term.
                    if (this.searchFilter) {
                        const searchLower = this.searchFilter.toLowerCase();
                        const nameMatch = contentType.displayName.toLowerCase().indexOf(searchLower) >= 0;
                        const descMatch = !!contentType.description && contentType.description.toLowerCase().indexOf(searchLower) >= 0;
                        return nameMatch || descMatch;
                    }
                    return true;
                });
            },
        },
        methods: {
            configure(this: VueInstance, config: ContentTypePickerConfig) {
                this.currentConfig = config;
                this.contentTypes = config.contentTypes || [];

                // Build unique categories list.
                const categorySet: Record<string, true> = {};
                this.contentTypes.forEach((contentType) => {
                    if (contentType.category) {
                        categorySet[contentType.category] = true;
                    }
                });
                this.categories = ["All"].concat(Object.keys(categorySet).sort());

                // Reset filters and error state.
                this.searchFilter = "";
                this.selectedCategory = "All";
                this.hasError = false;
            },
            selectCategory(this: VueInstance, category: string) {
                this.selectedCategory = category;
            },
            selectContentType(this: VueInstance, contentType: ContentTypePickerContentType) {
                if (this.currentConfig && this.currentConfig.onContentTypeSelected) {
                    this.currentConfig.onContentTypeSelected(contentType, this.currentConfig);
                }
            },
            getThumbnailUrl(this: VueInstance, contentType: ContentTypePickerContentType): string | null {
                if (contentType.thumbnail) {
                    // Handle tilde paths.
                    if (contentType.thumbnail.startsWith("~/")) {
                        return this.pathBase + contentType.thumbnail.substring(1);
                    }
                    return contentType.thumbnail;
                }
                return null;
            },
        },
    });

    // Set up shared modal.
    const modalElement = document.getElementById("contentTypePickerModal");
    if (modalElement) {
        sharedContentTypePickerModal = new bootstrap.Modal(modalElement);

        // Blur focused element before modal hides to prevent aria-hidden warning.
        modalElement.addEventListener("hide.bs.modal", () => {
            if (document.activeElement instanceof HTMLElement && modalElement.contains(document.activeElement)) {
                document.activeElement.blur();
            }
        });

        // Reset state when modal is hidden.
        modalElement.addEventListener("hidden.bs.modal", () => {
            if (contentTypePickerApp) {
                contentTypePickerApp.searchFilter = "";
                contentTypePickerApp.selectedCategory = "All";
                contentTypePickerApp.currentConfig = null;
                contentTypePickerApp.hasError = false;
            }
        });
    }
};

// Remove any duplicate modals that may have been injected via AJAX.
function removeDuplicateModals() {
    const modals = document.querySelectorAll("#contentTypePickerModal");
    for (let i = 1; i < modals.length; i++) {
        modals[i].remove();
    }
}

// Show the shared modal with configuration.
window.showContentTypePicker = function (config) {
    // Remove any duplicate modals first.
    removeDuplicateModals();

    // Try to initialize if not already done (handles late DOM injection).
    if (!contentTypePickerInitialized) {
        window.initializeContentTypePickerApplication(config.pathBase || "");
    }

    if (contentTypePickerApp && sharedContentTypePickerModal) {
        contentTypePickerApp.configure(config);

        // Update modal title if provided.
        if (config.modalTitle) {
            const titleElement = document.getElementById("contentTypePickerModalLabel");

            if (titleElement) {
                titleElement.textContent = config.modalTitle;
            }
        }

        sharedContentTypePickerModal.show();
    }
};

// Hide the shared modal.
function hideContentTypePicker() {
    if (sharedContentTypePickerModal) {
        // Blur any focused element to prevent aria-hidden warning.
        if (document.activeElement instanceof HTMLElement) {
            document.activeElement.blur();
        }
        sharedContentTypePickerModal.hide();
    }
}

// Direct widget creation via AJAX.
window.contentTypePickerSelectContentType = function (contentType, config) {
    const targetId = config.targetId;
    const htmlFieldPrefix = config.htmlFieldPrefix;
    const target = document.getElementById(targetId);
    if (!target) {
        return;
    }

    const createEditorUrl = target.dataset.buildeditorurl;
    const form = target.closest("form");

    // Calculate next prefix index.
    const indexes = Array.from(form?.querySelectorAll<HTMLInputElement>("input[name*='Prefixes']") || [])
        .filter((e) => e.value.substring(0, e.value.lastIndexOf("-")) === htmlFieldPrefix)
        .map((e) => parseInt(e.value.substring(e.value.lastIndexOf("-") + 1)) || 0);

    const index = indexes.length ? Math.max(...indexes) + 1 : 0;
    const prefix = htmlFieldPrefix + "-" + index.toString();

    // Build URL with proper encoding.
    const params = new URLSearchParams({
        id: contentType.name,
        prefix: prefix,
        prefixesName: config.prefixesName,
        contentTypesName: config.contentTypesName,
        contentItemsName: config.contentItemsName,
        targetId: targetId,
        flowmetadata: (config.flowmetadata || false).toString(),
        parentContentType: config.parentContentType,
        partName: config.partName,
    });

    // Add optional cardCollectionType.
    if (config.cardCollectionType) {
        params.append("cardCollectionType", config.cardCollectionType);
    }

    fetch(createEditorUrl + "?" + params.toString())
        .then((response) => response.text())
        .then((data) => {
            const result = JSON.parse(data);

            // Insert before element if specified, otherwise append to target.
            if (config.insertBefore) {
                config.insertBefore.insertAdjacentHTML("beforebegin", result.Content);
            } else {
                target.insertAdjacentHTML("beforeend", result.Content);
            }

            evalScripts(result.Scripts);

            hideContentTypePicker();
        }, () => {
            if (contentTypePickerApp) {
                contentTypePickerApp.hasError = true;
            }
        });
};
