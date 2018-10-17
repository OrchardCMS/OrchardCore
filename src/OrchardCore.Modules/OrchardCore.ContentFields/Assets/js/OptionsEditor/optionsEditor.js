function initializeOptionsEditor(elem, data, defaultValue, modalBodyElement) {

    var previouslyChecked;

    var optionsTable = {
        template: '#options-table',
        props: ['value'],
        name: 'options-table',
        data: function() {
            return {
                options: data,
                selected: defaultValue
            }
        },
        methods: {
            add: function () {
                this.options.push({ name: '', value: ''});
            },
            remove: function (index) {
                this.options.splice(index, 1);
            },
            uncheck: function (index, value) {
                if (index == previouslyChecked) {
                    $('#customRadio_' + index)[0].checked = false;
                    previouslyChecked = null;
                }
                else {
                    previouslyChecked = index;
                }

            },
            getFormattedList: function () {
                return JSON.stringify(this.options.filter(function (x) { return !IsNullOrWhiteSpace(x.name) && !IsNullOrWhiteSpace(x.value) }));
            }
        }
    };

    new Vue({
        components: {
            optionsTable: optionsTable
        },
        el: elem,
        data: {
            dragging: false
        },
        methods: {
            showModal: function (event) {

                var modal = $(modalBodyElement).modal();
                modal.show();

                if (this.canAddMedia) {
                    $("#mediaApp").detach().appendTo($(modalBodyElement).find('.modal-body'));
                    $("#mediaApp").show();
                    var modal = $(modalBodyElement).modal();
                    $(modalBodyElement).find('.mediaFieldSelectButton').off('click').on('click', function (v) {
                        if ((mediaApp.selectedMedias.length > 1) && (allowMultiple === false)) {
                            alert($('#onlyOneItemMessage').val());
                            mediaFieldApp.mediaItems.push(mediaApp.selectedMedias[0]);
                        } else {
                            mediaFieldApp.mediaItems = mediaFieldApp.mediaItems.concat(mediaApp.selectedMedias);
                        }
                        // we don't want the included medias to be still selected the next time we open the modal.
                        mediaApp.selectedMedias = [];

                        modal.modal('hide');
                        return true;
                    });
                }
            }
        }
    });

}

function IsNullOrWhiteSpace(str) {
    return str === null || str.match(/^ *$/) !== null;
}