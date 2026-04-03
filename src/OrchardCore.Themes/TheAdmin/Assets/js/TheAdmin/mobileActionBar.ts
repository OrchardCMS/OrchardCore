// Keeps --oc-action-bar-height in sync with the actual rendered height
// of the fixed mobile action bar so that the edit container always has
// enough bottom padding to prevent content from being covered.
(function () {
    const actionBar = document.querySelector<HTMLElement>('.action-bar');
    if (!actionBar) return;

    new ResizeObserver(() => {
        document.documentElement.style.setProperty('--oc-action-bar-height', actionBar.offsetHeight + 'px');
    }).observe(actionBar);
})();
