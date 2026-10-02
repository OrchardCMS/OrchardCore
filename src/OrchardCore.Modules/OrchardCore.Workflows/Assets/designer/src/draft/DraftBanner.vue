<script setup lang="ts">
import { formatDateTime } from "./formatDateTime";
import { t } from "../i18n";

defineProps<{ modifiedBy: string | null; modifiedUtc: string | null }>();

const emit = defineEmits<{ (event: "dismiss"): void }>();
</script>

<template>
    <div class="alert alert-info alert-dismissible wfd-draft-banner mb-0 rounded-0" role="status" data-cy="draft-banner">
        <i class="fa-solid fa-user-pen" aria-hidden="true"></i>
        {{ t("DraftLastEditedBy", modifiedBy || t("AnotherUser"), formatDateTime(modifiedUtc)) }}
        <button type="button" class="btn-close" :aria-label="t('Close')" data-cy="draft-banner-dismiss" @click="emit('dismiss')"></button>
    </div>
</template>
