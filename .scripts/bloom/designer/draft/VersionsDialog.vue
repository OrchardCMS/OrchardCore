<script setup lang="ts" generic="TVersion extends VersionSummary">
import { onMounted, ref, shallowRef } from "vue";
import ModalDialog from "../ui/ModalDialog.vue";
import type { DraftSummary, VersionSummary, VersionsData } from "../core/types";
import { formatDateTime } from "../core/formatDateTime";
import { t } from "../core/i18n";

// The published versions of a definition, the most recent first, and its draft. The designer describes each
// version, and adds its own details and actions through the slots.
const props = defineProps<{
    load: () => Promise<VersionsData<TVersion>>;
    // The text of the second column, such as when and by whom the version was published.
    describe: (version: TVersion) => string;
    // The URL of a page showing a version, if any.
    viewUrl?: (version: TVersion) => string | null;
    // Whether a version can be restored into the draft; by default, all but the published one.
    canRestore?: (version: TVersion) => boolean;
}>();

const emit = defineEmits<{
    (event: "close"): void;
    (event: "restore", version: TVersion): void;
}>();

defineSlots<{
    intro?: () => unknown;
    "draft-actions"?: (props: { draft: DraftSummary }) => unknown;
    details?: (props: { version: TVersion }) => unknown;
    actions?: (props: { version: TVersion }) => unknown;
}>();

const loading = ref(true);
const error = ref(false);
const data = shallowRef<VersionsData<TVersion> | null>(null);

const changedBy = (draft: DraftSummary) =>
    draft.modifiedBy ? t("ChangedOnBy", "Changed on {0} by {1}", formatDateTime(draft.modifiedUtc), draft.modifiedBy) : formatDateTime(draft.modifiedUtc);

const restorable = (version: TVersion) => (props.canRestore ? props.canRestore(version) : !version.isPublished);

onMounted(async () => {
    try {
        data.value = await props.load();
    } catch {
        error.value = true;
    } finally {
        loading.value = false;
    }
});
</script>

<template>
    <ModalDialog :title="t('Versions', 'Versions')" size="lg" data-cy="versions-dialog" @close="emit('close')">
        <p class="text-body-secondary">
            <slot name="intro">{{ t("VersionsIntro", "Each publish creates a version. Restoring a version copies it into the draft.") }}</slot>
        </p>

        <p v-if="loading" class="mb-0">
            <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
            {{ t("Loading", "Loading…") }}
        </p>
        <p v-else-if="error" class="text-danger mb-0" role="alert">{{ t("VersionsLoadFailed", "The versions could not be loaded.") }}</p>

        <table v-else-if="data" class="table table-sm align-middle mb-0" data-cy="versions-table">
            <tbody>
                <tr v-if="data.draft" data-cy="versions-draft">
                    <th scope="row">{{ t("Draft", "Draft") }}</th>
                    <td class="small text-body-secondary">{{ changedBy(data.draft) }}</td>
                    <td></td>
                    <td class="text-end text-nowrap">
                        <slot name="draft-actions" :draft="data.draft"></slot>
                    </td>
                </tr>
                <tr v-for="version in data.versions" :key="version.versionId" :data-cy="`version-${version.number}`">
                    <th scope="row" class="text-nowrap">
                        {{ t("VersionNumber", "Version {0}", version.number) }}
                        <span v-if="version.isPublished" class="badge text-bg-success ms-1" data-cy="version-published">{{ t("PublishedBadge", "Published") }}</span>
                    </th>
                    <td class="small text-body-secondary">{{ describe(version) }}</td>
                    <td class="small text-nowrap">
                        <slot name="details" :version="version"></slot>
                    </td>
                    <td class="text-end text-nowrap">
                        <a v-if="viewUrl?.(version)" class="btn btn-sm btn-link" :href="viewUrl(version)!" data-cy="version-view">{{ t("View", "View") }}</a>
                        <slot name="actions" :version="version"></slot>
                        <button v-if="restorable(version)" type="button" class="btn btn-sm btn-link" data-cy="version-restore" @click="emit('restore', version)">
                            {{ t("Restore", "Restore") }}
                        </button>
                    </td>
                </tr>
                <tr v-if="data.versions.length === 0">
                    <td colspan="4" class="text-body-secondary">{{ t("NoVersions", "It was never published.") }}</td>
                </tr>
            </tbody>
        </table>

        <template #footer>
            <button type="button" class="btn btn-secondary" data-autofocus data-cy="versions-dialog-close" @click="emit('close')">{{ t("Close", "Close") }}</button>
        </template>
    </ModalDialog>
</template>
