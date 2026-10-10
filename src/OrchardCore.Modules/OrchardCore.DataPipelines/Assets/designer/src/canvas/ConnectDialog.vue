<script setup lang="ts">
import { computed, nextTick, onMounted, ref, watch } from "vue";
import type { DesignerConnection, DesignerNode } from "../api/types";
import { rejectConnection } from "../state/ports";
import { t } from "../i18n";

// The keyboard way to draw a connection: pick an output of the source step, then an input of the same kind of another
// step. Inputs that would make a loop aren't offered.
const props = defineProps<{ source: DesignerNode; nodes: DesignerNode[]; connections: DesignerConnection[]; port?: string }>();

const emit = defineEmits<{
    (event: "connect", connection: DesignerConnection): void;
    (event: "cancel"): void;
}>();

const selectedPort = ref(props.port ?? props.source.outputs[0]?.name ?? "");
const firstField = ref<HTMLSelectElement | null>(null);

const targets = computed(() =>
    props.nodes
        .filter((node) => node.id !== props.source.id)
        .flatMap((node) =>
            node.inputs
                .map((input) => ({ node, input, connection: { sourceStepId: props.source.id, sourcePort: selectedPort.value, targetStepId: node.id, targetPort: input.name } }))
                .filter((target) => rejectConnection(props.nodes, props.connections, target.connection) === null),
        )
        .map((target) => ({
            ...target,
            value: `${target.node.id}\n${target.input.name}`,
            label: target.node.inputs.length > 1 ? `${target.node.displayText}: ${target.input.displayName}` : target.node.displayText,
        })),
);

const selectedTarget = ref(targets.value[0]?.value ?? "");

watch(targets, (next) => {
    if (!next.some((target) => target.value === selectedTarget.value)) {
        selectedTarget.value = next[0]?.value ?? "";
    }
});

onMounted(async () => {
    await nextTick();
    firstField.value?.focus();
});

const submit = () => {
    const target = targets.value.find((item) => item.value === selectedTarget.value);

    if (target) {
        emit("connect", target.connection);
    }
};
</script>

<template>
    <div class="card shadow dpd-connect-dialog" role="dialog" aria-labelledby="dpd-connect-title" data-cy="connect-dialog" @pointerdown.stop @keydown.esc.prevent.stop="emit('cancel')">
        <form class="card-body" @submit.prevent="submit">
            <h3 id="dpd-connect-title" class="h6 card-title">{{ t("ConnectOutputOf", "Connect an output of {0}", source.displayText) }}</h3>
            <div class="mb-2">
                <label class="form-label" for="dpd-connect-port">{{ t("Output", "Output") }}</label>
                <select id="dpd-connect-port" ref="firstField" v-model="selectedPort" class="form-select form-select-sm" data-cy="connect-dialog-port">
                    <option v-for="item in source.outputs" :key="item.name" :value="item.name">{{ item.displayName }}</option>
                </select>
            </div>
            <div class="mb-3">
                <label class="form-label" for="dpd-connect-target">{{ t("ConnectTo", "To") }}</label>
                <select id="dpd-connect-target" v-model="selectedTarget" class="form-select form-select-sm" :disabled="targets.length === 0" data-cy="connect-dialog-target">
                    <option v-for="target in targets" :key="target.value" :value="target.value">{{ target.label }}</option>
                </select>
                <p v-if="targets.length === 0" class="form-text mb-0" data-cy="connect-dialog-none">{{ t("NoCompatibleInputs", "No step can receive this output.") }}</p>
            </div>
            <div class="d-flex gap-2 justify-content-end">
                <button type="button" class="btn btn-sm btn-secondary" data-cy="connect-dialog-cancel" @click="emit('cancel')">{{ t("Cancel", "Cancel") }}</button>
                <button type="submit" class="btn btn-sm btn-primary" :disabled="!selectedTarget" data-cy="connect-dialog-confirm">{{ t("Connect", "Connect") }}</button>
            </div>
        </form>
    </div>
</template>
