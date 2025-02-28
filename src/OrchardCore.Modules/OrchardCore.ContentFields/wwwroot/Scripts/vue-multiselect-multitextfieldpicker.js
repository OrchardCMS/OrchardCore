function initMultiTextFieldPicker(element) {
    // only run script if element exists
    if (element) {
        var elementId = element.id;
        var selectedValues = JSON.parse(element.dataset.selectedvalues || "[]");
        var options = JSON.parse(element.dataset.options || "[]");

        var vueMultiselect = Vue.component('vue-multiselect', window.VueMultiselect.default);

        var vm = new Vue({
            el: '#' + elementId,
            components: { 'vue-multiselect': vueMultiselect },
            data: {
                value: selectedValues,
                options: options,
                valuesKey: element.dataset.valueskey
            },
            watch: {
                value: function () {
                    // We add a delay to allow for the <input> to get the actual value	
                    // before the form is submitted	
                    setTimeout(function () { $(document).trigger('contentpreview:render') }, 100);
                }
            },
        })
        
        /*Hook for other scripts that might want to have access to the view model*/
        var event = new CustomEvent("multitextfield-picker-created", { detail: { vm: vm } });
        document.querySelector("body").dispatchEvent(event);
    }
}
