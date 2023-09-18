const darkThemeName = 'dark';
const lightThemeName = 'light';
const getStoredTheme = () => localStorage.getItem('theme');
const setStoredTheme = theme => localStorage.setItem('theme', theme);
const getPreferredTheme = () => {
    const storedTheme = getStoredTheme()
    if (storedTheme) {
        return storedTheme;
    }

    return window.matchMedia('(prefers-color-scheme: dark)').matches ? darkThemeName : lightThemeName;
}
const setTheme = theme => {
    if (theme === 'auto' && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        document.documentElement.setAttribute('data-bs-theme', darkThemeName);
    } else {
        document.documentElement.setAttribute('data-bs-theme', theme);
    }
}

// We add some classes to the body tag to restore the sidebar to the state is was before reload.
// That state was saved to localstorage by userPreferencesPersistor.js
// We need to apply the classes BEFORE the page is rendered. 
// That is why we use a MutationObserver instead of document.Ready().
const themeObserver = new MutationObserver(function (mutations) {
    //const html = document.documentElement || document.body;
    //const tenant = html.getAttribute('data-tenant');

    for (let i = 0; i < mutations.length; i++) {
        for (let j = 0; j < mutations[i].addedNodes.length; j++) {

            if (mutations[i].addedNodes[j].tagName == 'BODY') {
                var body = mutations[i].addedNodes[j];

                body.classList.add('no-admin-preferences');

                let preferredTheme = getPreferredTheme();
                setTheme(preferredTheme);

                // we're done: 
                themeObserver.disconnect();
            };
        }
    }
});

themeObserver.observe(document.documentElement, {
    childList: true,
    subtree: true
});
