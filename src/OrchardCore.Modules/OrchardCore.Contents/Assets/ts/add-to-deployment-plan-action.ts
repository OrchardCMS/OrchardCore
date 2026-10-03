import { wireDeploymentModalButtons } from "@orchardcore/bloom/helpers/deploymentForm";

wireDeploymentModalButtons("modalAddToDeploymentPlanActionDeploymentPlan", ".add-to-deployment-plan", (event) => {
    const relatedTarget = (event as Event & { relatedTarget?: HTMLElement }).relatedTarget;

    return { contentItemId: relatedTarget?.dataset.contentItemId ?? "" };
});
