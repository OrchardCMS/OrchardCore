import mitt from "mitt";
import dbg from "debug";
import { SeverityLevel } from "./interfaces";

const debug = dbg("orchardcore:bloom:notifier");

interface ProblemDetailsLike {
    title?: string;
    detail?: string;
}

interface ValidationProblemDetailsLike extends ProblemDetailsLike {
    errors?: Record<string, string[]>;
}

declare global {
    interface Window {
        notificationBus: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    }
}

export function registerNotificationBus() {
    if (window.notificationBus === undefined) {
        window.notificationBus = mitt();
    }

    return window.notificationBus;
}

interface INotificationMessage {
    summary?: string;
    detail?: string;
    severity?: SeverityLevel;
}

export class NotificationMessage implements INotificationMessage {
    readonly summary?: string;
    readonly detail?: string;
    readonly severity?: SeverityLevel;

    constructor(data?: INotificationMessage | null) {
        if (!data) {
            return;
        }

        Object.getOwnPropertyNames(data).forEach((property) => {
            (this as any)[property] = (data as any)[property]; // eslint-disable-line @typescript-eslint/no-explicit-any
        });
    }
}

function notifyMessage(message: NotificationMessage | null | undefined): void {
    if (!message) {
        debug("Notification message is null or undefined");
        return;
    }

    if (window.notificationBus !== undefined) {
        try {
            window.notificationBus.emit("notify", message);
        }
        catch (error) {
            debug("Notification bus not registered", error);
        }
    }
}

export function notify(message: unknown, severityLevel?: SeverityLevel): void {
    if (!message) {
        notifyMessage({
            summary: "Error",
            detail: "Unknown error occured",
            severity: SeverityLevel.Error,
        });

        return;
    }

    const notificationMessage = getNotificationMessageFromObject(message, severityLevel);
    if (notificationMessage) {
        notifyMessage(notificationMessage);
    }
}

export function notifiersClose() {
    const notifiers = document.querySelectorAll<HTMLElement>(".p-message[role='alert']");

    if (notifiers.length <= 0) {
        return;
    }

    notifiers.forEach((notifier) => {
        const btnClose = notifier.querySelector<HTMLElement>(":scope > .p-message-wrapper > .p-message-close");

        if (btnClose) {
            btnClose.addEventListener("click", () => {
                notifier.classList.add("hidden");
            });
        }

        setTimeout(() => {
            notifier.style.transition = "0.8s";
            notifier.style.opacity = "0";
        }, 5000);

        setTimeout(() => {
            notifier.style.display = "none";
        }, 6000);
    });
}

function getNotificationMessageFromObject(message: unknown, severityLevel?: SeverityLevel): NotificationMessage | undefined {
    if (isNotificationMessage(message)) {
        return message;
    }

    if (isValidationProblemDetailsLike(message)) {
        const detail = message.errors && Object.values(message.errors).length > 0
            ? Object.values(message.errors).flat().join("\r\n")
            : (message.detail ?? "");

        return {
            summary: message.title ?? "",
            detail,
            severity: severityLevel ?? SeverityLevel.Error,
        };
    }

    if (isProblemDetailsLike(message)) {
        return {
            summary: message.title ?? "",
            detail: message.detail ?? "",
            severity: severityLevel ?? SeverityLevel.Error,
        };
    }

    if (message instanceof Error) {
        return {
            summary: "Server Error",
            detail: message.message,
            severity: severityLevel ?? SeverityLevel.Error,
        };
    }

    return undefined;
}

function isNotificationMessage(message: unknown): message is NotificationMessage {
    return message instanceof NotificationMessage;
}

function isProblemDetailsLike(message: unknown): message is ProblemDetailsLike {
    return !!message && typeof message === "object" && ("title" in message || "detail" in message);
}

function isValidationProblemDetailsLike(message: unknown): message is ValidationProblemDetailsLike {
    return isProblemDetailsLike(message) && "errors" in (message as ValidationProblemDetailsLike);
}
