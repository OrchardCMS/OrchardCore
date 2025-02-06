formElementLabelManager = function () {
    const initilize = (wrapper) => {
        var selectMenus = wrapper.getElementsByClassName('field-label-option-select-menu');

        for (let i = 0; i < selectMenus.length; i++) {
            var selectMenu = selectMenus[i];
            selectMenu.addEventListener('change', function (e) {
                var labelTextContainer = wrapper.querySelector('.label-text-container');

                if (e.target.value == 'None') {
                    labelTextContainer.classList.add('d-none');
                } else {
                    labelTextContainer.classList.remove('d-none');
                }
            });
            selectMenu.dispatchEvent(new Event('change'));
        }
    };

    return {
        initilize: initilize
    };
}();

document.addEventListener('DOMContentLoaded', function () {
    formElementLabelManager.initilize(document);
});
