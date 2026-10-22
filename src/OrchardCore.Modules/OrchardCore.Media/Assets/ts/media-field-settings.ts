const buttons = document.querySelectorAll<HTMLInputElement>(".media-extensions-selector");
const mediaExtensionCheckboxes = document.querySelectorAll<HTMLInputElement>(".media-extension");

buttons.forEach((button) => {
    button.addEventListener("click", () => {
        const contentTypes = button.value.split(";");

        mediaExtensionCheckboxes.forEach((checkbox) => {
            if (contentTypes.includes(checkbox.dataset.mediaContentType ?? "")) {
                checkbox.checked = button.checked;
            }
        });
    });
});
