const root = document.querySelector<HTMLElement>(".rate-limit-policy-fields");
const scopeElement = document.getElementById("Scope") as HTMLSelectElement | null;
const pathGroup = document.getElementById("Path_group");
const rateLimitGroup = document.getElementById("GroupName_group");

if (root && scopeElement && pathGroup && rateLimitGroup) {
    const isTargetLocked = root.dataset.isTargetLocked === "true";
    const endpointScope = root.dataset.endpointScope ?? "";
    const groupScope = root.dataset.groupScope ?? "";

    const updateScopeFields = () => {
        if (isTargetLocked) {
            return;
        }

        const scope = scopeElement.value;
        pathGroup.classList.toggle("d-none", scope !== endpointScope);
        rateLimitGroup.classList.toggle("d-none", scope !== groupScope);
    };

    scopeElement.addEventListener("change", updateScopeFields);
    updateScopeFields();
}

export {};
