var createEditorUrl = $('#buildEditorUrl').attr("value");
var widgetTemplate = function (data, prefixesName, prefix, contentTypesName, contentType) {
    return '<div class="widget-template col-12">' + data + '<input type="hidden" name="' + prefixesName + '" value="' + prefix + '" /><input type="hidden" name="' + contentTypesName + '" value="' + contentType + '" /></div>';
};
function guid() {
    function s4() {
        return Math
            .floor((1 + Math.random()) * 0x10000)
            .toString(16)
            .substring(1);
    }
    return s4() + s4() + s4() + s4() + s4() + s4() + s4() + s4();
}

$(function () {

    $(document).on('click', '.add-widget', function (event) {
        var type = $(this).data("widget-type");
        var targetId = $(this).data("target-id");
        var prefixesName = $(this).data("prefixes-name");
        var flowmetadata = $(this).data("flowmetadata");
        var prefix = guid();
        var contentTypesName = $(this).data("contenttypes-name");
        $.ajax({
            url: createEditorUrl + "/" + type + "?prefix=" + prefix + "&prefixesName=" + prefixesName + "&contentTypesName=" + contentTypesName + "&targetId=" + targetId + "&flowmetadata=" + flowmetadata
        }).done(function (data) {
            var result = JSON.parse(data);
            $(document.getElementById(targetId)).append(widgetTemplate(result.Content, prefixesName, prefix, contentTypesName, type));

            var dom = $(result.Scripts);
            dom.filter('script').each(function () {
                $.globalEval(this.text || this.textContent || this.innerHTML || '');
            });
        });
    });

    $(document).on('click', '.insert-widget', function (event) {
        var type = $(this).data("widget-type");
        var target = $(this).closest('.widget-template');
        var targetId = $(this).data("target-id");
        var flowmetadata = $(this).data("flowmetadata");
        var prefixesName = $(this).data("prefixes-name");
        var prefix = guid();
        var contentTypesName = $(this).data("contenttypes-name");
        $.ajax({
            url: createEditorUrl + "/" + type + "?prefix=" + prefix + "&prefixesName=" + prefixesName + "&contentTypesName=" + contentTypesName + "&targetId=" + targetId + "&flowmetadata=" + flowmetadata
        }).done(function (data) {
            var result = JSON.parse(data);
            $(widgetTemplate(result.Content, prefixesName, prefix, contentTypesName, type)).insertBefore(target);

            var dom = $(result.Scripts);
            dom.filter('script').each(function () {
                $.globalEval(this.text || this.textContent || this.innerHTML || '');
            });
        });
    });

    $(document).on('click', '.widget-delete', function () {
        var title = $(this).data('title');
        var message = $(this).data('message');
        confirmDialog(title, message, r => {
            if (r) {
                $(this).closest('.widget-template').remove();
                $(document).trigger('contentpreview:render');
            }
        });
    });

    $(document).on('change', '.widget-editor-footer label, .widget-editor-header label', function () {

        var $tmpl = $(this).closest('.widget-template');
        var $radio = $(this).find("input:first-child");
        if ($radio[0].id !== 'undefined' && $radio[0].id.indexOf('Size') > 0) {
            var $radiSize = $(this).find("input:first-child").val();
            var classList = $tmpl.attr('class').split(' ');
            $.each(classList, function (id, item) {
                if (item.indexOf('col-') === 0) $tmpl.removeClass(item);                
            });
            if ($radiSize === '25')
                $tmpl.addClass('col-3');
            else if ($radiSize === '33')
                $tmpl.addClass('col-4');
            else if ($radiSize === '50')
                $tmpl.addClass('col-6');
            else if ($radiSize === '66')
                $tmpl.addClass('col-8');
            else if ($radiSize === '75')
                $tmpl.addClass('col-9');
            else
                $tmpl.addClass('col-12');

            var dropdown = $(this).closest('.dropdown-menu');
            dropdown.prev('button').text($radiSize + '%');
        } else if ($radio[0].id !== 'undefined' && $radio[0].id.indexOf('Alignment') > 0) {            
            var svg = $(this).find('svg')[0].outerHTML;
            var alignDropdown = $(this).closest('.dropdown-menu');            
            var $btn = alignDropdown.prev('button');
            $btn.html(svg );
       
        }
        $(document).trigger('contentpreview:render');        
    });

    $(document).on('click', '.widget-editor-btn-toggle', function () {
        $(this).closest('.widget-editor').toggleClass('collapsed');
    });

    $(document).on('keyup', '.widget-editor-body .form-group input.content-caption-text', function () {
        var headerTextLabel = $(this).closest('.widget-editor').find('.widget-editor-header:first .widget-editor-header-text');
        var contentTypeDisplayText = headerTextLabel.data('content-type-display-text');
        var title = $(this).val();
        var newDisplayText = title + ' ' + contentTypeDisplayText;

        headerTextLabel.text(newDisplayText);
    });
});
