// a wrapper around https://github.com/farbelous/fontawesome-iconpicker
// It makes easier to use a single picker instance with several input fields.
// How to use it: Call show() from outside , passing it the id's of the input fields you want the icon classes applied to.

var iconPickerVue = new Vue({
    el: '#iconPickerVue',
    data: {
        targetInputField: '',
        targetIconTag: '',
        iconPickerModal: null,
    },
    mounted: function () {
        const self = this;

        document.querySelectorAll('.icp-auto').forEach(element => {
            self.initIconPicker(element);
        });

        document.getElementById('inline-picker').addEventListener('iconpickerSelected', function (e) {
            const selected = e.detail.iconpickerInstance.options.fullClassFormatter(e.detail.iconpickerValue);
            
            if (self.targetInputField) {
                document.getElementById(self.targetInputField).value = selected;
            }

            if (self.targetIconTag) {
                // We need to replace the full tag with the new class.
                // We could simply apply the new selected class to the i element.
                // But there is an issue: when the previous class is not a valid fa icon the icon does not refresh.
                const iconTag = document.getElementById(self.targetIconTag);
                const newIconTag = document.createElement('i');
                newIconTag.id = self.targetIconTag;
                newIconTag.className = selected;
                iconTag.parentNode.replaceChild(newIconTag, iconTag);                
            }

            if (self.iconPickerModal != null)
            {
                self.iconPickerModal.hide();
            }
        });

    },
    methods: {
        initIconPicker(element) {
            $(element).iconpicker({
                title: false,
                templates: {
                    search: '<input type="search" class="form-control iconpicker-search" placeholder="" />' // just to leave empty the placeholder because it is not localized
                }
            });
        },
        show: function (targetInputField, targetIconTag) {
            this.targetInputField = targetInputField;
            this.targetIconTag = targetIconTag;

            if (this.iconPickerModal == null)
            {
                this.iconPickerModal = new bootstrap.Modal($("#iconPickerModal"), {
                    keyboard: false
                });
            }

            this.iconPickerModal.show();
        }
    }
})
