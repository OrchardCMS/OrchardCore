<script setup lang="ts">
import ModalDialog from "../ui/ModalDialog.vue";
import IssuesList from "../panel/IssuesList.vue";
import type { DesignIssue, DesignerNode } from "../api/types";
import type { PublishDecision } from "./publishDecision";
import { t } from "../i18n";

// The dialog of a Publish that needs the user: the errors that block it, or the warnings to acknowledge.
defineProps<{ decision: Exclude<PublishDecision, { kind: "publish" }>; nodes: DesignerNode[] }>();

const emit = defineEmits<{
    (event: "publish"): void;
    (event: "close"): void;
    (event: "select-issue", issue: DesignIssue): void;
}>();
</script>

<template>
    <ModalDialog v-if="decision.kind === 'blocked'" :title="t('PublishBlockedTitle', 'The pipeline cannot be published')" data-cy="publish-blocked-dialog" @close="emit('close')">
        <p>{{ t("PublishBlockedMessage", "Fix these {0} errors first.", decision.errors.length) }}</p>
        <IssuesList :issues="decision.errors" :nodes="nodes" @select="emit('select-issue', $event)" />
        <template #footer>
            <button type="button" class="btn btn-primary" data-autofocus data-cy="publish-dialog-close" @click="emit('close')">{{ t("Close", "Close") }}</button>
        </template>
    </ModalDialog>

    <ModalDialog v-else :title="t('PublishTitle', 'Publish the pipeline?')" data-cy="publish-confirm-dialog" @close="emit('close')">
        <p>{{ t("PublishWarningsMessage", "The pipeline has {0} warnings. Runs will use this version once it is published.", decision.warnings.length) }}</p>
        <IssuesList :issues="decision.warnings" :nodes="nodes" @select="emit('select-issue', $event)" />
        <template #footer>
            <button type="button" class="btn btn-secondary" data-cy="publish-dialog-cancel" @click="emit('close')">{{ t("Cancel", "Cancel") }}</button>
            <button type="button" class="btn btn-primary" data-autofocus data-cy="publish-dialog-confirm" @click="emit('publish')">{{ t("PublishAnyway", "Publish anyway") }}</button>
        </template>
    </ModalDialog>
</template>
