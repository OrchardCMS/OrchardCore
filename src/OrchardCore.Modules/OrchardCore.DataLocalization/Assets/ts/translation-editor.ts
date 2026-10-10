import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";

declare const Vue: {
    createApp(options: Record<string, unknown>): { mount(selector: string): void };
};

interface TranslationString {
    context: string;
    key: string;
    value: string;
    plural?: string | null;
    values?: string[];
    metadata?: string[];
    formatArguments?: string[];
    changed?: boolean;
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
    incompletePlural: string;
    importError: string;
    imported: string;
    importFileRequired: string;
}

interface EditorInstance {
    cultures: Culture[];
    currentCulture: string;
    loadedCulture: string;
    providers: TranslationProvider[];
    pluralFormExamples: number[];
    isReadOnly: boolean;
    saveUrl?: string;
    getStringsUrl?: string;
    importUrl?: string;
    exportUrl?: string;
    isUiLocalization: boolean;
    isImporting: boolean;
    importFile: File | null;
    editRevision: number;
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
    getEntries(): TranslationString[];
    hasTranslation(str: TranslationString): boolean;
    hasInvalidChanges(): boolean;
    onTranslationChange(str: TranslationString): void;
    cancelAutoSave(): void;
    loadCulture(culture: string): Promise<boolean>;
    saveTranslations(): Promise<void>;
    showNotification(message: string, type: string): void;
    createToastContainer(): HTMLElement;
}

const editorEl = document.getElementById("translation-editor");

const readResponse = async (response: Response, fallbackMessage: string) => {
    if (!response.ok || response.redirected) {
        const body = response.headers.get("Content-Type")?.includes("application/json") ? await response.json() : null;
        throw new Error(body?.message || fallbackMessage);
    }
    return response.json();
};

