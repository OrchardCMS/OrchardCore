import { describe, expect, it } from "vitest";
import { decidePublish, hasErrors } from "../publishDecision";
import type { DesignIssue } from "../../core/types";

const error: DesignIssue = { severity: "Error", message: "No start.", code: "NoStart" };
const warning: DesignIssue = { severity: "Warning", message: "Unreachable.", code: "Unreachable" };

describe("decidePublish", () => {
    it("decidePublish_Errors_IsBlocked", () => {
        expect(decidePublish([warning, error])).toEqual({ kind: "blocked", errors: [error] });
        expect(hasErrors([warning, error])).toBe(true);
    });

    it("decidePublish_OnlyWarnings_AsksForConfirmation", () => {
        expect(decidePublish([warning])).toEqual({ kind: "confirm", warnings: [warning] });
        expect(hasErrors([warning])).toBe(false);
    });

    it("decidePublish_NoIssues_Publishes", () => {
        expect(decidePublish([])).toEqual({ kind: "publish" });
    });
});
