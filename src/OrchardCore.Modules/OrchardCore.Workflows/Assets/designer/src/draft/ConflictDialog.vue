<script setup lang="ts">
import ModalDialog from "../ui/ModalDialog.vue";
import type { ProblemDetails } from "../api/types";
import { formatDateTime } from "./formatDateTime";
import { t } from "../i18n";

// Shown when the draft changed since this designer last saved it (HTTP 409). Closing it keeps the conflict:
// autosave stays stopped until one of the two choices is made.
defineProps<{ problem: ProblemDetails }>();

const emit = defineEmits<{
    (event: "reload"): void;
    (event: "overwrite"): void;
    (event: "close"): void;
}>();
</script>

<template>
    <ModalDialog :title="t('ConflictTitle')" data-cy="conflict-dialog" @close="emit('close')">
        <p>
            {{
                problem.modifiedUtc
                    ? t("ConflictMessage", problem.modifiedBy || t("AnotherUser"), formatDateTime(problem.modifiedUtc))
                    : t("ConflictMessageUnknown")
            }}
        </p>
        <ul class="mb-0">
            <li>{{ t("ConflictReloadHint") }}</li>
            <li>{{ t("ConflictOverwriteHint") }}</li>
        </ul>
        <template #footer>
            <button type="button" class="btn btn-danger" data-cy="conflict-overwrite" @click="emit('overwrite')">{{ t("Overwrite") }}</button>
            <button type="button" class="btn btn-primary" data-autofocus data-cy="conflict-reload" @click="emit('reload')">{{ t("Reload") }}</button>
        </template>
    </ModalDialog>
</template>
