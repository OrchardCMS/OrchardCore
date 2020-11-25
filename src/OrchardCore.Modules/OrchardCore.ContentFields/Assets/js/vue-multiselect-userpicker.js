function debounceUserPicker(func, wait, immediate) {
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
function initVueMultiselectUserPicker(element) {
    // only run script if element exists
    if (element) {
        var elementId = element.id;
        var selectedUsers = JSON.parse(element.dataset.selectedUsers || "[]");
        var searchUrl = element.dataset.searchUrl;
        var multiple = JSON.parse(element.dataset.multiple);

        var debouncedSearch = debounceUserPicker(function (vm, query) {
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
                arrayOfUsers: selectedUsers,
                options: [],
            },
            computed: {
                selectedIds: function () {
                    return this.arrayOfUsers.map(function (x) { return x.id }).join(',');
                },
                isDisabled: function () {
                    return this.arrayOfUsers.length > 0 && !multiple;
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
            methods: {
                asyncFind: function (query) {
                    var self = this;
                    debouncedSearch(self, query);
                },
                onSelect: function (selectedOption, id) {
                    var self = this;

                    for (i = 0; i < self.arrayOfUsers.length; i++) {
                        if (self.arrayOfUsers[i].id === selectedOption.id) {
                            return;
                        }
                    }

                    self.arrayOfUsers.push(selectedOption);
                },
                remove: function (user) {
                    this.arrayOfUsers.splice(this.arrayOfUsers.indexOf(user), 1)
                }
            }
        })
        
        /*Hook for other scripts that might want to have access to the view model*/
        var event = new CustomEvent("vue-multiselect-userpicker-created", { detail: { vm: vm } });
        document.querySelector("body").dispatchEvent(event);
    }
}
