<script setup lang="ts">
import { computed, onMounted } from "vue";
import type { DesignerNode, DesignerRun, RunStatus } from "../api/types";
import type { RunTracker } from "../runs/runTracker";
import { fileCount, isActiveRun, rowCount, runStatusLabel, stepStatusLabel } from "../runs/runOverlay";
import { formatDateTime } from "../draft/formatDateTime";
import { showToast } from "../ui/toasts";
import { t } from "../i18n";

// The most recent runs of the pipeline. Selecting one shows the status and the counts of its steps on the canvas, and
// its deliveries and log here; a queued or running run is fetched again every few seconds.
const props = defineProps<{
    tracker: RunTracker;
    nodes: DesignerNode[];
    // Whether "Run now" is offered, and why it is disabled (null when it isn't).
    canRun: boolean;
    runDisabledReason: string | null;
    starting: boolean;
    runsPageUrl?: string | null;
}>();

const emit = defineEmits<{
    (event: "run"): void;
    (event: "select-step", stepId: string): void;
}>();

const runs = computed(() => props.tracker.state.runs);
const selected = computed<DesignerRun | null>(() => runs.value.find((run) => run.runId === props.tracker.state.selectedRunId) ?? null);

const STATUS_CLASSES: Record<RunStatus, string> = {
    Queued: "text-bg-secondary",
    Running: "text-bg-primary",
    Succeeded: "text-bg-success",
    Failed: "text-bg-danger",
    Cancelled: "text-bg-warning",
};

const stepTitle = (stepId: string | null | undefined, fallback = "") => props.nodes.find((node) => node.id === stepId)?.displayText ?? fallback;

const select = (run: DesignerRun) => props.tracker.select(props.tracker.state.selectedRunId === run.runId ? null : run.runId);

const cancel = async (run: DesignerRun) => {
    try {
        await props.tracker.cancel(run.runId);
        showToast({ message: t("RunCancelRequested", "The run is being cancelled."), variant: "info" });
    } catch {
        showToast({ message: t("RunCancelFailed", "The run could not be cancelled."), variant: "danger" });
    }
};

const duration = (run: DesignerRun) => {
    if (!run.startedUtc || !run.completedUtc) {
        return null;
    }

    const seconds = Math.max(0, Math.round((Date.parse(run.completedUtc) - Date.parse(run.startedUtc)) / 1000));

    return seconds < 60 ? t("DurationSeconds", "{0} s", seconds) : t("DurationMinutes", "{0} min {1} s", Math.floor(seconds / 60), seconds % 60);
};

onMounted(() => {
    if (!props.tracker.state.loaded) {
        void props.tracker.refresh();
    }
});
</script>

