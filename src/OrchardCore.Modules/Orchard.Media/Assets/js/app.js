var root = {
    name: 'Media Library',
    path: '',
    folder: '',
    isDirectory: true
}

var bus = new Vue();


// define the folder component
Vue.component('folder', {
    template: '#folder-template',
    props: {
        model: Object
    },
    data: function () {
        return {
            open: false,
            children: null,
            parent: null,
            selected: false
        }
    },
    computed: {
        empty: function () {
            return this.children && this.children.length == 0;
        }
    },
    created: function () {
        var self = this;
        bus.$on('delete', function (folder) {
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
                self.children.push(folder);
                bus.$emit('folderAdded', folder);
            }
        });
        
        bus.$on('folderSelected', function (folder) {
            self.selected = self.model == folder;
        });
    },
    methods: {
        toggle: function () {
            this.open = !this.open
            var self = this;
            if (this.open && !this.children) {
                $.ajax({
                    url: $('#getFoldersUrl').val() + "?path=" + encodeURIComponent(self.model.path),
                    method: 'GET',
                    success: function (data) {
                        self.children = data;
                        self.children.forEach(function(c) { 
                            c.parent = self.model; 
                        });
                    },
                    error: function (error) {
                        emtpy = false;
                        alert(JSON.stringify(error));
                    }
                });
            }
        },
        select: function () {
            mediaApp.selectFolder(this.model);
        }
    }
});

var mediaApp = new Vue({
    el: '#mediaApp',
    data: {
        selectedFolder: root,
        mediaItems: [],
        selectedMedia: null
    },
    created: function() {
        var self = this;

        bus.$on('folderDeleted', function () {
            self.selectRoot();
        });

        bus.$on('folderAdded', function (folder) {
            self.selectFolder(folder);
        });
    },
    computed: {
        isHome: function () {
            return this.selectedFolder == root;
        },
        parents: function () {
            var p = [];
            parent = this.selectedFolder;
            while (parent && parent != root) {
                p.unshift(parent);
                parent = parent.parent;
            }
            return p;
        },
        root: function () {
            return root;
        }
    },
    mounted: function() {
        this.selectRoot();
    },
    methods: {
        selectFolder: function (folder) {
            this.selectedFolder = folder;
            this.loadFolder(folder);
            bus.$emit('folderSelected', folder);
        },
        uploadUrl: function () {
            return this.selectedFolder ? $('#uploadFiles').val() + "?path=" + encodeURIComponent(this.selectedFolder.path) : null;
        },
        selectRoot: function () {
            this.selectFolder(this.root);
        },
        loadFolder: function (folder) {
            this.selectedMedia = null;
            var self = this;
            $.ajax({
                url: $('#getMediaItemsUrl').val() + "?path=" + encodeURIComponent(folder.path),
                method: 'GET',
                success: function (data) {
                    data.forEach(function (item) {
                        item.open = false;
                    });
                    self.mediaItems = data;
                },
                error: function (error) {
                    alert(JSON.stringify(error));
                }
            });
        },
        selectMedia: function (media) {
            this.selectedMedia = media;
        },
        deleteFolder: function () {
            var folder = this.selectedFolder
            var self = this;
            // The root folder can't be deleted
            if (folder == this.root.model) {
                return;
            }

            if (!confirm($('#deleteMessage').val())) {
                return;
            }

            $.ajax({
                url: $('#deleteFolderUrl').val() + "?path=" + encodeURIComponent(folder.path),
                method: 'POST',
                data: {
                    __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                },
                success: function (data) {
                    bus.$emit('delete', folder);
                },
                error: function (error) {
                    alert(JSON.stringify(error));
                }
            });
        },
        createFolder: function () {
            $('#createFolderModal').modal('show');
            $('.modal-body input').val('').focus();

            $('#modalFooterOk').on('click', function (e) {
                var name = $('.modal-body input').val();

                $.ajax({
                    url: $('#createFolderUrl').val() + "?path=" + encodeURIComponent(mediaApp.selectedFolder.path) + "&name=" + encodeURIComponent(name),
                    method: 'POST',
                    data: {
                        __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                    },
                    success: function (data) {
                        bus.$emit('addFolder', mediaApp.selectedFolder, data);
                        $('#createFolderModal').modal('hide');
                    },
                    error: function (error) {
                        alert(JSON.stringify(error));
                    }
                });
            });            
        }
    }
});
