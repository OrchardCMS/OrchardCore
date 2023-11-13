<script setup lang="ts">
import { ref } from 'vue'
import { VueFinalModal } from 'vue-final-modal'

const props = defineProps<{
  title?: string
  modalName: string
  newName: string
  actionName: string
  t: any
}>()

let showModal = ref(false);
let newName = ref(props.newName);

const emit = defineEmits<{
  (e: 'confirm', fileName: string): void
}>()
</script>

<template>
  <VueFinalModal v-model="showModal" :modal-id="modalName" class="flex justify-center items-center"
    content-class="flex flex-column max-w-xl mx-4 p-4 bg-white dark:bg-gray-900 border dark:border-gray-700 rounded-lg space-y-2">
    <div class="flex justify-content-between">
      <span class="modal__title">
        {{ title }}
      </span>
      <span class="cursor-pointer" @click="showModal = false">
        <fa-icon icon="fa-solid fa-xmark fa-2xl"></fa-icon>
      </span>
    </div>
    <slot />
    <div>
      <input class="p-inputtext p-component w-full" type="text" name="rename" v-model="newName" />
    </div>
    <div class="mt-3 flex flex-row justify-content-end">
      <button class="btn btn-secondary" @click="showModal = false">
        {{ t.Cancel }}
      </button>
      <button class="ml-2 btn btn-primary" @click="emit('confirm', newName)">
        {{ actionName }}
      </button>
    </div>
  </VueFinalModal>
</template>