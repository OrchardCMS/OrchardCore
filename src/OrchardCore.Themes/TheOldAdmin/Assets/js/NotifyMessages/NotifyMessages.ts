if (bootstrap?.Toast) {
    document.querySelectorAll<HTMLElement>(".oc-notify-toast-container .toast").forEach((toastElement) => {
        const toast = bootstrap.Toast.getOrCreateInstance(toastElement);
        toast.show();
        toastElement.addEventListener("hidden.bs.toast", () => toastElement.remove(), { once: true });
    });
}
