import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";

declare const Vue: {
    createApp(options: Record<string, unknown>): { mount(selector: string): void };
};

interface TranslationString {
    context: string;
    key: string;
    value: string;
}

interface TranslationSubGroup {
    name: string;
    strings: TranslationString[];
}

interface TranslationProvider {
    name: string;
    strings: TranslationString[];
    subGroups?: TranslationSubGroup[];
}

interface Culture {
    name: string;
    displayName: string;
    canEdit: boolean;
}

interface TranslationMessages {
    saved: string;
    failedDefault: string;
    savingError: string;
    discardConfirm: string;
    loadError: string;
}

interface EditorInstance {
    cultures: Culture[];
    currentCulture: string;
    providers: TranslationProvider[];
    isReadOnly: boolean;
    saveUrl?: string;
    getStringsUrl?: string;
    searchQuery: string;
    categoryFilter: string;
    showMissingOnly: boolean;
    autoSave: boolean;
    isDirty: boolean;
    isLoading: boolean;
    isSaving: boolean;
    autoSaveTimeout: ReturnType<typeof setTimeout> | null;
    canEditCurrentCulture: boolean;
    scheduleAutoSave(): void;
    getFilteredStrings(strings: TranslationString[]): TranslationString[];
    getFilteredSubGroups(provider: TranslationProvider): TranslationSubGroup[];
    getAllStrings(): TranslationString[];
    saveTranslations(): Promise<void>;
    showNotification(message: string, type: string): void;
    createToastContainer(): HTMLElement;
}

const editorEl = document.getElementById("translation-editor");

