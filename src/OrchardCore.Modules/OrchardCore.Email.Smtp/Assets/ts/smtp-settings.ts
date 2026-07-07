const deliveryMethod = document.getElementById("DeliveryMethod") as HTMLSelectElement | null;
const autoSelectEncryption = document.getElementById("AutoSelectEncryption") as HTMLInputElement | null;
const encryptionMethod = document.getElementById("EncryptionMethod") as HTMLSelectElement | null;

const showSelectedCollapse = () => {
    const option = deliveryMethod?.selectedOptions?.[0];
    const target = option?.dataset.bsTarget;

    if (!target) {
        return;
    }

    const collapseElement = document.querySelector<HTMLElement>(target);

    if (collapseElement) {
        bootstrap.Collapse.getOrCreateInstance(collapseElement).show();
    }
};

const showEncryptionMethod = () => {
    if (encryptionMethod && autoSelectEncryption) {
        encryptionMethod.disabled = autoSelectEncryption.checked;
    }
};

deliveryMethod?.addEventListener("change", showSelectedCollapse);
showSelectedCollapse();

autoSelectEncryption?.addEventListener("change", showEncryptionMethod);
showEncryptionMethod();

export {};
