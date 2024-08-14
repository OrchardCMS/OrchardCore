document.addEventListener('DOMContentLoaded', function () {
    const setTheme = () => {
        var isDark = false;

        if (typeof getPreferredTheme === 'function') {
            isDark = getPreferredTheme() == (darkThemeName || 'dark');
        } else {
            isDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        }

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

