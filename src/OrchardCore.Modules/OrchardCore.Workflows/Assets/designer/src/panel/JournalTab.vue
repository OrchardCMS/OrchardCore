<script setup lang="ts">
import { computed } from "vue";
import type { JournalRecord } from "../api/types";
import type { DesignerStore } from "../state/designerStore";
import { formatDateTime } from "../draft/formatDateTime";
import { t } from "../i18n";

const props = defineProps<{
    store: DesignerStore;
}>();

const emit = defineEmits<{
    (event: "select", activityId: string): void;
}>();

const state = props.store.state;
const records = computed(() => state.instance?.journal ?? []);

const titleOf = (record: JournalRecord) => record.activityTitle || props.store.getNode(record.activityId)?.title || record.activityName;

const statusClass = (record: JournalRecord) =>
    record.status === "Faulted" ? "text-bg-danger" : record.status === "Halted" ? "text-bg-info" : "text-bg-success";

// Direct t("…") calls, so the translations spec sees every key.
const statusLabel = (record: JournalRecord) =>
    record.status === "Faulted" ? t("JournalFaulted") : record.status === "Halted" ? t("JournalHalted") : t("JournalCompleted");

const duration = (record: JournalRecord) => t("DurationMilliseconds", Math.round(record.durationMilliseconds));
</script>

<template>
    <div class="wfd-journal" data-cy="journal-tab">
        <p class="wfd-panel-intro">{{ t("JournalTabHint") }}</p>
        <p v-if="records.length === 0" class="wfd-panel-message" data-cy="journal-empty">{{ t("NoJournal") }}</p>
        <ol v-else class="wfd-journal-list list-unstyled">
            <li v-for="record in records" :key="record.sequence" :data-cy="`journal-record-${record.sequence}`">
                <button type="button" class="wfd-journal-record" :class="{ 'is-faulted': record.status === 'Faulted' }" @click="emit('select', record.activityId)">
                    <span class="wfd-journal-sequence">#{{ record.sequence }}</span>
                    <span class="wfd-journal-main">
                        <span class="wfd-journal-title">{{ titleOf(record) }}</span>
                        <span class="wfd-journal-details">
                            <span class="badge" :class="statusClass(record)" data-cy="journal-status">{{ statusLabel(record) }}</span>
                            <span v-if="record.isResume" class="badge text-bg-secondary">{{ t("JournalResumed") }}</span>
                            <span v-if="record.outcomes.length > 0" data-cy="journal-outcomes">{{ record.outcomes.join(", ") }}</span>
                            <time :datetime="record.startedUtc" :title="formatDateTime(record.startedUtc)">{{ duration(record) }}</time>
                        </span>
                        <span v-if="record.error" class="wfd-journal-error" data-cy="journal-error">{{ record.error }}</span>
                    </span>
                </button>
            </li>
        </ol>
    </div>
</template>
