<script setup lang="ts">
import { computed, nextTick, onMounted, ref } from "vue";
import type { DesignerNode } from "../api/types";
import { t } from "../i18n";

// The keyboard way to draw a connection: pick an outcome of the source activity and a target activity.
const props = defineProps<{ source: DesignerNode; nodes: DesignerNode[]; outcome?: string }>();

const emit = defineEmits<{
    (event: "connect", outcome: string, targetId: string): void;
    (event: "cancel"): void;
}>();

const selectedOutcome = ref(props.outcome ?? props.source.outcomes[0]?.name ?? "");
const targets = computed(() => props.nodes.filter((node) => node.id !== props.source.id));
const selectedTarget = ref(targets.value[0]?.id ?? "");
const firstField = ref<HTMLSelectElement | null>(null);

onMounted(async () => {
    await nextTick();
    firstField.value?.focus();
});

const submit = () => {
    if (selectedOutcome.value && selectedTarget.value) {
        emit("connect", selectedOutcome.value, selectedTarget.value);
    }
};
</script>

<template>
    <div
        class="card shadow wfd-connect-dialog"
        role="dialog"
        aria-labelledby="wfd-connect-title"
        data-cy="connect-dialog"
        @pointerdown.stop
        @keydown.esc.prevent.stop="emit('cancel')"
    >
        <form class="card-body" @submit.prevent="submit">
            <h3 id="wfd-connect-title" class="h6 card-title">{{ t("ConnectOutcomeOf", source.title) }}</h3>
            <div class="mb-2">
                <label class="form-label" for="wfd-connect-outcome">{{ t("Outcome") }}</label>
                <select id="wfd-connect-outcome" ref="firstField" v-model="selectedOutcome" class="form-select form-select-sm" data-cy="connect-dialog-outcome">
                    <option v-for="item in source.outcomes" :key="item.name" :value="item.name">{{ item.displayName }}</option>
                </select>
            </div>
            <div class="mb-3">
                <label class="form-label" for="wfd-connect-target">{{ t("Target") }}</label>
                <select id="wfd-connect-target" v-model="selectedTarget" class="form-select form-select-sm" data-cy="connect-dialog-target">
                    <option v-for="node in targets" :key="node.id" :value="node.id">{{ node.title }} ({{ node.displayText }})</option>
                </select>
            </div>
            <div class="d-flex gap-2 justify-content-end">
                <button type="button" class="btn btn-sm btn-secondary" data-cy="connect-dialog-cancel" @click="emit('cancel')">{{ t("Cancel") }}</button>
                <button type="submit" class="btn btn-sm btn-primary" :disabled="!selectedOutcome || !selectedTarget" data-cy="connect-dialog-confirm">
                    {{ t("Connect") }}
                </button>
            </div>
        </form>
    </div>
</template>
