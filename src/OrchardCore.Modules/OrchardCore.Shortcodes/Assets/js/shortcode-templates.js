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
        var editor = CodeMirror.fromTextArea(usageElement, {
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
                $(previewElement).show();
                $(previewElement).find('.shortcode-usage').html(e.doc.getValue());
            });
        }
    }

    if (nameElement && previewElement) {
        $(nameElement).on('keyup paste', function () {
            $(previewElement).show();
            $(previewElement).find('.shortcode-name').html($(this).val())
        });
    }

    if (hintElement && previewElement) {
        $(hintElement).on('keyup paste', function () {
            $(previewElement).show();
            $(previewElement).find('.shortcode-hint').html($(this).val())
        });
    }

    if (categoriesElement) {
        var vueMultiselect = Vue.component('vue-multiselect', window.VueMultiselect["default"]);
        var vm = new Vue({
            el: categoriesElement,
            components: {
                'vue-multiselect': vueMultiselect
            },
            data: function data() {
                var allCategories = JSON.parse(categoriesElement.dataset.categories || "[]");
                var selectedCategories = JSON.parse(categoriesElement.dataset.selectedCategories || "[]");
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
