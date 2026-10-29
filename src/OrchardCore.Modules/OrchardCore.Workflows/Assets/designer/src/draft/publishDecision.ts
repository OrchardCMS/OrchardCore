import { decidePublish as decide, type PublishDecision as DraftPublishDecision } from "@bloom/designer/draft/publishDecision";
import type { DesignIssue } from "../api/types";

/**
 * What clicking Publish does, given the draft's issues and running instances (see the shared decision). When the
 * draft has warnings, the confirmation also mentions the running instances, which keep running on their version.
 */
export type PublishDecision =
    | Exclude<DraftPublishDecision<DesignIssue>, { kind: "confirm" }>
    | { kind: "confirm"; warnings: DesignIssue[]; runningInstanceCount: number };

export const decidePublish = (issues: DesignIssue[], runningInstanceCount: number): PublishDecision => {
    const decision = decide(issues);

    // Running instances keep running on the version they started on, so they alone don't need confirming.
    return decision.kind === "confirm" ? { ...decision, runningInstanceCount } : decision;
};
