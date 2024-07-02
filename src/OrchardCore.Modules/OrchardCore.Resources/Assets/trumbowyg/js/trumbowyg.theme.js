document.addEventListener('DOMContentLoaded', function () {
    function setTheme() {
        var isDark = getPreferredTheme() == darkThemeName;

        document
            .querySelectorAll('.trumbowyg')
            .forEach((element) =>
                element.parentElement.classList.toggle('trumbowyg-dark', isDark)
            );
    }

    const mutationObserver = new MutationObserver(setTheme);
    mutationObserver.observe(document.documentElement, {
        attributes: true,
        childList: true,
        subtree: true,
    });

    setTheme();
});

