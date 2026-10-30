<script setup lang="ts">
import { computed } from "vue";
import type { WorkflowTypeChangedMessage } from "./realtime";
import { t } from "../i18n";

const props = defineProps<{ change: WorkflowTypeChangedMessage }>();

const emit = defineEmits<{
    (event: "reload"): void;
    (event: "dismiss"): void;
}>();

const message = computed(() => {
    const name = props.change.userName || t("AnotherUser");

    if (props.change.kind === "Published") {
        return t("RemotePublished", name);
    }

    return props.change.kind === "DraftDiscarded" ? t("RemoteDraftDiscarded", name) : t("RemoteDraftChanged", name);
});
</script>

<template>
    <div class="alert alert-warning alert-dismissible wfd-remote-change mb-0 rounded-0" role="status" data-cy="remote-change">
        <i class="fa-solid fa-users" aria-hidden="true"></i>
        {{ message }}
        <button type="button" class="btn btn-sm btn-warning ms-2" data-cy="remote-change-reload" @click="emit('reload')">{{ t("Reload") }}</button>
        <button type="button" class="btn-close" :aria-label="t('Close')" data-cy="remote-change-dismiss" @click="emit('dismiss')"></button>
    </div>
</template>
