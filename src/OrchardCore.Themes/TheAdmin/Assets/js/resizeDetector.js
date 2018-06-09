// System to detect when the window's size reaches a given breakpoint
// Right now it is only used to compact the lefbar when resizing under 768px
// In the future maybe this is useful to do other things on resizing.

$(function () {
    lastWidth = $(this).width(); //this = window
    var breakPoint = 768;
    lastDirection = "";
    var lastDirectionManaged = "";
    BreakpointChangeManaged = false;


    $(window).on('resize', function () {

        var width = $(this).width();
        var breakPoint = 768;
        var direction = width < lastWidth ? 'reducing' : 'increasing';


        if (direction !== lastDirection) {
            BreakpointChangeManaged = false; // need to listen for breakpoint            
        }

        if ((BreakpointChangeManaged == false) && (direction != lastDirectionManaged)) {
            if ((direction == "reducing") && (width < breakPoint)) {
                // breakpoint reached while going down
                setCompactStatus();
                lastDirectionManaged = direction;
                BreakpointChangeManaged = true;
            }


            if ((direction == "increasing") && (width > breakPoint)) {
                // breakpoint reached while going up
                if (isCompactExplicit == false) {
                    unSetCompactStatus();
                }
                lastDirectionManaged = direction;
                BreakpointChangeManaged = true;
            }
        }

        lastDirection = direction;
        lastWidth = width;
    });

});