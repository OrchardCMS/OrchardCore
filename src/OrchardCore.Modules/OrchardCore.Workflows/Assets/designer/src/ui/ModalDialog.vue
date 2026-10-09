<script setup lang="ts">
import { nextTick, onBeforeUnmount, onMounted, ref, useId } from "vue";
import { t } from "../i18n";

// A Bootstrap modal rendered by Vue, for the dialogs that need more than the admin theme's OK/Cancel
// confirmDialog. Like that one, it is rendered at the end of <body>, above the admin navigation bar. It keeps
// the focus inside while open and gives it back when closed.
defineOptions({ inheritAttrs: false });

// The size is Bootstrap's: 'lg' or 'xl' for a dialog with a table, the default width otherwise.
defineProps<{ title: string; size?: "lg" | "xl" }>();

const emit = defineEmits<{ (event: "close"): void }>();

const titleId = `wfd-dialog-${useId()}`;
const dialog = ref<HTMLElement | null>(null);
let previousFocus: HTMLElement | null = null;

const focusable = () => Array.from(dialog.value?.querySelectorAll<HTMLElement>("button:not([disabled]), [href], input, select, textarea, [tabindex]:not([tabindex='-1'])") ?? []);

const onKeyDown = (event: KeyboardEvent) => {
    if (event.key === "Escape") {
        event.preventDefault();
        event.stopPropagation();
        emit("close");
    } else if (event.key === "Tab") {
        const elements = focusable();

        if (elements.length === 0) {
            return;
        }

        const first = elements[0];
        const last = elements[elements.length - 1];

        if (event.shiftKey && document.activeElement === first) {
            event.preventDefault();
            last.focus();
        } else if (!event.shiftKey && document.activeElement === last) {
            event.preventDefault();
            first.focus();
        }
    }
};

onMounted(async () => {
    previousFocus = document.activeElement instanceof HTMLElement ? document.activeElement : null;
    await nextTick();
    (dialog.value?.querySelector<HTMLElement>("[data-autofocus]") ?? focusable()[0])?.focus();
});

onBeforeUnmount(() => previousFocus?.focus());
</script>

<template>
    <Teleport to="body">
        <div class="wfd-modal-host" v-bind="$attrs">
            <div class="modal-backdrop fade show"></div>
            <div ref="dialog" class="modal fade show d-block" tabindex="-1" role="dialog" aria-modal="true" :aria-labelledby="titleId" @keydown="onKeyDown">
                <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable" :class="size ? `modal-${size}` : null">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h2 :id="titleId" class="modal-title h5">{{ title }}</h2>
                            <button type="button" class="btn-close" :aria-label="t('Close')" data-cy="dialog-close" @click="emit('close')"></button>
                        </div>
                        <div class="modal-body">
                            <slot></slot>
                        </div>
                        <div class="modal-footer">
                            <slot name="footer"></slot>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </Teleport>
</template>
