<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from "vue";
import { loadScripts, loadStyles } from "@bloom/helpers/loadAssets";
import { dispatchEditorUnmounting } from "@bloom/helpers/editorLifecycle";
import type { FormFragment } from "../api/types";
import { AUTO_APPLY_DELAY, type FormApplyResult } from "./types";
import { t } from "../i18n";

// Hosts a server-rendered form (an activity editor or the workflow settings). The markup, scripts and
// styles come from the server, so the content is managed outside Vue's rendering: replacing it first
// dispatches "oc:editor-unmounting" so the editors' scripts can release what they created.
const props = withDefaults(
    defineProps<{
        formKey: string;
        load: () => Promise<FormFragment>;
        submit: (form: FormData) => Promise<FormApplyResult>;
        autoApplyDelay?: number;
        label: string;
    }>(),
    { autoApplyDelay: AUTO_APPLY_DELAY },
);

const emit = defineEmits<{
    (event: "loaded"): void;
    (event: "applied", result: FormApplyResult & { valid: true }): void;
    (event: "error", error: unknown): void;
}>();

const form = ref<HTMLFormElement | null>(null);
const content = ref<HTMLElement | null>(null);
const loading = ref(false);
const applying = ref(false);
const invalid = ref(false);
const dirty = ref(false);
const loadFailed = ref(false);

let timer: ReturnType<typeof setTimeout> | undefined;
let pending: Promise<boolean> | null = null;
let syncing = false;
let loadToken = 0;

// A field marked with data-wfd-reload changes the form itself (for example the workflow an Execute Workflow task
// runs, whose inputs it shows): the form is loaded again once the change is applied.
let reloadWhenApplied = false;

const clearContent = () => {
    if (content.value && content.value.childNodes.length > 0) {
        dispatchEditorUnmounting(content.value);
        content.value.innerHTML = "";
    }
};

const render = async (fragment: FormFragment) => {
    clearContent();
    loadStyles(fragment.styles);
    content.value!.innerHTML = fragment.content;
    await loadScripts(fragment.scripts);
};

const reload = async () => {
    const token = ++loadToken;

    clearTimeout(timer);
    reloadWhenApplied = false;
    loading.value = true;
    loadFailed.value = false;
    invalid.value = false;
    dirty.value = false;

    try {
        const fragment = await props.load();

        if (token === loadToken) {
            await render(fragment);
            emit("loaded");
        }
    } catch (error) {
        if (token === loadToken) {
            clearContent();
            loadFailed.value = true;
            emit("error", error);
        }
    } finally {
        if (token === loadToken) {
            loading.value = false;
        }
    }
};

const doApply = async () => {
    const element = form.value!;

    // CodeMirror and Monaco copy their value into their <textarea> on "submit". A synthetic event runs those
    // handlers without submitting anything.
    syncing = true;
    element.dispatchEvent(new Event("submit", { bubbles: true, cancelable: true }));
    syncing = false;

    const data = new FormData(element);
    applying.value = true;

    try {
        const result = await props.submit(data);

        if (!result.valid) {
            invalid.value = true;
            await render(result);

            return false;
        }

        invalid.value = false;
        dirty.value = false;
        emit("applied", result);

        if (reloadWhenApplied) {
            reloadWhenApplied = false;
            await reload();
        }

        return true;
    } catch (error) {
        emit("error", error);

        return false;
    } finally {
        applying.value = false;
    }
};

/**
 * Posts the form if it changed since it was loaded or last applied. Resolves to false when the form is
 * invalid or the request failed.
 */
const apply = (): Promise<boolean> => {
    clearTimeout(timer);

    if (pending) {
        return pending;
    }

    if (!dirty.value || loading.value) {
        return Promise.resolve(!invalid.value);
    }

    pending = doApply().finally(() => (pending = null));

    return pending;
};

// "change" fires when a field is committed (blur, select, checkbox), including from the rich editors.
const onChange = (event: Event) => {
    if (event.target instanceof Element && event.target.closest("[data-wfd-reload]")) {
        reloadWhenApplied = true;
    }

    dirty.value = true;
    clearTimeout(timer);
    timer = setTimeout(() => void apply(), props.autoApplyDelay);
};

const onInput = () => {
    dirty.value = true;
};

const onSubmit = (event: Event) => {
    event.preventDefault();

    if (!syncing) {
        void apply();
    }
};

watch(() => props.formKey, reload);

onMounted(reload);

onBeforeUnmount(() => {
    clearTimeout(timer);
    clearContent();
});

defineExpose({
    apply,
    reload,
    isDirty: () => dirty.value,
    isInvalid: () => invalid.value,
    /**
     * Forgets the unapplied changes, for example when the user chose to discard them.
     */
    discard: () => {
        clearTimeout(timer);
        dirty.value = false;
        invalid.value = false;
    },
});
</script>

<template>
    <div class="wfd-form-host">
        <div v-if="loading" class="wfd-panel-message">
            <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
            {{ t("LoadingForm") }}
        </div>
        <div v-if="loadFailed" class="alert alert-danger m-2" role="alert">{{ t("FormLoadFailed") }}</div>
        <form
            v-show="!loading && !loadFailed"
            ref="form"
            class="wfd-form"
            novalidate
            :aria-label="label"
            :aria-busy="applying"
            @change="onChange"
            @input="onInput"
            @submit="onSubmit"
        >
            <div ref="content" class="wfd-form-content"></div>
            <div class="wfd-form-actions">
                <span class="wfd-form-status" role="status" aria-live="polite">
                    <template v-if="applying">{{ t("Applying") }}</template>
                    <span v-else-if="invalid" class="text-danger">{{ t("FormHasErrors") }}</span>
                </span>
                <button type="submit" class="btn btn-sm btn-primary" :disabled="applying" data-cy="panel-apply">{{ t("Apply") }}</button>
            </div>
        </form>
    </div>
</template>
