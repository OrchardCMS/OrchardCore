import type { DesignerNode, DesignerRun, RunStatus, RunStep, StepStatus } from "../api/types";
import { t } from "../i18n";

// What the canvas shows of a run selected in the Runs tab: each step's status, as a colored border, and the rows (or
// files) it produced, as a badge.

export interface StepOverlay {
    status: StepStatus;
    // The badge, such as "1,234 rows", or null when the step didn't run.
    label: string | null;
    // The details shown on hover and read by screen readers.
    title: string;
}

export const formatNumber = (value: number) => new Intl.NumberFormat(document.documentElement.lang || undefined).format(value);

export const rowCount = (count: number) => (count === 1 ? t("OneRow", "1 row") : t("RowCount", "{0} rows", formatNumber(count)));

export const fileCount = (count: number) => (count === 1 ? t("OneFile", "1 file") : t("FileCount", "{0} files", formatNumber(count)));

export const isActiveRun = (run: { status: RunStatus } | null | undefined) => run?.status === "Queued" || run?.status === "Running";

export const runStatusLabel = (status: RunStatus) =>
    ({
        Queued: () => t("RunQueued", "Queued"),
        Running: () => t("RunRunning", "Running"),
        Succeeded: () => t("RunSucceeded", "Succeeded"),
        Failed: () => t("RunFailed", "Failed"),
        Cancelled: () => t("RunCancelled", "Cancelled"),
    })[status]();

export const stepStatusLabel = (status: StepStatus) =>
    ({
        Pending: () => t("StepPending", "Pending"),
        Running: () => t("StepRunning", "Running"),
        Succeeded: () => t("StepSucceeded", "Succeeded"),
        Failed: () => t("StepFailed", "Failed"),
        Cancelled: () => t("StepCancelled", "Cancelled"),
        Skipped: () => t("StepSkipped", "Skipped"),
    })[status]();

/**
 * The badge of a step: what it produced, or what it received when it has no outputs (a destination). Files are shown
 * when the step has file ports or moved files.
 */
export const stepCountLabel = (step: RunStep, node: DesignerNode | undefined): string | null => {
    if (step.status === "Pending" || step.status === "Skipped") {
        return null;
    }

    const producesOutputs = node ? node.outputs.length > 0 : step.rowsOut > 0 || step.filesOut > 0;
    const rows = producesOutputs ? step.rowsOut : step.rowsIn;
    const files = producesOutputs ? step.filesOut : step.filesIn;
    const ports = node ? (producesOutputs ? node.outputs : node.inputs) : [];
    const hasFilePorts = ports.some((port) => port.kind === "Files");
    const hasRecordPorts = ports.length === 0 || ports.some((port) => port.kind === "Records");
    const parts: string[] = [];

    if (hasRecordPorts || rows > 0) {
        parts.push(rowCount(rows));
    }

    if (hasFilePorts || files > 0) {
        parts.push(fileCount(files));
    }

    return parts.join(" · ");
};

const stepTitle = (step: RunStep) => {
    const lines = [
        `${t("Status", "Status")}: ${stepStatusLabel(step.status)}`,
        `${t("Received", "Received")}: ${rowCount(step.rowsIn)}${step.filesIn > 0 ? `, ${fileCount(step.filesIn)}` : ""}`,
        `${t("Produced", "Produced")}: ${rowCount(step.rowsOut)}${step.filesOut > 0 ? `, ${fileCount(step.filesOut)}` : ""}`,
    ];

    if (step.warnings > 0) {
        lines.push(t("WarningCount", "{0} warnings", step.warnings));
    }

    if (step.error) {
        lines.push(step.error);
    }

    return lines.join("\n");
};

/**
 * The overlay of each step of `run`, by step id. Steps of the run that are no longer in the pipeline are left out.
 */
export const runOverlay = (run: DesignerRun | null, nodes: DesignerNode[]): Record<string, StepOverlay> => {
    const overlay: Record<string, StepOverlay> = {};

    if (!run) {
        return overlay;
    }

    const byId = new Map(nodes.map((node) => [node.id, node]));

    for (const step of run.steps) {
        const node = byId.get(step.stepId);

        if (node) {
            overlay[step.stepId] = { status: step.status, label: stepCountLabel(step, node), title: stepTitle(step) };
        }
    }

    return overlay;
};
