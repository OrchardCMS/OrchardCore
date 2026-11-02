import type { DesignIssue, DesignerNode } from "../api/types";

// How the shared issues list reads the issues of a pipeline: they target a step.

export const issueTarget = (issue: DesignIssue) => issue.stepId;

export const issueTitle = (nodes: DesignerNode[], issue: DesignIssue) => nodes.find((node) => node.id === issue.stepId)?.displayText;
