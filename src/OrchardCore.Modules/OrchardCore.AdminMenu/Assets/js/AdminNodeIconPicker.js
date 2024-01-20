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
        var self = this;

        $('.icp-auto').iconpicker({
            title: false,
            templates: {
                search: '<input type="search" class="form-control iconpicker-search" placeholder="" />' // just to leave empty the placeholder because it is not localized
            }
        });

        $('#inline-picker').on('iconpickerSelected', function (e) {
            var selected = e.iconpickerInstance.options.fullClassFormatter(e.iconpickerValue);
            
            if (self.targetInputField) {
                $('#' + self.targetInputField).val(selected);
            }

            if (self.targetIconTag) {
                // We need to replace the full tag with the new class.
                // We could simply apply the new selected class to the i element.
                // But there is an issue: when the previous class is not a valid fa icon the icon does not refresh.
                $('#' + self.targetIconTag).replaceWith('<i id="' + self.targetIconTag + '" class="'+ selected + '"></i>')                
            }

            if (self.iconPickerModal != null)
            {
                self.iconPickerModal.hide();
            }
        });

    },
    methods: {
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
