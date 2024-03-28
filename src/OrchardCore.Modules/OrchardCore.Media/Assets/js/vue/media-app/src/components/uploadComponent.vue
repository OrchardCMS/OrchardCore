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
    uploadInputId: String
  },
  mounted: function () {
    const me = this;
    const uploadInput = <HTMLInputElement>document.getElementById(me.uploadInputId ?? 'fileupload');

    // TODO: refactor as JQuery bind method is deprecated
    $(uploadInput).bind('fileuploadprogress', function (e, data) {
      if (data.files[0].name !== me.model.name) {
        return;
      }
      me.model.percentage = Math.round(data.loaded / data.total * 100);
    });

    // TODO: refactor as JQuery bind method is deprecated
    $(uploadInput).bind('fileuploaddone', function (e, data) {
      if (data.files[0].name !== me.model.name) {
        return;
      }
      if (data.result.files[0].error) {
        me.handleFailure(data.files[0].name, data.result.files[0].error);
      } else {
        me.emitter.emit('removalRequest', me.model);
      }
    });

    // TODO: refactor as JQuery bind method is deprecated
    $(uploadInput).bind('fileuploadfail', function (e, data) {
      if (data.files[0].name !== me.model.name) {
        return;
      }
      me.handleFailure(data.files[0].name, $('#t-error').val());
    });
  },
  methods: {
    handleFailure: function (fileName: any, message: any) {
      if (fileName !== this.model.name) {
        return;
      }

      this.model.errorMessage = message;
      this.emitter.emit('ErrorOnUpload', this.model);
    },
    dismissWarning: function () {
      this.emitter.emit('removalRequest', this.model);
    }
  }
});
</script>
