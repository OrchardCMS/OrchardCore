document.addEventListener('DOMContentLoaded', function () {
    function setTheme() {
        var isDark = getPreferredTheme() == darkThemeName;

        document
            .querySelectorAll('.trumbowyg-wrapper')
            .forEach((element) =>
                element.classList.toggle('trumbowyg-dark', isDark)
            );
    }

    const mutationObserver = new MutationObserver(setTheme);
    mutationObserver.observe(document.documentElement, { attributes: true });

    setTheme();
});

