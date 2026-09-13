document.querySelectorAll<HTMLElement>(".check-all-container").forEach((container) => {
    const master = container.querySelector<HTMLInputElement>('input[type="checkbox"].check-all-master');
    const slaves = container.querySelectorAll<HTMLInputElement>(
        '.check-all-slave input[type="checkbox"]:not(:disabled)',
    );

    if (!master) {
        return;
    }

    const updateMaster = () => {
        master.checked = Array.from(slaves).filter((slave) => !slave.checked).length === 0;
    };

    master.addEventListener("change", () => {
        slaves.forEach((slave) => {
            slave.checked = master.checked;
        });
    });

    slaves.forEach((slave) => {
        slave.addEventListener("change", updateMaster);
    });

    updateMaster();
});
