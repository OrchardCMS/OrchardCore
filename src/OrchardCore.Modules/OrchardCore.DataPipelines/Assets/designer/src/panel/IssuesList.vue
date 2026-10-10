<script setup lang="ts">
import { computed } from "vue";
import type { DesignIssue, DesignerNode } from "../api/types";
import { t } from "../i18n";

const props = defineProps<{ issues: DesignIssue[]; nodes: DesignerNode[] }>();

const emit = defineEmits<{ (event: "select", issue: DesignIssue): void }>();

// Errors first, then warnings, in the server's order.
const sorted = computed(() => [...props.issues].sort((a, b) => (a.severity === b.severity ? 0 : a.severity === "Error" ? -1 : 1)));

const titleOf = (issue: DesignIssue) => props.nodes.find((node) => node.id === issue.stepId)?.displayText;
</script>

<template>
    <div class="dpd-issues">
        <p v-if="issues.length === 0" class="dpd-panel-message" data-cy="issues-empty">
            <i class="fa-solid fa-circle-check text-success" aria-hidden="true"></i>
            {{ t("NoIssues", "No issues: the pipeline can be published.") }}
        </p>
        <ul v-else class="list-group list-group-flush">
            <li v-for="(issue, index) in sorted" :key="`${issue.stepId ?? ''}-${index}`" class="list-group-item p-0">
                <button
                    type="button"
                    class="btn btn-link dpd-issue text-start w-100"
                    :disabled="!issue.stepId"
                    :data-cy="`issue-${index}`"
                    @click="emit('select', issue)"
                >
                    <i
                        class="fa-solid fa-fw"
                        :class="issue.severity === 'Error' ? 'fa-circle-xmark text-danger' : 'fa-triangle-exclamation text-warning'"
                        :aria-label="issue.severity === 'Error' ? t('Error', 'Error') : t('Warning', 'Warning')"
                        role="img"
                    ></i>
                    <span class="dpd-issue-text">
                        <span v-if="titleOf(issue)" class="dpd-issue-step">{{ titleOf(issue) }}</span>
                        {{ issue.message }}
                    </span>
                </button>
            </li>
        </ul>
    </div>
</template>
