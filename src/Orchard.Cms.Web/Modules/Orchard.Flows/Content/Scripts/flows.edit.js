var flowEditorIndexes = {}; // contains the current indexes for each flow part
var createEditorUrl = $('#buildEditorUrl').attr("value");
var widgetTemplate = function (data, indexesName, index, contentTypesName, contentType) {
    return '<div class="widget-template">' + data + '<input type="hidden" name="' + indexesName + '" value="' + index + '" /><input type="hidden" name="' + contentTypesName + '" value="' + contentType + '" /></div>';
};

$(function () {

    $(document).on('click', '.add-widget', function (event) {
        var type = $(this).data("widget-type");
        var targetId = $(this).data("target-id");
        var indexesName = $(this).data("indexes-name");
        var contentTypesName = $(this).data("contenttypes-name");
        $.ajax({
            url: createEditorUrl + "/" + type + "?prefix=" + "FlowPart." + flowEditorIndexes[targetId] + "&indexesName=" + indexesName + "&contentTypesName=" + contentTypesName + "&targetId=" + targetId
        })
        .done(function (data) {
            var result = JSON.parse(data);
            $(document.getElementById(targetId)).append(widgetTemplate(result.Content, indexesName, flowEditorIndexes[targetId], contentTypesName, type));

            var dom = $(result.Scripts);
            dom.filter('script').each(function () {
                $.globalEval(this.text || this.textContent || this.innerHTML || '');
            });

            flowEditorIndexes[targetId]++;
        });
    });

    $(document).on('click', '.insert-widget', function (event) {
        var type = $(this).data("widget-type");
        var target = $(this).closest('.widget-template');
        var targetId = $(this).data("target-id");
        var indexesName = $(this).data("indexes-name");
        var contentTypesName = $(this).data("contenttypes-name");
        $.ajax({
            url: createEditorUrl + "/" + type + "?prefix=FlowPart." + flowEditorIndexes[targetId] + "&indexesName=" + indexesName + "&contentTypesName=" + contentTypesName + "&targetId=" + targetId
        })
        .done(function (data) {
            var result = JSON.parse(data);
            $(widgetTemplate(result.Content, indexesName, flowEditorIndexes[targetId], contentTypesName, type)).insertBefore(target);

            var dom = $(result.Scripts);
            dom.filter('script').each(function () {
                $.globalEval(this.text || this.textContent || this.innerHTML || '');
            });

            flowEditorIndexes[targetId]++;
        });
    });

    $(document).on('click', '.widget-delete', function () {
        $(this).closest('.widget-template').remove();
        $(document).trigger('contentpreview:render');
    });

    $(document).on('change', '.widget-editor-footer label', function () {
        $(document).trigger('contentpreview:render');
    });

});
