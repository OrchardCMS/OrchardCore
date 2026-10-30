<script setup lang="ts">
import { nextTick, ref, useId } from "vue";
import { t } from "../i18n";

// A row of a query string or of a form: the names the workflow reads are listed first, with a fixed name.
export interface NameValueRow {
    name: string;
    value: string;
    // Whether the workflow reads this name, found in its expressions.
    isKnown?: boolean;
}

defineProps<{
    // Names the rows for screen readers, such as "Query string".
    label: string;
    // The data-cy prefix of the rows and of the add button.
    cy: string;
}>();

const rows = defineModel<NameValueRow[]>({ required: true });

const ids = `wfd-rows-${useId()}`;
const list = ref<HTMLElement | null>(null);

const add = async () => {
    rows.value = [...rows.value, { name: "", value: "" }];
    await nextTick();
    list.value?.querySelector<HTMLInputElement>(`#${ids}-name-${rows.value.length - 1}`)?.focus();
};

const remove = (index: number) => {
    rows.value = rows.value.filter((_, i) => i !== index);
};
</script>

<template>
    <div ref="list" class="wfd-name-values" role="group" :aria-label="label">
        <div v-for="(row, index) in rows" :key="index" class="wfd-name-value" :data-cy="`${cy}-row-${index}`">
            <span v-if="row.isKnown" :id="`${ids}-name-${index}`" class="wfd-name-value-name font-monospace" :title="t('RunReadByTheWorkflow')" :data-cy="`${cy}-name-${index}`">
                {{ row.name }}
            </span>
            <input
                v-else
                :id="`${ids}-name-${index}`"
                v-model="row.name"
                type="text"
                class="form-control form-control-sm font-monospace"
                :placeholder="t('RunName')"
                :aria-label="t('RunName')"
                :data-cy="`${cy}-name-${index}`"
            />
            <input
                v-model="row.value"
                type="text"
                class="form-control form-control-sm"
                :placeholder="t('RunValue')"
                :aria-label="row.isKnown ? t('RunValueOf', row.name) : t('RunValue')"
                :aria-describedby="row.isKnown ? `${ids}-name-${index}` : undefined"
                :data-cy="`${cy}-value-${index}`"
            />
            <button
                v-if="!row.isKnown"
                type="button"
                class="btn btn-sm btn-link text-danger"
                :title="t('RunRemove')"
                :aria-label="t('RunRemove')"
                :data-cy="`${cy}-remove-${index}`"
                @click="remove(index)"
            >
                <i class="fa-solid fa-xmark" aria-hidden="true"></i>
            </button>
            <span v-else class="wfd-name-value-spacer" aria-hidden="true"></span>
        </div>
        <button type="button" class="btn btn-sm btn-link px-0" :data-cy="`${cy}-add`" @click="add">
            <i class="fa-solid fa-plus" aria-hidden="true"></i>
            {{ t("RunAddParameter") }}
        </button>
    </div>
</template>
