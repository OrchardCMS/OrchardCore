document.addEventListener('DOMContentLoaded', function () {

    var selectMenus = document.getElementsByClassName('field-type-select-menu');

    for (let x = 0; x < selectMenus.length; x++) {
        selectMenus[x].addEventListener('change', function (e) {
            var wrapper = e.target.closest('.properties-wrapper');
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

    var labelOptions = document.getElementsByClassName('field-label-option-select-menu');

    for (let x = 0; x < labelOptions.length; x++) {
        labelOptions[x].addEventListener('change', function (e) {

            var wrapper = e.target.closest('.properties-wrapper');
            var labelTextContainer = wrapper.querySelector('.label-text-container');

            if (e.target.value != 'None') {
                labelTextContainer.classList.remove('d-none');
            } else {
                labelTextContainer.classList.add('d-none');
            }
        });
    }
});
