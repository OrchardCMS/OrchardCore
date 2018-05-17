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
                    selectedMedias: [],
                    errors: [],
                    dragDropThumbnail: new Image(),
                    smallThumbs: false,
                    gridView: false,
                    mediaFilter: ''
                },
                created: function () {
                    var self = this;

                    self.dragDropThumbnail.src = '../Images/drag-thumbnail.png';

                    bus.$on('folderSelected', function (folder) {
                        self.selectedFolder = folder;
                    });

                    bus.$on('folderDeleted', function () {
                        self.selectRoot();
                    });

                    bus.$on('folderAdded', function (folder) {
                        self.selectedFolder = folder;
                        folder.selected = true;
                    });

                    bus.$on('beforeFolderAdded', function (folder) {
                        self.loadFolder(folder);
                    });

                    bus.$on('mediaListMoved', function (errorInfo) {                        
                        self.loadFolder(self.selectedFolder);
                        if (errorInfo) {
                            self.errors.push(errorInfo);                          
                        }
                    });

                    bus.$on('mediaRenamed', function (newName, newPath, oldPath) {
                        var media = self.mediaItems.filter(function (item) {                            
                            return item.mediaPath === oldPath;
                        })[0];
                        
                        media.mediaPath = newPath;
                        media.name = newName;
                    });

                    self.currentPrefs = JSON.parse(localStorage.getItem('mediaApplicationPrefs'));
                },
                computed: {
                    isHome: function () {
                        return this.selectedFolder == root;
                    },
                    parents: function () {
                        var p = [];
                        parent = this.selectedFolder;
                        while (parent && parent.path != '') {
                            p.unshift(parent);
                            parent = parent.parent;
                        }
                        return p;
                    },
                    root: function () {
                        return root;
                    },
                    filteredMediaItems: function () {
                        var self = this;

                        self.selectedMedias = [];
                        
                        return  self.mediaItems.filter(function (item) {
                            return item.name.toLowerCase().indexOf(self.mediaFilter.toLowerCase()) > - 1;
                        });
                    },
                    hiddenCount: function () {
                        var result = 0;
                        result = this.mediaItems.length - this.filteredMediaItems.length;
                        return result;
                    },
                    thumbSize: function () {
                        return this.smallThumbs ? 120 : 240 ;
                    },
                    currentPrefs: {
                        get: function () {
                            return {
                                smallThumbs: this.smallThumbs,
                                selectedFolder: this.selectedFolder,
                                gridView: this.gridView
                            }
                        },
                        set: function (newPrefs) {
                            if (!newPrefs) {
                                return;
                            }

                            this.smallThumbs = newPrefs.smallThumbs;
                            this.selectedFolder = newPrefs.selectedFolder;
                            this.gridView = newPrefs.gridView;
                        }
                    }
                },
                watch: {
                    currentPrefs: function (newPrefs) {
                        localStorage.setItem('mediaApplicationPrefs', JSON.stringify(newPrefs));
                    },
                    selectedFolder: function (newFolder) {
                        this.mediaFilter = '';
                        this.selectedFolder = newFolder;
                        this.loadFolder(newFolder);

                    }
               
                },
                mounted: function () {
                    this.$refs.rootFolder.toggle();
                },
                methods: {
                    uploadUrl: function () {
                        return this.selectedFolder ? $('#uploadFiles').val() + "?path=" + encodeURIComponent(this.selectedFolder.path) : null;
                    },
                    selectRoot: function () {
                        this.selectedFolder = this.root;
                    },
                    loadFolder: function (folder) {
                        this.errors = [];
                        this.selectedMedias = [];
                        var self = this;
                        $.ajax({
                            url: $('#getMediaItemsUrl').val() + "?path=" + encodeURIComponent(folder.path),
                            method: 'GET',
                            success: function (data) {
                                data.forEach(function (item) {
                                    item.open = false;
                                });
                                self.mediaItems = data;
                                self.selectedMedias = [];
                            },
                            error: function (error) {
                                console.error(error.responseText);
                            }
                        });
                    },
                    selectAll: function () {
                        this.selectedMedias = [];
                        for (var i = 0; i < this.filteredMediaItems.length; i++) {
                            this.selectedMedias.push(this.filteredMediaItems[i]);
                        }
                    },
                    unSelectAll: function () {
                        this.selectedMedias = [];
                    },
                    invertSelection: function () {
                        var temp = [];
                        for (var i = 0; i < this.filteredMediaItems.length; i++) {
                            if (this.isMediaSelected(this.filteredMediaItems[i]) == false) {
                                temp.push(this.filteredMediaItems[i]);
                            }
                        }
                        this.selectedMedias = temp;
                    },
                    toggleSelectionOfMedia: function (media) {
                        if (this.isMediaSelected(media) == true) {
                            this.selectedMedias.splice(this.selectedMedias.indexOf(media), 1);
                        } else {
                            this.selectedMedias.push(media);
                        }
                    },
                    isMediaSelected: function (media) {
                        var result = this.selectedMedias.some(function (element, index, array) {
                            return element.url.toLowerCase() === media.url.toLowerCase();
                        });
                        return result;
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
                        $('#createFolderModal .modal-body input').val('').focus();
                    },
                    renameMedia: function (media) {
                        $('#renameMediaModal-errors').empty();
                        $('#renameMediaModal').modal('show');                       
                        $('#old-item-name').val(media.name);
                        $('#renameMediaModal .modal-body input').val(media.name).focus();
                    },
                    selectAndDeleteMedia: function (media) {
                        this.deleteMedia();
                    },
                    deleteMediaList: function () {
                        var mediaList = this.selectedMedias;
                        var self = this;

                        if (mediaList.length < 1) {
                            return;
                        }

                        if (!confirm($('#deleteMediaMessage').val())) {
                            return;
                        }

                        var paths = [];
                        for (var i = 0; i < mediaList.length; i++) {
                            paths.push(mediaList[i].mediaPath);
                        }

                        $.ajax({
                            url: $('#deleteMediaListUrl').val(),
                            method: 'POST',
                            data: {
                                __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
                                paths: paths
                            },
                            success: function (data) {
                                for (var i = 0; i < self.selectedMedias.length; i++) {
                                    var index = self.mediaItems && self.mediaItems.indexOf(self.selectedMedias[i])
                                    if (index > -1) {
                                        self.mediaItems.splice(index, 1);
                                        bus.$emit('mediaDeleted', self.selectedMedias[i]);
                                    }
                                }
                                self.selectedMedias = [];
                            },
                            error: function (error) {
                                console.error(error.responseText);
                            }
                        });
                    },
                    deleteMediaItem: function (media) {

                        var self = this;
                        console.log('media is : ');
                        console.log(media);

                        if (!media) {
                            return;
                        }

                        if (!confirm($('#deleteMediaMessage').val())) {
                            return;
                        }

                        $.ajax({
                            url: $('#deleteMediaUrl').val() + "?path=" + encodeURIComponent(media.mediaPath),
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
                                //self.selectedMedia = null;
                            },
                            error: function (error) {
                                console.error(error.responseText);
                            }
                        });
                    },
                    handleDragStart: function (media, e) {
                        // first part of move media to folder:
                        // prepare the data that will be handled by the folder component on drop event
                        var mediaNames = [];
                        this.selectedMedias.forEach(function (item) {
                            mediaNames.push(item.name);
                        });

                        // in case the user drags an unselected item, we select it first
                        if (this.isMediaSelected(media) == false) {
                            mediaNames.push(media.name);
                            this.selectedMedias.push(media);
                        }

                        e.dataTransfer.setData('mediaNames', JSON.stringify(mediaNames));
                        e.dataTransfer.setData('sourceFolder', this.selectedFolder.path);
                        e.dataTransfer.setDragImage(this.dragDropThumbnail, 10, 10);
                        e.dataTransfer.effectAllowed = 'move';                        
                    },
                    handleScrollWhileDrag: function (e) {
                        if (e.clientY < 150) {                            
                            window.scrollBy(0, -10);
                        }

                        if (e.clientY > window.innerHeight - 100) {                            
                            window.scrollBy(0, 10);
                        }
                    }
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

                if (name === "") {
                    return;
                }

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

            $('#renameMediaModalFooterOk').on('click', function (e) {
                var newName = $('#new-item-name').val();
                var oldName = $('#old-item-name').val();

                if (newName === "") {
                    return;
                }

                var currentFolder = mediaApp.selectedFolder.path + "/" ;
                if (currentFolder === "/") {
                    currentFolder = "";
                }

                var newPath = currentFolder + newName;
                var oldPath = currentFolder + oldName;

                if (newPath.toLowerCase() === oldPath.toLowerCase()) {
                    $('#renameMediaModal').modal('hide');
                    return;
                }

                $.ajax({
                    url: $('#renameMediaUrl').val() + "?oldPath=" + encodeURIComponent(oldPath) + "&newPath=" + encodeURIComponent(newPath),
                    method: 'POST',
                    data: {
                        __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                    },
                    success: function (data) {
                        $('#renameMediaModal').modal('hide');
                        bus.$emit('mediaRenamed', newName, newPath, oldPath);
                    },
                    error: function (error) {
                        $('#renameMediaModal-errors').empty();
                        $('<div class="alert alert-danger" role="alert"></div>').text(error.responseText).appendTo($('#renameMediaModal-errors'));
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