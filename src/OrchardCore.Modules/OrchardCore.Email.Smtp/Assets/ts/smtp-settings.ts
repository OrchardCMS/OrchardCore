// The DisplayDriver prefixes generated ids (SiteDisplayDriver/SectionDisplayDriverBase's
// default prefix includes the settings class name), so hardcoded getElementById(...) never
// matches - use attribute selectors against each field-name suffix instead.
const deliveryMethod = document.querySelector<HTMLSelectElement>("select[id$='DeliveryMethod']");
const autoSelectEncryption = document.querySelector<HTMLInputElement>("input[id$='AutoSelectEncryption']");
const encryptionMethod = document.querySelector<HTMLSelectElement>("select[id$='EncryptionMethod']");

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
