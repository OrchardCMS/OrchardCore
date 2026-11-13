<script setup lang="ts">
import { computed, nextTick, onMounted, ref } from "vue";
import type { DesignerConfig } from "../config";
import type { DesignerApi } from "../api/designerApi";
import type { DesignerComparison, DesignerDefinition } from "../api/types";
import { createDesignerStore } from "../state/designerStore";
import DesignerCanvas from "../canvas/DesignerCanvas.vue";
import type { CanvasChanges, ChangeKind } from "../canvas/changes";
import { t } from "../i18n";

// Two definitions of a workflow type side by side (a version and the draft, or two versions), with what
// changed from the first to the second highlighted on both, and listed below.
const props = defineProps<{ config: DesignerConfig; api: DesignerApi }>();

const loading = ref(true);
const loadError = ref<string | null>(null);
const comparison = ref<DesignerComparison | null>(null);
const fromStore = createDesignerStore();
const toStore = createDesignerStore();
const fromCanvas = ref<InstanceType<typeof DesignerCanvas> | null>(null);
const toCanvas = ref<InstanceType<typeof DesignerCanvas> | null>(null);

const byId = (ids: string[], kind: ChangeKind) => Object.fromEntries(ids.map((id) => [id, kind] as const));

// The earlier definition shows what was removed, the later one what was added; both show what changed.
const fromChanges = computed<CanvasChanges | null>(() => {
    const changes = comparison.value?.changes;

    return changes
        ? {
              nodes: { ...byId(changes.removedActivityIds, "removed"), ...byId(changes.changedActivityIds, "changed"), ...byId(changes.movedActivityIds, "moved") },
              edges: byId(changes.removedTransitionKeys, "removed"),
          }
        : null;
});

const toChanges = computed<CanvasChanges | null>(() => {
    const changes = comparison.value?.changes;

    return changes
        ? {
              nodes: { ...byId(changes.addedActivityIds, "added"), ...byId(changes.changedActivityIds, "changed"), ...byId(changes.movedActivityIds, "moved") },
              edges: byId(changes.addedTransitionKeys, "added"),
          }
        : null;
});

const label = (definition: DesignerDefinition) => {
    if (!definition.version) {
        return t("Draft");
    }

    return definition.version.isPublished ? t("VersionPublished", definition.version.version) : t("VersionNumber", definition.version.version);
};

const titleOf = (definition: DesignerDefinition, id: string) => definition.nodes.find((node) => node.id === id)?.title || id;

// The changes, in words: each group lists the activities by title.
const summary = computed(() => {
    const value = comparison.value;

    if (!value) {
        return [];
    }

    const { from, to, changes } = value;
    const transition = (definition: DesignerDefinition, key: string) => {
        const [source, outcome, destination] = key.split(":");

        return `${titleOf(definition, source)} (${outcome}) → ${titleOf(definition, destination)}`;
    };

    return [
        { key: "added", title: t("ActivitiesAdded"), items: changes.addedActivityIds.map((id) => titleOf(to, id)) },
        { key: "removed", title: t("ActivitiesRemoved"), items: changes.removedActivityIds.map((id) => titleOf(from, id)) },
        { key: "changed", title: t("ActivitiesChanged"), items: changes.changedActivityIds.map((id) => titleOf(to, id)) },
        { key: "moved", title: t("ActivitiesMoved"), items: changes.movedActivityIds.map((id) => titleOf(to, id)) },
        { key: "connections-added", title: t("ConnectionsAdded"), items: changes.addedTransitionKeys.map((key) => transition(to, key)) },
        { key: "connections-removed", title: t("ConnectionsRemoved"), items: changes.removedTransitionKeys.map((key) => transition(from, key)) },
        { key: "settings", title: t("SettingsChanged"), items: changes.changedSettings },
    ].filter((group) => group.items.length > 0);
});

onMounted(async () => {
    try {
        const value = await props.api.getComparison();

        fromStore.loadDefinition(value.from);
        toStore.loadDefinition(value.to);
        comparison.value = value;
    } catch {
        loadError.value = t("CompareLoadFailed");
    } finally {
        loading.value = false;
    }

    await nextTick();

    // A collapsed branch would hide changes.
    fromCanvas.value?.reveal(Object.keys(fromChanges.value?.nodes ?? {}));
    toCanvas.value?.reveal(Object.keys(toChanges.value?.nodes ?? {}));
    fromCanvas.value?.fit();
    toCanvas.value?.fit();
});
</script>

<template>
    <div class="wfd wfd-readonly wfd-compare" data-cy="workflow-compare">
        <header class="wfd-toolbar">
            <h2 class="wfd-title text-truncate">{{ comparison?.to.settings.name }}</h2>
            <span class="wfd-legend small text-body-secondary" data-cy="compare-legend">
                <span class="badge wfd-change-badge is-added">{{ t("ChangeAdded") }}</span>
                <span class="badge wfd-change-badge is-removed">{{ t("ChangeRemoved") }}</span>
                <span class="badge wfd-change-badge is-changed">{{ t("ChangeChanged") }}</span>
                <span class="badge wfd-change-badge is-moved">{{ t("ChangeMoved") }}</span>
            </span>
            <a v-if="config.designerUrl" :href="config.designerUrl" class="btn btn-sm btn-outline-secondary ms-auto" data-cy="compare-designer">{{ t("OpenDesigner") }}</a>
        </header>

        <div v-if="loading" class="wfd-message">
            <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
            {{ t("Loading") }}
        </div>
        <div v-else-if="loadError" class="wfd-message text-danger" role="alert">{{ loadError }}</div>

        <template v-else-if="comparison">
            <div class="wfd-compare-body">
                <section class="wfd-compare-side" :aria-label="label(comparison.from)" data-cy="compare-from">
                    <h3 class="wfd-compare-heading">{{ label(comparison.from) }}</h3>
                    <div class="wfd-canvas-host">
                        <DesignerCanvas ref="fromCanvas" :store="fromStore" read-only :changes="fromChanges" />
                    </div>
                </section>
                <section class="wfd-compare-side" :aria-label="label(comparison.to)" data-cy="compare-to">
                    <h3 class="wfd-compare-heading">{{ label(comparison.to) }}</h3>
                    <div class="wfd-canvas-host">
                        <DesignerCanvas ref="toCanvas" :store="toStore" read-only :changes="toChanges" />
                    </div>
                </section>
            </div>

            <section class="wfd-compare-changes" :aria-label="t('Changes')" data-cy="compare-changes">
                <p v-if="!comparison.changes.hasChanges" class="mb-0" data-cy="compare-no-changes">{{ t("NoChanges") }}</p>
                <dl v-else class="mb-0">
                    <template v-for="group in summary" :key="group.key">
                        <dt :data-cy="`compare-group-${group.key}`">{{ group.title }}</dt>
                        <dd>{{ group.items.join(", ") }}</dd>
                    </template>
                </dl>
            </section>
        </template>
    </div>
</template>
