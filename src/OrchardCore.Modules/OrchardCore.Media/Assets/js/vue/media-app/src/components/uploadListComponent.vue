<!--
    <upload-list> component
-->

<template>
  <div class="upload-list" v-show="files.length > 0">
    <div class="header" @click="expanded = !expanded">
      <div>
        <span> {{ t.Uploads }} </span>
        <span v-show="pendingCount"> (Pending: {{ pendingCount }}) </span>
        <span v-show="errorCount" :class="{ 'text-danger': errorCount }"> ( {{ t.Errors }}: {{ errorCount }} / <a
            href="javascript:void(0)" v-on:click.stop="clearErrors"> {{ t.ClearErrors }} </a>)</span>
      </div>
      <div class="toggle-button">
        <div v-show="expanded">
          <fa-icon icon="fa-solid fa-chevron-down"></fa-icon>
        </div>
        <div v-show="!expanded">
          <fa-icon icon="fa-solid fa-chevron-up"></fa-icon>
        </div>
      </div>
    </div>
    <div class="card-body" v-show="expanded">
      <div class="d-grid p-2">
        <upload v-for="f in files" :key="f.name" :model="f"></upload>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import UploadComponent from './uploadComponent.vue';

interface UploadFile {
  name: string;
  percentage: number;
  errorMessage: string;
}

export default defineComponent({
  components: {
    Upload: UploadComponent,
  },
  name: "uploadList",
  data: function () {
    return {
      files: [] as UploadFile[],
      expanded: false,
      pendingCount: 0,
      errorCount: 0
    }
  },
  props: {
    t: {
      type: Object,
      required: true
    }
  },
  computed: {
    fileCount: function () {
      return this.files.length;
    }
  },
  mounted: function () {
    const me = this;

    this.emitter.on('uploadFileAdded', (file: { name: string }) => {
      const alreadyInList = me.files.some((f) => f.name === file.name);

      if (!alreadyInList) {
        me.files.push({ name: file.name, percentage: 0, errorMessage: '' });
        me.expanded = true;
      }
    });

    this.emitter.on('uploadProgress', (data: { name: string; percentage: number }) => {
      const file = me.files.find((f) => f.name === data.name);
      if (file) {
        file.percentage = data.percentage;
      }
    });

    this.emitter.on('uploadSuccess', (data: { name: string }) => {
      me.files = me.files.filter((f) => f.name !== data.name);
    });

    this.emitter.on('uploadError', (data: { name: string; errorMessage: string }) => {
      const file = me.files.find((f) => f.name === data.name);
      if (file) {
        file.errorMessage = data.errorMessage;
      }
    });

    this.emitter.on('removalRequest', (fileUpload: { name: string }) => {
      me.files = me.files.filter((f) => f.name !== fileUpload.name);
    });
  },
  methods: {
    updateCount: function () {
      this.errorCount = this.files.filter((item) => item.errorMessage !== '').length;
      this.pendingCount = this.files.length - this.errorCount;

      if (this.files.length < 1) {
        this.expanded = false;
      }
    },
    clearErrors: function () {
      this.files = this.files.filter((item) => item.errorMessage === '');
    }
  },
  watch: {
    files: function () {
      this.updateCount();
    }
  }
});
</script>
