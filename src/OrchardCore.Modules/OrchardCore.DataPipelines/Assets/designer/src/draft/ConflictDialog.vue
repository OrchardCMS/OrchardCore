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
    <ModalDialog :title="t('ConflictTitle', 'The pipeline was changed elsewhere')" data-cy="conflict-dialog" @close="emit('close')">
        <p>
            {{
                problem.modifiedUtc
                    ? t("ConflictMessage", "{0} changed the draft on {1}, after you opened it.", problem.modifiedBy || t("AnotherUser", "Another user"), formatDateTime(problem.modifiedUtc))
                    : t("ConflictMessageUnknown", "The draft changed after you opened it.")
            }}
        </p>
        <ul class="mb-0">
            <li>{{ t("ConflictReloadHint", "Reload: show their changes, and lose your unsaved changes.") }}</li>
            <li>{{ t("ConflictOverwriteHint", "Overwrite: save your changes over theirs.") }}</li>
        </ul>
        <template #footer>
            <button type="button" class="btn btn-danger" data-cy="conflict-overwrite" @click="emit('overwrite')">{{ t("Overwrite", "Overwrite") }}</button>
            <button type="button" class="btn btn-primary" data-autofocus data-cy="conflict-reload" @click="emit('reload')">{{ t("Reload", "Reload") }}</button>
        </template>
    </ModalDialog>
</template>
