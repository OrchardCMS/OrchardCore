// Manages the sticky action bars at the top of admin pages (.action-bar with position: sticky).
//
// - Stacks them: a page can render several (a module's search bar above the bar of a list, say), and each one sticks
//   under those before it rather than on top of them. A bar is offset by the height of every earlier bar that sticks
//   for as long as it does, i.e. whose sticky container (its parent) holds it. The offset is set as the
//   --ta-action-bar-offset custom property, which the theme adds to the bar's 'top'.
// - Marks a bar with .action-bar-stuck while it is pinned, so that the theme can let its background span the whole
//   content area. The bar is either the card itself or an element around it, e.g. a search form. A pinned bar with
//   another pinned under it is also marked with .action-bar-stacked, so that only the lowest one casts a shadow.
//
// This file must remain in the foot bundle (TheAdmin.ts) since it queries the live DOM.

(function () {
    let scheduled = false;

    const stickyBars = () =>
        Array.from(document.querySelectorAll<HTMLElement>('.action-bar')).filter(bar => {
            if (getComputedStyle(bar).position === 'sticky') {
                return true;
            }

            bar.classList.remove('action-bar-stuck');
            bar.style.removeProperty('--ta-action-bar-offset');

            return false;
        });

    // Sets --ta-action-bar-bleed-start and --ta-action-bar-bleed-end, the distances from the inner edges of the bar's
    // parent to the edges of the content area. They are measured from the parent, not the bar, as the pinned bar's
    // negative margins move the bar itself.
    const setBleed = (bar: HTMLElement) => {
        const parent = bar.parentElement;
        const root = bar.closest<HTMLElement>('.ta-content');

        if (!parent || !root) {
            return;
        }

        const parentRect = parent.getBoundingClientRect();
        const rootRect = root.getBoundingClientRect();
        const style = getComputedStyle(parent);
        const left = Math.max(0, parentRect.left + (parseFloat(style.borderLeftWidth) || 0) + (parseFloat(style.paddingLeft) || 0) - rootRect.left);
        const right = Math.max(0, rootRect.right - (parentRect.right - (parseFloat(style.borderRightWidth) || 0) - (parseFloat(style.paddingRight) || 0)));
        const rtl = getComputedStyle(bar).direction === 'rtl';

        bar.style.setProperty('--ta-action-bar-bleed-start', (rtl ? right : left) + 'px');
        bar.style.setProperty('--ta-action-bar-bleed-end', (rtl ? left : right) + 'px');
    };

    const update = () => {
        scheduled = false;

        const bars = stickyBars();

        // The offsets first, from the heights of the bars before each one.
        bars.forEach((bar, index) => {
            let offset = 0;

            for (const previous of bars.slice(0, index)) {
                if (previous.parentElement?.contains(bar)) {
                    offset += previous.offsetHeight;
                }
            }

            if (offset > 0) {
                bar.style.setProperty('--ta-action-bar-offset', offset + 'px');
            } else {
                bar.style.removeProperty('--ta-action-bar-offset');
            }
        });

        // How far each bar is inset from the edges of the content area, so that a pinned bar can span all of it. This is
        // the content padding for a bar placed directly in the page, and more for one nested in a card or a tab pane.
        bars.forEach(setBleed);

        // Then the pinned state, against the offset each bar sticks at.
        const stuckBars = bars.filter(bar => {
            const stickyTop = parseFloat(getComputedStyle(bar).top) || 0;
            const stuck = window.scrollY > 0 && bar.getBoundingClientRect().top <= stickyTop + 1;

            bar.classList.toggle('action-bar-stuck', stuck);

            return stuck;
        });

        // A pinned bar with another one pinned under it is marked, so that only the lowest bar of a stack casts the shadow.
        stuckBars.forEach((bar, index) => {
            const covered = stuckBars.slice(index + 1).some(next => bar.parentElement?.contains(next));

            bar.classList.toggle('action-bar-stacked', covered);
        });

        bars.filter(bar => !stuckBars.includes(bar)).forEach(bar => bar.classList.remove('action-bar-stacked'));
    };

    const schedule = () => {
        if (!scheduled) {
            scheduled = true;
            requestAnimationFrame(update);
        }
    };

    window.addEventListener('scroll', schedule, { passive: true });
    window.addEventListener('resize', schedule, { passive: true });

    // A bar that changes height (its buttons wrap, say) moves the ones under it.
    const resizeObserver = new ResizeObserver(schedule);
    const observeBars = (root: ParentNode) => {
        if (root instanceof HTMLElement && root.matches('.action-bar')) {
            resizeObserver.observe(root);
        }

        root.querySelectorAll<HTMLElement>('.action-bar').forEach(bar => resizeObserver.observe(bar));
    };

    observeBars(document);

    // A bar rendered after the page loads (by a Vue app, say) is stacked and watched too.
    new MutationObserver(mutations => {
        let added = false;

        for (const mutation of mutations) {
            mutation.addedNodes.forEach(node => {
                if (node instanceof HTMLElement && (node.matches('.action-bar') || node.querySelector('.action-bar'))) {
                    observeBars(node);
                    added = true;
                }
            });
        }

        if (added) {
            schedule();
        }
    }).observe(document.body, { childList: true, subtree: true });

    update();
})();
