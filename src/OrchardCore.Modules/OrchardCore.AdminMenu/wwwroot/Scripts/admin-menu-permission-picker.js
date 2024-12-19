function initAdminMenuPermissionsPicker(element) {
    // only run script if element exists
    if (element) {
        var elementId = element.id;
        var selectedItems = JSON.parse(element.dataset.selectedItems || "[]");
        var allItems = JSON.parse(element.dataset.allItems || "[]");

        var vueMultiselect = Vue.component('vue-multiselect', window.VueMultiselect.default);

        var vm = new Vue({
            el: '#' + elementId,
            components: { 'vue-multiselect': vueMultiselect },
            data: {
                value: null,
                arrayOfItems: selectedItems,
                options: allItems,
            },
            computed: {
                selectedNames: function () {
                    return this.arrayOfItems.map(function (x) { return x.name }).join(',');
                }
            },
            methods: {
                onSelect: function (selectedOption, name) {
                    var self = this;

                    for (i = 0; i < self.arrayOfItems.length; i++) {
                        if (self.arrayOfItems[i].name === selectedOption.name) {
                            return;
                        }
                    }

                    self.arrayOfItems.push(selectedOption);
                },
                remove: function (item) {
                    this.arrayOfItems.splice(this.arrayOfItems.indexOf(item), 1)
                }
            }
        })
        
        /*Hook for other scripts that might want to have access to the view model*/
        var event = new CustomEvent("admin-menu-permission-picker-created", { detail: { vm: vm } });
        document.querySelector("body").dispatchEvent(event);
    }
}
