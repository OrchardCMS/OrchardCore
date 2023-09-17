/*!
 * Color mode toggler for Bootstrap's docs (https://getbootstrap.com/)
 * Copyright 2011-2023 The Bootstrap Authors
 * Licensed under the Creative Commons Attribution 3.0 Unported License.
 */

const darkThemeName = 'dark';
const defaultThemeName = 'light';

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
    const html = document.documentElement || document.body;
    const tenant = html.getAttribute('data-tenant');
    const key = tenant + '-adminPreferences';
    let adminPreferences = JSON.parse(localStorage.getItem(key));

    for (let i = 0; i < mutations.length; i++) {
        for (let j = 0; j < mutations[i].addedNodes.length; j++) {

            if (mutations[i].addedNodes[j].tagName == 'BODY') {
                var body = mutations[i].addedNodes[j];

                if (adminPreferences != null && adminPreferences.darkMode) {
                    setTheme(darkThemeName);
                    console.log('showing the dark theme');
                }
                else {
                    body.classList.add('no-admin-preferences');

                    let preferredTheme = getPreferredTheme();
                    setTheme(preferredTheme);
                    console.log('showing the prefered theme: ' + preferredTheme);
                }

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

(() => {
    'use strict'

    const getStoredTheme = () => localStorage.getItem('theme');
    const setStoredTheme = theme => localStorage.setItem('theme', theme);

    const getPreferredTheme = () => {
        const storedTheme = getStoredTheme()
        if (storedTheme) {
            return storedTheme;
        }

        return window.matchMedia('(prefers-color-scheme: dark)').matches ? darkThemeName : defaultThemeName;
    }

    //setTheme(getPreferredTheme())

    const showActiveTheme = (theme, focus = false) => {
        const themeSwitcher = document.querySelector('#bd-theme');

        if (!themeSwitcher) {
            return;
        }

        const themeSwitcherText = document.querySelector('#bd-theme-text');
        const activeThemeIcon = document.querySelector('.theme-icon-active');
        const btnToActive = document.querySelector(`[data-bs-theme-value="${theme}"]`);
        const svgOfActiveBtn = btnToActive.querySelector('.theme-icon');

        btnToActive.classList.add('active');
        btnToActive.setAttribute('aria-pressed', 'true');

        activeThemeIcon.innerHTML = svgOfActiveBtn.innerHTML;

        const themeSwitcherLabel = `${themeSwitcherText.textContent} (${btnToActive.dataset.bsThemeValue})`;
        themeSwitcher.setAttribute('aria-label', themeSwitcherLabel);

        const btnsToInactive = document.querySelectorAll(`[data-bs-theme-value]:not([data-bs-theme-value="${theme}"])`);

        for (let i = 0; i < btnsToInactive.length; i++) {
            btnsToInactive[i].classList.remove('active');
            btnsToInactive[i].setAttribute('aria-pressed', 'false');
        }

        if (focus) {
            themeSwitcher.focus();
        }
    }

    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
        const storedTheme = getStoredTheme()
        if (storedTheme !== defaultThemeName && storedTheme !== darkThemeName) {
            setTheme(getPreferredTheme());
        }
    });

    window.addEventListener('DOMContentLoaded', () => {
        console.log('page loaded');

        showActiveTheme(getPreferredTheme());

        document.querySelectorAll('[data-bs-theme-value]')
            .forEach(toggle => {
                toggle.addEventListener('click', () => {
                    const theme = toggle.getAttribute('data-bs-theme-value');
                    setStoredTheme(theme);
                    setTheme(theme);
                    showActiveTheme(theme, true);
                    persistAdminPreferences(theme);
                })
            })
    });
})();
