<script setup lang="ts">
import { computed, onMounted, reactive, ref, useId } from "vue";
import ModalDialog from "../ui/ModalDialog.vue";
import NameValueRows, { type NameValueRow } from "./NameValueRows.vue";
import type { DesignerApi } from "../api/designerApi";
import { DesignerApiError } from "../api/designerApi";
import type { DesignerRun, HttpRunResponse, RunResult, VariableDefinition } from "../api/types";
import { t } from "../i18n";

// Runs the published version of the workflow: with the values of its input variables when it starts with Started by
// Workflow, or with a request to its URL when it starts with an HTTP Request event.
const props = defineProps<{
    api: DesignerApi;
    workflowTypeId: number;
    // Whether the draft has changes, which the run doesn't include.
    hasChanges: boolean;
}>();

const emit = defineEmits<{ (event: "close"): void }>();

const ids = `wfd-run-${useId()}`;
const loading = ref(true);
const running = ref(false);
const run = ref<DesignerRun | null>(null);
const error = ref<string | null>(null);
// The fields' values: a number input's v-model gives a number.
const values = reactive<Record<string, string | number | boolean>>({});
// The request: its query string and its body, as form fields or as text.
const queryRows = ref<NameValueRow[]>([]);
const formRows = ref<NameValueRow[]>([]);
const bodyKind = ref<"form" | "text">("text");
const body = ref("");
const contentType = ref("application/json");
const result = ref<RunResult | null>(null);
const response = ref<HttpRunResponse | null>(null);

// The longest response body shown.
const MAX_BODY_LENGTH = 5000;

const canRun = computed(() => !!run.value?.mode && run.value.isEnabled && !running.value);
const hasBody = computed(() => !!run.value?.httpMethod && !["GET", "HEAD"].includes(run.value.httpMethod.toUpperCase()));

const fieldKind = (variable: VariableDefinition) => {
    switch (variable.typeName) {
        case "boolean":
            return "boolean";
        case "number":
            return "number";
        case "object":
        case "array":
        case "any":
            return "json";
        default:
            return "text";
    }
};

onMounted(async () => {
    try {
        // The definition says how the published version runs, which a publish since the page loaded may have changed.
        run.value = (await props.api.getDefinition()).run ?? null;

        for (const variable of run.value?.inputs ?? []) {
            values[variable.name] = fieldKind(variable) === "boolean" ? false : "";
        }

        // The names the workflow reads from the request come first.
        queryRows.value = (run.value?.queryParameters ?? []).map((name) => ({ name, value: "", isKnown: true }));
        formRows.value = (run.value?.formFields ?? []).map((name) => ({ name, value: "", isKnown: true }));
        bodyKind.value = formRows.value.length > 0 ? "form" : "text";
    } catch (failure) {
        error.value = failure instanceof DesignerApiError ? failure.message : t("RunFailed");
    } finally {
        loading.value = false;
    }
});

// The typed values of the fields that have one; an empty field keeps the variable's default value.
const readInputs = (): Record<string, unknown> | null => {
    const inputs: Record<string, unknown> = {};

    for (const variable of run.value?.inputs ?? []) {
        const value = values[variable.name];
        const kind = fieldKind(variable);

        if (kind === "boolean") {
            inputs[variable.name] = value === true;
        } else if (typeof value === "number") {
            inputs[variable.name] = value;
        } else if (typeof value === "string" && value.trim() !== "") {
            if (kind === "number") {
                inputs[variable.name] = Number(value);
            } else if (kind === "json") {
                try {
                    inputs[variable.name] = JSON.parse(value);
                } catch {
                    if (variable.typeName === "any") {
                        inputs[variable.name] = value;
                    } else {
                        error.value = t("RunInvalidJson", variable.name);

                        return null;
                    }
                }
            } else {
                inputs[variable.name] = value;
            }
        }
    }

    return inputs;
};

const runWithInputs = async () => {
    const inputs = readInputs();

    if (inputs) {
        result.value = await props.api.run(inputs);
    }
};

// The rows that are sent: those with a name, except the ones the workflow reads that were left empty.
const encode = (rows: NameValueRow[]) =>
    new URLSearchParams(rows.filter((row) => row.name.trim() !== "" && (!row.isKnown || row.value !== "")).map((row) => [row.name.trim(), row.value])).toString();

