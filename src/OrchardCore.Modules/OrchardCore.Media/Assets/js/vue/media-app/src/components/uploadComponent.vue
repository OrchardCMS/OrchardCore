<!--
    <upload> component
-->

<template>
  <div :class="{ 'upload-warning': model?.errorMessage }" class="upload p-2">
    <span v-if="model?.errorMessage" v-on:click="dismissWarning()" class="close-warning">
      <fa-icon icon="fa-solid fa-times"></fa-icon>
    </span>
    <p class="upload-name" :title="model?.errorMessage">{{ model?.name }}</p>
    <div>
      <span v-show="!model?.errorMessage" :style="{ width: model?.percentage + '%' }" class="progress-bar"></span>
      <span v-if="model?.errorMessage" class="error-message" :title="model.errorMessage">
        Error: {{ model.errorMessage }}
      </span>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'

export default defineComponent({
  name: "upload",
  props: {
    model: {
      type: Object,
      required: true,
    },
  },
  methods: {
    dismissWarning: function () {
      this.emitter.emit('removalRequest', this.model);
    }
  }
});
</script>
