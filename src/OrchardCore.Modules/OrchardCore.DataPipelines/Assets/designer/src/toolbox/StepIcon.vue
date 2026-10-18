<script setup lang="ts">
import type { StepCategory } from "../api/types";
import { categoryIcon } from "./filter";

// The icon of a step type: the DataPipelineStepIcon shape the server renders, which a template of the step type
// customizes, or the icon of its category when the step type is not available.
defineProps<{ step: { iconHtml?: string | null; category: StepCategory } }>();
</script>

<template>
    <!-- The HTML comes from the server's shape templates, like the summaries of the steps. -->
    <span v-if="step.iconHtml" class="dpd-step-icon" aria-hidden="true" v-html="step.iconHtml"></span>
    <i v-else class="dpd-step-icon" :class="categoryIcon(step.category)" aria-hidden="true"></i>
</template>
