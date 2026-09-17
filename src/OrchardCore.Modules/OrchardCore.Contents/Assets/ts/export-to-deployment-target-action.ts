import { wireDeploymentModalButtons } from "@orchardcore/bloom/helpers/deploymentForm";

wireDeploymentModalButtons(
    "modalExportContentToDeploymentTargetActionTarget",
    ".export-to-deployment-target",
    (event) => {
        const relatedTarget = (event as Event & { relatedTarget?: HTMLElement }).relatedTarget;
        const fields: Record<string, string> = {
            "ExportContentToDeploymentTarget.ContentItemId": relatedTarget?.dataset.contentItemId ?? "",
        };
        const latest = relatedTarget?.dataset.latest;

        if (latest) {
            fields["ExportContentToDeploymentTarget.Latest"] = latest;
        }

        return fields;
    },
);