if (editorEl) {
    const cultures = getDatasetJson<Culture[]>(editorEl, "cultures") ?? [];
    const providers = getDatasetJson<TranslationProvider[]>(editorEl, "providers") ?? [];
    const messages = getDatasetJson<TranslationMessages>(editorEl, "messages") ?? ({} as TranslationMessages);

    const getAntiForgeryToken = () => {
        const input = editorEl.querySelector<HTMLInputElement>('input[name="__RequestVerificationToken"]');
        return input ? input.value : "";
    };

    const { createApp } = Vue;

    const app = createApp({
        data() {
            return {
                cultures,
                currentCulture: editorEl.dataset.currentCulture || "",
                providers,
                isReadOnly: editorEl.dataset.isReadOnly === "true",
                saveUrl: editorEl.dataset.saveUrl,
                getStringsUrl: editorEl.dataset.getStringsUrl,
                searchQuery: "",
                categoryFilter: "",
                showMissingOnly: false,
                autoSave: true,
                isDirty: false,
                isLoading: false,
                isSaving: false,
                autoSaveTimeout: null as ReturnType<typeof setTimeout> | null,
            };
        },
        computed: {
            filteredProviders(this: EditorInstance) {
                let result = this.providers;

                if (this.categoryFilter) {
                    result = result.filter((p: TranslationProvider) => p.name === this.categoryFilter);
                }

                return result.filter(
                    (p: TranslationProvider) =>
                        this.getFilteredStrings(p.strings).length > 0 || this.getFilteredSubGroups(p).length > 0,
                );
            },
            canEditCurrentCulture(this: EditorInstance) {
                const culture = this.cultures.find((c: Culture) => c.name === this.currentCulture);
                return culture && culture.canEdit;
            },
        },
        methods: {
            getFilteredStrings(this: EditorInstance, strings: TranslationString[]) {
                if (!strings) return [];
                let result = strings;

                if (this.showMissingOnly) {
                    result = result.filter((s: TranslationString) => !s.value || s.value.trim() === "");
                }

                if (this.searchQuery) {
                    const query = this.searchQuery.toLowerCase();
                    result = result.filter(
                        (s: TranslationString) =>
                            s.key.toLowerCase().includes(query) || (s.value && s.value.toLowerCase().includes(query)),
                    );
                }

                return result;
            },
            getFilteredSubGroups(this: EditorInstance, provider: TranslationProvider) {
                if (!provider.subGroups) return [];

                return provider.subGroups.filter((sg) => this.getFilteredStrings(sg.strings).length > 0);
            },
            getTotalStringsCount(provider: TranslationProvider) {
                let count = (provider.strings || []).length;
                if (provider.subGroups) {
                    count += provider.subGroups.reduce((sum, sg) => sum + (sg.strings || []).length, 0);
                }
                return count;
            },
            getTotalTranslatedCount(provider: TranslationProvider) {
                let count = (provider.strings || []).filter((s) => s.value && s.value.trim() !== "").length;
                if (provider.subGroups) {
                    count += provider.subGroups.reduce(
                        (sum, sg) => sum + (sg.strings || []).filter((s) => s.value && s.value.trim() !== "").length,
                        0,
                    );
                }
                return count;
            },
            onTranslationChange(this: EditorInstance) {
                this.isDirty = true;

                if (this.autoSave && !this.isReadOnly && this.canEditCurrentCulture) {
                    this.scheduleAutoSave();
                }
            },
            scheduleAutoSave(this: EditorInstance) {
                if (this.autoSaveTimeout) {
                    clearTimeout(this.autoSaveTimeout);
                }

                this.autoSaveTimeout = setTimeout(() => {
                    this.saveTranslations();
                }, 2000);
            },
            getAllStrings(this: EditorInstance) {
                const translations: TranslationString[] = [];
                for (const provider of this.providers as TranslationProvider[]) {
                    if (provider.strings) {
                        for (const str of provider.strings) {
                            translations.push({ context: str.context, key: str.key, value: str.value || "" });
                        }
                    }
                    if (provider.subGroups) {
                        for (const subGroup of provider.subGroups) {
                            if (subGroup.strings) {
                                for (const str of subGroup.strings) {
                                    translations.push({ context: str.context, key: str.key, value: str.value || "" });
                                }
                            }
                        }
                    }
                }
                return translations;
            },
            async saveTranslations(this: EditorInstance) {
                if (this.isReadOnly || !this.canEditCurrentCulture || this.isSaving) {
                    return;
                }

                this.isSaving = true;

                try {
                    const translations = this.getAllStrings();

                    const response = await fetch(this.saveUrl ?? "", {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json",
                            RequestVerificationToken: getAntiForgeryToken(),
                        },
                        body: JSON.stringify({ culture: this.currentCulture, translations }),
                    });

                    if (response.ok) {
                        this.isDirty = false;
                        this.showNotification(messages.saved, "success");
                    } else {
                        const error = await response.json();
                        this.showNotification(error.message || messages.failedDefault, "danger");
                    }
                } catch (error) {
                    console.error("Save error:", error);
                    this.showNotification(messages.savingError, "danger");
                } finally {
                    this.isSaving = false;
                }
            },
            async onCultureChange(this: EditorInstance) {
                if (this.isDirty) {
                    if (!confirm(messages.discardConfirm)) {
                        return;
                    }
                }

                this.isLoading = true;
                this.isDirty = false;

                try {
                    const response = await fetch(
                        `${this.getStringsUrl}?culture=${encodeURIComponent(this.currentCulture)}`,
                    );
                    if (response.ok) {
                        const data = await response.json();
                        this.providers = data.providers;

                        const culture = this.cultures.find((c: Culture) => c.name === this.currentCulture);
                        this.isReadOnly = !culture || !culture.canEdit;
                    }
                } catch (error) {
                    console.error("Load error:", error);
                    this.showNotification(messages.loadError, "danger");
                } finally {
                    this.isLoading = false;
                }
            },
            showNotification(this: EditorInstance, message: string, type: string) {
                const toastContainer =
                    document.querySelector<HTMLElement>(".toast-container") || this.createToastContainer();
                const toastId = "toast-" + Date.now();

                const toastHtml = `
                    <div id="${toastId}" class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                        <div class="d-flex">
                            <div class="toast-body">${message}</div>
                            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                        </div>
                    </div>
                `;

                toastContainer.insertAdjacentHTML("beforeend", toastHtml);
                const toastEl = document.getElementById(toastId);
                if (!toastEl) return;
                const toast = new bootstrap.Toast(toastEl, { delay: 3000 });
                toast.show();

                toastEl.addEventListener("hidden.bs.toast", () => toastEl.remove());
            },
            createToastContainer() {
                const container = document.createElement("div");
                container.className = "toast-container position-fixed top-0 end-0 p-3";
                container.style.zIndex = "1100";
                document.body.appendChild(container);
                return container;
            },
        },
    });

    app.mount("#translation-editor");
}
