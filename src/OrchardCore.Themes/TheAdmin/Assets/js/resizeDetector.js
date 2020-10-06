// System to detect when the window's size reaches a given breakpoint
// Right now it is only used to compact the lefbar when resizing under 768px
// In the future maybe this is useful to do other things on resizing.

$(function () {
    lastWidth = $(this).width(); //this = window
    var lastHeight = $(this).height(); //this = window
    leftNav = $('#left-nav');
    topBar = $('.ta-navbar-top');
    var widthBreakPoint = 768;
    var heightBreakPoint = leftNav.height();
    lastWidthDirection = "";
    lastHeightDirection = "";
    var lastWidthDirectionManaged = "";
    var lastHeightDirectionManaged = "";
    BreakpointWidthChangeManaged = false;
    BreakpointHeightChangeManaged = false;

    $(window).on('resize', function () {

        var width = $(this).width();
        var height = $(this).height();
        var widthBreakPoint = 768;
        var heightBreakPoint = leftNav.height() + topBar.height();
        var widthDirection = width < lastWidth ? 'reducing' : 'increasing';
        var heightDirection = height < lastHeight ? 'reducing' : 'increasing';

        if (widthDirection !== lastWidthDirection) {
            BreakpointWidthChangeManaged = false; // need to listen for breakpoint            
        }

        if (heightDirection !== lastHeightDirection) {
            BreakpointHeightChangeManaged = false; // need to listen for breakpoint            
        }

        if ((BreakpointWidthChangeManaged == false) && (widthDirection != lastWidthDirectionManaged)) {
            if ((widthDirection == "reducing") && (width < widthBreakPoint)) {
                // breakpoint reached while going down
                setCompactStatus();
                lastWidthDirectionManaged = widthDirection;
                BreakpointWidthChangeManaged = true;
            }

            if ((widthDirection == "increasing") && (width > widthBreakPoint)) {
                // breakpoint reached while going up
                if (isCompactExplicit == false) {
                    unSetCompactStatus();
                }
                lastWidthDirectionManaged = widthDirection;
                BreakpointWidthChangeManaged = true;
            }
        }

        if ((BreakpointHeightChangeManaged == false) && (heightDirection != lastHeightDirectionManaged)) {
            if ((heightDirection == "reducing") && (height < heightBreakPoint)) {
                // breakpoint reached while going down
                unSetCompactStatus();
                lastHeightDirectionManaged = heightDirection;
                BreakpointHeightChangeManaged = true;
            }

            if ((heightDirection == "increasing") && (height > heightBreakPoint)) {
                // breakpoint reached while going up
                if (isCompactExplicit == false) {
                    setCompactStatus();
                }
                lastHeightDirectionManaged = heightDirection;
                BreakpointHeightChangeManaged = true;
            }
        }

        lastWidthDirection = widthDirection;
        lastWidth = width;
        lastHeightDirection = heightDirection;
        lastHeight = height;
    });

});