import type { DesignIssue } from "../api/types";

/**
 * What clicking Publish does, given the draft's issues:
 * - `blocked`: the draft has errors, which must be fixed first (Publish is disabled then, but the server may find some);
 * - `confirm`: the draft has warnings, so ask before publishing;
 * - `publish`: publish right away.
 */
export type PublishDecision = { kind: "blocked"; errors: DesignIssue[] } | { kind: "confirm"; warnings: DesignIssue[] } | { kind: "publish" };

export const decidePublish = (issues: DesignIssue[]): PublishDecision => {
    const errors = issues.filter((issue) => issue.severity === "Error");

    if (errors.length > 0) {
        return { kind: "blocked", errors };
    }

    const warnings = issues.filter((issue) => issue.severity === "Warning");

    return warnings.length > 0 ? { kind: "confirm", warnings } : { kind: "publish" };
};

export const hasErrors = (issues: DesignIssue[]) => issues.some((issue) => issue.severity === "Error");
