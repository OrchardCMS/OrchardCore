import { reactive } from "vue";

export type ToastVariant = "info" | "success" | "warning" | "danger";

export interface ToastAction {
    label: string;
    run(): void;
}

export interface Toast {
    id: number;
    message: string;
    variant: ToastVariant;
    action?: ToastAction;
}

export interface ToastOptions {
    message: string;
    variant?: ToastVariant;
    action?: ToastAction;
    /**
     * How long the toast stays, in milliseconds; 0 keeps it until it is dismissed.
     */
    timeout?: number;
}

// The designer's own toasts (the admin theme has no global toast API). ToastHost.vue renders them in an
// aria-live region.
const toasts = reactive<Toast[]>([]);
let nextId = 1;

export const dismissToast = (id: number) => {
    const index = toasts.findIndex((toast) => toast.id === id);

    if (index >= 0) {
        toasts.splice(index, 1);
    }
};

export const showToast = (options: ToastOptions) => {
    const id = nextId++;

    toasts.push({ id, message: options.message, variant: options.variant ?? "info", action: options.action });

    const timeout = options.timeout ?? (options.action ? 8000 : 5000);

    if (timeout > 0) {
        setTimeout(() => dismissToast(id), timeout);
    }

    return id;
};

export const useToasts = () => toasts;

export const clearToasts = () => {
    toasts.splice(0, toasts.length);
};
