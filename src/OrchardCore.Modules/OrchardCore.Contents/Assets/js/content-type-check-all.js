/** Content Type Check All **/
document.addEventListener('DOMContentLoaded', function (event) {
    const checkAllContainers = document.querySelectorAll('.check-all.content-types');

    checkAllContainers.forEach(container => {
        const master = container.querySelector('input[type="checkbox"].master');
        const slaves = [...container.querySelectorAll('.slaves input[type="checkbox"]:not(:disabled)')];

        const updateMaster = () => {
            const allChecked = slaves.every(slave => slave.checked);
            master.checked = allChecked;
        };

        master.addEventListener('change', event => {
            const isChecked = event.target.checked;

            slaves.forEach(slave => {
                slave.checked = isChecked;
            });
        });

        slaves.forEach(slave => {
            slave.addEventListener('change', updateMaster);
        });

        updateMaster();
    });
});

