<script setup lang="ts">
import { computed, ref } from "vue";
import type { DesignerApi } from "../api/designerApi";
import type { PreviewPort, PreviewResult } from "../api/types";
import { buildPreviewTable, formatFileSize } from "../preview/previewTable";
import { formatNumber, rowCount } from "../runs/runOverlay";
import { t } from "../i18n";

// The Data tab: the first rows the step produces (or, for a destination, receives), computed on demand from the draft.
const props = defineProps<{
    stepId: string;
    api: DesignerApi;
    // Applies the open forms and saves the graph first; resolves to false when that failed.
    prepare: () => Promise<boolean>;
}>();

const loading = ref(false);
const failed = ref(false);
const result = ref<PreviewResult | null>(null);

const ports = computed(() =>
    (result.value?.ports ?? []).map((port: PreviewPort) => ({
        port,
        table: port.kind === "Records" ? buildPreviewTable(port) : null,
    })),
);

const run = async () => {
    loading.value = true;
    failed.value = false;

    try {
        if (!(await props.prepare())) {
            failed.value = true;

            return;
        }

        result.value = await props.api.preview(props.stepId);
    } catch {
        failed.value = true;
    } finally {
        loading.value = false;
    }
};

defineExpose({ run });
</script>

<template>
    <div class="dpd-preview" data-cy="preview-tab">
        <div class="dpd-preview-actions">
            <button type="button" class="btn btn-sm btn-primary" :disabled="loading" data-cy="preview-run" @click="run">
                <span v-if="loading" class="spinner-border spinner-border-sm" aria-hidden="true"></span>
                <i v-else class="fa-solid fa-eye" aria-hidden="true"></i>
                {{ result ? t("PreviewAgain", "Preview again") : t("Preview", "Preview") }}
            </button>
            <span class="dpd-section-hint m-0">{{ t("PreviewHint", "Runs the pipeline up to this step on a sample of the rows, with the draft's settings.") }}</span>
            <span v-if="result" class="small text-body-secondary ms-auto" data-cy="preview-duration">{{ t("PreviewDuration", "{0} ms", formatNumber(result.durationMilliseconds)) }}</span>
        </div>

        <div role="status" aria-live="polite">
            <p v-if="failed" class="alert alert-danger m-2" data-cy="preview-failed">{{ t("PreviewFailed", "The preview could not be computed.") }}</p>
            <p v-else-if="!result && !loading" class="dpd-panel-message" data-cy="preview-empty">{{ t("PreviewEmpty", "Preview the step to see its first rows.") }}</p>
        </div>

        <template v-if="result">
            <p v-if="result.isInputPreview" class="alert alert-info m-2 py-2" data-cy="preview-input-note">
                <i class="fa-solid fa-circle-info" aria-hidden="true"></i>
                {{ t("PreviewInputNote", "Destinations are never executed by a preview; this is what the step would receive.") }}
            </p>
            <p v-if="result.error" class="alert alert-danger m-2 py-2" role="alert" data-cy="preview-error">
                <i class="fa-solid fa-circle-xmark" aria-hidden="true"></i>
                {{ result.error }}
            </p>
            <ul v-if="result.issues.length > 0" class="dpd-preview-issues" data-cy="preview-issues">
                <li v-for="(issue, index) in result.issues" :key="index" :class="issue.severity === 'Error' ? 'text-danger' : 'text-warning-emphasis'">{{ issue.message }}</li>
            </ul>

            <section v-for="{ port, table } in ports" :key="port.name" class="dpd-preview-port" :data-cy="`preview-port-${port.name}`">
                <h4 class="dpd-section-title">
                    <i class="fa-solid fa-fw" :class="port.kind === 'Files' ? 'fa-file-lines' : 'fa-table'" aria-hidden="true"></i>
                    {{ port.displayName }}
                    <span class="small fw-normal text-body-secondary">
                        <template v-if="table">· {{ rowCount(port.rows.length) }}</template>
                    </span>
                </h4>

                <template v-if="table">
                    <p v-if="table.headers.length === 0" class="dpd-section-hint">{{ t("NoFields", "No fields.") }}</p>
                    <div v-else class="dpd-preview-scroll" tabindex="0" :aria-label="t('PreviewOf', 'The rows of {0}', port.displayName)">
                        <table class="table table-sm table-bordered dpd-preview-table mb-0">
                            <thead>
                                <tr>
                                    <th v-for="header in table.headers" :key="header.name" scope="col" :title="header.name">
                                        <span class="dpd-preview-field">{{ header.displayName }}</span>
                                        <span class="dpd-preview-type">{{ header.typeLabel }}</span>
                                    </th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="(row, rowIndex) in table.rows" :key="rowIndex">
                                    <td v-for="(cell, cellIndex) in row" :key="cellIndex" :class="{ 'text-end': cell.isNumber, 'dpd-preview-null': cell.isNull }">{{ cell.text }}</td>
                                </tr>
                                <tr v-if="table.rows.length === 0">
                                    <td :colspan="table.headers.length" class="text-body-secondary">{{ t("NoRows", "No rows.") }}</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                    <p v-if="table.note" class="dpd-section-hint mt-1" data-cy="preview-truncated">{{ table.note }}</p>
                </template>

                <template v-else>
                    <ul v-if="port.files.length > 0" class="dpd-preview-files" data-cy="preview-files">
                        <li v-for="file in port.files" :key="file.fileName">
                            <i class="fa-regular fa-file" aria-hidden="true"></i>
                            <span class="dpd-preview-file-name">{{ file.fileName }}</span>
                            <span class="small text-body-secondary">
                                {{ file.contentType }} · {{ formatFileSize(file.length) }}<template v-if="file.rowCount != null"> · {{ rowCount(file.rowCount) }}</template>
                            </span>
                        </li>
                    </ul>
                    <p v-else class="dpd-section-hint">{{ t("NoFiles", "No files.") }}</p>
                </template>
            </section>
        </template>
    </div>
</template>
