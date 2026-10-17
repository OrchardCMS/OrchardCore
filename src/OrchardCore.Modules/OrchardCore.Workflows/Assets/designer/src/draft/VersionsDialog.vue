<script setup lang="ts">
import { computed, shallowRef } from "vue";
import DraftVersionsDialog from "@bloom/designer/draft/VersionsDialog.vue";
import type { VersionsData } from "@bloom/designer/core/types";
import { formatDateTime } from "@bloom/designer/core/formatDateTime";
import { withQuery, type DesignerApi } from "../api/designerApi";
import type { DesignerVersion } from "../api/types";
import { t } from "../i18n";

// The versions of the workflow type: each publish created one. A version can be viewed, compared with what is
// being edited (the draft, or the published version), or restored into the draft. It also tells how many instances
// run on each version.
const props = defineProps<{ api: DesignerApi; versionPageUrl?: string | null; comparePageUrl?: string | null }>();

const emit = defineEmits<{
    (event: "close"): void;
    (event: "restore", version: DesignerVersion): void;
}>();

type VersionRow = DesignerVersion & { number: number };

const data = shallowRef<VersionsData<VersionRow> | null>(null);

const load = async (): Promise<VersionsData<VersionRow>> => {
    const versions = await props.api.getVersions();

    data.value = { draft: versions.draft, versions: versions.versions.map((version) => ({ ...version, number: version.version })) };

    return data.value;
};

const published = computed(() => data.value?.versions.find((version) => version.isPublished) ?? null);

// What a version is compared with: the draft when there is one, otherwise the published version.
const compareTarget = computed(() => (data.value?.draft ? "draft" : (published.value?.versionId ?? null)));

const viewUrl = (version: VersionRow) => (props.versionPageUrl ? withQuery(props.versionPageUrl, { versionId: version.versionId }) : null);

const compareUrl = (from: string) =>
    props.comparePageUrl && compareTarget.value && from !== compareTarget.value ? withQuery(props.comparePageUrl, { from, to: compareTarget.value }) : null;

const createdBy = (version: VersionRow) =>
    version.createdBy ? t("CreatedOnBy", formatDateTime(version.createdUtc), version.createdBy) : formatDateTime(version.createdUtc);
</script>

<template>
    <DraftVersionsDialog :load="load" :describe="createdBy" :view-url="viewUrl" @close="emit('close')" @restore="emit('restore', $event)">
        <template #intro>{{ t("VersionsIntro") }}</template>
        <template #draft-actions>
            <a
                v-if="published && comparePageUrl"
                class="btn btn-sm btn-link"
                :href="withQuery(comparePageUrl, { from: published.versionId, to: 'draft' })"
                data-cy="versions-draft-compare"
            >
                {{ t("Compare") }}
            </a>
        </template>
        <template #details="{ version }">
            <span v-if="version.instanceCount" :title="t('VersionInstancesHint')">{{ t("InstanceCount", version.instanceCount) }}</span>
        </template>
        <template #actions="{ version }">
            <a v-if="compareUrl(version.versionId)" class="btn btn-sm btn-link" :href="compareUrl(version.versionId)!" data-cy="version-compare">
                {{ t("Compare") }}
            </a>
        </template>
    </DraftVersionsDialog>
</template>
