// Keeps --oc-action-bar-mobile-height in sync with the actual rendered height
// of the fixed mobile action bar so that .action-bar-mobile-content always has
// enough bottom padding to prevent content from being covered.
(function () {
    const mobileBar = document.querySelector<HTMLElement>('.action-bar-mobile');
    if (!mobileBar) return;

    new ResizeObserver(() => {
        document.documentElement.style.setProperty('--oc-action-bar-mobile-height', mobileBar.offsetHeight + 'px');
    }).observe(mobileBar);
})();
