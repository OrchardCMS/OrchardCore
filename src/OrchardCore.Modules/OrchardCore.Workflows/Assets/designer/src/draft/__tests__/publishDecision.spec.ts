import { describe, expect, it } from "vitest";
import { decidePublish } from "../publishDecision";
import type { DesignIssue } from "../../api/types";

const error: DesignIssue = { severity: "Error", code: "InvalidTransition", message: "Broken", activityId: "a" };
const warning: DesignIssue = { severity: "Warning", code: "UnreachableActivity", message: "Unreachable", activityId: "b" };

describe("decidePublish", () => {
    it("decidePublish_Errors_BlocksWithTheErrorsOnly", () => {
        expect(decidePublish([warning, error], 3)).toEqual({ kind: "blocked", errors: [error] });
    });

    it("decidePublish_Warnings_AsksWithTheWarnings", () => {
        expect(decidePublish([warning], 0)).toEqual({ kind: "confirm", warnings: [warning], runningInstanceCount: 0 });
    });

    it("decidePublish_RunningInstancesOnly_AsksWithTheirCount", () => {
        expect(decidePublish([], 2)).toEqual({ kind: "confirm", warnings: [], runningInstanceCount: 2 });
    });

    it("decidePublish_NoIssuesNorInstances_PublishesRightAway", () => {
        expect(decidePublish([], 0)).toEqual({ kind: "publish" });
    });
});
