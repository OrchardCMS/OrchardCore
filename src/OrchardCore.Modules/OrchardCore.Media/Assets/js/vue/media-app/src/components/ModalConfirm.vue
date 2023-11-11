<script setup lang="ts">
import { VueFinalModal } from 'vue-final-modal'

const props = defineProps<{
    title?: string
    modalName: string
}>()

const emit = defineEmits<{
    (e: 'confirm'): void
}>()
</script>

<template>
    <VueFinalModal :modal-id="modalName" class="flex justify-center items-center"
        content-class="flex flex-col max-w-xl mx-4 p-4 bg-white dark:bg-gray-900 border dark:border-gray-700 rounded-lg space-y-2">
        <h1 class="text-xl">
            {{ title }}
        </h1>
        <slot />
        <button class="mt-1 ml-auto px-2 border rounded-lg" @click="emit('confirm')">
            Confirm
        </button>
    </VueFinalModal>
</template>