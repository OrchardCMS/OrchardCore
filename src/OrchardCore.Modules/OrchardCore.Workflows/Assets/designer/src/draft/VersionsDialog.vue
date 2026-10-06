<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import ModalDialog from "../ui/ModalDialog.vue";
import { withQuery, type DesignerApi } from "../api/designerApi";
import type { DesignerVersion, DesignerVersions } from "../api/types";
import { formatDateTime } from "./formatDateTime";
import { t } from "../i18n";

// The versions of the workflow type: each publish created one. A version can be viewed, compared with what is
// being edited (the draft, or the published version), or restored into the draft.
const props = defineProps<{ api: DesignerApi; versionPageUrl?: string | null; comparePageUrl?: string | null }>();

const emit = defineEmits<{
    (event: "close"): void;
    (event: "restore", version: DesignerVersion): void;
}>();

const loading = ref(true);
const error = ref(false);
const data = ref<DesignerVersions | null>(null);

const published = computed(() => data.value?.versions.find((version) => version.isPublished) ?? null);

// What a version is compared with: the draft when there is one, otherwise the published version.
const compareTarget = computed(() => (data.value?.draft ? "draft" : (published.value?.versionId ?? null)));

const viewUrl = (version: DesignerVersion) => (props.versionPageUrl ? withQuery(props.versionPageUrl, { versionId: version.versionId }) : null);

const compareUrl = (from: string) =>
    props.comparePageUrl && compareTarget.value && from !== compareTarget.value ? withQuery(props.comparePageUrl, { from, to: compareTarget.value }) : null;

const createdBy = (version: DesignerVersion) =>
    version.createdBy ? t("CreatedOnBy", formatDateTime(version.createdUtc), version.createdBy) : formatDateTime(version.createdUtc);

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
    <ModalDialog :title="t('Versions')" data-cy="versions-dialog" @close="emit('close')">
        <p class="text-body-secondary">{{ t("VersionsIntro") }}</p>

        <p v-if="loading" class="mb-0">
            <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
            {{ t("Loading") }}
        </p>
        <p v-else-if="error" class="text-danger mb-0" role="alert">{{ t("VersionsLoadFailed") }}</p>

        <table v-else-if="data" class="table table-sm align-middle mb-0" data-cy="versions-table">
            <tbody>
                <tr v-if="data.draft" data-cy="versions-draft">
                    <th scope="row">{{ t("Draft") }}</th>
                    <td class="small text-body-secondary">
                        {{ data.draft.modifiedBy ? t("ChangedOnBy", formatDateTime(data.draft.modifiedUtc), data.draft.modifiedBy) : formatDateTime(data.draft.modifiedUtc) }}
                    </td>
                    <td></td>
                    <td class="text-end text-nowrap">
                        <a
                            v-if="published && comparePageUrl"
                            class="btn btn-sm btn-link"
                            :href="withQuery(comparePageUrl, { from: published.versionId, to: 'draft' })"
                            data-cy="versions-draft-compare"
                        >
                            {{ t("Compare") }}
                        </a>
                    </td>
                </tr>
                <tr v-for="version in data.versions" :key="version.versionId" :data-cy="`version-${version.version}`">
                    <th scope="row" class="text-nowrap">
                        {{ t("VersionNumber", version.version) }}
                        <span v-if="version.isPublished" class="badge text-bg-success ms-1" data-cy="version-published">{{ t("PublishedBadge") }}</span>
                    </th>
                    <td class="small text-body-secondary">{{ createdBy(version) }}</td>
                    <td class="small text-nowrap">
                        <span v-if="version.instanceCount" :title="t('VersionInstancesHint')">{{ t("InstanceCount", version.instanceCount) }}</span>
                    </td>
                    <td class="text-end text-nowrap">
                        <a v-if="viewUrl(version)" class="btn btn-sm btn-link" :href="viewUrl(version)!" data-cy="version-view">{{ t("View") }}</a>
                        <a v-if="compareUrl(version.versionId)" class="btn btn-sm btn-link" :href="compareUrl(version.versionId)!" data-cy="version-compare">
                            {{ t("Compare") }}
                        </a>
                        <button v-if="!version.isPublished" type="button" class="btn btn-sm btn-link" data-cy="version-restore" @click="emit('restore', version)">
                            {{ t("Restore") }}
                        </button>
                    </td>
                </tr>
                <tr v-if="data.versions.length === 0">
                    <td colspan="4" class="text-body-secondary">{{ t("NoVersions") }}</td>
                </tr>
            </tbody>
        </table>

        <template #footer>
            <button type="button" class="btn btn-secondary" data-autofocus data-cy="versions-dialog-close" @click="emit('close')">{{ t("Close") }}</button>
        </template>
    </ModalDialog>
</template>
