// Keeps --oc-action-bar-height in sync with the actual rendered height
// of the fixed mobile action bar so that the edit container always has
// enough bottom padding to prevent content from being covered.
// When .action-bar-actions is present, also injects a toggle button so that
// secondary actions are collapsed on mobile and expanded on demand.
(function () {
    const actionBar = document.querySelector<HTMLElement>('.action-bar');
    if (!actionBar) return;

    new ResizeObserver(() => {
        document.documentElement.style.setProperty('--oc-action-bar-height', actionBar.offsetHeight + 'px');
    }).observe(actionBar);

    const actions = actionBar.querySelector<HTMLElement>('.action-bar-actions');
    if (!actions) return;

    // Only add the collapse feature when there is more than one visible action.
    const actionCount = actions.querySelectorAll(':scope > .btn, :scope > .btn-group').length;
    if (actionCount <= 1) return;

    const toggle = document.createElement('button');
    toggle.type = 'button';
    toggle.className = 'btn btn-sm btn-outline-secondary action-bar-toggle d-md-none';
    toggle.setAttribute('aria-expanded', 'false');
    toggle.setAttribute('aria-label', 'Toggle actions');
    actions.parentElement!.insertBefore(toggle, actions);

    // Opt the action bar in to the CSS collapse rules.
    actionBar.classList.add('action-bar-js');

    toggle.addEventListener('click', () => {
        const expanded = actionBar.classList.toggle('action-bar-expanded');
        toggle.setAttribute('aria-expanded', String(expanded));
    });
})();
