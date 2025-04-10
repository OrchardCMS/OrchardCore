const insertAtCaret = (element, myValue) => {
    if (document.selection) {
        //For browsers like Internet Explorer
        element.focus();
        const sel = document.selection.createRange();
        sel.text = myValue;
        element.focus();
    } else if (element.selectionStart || element.selectionStart === "0") {
        //For browsers like Firefox and Webkit based
        const startPos = element.selectionStart;
        const endPos = element.selectionEnd;
        const scrollTop = element.scrollTop;
        element.value = element.value.substring(0, startPos) + myValue + element.value.substring(endPos, element.value.length);
        element.focus();
        element.selectionStart = startPos + myValue.length;
        element.selectionEnd = startPos + myValue.length;
        element.scrollTop = scrollTop;
    } else {
        element.value += myValue;
        element.focus();
    }
};

const shortcodeBtnTemplate = `
<button type="button" class="shortcode-modal-btn btn btn-sm">
    <span class="icon-shortcode"></span>
</button>
`;

// Wraps each .shortcode-modal-input class with a wrapper, and attaches detaches the shortcode app as required.
document.addEventListener('DOMContentLoaded', () => {
    const inputs = document.querySelectorAll('.shortcode-modal-input');
    inputs.forEach(input => {
        const wrapper = document.createElement('div');
        wrapper.classList.add('shortcode-modal-wrapper');
        input.parentElement.insertBefore(wrapper, input);
        wrapper.appendChild(input);

        input.parentElement.insertAdjacentHTML('beforeend', shortcodeBtnTemplate);        
    });

    const buttons = document.querySelectorAll('.shortcode-modal-btn');
    buttons.forEach(button => {
        button.addEventListener('click', event => {
            const input = event.target.closest('.shortcode-modal-wrapper').querySelector('.shortcode-modal-input');

            shortcodesApp.init(defaultValue => {
                insertAtCaret(input, defaultValue);
            });
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
                    this.$el.addEventListener('shown.bs.modal', function (e) {
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

    const wrapper = document.createElement('div');
    wrapper.classList.add('shortcode-modal-wrapper');
    codemirrorWrapper.parentElement.insertBefore(wrapper, codemirrorWrapper);
    wrapper.appendChild(codemirrorWrapper);

    codemirrorWrapper.parentElement.insertAdjacentHTML('beforeend', shortcodeBtnTemplate);
    codemirrorWrapper.parentElement.querySelector('.shortcode-modal-btn').addEventListener('click', () => {
        shortcodesApp.init(defaultValue => {
            editor.replaceSelection(defaultValue);   
        });   
    });
}