if (editorEl) {
    const cultures = getDatasetJson<Culture[]>(editorEl, "cultures") ?? [];
    const providers = getDatasetJson<TranslationProvider[]>(editorEl, "providers") ?? [];
    const pluralFormExamples = getDatasetJson<number[]>(editorEl, "pluralFormExamples") ?? [];
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
                loadedCulture: editorEl.dataset.currentCulture || "",
                providers,
                pluralFormExamples,
                isReadOnly: editorEl.dataset.isReadOnly === "true",
                saveUrl: editorEl.dataset.saveUrl,
                getStringsUrl: editorEl.dataset.getStringsUrl,
                importUrl: editorEl.dataset.importUrl,
                exportUrl: editorEl.dataset.exportUrl,
                isUiLocalization: editorEl.dataset.uiLocalization === "true",
                isImporting: false,
                importFile: null as File | null,
                editRevision: 0,
                searchQuery: editorEl.dataset.search || "",
                categoryFilter: "",
                showMissingOnly: false,
                autoSave: true,
                isDirty: false,
                isLoading: false,
                isSaving: false,
                autoSaveTimeout: null as ReturnType<typeof setTimeout> | null,
            };
        },
        mounted(this: EditorInstance) {
            window.addEventListener("beforeunload", event => {
                if (this.isDirty) {
                    event.preventDefault();
                    event.returnValue = "";
                }
            });
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
                    result = result.filter((s: TranslationString) => !this.hasTranslation(s));
                }

                if (this.searchQuery) {
                    const query = this.searchQuery.toLowerCase();
                    result = result.filter(
                        (s: TranslationString) =>
                            s.key.toLowerCase().includes(query) || (s.value && s.value.toLowerCase().includes(query)) ||
                            (this.isUiLocalization && (
                                s.context.toLowerCase().includes(query) || s.plural?.toLowerCase().includes(query) ||
                                s.values?.some(value => value?.toLowerCase().includes(query)) ||
                                s.metadata?.some(value => value.toLowerCase().includes(query))
                            )),
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
            getTotalTranslatedCount(this: EditorInstance, provider: TranslationProvider) {
                let count = (provider.strings || []).filter((s) => this.hasTranslation(s)).length;
                if (provider.subGroups) {
                    count += provider.subGroups.reduce(
                        (sum, sg) => sum + (sg.strings || []).filter((s) => this.hasTranslation(s)).length,
                        0,
                    );
                }
                return count;
            },
            hasTranslation(this: EditorInstance, str: TranslationString) {
                return this.isUiLocalization
                    ? Boolean(str.values?.every(value => value?.length > 0))
                    : Boolean(str.value?.trim());
            },
            hasInvalidChanges(this: EditorInstance) {
                return this.isUiLocalization && this.getEntries().some(str =>
                    str.changed && str.values?.some(value => value?.length > 0) && !this.hasTranslation(str));
            },
            onTranslationChange(this: EditorInstance, str: TranslationString) {
                str.changed = true;
                this.editRevision++;
                this.isDirty = true;

                if (this.autoSave && !this.isReadOnly && this.canEditCurrentCulture) {
                    this.scheduleAutoSave();
                }
            },
            scheduleAutoSave(this: EditorInstance) {
                this.cancelAutoSave();

                this.autoSaveTimeout = setTimeout(() => {
                    if (this.autoSave && !this.hasInvalidChanges()) {
                        this.saveTranslations();
                    }
                }, 2000);
            },
            cancelAutoSave(this: EditorInstance) {
                if (this.autoSaveTimeout) {
                    clearTimeout(this.autoSaveTimeout);
                    this.autoSaveTimeout = null;
                }
            },
            restoreFallback(this: EditorInstance, str: TranslationString) {
                if (this.isReadOnly || !str.values) return;
                str.values = str.values.map(() => "");
                this.onTranslationChange(str);
            },
            getEntries(this: EditorInstance) {
                return this.providers.flatMap(provider => [
                    ...provider.strings,
                    ...(provider.subGroups || []).flatMap(group => group.strings),
                ]);
            },
            getAllStrings(this: EditorInstance) {
                return this.getEntries().filter(str => !this.isUiLocalization || str.changed).map(str =>
                    this.isUiLocalization
                        ? { context: str.context, key: str.key, plural: str.plural, value: "",
                            values: str.values?.every(value => !value) ? [] : [...(str.values || [])] }
                        : { context: str.context, key: str.key, value: str.value || "" });
            },
            async saveTranslations(this: EditorInstance) {
                if (this.isReadOnly || !this.canEditCurrentCulture || this.isSaving || this.isLoading || !this.isDirty) {
                    return;
                }
                this.cancelAutoSave();
                if (this.hasInvalidChanges()) {
                    this.showNotification(messages.incompletePlural, "danger");
                    return;
                }

                this.isSaving = true;
                const revision = this.editRevision;
                const changes = this.getEntries().filter(str => str.changed).map(str => ({
                    str, snapshot: JSON.stringify(str.values ?? str.value),
                }));

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

                    await readResponse(response, messages.failedDefault);
                    for (const { str, snapshot } of changes) {
                        if (snapshot === JSON.stringify(str.values ?? str.value)) {
                            str.changed = false;
                        }
                    }
                    this.isDirty = this.editRevision !== revision;
                    this.showNotification(messages.saved, "success");
                } catch (error) {
                    console.error("Save error:", error);
                    this.showNotification(error instanceof Error ? error.message : messages.savingError, "danger");
                } finally {
                    this.isSaving = false;
                    if (this.editRevision !== revision && this.autoSave) this.scheduleAutoSave();
                }
            },
            async onCultureChange(this: EditorInstance, event: Event) {
                const select = event.target as HTMLSelectElement;
                const culture = select.value;
                // Reset the native selection too: a cancelled v-model update can leave Vue's previous value unchanged.
                this.currentCulture = this.loadedCulture;
                select.value = this.loadedCulture;
                if (this.isSaving || this.isLoading || (this.isDirty && !confirm(messages.discardConfirm))) {
                    return;
                }
                this.cancelAutoSave();
                await this.loadCulture(culture);
            },
            async loadCulture(this: EditorInstance, culture: string) {
                this.cancelAutoSave();

                this.isLoading = true;

                try {
                    const response = await fetch(
                        `${this.getStringsUrl}?culture=${encodeURIComponent(culture)}`,
                    );
                    const data = await readResponse(response, messages.loadError);
                    this.providers = data.providers;
                    this.pluralFormExamples = data.pluralFormExamples ?? [];
                    this.loadedCulture = this.currentCulture = data.culture;
                    this.isDirty = false;
                    this.editRevision = 0;

                    const selected = this.cultures.find((c: Culture) => c.name === this.loadedCulture);
                    this.isReadOnly = !selected || !selected.canEdit;
                    return true;
                } catch (error) {
                    console.error("Load error:", error);
                    this.currentCulture = this.loadedCulture;
                    this.showNotification(error instanceof Error ? error.message : messages.loadError, "danger");
                    return false;
                } finally {
                    this.isLoading = false;
                }
            },
            onImportFileChange(this: EditorInstance, event: Event) {
                this.importFile = (event.target as HTMLInputElement).files?.[0] ?? null;
            },
            async importTranslations(this: EditorInstance) {
                if (this.isReadOnly || !this.canEditCurrentCulture || this.isSaving || this.isLoading) return;
                if (!this.importFile || this.importFile.size === 0 || this.importFile.size > 2 * 1024 * 1024) {
                    this.showNotification(messages.importFileRequired, "danger");
                    return;
                }
                if (this.isDirty && !confirm(messages.discardConfirm)) return;
                this.cancelAutoSave();
                this.isSaving = true;
                this.isImporting = true;
                const form = new FormData();
                form.append("culture", this.loadedCulture);
                form.append("file", this.importFile);
                form.append("__RequestVerificationToken", getAntiForgeryToken());
                try {
                    const response = await fetch(this.importUrl ?? "", { method: "POST", body: form });
                    await readResponse(response, messages.importError);
                    this.showNotification(messages.imported, "success");
                    this.isDirty = false;
                    if (!await this.loadCulture(this.loadedCulture)) {
                        this.isReadOnly = true;
                    }
                } catch (error) {
                    console.error("Import error:", error);
                    this.showNotification(error instanceof Error ? error.message : messages.importError, "danger");
                } finally {
                    this.isSaving = false;
                    this.isImporting = false;
                }
            },
            showNotification(this: EditorInstance, message: string, type: string) {
                const toastContainer =
                    document.querySelector<HTMLElement>(".toast-container") || this.createToastContainer();
                const toastId = "toast-" + Date.now();

                const toastHtml = `
                    <div id="${toastId}" class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                        <div class="d-flex">
                            <div class="toast-body"></div>
                            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                        </div>
                    </div>
                `;

                toastContainer.insertAdjacentHTML("beforeend", toastHtml);
                const toastEl = document.getElementById(toastId);
                if (!toastEl) return;
                const body = toastEl.querySelector<HTMLElement>(".toast-body");
                if (body) body.textContent = message;
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
