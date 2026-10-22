import { getDatasetJson } from "../helpers/dataset";

const LIQUID_SYNTAX = "Liquid";

// Toggles two sets of sibling groups (looked up by id) based on whether a <select>'s
// value matches the "Liquid" syntax option - the JavaScript-syntax groups are shown
// otherwise. Reads the group id lists off the select's own data-javascript-group-ids/
// data-liquid-group-ids attributes (JSON arrays of element ids), so one shared
// implementation covers every "pick a syntax, show the matching input(s)" view instead
// of duplicating the same toggle logic per view.
const initSyntaxToggle = (element: HTMLElement) => {
    const select = element as HTMLSelectElement;
    const javaScriptGroupIds = getDatasetJson<string[]>(select, "javascriptGroupIds") ?? [];
    const liquidGroupIds = getDatasetJson<string[]>(select, "liquidGroupIds") ?? [];

    const javaScriptGroups = javaScriptGroupIds
        .map((id) => document.getElementById(id))
        .filter((group): group is HTMLElement => group !== null);
    const liquidGroups = liquidGroupIds
        .map((id) => document.getElementById(id))
        .filter((group): group is HTMLElement => group !== null);

    const toggleGroups = () => {
        const useLiquid = select.value === LIQUID_SYNTAX;

        javaScriptGroups.forEach((group) => group.classList.toggle("d-none", useLiquid));
        liquidGroups.forEach((group) => group.classList.toggle("d-none", !useLiquid));
    };

    select.addEventListener("change", toggleGroups);
    toggleGroups();
};

export default initSyntaxToggle;
