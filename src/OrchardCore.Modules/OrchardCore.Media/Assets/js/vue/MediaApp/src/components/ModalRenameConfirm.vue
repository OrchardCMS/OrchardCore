<script setup lang="ts">
import { ref } from 'vue'
import { VueFinalModal } from 'vue-final-modal'

const props = defineProps<{
    title?: string
    modalName: string
    fileName: string
}>()

let newFileName = ref(props.fileName);

const emit = defineEmits<{
    (e: 'confirm', fileName: string): void
}>()
</script>

<template>
    <VueFinalModal :modal-id="modalName" class="flex justify-center items-center"
        content-class="flex flex-col max-w-xl mx-4 p-4 bg-white dark:bg-gray-900 border dark:border-gray-700 rounded-lg space-y-2">
        <h1 class="text-xl">
            {{ title }}
        </h1>
        <slot />
        <div>
            <input type="text" name="renameFile" v-model="newFileName" />
        </div>
        <button class="mt-1 ml-auto px-2 border rounded-lg" @click="emit('confirm', newFileName)">
            Rename
        </button>
    </VueFinalModal>
</template>