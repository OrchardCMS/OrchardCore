import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

// The DisplayDriver prefixes generated ids, so hardcoded getElementById("Pattern")/
// getElementById("ManageContainedItemRoutes") never match - use attribute selectors
// against the field-name suffix instead, matching how @Html.IdFor(...) resolved them
// before this script was extracted from an inline @Html.IdFor-based Razor block.
const patternTextArea = document.querySelector<HTMLTextAreaElement>("textarea[id$='Pattern']");

if (patternTextArea) {
    initLiquidPatternEditor(patternTextArea);
}

const manageContainedItemRoutesElement = document.querySelector<HTMLInputElement>("input[id$='ManageContainedItemRoutes']");

manageContainedItemRoutesElement?.addEventListener("change", (e) => {
    const checked = (e.target as HTMLInputElement).checked;
    document.querySelectorAll<HTMLElement>(".manage-contained-item-routes").forEach((element) => {
        element.style.display = checked ? "" : "none";
    });
});
