<!-- 
    <upload-list> component 
-->
<template>
    <div class="upload-list" v-show="files.length > 0">
        <div class="header" @click="expanded = !expanded">
            <span> {{ T.uploads }} </span>
            <span v-show="pendingCount"> (Pending: {{ pendingCount }}) </span>
            <span v-show="errorCount" :class="{ 'text-danger': errorCount }"> ( {{ T.errors }}: {{ errorCount }} / <a
                    href="javascript:;" v-on:click.stop="clearErrors"> {{ T.clearErrors }} </a>)</span>
            <div class="toggle-button">
                <div v-show="expanded">
                    <i class="fa-solid fa-chevron-down" aria-hidden="true"></i>
                </div>
                <div v-show="!expanded">
                    <i class="fa-solid fa-chevron-up" aria-hidden="true"></i>
                </div>
            </div>
        </div>
        <div class="card-body" v-show="expanded">
            <div class="d-flex flex-wrap">
                <upload :upload-input-id="uploadInputId" v-for="f in files" :key="f.name" :model="f"></upload>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import UploadComponent from './uploadComponent.vue';

export default defineComponent({
    components: {
        Upload: UploadComponent,
    },
    name: "uploadList",
    data: function () {
        return {
            files: [],
            T: {},
            expanded: false,
            pendingCount: 0,
            errorCount: 0
        }
    },
    props: {
        uploadInputId: String
    },
    created: function () {
        // retrieving localized strings from view
        this.T.uploads = (<HTMLInputElement>document.getElementById('t-uploads'))?.value;
        this.T.errors = (<HTMLInputElement>document.getElementById('t-errors'))?.value;
        this.T.clearErrors = (<HTMLInputElement>document.getElementById('t-clear-errors'))?.value;;
    },
    computed: {
        fileCount: function () {
            return this.files.length;
        }
    },
    mounted: function () {
        let self = this;
        let uploadInput = document.getElementById(self.uploadInputId ?? 'fileupload');

        uploadInput?.addEventListener('fileuploadadd', this.fileUploadAdd);

        this.emitter.on('removalRequest', (fileUpload: { name: any; }) => {
            self.files.forEach(function (item, index, array) {
                if (item.name == fileUpload.name) {
                    array.splice(index, 1);
                }
            });
        })

        this.emitter.on('ErrorOnUpload', () => {
            self.updateCount();
        })
    },
    methods: {
        fileUploadAdd: function (data: any, ev: Event) {
            let self = this;

            if (!data.files) {
                return;
            }
            data.files.forEach(function (newFile: { name: string; }) {
                let alreadyInList = self.files.some(function (f) {
                    return f.name == newFile.name;
                });

                if (!alreadyInList) {
                    self.files.push({ name: newFile.name, percentage: 0, errorMessage: '' });
                } else {
                    console.error('A file with the same name is already on the queue:' + newFile.name);
                }
            });
        },
        updateCount: function () {
            this.errorCount = this.files.filter(function (item) {
                return item.errorMessage != '';
            }).length;
            this.pendingCount = this.files.length - this.errorCount;
            if (this.files.length < 1) {
                this.expanded = false;
            }
        },
        clearErrors: function () {
            this.files = this.files.filter(function (item) {
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
