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
        <upload :upload-input-id="uploadInputId" v-for="f in files" :key="f.name" :model="f"></upload>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import UploadComponent from './uploadComponent.vue';

export default defineComponent({
  components: {
    Upload: UploadComponent,
  },
  name: "uploadList",
  data: function () {
    return {
      files: <any>[],
      expanded: false,
      pendingCount: 0,
      errorCount: 0
    }
  },
  props: {
    uploadInputId: {
      type: String,
      required: true
    },
    t: {
      type: Object,
      required: true
    }
  },
  created: function () {

  },
  computed: {
    fileCount: function () {
      return this.files.length;
    }
  },
  mounted: function () {
    const me = this;

    $('#' + me.uploadInputId).on('fileuploadadd', function (e: any, data: any) {
      if (!data.files) {
        return;
      }

      data.files.forEach(function (newFile: any) {
        var alreadyInList = me.files.some(function (f: any) {
          return f.name == newFile.name;
        });

        if (!alreadyInList) {
          me.files.push({ name: newFile.name, percentage: 0, errorMessage: '' });
        } else {
          console.error('A file with the same name is already on the queue:' + newFile.name);
        }
      });
    });

    this.emitter.on('removalRequest', (fileUpload: any) => {
      me.files.forEach(function (item: any, index: any, array: any) {
        if (item.name == fileUpload.name) {
          array.splice(index, 1);
        }
      });
    })

    this.emitter.on('ErrorOnUpload', () => {
      me.updateCount();
    })
  },
  methods: {
    fileUploadAdd: function (data: any, ev: any) {
      const me = this;

      if (!data.files) {
        return;
      }

      data.files.forEach(function (newFile: any) {
        let alreadyInList = me.files.some(function (f: any) {
          return f.name == newFile.name;
        });

        if (!alreadyInList) {
          me.files.push({ name: newFile.name, percentage: 0, errorMessage: '' });
        } else {
          console.error('A file with the same name is already on the queue:' + newFile.name);
        }
      });
    },
    updateCount: function () {
      this.errorCount = this.files.filter(function (item: any) {
        return item.errorMessage != '';
      }).length;

      this.pendingCount = this.files.length - this.errorCount;

      if (this.files.length < 1) {
        this.expanded = false;
      }
    },
    clearErrors: function () {
      this.files = this.files.filter(function (item: any) {
        return item.errorMessage == '';
      });
    }
  },
  watch: {
    files: function () {
      this.updateCount();
    }
  }
});
</script>
