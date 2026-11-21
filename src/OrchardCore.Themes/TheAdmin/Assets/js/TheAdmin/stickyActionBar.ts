// Marks the sticky action bar at the top of admin pages (.action-bar) with .action-bar-stuck while it is pinned under
// the top bar, so that the theme can let its background span the whole content area. The bar is either the card
// itself or an element around it, e.g. a search form.
//
// This file must remain in the foot bundle (TheAdmin.ts) since it queries the live DOM.

(function () {
    let scheduled = false;

    const update = () => {
        scheduled = false;

        document.querySelectorAll<HTMLElement>('.action-bar').forEach(bar => {
            const style = getComputedStyle(bar);

            if (style.position !== 'sticky') {
                bar.classList.remove('action-bar-stuck');
                return;
            }

            // The bar is pinned when it sits at its sticky offset, which it can only reach once the page scrolled.
            const stickyTop = parseFloat(style.top) || 0;
            const stuck = window.scrollY > 0 && bar.getBoundingClientRect().top <= stickyTop + 1;

            bar.classList.toggle('action-bar-stuck', stuck);
        });
    };

    const schedule = () => {
        if (!scheduled) {
            scheduled = true;
            requestAnimationFrame(update);
        }
    };

    window.addEventListener('scroll', schedule, { passive: true });
    window.addEventListener('resize', schedule, { passive: true });
    update();
})();
