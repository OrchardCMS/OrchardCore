/// jQuery helper to append text to a textarea or input.
jQuery.fn.extend({
    insertAtCaret: function (myValue) {
        return this.each(function (i) {
            if (document.selection) {
                //For browsers like Internet Explorer
                this.focus();
                sel = document.selection.createRange();
                sel.text = myValue;
                this.focus();
            } else if (this.selectionStart || this.selectionStart === "0") {
                //For browsers like Firefox and Webkit based
                var startPos = this.selectionStart;
                var endPos = this.selectionEnd;
                var scrollTop = this.scrollTop;
                this.value = this.value.substring(0, startPos) + myValue + this.value.substring(endPos, this.value.length);
                this.focus();
                this.selectionStart = startPos + myValue.length;
                this.selectionEnd = startPos + myValue.length;
                this.scrollTop = scrollTop;
            } else {
                this.value += myValue;
                this.focus();
            }
        });
    }
});

const shortcodeWrapperTemplate = `
<div class="shortcode-modal-wrapper"></div>
`;

const shortcodeBtnTemplate = `
<button type="button" class="shortcode-modal-btn btn btn-sm">
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
    <path d="M16 4.2v1.5h2.5v12.5H16v1.5h4V4.2h-4zM4.2 19.8h4v-1.5H5.8V5.8h2.5V4.2h-4l-.1 15.6zm5.1-3.1l1.4.6 4-10-1.4-.6-4 10z"></path>
</svg>
</button>
`;

// Wraps each .shortcode-modal-input class with a wrapper, and attaches detaches the shortcode app as required.
$(function () {
    $('.shortcode-modal-input').each(function () {
        $(this).wrap(shortcodeWrapperTemplate);
        $(this).parent().append(shortcodeBtnTemplate);
    });

    $('.shortcode-modal-btn').on('click', function() {
        var input = $(this).siblings('.shortcode-modal-input');

        shortcodesApp.init(function (returnShortcode) {
            input.insertAtCaret(returnShortcode);
        });    
    });
})

var shortcodesApp;

function initializeShortcodesApp(element) {
    if (element) {
        var elementId = element.id;

        shortcodesApp = new Vue({
            el: '#' + elementId,
            data : function () {
                
                var shortcodes = JSON.parse(element.dataset.shortcodes || "[]");
                var categories = JSON.parse(element.dataset.categories || "[]");

                return {
                    filter: '',
                    allShortcodes: shortcodes,
                    filteredShortcodes: shortcodes,
                    categories: categories,
                    returnShortcode: ''
                }
            },
            watch:
            {
                filter(filter) {
                    if (filter) {
                        var lower = filter.toLowerCase();
                        this.filteredShortcodes = this.allShortcodes
                            .filter(s => s.name.startsWith(lower));
                    } else {
                        this.filteredShortcodes = this.allShortcodes;
                    }
                }
            },            
            methods: {
                init(onClose)
                {
                    if (onClose) {
                        this.onClose = onClose;
                    }
                    this.selectedValue = '';
                    $(this.$el).modal('show');
                    var self = this;
                    $(this.$el).on('shown.bs.modal', function (e) {
                        self.$refs.filter.focus();
                    });
                },
                onClose(returnShortcode)
                {
                    return;
                },
                setCategory(category)
                {
                    if (category) {
                        this.filteredShortcodes = this.allShortcodes
                            .filter(s => s.categories.some(c => c.toLowerCase() === category.toLowerCase()));
                    } else {
                        this.filteredShortcodes = this.allShortcodes;
                    }
                    this.filter = '';
                },            
                isVisible(name) {
                    return this.filteredShortcodes.some(s => s.name === name);
                },
                insertShortcode(returnShortcode) {
                    this.returnShortcode = returnShortcode;
                    $(this.$el).modal('hide');
                    this.onClose(this.returnShortcode);
                }
            }
        });

        return shortcodesApp;
    }
}


// initializes a code mirror editor with a shortcode modal.
function initializeCodeMirrorShortcodeWrapper(editor) {
    const codemirrorWrapper = editor.display.wrapper;

    $(codemirrorWrapper).wrap(shortcodeWrapperTemplate);
    $(codemirrorWrapper).parent().append(shortcodeBtnTemplate);
    $(codemirrorWrapper).siblings('.shortcode-modal-btn').on('click', function () {
        shortcodesApp.init(function (returnShortcode) {
            editor.replaceSelection(returnShortcode);   
        });   
    });  
}






