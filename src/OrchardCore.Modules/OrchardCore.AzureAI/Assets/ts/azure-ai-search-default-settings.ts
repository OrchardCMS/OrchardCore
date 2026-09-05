const typeMenu = document.querySelector<HTMLSelectElement>(".azure-ai-auth-type");
const keyWrapper = document.querySelector<HTMLElement>(".azure-ai-api-key-wrapper");
const identityWrapper = document.querySelector<HTMLElement>(".azure-ai-identity-wrapper");

if (typeMenu && keyWrapper && identityWrapper) {
    typeMenu.addEventListener("change", () => {
        if (typeMenu.value === "ApiKey") {
            keyWrapper.classList.remove("d-none");
            identityWrapper.classList.add("d-none");
        } else if (typeMenu.value === "ManagedIdentity") {
            keyWrapper.classList.add("d-none");
            identityWrapper.classList.remove("d-none");
        } else {
            keyWrapper.classList.add("d-none");
            identityWrapper.classList.add("d-none");
        }
    });
}
