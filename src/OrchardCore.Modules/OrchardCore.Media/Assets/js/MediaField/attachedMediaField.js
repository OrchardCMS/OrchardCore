function initializeAttachedMediaField(el, idOfUploadButton, uploadAction, mediaItemUrl, allowMultiple, allowMediaText, allowAnchors, tempUploadFolder, maxUploadChunkSize) {

    var target = $(document.getElementById($(el).data('for')));
    var initialPaths = target.data("init");

    var mediaFieldEditor = $(el);
    var idprefix = mediaFieldEditor.attr("id");
    var mediaFieldApp;

    mediaFieldApps.push(mediaFieldApp = new Vue({
        el: mediaFieldEditor.get(0),
        data: {
            mediaItems: [],
            selectedMedia: null,
            smallThumbs: false,
            idPrefix: idprefix,
            initialized: false,
            allowMediaText: allowMediaText,
            backupMediaText: '',
            allowAnchors: allowAnchors,
            backupAnchor: null,
            mediaTextmodal: null,
            anchoringModal: null
        },
        created: function () {
            var self = this;

            self.currentPrefs = JSON.parse(localStorage.getItem('mediaFieldPrefs'));
        },
        computed: {
            paths: {
                get: function () {
                    var mediaPaths = [];
                    if (!this.initialized) {
                        return JSON.stringify(initialPaths);
                    }
                    this.mediaItems.forEach(function (x) {
                        if (x.mediaPath === 'not-found') {
                            return;
                        }
                        mediaPaths.push({ path: x.mediaPath, isRemoved: x.isRemoved, isNew: x.isNew, mediaText: x.mediaText, anchor: x.anchor, attachedFileName: x.attachedFileName });
                    });
                    return JSON.stringify(mediaPaths);
                },
                set: function (values) {
                    var self = this;
                    var mediaPaths = values || [];
                    var signal = $.Deferred();
                    var items = [];
                    var length = 0;
                    mediaPaths.forEach(function (x, i) {
                        items.push({ name: ' ' + x.path, mime: '', mediaPath: '', anchor: x.anchor, attachedFileName: x.attachedFileName }); // don't remove the space. Something different is needed or it wont react when the real name arrives.
                        promise = $.when(signal).done(function () {
                            $.ajax({
                                url: mediaItemUrl + "?path=" + encodeURIComponent(x.path),
                                method: 'GET',
                                success: function (data) {
                                    data.vuekey = data.name + i.toString(); // Because a unique key is required by Vue on v-for 
                                    data.mediaText = x.mediaText; // This value is not returned from the ajax call.
                                    data.anchor = x.anchor; // This value is not returned from the ajax call.
                                    data.attachedFileName = x.attachedFileName;// This value is not returned from the ajax call.
                                    items.splice(i, 1, data);
                                    if (items.length === ++length) {
                                        items.forEach(function (x) {
                                            self.mediaItems.push(x);
                                        });
                                        self.initialized = true;
                                    }
                                },
                                error: function (error) {
                                    console.log(JSON.stringify(error));
                                    items.splice(i, 1, { name: x.path, mime: '', mediaPath: 'not-found', mediaText: '', anchor: { x: 0.5, y: 0.5 }, attachedFileName: x.attachedFileName });
                                    if (items.length === ++length) {
                                        items.forEach(function (x) {
                                            self.mediaItems.push(x);
                                        });
                                        self.initialized = true;
                                    }
                                }
                            });
                        });
                    });

                    signal.resolve();
                }
            },
            fileSize: function () {
                return Math.round(this.selectedMedia.size / 1024);
            },
            canAddMedia: function () {
                var nonRemovedMediaItems = [];
                for (var i = 0; i < this.mediaItems.length; i++) {
                    if (!this.mediaItems[i].isRemoved) {
                        nonRemovedMediaItems.push(this.mediaItems[i]);
                    }
                }

                return nonRemovedMediaItems.length === 0 || nonRemovedMediaItems.length > 0 && allowMultiple;
            },
            thumbSize: function () {
                return this.smallThumbs ? 120 : 240;
            },
            currentPrefs: {
                get: function () {
                    return {
                        smallThumbs: this.smallThumbs
                    };
                },
                set: function (newPrefs) {
                    if (!newPrefs) {
                        return;
                    }
                    this.smallThumbs = newPrefs.smallThumbs;
                }
            }
        },
        mounted: function () {
            var self = this;

            self.paths = initialPaths;

            self.$on('selectAndDeleteMediaRequested', function (media) {
                self.selectAndDeleteMedia(media);
            });

            self.$on('selectMediaRequested', function (media) {
                self.selectMedia(media);
            });

            var selector = '#' + idOfUploadButton;
            var editorId = mediaFieldEditor.attr('id');
            var chunkedFileUploadId = crypto.randomUUID();

            $(selector)
                .fileupload({
                    limitConcurrentUploads: 20,
                    dropZone: $('#' + editorId),
                    dataType: 'json',
                    url: uploadAction,
                    maxChunkSize: maxUploadChunkSize,
                    add: function (e, data) {
                        var count = data.files.length;
                        var i;
                        for (i = 0; i < count; i++) {
                            data.files[i].uploadName =
                                self.getUniqueId() + data.files[i].name;
                            data.files[i].attachedFileName = data.files[i].name;
                        }
                        data.submit();
                    },
                    formData: function () {
                        var antiForgeryToken = $("input[name=__RequestVerificationToken]").val();

                        return [
                            { name: 'path', value: tempUploadFolder },
                            { name: '__RequestVerificationToken', value: antiForgeryToken },
                            { name: '__chunkedFileUploadId', value: chunkedFileUploadId },
                        ];
                    },
                    done: function (e, data) {
                        var newMediaItems = [];
                        var errormsg = "";
                    
                        if (data.result.files.length > 0) {
                            for (var i = 0; i < data.result.files.length; i++) {
                                data.result.files[i].isNew = true;
                                //if error is defined probably the file type is not allowed
                                if (data.result.files[i].error === undefined || data.result.files[i].error === null) {
                                    data.result.files[i].attachedFileName = data.files[i].attachedFileName;
                                    newMediaItems.push(data.result.files[i]);
                                }
                                else
                                    errormsg += data.result.files[i].error + "\n";
                            }
                        }
                    
                        if (errormsg !== "") {
                            alert(errormsg);
                            return;
                        }
                        console.log(newMediaItems);
                        if (newMediaItems.length > 1 && allowMultiple === false) {
                            alert($('#onlyOneItemMessage').val());
                            mediaFieldApp.mediaItems.push(newMediaItems[0]);
                            mediaFieldApp.initialized = true;
                        } else {
                            mediaFieldApp.mediaItems = mediaFieldApp.mediaItems.concat(newMediaItems);
                            mediaFieldApp.initialized = true;
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        console.log('Error on upload.');
                        console.log(jqXHR);
                        console.log(textStatus);
                        console.log(errorThrown);
                    }
                })
                .on('fileuploadchunkbeforesend', (e, options) => {
                    let file = options.files[0];
                    // Here we replace the blob with a File object to ensure the file name and others are preserved for the backend.
                    options.blob = new File(
                        [options.blob],
                        file.name,
                        {
                            type: file.type,
                            lastModified: file.lastModified,
                        });
                });
        },
        methods: {
            selectMedia: function (media) {
                this.selectedMedia = media;
            },
            getUniqueId: function () {
                return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                    var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
                    return v.toString(16);
                });
            },
            removeSelected: function (event) {
                var removed = {};
                if (this.selectedMedia) {
                    var index = this.mediaItems && this.mediaItems.indexOf(this.selectedMedia);
                    if (index > -1) {
                        removed = this.mediaItems[index];
                        removed.isRemoved = true;
                        //this.mediaItems.splice([index], 1, removed);
                        this.mediaItems.splice(index, 1);
                    }
                }
                else {
                    // The remove button can also remove a unique media item
                    if (this.mediaItems.length === 1) {
                        removed = this.mediaItems[index];
                        removed.isRemoved = true;
                        //this.mediaItems.splice(0, 1, removed);                        
                        this.mediaItems.splice(0, 1);
                    }
                }
                this.selectedMedia = null;
            },
            showMediaTextModal: function (event) {
                this.mediaTextModal = new bootstrap.Modal(this.$refs.mediaTextModal);
                this.mediaTextModal.show();
                this.backupMediaText = this.selectedMedia.mediaText;
            },
            cancelMediaTextModal: function (event) {
                this.mediaTextModal.hide();
                this.selectedMedia.mediaText = this.backupMediaText;
            }, 
            showAnchorModal: function (event) {
                this.anchoringModal = new bootstrap.Modal(this.$refs.anchoringModal);
                this.anchoringModal.show();
                // Cause a refresh to recalc heights.
                this.selectedMedia.anchor = {
                  x: this.selectedMedia.anchor.x,
                  y: this.selectedMedia.anchor.y
                }
                this.backupAnchor = this.selectedMedia.anchor;
            },           
            cancelAnchoringModal: function (event) {
                this.anchoringModal.hide();
                this.selectedMedia.anchor = this.backupAnchor;
            },            
            resetAnchor: function (event) {
                this.selectedMedia.anchor = { x: 0.5, y: 0.5 };
            }, 
            onAnchorDrop: function(event) {
                var image = this.$refs.anchorImage;
                this.selectedMedia.anchor = {
                   x: event.offsetX / image.clientWidth,
                   y: event.offsetY / image.clientHeight
                }
            },
            anchorLeft: function () {
                if (this.$refs.anchorImage && this.$refs.modalBody && this.selectedMedia) {
                    // When image is shrunk compare against the modal body.
                    var offset = (this.$refs.modalBody.clientWidth - this.$refs.anchorImage.clientWidth) / 2;
                    var position = (this.selectedMedia.anchor.x * this.$refs.anchorImage.clientWidth) + offset;
                    if (position < 17) { // Adjust so the target doesn't show outside image.
                        position = 17;
                    } else {
                        position = position - 8; // Adjust to hit the mouse pointer.
                    }
                    return position + 'px';
                } else {
                    return '0';
                }
            },           
            anchorTop: function () {
                if (this.$refs.anchorImage && this.selectedMedia) {
                    var position = this.selectedMedia.anchor.y * this.$refs.anchorImage.clientHeight;
                    if (position < 15) { // Adjustment so the target doesn't show outside image.
                        position = 15;
                    } else {
                        position = position + 5; // Adjust to hit the mouse pointer.
                    }
                    return position + 'px';
                } else {
                    return '0';
                }
            },
            setAnchor: function (event) {
                var image = this.$refs.anchorImage;
                this.selectedMedia.anchor = {
                    x: event.offsetX / image.clientWidth,
                    y: event.offsetY / image.clientHeight
                }
            },         
            addMediaFiles: function (files) {
                if ((files.length > 1) && (allowMultiple === false)) {
                    alert($('#onlyOneItemMessage').val());
                    mediaFieldApp.mediaItems.push(files[0]);
                    mediaFieldApp.initialized = true;
                } else {
                    mediaFieldApp.mediaItems = mediaFieldApp.mediaItems.concat(files);
                    mediaFieldApp.initialized = true;
                }
            },
            selectAndDeleteMedia: function (media) {
                var self = this;
                self.selectedMedia = media;
                // setTimeout because sometimes 
                // removeSelected was called even before the media was set.
                setTimeout(function () {
                    self.removeSelected();
                }, 100);
            }
        },
        watch: {
            mediaItems: {
                deep: true,
                handler () {
                    // Trigger preview rendering
                    setTimeout(function () { $(document).trigger('contentpreview:render'); }, 100);
                }
            }, 
            currentPrefs: function (newPrefs) {
                localStorage.setItem('mediaFieldPrefs', JSON.stringify(newPrefs));
            }
        }
    }));
}
