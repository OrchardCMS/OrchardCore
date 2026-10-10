<script setup lang="ts">
import { onMounted, ref } from "vue";
import ModalDialog from "../ui/ModalDialog.vue";
import { withQuery, type DesignerApi } from "../api/designerApi";
import type { DesignerVersion, DesignerVersions } from "../api/types";
import { formatDateTime } from "./formatDateTime";
import { t } from "../i18n";

// The versions of the pipeline: each publish created one. A version can be viewed, or restored into the draft.
const props = defineProps<{ api: DesignerApi; versionPageUrl?: string | null; canRestore: boolean }>();

const emit = defineEmits<{
    (event: "close"): void;
    (event: "restore", version: DesignerVersion): void;
}>();

const loading = ref(true);
const error = ref(false);
const data = ref<DesignerVersions | null>(null);

const viewUrl = (version: DesignerVersion) => (props.versionPageUrl ? withQuery(props.versionPageUrl, { versionId: version.versionId }) : null);

const publishedBy = (version: DesignerVersion) =>
    version.publishedBy
        ? t("PublishedOnBy", "Published on {0} by {1}", formatDateTime(version.publishedUtc), version.publishedBy)
        : t("PublishedOn", "Published on {0}", formatDateTime(version.publishedUtc));

onMounted(async () => {
    try {
        data.value = await props.api.getVersions();
    } catch {
        error.value = true;
    } finally {
        loading.value = false;
    }
});
</script>

<template>
    <ModalDialog :title="t('Versions', 'Versions')" size="lg" data-cy="versions-dialog" @close="emit('close')">
        <p class="text-body-secondary">{{ t("VersionsIntro", "Each publish creates a version. Runs use the published version; restoring a version copies it into the draft.") }}</p>

        <p v-if="loading" class="mb-0">
            <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
            {{ t("Loading", "Loading…") }}
        </p>
        <p v-else-if="error" class="text-danger mb-0" role="alert">{{ t("VersionsLoadFailed", "The versions could not be loaded.") }}</p>

        <table v-else-if="data" class="table table-sm align-middle mb-0" data-cy="versions-table">
            <tbody>
                <tr v-if="data.draft" data-cy="versions-draft">
                    <th scope="row">{{ t("Draft", "Draft") }}</th>
                    <td class="small text-body-secondary">
                        {{
                            data.draft.modifiedBy
                                ? t("ChangedOnBy", "Changed on {0} by {1}", formatDateTime(data.draft.modifiedUtc), data.draft.modifiedBy)
                                : formatDateTime(data.draft.modifiedUtc)
                        }}
                    </td>
                    <td></td>
                </tr>
                <tr v-for="version in data.versions" :key="version.versionId" :data-cy="`version-${version.number}`">
                    <th scope="row" class="text-nowrap">
                        {{ t("VersionNumber", "Version {0}", version.number) }}
                        <span v-if="version.isPublished" class="badge text-bg-success ms-1" data-cy="version-published">{{ t("PublishedBadge", "Published") }}</span>
                    </th>
                    <td class="small text-body-secondary">{{ publishedBy(version) }}</td>
                    <td class="text-end text-nowrap">
                        <a v-if="viewUrl(version)" class="btn btn-sm btn-link" :href="viewUrl(version)!" data-cy="version-view">{{ t("View", "View") }}</a>
                        <button v-if="canRestore" type="button" class="btn btn-sm btn-link" data-cy="version-restore" @click="emit('restore', version)">
                            {{ t("Restore", "Restore") }}
                        </button>
                    </td>
                </tr>
                <tr v-if="data.versions.length === 0">
                    <td colspan="3" class="text-body-secondary">{{ t("NoVersions", "The pipeline was never published.") }}</td>
                </tr>
            </tbody>
        </table>

        <template #footer>
            <button type="button" class="btn btn-secondary" data-autofocus data-cy="versions-dialog-close" @click="emit('close')">{{ t("Close", "Close") }}</button>
        </template>
    </ModalDialog>
</template>
