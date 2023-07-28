// <upload-list> component
Vue.component('uploadList', {
    template: '\
        <div class="upload-list" v-show="files.length > 0"> \
            <div class="header" @click="expanded = !expanded"> \
                <span> {{ T.uploads }} </span> \
                <span v-show="pendingCount"> (Pending: {{ pendingCount }}) </span> \
                <span v-show="errorCount" :class="{ \'text-danger\' : errorCount }"> ( {{ T.errors }}: {{ errorCount }} / <a href="javascript:;" v-on:click.stop="clearErrors" > {{ T.clearErrors }} </a>)</span> \
                    <div class="toggle-button"> \
                    <div v-show="expanded"> \
                        <i class="fa-solid fa-chevron-down" aria-hidden="true"></i> \
                    </div> \
                    <div v-show="!expanded"> \
                        <i class="fa-solid fa-chevron-up" aria-hidden="true"></i> \
                    </div> \
                </div> \
            </div> \
            <div class="card-body" v-show="expanded"> \
                <div class="d-flex flex-wrap"> \
                    <upload :upload-input-id="uploadInputId" v-for="f in files" :key="f.name"  :model="f"></upload> \
                </div > \
            </div> \
        </div> \
        ',
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
        var self = this;
        // retrieving localized strings from view
        self.T.uploads = $('#t-uploads').val();
        self.T.errors = $('#t-errors').val();
        self.T.clearErrors = $('#t-clear-errors').val();
    },
    computed: {
        fileCount: function () {
            return this.files.length;
        }
    },
    mounted: function () {
        var self = this;
        var uploadInput = document.getElementById(self.uploadInputId ?? 'fileupload');
        $(uploadInput).bind('fileuploadadd', function (e, data) {
            if (!data.files) { 
                return;
            }
            data.files.forEach(function (newFile) {                
                var alreadyInList = self.files.some(function (f) {
                    return f.name == newFile.name;
                });

                if (!alreadyInList) {
                    self.files.push({ name: newFile.name, percentage: 0, errorMessage: '' });
                } else {
                    console.error('A file with the same name is already on the queue:' + newFile.name);
                }         
            });            
        });

        bus.$on('removalRequest', function (fileUpload) {
            self.files.forEach(function (item, index, array) {
                if (item.name == fileUpload.name) {
                    array.splice(index, 1);
                }
            });
        });

        bus.$on('ErrorOnUpload', function (fileUpload) {
            self.updateCount();
        });
    },
    methods: {
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
