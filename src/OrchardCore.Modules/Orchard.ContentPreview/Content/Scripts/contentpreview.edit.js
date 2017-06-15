$(function () {
    $(document)
        .on('input', '.content-preview-text', function () {
            $(document).trigger('contentpreview:render');
        })
        .on('propertychange', '.content-preview-text', function () {
            $(document).trigger('contentpreview:render');
        })
        .on('keyup', '.content-preview-text', function (event) {
            // handle backspace
            if (event.keyCode == 46 || event.ctrlKey) {
                $(document).trigger('contentpreview:render');
            }
        })
        .on('change', '.content-preview-select', function () {
            $(document).trigger('contentpreview:render');
        });


    $(document)
        .on('input', '.trumbowyg-editor', function () {
            $(document).trigger('contentpreview:render');
        })
        .on('propertychange', '.trumbowyg-editor', function () {
            $(document).trigger('contentpreview:render');
        })
        .on('keyup', '.trumbowyg-editor', function (event) {
            // handle backspace
            if (event.keyCode == 46 || event.ctrlKey) {
                $(document).trigger('contentpreview:render');
            }
        })
});

$(function () {
    var refreshDelay = 2000;

    var iframe, delayTask, renderRequested, rendering, enabled;
    var previewButton = document.getElementById('previewButton');
    var form = $(previewButton).closest('form');
    var previewUrl = $(document.getElementById('previewUrl')).data('value');
    var contentItemType = $(document.getElementById('contentItemType')).data('value');
    var contentPreviewContainer = document.getElementById('contentPreviewContainer');
    var contentPreviewContent = document.getElementById('contentPreviewContent');
    var editBody = document.getElementsByClassName('edit-body');
    var editSidebar = document.getElementsByClassName('edit-sidebar');
    var editSidebarHandler = document.getElementsByClassName('edit-sidebar-handler');
    var previewErrors = document.getElementById('contentPreviewErrors');

    hidePreview();

    $(editBody).resizable2({
        handleSelector: ".edit-sidebar-handler",
        resizeHeight: false,
        onDragEnd: function () {
            localStorage.setItem('editBody:width', $(editBody).width());
        }
    });

    $(previewButton).on('click', function (event) {
        event.preventDefault();
        togglePreview();
    });

    $(document).on('contentpreview:render', function () {
        renderPreview();
    });

    $(document).on('contentpreview:toggle', function () {
        togglePreview();
    });

    $(document).on('contentpreview:auto', function (event, data) {
        if (data) {
            startAutoRefresh();
        }
        else {
            stopAutoRefresh();
        }
    });

    function togglePreview() {
        if (enabled) {
            hidePreview();
            stopAutoRefresh();
        }
        else {
            showPreview();
        }
    }

    function showPreview() {
        enabled = true;
        $(previewButton).text($(previewButton).data("stop"));
        renderPreview();
        $(editSidebar).css('visibility', 'visible');
        $(editSidebarHandler).css('visibility', 'visible');
        $(editBody).width(localStorage.getItem('editBody:width'));
        $(contentPreviewNavigation).show();
        $(previewErrors).hide();
    }

    function hidePreview() {
        $(previewButton).text($(previewButton).data("start"));
        $(editSidebar).css('visibility', 'hidden');
        $(editSidebarHandler).css('visibility', 'hidden');
        $(editBody).css('width', '100%');
        $(contentPreviewNavigation).hide();
        $(contentPreviewContent).empty();
        iframe = null;
        enabled = false;
    }

    function startAutoRefresh() {
        delayTask = setInterval(renderPreview, refreshDelay);
    }

    function stopAutoRefresh() {
        clearInterval(delayTask);
        delayTask = null;
    }

    function createIframe() {
        iframe = document.createElement('iframe');
        iframe.setAttribute('frameborder', '0');
        iframe.setAttribute('width', '100%');
        iframe.setAttribute('scrolling', 'yes');
        iframe.setAttribute('height', document.body.scrollHeight + 'px');
        contentPreviewContent.appendChild(iframe);
    }

    function renderPreview() {
        // Preview is not open
        if (!enabled) {
            return;
        }

        renderRequested = true;

        // Squashes all event calls into one
        if (rendering) {
            return;
        }

        // Pump renderPreview calls
        while (renderRequested) {
            renderRequested = false;
            rendering = true;
            var data = form.serialize();
            $.post(previewUrl + "?id=" + contentItemType, data)
                .done(function (data) {
                    $(previewErrors).hide();

                    if (!iframe || !iframe.contentWindow) {
                        createIframe();
                        iframe.contentWindow.document.open();
                        iframe.contentWindow.document.close();
                    }

                    iframe.contentWindow.document.body.innerHTML = '';
                    iframe.contentWindow.document.write(data);

                    if (!iframe.contentWindow.document.body || iframe.contentWindow.document.body.innerHTML == '') {
                        // the the html is invalid we need to rebuild an iframe or it fails to render
                        $(contentPreviewContent).empty();

                        createIframe();

                        iframe.contentWindow.document.open();
                        iframe.contentWindow.document.write(data);
                        iframe.contentWindow.document.close();
                    }
                    else {
                        // body rendered successfully
                    }
                })
                .fail(function (data) {
                    $(contentPreviewContent).empty();
                    $(previewErrors).empty().show();
                    if (data.responseJSON && data.responseJSON.errors) {
                        responseJSON.errors.forEach(function (error) {
                            $(previewErrors).append('<div>' + error + '</div>')
                        });
                    }
                    else {
                        $(previewErrors).append('<div>' + $('#unknownError').data('msg') + '</div>')
                    }
                })
                .always(function () {
                    rendering = false;
                });
        }
    }
});

