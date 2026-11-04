<script setup lang="ts">
import type { Outcome } from "../api/types";
import { t } from "../i18n";

const props = defineProps<{ activityId: string; outcome: Outcome; readOnly: boolean; connected: boolean }>();

const emit = defineEmits<{
    (event: "start", pointerEvent: PointerEvent): void;
    (event: "activate"): void;
}>();

const onPointerDown = (event: PointerEvent) => {
    // A port starts a connection, never a node drag.
    event.stopPropagation();

    if (!props.readOnly && event.button === 0) {
        event.preventDefault();
        emit("start", event);
    }
};
</script>

<template>
    <button
        type="button"
        class="wfd-port"
        :class="{ 'is-connected': connected }"
        :disabled="readOnly"
        :data-outcome="outcome.name"
        :data-cy="`port-${activityId}-${outcome.name}`"
        :aria-label="t('ConnectOutcome', outcome.displayName)"
        :title="readOnly ? outcome.displayName : t('ConnectOutcome', outcome.displayName)"
        @pointerdown="onPointerDown"
        @click.stop
        @keydown.enter.prevent.stop="emit('activate')"
        @keydown.space.prevent.stop="emit('activate')"
    >
        <span class="wfd-port-label">{{ outcome.displayName }}</span>
        <span class="wfd-port-dot" data-port-dot aria-hidden="true"></span>
    </button>
</template>
