function initializeTagsEditor(element) {
    if (element) {

        var elementId = element.id;
        var selectedTerms = JSON.parse(element.dataset.selectedTerms || "[]");
        var allTerms = JSON.parse(element.dataset.allTerms || "[]");
        //var tenantPath = element.dataset.tenantPath;
        //var searchApiUrl = element.dataset.searchUrl;
        var partName = element.dataset.part;
        var fieldName = element.dataset.field;
        var vueMultiselect = Vue.component('vue-multiselect', window.VueMultiselect.default);

        var vm = new Vue({
            el: '#' + elementId,
            components: { 'vue-multiselect': vueMultiselect },
            data: {
                arrayOfItems: selectedTerms,
                options: allTerms
            },
            computed: {
                isDisabled: function () {
                    return this.arrayOfItems.length > 0 && !multiple;
                }
            },
            created: function () {
                var self = this;
            },
            methods: {
                addTag(newTag) {
                    // TODO create tag term on fly to controller? Probably.
                },
                onSelect(selectedOption) {
                    var option = this.options.find(function (element) { return element.contentItemId === selectedOption.contentItemId });
                    option.selected = true;
                },
                onRemove(removedOption) {
                    var option = this.options.find(function (element) { return element.contentItemId === removedOption.contentItemId });
                    option.selected = false;
                },
                customLabel(item) {
                    return item.term.DisplayText;
                },
                //termEntriesContentItemId(option) {
                //    var indexOf = this.options.indexOf(option);
                //    return `${partName}_${fieldName}_TermEntries_${indexOf}__ContentItemdId`;
                //},
                //termEntriesSelectedId(option) {
                //    var indexOf = this.options.indexOf(option);
                //    return `${partName}_${fieldName}_TermEntries_${indexOf}__Selected`;
                //},
                termEntriesContentItemName(option) {
                    var indexOf = this.options.indexOf(option);
                    return `${partName}.${fieldName}.TermEntries[${indexOf}].ContentItemId`;
                },
                termEntriesSelectedName(option) {
                    var indexOf = this.options.indexOf(option);
                    return `${partName}.${fieldName}.TermEntries[${indexOf}].Selected`;
                }
            }
        });

        return vm;
    }
}
