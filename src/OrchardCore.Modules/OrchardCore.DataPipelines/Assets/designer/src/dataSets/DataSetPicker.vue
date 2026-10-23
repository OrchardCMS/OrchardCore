<script setup lang="ts">
import { computed, onMounted, ref, shallowRef } from "vue";
import ModalDialog from "@bloom/designer/ui/ModalDialog.vue";
import type { DataSetsResult, DesignerDataSet } from "../api/types";
import { t } from "../i18n";

// Chooses the data set a "Read a data source" step reads: the data sets of every data source the user may read, with
// their descriptions, filtered by words and by data source.
const props = defineProps<{
    load: () => Promise<DataSetsResult>;
    // The data set already chosen, such as "Contents::BlogPost", when the dialog changes it.
    current?: string | null;
}>();

const emit = defineEmits<{
    (event: "pick", dataSet: DesignerDataSet): void;
    (event: "close"): void;
}>();

const loading = ref(true);
const error = ref(false);
const dataSets = shallowRef<DesignerDataSet[]>([]);
const filter = ref("");
const category = ref("");

onMounted(async () => {
    try {
        dataSets.value = (await props.load()).dataSets ?? [];
    } catch {
        error.value = true;
    } finally {
        loading.value = false;
    }
});

const searchText = (dataSet: DesignerDataSet) =>
    [dataSet.displayName, dataSet.value, dataSet.description, dataSet.group, dataSet.source].filter(Boolean).join(" ").toLowerCase();

// The data sets that have every word of the filter, whatever their data source.
const matching = computed(() => {
    const terms = filter.value.trim().toLowerCase().split(/\s+/).filter(Boolean);

    return dataSets.value.filter((dataSet) => {
        const text = searchText(dataSet);

        return terms.every((term) => text.includes(term));
    });
});

// "All", then each data source, in the order of the data sources, with the number of matching data sets.
const categories = computed(() => {
    const counts = new Map<string, number>();

    for (const dataSet of matching.value) {
        counts.set(dataSet.source, (counts.get(dataSet.source) ?? 0) + 1);
    }

    const sources = [...new Set(dataSets.value.map((dataSet) => dataSet.source))];

    return [
        { name: "", label: t("AllDataSets", "All"), count: matching.value.length },
        ...sources.map((source) => ({ name: source, label: source, count: counts.get(source) ?? 0 })),
    ];
});

const visible = computed(() => (category.value ? matching.value.filter((dataSet) => dataSet.source === category.value) : matching.value));
</script>

<template>
    <ModalDialog :title="current ? t('ChangeDataSet', 'Change the data set') : t('AddDataSource', 'Add a data source')" size="xl" data-cy="data-set-picker" @close="emit('close')">
        <p v-if="current" class="text-body-secondary small">{{ t("ChangeDataSetHint", "Choosing another data set clears the fields and the filters of the step.") }}</p>
        <div class="row g-3">
            <div class="col-lg-3">
                <input
                    v-model="filter"
                    type="search"
                    class="form-control mb-3"
                    :placeholder="t('Filter', 'Filter')"
                    :aria-label="t('FilterDataSets', 'Filter the data sets')"
                    data-autofocus
                    data-cy="data-set-filter"
                />
                <nav class="nav nav-pills flex-nowrap flex-lg-column overflow-auto gap-1" :aria-label="t('DataSources', 'Data sources')">
                    <button
                        v-for="item in categories"
                        :key="item.name"
                        type="button"
                        class="nav-link text-start text-nowrap d-flex align-items-center gap-2"
                        :class="{ active: category === item.name }"
                        :aria-pressed="category === item.name"
                        :data-cy="`data-set-category-${item.name || 'all'}`"
                        @click="category = item.name"
                    >
                        <span class="flex-grow-1">{{ item.label }}</span>
                        <span class="badge rounded-pill" :class="category === item.name ? 'text-bg-light' : 'text-bg-secondary'">{{ item.count }}</span>
                    </button>
                </nav>
            </div>
            <div class="col-lg-9">
                <p v-if="loading" class="mb-0">
                    <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
                    {{ t("Loading", "Loading…") }}
                </p>
                <p v-else-if="error" class="text-danger mb-0" role="alert">{{ t("DataSetsLoadFailed", "The data sets could not be loaded.") }}</p>
                <template v-else>
                    <div class="row row-cols-1 row-cols-lg-2 row-cols-xl-3 g-2">
                        <div v-for="dataSet in visible" :key="dataSet.value" class="col" :data-cy="`data-set-${dataSet.value}`">
                            <div class="card h-100" :class="{ 'border-primary': dataSet.value === current }">
                                <div class="card-body">
                                    <h6 class="card-title d-flex align-items-baseline gap-2 mb-1">
                                        <i class="fa-solid fa-table fa-fw text-primary" aria-hidden="true"></i>
                                        <span>{{ dataSet.displayName }}</span>
                                    </h6>
                                    <p v-if="dataSet.description" class="card-text small text-body-secondary mb-0">{{ dataSet.description }}</p>
                                </div>
                                <div class="card-footer d-flex align-items-center gap-2">
                                    <div class="d-flex flex-wrap gap-1 me-auto">
                                        <span class="badge text-bg-light">{{ dataSet.source }}</span>
                                        <span v-if="dataSet.group" class="badge text-bg-light">{{ dataSet.group }}</span>
                                    </div>
                                    <span v-if="dataSet.value === current" class="small text-muted">{{ t("ChosenDataSet", "Chosen") }}</span>
                                    <button v-else type="button" class="btn btn-primary btn-sm" data-cy="data-set-choose" @click="emit('pick', dataSet)">
                                        {{ current ? t("Choose", "Choose") : t("Add", "Add") }}
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                    <p v-if="visible.length === 0" class="text-muted small py-3 text-center mb-0" data-cy="data-set-empty">
                        {{ dataSets.length === 0 ? t("NoDataSets", "No data set is available.") : t("NoMatchingDataSets", "No data set matches the filter.") }}
                    </p>
                </template>
            </div>
        </div>
        <template #footer>
            <button type="button" class="btn btn-secondary" data-cy="data-set-picker-cancel" @click="emit('close')">{{ t("Cancel", "Cancel") }}</button>
        </template>
    </ModalDialog>
</template>
