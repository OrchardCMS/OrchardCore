<script setup lang="ts">
import { computed, reactive, useId, watch } from "vue";
import type { DesignerApi } from "../api/designerApi";
import type { JournalData, JournalRecord } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { formatDateTime } from "../draft/formatDateTime";
import { t } from "../i18n";

// The executions of an activity by the instance shown, from its journal. Each opens on what the activity evaluated,
// set and changed, which is loaded when it's opened (see WorkflowType.RecordActivityData).
const props = defineProps<{
    store: DesignerStore;
    api: DesignerApi;
    activityId: string;
}>();

const state = props.store.state;
const ids = `wfd-runs-${useId()}`;

const runs = computed(() => (state.instance?.journal ?? []).filter((record) => record.activityId === props.activityId));

// The runs that are open, and the data of each, by sequence.
const open = reactive(new Set<number>());
const data = reactive(new Map<number, JournalData | "loading" | "failed">());

const load = async (record: JournalRecord) => {
    if (!record.hasData || data.has(record.sequence)) {
        return;
    }

    data.set(record.sequence, "loading");

    try {
        data.set(record.sequence, await props.api.getJournalData(record.sequence));
    } catch {
        data.set(record.sequence, "failed");
    }
};

const toggle = (record: JournalRecord) => {
    if (open.has(record.sequence)) {
        open.delete(record.sequence);

        return;
    }

    open.add(record.sequence);
    void load(record);
};

// A run selected in the journal is opened, and scrolled to.
watch(
    () => state.focusedRunSequence,
    (sequence) => {
        const record = runs.value.find((run) => run.sequence === sequence);

        if (record && !open.has(record.sequence)) {
            open.add(record.sequence);
            void load(record);
        }

        if (record) {
            requestAnimationFrame(() => document.getElementById(`${ids}-${record.sequence}`)?.scrollIntoView({ block: "nearest" }));
        }
    },
    { immediate: true },
);

const statusClass = (record: JournalRecord) => (record.status === "Faulted" ? "text-bg-danger" : record.status === "Halted" ? "text-bg-info" : "text-bg-success");

// Direct t("…") calls, so the translations spec sees every key.
const statusLabel = (record: JournalRecord) => (record.status === "Faulted" ? t("JournalFaulted") : record.status === "Halted" ? t("JournalHalted") : t("JournalCompleted"));

const loaded = (record: JournalRecord) => {
    const value = data.get(record.sequence);

    return typeof value === "object" ? value : null;
};

// JSON values are shown indented, unless they were cut and are no longer valid JSON.
const pretty = (value: string | null | undefined) => {
    if (value === null || value === undefined) {
        return "";
    }

    try {
        const parsed = JSON.parse(value);

        return typeof parsed === "object" && parsed !== null ? JSON.stringify(parsed, null, 2) : value;
    } catch {
        return value;
    }
};

const entries = (values: Record<string, string> | undefined) => Object.entries(values ?? {});
</script>

