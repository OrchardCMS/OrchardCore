import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

const patternTextArea = document.getElementById("Pattern") as HTMLTextAreaElement | null;

if (patternTextArea) {
    initLiquidPatternEditor(patternTextArea);
}

const manageContainedItemRoutesElement = document.getElementById("ManageContainedItemRoutes") as HTMLInputElement | null;

manageContainedItemRoutesElement?.addEventListener("change", (e) => {
    const checked = (e.target as HTMLInputElement).checked;
    document.querySelectorAll<HTMLElement>(".manage-contained-item-routes").forEach((element) => {
        element.style.display = checked ? "" : "none";
    });
});
