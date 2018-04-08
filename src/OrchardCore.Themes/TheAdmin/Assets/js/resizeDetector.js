// System to detect when the window's size reaches a given breakpoint
// Right now it is only used to remove a class when resizing under 768px
// It's the best way a found to fix an issue that ocurring when compact sidebar.
// In the future maybe this is required to do other things on resizing.

$(function () {    
    lastWidth = $(this).width(); //this = window
    lastDirection = "";
    var lastDirectionManaged = "";
    BreakpointChangeManaged = false;
        

    $(window).on('resize', function () {

        var width = $(this).width();
        var breakPoint = 768;
        var direction = "";        

    
        if (width < lastWidth) {            
            direction = "reducing";
        } else {            
            direction = "increasing";
        }

        if (direction != lastDirection)  {
            BreakpointChangeManaged = false; // need to listen for breakpoint            
        }

        if ((BreakpointChangeManaged == false) && ( direction != lastDirectionManaged)) {
                        
            if ((direction == "reducing") && (width < breakPoint)) { 
                // breakpoint passed down
                $('body').removeClass('leftbar-visible-on-small');
                lastDirectionManaged = direction;
                BreakpointChangeManaged = true;
            }

            // increasing case is not needed now.
            //if ((direction == "increasing") && (width > breakPoint)) {
            //    // breakpoint passed up 
            //    lastDirectionManaged = direction;
            //    BreakpointChangeManaged = true;
            //}
        }

        lastDirection = direction;
        lastWidth = width;
    });    
});