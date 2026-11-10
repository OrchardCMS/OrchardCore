document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.disabledContent__wrapper input').forEach((el) => el.disabled = true);
    document.querySelectorAll('.disabledContent__wrapper textarea').forEach((el) => el.disabled = true);
    document.querySelectorAll('.disabledContent__wrapper button').forEach((el) => el.disabled = true);
    document.querySelectorAll('.disabledContent__wrapper .widget-editor .widget-editor-header .widget-editor-btn-toggle').forEach((el) => el.disabled = false);

    setTimeout(() => {
        document.querySelectorAll('.disabledContent__wrapper textarea').forEach((el) => el.disabled = true);
    });
});
