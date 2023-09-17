/*!
 * Color mode toggler for Bootstrap's docs (https://getbootstrap.com/)
 * Copyright 2011-2023 The Bootstrap Authors
 * Licensed under the Creative Commons Attribution 3.0 Unported License.
 */

(() => {
    'use strict'

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
        if (storedTheme !== lightThemeName && storedTheme !== darkThemeName) {
            setTheme(getPreferredTheme());
        }
    });

    window.addEventListener('DOMContentLoaded', () => {
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
