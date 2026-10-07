<script setup lang="ts">
import { computed } from "vue";
import type { Presence } from "./realtime";
import { t } from "../i18n";

const props = defineProps<{ presence: Presence[] }>();

// One avatar per person, even with several tabs open.
const people = computed(() => {
    const byUser = new Map<string, Presence>();

    for (const presence of props.presence) {
        byUser.set(presence.userId || presence.connectionId, presence);
    }

    return [...byUser.values()];
});

const nameOf = (presence: Presence) => presence.userName || t("AnotherUser");

const initialsOf = (presence: Presence) =>
    nameOf(presence)
        .split(/[\s._@-]+/)
        .filter(Boolean)
        .slice(0, 2)
        .map((part) => part[0]?.toUpperCase())
        .join("");
</script>

<template>
    <span v-if="people.length > 0" class="wfd-presence" :aria-label="t('AlsoHere', people.map(nameOf).join(', '))" data-cy="presence">
        <span class="wfd-presence-label small text-body-secondary">{{ t("AlsoHereLabel") }}</span>
        <span v-for="person in people" :key="person.userId || person.connectionId" class="wfd-presence-avatar" :title="nameOf(person)" data-cy="presence-avatar">
            {{ initialsOf(person) }}
        </span>
    </span>
</template>
