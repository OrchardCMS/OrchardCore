const togglePassword = document.querySelector<HTMLButtonElement>("#togglePassword");
const password = document.getElementById("Password") as HTMLInputElement | null;

togglePassword?.addEventListener("click", function (this: HTMLButtonElement) {
    if (!password) {
        return;
    }

    const type = password.getAttribute("type") === "password" ? "text" : "password";
    password.setAttribute("type", type);

    const icon = this.getElementsByClassName("icon")[0];
    if (icon.getAttribute("data-icon")) {
        icon.setAttribute("data-icon", type === "password" ? "eye" : "eye-slash");
    } else {
        if (type === "password") {
            icon.classList.remove("fa-eye-slash");
        } else {
            icon.classList.remove("fa-eye");
        }
        if (type === "password") {
            icon.classList.add("fa-eye");
        } else {
            icon.classList.add("fa-eye-slash");
        }
    }
});

export {};