<template>
    <div class="wfd-runs" data-cy="runs-tab">
        <p v-if="runs.length === 0" class="wfd-panel-message" data-cy="runs-empty">{{ t("NoRuns") }}</p>
        <ol v-else class="wfd-runs-list list-unstyled">
            <li v-for="record in runs" :id="`${ids}-${record.sequence}`" :key="record.sequence" class="wfd-run" :data-cy="`run-${record.sequence}`">
                <button
                    type="button"
                    class="wfd-run-head"
                    :class="{ 'is-focused': state.focusedRunSequence === record.sequence }"
                    :aria-expanded="open.has(record.sequence)"
                    :aria-controls="`${ids}-${record.sequence}-data`"
                    data-cy="run-toggle"
                    @click="toggle(record)"
                >
                    <i class="fa-solid fa-chevron-right wfd-run-chevron wfd-mirror-rtl" aria-hidden="true"></i>
                    <span class="wfd-journal-sequence">#{{ record.sequence }}</span>
                    <span class="badge" :class="statusClass(record)">{{ statusLabel(record) }}</span>
                    <span v-if="record.isResume" class="badge text-bg-secondary">{{ t("JournalResumed") }}</span>
                    <span v-if="record.outcomes.length > 0">{{ record.outcomes.join(", ") }}</span>
                    <time class="text-secondary" :datetime="record.startedUtc">{{ formatDateTime(record.startedUtc) }}</time>
                    <span class="text-secondary">{{ t("DurationMilliseconds", Math.round(record.durationMilliseconds)) }}</span>
                </button>

                <div v-if="open.has(record.sequence)" :id="`${ids}-${record.sequence}-data`" class="wfd-run-data" data-cy="run-data">
                    <p v-if="record.error" class="wfd-journal-error">{{ record.error }}</p>

                    <p v-if="!record.hasData" class="wfd-section-hint" data-cy="run-no-data">{{ t("RunNoData") }}</p>
                    <p v-else-if="data.get(record.sequence) === 'loading'" class="wfd-section-hint">
                        <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
                        {{ t("LoadingForm") }}
                    </p>
                    <p v-else-if="data.get(record.sequence) === 'failed'" class="text-danger" role="alert">{{ t("RunDataLoadFailed") }}</p>

                    <template v-else-if="loaded(record)">
                        <section v-if="loaded(record)!.evaluations.length > 0" data-cy="run-evaluations">
                            <h5 class="wfd-run-section">{{ t("RunExpressions") }}</h5>
                            <div v-for="(evaluation, index) in loaded(record)!.evaluations" :key="index" class="wfd-run-evaluation">
                                <div class="wfd-run-evaluation-head">
                                    <span v-if="evaluation.property" class="wfd-run-name">{{ evaluation.property }}</span>
                                    <span v-if="evaluation.syntax" class="badge wfd-output-type">{{ evaluation.syntax }}</span>
                                </div>
                                <pre class="wfd-run-code">{{ evaluation.expression }}</pre>
                                <div class="wfd-run-result">
                                    <i class="fa-solid fa-arrow-right wfd-mirror-rtl" aria-hidden="true"></i>
                                    <pre class="wfd-run-code" data-cy="run-evaluation-result">{{ pretty(evaluation.result) }}</pre>
                                </div>
                            </div>
                        </section>

                        <section
                            v-for="group in [
                                { key: 'outputs', title: t('Outputs'), values: loaded(record)!.outputs },
                                { key: 'variables', title: t('VariablesTab'), values: loaded(record)!.variables },
                                { key: 'properties', title: t('RunProperties'), values: loaded(record)!.properties },
                            ]"
                            v-show="entries(group.values).length > 0"
                            :key="group.key"
                            :data-cy="`run-${group.key}`"
                        >
                            <h5 class="wfd-run-section">{{ group.title }}</h5>
                            <dl class="wfd-run-values">
                                <template v-for="[name, value] in entries(group.values)" :key="name">
                                    <dt class="wfd-run-name">{{ name }}</dt>
                                    <dd>
                                        <pre class="wfd-run-code">{{ pretty(value) }}</pre>
                                    </dd>
                                </template>
                            </dl>
                        </section>

                        <section v-if="loaded(record)!.lastResult != null" data-cy="run-last-result">
                            <h5 class="wfd-run-section">{{ t("LastResult") }}</h5>
                            <pre class="wfd-run-code">{{ pretty(loaded(record)!.lastResult) }}</pre>
                        </section>

                        <p v-if="loaded(record)!.isTruncated" class="wfd-section-hint" data-cy="run-truncated">
                            <i class="fa-solid fa-scissors" aria-hidden="true"></i>
                            {{ t("RunDataTruncated") }}
                        </p>
                    </template>
                </div>
            </li>
        </ol>
    </div>
</template>
