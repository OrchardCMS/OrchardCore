var mediaFieldApps = [];

$(function () {
    // Hide the media application on load  by moving it in the modal
    $("#mediaApp").detach().appendTo('#mediaFieldModalBody .modal-body');

    $(".mediafield-editor").each(function () {
        var target = $(document.getElementById($(this).data('for')));
        var initialPaths = target.data("init");
        var mediaItemUrl = $('#getMediaItemUrl').val();

        var mediaFieldEditor = $(this);
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
                    $("#mediaApp").detach().appendTo('#mediaFieldModalBody .modal-body');
                    var modal = $('#mediaFieldModalBody').modal();
                    $('#mediaFieldSelectButton').off('click').on('click', function (v) {
                        mediaFieldApp.mediaItems.push(mediaApp.selectedMedia);
                        
                        modal('hide');
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
                    $(document).trigger('contentpreview:render');
                }
            }
        }));
    });
});