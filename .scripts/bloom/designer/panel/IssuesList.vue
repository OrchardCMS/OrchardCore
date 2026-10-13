<script setup lang="ts" generic="TIssue extends DesignIssue">
import { computed } from "vue";
import type { DesignIssue } from "../core/types";
import { t } from "../core/i18n";

// The issues of a definition. An issue that targets a node can be selected, to show that node.
const props = defineProps<{
    issues: TIssue[];
    // The id of the node an issue targets, such as its `activityId` or `stepId`.
    targetOf: (issue: TIssue) => string | null | undefined;
    // The title of the node an issue targets, shown before its message.
    titleOf: (issue: TIssue) => string | null | undefined;
}>();

const emit = defineEmits<{ (event: "select", issue: TIssue): void }>();

// Errors first, then warnings, in the server's order.
const sorted = computed(() => [...props.issues].sort((a, b) => (a.severity === b.severity ? 0 : a.severity === "Error" ? -1 : 1)));
</script>

<template>
    <div class="ocd-issues">
        <p v-if="issues.length === 0" class="ocd-panel-message" data-cy="issues-empty">
            <i class="fa-solid fa-circle-check text-success" aria-hidden="true"></i>
            {{ t("NoIssues", "No issues: the draft can be published.") }}
        </p>
        <ul v-else class="list-group list-group-flush">
            <li v-for="(issue, index) in sorted" :key="`${issue.code ?? ''}-${targetOf(issue) ?? ''}-${index}`" class="list-group-item p-0">
                <button
                    type="button"
                    class="btn btn-link ocd-issue text-start w-100"
                    :disabled="!targetOf(issue)"
                    :data-cy="`issue-${issue.code ?? index}`"
                    @click="emit('select', issue)"
                >
                    <i
                        class="fa-solid fa-fw"
                        :class="issue.severity === 'Error' ? 'fa-circle-xmark text-danger' : 'fa-triangle-exclamation text-warning'"
                        :aria-label="issue.severity === 'Error' ? t('Error', 'Error') : t('Warning', 'Warning')"
                        role="img"
                    ></i>
                    <span class="ocd-issue-text">
                        <span v-if="titleOf(issue)" class="ocd-issue-node">{{ titleOf(issue) }}</span>
                        {{ issue.message }}
                    </span>
                </button>
            </li>
        </ul>
    </div>
</template>
