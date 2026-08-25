const root = document.querySelector<HTMLElement>(".rate-limit-policy-fields");
// The DisplayDriver prefixes generated ids (RateLimitPolicyDisplayDriver's default prefix
// is its model type name, "RateLimitPolicy"), so a hardcoded getElementById("Scope") never
// matches - use an attribute selector against the field-name suffix instead. "Path_group"/
// "GroupName_group" are plain unprefixed id="" on <div>s, not asp-for targets, so they're
// unaffected and stay getElementById lookups.
const scopeElement = document.querySelector<HTMLSelectElement>("select[id$='Scope']");
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
