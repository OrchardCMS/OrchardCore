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
    <i class="fa-solid fa-code" aria-hidden="true"></i>
</button>
`;

// Wraps an element with a .shortcode-modal-wrapper and appends the shortcode button to it.
// The click handler is attached to that exact button, never through a page-wide query, so a
// button created for another editor (e.g. a CodeMirror editor wrapped by
// initializeCodeMirrorShortcodeWrapper, which ES module editors call before DOMContentLoaded)
// never gets a second handler.
const wrapWithShortcodeButton = (element, onInsert) => {
    const wrapper = document.createElement('div');
    wrapper.classList.add('shortcode-modal-wrapper');
    element.parentElement.insertBefore(wrapper, element);
    wrapper.appendChild(element);

    wrapper.insertAdjacentHTML('beforeend', shortcodeBtnTemplate);
    wrapper.lastElementChild.addEventListener('click', () => {
        shortcodesApp.init(onInsert);
    });
};

// Wraps each .shortcode-modal-input with a wrapper and attaches the shortcode app as required.
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.shortcode-modal-input').forEach(input => {
        if (input.parentElement.classList.contains('shortcode-modal-wrapper')) {
            return;
        }

        wrapWithShortcodeButton(input, defaultValue => insertAtCaret(input, defaultValue));
    });
});

var shortcodesApp;

window.initializeShortcodesApp = function (element) {
    if (element && !shortcodesApp) {
        shortcodesApp = Vue.createApp({
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
                    // Vue 3's mount() renders the app's template INSIDE the container element
                    // rather than replacing it (unlike Vue 2, where this.$el was the container
                    // itself) - this.$el is now the modal-dialog div, not the .modal container,
                    // so bootstrap.Modal must target the actual container (`element`, closed
                    // over from initializeShortcodesApp's parameter) instead.
                    // A single Modal instance is reused: each extra instance on the same element
                    // adds its own backdrop, which stays over the page once the modal is closed.
                    // markRaw keeps Vue from wrapping the Bootstrap instance in a reactive proxy.
                    if (!this.modal) {
                        this.modal = Vue.markRaw(bootstrap.Modal.getOrCreateInstance(element));
                        element.addEventListener('shown.bs.modal', () => {
                            this.$refs.filter.focus();
                        });
                    }
                    this.modal.show();
                },
                onClose()
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
        }).mount(element);

        return shortcodesApp;
    }
};

// initializes a code mirror editor with a shortcode modal.
window.initializeCodeMirrorShortcodeWrapper = function (editor) {
    wrapWithShortcodeButton(editor.display.wrapper, defaultValue => {
        editor.replaceSelection(defaultValue);
    });
};
