<script setup lang="ts">
import { nextTick, onBeforeUnmount, onMounted, ref } from "vue";
import type { MenuItem } from "./menu";

defineProps<{ x: number; y: number; items: MenuItem[]; label: string }>();

const emit = defineEmits<{ (event: "close"): void }>();

const menu = ref<HTMLElement | null>(null);

const buttons = () => Array.from(menu.value?.querySelectorAll<HTMLButtonElement>("[role=menuitem]") ?? []);

const focusAt = (index: number) => {
    const all = buttons();

    if (all.length > 0) {
        all[(index + all.length) % all.length].focus();
    }
};

const onKeyDown = (event: KeyboardEvent) => {
    const all = buttons();
    const index = all.indexOf(document.activeElement as HTMLButtonElement);

    switch (event.key) {
        case "ArrowDown":
            event.preventDefault();
            focusAt(index + 1);
            break;
        case "ArrowUp":
            event.preventDefault();
            focusAt(index - 1);
            break;
        case "Home":
            event.preventDefault();
            focusAt(0);
            break;
        case "End":
            event.preventDefault();
            focusAt(all.length - 1);
            break;
        case "Escape":
        case "Tab":
            event.preventDefault();
            event.stopPropagation();
            emit("close");
            break;
    }
};

const run = (item: MenuItem) => {
    emit("close");
    item.run();
};

const onOutside = (event: PointerEvent) => {
    if (menu.value && !menu.value.contains(event.target as Node)) {
        emit("close");
    }
};

onMounted(async () => {
    await nextTick();
    focusAt(0);
    document.addEventListener("pointerdown", onOutside, true);
});

onBeforeUnmount(() => document.removeEventListener("pointerdown", onOutside, true));
</script>

<template>
    <div
        ref="menu"
        class="dropdown-menu show wfd-context-menu"
        role="menu"
        :aria-label="label"
        :style="{ left: `${x}px`, top: `${y}px` }"
        data-cy="context-menu"
        @keydown="onKeyDown"
        @pointerdown.stop
        @contextmenu.prevent
    >
        <button
            v-for="item in items"
            :key="item.label"
            type="button"
            role="menuitem"
            class="dropdown-item"
            :class="{ 'text-danger': item.danger }"
            :data-cy="item.dataCy"
            @click="run(item)"
        >
            <i v-if="item.icon" :class="item.icon" class="me-2" aria-hidden="true"></i>{{ item.label }}
        </button>
    </div>
</template>
