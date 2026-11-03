const actions = document.getElementById("actions");
const selectAll = document.getElementById("select-all") as HTMLInputElement | null;
const items = document.getElementById("items");
const selectedItems = document.getElementById("selected-items");
const hiddenBulkAction = document.querySelector<HTMLInputElement>('[name="BulkAction"]');
const submit = document.querySelector<HTMLElement>('[name="submit.BulkAction"]');
const deletePolicyForm = document.getElementById("delete-policy-form") as HTMLFormElement | null;
const deletePolicyId = document.getElementById("delete-policy-id") as HTMLInputElement | null;
const enablePolicyForm = document.getElementById("enable-policy-form") as HTMLFormElement | null;
const enablePolicyId = document.getElementById("enable-policy-id") as HTMLInputElement | null;
const disablePolicyForm = document.getElementById("disable-policy-form") as HTMLFormElement | null;
const disablePolicyId = document.getElementById("disable-policy-id") as HTMLInputElement | null;
const clonePolicyForm = document.getElementById("clone-policy-form") as HTMLFormElement | null;
const clonePolicyId = document.getElementById("clone-policy-id") as HTMLInputElement | null;

const root = document.querySelector<HTMLElement>(".rate-limits-index");
const selectedText = root?.dataset.selectedText ?? "";

const getCheckboxes = () => [...document.querySelectorAll<HTMLInputElement>('input[name="policyIds"]')];

const updateSelection = () => {
    const checkboxes = getCheckboxes();
    const selectedCount = checkboxes.filter((x) => x.checked).length;
    const totalCount = checkboxes.length;

    if (selectAll) {
        selectAll.checked = totalCount > 0 && selectedCount === totalCount;
        selectAll.indeterminate = selectedCount > 0 && selectedCount < totalCount;
    }

    if (actions) {
        actions.classList.toggle("d-none", selectedCount === 0);
    }

    if (items && selectedItems) {
        items.classList.toggle("d-none", selectedCount > 0);
        selectedItems.classList.toggle("d-none", selectedCount === 0);
        selectedItems.textContent = `${selectedCount} ${selectedText}`;
    }
};

selectAll?.addEventListener("change", () => {
    for (const checkbox of getCheckboxes()) {
        checkbox.checked = selectAll.checked;
    }

    updateSelection();
});

for (const checkbox of getCheckboxes()) {
    checkbox.addEventListener("change", updateSelection);
}

for (const action of document.querySelectorAll<HTMLElement>('.dropdown-item[data-action]')) {
    action.addEventListener("click", () => {
        confirmDialog({
            ...action.dataset,
            callback: (confirmed: boolean) => {
                if (!confirmed || !hiddenBulkAction) {
                    return;
                }

                hiddenBulkAction.value = action.dataset.action ?? "";
                submit?.click();
            },
        });
    });
}

for (const button of document.querySelectorAll<HTMLElement>('[data-delete-policy-id]')) {
    button.addEventListener("click", () => {
        confirmDialog({
            ...button.dataset,
            callback: (confirmed: boolean) => {
                if (!confirmed || !deletePolicyId || !deletePolicyForm) {
                    return;
                }

                deletePolicyId.value = button.dataset.deletePolicyId ?? "";
                deletePolicyForm.submit();
            },
        });
    });
}

for (const button of document.querySelectorAll<HTMLElement>('[data-enable-policy-id]')) {
    button.addEventListener("click", () => {
        confirmDialog({
            ...button.dataset,
            callback: (confirmed: boolean) => {
                if (!confirmed || !enablePolicyId || !enablePolicyForm) {
                    return;
                }

                enablePolicyId.value = button.dataset.enablePolicyId ?? "";
                enablePolicyForm.submit();
            },
        });
    });
}

for (const button of document.querySelectorAll<HTMLElement>('[data-disable-policy-id]')) {
    button.addEventListener("click", () => {
        confirmDialog({
            ...button.dataset,
            callback: (confirmed: boolean) => {
                if (!confirmed || !disablePolicyId || !disablePolicyForm) {
                    return;
                }

                disablePolicyId.value = button.dataset.disablePolicyId ?? "";
                disablePolicyForm.submit();
            },
        });
    });
}

for (const button of document.querySelectorAll<HTMLElement>('[data-clone-policy-id]')) {
    button.addEventListener("click", () => {
        if (!clonePolicyId || !clonePolicyForm) {
            return;
        }

        clonePolicyId.value = button.dataset.clonePolicyId ?? "";
        clonePolicyForm.submit();
    });
}

export {};
