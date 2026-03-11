<template>
  <div v-if="isActive" class="sort-indicator">
    <span v-if="asc"><i class="tw:text-sm fa fa-chevron-up"></i></span>
    <span v-if="!asc"><i class="tw:text-sm fa fa-chevron-down"></i></span>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps({
  colname: {
    type: String,
    required: true,
  },
  selectedcolname: {
    type: String,
    required: true,
  },
  asc: Boolean
})

const isActive = computed(() => {
  return props.colname.toLowerCase() == props.selectedcolname.toLowerCase();
});
</script>
