<!--
    <upload-toast> component — bottom-right toast notifications for file uploads
-->

<template>
  <div class="upload-toast-container" v-show="files.length > 0">
    <div class="upload-toast-header" @click="expanded = !expanded">
      <div class="upload-toast-title">
        <span>{{ t.Uploads }}</span>
        <span v-if="pendingCount > 0" class="ms-1">({{ pendingCount }})</span>
        <span v-if="errorCount > 0" class="text-danger ms-1">
          {{ t.Errors }}: {{ errorCount }}
        </span>
      </div>
      <div class="upload-toast-actions">
        <button v-if="errorCount > 0" class="btn btn-link btn-sm text-danger p-0 me-2"
          @click.stop="clearErrors">{{ t.ClearErrors }}</button>
        <button class="btn btn-link btn-sm p-0" @click.stop="expanded = !expanded">
          <fa-icon :icon="expanded ? 'fa-solid fa-chevron-down' : 'fa-solid fa-chevron-up'"></fa-icon>
        </button>
        <button v-if="pendingCount === 0" class="btn btn-link btn-sm p-0 ms-2" @click.stop="dismissAll">
          <fa-icon icon="fa-solid fa-times"></fa-icon>
        </button>
      </div>
    </div>
    <div class="upload-toast-body" v-show="expanded">
      <div v-for="f in files" :key="f.name" class="upload-toast-item"
        :class="{ 'is-error': f.errorMessage, 'is-success': f.success }">
        <div class="d-flex justify-content-between align-items-center">
          <span class="upload-toast-filename" :title="f.errorMessage || f.name">{{ f.name }}</span>
          <button v-if="f.errorMessage" class="btn btn-link btn-sm p-0 text-danger"
            @click="dismiss(f)">
            <fa-icon icon="fa-solid fa-times"></fa-icon>
          </button>
          <fa-icon v-else-if="f.success" icon="fa-solid fa-check" class="text-success"></fa-icon>
        </div>
        <div v-if="f.errorMessage" class="upload-toast-error text-danger">
          {{ f.errorMessage }}
        </div>
        <div v-else-if="!f.success" class="upload-toast-progress">
          <div class="upload-toast-progress-bar" :style="{ width: f.percentage + '%' }"></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'

interface UploadFile {
  name: string;
  percentage: number;
  errorMessage: string;
  success: boolean;
}

export default defineComponent({
  name: "uploadToast",
  props: {
    t: {
      type: Object,
      required: true,
    }
  },
  data() {
    return {
      files: [] as UploadFile[],
      expanded: true,
      pendingCount: 0,
      errorCount: 0,
    }
  },
  mounted() {
    const me = this;

    this.emitter.on('uploadFileAdded', (file: { name: string }) => {
      const existing = me.files.find((f) => f.name === file.name);
      if (!existing) {
        me.files.push({ name: file.name, percentage: 0, errorMessage: '', success: false });
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
      const file = me.files.find((f) => f.name === data.name);
      if (file) {
        file.success = true;
        file.percentage = 100;
      }

      // Auto-dismiss successful uploads after 3 seconds
      setTimeout(() => {
        me.files = me.files.filter((f) => !(f.name === data.name && f.success));
        me.updateCount();
      }, 3000);
    });

    this.emitter.on('uploadError', (data: { name: string; errorMessage: string }) => {
      const file = me.files.find((f) => f.name === data.name);
      if (file) {
        file.errorMessage = data.errorMessage;
      }
    });
  },
  methods: {
    updateCount() {
      this.errorCount = this.files.filter((f) => f.errorMessage !== '').length;
      this.pendingCount = this.files.filter((f) => !f.errorMessage && !f.success).length;

      if (this.files.length < 1) {
        this.expanded = false;
      }
    },
    clearErrors() {
      this.files = this.files.filter((f) => f.errorMessage === '');
    },
    dismiss(file: UploadFile) {
      this.files = this.files.filter((f) => f.name !== file.name);
    },
    dismissAll() {
      this.files = [];
    },
  },
  watch: {
    files: {
      handler() {
        this.updateCount();
      },
      deep: true,
    }
  }
});
</script>
