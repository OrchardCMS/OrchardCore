var mdeToolbar;

function initializeMdeShortcodeWrapper(mde) {
    const toolbar = mde.gui.toolbar;

    const wrapper = document.createElement('div');
    wrapper.classList.add('shortcode-modal-wrapper');
    toolbar.parentNode.insertBefore(wrapper, toolbar);
    wrapper.appendChild(toolbar);
}

$(document).ready(function() {
    mdeToolbar = [
        {
            name: "guide",
            action: "https://www.markdownguide.org/basic-syntax/",
            className: "fab fa-markdown fa-sm",
            title: "Markdown Guide"
        },
        "|",
        {
            name: "undo",
            action: EasyMDE.undo,
            className: "fas fa-undo no-disable fa-sm",
            title: "Undo"
        },
        {
            name: "redo",
            action: EasyMDE.redo,
            className: "fas fa-redo no-disable fa-sm",
            title: "Redo"
        },
        "|",
        {
            name: "bold",
            action: EasyMDE.toggleBold,
            className: "fas fa-bold fa-sm",
            title: "Bold"
        },
        {
            name: "italic",
            action: EasyMDE.toggleItalic,
            className: "fas fa-italic fa-sm",
            title: "Italic"
        },
        {
            name: "strikethrough",
            action: EasyMDE.toggleStrikethrough,
            className: "fas fa-strikethrough fa-sm",
            title: "Strikethrough"
        },
        "|",
        {
            name: "heading",
            action: EasyMDE.toggleHeadingSmaller,
            className: "fas fa-heading fa-sm",
            title: "Heading"
        },
        "|",
        {
            name: "code",
            action: EasyMDE.toggleCodeBlock,
            className: "fas fa-code fa-sm",
            title: "Code"
        },
        {
            name: "quote",
            action: EasyMDE.toggleBlockquote,
            className: "fas fa-quote-left fa-sm",
            title: "Quote"
        },
        "|",
        {
            name: "link",
            action: EasyMDE.drawLink,
            className: "fas fa-link fa-sm",
            title: "Create Link"
        },
        "|",
        {
            name: "shortcode",
            className: "icon-shortcode",
            title: "Insert Shortcode",
            default: true,
            action: function(editor)
            {
                shortcodesApp.init(function (defaultValue) {
                    editor.codemirror.replaceSelection(defaultValue);    
                });    
            }
        },
        "|",
        {
            name: "image",
            action: function (editor) {
                const mediaAppElement = document.getElementById("mediaApp");
                mediaAppElement.parentNode.removeChild(mediaAppElement);
                document.getElementById("mediaModalMarkdown").querySelector(".modal-body").appendChild(mediaAppElement);
                mediaApp.selectedMedias = [];
                const modal = new bootstrap.Modal(document.getElementById("mediaModalMarkdown"));
                modal.show();
                document.getElementById("mediaMarkdownSelectButton").addEventListener("click", function (v) {
                    let mediaMarkdownContent = "";
                    for (let i = 0; i < mediaApp.selectedMedias.length; i++) {
                        mediaMarkdownContent += ' [image]' + mediaApp.selectedMedias[i].mediaPath + '[/image]';
                    }
                    const cm = editor.codemirror;
                    cm.replaceSelection(mediaMarkdownContent)
                    modal.hide();
                    this.removeEventListener("click", arguments.callee);
                });
            },
            className: "far fa-image fa-sm",
            title: "Insert Image",
            default: true
        },
        "|",
        {
            name: "unordered-list",
            action: EasyMDE.toggleUnorderedList,
            className: "fa fa-list-ul fa-sm",
            title: "Generic List"
        },
        {
            name: "ordered-list",
            action: EasyMDE.toggleOrderedList,
            className: "fa fa-list-ol fa-sm",
            title: "Numbered List"
        },
        {
            name: "mdtable",
            action: EasyMDE.drawTable,
            className: "fas fa-table fa-sm",
            title: "Insert Table"
        },
        "|",
        {
            name: "horizontal-rule",
            action: EasyMDE.drawHorizontalRule,
            className: "fas fa-minus fa-sm",
            title: "Insert Horizontal Line"
        },
        "|",
        {
            name: "preview",
            action: EasyMDE.togglePreview,
            className: "fas fa-eye no-disable fa-sm",
            title: "Toggle Preview"
        },
        {
            name: "side-by-side",
            action: EasyMDE.toggleSideBySide,
            className: "fas fa-columns no-disable no-mobile fa-sm",
            title: "Toggle Side by Side"
        },
        {
            name: "fullscreen",
            action: EasyMDE.toggleFullScreen,
            className: "fas fa-arrows-alt no-disable no-mobile fa-sm",
            title: "Toggle Fullscreen"
        }
    ];
});
