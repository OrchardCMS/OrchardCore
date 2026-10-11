<script setup lang="ts">
import { onMounted, ref, watch } from "vue";
import type { DesignerApi } from "../api/designerApi";
import type { DesignerField, StepFields } from "../api/types";
import { fieldReference, fieldTypeLabel } from "../preview/previewTable";
import { showToast } from "../ui/toasts";
import { t } from "../i18n";

// The Fields tab: the fields flowing into and out of the step, with their types. A field's reference can be copied,
// to use it in a formula.
const props = defineProps<{
    stepId: string;
    api: DesignerApi;
    // Changes when the step's settings were applied, so the fields are loaded again.
    refreshKey: number;
}>();

const loading = ref(false);
const failed = ref(false);
const fields = ref<StepFields | null>(null);

const load = async () => {
    loading.value = true;
    failed.value = false;

    try {
        fields.value = await props.api.getFields(props.stepId);
    } catch {
        failed.value = true;
    } finally {
        loading.value = false;
    }
};

const copy = async (field: DesignerField) => {
    const reference = fieldReference(field);

    try {
        await navigator.clipboard.writeText(reference);
        showToast({ message: t("FieldReferenceCopied", "{0} was copied.", reference), variant: "success" });
    } catch {
        showToast({ message: t("FieldReferenceCopyFailed", "The reference could not be copied: {0}", reference), variant: "warning" });
    }
};

watch(() => [props.stepId, props.refreshKey], load);

onMounted(load);

defineExpose({ load });
</script>

<template>
    <div class="dpd-fields" data-cy="fields-tab">
        <p v-if="loading && !fields" class="dpd-panel-message">
            <span class="spinner-border spinner-border-sm" aria-hidden="true"></span>
            {{ t("Loading", "Loading…") }}
        </p>
        <p v-else-if="failed" class="alert alert-danger m-2" role="alert" data-cy="fields-failed">{{ t("FieldsLoadFailed", "The fields could not be loaded.") }}</p>

        <template v-if="fields">
            <p class="dpd-panel-intro">{{ t("FieldsHint", "Copy a field's reference to use it in a formula.") }}</p>
            <div class="dpd-fields-columns">
                <section
                    v-for="group in [
                        { key: 'inputs', title: t('InputFields', 'Received'), ports: fields.inputs },
                        { key: 'outputs', title: t('OutputFields', 'Produced'), ports: fields.outputs },
                    ]"
                    :key="group.key"
                    class="dpd-fields-group"
                    :data-cy="`fields-${group.key}`"
                >
                    <h4 class="dpd-section-title">{{ group.title }}</h4>
                    <p v-if="group.ports.length === 0" class="dpd-section-hint">{{ group.key === "inputs" ? t("NoInputs", "This step has no input.") : t("NoOutputs", "This step has no output.") }}</p>
                    <div v-for="port in group.ports" :key="port.port" class="dpd-fields-port">
                        <div class="dpd-fields-port-name">
                            <i class="fa-solid fa-fw" :class="port.kind === 'Files' ? 'fa-file-lines' : 'fa-table'" aria-hidden="true"></i>
                            {{ port.displayName }}
                        </div>
                        <p v-if="port.kind === 'Files'" class="dpd-section-hint">{{ t("FilePortFields", "Files, without fields.") }}</p>
                        <p v-else-if="port.fields.length === 0" class="dpd-section-hint">{{ t("NoFieldsKnown", "No fields are known yet: check the settings of the steps before it.") }}</p>
                        <ul v-else class="dpd-fields-list">
                            <li v-for="field in port.fields" :key="field.name" class="dpd-field">
                                <button
                                    type="button"
                                    class="dpd-field-copy"
                                    :title="t('CopyFieldReference', 'Copy {0}', fieldReference(field))"
                                    :aria-label="t('CopyFieldReference', 'Copy {0}', fieldReference(field))"
                                    :data-cy="`field-${port.port}-${field.name}`"
                                    @click="copy(field)"
                                >
                                    <span class="dpd-field-name">{{ field.displayName || field.name }}</span>
                                    <code class="dpd-field-reference">{{ fieldReference(field) }}</code>
                                    <span class="dpd-field-type">{{ fieldTypeLabel(field.type) }}</span>
                                    <i class="fa-regular fa-copy dpd-field-icon" aria-hidden="true"></i>
                                </button>
                                <span v-if="field.group" class="dpd-field-group">{{ field.group }}</span>
                            </li>
                        </ul>
                    </div>
                </section>
            </div>
        </template>
    </div>
</template>
