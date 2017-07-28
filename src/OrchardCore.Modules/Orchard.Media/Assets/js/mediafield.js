var mediaFieldApps = [];

function initializeMediaFieldEditor(el, modalBodyElement, mediaItemUrl) {
    var target = $(document.getElementById($(el).data('for')));
    var initialPaths = target.data("init");

    var mediaFieldEditor = $(el);
    var mediaFieldApp;

    mediaFieldApps.push(mediaFieldApp = new Vue({
        el: mediaFieldEditor.get(0),
        data: {
            mediaItems: [],
            selectedMedia: null,
        },
        computed: {
            paths: {
                get: function () {
                    var mediaPaths = [];
                    this.mediaItems.forEach(function (x) { mediaPaths.push(x.mediaPath); });
                    return JSON.stringify(mediaPaths);
                },
                set: function (values) {
                    var self = this;
                    var mediaPaths = values || [];
                    mediaPaths.forEach(function (x) {
                        $.ajax({
                            url: mediaItemUrl + "?path=" + encodeURIComponent(x),
                            method: 'GET',
                            success: function (data) {
                                self.mediaItems.push(data);
                            },
                            error: function (error) {
                                console.log(JSON.stringify(error));
                            }
                        });
                    });
                }
            },
            fileSize: function () {
                return Math.round(this.selectedMedia.size / 1024);
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
                $("#mediaApp").detach().appendTo($(modalBodyElement).find('.modal-body'));
                $("#mediaApp").show();
                var modal = $(modalBodyElement).modal();
                $(modalBodyElement).find('.mediaFieldSelectButton').off('click').on('click', function (v) {
                    mediaFieldApp.mediaItems.push(mediaApp.selectedMedia);

                    modal.modal('hide');
                    return true;
                });
            },
            removeSelected: function (event) {
                if (this.selectedMedia) {
                    var index = this.mediaItems && this.mediaItems.indexOf(this.selectedMedia)
                    if (index > -1) {
                        this.mediaItems.splice(index, 1)
                    }
                }
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