<template>
    <div class="dpd-runs" data-cy="runs-tab">
        <div class="dpd-runs-actions">
            <button
                v-if="canRun || runDisabledReason"
                type="button"
                class="btn btn-sm btn-success"
                :disabled="!canRun || starting"
                :title="runDisabledReason ?? t('RunNowHint', 'Runs the published version now.')"
                data-cy="runs-run-now"
                @click="emit('run')"
            >
                <span v-if="starting" class="spinner-border spinner-border-sm" aria-hidden="true"></span>
                <i v-else class="fa-solid fa-play" aria-hidden="true"></i>
                {{ t("RunNow", "Run now") }}
            </button>
            <button type="button" class="btn btn-sm btn-outline-secondary" :disabled="tracker.state.loading" data-cy="runs-refresh" @click="tracker.refresh()">
                <i class="fa-solid fa-rotate" :class="{ 'fa-spin': tracker.state.loading }" aria-hidden="true"></i>
                {{ t("Refresh", "Refresh") }}
            </button>
            <a v-if="runsPageUrl" :href="runsPageUrl" class="btn btn-sm btn-link ms-auto" data-cy="runs-all">{{ t("RunHistory", "Run history") }}</a>
        </div>
        <p v-if="runDisabledReason" class="dpd-panel-intro pt-0" data-cy="runs-disabled-reason">{{ runDisabledReason }}</p>

        <p v-if="tracker.state.error" class="alert alert-danger m-2" role="alert">{{ t("RunsLoadFailed", "The runs could not be loaded.") }}</p>
        <p v-else-if="tracker.state.loaded && runs.length === 0" class="dpd-panel-message" data-cy="runs-empty">{{ t("NoRuns", "The pipeline has not run yet.") }}</p>

        <ul class="list-group list-group-flush dpd-run-list" :aria-label="t('Runs', 'Runs')">
            <li v-for="run in runs" :key="run.runId" class="list-group-item p-0">
                <button
                    type="button"
                    class="dpd-run-head"
                    :class="{ 'is-selected': run.runId === tracker.state.selectedRunId }"
                    :aria-pressed="run.runId === tracker.state.selectedRunId"
                    :data-cy="`run-${run.runId}`"
                    @click="select(run)"
                >
                    <span class="badge" :class="STATUS_CLASSES[run.status]">
                        <i v-if="isActiveRun(run)" class="fa-solid fa-circle-notch fa-spin" aria-hidden="true"></i>
                        {{ runStatusLabel(run.status) }}
                    </span>
                    <span class="dpd-run-when">{{ formatDateTime(run.startedUtc ?? run.queuedUtc) }}</span>
                    <span class="dpd-run-meta">
                        {{ t("VersionNumber", "Version {0}", run.versionNumber) }} · {{ run.triggeredBy ? t("TriggerBy", "{0} by {1}", run.trigger, run.triggeredBy) : run.trigger }}
                    </span>
                </button>

                <div v-if="run.runId === tracker.state.selectedRunId && selected" class="dpd-run-details" data-cy="run-details">
                    <p v-if="selected.error" class="dpd-run-error" data-cy="run-error">
                        <i class="fa-solid fa-circle-xmark" aria-hidden="true"></i>
                        <template v-if="selected.failedStepId">{{ stepTitle(selected.failedStepId) }}: </template>{{ selected.error }}
                    </p>
                    <p v-if="duration(selected)" class="small text-body-secondary mb-2">{{ t("RunDuration", "Took {0}.", duration(selected)!) }}</p>
                    <div class="d-flex gap-2 mb-2">
                        <button v-if="selected.canCancel" type="button" class="btn btn-sm btn-outline-danger" :disabled="tracker.state.cancelling" data-cy="run-cancel" @click="cancel(selected)">
                            <i class="fa-solid fa-stop" aria-hidden="true"></i>
                            {{ t("CancelRun", "Cancel the run") }}
                        </button>
                        <a :href="selected.url" class="btn btn-sm btn-link" data-cy="run-page">{{ t("OpenRun", "Open the run") }}</a>
                    </div>

                    <h4 class="dpd-section-title">{{ t("RunSteps", "Steps") }}</h4>
                    <table class="table table-sm dpd-run-steps mb-2">
                        <thead>
                            <tr>
                                <th scope="col">{{ t("Step", "Step") }}</th>
                                <th scope="col">{{ t("Status", "Status") }}</th>
                                <th scope="col" class="text-end">{{ t("Produced", "Produced") }}</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="step in selected.steps" :key="step.stepId" :class="{ 'table-danger': step.status === 'Failed' }">
                                <td>
                                    <button type="button" class="btn btn-link btn-sm p-0 text-start" :disabled="!nodes.some((node) => node.id === step.stepId)" @click="emit('select-step', step.stepId)">
                                        {{ stepTitle(step.stepId, step.title) }}
                                    </button>
                                    <div v-if="step.error" class="small text-danger">{{ step.error }}</div>
                                </td>
                                <td class="text-nowrap">
                                    {{ stepStatusLabel(step.status) }}
                                    <span v-if="step.warnings > 0" class="badge text-bg-warning" :title="t('WarningCount', '{0} warnings', step.warnings)">{{ step.warnings }}</span>
                                </td>
                                <td class="text-end text-nowrap">
                                    {{ rowCount(step.rowsOut) }}<template v-if="step.filesOut > 0"><br />{{ fileCount(step.filesOut) }}</template>
                                </td>
                            </tr>
                        </tbody>
                    </table>

                    <template v-if="selected.deliveries.length > 0">
                        <h4 class="dpd-section-title">{{ t("Deliveries", "Deliveries") }}</h4>
                        <ul class="dpd-run-deliveries" data-cy="run-deliveries">
                            <li v-for="(delivery, index) in selected.deliveries" :key="index">
                                <a v-if="delivery.url" :href="delivery.url" target="_blank" rel="noopener">{{ delivery.description }}</a>
                                <span v-else>{{ delivery.description }}</span>
                                <span class="small text-body-secondary"> · {{ formatDateTime(delivery.deliveredUtc) }}</span>
                            </li>
                        </ul>
                    </template>

                    <template v-if="selected.log.length > 0">
                        <h4 class="dpd-section-title">{{ t("RunLog", "Log") }}</h4>
                        <ol class="dpd-run-log" data-cy="run-log">
                            <li v-for="(entry, index) in selected.log" :key="index" :class="`is-${entry.level.toLowerCase()}`">
                                <time :datetime="entry.utc">{{ formatDateTime(entry.utc) }}</time>
                                <span v-if="entry.stepId" class="dpd-run-log-step">{{ stepTitle(entry.stepId, entry.stepId) }}</span>
                                <span class="dpd-run-log-message">{{ entry.message }}</span>
                            </li>
                        </ol>
                    </template>
                </div>
            </li>
        </ul>
    </div>
</template>
