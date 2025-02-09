/// <reference path="../bower_components/jquery/dist/jquery.js" />
/*
jquery-resizable
Version 0.35 - 11/18/2019
© 2015-2019 Rick Strahl, West Wind Technologies
www.west-wind.com
Licensed under MIT License
*/
(function(factory, undefined) {
	if (typeof define === 'function' && define.amd) {
		// AMD
		define(['jquery'], factory);
	} else if (typeof module === 'object' && typeof module.exports === 'object') {
		// CommonJS
		module.exports = factory(require('jquery'));
	} else {
		// Global jQuery
		factory(jQuery);
	}
}(function($, undefined) {
    
    if ($.fn.resizableSafe)
        return;

    $.fn.resizableSafe = function fnResizable(options) {
        var defaultOptions = {
            // selector for handle that starts dragging
            handleSelector: null,
            // resize the width
            resizeWidth: true,
            // resize the height
            resizeHeight: true,
            // the side that the width resizing is relative to
            resizeWidthFrom: 'right',
            // the side that the height resizing is relative to
            resizeHeightFrom: 'bottom',
            // hook into start drag operation (event passed)
            onDragStart: null,
            // hook into stop drag operation (event passed)
            onDragEnd: null,
            // hook into each drag operation (event passed)
            onDrag: null,
            // disable touch-action on $handle
            // prevents browser level actions like forward back gestures
            touchActionNone: true,
            // instance id
            instanceId: null
    };
        if (typeof options == "object")
            defaultOptions = $.extend(defaultOptions, options);

        return this.each(function () {
            var opt = $.extend({}, defaultOptions);
            if (!opt.instanceId)
                opt.instanceId = "rsz_" + new Date().getTime();            

            var startPos, startTransition;

            // get the element to resize 
            var $el = $(this);
            var $handle;

            if (options === 'destroy') {            
                opt = $el.data('resizable');
                if (!opt)
                    return;

                $handle = getHandle(opt.handleSelector, $el);
                $handle.off("mousedown." + opt.instanceId + " touchstart." + opt.instanceId);
                if (opt.touchActionNone)
                    $handle.css("touch-action", "");
                $el.removeClass("resizable");
                return;
            }
          
            $el.data('resizable', opt);

            // get the drag handle

            $handle = getHandle(opt.handleSelector, $el);

            if (opt.touchActionNone)
                $handle.css("touch-action", "none");

            $el.addClass("resizable");
            $handle.on("mousedown." + opt.instanceId + " touchstart." + opt.instanceId, startDragging);

            function noop(e) {
                e.stopPropagation();
                e.preventDefault();
            };

            function startDragging(e) {
                // Prevent dragging a ghost image in HTML5 / Firefox and maybe others    
                if ( e.preventDefault ) {
                  e.preventDefault();
                }
                
                startPos = getMousePos(e);
                startPos.width = parseInt($el.width(), 10);
                startPos.height = parseInt($el.height(), 10);

                startTransition = $el.css("transition");
                $el.css("transition", "none");

                if (opt.onDragStart) {
                    if (opt.onDragStart(e, $el, opt) === false)
                        return;
                }
                
                $(document).on('mousemove.' + opt.instanceId, doDrag);
                $(document).on('mouseup.' + opt.instanceId, stopDragging);
                if (window.Touch || navigator.maxTouchPoints) {
                    $(document).on('touchmove.' + opt.instanceId, doDrag);
                    $(document).on('touchend.' + opt.instanceId, stopDragging);
                }
                $(document).on('selectstart.' + opt.instanceId, noop); // disable selection
                $("iframe").css("pointer-events","none");
            }

            function doDrag(e) {
                
                var pos = getMousePos(e), newWidth, newHeight;

                if (opt.resizeWidthFrom === 'left')
                    newWidth = startPos.width - pos.x + startPos.x;
                else
                    newWidth = startPos.width + pos.x - startPos.x;

                if (opt.resizeHeightFrom === 'top')
                    newHeight = startPos.height - pos.y + startPos.y;
                else
                    newHeight = startPos.height + pos.y - startPos.y;

                if (!opt.onDrag || opt.onDrag(e, $el, newWidth, newHeight, opt) !== false) {
                    if (opt.resizeHeight)
                        $el.height(newHeight);                    

                    if (opt.resizeWidth)
                        $el.width(newWidth);                    
                }
            }

            function stopDragging(e) {
                e.stopPropagation();
                e.preventDefault();

                $(document).off('mousemove.' + opt.instanceId);
                $(document).off('mouseup.' + opt.instanceId);

                if (window.Touch || navigator.maxTouchPoints) {
                    $(document).off('touchmove.' + opt.instanceId);
                    $(document).off('touchend.' + opt.instanceId);
                }
                $(document).off('selectstart.' + opt.instanceId, noop);                

                // reset changed values
                $el.css("transition", startTransition);
                $("iframe").css("pointer-events","auto");

                if (opt.onDragEnd)
                    opt.onDragEnd(e, $el, opt);

                return false;
            }

            function getMousePos(e) {
                var pos = { x: 0, y: 0, width: 0, height: 0 };
                if (typeof e.clientX === "number") {
                    pos.x = e.clientX;
                    pos.y = e.clientY;
                } else if (e.originalEvent.touches) {
                    pos.x = e.originalEvent.touches[0].clientX;
                    pos.y = e.originalEvent.touches[0].clientY;
                } else
                    return null;

                return pos;
            }

            function getHandle(selector, $el) {
                if (selector && selector.trim()[0] === ">") {
                    selector = selector.trim().replace(/^>\s*/, "");
                    return $el.find(selector);
                }

                // Search for the selector, but only in the parent element to limit the scope
                // This works for multiple objects on a page (using .class syntax most likely)
                // as long as each has a separate parent container. 
                return selector ? $el.parent().find(selector) : $el;
            } 
        });
    };

    if (!$.fn.resizable)
        $.fn.resizable = $.fn.resizableSafe;
}));
