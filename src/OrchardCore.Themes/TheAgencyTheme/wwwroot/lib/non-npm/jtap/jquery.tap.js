(function($, specialEventName) {
  'use strict';

  /**
   * Native event names for creating custom one.
   *
   * @type {Object}
   */
  var nativeEvent = Object.create(null);
  /**
   * Get current time.
   *
   * @return {Number}
   */
  var getTime = function() {
    return new Date().getTime();
  };

  nativeEvent.original = 'click';

  if ('ontouchstart' in document.documentElement) {
    nativeEvent.start = 'touchstart';
    nativeEvent.end = 'touchend';
  } else {
    nativeEvent.start = 'mousedown';
    nativeEvent.end = 'mouseup';
  }

  $.event.special[specialEventName] = {
    setup: function(data, namespaces, eventHandle) {
      var $element = $(this);
      var eventData = {};

      $element
        // Remove all handlers that were set for an original event.
        .off(nativeEvent.original)
        // Prevent default actions.
        .on(nativeEvent.original, false)
        // Split original event by two different and collect an information
        // on every phase.
        .on(nativeEvent.start + ' ' + nativeEvent.end, function(event) {
          // Handle the event system of touchscreen devices.
          eventData.event = event.originalEvent.changedTouches ? event.originalEvent.changedTouches[0] : event;
        })
        .on(nativeEvent.start, function(event) {
          // Stop execution if an event is simulated.
          if (event.which && event.which !== 1) {
            return;
          }

          eventData.target = event.target;
          eventData.pageX = eventData.event.pageX;
          eventData.pageY = eventData.event.pageY;
          eventData.time = getTime();
        })
        .on(nativeEvent.end, function(event) {
          // Compare properties from two phases.
          if (
            // The target should be the same.
            eventData.target === event.target &&
            // Time between first and last phases should be less than 750 ms.
            getTime() - eventData.time < 750 &&
            // Coordinates, when event ends, should be the same as they were
            // on start.
            (
              eventData.pageX === eventData.event.pageX &&
              eventData.pageY === eventData.event.pageY
            )
          ) {
            event.type = specialEventName;
            event.pageX = eventData.event.pageX;
            event.pageY = eventData.event.pageY;

            eventHandle.call(this, event);

            // If an event wasn't prevented then execute original actions.
            if (!event.isDefaultPrevented()) {
              $element
                // Remove prevention of default actions.
                .off(nativeEvent.original)
                // Bring the action.
                .trigger(nativeEvent.original);
            }
          }
        });
    },

    remove: function() {
      $(this).off(nativeEvent.start + ' ' + nativeEvent.end);
    }
  };

  $.fn[specialEventName] = function(fn) {
    return this[fn ? 'on' : 'trigger'](specialEventName, fn);
  };
})(jQuery, 'tap');
