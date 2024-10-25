/** Content Type Check All **/
document.addEventListener('DOMContentLoaded', function (event) {
    let checkAllContainer = document.querySelectorAll('.check-all.content-types');

    checkAllContainer.forEach((container) => {
        let master = container.querySelector('input[type="checkbox"].master');
        let slaves = container.querySelectorAll('.slaves input[type="checkbox"]:not(:disabled)');

        let updateMaster = function () {
            let allChecked = container.querySelectorAll('.slaves input[type="checkbox"]:not(:checked)').length == 0;
            master.checked = allChecked;
        };

        master.addEventListener('change', function (elem) {
            let isChecked = elem.target.checked;

            slaves.forEach((slave) => {
                slave.checked = isChecked;
            });
        });

        slaves.forEach((slave) => {
            slave.addEventListener('change', function () {
                updateMaster();
            });
        });

        updateMaster();
    });
});
