var simplemdeToolbar;

$(function () {
    simplemdeToolbar = [
        {
            name: "guide",
            action: "https://simplemde.com/markdown-guide",
            className: "fab fa-markdown fa-sm",
            title: "Markdown Guide"
        },
        "|",
        {
            name: "undo",
            action: SimpleMDE.undo,
            className: "fas fa-undo no-disable fa-sm",
            title: "Undo"
        },
        {
            name: "redo",
            action: SimpleMDE.redo,
            className: "fas fa-redo no-disable fa-sm",
            title: "Redo"
        },
        "|",
        {
            name: "bold",
            action: SimpleMDE.toggleBold,
            className: "fas fa-bold fa-sm",
            title: "Bold"
        },
        {
            name: "italic",
            action: SimpleMDE.toggleItalic,
            className: "fas fa-italic fa-sm",
            title: "Italic"
        },
        {
            name: "strikethrough",
            action: SimpleMDE.toggleStrikethrough,
            className: "fas fa-strikethrough fa-sm",
            title: "Strikethrough"
        },
        "|",
        {
            name: "heading",
            action: SimpleMDE.toggleHeadingSmaller,
            className: "fas fa-heading fa-sm",
            title: "Heading"
        },
        "|",
        {
            name: "code",
            action: SimpleMDE.toggleCodeBlock,
            className: "fas fa-code fa-sm",
            title: "Code"
        },
        {
            name: "quote",
            action: SimpleMDE.toggleBlockquote,
            className: "fas fa-quote-left fa-sm",
            title: "Quote"
        },
        "|",
        {
            name: "link",
            action: SimpleMDE.drawLink,
            className: "fas fa-link fa-sm",
            title: "Create Link"
        },
        "|",
        {
            name: "image",
            action: function (editor) {
                // editor = e;
                $("#mediaApp").detach().appendTo('#mediaModalMarkdown .modal-body');
                $("#mediaApp").show();
                mediaApp.selectedMedias = [];
                var modal = $('#mediaModalMarkdown').modal();
                $('#mediaMarkdownSelectButton').on('click', function (v) {
                    var mediaMarkdownContent = "";
                    for (i = 0; i < mediaApp.selectedMedias.length; i++) {
                        mediaMarkdownContent += ' [image]' + mediaApp.selectedMedias[i].mediaPath + '[/image]';
                    }
                    var cm = editor.codemirror;
                    cm.replaceSelection(mediaMarkdownContent)
                    $('#mediaModalMarkdown').modal('hide');
                    $(this).off('click');
                });
            },
            className: "far fa-image fa-sm",
            title: "Insert Image",
            default: true
        },
        "|",
        {
            name: "unordered-list",
            action: SimpleMDE.toggleUnorderedList,
            className: "fa fa-list-ul fa-sm",
            title: "Generic List"
        },
        {
            name: "ordered-list",
            action: SimpleMDE.toggleOrderedList,
            className: "fa fa-list-ol fa-sm",
            title: "Numbered List"
        },
        {
            name: "table",
            action: SimpleMDE.drawTable,
            className: "fas fa-table fa-sm",
            title: "Insert Table"
        },
        "|",
        {
            name: "horizontal-rule",
            action: SimpleMDE.drawHorizontalRule,
            className: "fas fa-minus fa-sm",
            title: "Insert Horizontal Line"
        },
        "|",
        {
            name: "preview",
            action: SimpleMDE.togglePreview,
            className: "fas fa-eye no-disable fa-sm",
            title: "Toggle Preview"
        },
        {
            name: "side-by-side",
            action: SimpleMDE.toggleSideBySide,
            className: "fas fa-columns no-disable no-mobile fa-sm",
            title: "Toggle Side by Side"
        },
        {
            name: "fullscreen",
            action: SimpleMDE.toggleFullScreen,
            className: "fas fa-arrows-alt no-disable no-mobile fa-sm",
            title: "Toggle Fullscreen"
        }
    ];
});