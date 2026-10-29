<script setup lang="ts">
import DraftPublishDialog from "@bloom/designer/draft/PublishDialog.vue";
import type { DesignIssue, DesignerNode } from "../api/types";
import type { PublishDecision } from "./publishDecision";
import { issueTarget, issueTitle } from "../panel/issues";
import { t } from "../i18n";

// The dialog of a Publish that needs the user, with a note that the running instances keep their version.
const props = defineProps<{ decision: Exclude<PublishDecision, { kind: "publish" }>; nodes: DesignerNode[] }>();

const emit = defineEmits<{
    (event: "publish"): void;
    (event: "close"): void;
    (event: "select-issue", issue: DesignIssue): void;
}>();

const titleOf = (issue: DesignIssue) => issueTitle(props.nodes, issue);
</script>

<template>
    <DraftPublishDialog
        :decision="decision"
        :target-of="issueTarget"
        :title-of="titleOf"
        @publish="emit('publish')"
        @close="emit('close')"
        @select-issue="emit('select-issue', $event)"
    >
        <template #notes>
            <p v-if="decision.kind === 'confirm' && decision.runningInstanceCount > 0" class="alert alert-info mt-3 mb-0" data-cy="publish-running-instances">
                {{ t("RunningInstancesContinue", decision.runningInstanceCount) }}
            </p>
        </template>
    </DraftPublishDialog>
</template>
