<script setup lang="ts">
import ModalDialog from "../ui/ModalDialog.vue";
import IssuesList from "../panel/IssuesList.vue";
import type { DesignIssue, DesignerNode } from "../api/types";
import type { PublishDecision } from "./publishDecision";
import { t } from "../i18n";

// The dialog of a Publish that needs the user: the errors that block it, or the warnings and running
// instances to acknowledge.
defineProps<{ decision: Exclude<PublishDecision, { kind: "publish" }>; nodes: DesignerNode[] }>();

const emit = defineEmits<{
    (event: "publish"): void;
    (event: "close"): void;
    (event: "select-issue", issue: DesignIssue): void;
}>();
</script>

<template>
    <ModalDialog v-if="decision.kind === 'blocked'" :title="t('PublishBlockedTitle')" data-cy="publish-blocked-dialog" @close="emit('close')">
        <p>{{ t("PublishBlockedMessage", decision.errors.length) }}</p>
        <IssuesList :issues="decision.errors" :nodes="nodes" @select="emit('select-issue', $event)" />
        <template #footer>
            <button type="button" class="btn btn-primary" data-autofocus data-cy="publish-dialog-close" @click="emit('close')">{{ t("Close") }}</button>
        </template>
    </ModalDialog>

    <ModalDialog v-else :title="t('PublishTitle')" data-cy="publish-confirm-dialog" @close="emit('close')">
        <template v-if="decision.warnings.length > 0">
            <p>{{ t("PublishWarningsMessage", decision.warnings.length) }}</p>
            <IssuesList :issues="decision.warnings" :nodes="nodes" @select="emit('select-issue', $event)" />
        </template>
        <p v-if="decision.runningInstanceCount > 0" class="alert alert-warning mt-3 mb-0" data-cy="publish-running-instances">
            {{ t("RunningInstancesContinue", decision.runningInstanceCount) }}
        </p>
        <template #footer>
            <button type="button" class="btn btn-secondary" data-cy="publish-dialog-cancel" @click="emit('close')">{{ t("Cancel") }}</button>
            <button type="button" class="btn btn-primary" data-autofocus data-cy="publish-dialog-confirm" @click="emit('publish')">
                {{ decision.warnings.length > 0 ? t("PublishAnyway") : t("Publish") }}
            </button>
        </template>
    </ModalDialog>
</template>
