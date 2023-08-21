// <upload> component
Vue.component('upload', {
    template: '\
        <div :class="{ \'upload-warning\' : model.errorMessage }" class="upload m-2 p-2 pt-0"> \
            <span v-if="model.errorMessage" v-on:click="dismissWarning()" class="close-warning"><i class="fa-solid fa-times" aria-hidden="true"></i> </span>\
            <p class="upload-name" :title="model.errorMessage">{{ model.name }}</p> \
            <div> \
               <span v-show="!model.errorMessage" :style="{ width: model.percentage + \'%\'}" class="progress-bar"> </span> \
               <span v-if="model.errorMessage" class="error-message" :title="model.errorMessage"> Error: {{ model.errorMessage }} </span> \
            </div> \
        </div> \
        ',
    props: {
        model: Object,
        uploadInputId: String
    },
    mounted: function () {
        var self = this;
        var uploadInput = document.getElementById(self.uploadInputId ?? 'fileupload');
        $(uploadInput).bind('fileuploadprogress', function (e, data) {
            if (data.files[0].name !== self.model.name) {
                return;
            }            
            self.model.percentage = parseInt(data.loaded / data.total * 100, 10);
        });

        $(uploadInput).bind('fileuploaddone', function (e, data) {
            if (data.files[0].name !== self.model.name) {
                return;
            }
            if (data.result.files[0].error) {
                self.handleFailure(data.files[0].name, data.result.files[0].error);
            } else {  
                bus.$emit('removalRequest', self.model);
            }
        });

        $(uploadInput).bind('fileuploadfail', function (e, data) {
            if (data.files[0].name !== self.model.name) {
                return;
            }
            self.handleFailure(data.files[0].name, $('#t-error').val());
        });
    },
    methods: {
        handleFailure: function (fileName, message) {
            if (fileName !== this.model.name) {
                return;
            }
            this.model.errorMessage = message;
            bus.$emit('ErrorOnUpload', this.model);
        },
        dismissWarning: function () {
            bus.$emit('removalRequest', this.model);
        }
    }
});
