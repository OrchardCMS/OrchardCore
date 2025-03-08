function initializeShortcodesTemplateEditor(categoriesElement, contentElement, usageElement, previewElement, nameElement, hintElement) {
    if (contentElement) {
        CodeMirror.fromTextArea(contentElement, {
            autoCloseTags: true,
            autoRefresh: true,
            lineNumbers: true,
            lineWrapping: true,
            matchBrackets: true,
            styleActiveLine: true,
            mode: { name: "liquid" }
        });
    }

    if (usageElement) {
        const editor = CodeMirror.fromTextArea(usageElement, {
            autoCloseTags: true,
            autoRefresh: true,
            lineNumbers: true,
            lineWrapping: true,
            matchBrackets: true,
            styleActiveLine: true,
            mode: { name: "htmlmixed" }
        });
        if (previewElement) {
            editor.on('change', function (e) {
                previewElement.style.display = 'block';
                previewElement.querySelector('.shortcode-usage').innerHTML = e.doc.getValue();
            });
        }
    }

    if (nameElement && previewElement) {
        ['keyup', 'paste'].forEach(event => {
            nameElement.addEventListener(event, function () {
                previewElement.style.display = 'block';
                previewElement.querySelector('.shortcode-name').innerHTML = this.value;
            });
        });
    }

    if (hintElement && previewElement) {
        ['keyup', 'paste'].forEach(event => {
            hintElement.addEventListener(event, function () {
                previewElement.style.display = 'block';
                previewElement.querySelector('.shortcode-hint').innerHTML = this.value;
            });
        });
    }

    if (categoriesElement) {
        const vueMultiselect = Vue.component('vue-multiselect', window.VueMultiselect["default"]);
        const vm = new Vue({
            el: categoriesElement,
            components: {
                'vue-multiselect': vueMultiselect
            },
            data: function data() {
                const allCategories = JSON.parse(categoriesElement.dataset.categories || "[]");
                const selectedCategories = JSON.parse(categoriesElement.dataset.selectedCategories || "[]");
                return {
                    value: selectedCategories,
                    options: allCategories
                };
            },
            methods: {
                getSelectedCategories() {
                    return JSON.stringify(this.value);
                },
                addCategory(category) {
                    this.options.push(category);
                    this.value.push(category);
                }
            }
        });
        return vm;
    }
}
