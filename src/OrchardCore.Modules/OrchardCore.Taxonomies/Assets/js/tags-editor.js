function initializeTagsEditor(element) {
    if (element) {

        var elementId = element.id;
        var vueMultiselect = Vue.component('vue-multiselect', window.VueMultiselect.default);

        var vm = new Vue({
            el: '#' + elementId,
            components: { 'vue-multiselect': vueMultiselect },
            data: function () {
                // All terms generate a model binding.
                var allTagTerms = JSON.parse(element.dataset.allTagTerms || "[]");

                // Selectable terms are shown in options list.
                var selectableTagTerms = allTagTerms;

                // Leaves only filters selectableTerms.
                if (element.dataset.leavesOnly == 'true') {
                    selectableTagTerms = selectableTagTerms.filter(function (tagTerm) { return tagTerm.isLeaf });
                    // Self heal when leaves only value is updated.
                    allTagTerms.forEach(function (tagTerm) {
                        if (!selectableTagTerms.includes(tagTerm)) {
                            tagTerm.selected = false;
                        }
                    });
                }

                // Selected terms are show in selected tags field.
                selectedTagTerms = allTagTerms.filter(function (tagTerm) { return tagTerm.selected });

                return {
                    open: element.dataset.open,
                    taxonomyContentItemId: element.dataset.taxonomyContentItemId,
                    createTagUrl: element.dataset.createTagUrl,
                    createTagErrorMessage: element.dataset.createTagErrorMessage,
                    selectedTagTerms: selectedTagTerms,
                    selectableTagTerms: selectableTagTerms,
                    allTagTerms: allTagTerms
                }
            },
            computed: {
                isDisabled: function () {
                    if (this.open == 'false' && this.selectableTagTerms.length === 0) {
                        return true;
                    }
                    return false;
                },
                selectedTagTermsIds: function() {
                    if (!this.selectedTagTerms) {
                        return [];
                    }
                    if (Array.isArray(this.selectedTagTerms)) {
                        return this.selectedTagTerms.map(function (tagTerm) { return tagTerm.contentItemId });
                    } else {
                        return this.selectedTagTerms.contentItemId;
                    }
                }
            },
            methods: {
                createTagTerm(newTagTerm) {
                    var self = this;
                    $.ajax({
                        url: self.createTagUrl,
                        method: 'POST',
                        data: {
                            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
                            taxonomyContentItemId: self.taxonomyContentItemId,
                            displayText: newTagTerm
                        },
                        success: function (data) {
                            var tagTerm = {
                                contentItemId: data.contentItemId,
                                displayText: data.displayText,
                                selected: true
                            }
                            // Add to allTagTerms array so model binding will save tag as selected.
                            self.allTagTerms.push(tagTerm);

                            // Add to selectedTerms to display in vue-multi-select.
                            self.selectedTagTerms.push(tagTerm);

                        },
                        error: function () {
                            alert(self.createTagErrorMessage);
                        }
                    });
                },
                onSelect(selectedTagTerm) {
                    var tagTerm = this.allTagTerms.find(function (tagTerm) { return tagTerm.contentItemId === selectedTagTerm.contentItemId });
                    tagTerm.selected = true;
                    $(document).trigger('contentpreview:render');
                },
                onRemove(removedTagTerm) {
                    var tagTerm = this.allTagTerms.find(function (tagTerm) { return tagTerm.contentItemId === removedTagTerm.contentItemId });
                    tagTerm.selected = false;
                    $(document).trigger('contentpreview:render');
                }
            }
        });

        return vm;
    }
}
