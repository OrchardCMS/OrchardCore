import { setSidebarOpen, syncCompactState } from './menu';

// Below 768px the menu is an off canvas drawer, above it is a sidebar that can be
// compacted. Anything left open by one layout has to be closed when switching to the other.
(function () {
    const mobile = window.matchMedia('(max-width: 767.98px)');

    const onBreakpointChange = () => {
        setSidebarOpen(false);
        syncCompactState();
    };

    if (typeof mobile.addEventListener === 'function') {
        mobile.addEventListener('change', onBreakpointChange);
    } else {
        // Safari before 14 only supports the deprecated API.
        (mobile as MediaQueryList).addListener(onBreakpointChange);
    }
})();
