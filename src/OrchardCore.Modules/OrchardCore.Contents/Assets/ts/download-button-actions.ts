import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

const toggleDropdown = (element: HTMLElement) => {
    const dropdownMenu = element.closest(".dropdown-menu");
    const dropdownToggle = dropdownMenu?.previousElementSibling as HTMLElement | null;
    if (dropdownToggle) {
        bootstrap.Dropdown.getOrCreateInstance(dropdownToggle).toggle();
    }
};

observeAndInit(".download-json-toggle-dropdown", (element) => {
    element.addEventListener("click", () => toggleDropdown(element));
});
