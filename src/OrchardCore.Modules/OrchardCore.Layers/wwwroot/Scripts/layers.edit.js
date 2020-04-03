var createEditorUrl = $('#buildEditorUrl').attr("value");
var widgetTemplate = function (data, prefixesName, prefix, contentTypesName, contentType, zoneName, zone) {
    return '<div class="widget-template">' + data + '<input type="hidden" name="' + prefixesName + '" value="' + prefix + '" /><input type="hidden" name="' + contentTypesName + '" value="' + contentType + '" /><input type="hidden" name="' + zoneName + '" value="' + zone + '" class="source-zone" /></div>';
};
function guid() {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
          .toString(16)
          .substring(1);
    }
    return s4() + s4() + s4() + s4() + s4() + s4() + s4() + s4();
}

$(function () {

    $(document).on('click', '.add-layer-widget', function (event) {
        var type = $(this).data("widget-type");
        var targetId = $(this).data("target-id");
        var prefixesName = $(this).data("prefixes-name");
        var zonesName = $(this).data("zones-name");
        var flowmetadata = $(this).data("flowmetadata");
        var zone = $(this).data("zone");
        var prefix = guid();
        var contentTypesName = $(this).data("contenttypes-name");
        $.ajax({
            url: createEditorUrl + "/" + type + "?prefix=" + prefix + "&prefixesName=" + prefixesName + "&contentTypesName=" + contentTypesName + "&zonesName=" + zonesName + "&zone=" + zone + "&targetId=" + targetId + "&flowmetadata=" + flowmetadata
        })
        .done(function (data) {
            var result = JSON.parse(data);
            $(document.getElementById(targetId)).append(widgetTemplate(result.Content, prefixesName, prefix, contentTypesName, type, zonesName, zone));

            var dom = $(result.Scripts);
            dom.filter('script').each(function () {
                $.globalEval(this.text || this.textContent || this.innerHTML || '');
            });
        });
    });

    $(document).on('click', '.insert-layer-widget', function (event) {
        var type = $(this).data("widget-type");
        var target = $(this).closest('.widget-template');
        var targetId = $(this).data("target-id");
        var prefixesName = $(this).data("prefixes-name");
        var zonesName = $(this).data("zones-name");
        var zone = $(this).data("zone");
        var prefix = guid();
        var contentTypesName = $(this).data("contenttypes-name");
        $.ajax({
            url: createEditorUrl + "/" + type + "?prefix=" + prefix + "&prefixesName=" + prefixesName + "&contentTypesName=" + contentTypesName + "&zonesName=" + zonesName + "&zone=" + zone + "&targetId=" + targetId
        })
        .done(function (data) {
            var result = JSON.parse(data);
            $(widgetTemplate(result.Content, prefixesName, prefix, contentTypesName, type, zonesName, zone)).insertBefore(target);

            var dom = $(result.Scripts);
            dom.filter('script').each(function () {
                $.globalEval(this.text || this.textContent || this.innerHTML || '');
            });
        });
    });

    $(document).on('click', '.widget-layer-delete', function () {
        $(this).closest('.widget-template').remove();
        $(document).trigger('contentpreview:render');
    });

    $(document).on('change', '.widget-editor-footer label', function () {
        $(document).trigger('contentpreview:render');
    });

    $(document).on('click', '.widget-layer-editor-btn-toggle', function () {
        $(this).closest('.widget-editor').toggleClass('collapsed');
    });

});
