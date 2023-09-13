/*!
 * Color mode toggler for Bootstrap's docs (https://getbootstrap.com/)
 * Copyright 2011-2023 The Bootstrap Authors
 * Licensed under the Creative Commons Attribution 3.0 Unported License.
 */

(() => {
    'use strict'

    const getStoredTheme = () => localStorage.getItem('theme')
    const setStoredTheme = theme => localStorage.setItem('theme', theme)
    const setPreferred = (selector, theme) => {
        let iconName = '';
        let iconTitle = '';
        if (theme == 'dark') {
            iconName = selector.getAttribute('data-theme-icon-light');
            iconTitle = selector.getAttribute('data-theme-name-light');
        } else {
            iconName = selector.getAttribute('data-theme-icon-dark');
            iconTitle = selector.getAttribute('data-theme-name-dark');
        }

        selector.innerHTML = `<i class="${iconName}" aria-hidden="true"></i>`;
        selector.setAttribute('title', iconTitle);
        selector.setAttribute('data-bs-original-title', iconTitle);
        selector.setAttribute('aria-label', iconTitle);

        setStoredTheme(theme);
        setTheme(theme);
        persistAdminPreferences(theme);
    }

    const getPreferredTheme = () => {
        const storedTheme = getStoredTheme()
        if (storedTheme) {
            return storedTheme
        }

        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
    }

    const setTheme = theme => {
        if (theme === 'auto' && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            document.documentElement.setAttribute('data-bs-theme', 'dark')
        } else {
            document.documentElement.setAttribute('data-bs-theme', theme)
        }
    }

    const themeSwitcher = document.getElementById('btn-darkmode')

    setPreferred(themeSwitcher, getPreferredTheme())

    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
        const storedTheme = getStoredTheme()
        if (storedTheme !== 'light' && storedTheme !== 'dark') {
            setTheme(getPreferredTheme())
        }
    });

    if (themeSwitcher) {
        themeSwitcher.addEventListener('click', () => {

            let currentTheme = getStoredTheme();
            if (currentTheme == 'dark') {
                setPreferred(themeSwitcher, 'light');
            } else {
                setPreferred(themeSwitcher, 'dark');
            }
        });
    }
})();
