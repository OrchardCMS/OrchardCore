// <upload-list> component
Vue.component('uploadList', {
    template: '\
        <div class="upload-list" v-show="files.length > 0"> \
            <div class="header" @click="expanded = !expanded"> \
                <span :class="{ \'text-danger\' : errorCount }"> {{ T.uploads }} ({{ fileCount }})</span> \
                    <div class="toggle-button"> \
                    <div v-show="expanded"> \
                        <i class="fa fa-chevron-down"></i> \
                    </div> \
                    <div v-show="!expanded"> \
                        <i class="fa fa-chevron-up"></i> \
                    </div> \
                </div> \
            </div> \
            <div class="card-body" v-show="expanded"> \
                <div class="d-flex flex-wrap"> \
                    <div v-for="f in files" :key="f.name" > <upload :model="f"></upload> </div > \
                </div > \
            </div> \
        </div> \
        ',
    data: function () {
        return {
            files: [],
            T: {},
            expanded: false,
            errorCount: 0
        }
    },
    created: function () {
        var self = this;
        // retrieving localized strings from view
        self.T.uploads = $('#t-uploads').val();
    },
    computed: {
        fileCount: function () {
            return this.files.length;
        }
    },
    mounted: function () {
        var self = this;

        $('#fileupload').bind('fileuploadadd', function (e, data) {
            if (!data.files) { 
                return;
            }
            data.files.forEach(function (f) {
                    self.files.push({ name: f.name, percentage: 0, errorMessage: ''});
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
            self.updateErrorCount();
        });
    },
    methods: {
        updateErrorCount: function () {
            var result = this.files.filter(function (item) {
                return item.errorMessage != '';
            }).length;
            this.errorCount = result;
        }
    }
});
