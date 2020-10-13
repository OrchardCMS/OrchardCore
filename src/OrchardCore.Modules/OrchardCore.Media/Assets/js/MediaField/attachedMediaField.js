function initializeAttachedMediaField(el, idOfUploadButton, uploadAction, mediaItemUrl, allowMultiple, allowMediaText, allowCenterCropping, tempUploadFolder) {

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
            allowCenterCropping: allowCenterCropping,
            backupCenter: [ null, null ]
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
                        mediaPaths.push({ path: x.mediaPath, isRemoved: x.isRemoved, isNew: x.isNew, mediaText: x.mediaText, center: x.center });
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
                        items.push({ name: ' ' + x.path, mime: '', mediaPath: '' }); // don't remove the space. Something different is needed or it wont react when the real name arrives.
                        promise = $.when(signal).done(function () {
                            $.ajax({
                                url: mediaItemUrl + "?path=" + encodeURIComponent(x.path),
                                method: 'GET',
                                success: function (data) {
                                    data.vuekey = data.name + i.toString(); // Because a unique key is required by Vue on v-for 
                                    data.mediaText = x.mediaText; // This value is not returned from the ajax call.
                                    data.center = x.center; // This value is not returned from the ajax call.
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
                                    items.splice(i, 1, { name: x.path, mime: '', mediaPath: 'not-found', mediaText: '', center: [ null, null ]  });
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

            $(selector).fileupload({
                limitConcurrentUploads: 20,
                dropZone: $('#' + editorId),
                dataType: 'json',
                url: uploadAction,
                add: function (e, data) {
                    var count = data.files.length;
                    var i;
                    for (i = 0; i < count; i++) {
                        data.files[i].uploadName =
                            self.getUniqueId() + data.files[i].name;
                    }
                    data.submit();
                },
                formData: function () {
                    var antiForgeryToken = $("input[name=__RequestVerificationToken]").val();

                    return [
                        { name: 'path', value: tempUploadFolder },
                        { name: '__RequestVerificationToken', value: antiForgeryToken }
                    ];
                },
                done: function (e, data) {
                    var newMediaItems = [];
                    var errormsg = "";
                    
                    if (data.result.files.length > 0) {
                        for (var i = 0; i < data.result.files.length; i++) {
                            data.result.files[i].isNew = true;
                            //if error is defined probably the file type is not allowed
                            if(data.result.files[i].error === undefined || data.result.files[i].error === null)
                                newMediaItems.push(data.result.files[i]);
                            else
                                errormsg += data.result.files[i].error + "\n";
                        }
                    }
                    
                    if (errormsg !== "") {
                        alert(errormsg);
                        return;
                    }

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
                    console.log('error on upload!!');
                    console.log(jqXHR);
                    console.log(textStatus);
                    console.log(errorThrown);
                }
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
                $(this.$refs.mediaTextModal).modal();
                this.backupMediaText = this.selectedMedia.mediaText;
            },
            showCenterCroppingModal: function (event) {
                $(this.$refs.centerCroppingModal).modal();
                // Cause a refresh to recalc heights.
                this.$set(this.selectedMedia.center, 0, this.selectedMedia.center[0]);
                this.$set(this.selectedMedia.center, 1, this.selectedMedia.center[1]);
                this.backupCenter = this.selectedMedia.center;
            },
            cancelMediaTextModal: function (event) {
                $(this.$refs.mediaTextModal).modal('hide');
                this.selectedMedia.mediaText = this.backupMediaText;
            },           
            cancelCenterCroppingModal: function (event) {
                $(this.$refs.centerCroppingModal).modal('hide');
                this.selectedMedia.center = this.backupCenter;
            },            
            clearCenterCrop: function (event) {
                this.$set(this.selectedMedia.center, 0, null);
                this.$set(this.selectedMedia.center, 1, null);
            },  
            onCropDrop: function(event) {
                var image = this.$refs.cropImage;

                this.$set(this.selectedMedia.center, 0, event.offsetX / image.clientWidth);
                this.$set(this.selectedMedia.center, 1, event.offsetY / image.clientHeight);
            },
            cropLeft: function () {
                if (this.$refs.cropImage && this.selectedMedia) {
                    var position = this.selectedMedia.center[0] * this.$refs.cropImage.clientWidth;
                    if (position < 17) {
                        position = 17;
                    }
                    return position + 'px';
                } else {
                    return '0';
                }
            },            
            cropTop: function () {
                if (this.$refs.cropImage && this.selectedMedia) {
                    var position = this.selectedMedia.center[1] * this.$refs.cropImage.clientHeight;
                    if (position < 15) {
                        position = 15;
                    }
                    return position + 'px';
                } else {
                    return '0';
                }
            },
            setCrop: function (event) {
                var image = this.$refs.cropImage;
                this.$set(this.selectedMedia.center, 0, event.offsetX / image.clientWidth);
                this.$set(this.selectedMedia.center, 1, event.offsetY / image.clientHeight);
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