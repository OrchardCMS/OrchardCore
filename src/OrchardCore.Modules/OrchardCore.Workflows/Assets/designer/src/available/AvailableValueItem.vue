<script setup lang="ts">
import type { AvailableValue } from "./availableData";
import { t } from "../i18n";

// A value of the available data: its name and type, what it is, its JavaScript and Liquid expressions, and its
// fields, which are listed the same way under it.
defineProps<{ value: AvailableValue; detail: (value: AvailableValue) => string; dataKey: string }>();

const emit = defineEmits<{ (event: "insert", text: string): void }>();

// The parts of an expression, which a narrow panel wraps after a dot rather than inside a name.
const partsOf = (expression: string) => expression.split(/(?<=\.)/);
</script>

<template>
    <li class="wfd-available-value" :data-cy="dataKey">
        <div class="wfd-available-head">
            <code class="wfd-available-name">{{ value.name }}</code>
            <span class="wfd-available-type">{{ detail(value) }}</span>
        </div>
        <div v-if="value.description" class="wfd-available-description">{{ value.description }}</div>
        <div class="wfd-available-expressions">
            <!-- The buttons don't take the focus, so the editor keeps its cursor. -->
            <button type="button" class="wfd-snippet" :title="t('InsertExpression', value.javaScript)" data-cy="available-javascript" @mousedown.prevent @click="emit('insert', value.javaScript)">
                <span class="wfd-snippet-syntax">JS</span>
                <code><template v-for="(part, index) in partsOf(value.javaScript)" :key="index"><wbr v-if="index > 0" />{{ part }}</template></code>
                <i class="fa-solid fa-copy wfd-snippet-icon" aria-hidden="true"></i>
            </button>
            <button type="button" class="wfd-snippet" :title="t('InsertExpression', value.liquid)" data-cy="available-liquid" @mousedown.prevent @click="emit('insert', value.liquid)">
                <span class="wfd-snippet-syntax">Liquid</span>
                <code><template v-for="(part, index) in partsOf(value.liquid)" :key="index"><wbr v-if="index > 0" />{{ part }}</template></code>
                <i class="fa-solid fa-copy wfd-snippet-icon" aria-hidden="true"></i>
            </button>
        </div>
        <details v-if="value.members && value.members.length > 0" class="wfd-available-members" data-cy="available-members">
            <summary>{{ t("AvailableFields", value.members.length) }}</summary>
            <ul class="list-unstyled">
                <AvailableValueItem
                    v-for="member in value.members"
                    :key="member.key"
                    :value="member"
                    :detail="detail"
                    :data-key="`${dataKey}.${member.name}`"
                    @insert="emit('insert', $event)"
                />
            </ul>
        </details>
    </li>
</template>
