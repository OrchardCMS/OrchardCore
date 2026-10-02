import type { DesignIssue } from "../api/types";

/**
 * What clicking Publish does, given the draft's issues and running instances:
 * - `blocked`: the draft has errors, which must be fixed first;
 * - `confirm`: the draft has warnings or running instances, so ask before publishing;
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

    // Running instances continue on the new definition (there is no version pinning in Phase 1), so the
    // user is told before publishing.
    if (warnings.length > 0 || runningInstanceCount > 0) {
        return { kind: "confirm", warnings, runningInstanceCount };
    }

    return { kind: "publish" };
};
