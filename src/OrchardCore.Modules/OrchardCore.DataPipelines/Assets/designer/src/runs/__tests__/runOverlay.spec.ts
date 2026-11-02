import { describe, expect, it } from "vitest";
import { isActiveRun, runOverlay, stepCountLabel } from "../runOverlay";
import { createDefinition, createRun } from "../../__tests__/fixtures";

const nodes = createDefinition().nodes;
const node = (id: string) => nodes.find((item) => item.id === id);

describe("runOverlay", () => {
    it("runOverlay_Run_MapsEachStepStatusAndCount", () => {
        const overlay = runOverlay(createRun(), nodes);

        expect(overlay.source).toMatchObject({ status: "Succeeded", label: "1,234 rows" });
        expect(overlay.filter).toMatchObject({ status: "Running", label: "1 row" });
        // A pending step has no count yet.
        expect(overlay.csv).toMatchObject({ status: "Pending", label: null });
        expect(overlay.filter.title).toContain("2 warnings");
        expect(overlay.join).toBeUndefined();
    });

    it("runOverlay_NoRunOrRemovedStep_IsEmptyForThem", () => {
        expect(runOverlay(null, nodes)).toEqual({});

        const run = createRun({ steps: [{ stepId: "gone", title: "Gone", status: "Succeeded", rowsIn: 1, rowsOut: 1, filesIn: 0, filesOut: 0, warnings: 0 }] });
        expect(runOverlay(run, nodes)).toEqual({});
    });

    it("stepCountLabel_FileStepAndDestination_ShowsFilesAndWhatADestinationReceived", () => {
        const step = { stepId: "csv", title: "", status: "Succeeded" as const, rowsIn: 10, rowsOut: 0, filesIn: 0, filesOut: 1, warnings: 0, error: null };

        // The CSV step's only output carries files.
        expect(stepCountLabel(step, node("csv"))).toBe("1 file");
        // A destination has no outputs: what it received.
        expect(stepCountLabel({ ...step, stepId: "table", rowsIn: 2500, filesOut: 0 }, node("table"))).toBe("2,500 rows");
        expect(stepCountLabel({ ...step, stepId: "email", rowsIn: 0, filesIn: 3, filesOut: 0 }, node("email"))).toBe("3 files");
    });

    it("isActiveRun_Statuses_OnlyQueuedAndRunning", () => {
        expect(isActiveRun(createRun({ status: "Queued" }))).toBe(true);
        expect(isActiveRun(createRun({ status: "Running" }))).toBe(true);
        expect(isActiveRun(createRun({ status: "Failed" }))).toBe(false);
        expect(isActiveRun(null)).toBe(false);
    });
});
