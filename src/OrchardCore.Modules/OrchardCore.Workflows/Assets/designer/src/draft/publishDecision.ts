import type { DesignIssue } from "../api/types";

/**
 * What clicking Publish does, given the draft's issues and running instances:
 * - `blocked`: the draft has errors, which must be fixed first;
 * - `confirm`: the draft has warnings, so ask before publishing (the dialog also mentions the running
 *   instances, which keep running on their version);
 * - `publish`: publish right away.
 */
export type PublishDecision =
    | { kind: "blocked"; errors: DesignIssue[] }
    | { kind: "confirm"; warnings: DesignIssue[]; runningInstanceCount: number }
    | { kind: "publish" };

export const decidePublish = (issues: DesignIssue[], runningInstanceCount: number): PublishDecision => {
    const errors = issues.filter((issue) => issue.severity === "Error");

    if (errors.length > 0) {
        return { kind: "blocked", errors };
    }

    const warnings = issues.filter((issue) => issue.severity === "Warning");

    // Running instances keep running on the version they started on, so they alone don't need confirming.
    if (warnings.length > 0) {
        return { kind: "confirm", warnings, runningInstanceCount };
    }

    return { kind: "publish" };
};
