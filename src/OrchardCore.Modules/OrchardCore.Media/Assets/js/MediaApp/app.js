var initialized;
var mediaApp;

var bus = new Vue();

function initializeMediaApplication(displayMediaApplication, mediaApplicationUrl, pathBase) {

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
            
            var root = {
                name:  $('#t-mediaLibrary').text(),
                path: '',
                folder: '',
                isDirectory: true
            };

            mediaApp = new Vue({
                el: '#mediaApp',
                data: {
                    selectedFolder: {},
                    mediaItems: [],
                    selectedMedias: [],
                    errors: [],
                    dragDropThumbnail: new Image(),
                    smallThumbs: false,
                    gridView: false,
                    mediaFilter: '',
                    sortBy: '',
                    sortAsc: true,
                    itemsInPage: []
                },
                created: function () {
                    var self = this;

                    self.dragDropThumbnail.src = (pathBase || '') + '/OrchardCore.Media/Images/drag-thumbnail.png';

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

                    bus.$on('createFolderRequested', function (media) {
                        self.createFolder();                        
                    });

                    bus.$on('deleteFolderRequested', function (media) {
                        self.deleteFolder();
                    });

                    // common handlers for actions in both grid and table view.
                    bus.$on('sortChangeRequested', function (newSort) {
                        self.changeSort(newSort);
                    });

                    bus.$on('mediaToggleRequested', function (media) {
                        self.toggleSelectionOfMedia(media);
                    });

                    bus.$on('renameMediaRequested', function (media) {
                        self.renameMedia(media);
                    });

                    bus.$on('deleteMediaRequested', function (media) {
                        self.deleteMediaItem(media);
                    });

                    bus.$on('mediaDragStartRequested', function (media, e) {
                        self.handleDragStart(media, e);
                    });


                    // handler for pager events
                    bus.$on('pagerEvent', function (itemsInPage) {
                        self.itemsInPage = itemsInPage;
                        self.selectedMedias = [];
                    });                                                          

                    if (!localStorage.getItem('mediaApplicationPrefs')) {
                        self.selectedFolder = root;
                        return;
                    }

                    self.currentPrefs = JSON.parse(localStorage.getItem('mediaApplicationPrefs'));
                },
                computed: {
                    isHome: function () {
                        return this.selectedFolder == root;
                    },
                    parents: function () {
                        var p = [];
                        parentFolder = this.selectedFolder;
                        while (parentFolder && parentFolder.path != '') {
                            p.unshift(parentFolder);
                            parentFolder = parentFolder.parent;                            
                        }
                        return p;
                    },
                    root: function () {
                        return root;
                    },
                    filteredMediaItems: function () {
                        var self = this;

                        self.selectedMedias = [];
                        
                        var filtered = self.mediaItems.filter(function (item) {
                            return item.name.toLowerCase().indexOf(self.mediaFilter.toLowerCase()) > - 1;
                        });

                        switch (self.sortBy) {
                            case 'size':
                                filtered.sort(function (a, b) {
                                    return self.sortAsc ? a.size - b.size : b.size - a.size;
                                });
                                break;
                            case 'mime':
                                filtered.sort(function (a, b) {
                                    return self.sortAsc ? a.mime.toLowerCase().localeCompare(b.mime.toLowerCase()) : b.mime.toLowerCase().localeCompare(a.mime.toLowerCase());
                                });
                                break;
                            case 'lastModify':
                                filtered.sort(function (a, b) {
                                    return self.sortAsc ? a.lastModify - b.lastModify : b.lastModify - a.lastModify;
                                });
                                break;
                            default:
                                filtered.sort(function (a, b) {
                                    return self.sortAsc ? a.name.toLowerCase().localeCompare(b.name.toLowerCase()) : b.name.toLowerCase().localeCompare(a.name.toLowerCase());
                                });
                        }                        

                        return filtered;
                    },
                    hiddenCount: function () {
                        var result = 0;
                        result = this.mediaItems.length - this.filteredMediaItems.length;
                        return result;
                    },
                    thumbSize: function () {
                        return this.smallThumbs ? 100 : 240;
                    },
                    currentPrefs: {
                        get: function () {
                            return {
                                smallThumbs: this.smallThumbs,
                                selectedFolder: this.selectedFolder,
                                gridView: this.gridView
                            };
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
                                self.sortBy = '';
                                self.sortAsc = true;
                            },
                            error: function (error) {
                                console.log('error loading folder:' + folder.path);                                
                                self.selectRoot();                
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
                        var folder = this.selectedFolder;
                        var self = this;
                        // The root folder can't be deleted
                        if (folder == this.root.model) {
                            return;
                        }

                        confirmDialog({...$("#deleteFolder").data(), callback: function (resp) {
                            if (resp) {
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
                            }
                        }});
                    },
                    createFolder: function () {
                        $('#createFolderModal-errors').empty();
                        var modal = new bootstrap.Modal($('#createFolderModal'));
                        modal.show();
                        $('#createFolderModal .modal-body input').val('').focus();
                    },
                    renameMedia: function (media) {
                        $('#renameMediaModal-errors').empty();
                        var modal = new bootstrap.Modal($('#renameMediaModal'));
                        modal.show();
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

                        confirmDialog({...$("#deleteMedia").data(), callback: function (resp) {
                            if (resp) {
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
                                            var index = self.mediaItems && self.mediaItems.indexOf(self.selectedMedias[i]);
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
                            }
                        }});
                    },
                    deleteMediaItem: function (media) {
                        var self = this;
                        if (!media) {
                            return;
                        }

                        confirmDialog({...$("#deleteMedia").data(), callback: function (resp) {
                            if (resp) {
                                $.ajax({
                                    url: $('#deleteMediaUrl').val() + "?path=" + encodeURIComponent(media.mediaPath),
                                    method: 'POST',
                                    data: {
                                        __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                                    },
                                    success: function (data) {
                                        var index = self.mediaItems && self.mediaItems.indexOf(media)
                                        if (index > -1) {
                                            self.mediaItems.splice(index, 1);
                                            bus.$emit('mediaDeleted', media);
                                        }
                                        //self.selectedMedia = null;
                                    },
                                    error: function (error) {
                                        console.error(error.responseText);
                                    }
                                });
                            }
                        }});
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
                    },
                    changeSort: function (newSort) {
                        if (this.sortBy == newSort) {
                            this.sortAsc = !this.sortAsc;
                        } else {
                            this.sortAsc = true;
                            this.sortBy = newSort;
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
                        var modal = new bootstrap.Modal($('#createFolderModal'));
                        modal.hide();
                    },
                    error: function (error) {
                        $('#createFolderModal-errors').empty();
                        var errorMessage = JSON.parse(error.responseText).value;
                        $('<div class="alert alert-danger" role="alert"></div>').text(errorMessage).appendTo($('#createFolderModal-errors'));
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
                    var modal = new bootstrap.Modal($('#renameMediaModal'));
                    modal.hide();
                    return;
                }

                $.ajax({
                    url: $('#renameMediaUrl').val() + "?oldPath=" + encodeURIComponent(oldPath) + "&newPath=" + encodeURIComponent(newPath),
                    method: 'POST',
                    data: {
                        __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                    },
                    success: function (data) {
                        var modal = new bootstrap.Modal($('#renameMediaModal'));
                        modal.hide();
                        bus.$emit('mediaRenamed', newName, newPath, oldPath);
                    },
                    error: function (error) {
                        $('#renameMediaModal-errors').empty();
                        var errorMessage = JSON.parse(error.responseText).value;
                        $('<div class="alert alert-danger" role="alert"></div>').text(errorMessage).appendTo($('#renameMediaModal-errors'));
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