const runWithRequest = async () => {
    const before = await props.api.getLatestInstance();
    const url = await props.api.generateHttpUrl(props.workflowTypeId, run.value!.activityId!);
    const search = encode(queryRows.value);
    const isForm = bodyKind.value === "form";

    response.value = await props.api.sendHttpRequest(
        search ? `${url}${url.includes("?") ? "&" : "?"}${search}` : url,
        run.value!.httpMethod ?? "GET",
        hasBody.value ? (isForm ? encode(formRows.value) : body.value) : null,
        isForm ? "application/x-www-form-urlencoded" : contentType.value,
    );

    // The instance the request started: the newest one, when it's not the one that was newest before.
    const after = await props.api.getLatestInstance();
    result.value = after.instanceId && after.instanceId !== before.instanceId ? after : null;
};

const start = async () => {
    error.value = null;
    result.value = null;
    response.value = null;
    running.value = true;

    try {
        await (run.value?.mode === "inputs" ? runWithInputs() : runWithRequest());
    } catch (failure) {
        error.value = failure instanceof DesignerApiError ? failure.message : t("RunFailed");
    } finally {
        running.value = false;
    }
};

const statusClass = (status: string | null | undefined) =>
    status === "Finished" ? "text-bg-success" : status === "Faulted" || status === "Aborted" ? "text-bg-danger" : status === "Halted" ? "text-bg-info" : "text-bg-secondary";

const responseBody = computed(() => {
    const text = response.value?.body ?? "";

    return text.length > MAX_BODY_LENGTH ? `${text.slice(0, MAX_BODY_LENGTH)}…` : text;
});

const outputs = computed(() => Object.entries(result.value?.outputs ?? {}));

const formatValue = (value: unknown) => (typeof value === "string" ? value : JSON.stringify(value, null, 2));
</script>

