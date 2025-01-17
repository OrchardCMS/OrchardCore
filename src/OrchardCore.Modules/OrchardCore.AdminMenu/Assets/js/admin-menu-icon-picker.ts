// a wrapper around https://github.com/farbelous/fontawesome-iconpicker
// It makes easier to use a single picker instance with several input fields.
// How to use it: Call show() from outside , passing it the id's of the input fields you want the icon classes applied to.

import Vue from "vue";
import "fontawesome-iconpicker"
import { Modal } from "bootstrap";

var iconPickerVue = new Vue({
    el: '#iconPickerVue',
    data: {
        targetInputField: '',
        targetIconTag: '',
        iconPickerModal: null,
    },
    mounted: function () {
        var self = this;

        this.$el.querySelector('.icp-auto').iconpicker({
            title: false,
            templates: {
                search: '<input type="search" class="form-control iconpicker-search" placeholder="" />' // just to leave empty the placeholder because it is not localized
            }
        });

        this.$el.querySelector('#inline-picker').addEventListener('iconpickerSelected', function (e) {
            var selected = e.iconpickerInstance.options.fullClassFormatter(e.iconpickerValue);
            
            if (self.targetInputField) {
                document.getElementById(self.targetInputField).value = selected;
            }

            if (self.targetIconTag) {
                // We need to replace the full tag with the new class.
                // We could simply apply the new selected class to the i element.
                // But there is an issue: when the previous class is not a valid fa icon the icon does not refresh.
                var iconTagElement = document.getElementById(self.targetIconTag);
                var newIconTagElement = document.createElement('i');
                newIconTagElement.id = self.targetIconTag;
                newIconTagElement.className = selected;
                iconTagElement?.parentNode?.replaceChild(newIconTagElement, iconTagElement);
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
                this.iconPickerModal = new Modal(this.$el.querySelector("#iconPickerModal"), {
                    keyboard: false
                });
            }

            this.iconPickerModal.show();
        }
    }
})
