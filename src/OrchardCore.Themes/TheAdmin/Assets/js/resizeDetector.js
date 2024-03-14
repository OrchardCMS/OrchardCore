// System to detect when the window's size reaches a given breakpoint
// Right now it is only used to compact the lefbar when resizing under 768px
// In the future maybe this is useful to do other things on resizing.

(function () {
    const breakPoint = 768;
    let lastDirection = '';
    let lastDirectionManaged = '';
    let breakpointChangeManaged = false;
    let lastWidth = document.body.clientWidth;

    window.addEventListener('resize', () => {

        const width = document.body.clientWidth;
        const direction = width < lastWidth ? 'reducing' : 'increasing';

        if (direction !== lastDirection) {
            breakpointChangeManaged = false; // need to listen for breakpoint            
        }

        if (breakpointChangeManaged == false && direction != lastDirectionManaged) {
            if (direction == 'reducing' && width < breakPoint) {
                // breakpoint reached while going down
                setCompactStatus();
                lastDirectionManaged = direction;
                breakpointChangeManaged = true;
            }

            if (direction == 'increasing' && width > breakPoint) {
                // breakpoint reached while going up
                if (isCompactExplicit == false) {
                    unSetCompactStatus();
                }
                lastDirectionManaged = direction;
                breakpointChangeManaged = true;
            }
        }

        lastDirection = direction;
        lastWidth = width;
    });
})();
