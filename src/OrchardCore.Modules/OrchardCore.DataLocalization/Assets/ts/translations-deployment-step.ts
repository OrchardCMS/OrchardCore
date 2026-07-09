const includeAllCheckbox = document.querySelector<HTMLInputElement>(".translations-include-all");
const culturesSelection = document.getElementById("cultures-selection");

if (includeAllCheckbox && culturesSelection) {
    const toggleCulturesSelection = () => {
        culturesSelection.style.display = includeAllCheckbox.checked ? "none" : "block";
    };

    includeAllCheckbox.addEventListener("change", toggleCulturesSelection);
    toggleCulturesSelection();
}
