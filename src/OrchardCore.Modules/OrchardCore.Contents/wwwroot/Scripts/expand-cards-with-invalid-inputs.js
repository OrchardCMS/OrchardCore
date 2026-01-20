(function () {
    function expandCardsWithInvalidInputs(event) {
        const elements = event.target.form.querySelectorAll(`input`);
        Array.from(elements).filter(element => !element.checkValidity()).forEach(current => {
            current = current.parentElement;
            while (current) {
                if (current.matches('.widget.widget-editor.card.collapsed')) {
                    current.classList.remove('collapsed');
                }

                current = current.parentElement;
            }
        });
    }
    document.addEventListener("DOMContentLoaded", () => {
        document.querySelectorAll('button[name="submit.Publish"],button[name="submit.Save"]').forEach(button => {
            button.addEventListener('click', (event) => expandCardsWithInvalidInputs(event));
        });
    });
})();
