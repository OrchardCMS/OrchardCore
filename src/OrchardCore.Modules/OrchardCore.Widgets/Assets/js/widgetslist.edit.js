$(function () {
    //function scoped variables
    function guid() {
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000)
                .toString(16)
                .substring(1);
        }
        return s4() + s4() + s4() + s4() + s4() + s4() + s4() + s4();
    }

    $(document).on('click', '.tool-insert-widget', function (event) {
        var $this = $(this);
        var appendWidget = $this.data("append");
        var target = $this.closest('.widget-template');
        var targetId = $this.data("target-id");
        var createEditorUrl = $this.data("buildeditorurl");
        var prefix = guid();
        $.ajax({
            url: createEditorUrl + "&prefix=" + prefix
        }).done(function (data) {
            var result = JSON.parse(data);
            if (appendWidget === "True") {
                $(document.getElementById(targetId)).append(result.Content);
            } else {
                $(result.Content).insertBefore(target);
            }
            var dom = $(result.Scripts);
            dom.filter('script').each(function () {
                $.globalEval(this.text || this.textContent || this.innerHTML || '');
            });
        });
    });

    $(document).on('click', '.widget-delete', function () {
        var $this = $(this);
        confirmDialog({
            ...$this.data(), callback: function (r) {
                if (r) {
                    $this.closest('.widget-template').remove();
                    $(document).trigger('contentpreview:render');
                }
            }
        });
    });

    $(document).on('click', '.widget-editor-btn-toggle', function () {
        $(this).closest('.widget-editor').toggleClass('collapsed');
    });

    $(document).on('click','.toggleAll', function (){
        $(this).find('.toggleAll > svg').toggleClass('fa-angle-double-up fa-angle-double-down');
        $(this).closest('.widget.widget-editor.card').toggleClass('collapsed');
    });

    $(document).on('keyup', '.widget-editor-body .form-group input.content-caption-text', function () {
        var headerTextLabel = $(this).closest('.widget-editor').find('.widget-editor-header:first .widget-editor-header-text');
        var contentTypeDisplayText = headerTextLabel.data('content-type-display-text');
        var title = $(this).val();
        var newDisplayText = title + ' ' + contentTypeDisplayText;

        headerTextLabel.text(newDisplayText);
    });

});