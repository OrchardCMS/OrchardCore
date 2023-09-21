<!-- 
    <upload> component
-->

<template>
    <div :class="{ 'upload-warning': model?.errorMessage }" class="upload m-2 p-2 pt-0">
        <span v-if="model?.errorMessage" v-on:click="dismissWarning()" class="close-warning"><i class="fa-solid fa-times"
                aria-hidden="true"></i> </span>
        <p class="upload-name" :title="model?.errorMessage">{{ model?.name }}</p>
        <div>
            <span v-show="!model?.errorMessage" :style="{
                width: model?.percentage + '%'
            }" class="progress-bar"> </span>
            <span v-if="model?.errorMessage" class="error-message" :title="model.errorMessage"> Error: {{ model.errorMessage
            }} </span>
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
        let self = this;
        const uploadInput = document.getElementById(self.uploadInputId ?? 'fileupload');

        uploadInput.addEventListener('fileuploadprogress', (data) => {
            if (data.files[0].name !== self.model.name) {
                return;
            }
            
            self.model.percentage = parseInt(data.loaded / data.total * 100, 10);
        });

        uploadInput.addEventListener('fileuploaddone', (data) => {
            if (data.files[0].name !== self.model.name) {
                return;
            }

            if (data.result.files[0].error) {
                self.handleFailure(data.files[0].name, data.result.files[0].error);
            } else {
                this.emitter.emit('removalRequest', self.model);
            }
        });

        uploadInput.addEventListener('fileuploadfail', (data) => {
            if (data.files[0].name !== self.model.name) {
                return;
            }

            self.handleFailure(data.files[0].name, document.getElementById('t-error').value);
        });
    },
    methods: {
        handleFailure: function (fileName, message) {
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
