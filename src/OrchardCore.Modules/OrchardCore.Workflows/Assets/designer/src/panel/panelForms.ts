// What the activity and workflow panels share: settling the form they show, moving between their tabs with the
// keyboard, and reporting the errors of their changes.

import type ServerFormHost from "@bloom/designer/panel/ServerFormHost.vue";
import { confirmAction } from "@bloom/designer/ui/confirm";
import { showToast } from "@bloom/designer/ui/toasts";
import { DesignerApiError } from "../api/designerApi";
import { t } from "../i18n";

type FormHost = Pick<InstanceType<typeof ServerFormHost>, "apply" | "isInvalid" | "discard">;

/**
 * Applies the pending changes of a form. When they are invalid, asks whether to discard them; resolves to false when
 * the user wants to keep editing.
 */
export const settleForm = async (host: FormHost | null | undefined) => {
    if (!host || (await host.apply()) || !host.isInvalid()) {
        return true;
    }

    const discard = await confirmAction({
        title: t("DiscardChangesTitle"),
        message: t("DiscardChangesMessage"),
        okText: t("Discard"),
        cancelText: t("KeepEditing"),
    });

    if (discard) {
        host.discard();
    }

    return discard;
};

/**
 * The index of the tab a key of a tab list moves to, following the ARIA tabs pattern: arrow keys (mirrored in RTL),
 * Home and End. Returns null, without handling the event, for the other keys.
 */
export const tabIndexForKey = (event: KeyboardEvent, index: number, count: number, tabList: HTMLElement | null | undefined) => {
    const rtl = !!tabList && getComputedStyle(tabList).direction === "rtl";
    let next: number;

    if (event.key === "ArrowRight" || event.key === "ArrowLeft") {
        next = index + ((event.key === "ArrowRight") !== rtl ? 1 : -1);
    } else if (event.key === "Home") {
        next = 0;
    } else if (event.key === "End") {
        next = count - 1;
    } else {
        return null;
    }

    event.preventDefault();

    return (next + count) % count;
};

/**
 * Reports an error of a change made in a panel: a conflict goes to `onConflict`, any other error shows a toast.
 */
export const reportPanelError = (error: unknown, onConflict: (error: DesignerApiError) => void) => {
    if (error instanceof DesignerApiError && error.isConflict) {
        onConflict(error);

        return;
    }

    showToast({ message: t("ApplyFailed"), variant: "danger" });
};
