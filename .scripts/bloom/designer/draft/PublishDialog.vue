<script setup lang="ts" generic="TIssue extends DesignIssue">
import ModalDialog from "../ui/ModalDialog.vue";
import IssuesList from "../panel/IssuesList.vue";
import type { DesignIssue } from "../core/types";
import type { PublishDecision } from "./publishDecision";
import { t } from "../core/i18n";

// The dialog of a Publish that needs the user: the errors that block it, or the warnings to acknowledge. The
// designer can add to the confirmation through the "notes" slot.
defineProps<{
    decision: Exclude<PublishDecision<TIssue>, { kind: "publish" }>;
    targetOf: (issue: TIssue) => string | null | undefined;
    titleOf: (issue: TIssue) => string | null | undefined;
}>();

const emit = defineEmits<{
    (event: "publish"): void;
    (event: "close"): void;
    (event: "select-issue", issue: TIssue): void;
}>();
</script>

<template>
    <ModalDialog v-if="decision.kind === 'blocked'" :title="t('PublishBlockedTitle', 'The draft cannot be published')" data-cy="publish-blocked-dialog" @close="emit('close')">
        <p>{{ t("PublishBlockedMessage", "Fix these {0} errors first.", decision.errors.length) }}</p>
        <IssuesList :issues="decision.errors" :target-of="targetOf" :title-of="titleOf" @select="emit('select-issue', $event)" />
        <template #footer>
            <button type="button" class="btn btn-primary" data-autofocus data-cy="publish-dialog-close" @click="emit('close')">{{ t("Close", "Close") }}</button>
        </template>
    </ModalDialog>

    <ModalDialog v-else :title="t('PublishTitle', 'Publish the draft?')" data-cy="publish-confirm-dialog" @close="emit('close')">
        <template v-if="decision.warnings.length > 0">
            <p>{{ t("PublishWarningsMessage", "The draft has {0} warnings.", decision.warnings.length) }}</p>
            <IssuesList :issues="decision.warnings" :target-of="targetOf" :title-of="titleOf" @select="emit('select-issue', $event)" />
        </template>
        <slot name="notes"></slot>
        <template #footer>
            <button type="button" class="btn btn-secondary" data-cy="publish-dialog-cancel" @click="emit('close')">{{ t("Cancel", "Cancel") }}</button>
            <button type="button" class="btn btn-primary" data-autofocus data-cy="publish-dialog-confirm" @click="emit('publish')">
                {{ decision.warnings.length > 0 ? t("PublishAnyway", "Publish anyway") : t("Publish", "Publish") }}
            </button>
        </template>
    </ModalDialog>
</template>