<template>
    <ModalDialog :title="t('RunTitle')" size="lg" data-cy="run-dialog" @close="emit('close')">
        <p v-if="loading" class="text-body-secondary">{{ t("RunLoading") }}</p>

        <template v-else-if="run">
            <p v-if="hasChanges" class="alert alert-info" data-cy="run-draft-note">{{ t("RunPublishedVersionNote") }}</p>

            <p v-if="!run.mode" class="alert alert-warning mb-0" data-cy="run-unavailable">{{ t("RunUnavailable") }}</p>
            <p v-else-if="!run.isEnabled" class="alert alert-warning" data-cy="run-disabled">{{ t("RunDisabled") }}</p>

            <form v-if="run.mode" :id="`${ids}-form`" class="wfd-run-form" @submit.prevent="start">
                <template v-if="run.mode === 'inputs'">
                    <p class="wfd-section-hint">{{ run.inputs.length > 0 ? t("RunInputsHint") : t("RunNoInputs") }}</p>
                    <div v-for="variable in run.inputs" :key="variable.name" class="mb-3" :data-cy="`run-input-${variable.name}`">
                        <div v-if="fieldKind(variable) === 'boolean'" class="form-check">
                            <input :id="`${ids}-${variable.name}`" v-model="values[variable.name]" type="checkbox" class="form-check-input" />
                            <label :for="`${ids}-${variable.name}`" class="form-check-label">{{ variable.name }}</label>
                        </div>
                        <template v-else>
                            <label :for="`${ids}-${variable.name}`" class="form-label">
                                {{ variable.name }} <span class="text-body-secondary small">{{ variable.typeName }}</span>
                            </label>
                            <textarea
                                v-if="fieldKind(variable) === 'json'"
                                :id="`${ids}-${variable.name}`"
                                v-model="values[variable.name] as string"
                                class="form-control font-monospace"
                                rows="3"
                                spellcheck="false"
                            ></textarea>
                            <input
                                v-else
                                :id="`${ids}-${variable.name}`"
                                v-model="values[variable.name] as string"
                                :type="fieldKind(variable) === 'number' ? 'number' : 'text'"
                                step="any"
                                class="form-control"
                            />
                        </template>
                        <div v-if="variable.description" class="form-text">{{ variable.description }}</div>
                    </div>
                </template>

                <template v-else>
                    <p class="wfd-section-hint">{{ t("RunRequestHint", run.httpMethod ?? "GET") }}</p>
                    <fieldset class="mb-3">
                        <legend class="form-label fs-6">{{ t("RunQueryString") }}</legend>
                        <p v-if="queryRows.length === 0" class="wfd-section-hint mb-1" data-cy="run-query-none">{{ t("RunNoQueryParameters") }}</p>
                        <NameValueRows v-model="queryRows" :label="t('RunQueryString')" cy="run-query" />
                    </fieldset>
                    <fieldset v-if="hasBody" class="mb-3">
                        <legend class="form-label fs-6">{{ t("RunBody") }}</legend>
                        <div class="btn-group btn-group-sm mb-2" role="radiogroup" :aria-label="t('RunBody')">
                            <input :id="`${ids}-body-form`" v-model="bodyKind" type="radio" class="btn-check" value="form" data-cy="run-body-form" />
                            <label class="btn btn-outline-secondary" :for="`${ids}-body-form`">{{ t("RunFormFields") }}</label>
                            <input :id="`${ids}-body-text`" v-model="bodyKind" type="radio" class="btn-check" value="text" data-cy="run-body-text" />
                            <label class="btn btn-outline-secondary" :for="`${ids}-body-text`">{{ t("RunBodyText") }}</label>
                        </div>
                        <NameValueRows v-if="bodyKind === 'form'" v-model="formRows" :label="t('RunFormFields')" cy="run-form" />
                        <template v-else>
                            <div class="mb-2">
                                <label :for="`${ids}-content-type`" class="form-label small">{{ t("RunContentType") }}</label>
                                <input
                                    :id="`${ids}-content-type`"
                                    v-model="contentType"
                                    type="text"
                                    class="form-control form-control-sm font-monospace"
                                    data-cy="run-content-type"
                                />
                            </div>
                            <textarea v-model="body" class="form-control font-monospace" rows="4" spellcheck="false" :aria-label="t('RunBodyText')" data-cy="run-body"></textarea>
                        </template>
                    </fieldset>
                </template>
            </form>
        </template>

        <p v-if="error" class="alert alert-danger mt-3 mb-0" role="alert" data-cy="run-error">{{ error }}</p>

        <section v-if="response || result" class="wfd-run-result" aria-live="polite" data-cy="run-result">
            <h3 class="wfd-section-title">{{ t("RunResult") }}</h3>
            <div v-if="response" class="mb-2" data-cy="run-response">
                {{ t("RunResponseStatus") }}
                <span class="badge" :class="response.status < 400 ? 'text-bg-success' : 'text-bg-danger'" data-cy="run-response-status">{{ response.status }}</span>
                <pre v-if="responseBody" class="wfd-run-response-body" data-cy="run-response-body">{{ responseBody }}</pre>
            </div>
            <template v-if="result">
                <p class="mb-2">
                    {{ t("RunInstanceStatus") }}
                    <span class="badge" :class="statusClass(result.status)" data-cy="run-status">{{ result.status }}</span>
                </p>
                <p v-if="result.faultMessage" class="wfd-journal-error" data-cy="run-fault">{{ result.faultMessage }}</p>
                <dl v-if="outputs.length > 0" class="wfd-run-outputs" data-cy="run-outputs">
                    <template v-for="[name, value] in outputs" :key="name">
                        <dt>{{ name }}</dt>
                        <dd>
                            <pre :data-cy="`run-output-${name}`">{{ formatValue(value) }}</pre>
                        </dd>
                    </template>
                </dl>
                <a v-if="result.instanceUrl" :href="result.instanceUrl" class="btn btn-sm btn-outline-primary" data-cy="run-open-instance">{{ t("RunOpenInstance") }}</a>
            </template>
            <p v-else-if="response" class="wfd-section-hint mb-0" data-cy="run-no-instance">{{ t("RunNoInstance") }}</p>
        </section>

        <template #footer>
            <button type="button" class="btn btn-secondary" data-cy="run-close" @click="emit('close')">{{ t("Close") }}</button>
            <button v-if="run?.mode" type="submit" :form="`${ids}-form`" class="btn btn-primary" data-autofocus :disabled="!canRun" data-cy="run-start">
                <span v-if="running" class="spinner-border spinner-border-sm" aria-hidden="true"></span>
                <i v-else class="fa-solid fa-play" aria-hidden="true"></i>
                {{ t("Run") }}
            </button>
        </template>
    </ModalDialog>
</template>
