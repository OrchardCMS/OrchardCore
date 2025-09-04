function debounce(func, wait, immediate) {
    var timeout;
    return function () {
        var context = this, args = arguments;
        var later = function () {
            timeout = null;
            if (!immediate) func.apply(context, args);
        };
        var callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func.apply(context, args);
    };
};
function initVueMultiselect(element) {
    // only run script if element exists
    if (element) {
        var elementId = element.id;
        var selectedItems = JSON.parse(element.dataset.selectedItems || "[]");
        var editUrl = element.dataset.editUrl;
        var viewUrl = element.dataset.viewUrl;
        var searchUrl = element.dataset.searchUrl;
        var multiple = JSON.parse(element.dataset.multiple);

        var debouncedSearch = debounce(function (vm, query) {
            vm.isLoading = true;
            var searchFullUrl = searchUrl;
            if (query) {
                searchFullUrl += '&query=' + query;
            }
            fetch(searchFullUrl).then(function (res) {
                res.json().then(function (json) {
                    vm.options = json;
                    vm.isLoading = false;
                })
            });
        }, 250);

        var vueMultiselect = Vue.component('vue-multiselect', window.VueMultiselect.default);

        var vm = new Vue({
            el: '#' + elementId,
            components: { 'vue-multiselect': vueMultiselect },
            data: {
                value: null,
                arrayOfItems: selectedItems,
                options: [],
            },
            computed: {
                selectedIds: function () {
                    return this.arrayOfItems.map(function (x) { return x.id }).join(',');
                },
                isDisabled: function () {
                    return this.arrayOfItems.length > 0 && !multiple;
                }
            },
            watch: {
                selectedIds: function () {
                    // We add a delay to allow for the <input> to get the actual value	
                    // before the form is submitted	
                    setTimeout(function () { $(document).trigger('contentpreview:render') }, 100);
                }
            },
            created: function () {
                var self = this;
                self.asyncFind();
            },
            mounted: function () {
                // Store a reference to the div containing the search box used to select content
                // items so we can hide/show it later (in onSelect and remove). We use the "mounted"
                // lifecycle method rather than "created" so we know the component has been attached 
                // to the DOM and we can therefore travese the DOM to find the desired div.
                this.searchBoxContainer = $(this.$el).children().last();

                // If we're loading an existing content item, we may already have a content picker
                // configured to only allow a single content item and that item has already been selected. 
                // In this case, we need to hide the search box now and not wait for onSelect or remove.
                this.searchBoxContainer.css("display", multiple || this.arrayOfItems.length === 0 ? "block" : "none");
            },
            methods: {
                asyncFind: function (query) {
                    var self = this;
                    debouncedSearch(self, query);
                },
                onSelect: function (selectedOption, id) {
                    var self = this;

                    for (i = 0; i < self.arrayOfItems.length; i++) {
                        if (self.arrayOfItems[i].id === selectedOption.id) {
                            return;
                        }
                    }

                    self.arrayOfItems.push(selectedOption);

                    // We don't want to show the search box if we are only allowing a single content 
                    // item and a content item has already been selected. We don't need that search 
                    // box again unless and until we delete the currently selected content item. 
                    // So here we set the display mode accordingly. We always show the select list 
                    // if allowing multiple content items and do not show it if we're only allowing 
                    // a single content item and we've just selected that one item.
                    this.searchBoxContainer.css("display", multiple ? "block" : "none");
                },
                url: function(item) {
                    var url = item.isEditable ? editUrl : viewUrl;
                    return url.replace('contentItemId', item.id);
                },
                remove: function (item) {
                    this.arrayOfItems.splice(this.arrayOfItems.indexOf(item), 1);

                    // After removing a selected content item, we always want to show the search box 
                    // since (1) if we are allowing multiple content types to be selected, we always 
                    // want to show it, and (2) if we are only allowing a single content type to be 
                    // selected, and we've just removed that content type, we now need to show the 
                    // search box so we are able to add a new one.
                    this.searchBoxContainer.css("display", "block");
                }
            }
        })
        
        /*Hook for other scripts that might want to have access to the view model*/
        var event = new CustomEvent("vue-multiselect-created", { detail: { vm: vm } });
        document.querySelector("body").dispatchEvent(event);
    }
}
