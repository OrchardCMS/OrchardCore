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
                if (element.dataset.leavesOnly === 'true') {
                    selectableTagTerms = selectableTagTerms.filter(tagTerm => tagTerm.isLeaf);
                    // Self heal when leaves only value is updated.
                    allTagTerms.forEach(tagTerm => {
                        if (!selectableTagTerms.includes(tagTerm)) {
                            tagTerm.selected = false;
                        }
                    });
                }

                // Selected terms are show in selected tags field.
                var selectedTagTerms = allTagTerms.filter(tagTerm => tagTerm.selected);

                return {
                    open: element.dataset.open,
                    taxonomyContentItemId: element.dataset.taxonomyContentItemId,
                    createTagUrl: element.dataset.createTagUrl,
                    createTagErrorMessage: element.dataset.createTagErrorMessage,
                    selectedTagTerms,
                    selectableTagTerms,
                    allTagTerms
                }
            },
            computed: {
                isDisabled() {
                    return this.open === 'false' && this.selectableTagTerms.length === 0;
                },
                selectedTagTermsIds() {
                    if (!this.selectedTagTerms) {
                        return [];
                    }
                    if (Array.isArray(this.selectedTagTerms)) {
                        return this.selectedTagTerms.map(tagTerm => tagTerm.contentItemId);
                    } else {
                        return this.selectedTagTerms.contentItemId;
                    }
                }
            },
            methods: {
                async createTagTerm(newTagTerm) {
                    const self = this;
                    try {
                        const response = await fetch(self.createTagUrl, {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json'
                            },
                            body: JSON.stringify({
                                __RequestVerificationToken: document.querySelector("input[name='__RequestVerificationToken']").value,
                                taxonomyContentItemId: self.taxonomyContentItemId,
                                displayText: newTagTerm
                            })
                        });
                        const data = await response.json();
                        const tagTerm = {
                            contentItemId: data.contentItemId,
                            displayText: data.displayText,
                            selected: true
                        };
                        // Add to allTagTerms array so model binding will save tag as selected.
                        self.allTagTerms.push(tagTerm);

                        // Add to selectedTerms to display in vue-multi-select.
                        self.selectedTagTerms.push(tagTerm);
                    } catch (error) {
                        alert(self.createTagErrorMessage);
                    }
                },
                onSelect(selectedTagTerm) {
                    const tagTerm = this.allTagTerms.find(tagTerm => tagTerm.contentItemId === selectedTagTerm.contentItemId);
                    tagTerm.selected = true;
                    document.dispatchEvent(new Event('contentpreview:render'));
                },
                onRemove(removedTagTerm) {
                    const tagTerm = this.allTagTerms.find(tagTerm => tagTerm.contentItemId === removedTagTerm.contentItemId);
                    tagTerm.selected = false;
                    document.dispatchEvent(new Event('contentpreview:render'));
                }
            }
        });

        return vm;
    }
}
