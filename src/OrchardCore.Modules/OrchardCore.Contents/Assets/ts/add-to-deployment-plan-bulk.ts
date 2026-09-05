import { wireDeploymentModalButtons } from "@orchardcore/bloom/helpers/deploymentForm";

wireDeploymentModalButtons(
    "modalAddToDeploymentPlanContentsBulkActionsDeploymentPlan",
    ".add-to-deployment-plan-bulk-actions",
    () => {
        const itemIds = Array.from(
            document.querySelectorAll<HTMLInputElement>("input[type='checkbox'][name='itemIds']:checked"),
        ).map((element) => element.value);

        return itemIds.length > 1 ? { itemIds } : null;
    },
);
