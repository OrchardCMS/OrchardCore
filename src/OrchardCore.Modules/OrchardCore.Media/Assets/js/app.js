var initialized;
var mediaApp;

var root = {
    name: 'Media Library',
    path: '',
    folder: '',
    isDirectory: true
}

var bus = new Vue();

function initializeMediaApplication(displayMediaApplication, mediaApplicationUrl) {

    if (initialized) {
        return;
    }

    initialized = true;

    if (!mediaApplicationUrl) {
        console.error('mediaApplicationUrl variable is not defined');
    }

    $.ajax({
        url: mediaApplicationUrl,
        method: 'GET',
        success: function (content) {
            $('.ta-content').append(content);

            $(document).trigger('mediaapplication:ready');

            mediaApp = new Vue({
                el: '#mediaApp',
                data: {
                    selectedFolder: root,
                    mediaItems: [],
                    selectedMedia: null
                },
                created: function () {
                    var self = this;

                    bus.$on('folderDeleted', function () {
                        self.selectRoot();
                    });

                    bus.$on('folderAdded', function (folder) {
                        self.selectFolder(folder);
                        folder.selected = true;
                    });

                    bus.$on('beforeFolderAdded', function (folder) {
                        self.loadFolder(folder);
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
                mounted: function () {
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
                                console.error(error.responseText);
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

                        if (!confirm($('#deleteFolderMessage').val())) {
                            return;
                        }

                        $.ajax({
                            url: $('#deleteFolderUrl').val() + "?path=" + encodeURIComponent(folder.path),
                            method: 'POST',
                            data: {
                                __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                            },
                            success: function (data) {
                                bus.$emit('deleteFolder', folder);
                            },
                            error: function (error) {
                                console.error(error.responseText);
                            }
                        });
                    },
                    createFolder: function () {
                        $('#createFolderModal-errors').empty();
                        $('#createFolderModal').modal('show');
                        $('.modal-body input').val('').focus();
                    },
                    deleteMedia: function () {
                        var media = this.selectedMedia;
                        var self = this;

                        if (!media) {
                            return;
                        }

                        if (!confirm($('#deleteMediaMessage').val())) {
                            return;
                        }

                        $.ajax({
                            url: $('#deleteMediaUrl').val() + "?path=" + encodeURIComponent(self.selectedMedia.mediaPath),
                            method: 'POST',
                            data: {
                                __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                            },
                            success: function (data) {
                                var index = self.mediaItems && self.mediaItems.indexOf(media)
                                if (index > -1) {
                                    self.mediaItems.splice(index, 1)
                                    bus.$emit('mediaDeleted', media);
                                }
                                self.selectedMedia = null;
                            },
                            error: function (error) {
                                console.error(error.responseText);
                            }
                        });
                    },
                }
            });

            $('#create-folder-name').keypress(function (e) {
                var key = e.which;
                if (key == 13) {  // the enter key code
                    $('#modalFooterOk').click();
                    return false;
                }
            });

            $('#modalFooterOk').on('click', function (e) {
                var name = $('#create-folder-name').val();

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
                        $('#createFolderModal-errors').empty();
                        $('<div class="alert alert-danger" role="alert"></div>').text(error.responseText).appendTo($('#createFolderModal-errors'));
                    }
                });
            });

            if (displayMediaApplication) {
                document.getElementById('mediaApp').style.display = "";
            }

            $(document).trigger('mediaApp:ready');

        },
        error: function (error) {
            console.error(error.responseText);
        }
    });
}

// <folder> component
Vue.component('folder', {
    template: '\
        <li :class="{selected: selected}">\
            <div>\
                <a href="javascript:;" v-on:click="toggle" class="expand" v-bind:class="{opened: open, closed: !open, empty: empty}"><i class="fas fa-caret-right"></i></a>\
                <a href="javascript:;" v-on:click="select">\
                    <i class="folder fa fa-folder"></i>\
                    {{model.name}}\
                </a>\
            </div>\
            <ol v-show="open">\
                <folder v-for="folder in children"\
                        :key="folder.path"\
                        :model="folder">\
                </folder>\
            </ol>\
        </li>\
        ',
    props: {
        model: Object
    },
    data: function () {
        return {
            open: false,
            children: null, // not initialized state (for lazy-loading)
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

                self.children.push(folder);
                folder.parent = self.model;
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
            mediaApp.selectFolder(this.model);
        }
    }
});
