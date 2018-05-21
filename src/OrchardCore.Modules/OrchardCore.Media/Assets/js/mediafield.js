var mediaFieldApps = [];

function initializeMediaFieldEditor(el, modalBodyElement, mediaItemUrl, allowMultiple) {
    var target = $(document.getElementById($(el).data('for')));
    var initialPaths = target.data("init");

    var mediaFieldEditor = $(el);
    var mediaFieldApp;

    mediaFieldApps.push(mediaFieldApp = new Vue({
        el: mediaFieldEditor.get(0),
        data: {
            mediaItems: [],
            selectedMedia: null
        },
        computed: {
            paths: {
                get: function () {
                    var mediaPaths = [];
                    this.mediaItems.forEach(function (x) {
                        if (x.mediaPath === 'not-found') {
                            return;
                        }
                        mediaPaths.push(x.mediaPath);
                    });
                    return JSON.stringify(mediaPaths);
                },
                set: function (values) {
                    var self = this;
                    var mediaPaths = values || [];
                    var signal = $.Deferred();
                    mediaPaths.forEach(function (x, i) {
                        self.mediaItems.push({ name: ' ' + x, mime: '', mediaPath: '' }); // don't remove the space. Something different is needed or it wont react when the real name arrives.

                        promise = $.when(signal).done(function () {
                            $.ajax({
                                url: mediaItemUrl + "?path=" + encodeURIComponent(x),
                                method: 'GET',
                                success: function (data) {
                                    self.mediaItems.splice( i, 1, data);
                                },
                                error: function (error) {
                                    console.log(JSON.stringify(error));
                                    self.mediaItems.splice(i, 1, { name: x, mime: '', mediaPath: 'not-found' });
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
                return this.mediaItems.length === 0 || this.mediaItems.length > 0 && allowMultiple;
            },
            canRemoveMedia: function () {
                return this.selectedMedia || this.mediaItems.length === 1;
            }

        },
        mounted: function () {
            this.paths = initialPaths;
        },
        methods: {
            selectMedia: function (media) {
                this.selectedMedia = media;
            },
            showModal: function (event) {
                if (this.canAddMedia) {
                    $("#mediaApp").detach().appendTo($(modalBodyElement).find('.modal-body'));
                    $("#mediaApp").show();
                    var modal = $(modalBodyElement).modal();
                    $(modalBodyElement).find('.mediaFieldSelectButton').off('click').on('click', function (v) {
                        if (mediaApp.selectedMedia != null) {
                            mediaFieldApp.mediaItems.push(mediaApp.selectedMedia);
                        }

                        modal.modal('hide');
                        return true;
                    });
                }
            },
            removeSelected: function (event) {
                if (this.selectedMedia) {
                    var index = this.mediaItems && this.mediaItems.indexOf(this.selectedMedia);
                    if (index > -1) {
                        this.mediaItems.splice(index, 1);
                    }
                }
                else {
                    // The remove button can also remove a unique media item
                    if (this.mediaItems.length === 1) {
                        this.mediaItems.splice(0, 1);
                    }
                }
                this.selectedMedia = null;
            }
        },
        watch: {
            mediaItems: function () {
                // Trigger preview rendering
                setTimeout(function () { $(document).trigger('contentpreview:render'); }, 100);
            }
        }
    }));
}