import type { JournalRecord } from "../api/types";
import { t } from "../i18n";

// How a journal record's status is shown: a faulted or failed execution in red, a failed attempt that's retried in
// yellow, an activity that waits in blue, and a completed one in green.
export const journalStatusClass = (record: JournalRecord): string => {
    switch (record.status) {
        case "Faulted":
        case "Failed":
            return "text-bg-danger";
        case "Retrying":
            return "text-bg-warning";
        case "Halted":
            return "text-bg-info";
        default:
            return "text-bg-success";
    }
};

// Direct t("…") calls, so the translations spec sees every key.
export const journalStatusLabel = (record: JournalRecord): string => {
    switch (record.status) {
        case "Faulted":
            return t("JournalFaulted");
        case "Failed":
            return t("JournalFailed");
        case "Retrying":
            return t("JournalRetrying");
        case "Halted":
            return t("JournalHalted");
        default:
            return t("JournalCompleted");
    }
};

// A record that completed or halted with an error ran a script that failed and used its fallback value; the other
// records with an error are the activity's own failures.
export const isScriptError = (record: JournalRecord): boolean => (record.status === "Completed" || record.status === "Halted") && !!record.error;
