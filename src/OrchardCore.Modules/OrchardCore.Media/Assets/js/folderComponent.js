// <folder> component
Vue.component('folder', {
    template: '\
        <li :class="{selected: isSelected}" v-on:dragenter.prevent="handleDragEnter($event);" v-on:dragleave.prevent="handleDragLeave($event);" ondragover="event.preventDefault();" v-on:drop.stop="moveMediaToFolder(model, $event)" >\
            <div>\
                <a href="javascript:;" v-on:click="toggle" class="expand" v-bind:class="{opened: open, closed: !open, empty: empty}"><i class="fas fa-caret-right"></i></a>\
                <a href="javascript:;" v-on:click="select" draggable="false" >\
                    <i class="folder fa fa-folder"></i>\
                    {{model.name}}\
                </a>\
            </div>\
            <ol v-show="open">\
                <folder v-for="folder in children"\
                        :key="folder.path"\
                        :model="folder" \
                        :selected-in-media-app="selectedInMediaApp">\
                </folder>\
            </ol>\
        </li>\
        ',
    props: {
        model: Object,
        selectedInMediaApp: Object
    },
    data: function () {
        return {
            open: false,
            children: null, // not initialized state (for lazy-loading)
            parent: null
        }
    },
    computed: {
        empty: function () {
            return this.children && this.children.length == 0;
        },
        isSelected: function () {
            return (this.selectedInMediaApp.name == this.model.name) && (this.selectedInMediaApp.path == this.model.path);
        }
    },
    mounted: function () {
        if ((this.isRoot() == false) && (this.isAncestorOfSelectedFolder())){
            this.toggle();
        }
    },
    created: function () {
        var self = this;
        bus.$on('deleteFolder', function (folder) {
            if (self.children) {
                var index = self.children && self.children.indexOf(folder)
                if (index > -1) {
                    self.children.splice(index, 1)
                    bus.$emit('folderDeleted');
                }
            }
        });

        bus.$on('addFolder', function (target, folder) {
            if (self.model == target) {

                bus.$emit('beforeFolderAdded', self.model);
                if (self.children !== null) {
                    self.children.push(folder);
                }                
                folder.parent = self.model;
                bus.$emit('folderAdded', folder);
            }
        });
    },
    methods: {
        isRoot: function () {
            return this.model.path === '';
        },
        isAncestorOfSelectedFolder: function () {
            return mediaApp.selectedFolder.path.indexOf(this.model.path) > -1;
        },
        toggle: function () {
            this.open = !this.open
            var self = this;
            if (this.open && !this.children) {
                $.ajax({
                    url: $('#getFoldersUrl').val() + "?path=" + encodeURIComponent(self.model.path),
                    method: 'GET',
                    success: function (data) {
                        self.children = data;
                        self.children.forEach(function (c) {
                            c.parent = self.model;
                        });
                    },
                    error: function (error) {
                        emtpy = false;
                        console.error(error.responseText);
                    }
                });
            }
        },
        select: function () {
            bus.$emit('folderSelected', this.model);
        },
        handleDragEnter: function (e) {                     
            if (e.target.classList.contains('folder-dragover') === false) {                
                e.target.classList.add('folder-dragover');
            }
        },
        handleDragLeave: function (e) {
            if (e.target.classList.contains('folder-dragover') === true) {                
                e.target.classList.remove('folder-dragover');
            }
        },
        moveMediaToFolder: function (folder, e) {
            if (e.target.classList.contains('folder-dragover') === true) {                
                e.target.classList.remove('folder-dragover');
            }

            var self = this;
            var mediaNames = JSON.parse(e.dataTransfer.getData('mediaNames')); 

            if (mediaNames.length < 1) {
                return;
            }

            var sourceFolder = e.dataTransfer.getData('sourceFolder');
            var targetFolder = folder.path;

            if (sourceFolder === '') {
                sourceFolder = 'root';
            }

            if (targetFolder === '') {
                targetFolder = 'root';
            }

            if (sourceFolder === targetFolder) {
                alert($('#sameFolderMessage').val());
                return;
            }

            if (!confirm($('#moveMediaMessage').val())) {
                return;
            }            

            $.ajax({
                url: $('#moveMediaListUrl').val(),
                method: 'POST',
                data: {
                    __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
                    mediaNames: mediaNames,
                    sourceFolder: sourceFolder,
                    targetFolder: targetFolder
                },
                success: function () {
                    bus.$emit('mediaListMoved'); // MediaApp will listen to this, and then it will reload page so the moved medias won't be there anymore
                },
                error: function (error) {
                    console.error(error.responseText);
                    bus.$emit('mediaListMoved', error.responseText);
                }
            });
        }
    }
});
