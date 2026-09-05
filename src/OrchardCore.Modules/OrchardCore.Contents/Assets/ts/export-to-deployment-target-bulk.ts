import { wireDeploymentModalButtons } from "@orchardcore/bloom/helpers/deploymentForm";

wireDeploymentModalButtons(
    "modalExportContentToDeploymentTargetContentBulkActionsTarget",
    ".export-to-deployment-target-bulk-actions",
    () => {
        const itemIds = Array.from(
            document.querySelectorAll<HTMLInputElement>("input[type='checkbox'][name='itemIds']:checked"),
        ).map((element) => element.value);

        return itemIds.length > 1 ? { "ExportContentToDeploymentTarget.ItemIds": itemIds } : null;
    },
);
