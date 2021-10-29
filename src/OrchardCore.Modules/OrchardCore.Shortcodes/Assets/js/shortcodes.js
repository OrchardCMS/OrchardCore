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
    <span class="icon-shortcode"></span>
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

        shortcodesApp.init(function (defaultValue) {
            input.insertAtCaret(defaultValue);
        });    
    });
})

var shortcodesApp;

function initializeShortcodesApp(element) {
    if (element && !shortcodesApp) {
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
                    defaultValue: '',
                    modal: null
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
                    this.modal = new bootstrap.Modal(this.$el);
                    this.modal.show();
                    var self = this;
                    $(this.$el).on('shown.bs.modal', function (e) {
                        self.$refs.filter.focus();
                    });
                },
                onClose(defaultValue)
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
                insertShortcode(event) {
                    this.defaultValue =  event.target.dataset.defaultValue;
                    this.modal.hide();
                    this.onClose(this.defaultValue);
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
        shortcodesApp.init(function (defaultValue) {
            editor.replaceSelection(defaultValue);   
        });   
    });  
}
