formElementLabelManager = function () {
    const initilize = (wrapper) => {
        const selectMenus = Array.from(wrapper.getElementsByClassName('field-label-option-select-menu'));

        selectMenus.forEach(selectMenu => {
            selectMenu.addEventListener('change', event => {
                const widgetWrapper = event.target.closest('.widget-editor-body');
                const labelTextContainer = widgetWrapper.querySelector('.label-text-container');

                if (event.target.value === 'None') {
                    labelTextContainer.classList.add('d-none');
                } else {
                    labelTextContainer.classList.remove('d-none');
                }
            });
            selectMenu.dispatchEvent(new Event('change'));
        });
    };

    return {
        initilize
    };
}();

document.addEventListener('DOMContentLoaded', () => formElementLabelManager.initilize(document));
