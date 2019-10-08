function initializeAttachedMediaField(el, idOfUploadButton, uploadAction, mediaItemUrl, allowMultiple, tempUploadFolder) {
    
    var target = $(document.getElementById($(el).data('for')));
    var initialPaths = target.data("init");

    var mediaFieldEditor = $(el);
    var mediaFieldApp;

    mediaFieldApps.push(mediaFieldApp = new Vue({
        el: mediaFieldEditor.get(0),
        data: {
            mediaItems: [],
            selectedMedia: null,
            smallThumbs: false
        },
        created: function () {
            var self = this;

            self.currentPrefs = JSON.parse(localStorage.getItem('mediaFieldPrefs'));
        },
        computed: {
            paths: {
                get: function () {
                    var mediaPaths = [];
                    this.mediaItems.forEach(function (x) {
                        if (x.mediaPath === 'not-found') {
                            return;
                        }
                        mediaPaths.push({ Path: x.mediaPath, IsRemoved: x.isRemoved, IsNew: x.isNew });
                    });
                    return JSON.stringify(mediaPaths);
                },
                set: function (values) {
                    var self = this;
                    var mediaPaths = values || [];
                    var signal = $.Deferred();
                    mediaPaths.forEach(function (x, i) {
                        self.mediaItems.push({ name: ' ' + x.Path, mime: '', mediaPath: '' }); // don't remove the space. Something different is needed or it wont react when the real name arrives.

                        promise = $.when(signal).done(function () {
                            $.ajax({
                                url: mediaItemUrl + "?path=" + encodeURIComponent(x.Path),
                                method: 'GET',
                                success: function (data) {
                                    data.vuekey = data.name + i.toString(); // just because a unique key is required by Vue on v-for 
                                    self.mediaItems.splice( i, 1, data);
                                },
                                error: function (error) {
                                    console.log(JSON.stringify(error));
                                    self.mediaItems.splice(i, 1, { name: x.Path, mime: '', mediaPath: 'not-found' });
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

            $(document).bind('drop dragover', function (e) {
                e.preventDefault();
            });

            
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
                    if (data.result.files.length > 0) {
                        for (var i = 0; i < data.result.files.length; i++) {
                            data.result.files[i].isNew = true;
                            newMediaItems.push(data.result.files[i]);
                        }
                    }

                    if (newMediaItems.length > 1 && allowMultiple === false) {
                        alert($('#onlyOneItemMessage').val());
                        mediaFieldApp.mediaItems.push(newMediaItems[0]);
                    } else {
                        mediaFieldApp.mediaItems = mediaFieldApp.mediaItems.concat(newMediaItems);
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
                        this.mediaItems.splice([index], 1, removed);
                        //this.mediaItems.splice(index, 1);
                    }
                }
                else {
                    // The remove button can also remove a unique media item
                    if (this.mediaItems.length === 1) {
                        removed = this.mediaItems[index];
                        removed.isRemoved = true;
                        this.mediaItems.splice(0, 1, removed);                        
                        //this.mediaItems.splice(0, 1);
                    }
                }
                this.selectedMedia = null;
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
            mediaItems: function () {
                // Trigger preview rendering
                setTimeout(function () { $(document).trigger('contentpreview:render'); }, 100);
            },
            currentPrefs: function (newPrefs) {
                localStorage.setItem('mediaFieldPrefs', JSON.stringify(newPrefs));
            }
        }
    }));
}