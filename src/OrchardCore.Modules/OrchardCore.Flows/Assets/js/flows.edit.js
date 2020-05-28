//variables used in FlowPart.Edit sortable
var widgetDragItem, lastContainer, widgetItemSourceId, widgetItemDestId;


$(function () {

    $(document).on('change', '.widget-editor-footer label, .widget-editor-header label', function () {

        var $tmpl = $(this).closest('.widget-template');
        var $radio = $(this).find("input:first-child");
        if ($radio[0].id !== 'undefined' && $radio[0].id.indexOf('Size') > 0) {
            var $radioSize = $(this).find("input:first-child").val();
            var classList = $tmpl.attr('class').split(' ');
            $.each(classList, function (id, item) {
                if (item.indexOf('col-md-') === 0) $tmpl.removeClass(item);
            });
            var colSize = Math.round($radioSize / 100 * 12);
            $tmpl.addClass('col-md-' + colSize);

            var dropdown = $(this).closest('.dropdown-menu');
            dropdown.prev('button').text($radioSize + '%');
        } else if ($radio[0].id !== 'undefined' && $radio[0].id.indexOf('Alignment') > 0) {
            var svg = $(this).find('svg')[0].outerHTML;
            var alignDropdown = $(this).closest('.dropdown-menu');
            var $btn = alignDropdown.prev('button');
            $btn.html(svg);

        }
        $(document).trigger('contentpreview:render');
    });


    $(document).on('keyup', '.widget-editor .form-group input.content-caption-text', function () {
        var title = $(this).val();
        var widgetEditor = $(this).closest('.widget-editor');
        var headerTextLabel = widgetEditor
            .find('.widget-editor-header .widget-editor-header-text, .widget-editor-header .label-display-text')
            .filter(function () {
                return $(this).closest('.widget-editor').is(widgetEditor);
            }).html(title);

    });
});
