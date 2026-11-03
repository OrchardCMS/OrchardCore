import type { DesignIssue } from "../core/types";

/**
 * What clicking Publish does, given the draft's issues:
 * - `blocked`: the draft has errors, which must be fixed first;
 * - `confirm`: the draft has warnings, so ask before publishing;
 * - `publish`: publish right away.
 */
export type PublishDecision<TIssue extends DesignIssue = DesignIssue> =
    | { kind: "blocked"; errors: TIssue[] }
    | { kind: "confirm"; warnings: TIssue[] }
    | { kind: "publish" };

export const decidePublish = <TIssue extends DesignIssue>(issues: TIssue[]): PublishDecision<TIssue> => {
    const errors = issues.filter((issue) => issue.severity === "Error");

    if (errors.length > 0) {
        return { kind: "blocked", errors };
    }

    const warnings = issues.filter((issue) => issue.severity === "Warning");

    return warnings.length > 0 ? { kind: "confirm", warnings } : { kind: "publish" };
};

export const hasErrors = (issues: DesignIssue[]) => issues.some((issue) => issue.severity === "Error");
