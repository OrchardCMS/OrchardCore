<script setup lang="ts">
import { computed } from "vue";
import type { Library } from "../api/types";
import { sourceSteps, stepIcon } from "../toolbox/filter";
import { t } from "../i18n";

// What an empty pipeline shows: every pipeline starts with a source, so it offers them.
const props = defineProps<{ library: Library | null }>();

const emit = defineEmits<{ (event: "pick", type: string): void }>();

const MAX = 8;

const sources = computed(() => sourceSteps(props.library).slice(0, MAX));
</script>

<template>
    <section v-if="sources.length > 0" class="dpd-start-picker" aria-labelledby="dpd-start-picker-title" data-cy="start-picker">
        <h3 id="dpd-start-picker-title" class="dpd-start-picker-title">{{ t("StartWithASource", "Start with a source") }}</h3>
        <p class="dpd-start-picker-hint">{{ t("StartWithASourceHint", "A pipeline reads its rows from a source, then transforms them and sends them to destinations.") }}</p>
        <div class="dpd-start-picker-steps">
            <button
                v-for="step in sources"
                :key="step.name"
                type="button"
                class="dpd-start-picker-step"
                :title="step.description"
                :data-cy="`start-picker-${step.name}`"
                @click="emit('pick', step.name)"
            >
                <i class="fa-fw" :class="stepIcon(step)" aria-hidden="true"></i>
                <span>{{ step.displayText }}</span>
            </button>
        </div>
    </section>
</template>
