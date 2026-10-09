<script setup lang="ts">
import { computed } from "vue";
import type { Library, LibraryActivity } from "../api/types";
import { toolboxKey } from "../api/presets";
import { t } from "../i18n";

// What an empty workflow shows: every workflow starts with an event, so it offers the common ones.
const props = defineProps<{ library: Library | null }>();

const emit = defineEmits<{ (event: "pick", key: string): void }>();

// The events most workflows start with, in this order, when their feature is enabled.
const SUGGESTED = ["ContentPublishedEvent", "ContentCreatedEvent", "HttpRequestEvent", "TimerEvent", "UserCreatedEvent", "SignalEvent"];
const MAX = 6;

const events = computed<LibraryActivity[]>(() => {
    const all = (props.library?.categories ?? []).flatMap((category) => category.activities).filter((activity) => activity.isEvent && !activity.preset);
    const suggested = SUGGESTED.map((name) => all.find((activity) => activity.name === name)).filter((activity): activity is LibraryActivity => !!activity);
    const others = all.filter((activity) => !suggested.includes(activity)).sort((a, b) => a.displayText.localeCompare(b.displayText));

    return [...suggested, ...others].slice(0, MAX);
});
</script>

<template>
    <section v-if="events.length > 0" class="wfd-start-picker" :aria-labelledby="'wfd-start-picker-title'" data-cy="start-picker">
        <h3 id="wfd-start-picker-title" class="wfd-start-picker-title">{{ t("StartWithAnEvent") }}</h3>
        <p class="wfd-start-picker-hint">{{ t("StartWithAnEventHint") }}</p>
        <div class="wfd-start-picker-events">
            <button
                v-for="activity in events"
                :key="toolboxKey(activity)"
                type="button"
                class="wfd-start-picker-event"
                :data-cy="`start-picker-${activity.name}`"
                @click="emit('pick', toolboxKey(activity))"
            >
                <i :class="activity.icon || 'fa-solid fa-bolt'" aria-hidden="true"></i>
                <span>{{ activity.displayText }}</span>
            </button>
        </div>
    </section>
</template>
