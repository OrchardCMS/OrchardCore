import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

// Wires the "Select all" checkbox of every content type picker (SelectContentTypes view component),
// including pickers injected after the page loaded, for example in the workflow designer panel.
const initContentTypeCheckAll = (container: HTMLElement) => {
    const master = container.querySelector<HTMLInputElement>('input[type="checkbox"].master');
    const slaves = container.querySelectorAll<HTMLInputElement>('.slaves input[type="checkbox"]:not(:disabled)');

    if (!master) {
        return;
    }

    const updateMaster = () => {
        master.checked = container.querySelectorAll('.slaves input[type="checkbox"]:not(:checked)').length === 0;
    };

    master.addEventListener("change", () => {
        slaves.forEach((slave) => {
            slave.checked = master.checked;
        });
    });

    slaves.forEach((slave) => slave.addEventListener("change", updateMaster));

    updateMaster();
};

observeAndInit(".check-all.content-types", initContentTypeCheckAll);
