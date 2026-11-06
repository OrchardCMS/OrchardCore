import type { DesignIssue, DesignerNode } from "../api/types";

// How the shared issues list reads the issues of a workflow: they target an activity.

export const issueTarget = (issue: DesignIssue) => issue.activityId;

export const issueTitle = (nodes: DesignerNode[], issue: DesignIssue) => nodes.find((node) => node.id === issue.activityId)?.title;
