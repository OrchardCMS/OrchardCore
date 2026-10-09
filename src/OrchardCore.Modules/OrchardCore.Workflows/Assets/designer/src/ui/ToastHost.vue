<script setup lang="ts">
import { dismissToast, useToasts, type Toast } from "./toasts";
import { t } from "../i18n";

const toasts = useToasts();

const runAction = (toast: Toast) => {
    dismissToast(toast.id);
    toast.action?.run();
};
</script>

<template>
    <div class="wfd-toasts toast-container" aria-live="polite" aria-atomic="false" data-cy="toasts">
        <div
            v-for="toast in toasts"
            :key="toast.id"
            class="toast show align-items-center border-0"
            :class="`text-bg-${toast.variant === 'info' ? 'dark' : toast.variant}`"
            :role="toast.variant === 'danger' ? 'alert' : 'status'"
            data-cy="toast"
        >
            <div class="d-flex align-items-center">
                <div class="toast-body">{{ toast.message }}</div>
                <button
                    v-if="toast.action"
                    type="button"
                    class="btn btn-sm btn-link text-reset fw-semibold"
                    data-cy="toast-action"
                    @click="runAction(toast)"
                >
                    {{ toast.action.label }}
                </button>
                <button
                    type="button"
                    class="btn-close me-2 m-auto"
                    :class="{ 'btn-close-white': toast.variant !== 'warning' }"
                    :aria-label="t('Close')"
                    @click="dismissToast(toast.id)"
                ></button>
            </div>
        </div>
    </div>
</template>
