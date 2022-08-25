formsWidgetEditor = function () {

    const initilizeFieldType = (wrapper) => {
        var selectMenus = wrapper.getElementsByClassName('field-type-select-menu');

        for (let x = 0; x < selectMenus.length; x++) {
            selectMenus[x].addEventListener('change', function (e) {
                var labelTextContainer = wrapper.querySelector('.label-text-container');
                var labelOption = wrapper.querySelector('.field-label-option-select-menu');
                var visibleForInputContainers = wrapper.getElementsByClassName('show-for-input');

                if (e.target.value == 'reset' || e.target.value == 'submit' || e.target.value == 'hidden') {
                    for (let i = 0; i < visibleForInputContainers.length; i++) {
                        visibleForInputContainers[i].classList.add('d-none');
                    }
                    labelTextContainer.classList.add('d-none');
                } else {
                    for (let i = 0; i < visibleForInputContainers.length; i++) {
                        visibleForInputContainers[i].classList.remove('d-none');
                    }
                    labelOption.dispatchEvent(new Event('change'));
                }
            });
        }
    };

    const initilizeLabelOptions = (wrapper) => {
        var labelOptions = wrapper.getElementsByClassName('field-label-option-select-menu');

        for (let x = 0; x < labelOptions.length; x++) {
            labelOptions[x].addEventListener('change', function (e) {
                var container = labelOptions[x].closest('.properties-wrapper');
                if (container == null) {
                    contine;
                }
                var labelContainer = container.querySelector('.label-text-container');
                if (e.target.value != 'None') {
                    labelContainer.classList.remove('d-none');
                } else {
                    labelContainer.classList.add('d-none');
                }
            });
        }
    };

    return {
        initilizeFieldType: initilizeFieldType,
        initilizeLabelOptions: initilizeLabelOptions
    };
}();

document.addEventListener('DOMContentLoaded', function () {
    formsWidgetEditor.initilizeFieldType(document);
    formsWidgetEditor.initilizeLabelOptions(document);
});
