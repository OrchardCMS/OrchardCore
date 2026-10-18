<script setup lang="ts">
import DraftVersionsDialog from "@bloom/designer/draft/VersionsDialog.vue";
import { formatDateTime } from "@bloom/designer/core/formatDateTime";
import { withQuery, type DesignerApi } from "../api/designerApi";
import type { DesignerVersion } from "../api/types";
import { t } from "../i18n";

// The versions of the pipeline: each publish created one. A version can be viewed, or restored into the draft.
const props = defineProps<{ api: DesignerApi; versionPageUrl?: string | null; canRestore: boolean }>();

const emit = defineEmits<{
    (event: "close"): void;
    (event: "restore", version: DesignerVersion): void;
}>();

const viewUrl = (version: DesignerVersion) => (props.versionPageUrl ? withQuery(props.versionPageUrl, { versionId: version.versionId }) : null);

const publishedBy = (version: DesignerVersion) =>
    version.publishedBy
        ? t("PublishedOnBy", "Published on {0} by {1}", formatDateTime(version.publishedUtc), version.publishedBy)
        : t("PublishedOn", "Published on {0}", formatDateTime(version.publishedUtc));
</script>

<template>
    <DraftVersionsDialog
        :load="api.getVersions"
        :describe="publishedBy"
        :view-url="viewUrl"
        :can-restore="() => canRestore"
        @close="emit('close')"
        @restore="emit('restore', $event)"
    />
</template>
