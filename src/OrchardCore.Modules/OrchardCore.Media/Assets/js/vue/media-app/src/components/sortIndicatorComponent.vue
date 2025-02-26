<!-- 
    <sort-indicator> component
-->

<template>
  <div v-show="isActive" class="sort-indicator">
    <span v-show="asc"><i class="small fa fa-chevron-up"></i></span>
    <span v-show="!asc"><i class="small fa fa-chevron-down"></i></span>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';

export default defineComponent({
  name: "sortIndicator",
  props: {
    colname: {
      type: String,
      required: true,
    },
    selectedcolname: {
      type: String,
      required: true,
    },
    asc: Boolean
  },
  computed: {
    isActive: function () {
      return this.colname.toLowerCase() == this.selectedcolname.toLowerCase();
    }
  }
});
</script>
