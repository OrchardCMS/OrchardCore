// <folder> component
Vue.component('folder', {
    template: '\
        <li :class="{selected: isSelected}" \
                v-on:dragleave.prevent = "handleDragLeave($event);" \
                v-on:dragover.prevent.stop="handleDragOver($event);" \
                v-on:drop.prevent.stop = "moveMediaToFolder(model, $event)" >\
            <div :class="{folderhovered: isHovered}" >\
                <a href="javascript:;" v-on:click="toggle" class="expand" :class="{opened: open, closed: !open, empty: empty}"><i class="fas fa-caret-right"></i></a>\
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
            parent: null,
            isHovered : false
        }
    },
    computed: {
        empty: function () {
            return !this.children || this.children.length == 0;
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
            parentFolder = mediaApp.selectedFolder;
            while (parentFolder) {
                if (parentFolder.path == this.model.path) {
                    return true;
                }
            parentFolder = parentFolder.parent;
            }

            return false;
        },
        toggle: function () {
            this.open = !this.open;
            if (this.open && !this.children) {
                this.loadChildren();
            }
        },
        select: function () {
            bus.$emit('folderSelected', this.model);
            this.loadChildren();
        },
        loadChildren: function () {            
            var self = this;
            if (this.open == false) {
                this.open = true;
            }
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
        },
        handleDragOver: function (e) {
            this.isHovered = true;
        },
        handleDragLeave: function (e) {
            this.isHovered = false;            
        },
        moveMediaToFolder: function (folder, e) {

            var self = this;
            self.isHovered = false;

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
