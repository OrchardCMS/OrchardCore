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


    $(document).on('keyup', '.widget-editor .form-group input[data-card-bind], .widget-editor', function (e) {

        // Do nothing if it's inline view
        if ($(e.target).parent().is(".inline-card-item")) {
            return;
        }
        var title = $(this).val();
        var widgetEditor = $(this).closest('.widget-editor');
        var data = $(this).data('card-bind');

        //find bound element
        var boundElement = widgetEditor
            .find('[data-card-bind*=' + data + ']')
            .filter(function () {
                return $(this).closest('.widget-editor').is(widgetEditor) && e.target != this;
            });

        //trigger change event,to allow each bound element to configure display differently.
        boundElement.each(function () {
            $(this).trigger('contentcard:change', [data, title]);
        });
    });

    $(document).on('contentcard:change', 'span[data-card-bind], label[data-card-bind], div[data-card-bind]', function (evt, prop, val) {
        // default - change inner html with value
        $(this).html(val);
    });

    $(document).on('click', '.btn.toggleAll', function (e) {
        e.preventDefault();
        $('.toggleAll > svg').toggleClass('fa-angle-double-up fa-angle-double-down');
        $('.widget.widget-editor.card').filter(':not(.widget-nocollapse)').toggleClass('collapsed');
    })
});
