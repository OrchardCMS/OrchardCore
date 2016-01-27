'use strict';

var _createClass = (function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ('value' in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; })();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError('Cannot call a class as a function'); } }

/**
 * --------------------------------------------------------------------------
 * Bootstrap (v4.0.0-alpha.2): alert.js
 * Licensed under MIT (https://github.com/twbs/bootstrap/blob/master/LICENSE)
 * --------------------------------------------------------------------------
 */

var Alert = (function ($) {

  /**
   * ------------------------------------------------------------------------
   * Constants
   * ------------------------------------------------------------------------
   */

  var NAME = 'alert';
  var VERSION = '4.0.0-alpha';
  var DATA_KEY = 'bs.alert';
  var EVENT_KEY = '.' + DATA_KEY;
  var DATA_API_KEY = '.data-api';
  var JQUERY_NO_CONFLICT = $.fn[NAME];
  var TRANSITION_DURATION = 150;

  var Selector = {
    DISMISS: '[data-dismiss="alert"]'
  };

  var Event = {
    CLOSE: 'close' + EVENT_KEY,
    CLOSED: 'closed' + EVENT_KEY,
    CLICK_DATA_API: 'click' + EVENT_KEY + DATA_API_KEY
  };

  var ClassName = {
    ALERT: 'alert',
    FADE: 'fade',
    IN: 'in'
  };

  /**
   * ------------------------------------------------------------------------
   * Class Definition
   * ------------------------------------------------------------------------
   */

  var Alert = (function () {
    function Alert(element) {
      _classCallCheck(this, Alert);

      this._element = element;
    }

    /**
     * ------------------------------------------------------------------------
     * Data Api implementation
     * ------------------------------------------------------------------------
     */

    // getters

    _createClass(Alert, [{
      key: 'close',

      // public

      value: function close(element) {
        element = element || this._element;

        var rootElement = this._getRootElement(element);
        var customEvent = this._triggerCloseEvent(rootElement);

        if (customEvent.isDefaultPrevented()) {
          return;
        }

        this._removeElement(rootElement);
      }
    }, {
      key: 'dispose',
      value: function dispose() {
        $.removeData(this._element, DATA_KEY);
        this._element = null;
      }

      // private

    }, {
      key: '_getRootElement',
      value: function _getRootElement(element) {
        var selector = Util.getSelectorFromElement(element);
        var parent = false;

        if (selector) {
          parent = $(selector)[0];
        }

        if (!parent) {
          parent = $(element).closest('.' + ClassName.ALERT)[0];
        }

        return parent;
      }
    }, {
      key: '_triggerCloseEvent',
      value: function _triggerCloseEvent(element) {
        var closeEvent = $.Event(Event.CLOSE);

        $(element).trigger(closeEvent);
        return closeEvent;
      }
    }, {
      key: '_removeElement',
      value: function _removeElement(element) {
        $(element).removeClass(ClassName.IN);

        if (!Util.supportsTransitionEnd() || !$(element).hasClass(ClassName.FADE)) {
          this._destroyElement(element);
          return;
        }

        $(element).one(Util.TRANSITION_END, $.proxy(this._destroyElement, this, element)).emulateTransitionEnd(TRANSITION_DURATION);
      }
    }, {
      key: '_destroyElement',
      value: function _destroyElement(element) {
        $(element).detach().trigger(Event.CLOSED).remove();
      }

      // static

    }], [{
      key: '_jQueryInterface',
      value: function _jQueryInterface(config) {
        return this.each(function () {
          var $element = $(this);
          var data = $element.data(DATA_KEY);

          if (!data) {
            data = new Alert(this);
            $element.data(DATA_KEY, data);
          }

          if (config === 'close') {
            data[config](this);
          }
        });
      }
    }, {
      key: '_handleDismiss',
      value: function _handleDismiss(alertInstance) {
        return function (event) {
          if (event) {
            event.preventDefault();
          }

          alertInstance.close(this);
        };
      }
    }, {
      key: 'VERSION',
      get: function get() {
        return VERSION;
      }
    }]);

    return Alert;
  })();

  $(document).on(Event.CLICK_DATA_API, Selector.DISMISS, Alert._handleDismiss(new Alert()));

  /**
   * ------------------------------------------------------------------------
   * jQuery
   * ------------------------------------------------------------------------
   */

  $.fn[NAME] = Alert._jQueryInterface;
  $.fn[NAME].Constructor = Alert;
  $.fn[NAME].noConflict = function () {
    $.fn[NAME] = JQUERY_NO_CONFLICT;
    return Alert._jQueryInterface;
  };

  return Alert;
})(jQuery);
//# sourceMappingURL=alert.js.map

/**
 * --------------------------------------------------------------------------
 * Bootstrap (v4.0.0-alpha.2): button.js
 * Licensed under MIT (https://github.com/twbs/bootstrap/blob/master/LICENSE)
 * --------------------------------------------------------------------------
 */

'use strict';

var _createClass = (function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ('value' in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; })();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError('Cannot call a class as a function'); } }

var Button = (function ($) {

  /**
   * ------------------------------------------------------------------------
   * Constants
   * ------------------------------------------------------------------------
   */

  var NAME = 'button';
  var VERSION = '4.0.0-alpha';
  var DATA_KEY = 'bs.button';
  var EVENT_KEY = '.' + DATA_KEY;
  var DATA_API_KEY = '.data-api';
  var JQUERY_NO_CONFLICT = $.fn[NAME];

  var ClassName = {
    ACTIVE: 'active',
    BUTTON: 'btn',
    FOCUS: 'focus'
  };

  var Selector = {
    DATA_TOGGLE_CARROT: '[data-toggle^="button"]',
    DATA_TOGGLE: '[data-toggle="buttons"]',
    INPUT: 'input',
    ACTIVE: '.active',
    BUTTON: '.btn'
  };

  var Event = {
    CLICK_DATA_API: 'click' + EVENT_KEY + DATA_API_KEY,
    FOCUS_BLUR_DATA_API: 'focus' + EVENT_KEY + DATA_API_KEY + ' ' + ('blur' + EVENT_KEY + DATA_API_KEY)
  };

  /**
   * ------------------------------------------------------------------------
   * Class Definition
   * ------------------------------------------------------------------------
   */

  var Button = (function () {
    function Button(element) {
      _classCallCheck(this, Button);

      this._element = element;
    }

    /**
     * ------------------------------------------------------------------------
     * Data Api implementation
     * ------------------------------------------------------------------------
     */

    // getters

    _createClass(Button, [{
      key: 'toggle',

      // public

      value: function toggle() {
        var triggerChangeEvent = true;
        var rootElement = $(this._element).closest(Selector.DATA_TOGGLE)[0];

        if (rootElement) {
          var input = $(this._element).find(Selector.INPUT)[0];

          if (input) {
            if (input.type === 'radio') {
              if (input.checked && $(this._element).hasClass(ClassName.ACTIVE)) {
                triggerChangeEvent = false;
              } else {
                var activeElement = $(rootElement).find(Selector.ACTIVE)[0];

                if (activeElement) {
                  $(activeElement).removeClass(ClassName.ACTIVE);
                }
              }
            }

            if (triggerChangeEvent) {
              input.checked = !$(this._element).hasClass(ClassName.ACTIVE);
              $(this._element).trigger('change');
            }
          }
        } else {
          this._element.setAttribute('aria-pressed', !$(this._element).hasClass(ClassName.ACTIVE));
        }

        if (triggerChangeEvent) {
          $(this._element).toggleClass(ClassName.ACTIVE);
        }
      }
    }, {
      key: 'dispose',
      value: function dispose() {
        $.removeData(this._element, DATA_KEY);
        this._element = null;
      }

      // static

    }], [{
      key: '_jQueryInterface',
      value: function _jQueryInterface(config) {
        return this.each(function () {
          var data = $(this).data(DATA_KEY);

          if (!data) {
            data = new Button(this);
            $(this).data(DATA_KEY, data);
          }

          if (config === 'toggle') {
            data[config]();
          }
        });
      }
    }, {
      key: 'VERSION',
      get: function get() {
        return VERSION;
      }
    }]);

    return Button;
  })();

  $(document).on(Event.CLICK_DATA_API, Selector.DATA_TOGGLE_CARROT, function (event) {
    event.preventDefault();

    var button = event.target;

    if (!$(button).hasClass(ClassName.BUTTON)) {
      button = $(button).closest(Selector.BUTTON);
    }

    Button._jQueryInterface.call($(button), 'toggle');
  }).on(Event.FOCUS_BLUR_DATA_API, Selector.DATA_TOGGLE_CARROT, function (event) {
    var button = $(event.target).closest(Selector.BUTTON)[0];
    $(button).toggleClass(ClassName.FOCUS, /^focus(in)?$/.test(event.type));
  });

  /**
   * ------------------------------------------------------------------------
   * jQuery
   * ------------------------------------------------------------------------
   */

  $.fn[NAME] = Button._jQueryInterface;
  $.fn[NAME].Constructor = Button;
  $.fn[NAME].noConflict = function () {
    $.fn[NAME] = JQUERY_NO_CONFLICT;
    return Button._jQueryInterface;
  };

  return Button;
})(jQuery);
//# sourceMappingURL=button.js.map

'use strict';

var _createClass = (function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ('value' in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; })();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError('Cannot call a class as a function'); } }

/**
 * --------------------------------------------------------------------------
 * Bootstrap (v4.0.0-alpha.2): carousel.js
 * Licensed under MIT (https://github.com/twbs/bootstrap/blob/master/LICENSE)
 * --------------------------------------------------------------------------
 */

var Carousel = (function ($) {

  /**
   * ------------------------------------------------------------------------
   * Constants
   * ------------------------------------------------------------------------
   */

  var NAME = 'carousel';
  var VERSION = '4.0.0-alpha';
  var DATA_KEY = 'bs.carousel';
  var EVENT_KEY = '.' + DATA_KEY;
  var DATA_API_KEY = '.data-api';
  var JQUERY_NO_CONFLICT = $.fn[NAME];
  var TRANSITION_DURATION = 600;

  var Default = {
    interval: 5000,
    keyboard: true,
    slide: false,
    pause: 'hover',
    wrap: true
  };

  var DefaultType = {
    interval: '(number|boolean)',
    keyboard: 'boolean',
    slide: '(boolean|string)',
    pause: '(string|boolean)',
    wrap: 'boolean'
  };

  var Direction = {
    NEXT: 'next',
    PREVIOUS: 'prev'
  };

  var Event = {
    SLIDE: 'slide' + EVENT_KEY,
    SLID: 'slid' + EVENT_KEY,
    KEYDOWN: 'keydown' + EVENT_KEY,
    MOUSEENTER: 'mouseenter' + EVENT_KEY,
    MOUSELEAVE: 'mouseleave' + EVENT_KEY,
    LOAD_DATA_API: 'load' + EVENT_KEY + DATA_API_KEY,
    CLICK_DATA_API: 'click' + EVENT_KEY + DATA_API_KEY
  };

  var ClassName = {
    CAROUSEL: 'carousel',
    ACTIVE: 'active',
    SLIDE: 'slide',
    RIGHT: 'right',
    LEFT: 'left',
    ITEM: 'carousel-item'
  };

  var Selector = {
    ACTIVE: '.active',
    ACTIVE_ITEM: '.active.carousel-item',
    ITEM: '.carousel-item',
    NEXT_PREV: '.next, .prev',
    INDICATORS: '.carousel-indicators',
    DATA_SLIDE: '[data-slide], [data-slide-to]',
    DATA_RIDE: '[data-ride="carousel"]'
  };

  /**
   * ------------------------------------------------------------------------
   * Class Definition
   * ------------------------------------------------------------------------
   */

  var Carousel = (function () {
    function Carousel(element, config) {
      _classCallCheck(this, Carousel);

      this._items = null;
      this._interval = null;
      this._activeElement = null;

      this._isPaused = false;
      this._isSliding = false;

      this._config = this._getConfig(config);
      this._element = $(element)[0];
      this._indicatorsElement = $(this._element).find(Selector.INDICATORS)[0];

      this._addEventListeners();
    }

    /**
     * ------------------------------------------------------------------------
     * Data Api implementation
     * ------------------------------------------------------------------------
     */

    // getters

    _createClass(Carousel, [{
      key: 'next',

      // public

      value: function next() {
        if (!this._isSliding) {
          this._slide(Direction.NEXT);
        }
      }
    }, {
      key: 'nextWhenVisible',
      value: function nextWhenVisible() {
        // Don't call next when the page isn't visible
        if (!document.hidden) {
          this.next();
        }
      }
    }, {
      key: 'prev',
      value: function prev() {
        if (!this._isSliding) {
          this._slide(Direction.PREVIOUS);
        }
      }
    }, {
      key: 'pause',
      value: function pause(event) {
        if (!event) {
          this._isPaused = true;
        }

        if ($(this._element).find(Selector.NEXT_PREV)[0] && Util.supportsTransitionEnd()) {
          Util.triggerTransitionEnd(this._element);
          this.cycle(true);
        }

        clearInterval(this._interval);
        this._interval = null;
      }
    }, {
      key: 'cycle',
      value: function cycle(event) {
        if (!event) {
          this._isPaused = false;
        }

        if (this._interval) {
          clearInterval(this._interval);
          this._interval = null;
        }

        if (this._config.interval && !this._isPaused) {
          this._interval = setInterval($.proxy(document.visibilityState ? this.nextWhenVisible : this.next, this), this._config.interval);
        }
      }
    }, {
      key: 'to',
      value: function to(index) {
        var _this = this;

        this._activeElement = $(this._element).find(Selector.ACTIVE_ITEM)[0];

        var activeIndex = this._getItemIndex(this._activeElement);

        if (index > this._items.length - 1 || index < 0) {
          return;
        }

        if (this._isSliding) {
          $(this._element).one(Event.SLID, function () {
            return _this.to(index);
          });
          return;
        }

        if (activeIndex === index) {
          this.pause();
          this.cycle();
          return;
        }

        var direction = index > activeIndex ? Direction.NEXT : Direction.PREVIOUS;

        this._slide(direction, this._items[index]);
      }
    }, {
      key: 'dispose',
      value: function dispose() {
        $(this._element).off(EVENT_KEY);
        $.removeData(this._element, DATA_KEY);

        this._items = null;
        this._config = null;
        this._element = null;
        this._interval = null;
        this._isPaused = null;
        this._isSliding = null;
        this._activeElement = null;
        this._indicatorsElement = null;
      }

      // private

    }, {
      key: '_getConfig',
      value: function _getConfig(config) {
        config = $.extend({}, Default, config);
        Util.typeCheckConfig(NAME, config, DefaultType);
        return config;
      }
    }, {
      key: '_addEventListeners',
      value: function _addEventListeners() {
        if (this._config.keyboard) {
          $(this._element).on(Event.KEYDOWN, $.proxy(this._keydown, this));
        }

        if (this._config.pause === 'hover' && !('ontouchstart' in document.documentElement)) {
          $(this._element).on(Event.MOUSEENTER, $.proxy(this.pause, this)).on(Event.MOUSELEAVE, $.proxy(this.cycle, this));
        }
      }
    }, {
      key: '_keydown',
      value: function _keydown(event) {
        event.preventDefault();

        if (/input|textarea/i.test(event.target.tagName)) {
          return;
        }

        switch (event.which) {
          case 37:
            this.prev();break;
          case 39:
            this.next();break;
          default:
            return;
        }
      }
    }, {
      key: '_getItemIndex',
      value: function _getItemIndex(element) {
        this._items = $.makeArray($(element).parent().find(Selector.ITEM));
        return this._items.indexOf(element);
      }
    }, {
      key: '_getItemByDirection',
      value: function _getItemByDirection(direction, activeElement) {
        var isNextDirection = direction === Direction.NEXT;
        var isPrevDirection = direction === Direction.PREVIOUS;
        var activeIndex = this._getItemIndex(activeElement);
        var lastItemIndex = this._items.length - 1;
        var isGoingToWrap = isPrevDirection && activeIndex === 0 || isNextDirection && activeIndex === lastItemIndex;

        if (isGoingToWrap && !this._config.wrap) {
          return activeElement;
        }

        var delta = direction === Direction.PREVIOUS ? -1 : 1;
        var itemIndex = (activeIndex + delta) % this._items.length;

        return itemIndex === -1 ? this._items[this._items.length - 1] : this._items[itemIndex];
      }
    }, {
      key: '_triggerSlideEvent',
      value: function _triggerSlideEvent(relatedTarget, directionalClassname) {
        var slideEvent = $.Event(Event.SLIDE, {
          relatedTarget: relatedTarget,
          direction: directionalClassname
        });

        $(this._element).trigger(slideEvent);

        return slideEvent;
      }
    }, {
      key: '_setActiveIndicatorElement',
      value: function _setActiveIndicatorElement(element) {
        if (this._indicatorsElement) {
          $(this._indicatorsElement).find(Selector.ACTIVE).removeClass(ClassName.ACTIVE);

          var nextIndicator = this._indicatorsElement.children[this._getItemIndex(element)];

          if (nextIndicator) {
            $(nextIndicator).addClass(ClassName.ACTIVE);
          }
        }
      }
    }, {
      key: '_slide',
      value: function _slide(direction, element) {
        var _this2 = this;

        var activeElement = $(this._element).find(Selector.ACTIVE_ITEM)[0];
        var nextElement = element || activeElement && this._getItemByDirection(direction, activeElement);

        var isCycling = Boolean(this._interval);

        var directionalClassName = direction === Direction.NEXT ? ClassName.LEFT : ClassName.RIGHT;

        if (nextElement && $(nextElement).hasClass(ClassName.ACTIVE)) {
          this._isSliding = false;
          return;
        }

        var slideEvent = this._triggerSlideEvent(nextElement, directionalClassName);
        if (slideEvent.isDefaultPrevented()) {
          return;
        }

        if (!activeElement || !nextElement) {
          // some weirdness is happening, so we bail
          return;
        }

        this._isSliding = true;

        if (isCycling) {
          this.pause();
        }

        this._setActiveIndicatorElement(nextElement);

        var slidEvent = $.Event(Event.SLID, {
          relatedTarget: nextElement,
          direction: directionalClassName
        });

        if (Util.supportsTransitionEnd() && $(this._element).hasClass(ClassName.SLIDE)) {

          $(nextElement).addClass(direction);

          Util.reflow(nextElement);

          $(activeElement).addClass(directionalClassName);
          $(nextElement).addClass(directionalClassName);

          $(activeElement).one(Util.TRANSITION_END, function () {
            $(nextElement).removeClass(directionalClassName).removeClass(direction);

            $(nextElement).addClass(ClassName.ACTIVE);

            $(activeElement).removeClass(ClassName.ACTIVE).removeClass(direction).removeClass(directionalClassName);

            _this2._isSliding = false;

            setTimeout(function () {
              return $(_this2._element).trigger(slidEvent);
            }, 0);
          }).emulateTransitionEnd(TRANSITION_DURATION);
        } else {
          $(activeElement).removeClass(ClassName.ACTIVE);
          $(nextElement).addClass(ClassName.ACTIVE);

          this._isSliding = false;
          $(this._element).trigger(slidEvent);
        }

        if (isCycling) {
          this.cycle();
        }
      }

      // static

    }], [{
      key: '_jQueryInterface',
      value: function _jQueryInterface(config) {
        return this.each(function () {
          var data = $(this).data(DATA_KEY);
          var _config = $.extend({}, Default, $(this).data());

          if (typeof config === 'object') {
            $.extend(_config, config);
          }

          var action = typeof config === 'string' ? config : _config.slide;

          if (!data) {
            data = new Carousel(this, _config);
            $(this).data(DATA_KEY, data);
          }

          if (typeof config === 'number') {
            data.to(config);
          } else if (typeof action === 'string') {
            if (data[action] === undefined) {
              throw new Error('No method named "' + action + '"');
            }
            data[action]();
          } else if (_config.interval) {
            data.pause();
            data.cycle();
          }
        });
      }
    }, {
      key: '_dataApiClickHandler',
      value: function _dataApiClickHandler(event) {
        var selector = Util.getSelectorFromElement(this);

        if (!selector) {
          return;
        }

        var target = $(selector)[0];

        if (!target || !$(target).hasClass(ClassName.CAROUSEL)) {
          return;
        }

        var config = $.extend({}, $(target).data(), $(this).data());
        var slideIndex = this.getAttribute('data-slide-to');

        if (slideIndex) {
          config.interval = false;
        }

        Carousel._jQueryInterface.call($(target), config);

        if (slideIndex) {
          $(target).data(DATA_KEY).to(slideIndex);
        }

        event.preventDefault();
      }
    }, {
      key: 'VERSION',
      get: function get() {
        return VERSION;
      }
    }, {
      key: 'Default',
      get: function get() {
        return Default;
      }
    }]);

    return Carousel;
  })();

  $(document).on(Event.CLICK_DATA_API, Selector.DATA_SLIDE, Carousel._dataApiClickHandler);

  $(window).on(Event.LOAD_DATA_API, function () {
    $(Selector.DATA_RIDE).each(function () {
      var $carousel = $(this);
      Carousel._jQueryInterface.call($carousel, $carousel.data());
    });
  });

  /**
   * ------------------------------------------------------------------------
   * jQuery
   * ------------------------------------------------------------------------
   */

  $.fn[NAME] = Carousel._jQueryInterface;
  $.fn[NAME].Constructor = Carousel;
  $.fn[NAME].noConflict = function () {
    $.fn[NAME] = JQUERY_NO_CONFLICT;
    return Carousel._jQueryInterface;
  };

  return Carousel;
})(jQuery);
//# sourceMappingURL=carousel.js.map

'use strict';

var _createClass = (function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ('value' in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; })();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError('Cannot call a class as a function'); } }

/**
 * --------------------------------------------------------------------------
 * Bootstrap (v4.0.0-alpha.2): collapse.js
 * Licensed under MIT (https://github.com/twbs/bootstrap/blob/master/LICENSE)
 * --------------------------------------------------------------------------
 */

var Collapse = (function ($) {

  /**
   * ------------------------------------------------------------------------
   * Constants
   * ------------------------------------------------------------------------
   */

  var NAME = 'collapse';
  var VERSION = '4.0.0-alpha';
  var DATA_KEY = 'bs.collapse';
  var EVENT_KEY = '.' + DATA_KEY;
  var DATA_API_KEY = '.data-api';
  var JQUERY_NO_CONFLICT = $.fn[NAME];
  var TRANSITION_DURATION = 600;

  var Default = {
    toggle: true,
    parent: ''
  };

  var DefaultType = {
    toggle: 'boolean',
    parent: 'string'
  };

  var Event = {
    SHOW: 'show' + EVENT_KEY,
    SHOWN: 'shown' + EVENT_KEY,
    HIDE: 'hide' + EVENT_KEY,
    HIDDEN: 'hidden' + EVENT_KEY,
    CLICK_DATA_API: 'click' + EVENT_KEY + DATA_API_KEY
  };

  var ClassName = {
    IN: 'in',
    COLLAPSE: 'collapse',
    COLLAPSING: 'collapsing',
    COLLAPSED: 'collapsed'
  };

  var Dimension = {
    WIDTH: 'width',
    HEIGHT: 'height'
  };

  var Selector = {
    ACTIVES: '.panel > .in, .panel > .collapsing',
    DATA_TOGGLE: '[data-toggle="collapse"]'
  };

  /**
   * ------------------------------------------------------------------------
   * Class Definition
   * ------------------------------------------------------------------------
   */

  var Collapse = (function () {
    function Collapse(element, config) {
      _classCallCheck(this, Collapse);

      this._isTransitioning = false;
      this._element = element;
      this._config = this._getConfig(config);
      this._triggerArray = $.makeArray($('[data-toggle="collapse"][href="#' + element.id + '"],' + ('[data-toggle="collapse"][data-target="#' + element.id + '"]')));

      this._parent = this._config.parent ? this._getParent() : null;

      if (!this._config.parent) {
        this._addAriaAndCollapsedClass(this._element, this._triggerArray);
      }

      if (this._config.toggle) {
        this.toggle();
      }
    }

    /**
     * ------------------------------------------------------------------------
     * Data Api implementation
     * ------------------------------------------------------------------------
     */

    // getters

    _createClass(Collapse, [{
      key: 'toggle',

      // public

      value: function toggle() {
        if ($(this._element).hasClass(ClassName.IN)) {
          this.hide();
        } else {
          this.show();
        }
      }
    }, {
      key: 'show',
      value: function show() {
        var _this = this;

        if (this._isTransitioning || $(this._element).hasClass(ClassName.IN)) {
          return;
        }

        var actives = undefined;
        var activesData = undefined;

        if (this._parent) {
          actives = $.makeArray($(Selector.ACTIVES));
          if (!actives.length) {
            actives = null;
          }
        }

        if (actives) {
          activesData = $(actives).data(DATA_KEY);
          if (activesData && activesData._isTransitioning) {
            return;
          }
        }

        var startEvent = $.Event(Event.SHOW);
        $(this._element).trigger(startEvent);
        if (startEvent.isDefaultPrevented()) {
          return;
        }

        if (actives) {
          Collapse._jQueryInterface.call($(actives), 'hide');
          if (!activesData) {
            $(actives).data(DATA_KEY, null);
          }
        }

        var dimension = this._getDimension();

        $(this._element).removeClass(ClassName.COLLAPSE).addClass(ClassName.COLLAPSING);

        this._element.style[dimension] = 0;
        this._element.setAttribute('aria-expanded', true);

        if (this._triggerArray.length) {
          $(this._triggerArray).removeClass(ClassName.COLLAPSED).attr('aria-expanded', true);
        }

        this.setTransitioning(true);

        var complete = function complete() {
          $(_this._element).removeClass(ClassName.COLLAPSING).addClass(ClassName.COLLAPSE).addClass(ClassName.IN);

          _this._element.style[dimension] = '';

          _this.setTransitioning(false);

          $(_this._element).trigger(Event.SHOWN);
        };

        if (!Util.supportsTransitionEnd()) {
          complete();
          return;
        }

        var capitalizedDimension = dimension[0].toUpperCase() + dimension.slice(1);
        var scrollSize = 'scroll' + capitalizedDimension;

        $(this._element).one(Util.TRANSITION_END, complete).emulateTransitionEnd(TRANSITION_DURATION);

        this._element.style[dimension] = this._element[scrollSize] + 'px';
      }
    }, {
      key: 'hide',
      value: function hide() {
        var _this2 = this;

        if (this._isTransitioning || !$(this._element).hasClass(ClassName.IN)) {
          return;
        }

        var startEvent = $.Event(Event.HIDE);
        $(this._element).trigger(startEvent);
        if (startEvent.isDefaultPrevented()) {
          return;
        }

        var dimension = this._getDimension();
        var offsetDimension = dimension === Dimension.WIDTH ? 'offsetWidth' : 'offsetHeight';

        this._element.style[dimension] = this._element[offsetDimension] + 'px';

        Util.reflow(this._element);

        $(this._element).addClass(ClassName.COLLAPSING).removeClass(ClassName.COLLAPSE).removeClass(ClassName.IN);

        this._element.setAttribute('aria-expanded', false);

        if (this._triggerArray.length) {
          $(this._triggerArray).addClass(ClassName.COLLAPSED).attr('aria-expanded', false);
        }

        this.setTransitioning(true);

        var complete = function complete() {
          _this2.setTransitioning(false);
          $(_this2._element).removeClass(ClassName.COLLAPSING).addClass(ClassName.COLLAPSE).trigger(Event.HIDDEN);
        };

        this._element.style[dimension] = 0;

        if (!Util.supportsTransitionEnd()) {
          complete();
          return;
        }

        $(this._element).one(Util.TRANSITION_END, complete).emulateTransitionEnd(TRANSITION_DURATION);
      }
    }, {
      key: 'setTransitioning',
      value: function setTransitioning(isTransitioning) {
        this._isTransitioning = isTransitioning;
      }
    }, {
      key: 'dispose',
      value: function dispose() {
        $.removeData(this._element, DATA_KEY);

        this._config = null;
        this._parent = null;
        this._element = null;
        this._triggerArray = null;
        this._isTransitioning = null;
      }

      // private

    }, {
      key: '_getConfig',
      value: function _getConfig(config) {
        config = $.extend({}, Default, config);
        config.toggle = Boolean(config.toggle); // coerce string values
        Util.typeCheckConfig(NAME, config, DefaultType);
        return config;
      }
    }, {
      key: '_getDimension',
      value: function _getDimension() {
        var hasWidth = $(this._element).hasClass(Dimension.WIDTH);
        return hasWidth ? Dimension.WIDTH : Dimension.HEIGHT;
      }
    }, {
      key: '_getParent',
      value: function _getParent() {
        var _this3 = this;

        var parent = $(this._config.parent)[0];
        var selector = '[data-toggle="collapse"][data-parent="' + this._config.parent + '"]';

        $(parent).find(selector).each(function (i, element) {
          _this3._addAriaAndCollapsedClass(Collapse._getTargetFromElement(element), [element]);
        });

        return parent;
      }
    }, {
      key: '_addAriaAndCollapsedClass',
      value: function _addAriaAndCollapsedClass(element, triggerArray) {
        if (element) {
          var isOpen = $(element).hasClass(ClassName.IN);
          element.setAttribute('aria-expanded', isOpen);

          if (triggerArray.length) {
            $(triggerArray).toggleClass(ClassName.COLLAPSED, !isOpen).attr('aria-expanded', isOpen);
          }
        }
      }

      // static

    }], [{
      key: '_getTargetFromElement',
      value: function _getTargetFromElement(element) {
        var selector = Util.getSelectorFromElement(element);
        return selector ? $(selector)[0] : null;
      }
    }, {
      key: '_jQueryInterface',
      value: function _jQueryInterface(config) {
        return this.each(function () {
          var $this = $(this);
          var data = $this.data(DATA_KEY);
          var _config = $.extend({}, Default, $this.data(), typeof config === 'object' && config);

          if (!data && _config.toggle && /show|hide/.test(config)) {
            _config.toggle = false;
          }

          if (!data) {
            data = new Collapse(this, _config);
            $this.data(DATA_KEY, data);
          }

          if (typeof config === 'string') {
            if (data[config] === undefined) {
              throw new Error('No method named "' + config + '"');
            }
            data[config]();
          }
        });
      }
    }, {
      key: 'VERSION',
      get: function get() {
        return VERSION;
      }
    }, {
      key: 'Default',
      get: function get() {
        return Default;
      }
    }]);

    return Collapse;
  })();

  $(document).on(Event.CLICK_DATA_API, Selector.DATA_TOGGLE, function (event) {
    event.preventDefault();

    var target = Collapse._getTargetFromElement(this);
    var data = $(target).data(DATA_KEY);
    var config = data ? 'toggle' : $(this).data();

    Collapse._jQueryInterface.call($(target), config);
  });

  /**
   * ------------------------------------------------------------------------
   * jQuery
   * ------------------------------------------------------------------------
   */

  $.fn[NAME] = Collapse._jQueryInterface;
  $.fn[NAME].Constructor = Collapse;
  $.fn[NAME].noConflict = function () {
    $.fn[NAME] = JQUERY_NO_CONFLICT;
    return Collapse._jQueryInterface;
  };

  return Collapse;
})(jQuery);
//# sourceMappingURL=collapse.js.map

'use strict';

var _createClass = (function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ('value' in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; })();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError('Cannot call a class as a function'); } }

/**
 * --------------------------------------------------------------------------
 * Bootstrap (v4.0.0-alpha.2): dropdown.js
 * Licensed under MIT (https://github.com/twbs/bootstrap/blob/master/LICENSE)
 * --------------------------------------------------------------------------
 */

var Dropdown = (function ($) {

  /**
   * ------------------------------------------------------------------------
   * Constants
   * ------------------------------------------------------------------------
   */

  var NAME = 'dropdown';
  var VERSION = '4.0.0-alpha';
  var DATA_KEY = 'bs.dropdown';
  var EVENT_KEY = '.' + DATA_KEY;
  var DATA_API_KEY = '.data-api';
  var JQUERY_NO_CONFLICT = $.fn[NAME];

  var Event = {
    HIDE: 'hide' + EVENT_KEY,
    HIDDEN: 'hidden' + EVENT_KEY,
    SHOW: 'show' + EVENT_KEY,
    SHOWN: 'shown' + EVENT_KEY,
    CLICK: 'click' + EVENT_KEY,
    CLICK_DATA_API: 'click' + EVENT_KEY + DATA_API_KEY,
    KEYDOWN_DATA_API: 'keydown' + EVENT_KEY + DATA_API_KEY
  };

  var ClassName = {
    BACKDROP: 'dropdown-backdrop',
    DISABLED: 'disabled',
    OPEN: 'open'
  };

  var Selector = {
    BACKDROP: '.dropdown-backdrop',
    DATA_TOGGLE: '[data-toggle="dropdown"]',
    FORM_CHILD: '.dropdown form',
    ROLE_MENU: '[role="menu"]',
    ROLE_LISTBOX: '[role="listbox"]',
    NAVBAR_NAV: '.navbar-nav',
    VISIBLE_ITEMS: '[role="menu"] li:not(.disabled) a, ' + '[role="listbox"] li:not(.disabled) a'
  };

  /**
   * ------------------------------------------------------------------------
   * Class Definition
   * ------------------------------------------------------------------------
   */

  var Dropdown = (function () {
    function Dropdown(element) {
      _classCallCheck(this, Dropdown);

      this._element = element;

      this._addEventListeners();
    }

    /**
     * ------------------------------------------------------------------------
     * Data Api implementation
     * ------------------------------------------------------------------------
     */

    // getters

    _createClass(Dropdown, [{
      key: 'toggle',

      // public

      value: function toggle() {
        if (this.disabled || $(this).hasClass(ClassName.DISABLED)) {
          return false;
        }

        var parent = Dropdown._getParentFromElement(this);
        var isActive = $(parent).hasClass(ClassName.OPEN);

        Dropdown._clearMenus();

        if (isActive) {
          return false;
        }

        if ('ontouchstart' in document.documentElement && !$(parent).closest(Selector.NAVBAR_NAV).length) {

          // if mobile we use a backdrop because click events don't delegate
          var dropdown = document.createElement('div');
          dropdown.className = ClassName.BACKDROP;
          $(dropdown).insertBefore(this);
          $(dropdown).on('click', Dropdown._clearMenus);
        }

        var relatedTarget = { relatedTarget: this };
        var showEvent = $.Event(Event.SHOW, relatedTarget);

        $(parent).trigger(showEvent);

        if (showEvent.isDefaultPrevented()) {
          return false;
        }

        this.focus();
        this.setAttribute('aria-expanded', 'true');

        $(parent).toggleClass(ClassName.OPEN);
        $(parent).trigger($.Event(Event.SHOWN, relatedTarget));

        return false;
      }
    }, {
      key: 'dispose',
      value: function dispose() {
        $.removeData(this._element, DATA_KEY);
        $(this._element).off(EVENT_KEY);
        this._element = null;
      }

      // private

    }, {
      key: '_addEventListeners',
      value: function _addEventListeners() {
        $(this._element).on(Event.CLICK, this.toggle);
      }

      // static

    }], [{
      key: '_jQueryInterface',
      value: function _jQueryInterface(config) {
        return this.each(function () {
          var data = $(this).data(DATA_KEY);

          if (!data) {
            $(this).data(DATA_KEY, data = new Dropdown(this));
          }

          if (typeof config === 'string') {
            if (data[config] === undefined) {
              throw new Error('No method named "' + config + '"');
            }
            data[config].call(this);
          }
        });
      }
    }, {
      key: '_clearMenus',
      value: function _clearMenus(event) {
        if (event && event.which === 3) {
          return;
        }

        var backdrop = $(Selector.BACKDROP)[0];
        if (backdrop) {
          backdrop.parentNode.removeChild(backdrop);
        }

        var toggles = $.makeArray($(Selector.DATA_TOGGLE));

        for (var i = 0; i < toggles.length; i++) {
          var _parent = Dropdown._getParentFromElement(toggles[i]);
          var relatedTarget = { relatedTarget: toggles[i] };

          if (!$(_parent).hasClass(ClassName.OPEN)) {
            continue;
          }

          if (event && event.type === 'click' && /input|textarea/i.test(event.target.tagName) && $.contains(_parent, event.target)) {
            continue;
          }

          var hideEvent = $.Event(Event.HIDE, relatedTarget);
          $(_parent).trigger(hideEvent);
          if (hideEvent.isDefaultPrevented()) {
            continue;
          }

          toggles[i].setAttribute('aria-expanded', 'false');

          $(_parent).removeClass(ClassName.OPEN).trigger($.Event(Event.HIDDEN, relatedTarget));
        }
      }
    }, {
      key: '_getParentFromElement',
      value: function _getParentFromElement(element) {
        var parent = undefined;
        var selector = Util.getSelectorFromElement(element);

        if (selector) {
          parent = $(selector)[0];
        }

        return parent || element.parentNode;
      }
    }, {
      key: '_dataApiKeydownHandler',
      value: function _dataApiKeydownHandler(event) {
        if (!/(38|40|27|32)/.test(event.which) || /input|textarea/i.test(event.target.tagName)) {
          return;
        }

        event.preventDefault();
        event.stopPropagation();

        if (this.disabled || $(this).hasClass(ClassName.DISABLED)) {
          return;
        }

        var parent = Dropdown._getParentFromElement(this);
        var isActive = $(parent).hasClass(ClassName.OPEN);

        if (!isActive && event.which !== 27 || isActive && event.which === 27) {

          if (event.which === 27) {
            var toggle = $(parent).find(Selector.DATA_TOGGLE)[0];
            $(toggle).trigger('focus');
          }

          $(this).trigger('click');
          return;
        }

        var items = $.makeArray($(Selector.VISIBLE_ITEMS));

        items = items.filter(function (item) {
          return item.offsetWidth || item.offsetHeight;
        });

        if (!items.length) {
          return;
        }

        var index = items.indexOf(event.target);

        if (event.which === 38 && index > 0) {
          // up
          index--;
        }

        if (event.which === 40 && index < items.length - 1) {
          // down
          index++;
        }

        if (! ~index) {
          index = 0;
        }

        items[index].focus();
      }
    }, {
      key: 'VERSION',
      get: function get() {
        return VERSION;
      }
    }]);

    return Dropdown;
  })();

  $(document).on(Event.KEYDOWN_DATA_API, Selector.DATA_TOGGLE, Dropdown._dataApiKeydownHandler).on(Event.KEYDOWN_DATA_API, Selector.ROLE_MENU, Dropdown._dataApiKeydownHandler).on(Event.KEYDOWN_DATA_API, Selector.ROLE_LISTBOX, Dropdown._dataApiKeydownHandler).on(Event.CLICK_DATA_API, Dropdown._clearMenus).on(Event.CLICK_DATA_API, Selector.DATA_TOGGLE, Dropdown.prototype.toggle).on(Event.CLICK_DATA_API, Selector.FORM_CHILD, function (e) {
    e.stopPropagation();
  });

  /**
   * ------------------------------------------------------------------------
   * jQuery
   * ------------------------------------------------------------------------
   */

  $.fn[NAME] = Dropdown._jQueryInterface;
  $.fn[NAME].Constructor = Dropdown;
  $.fn[NAME].noConflict = function () {
    $.fn[NAME] = JQUERY_NO_CONFLICT;
    return Dropdown._jQueryInterface;
  };

  return Dropdown;
})(jQuery);
//# sourceMappingURL=dropdown.js.map

'use strict';

var _createClass = (function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ('value' in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; })();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError('Cannot call a class as a function'); } }

/**
 * --------------------------------------------------------------------------
 * Bootstrap (v4.0.0-alpha.2): modal.js
 * Licensed under MIT (https://github.com/twbs/bootstrap/blob/master/LICENSE)
 * --------------------------------------------------------------------------
 */

var Modal = (function ($) {

  /**
   * ------------------------------------------------------------------------
   * Constants
   * ------------------------------------------------------------------------
   */

  var NAME = 'modal';
  var VERSION = '4.0.0-alpha';
  var DATA_KEY = 'bs.modal';
  var EVENT_KEY = '.' + DATA_KEY;
  var DATA_API_KEY = '.data-api';
  var JQUERY_NO_CONFLICT = $.fn[NAME];
  var TRANSITION_DURATION = 300;
  var BACKDROP_TRANSITION_DURATION = 150;

  var Default = {
    backdrop: true,
    keyboard: true,
    focus: true,
    show: true
  };

  var DefaultType = {
    backdrop: '(boolean|string)',
    keyboard: 'boolean',
    focus: 'boolean',
    show: 'boolean'
  };

  var Event = {
    HIDE: 'hide' + EVENT_KEY,
    HIDDEN: 'hidden' + EVENT_KEY,
    SHOW: 'show' + EVENT_KEY,
    SHOWN: 'shown' + EVENT_KEY,
    FOCUSIN: 'focusin' + EVENT_KEY,
    RESIZE: 'resize' + EVENT_KEY,
    CLICK_DISMISS: 'click.dismiss' + EVENT_KEY,
    KEYDOWN_DISMISS: 'keydown.dismiss' + EVENT_KEY,
    MOUSEUP_DISMISS: 'mouseup.dismiss' + EVENT_KEY,
    MOUSEDOWN_DISMISS: 'mousedown.dismiss' + EVENT_KEY,
    CLICK_DATA_API: 'click' + EVENT_KEY + DATA_API_KEY
  };

  var ClassName = {
    SCROLLBAR_MEASURER: 'modal-scrollbar-measure',
    BACKDROP: 'modal-backdrop',
    OPEN: 'modal-open',
    FADE: 'fade',
    IN: 'in'
  };

  var Selector = {
    DIALOG: '.modal-dialog',
    DATA_TOGGLE: '[data-toggle="modal"]',
    DATA_DISMISS: '[data-dismiss="modal"]',
    FIXED_CONTENT: '.navbar-fixed-top, .navbar-fixed-bottom, .is-fixed'
  };

  /**
   * ------------------------------------------------------------------------
   * Class Definition
   * ------------------------------------------------------------------------
   */

  var Modal = (function () {
    function Modal(element, config) {
      _classCallCheck(this, Modal);

      this._config = this._getConfig(config);
      this._element = element;
      this._dialog = $(element).find(Selector.DIALOG)[0];
      this._backdrop = null;
      this._isShown = false;
      this._isBodyOverflowing = false;
      this._ignoreBackdropClick = false;
      this._originalBodyPadding = 0;
      this._scrollbarWidth = 0;
    }

    /**
     * ------------------------------------------------------------------------
     * Data Api implementation
     * ------------------------------------------------------------------------
     */

    // getters

    _createClass(Modal, [{
      key: 'toggle',

      // public

      value: function toggle(relatedTarget) {
        return this._isShown ? this.hide() : this.show(relatedTarget);
      }
    }, {
      key: 'show',
      value: function show(relatedTarget) {
        var _this = this;

        var showEvent = $.Event(Event.SHOW, {
          relatedTarget: relatedTarget
        });

        $(this._element).trigger(showEvent);

        if (this._isShown || showEvent.isDefaultPrevented()) {
          return;
        }

        this._isShown = true;

        this._checkScrollbar();
        this._setScrollbar();

        $(document.body).addClass(ClassName.OPEN);

        this._setEscapeEvent();
        this._setResizeEvent();

        $(this._element).on(Event.CLICK_DISMISS, Selector.DATA_DISMISS, $.proxy(this.hide, this));

        $(this._dialog).on(Event.MOUSEDOWN_DISMISS, function () {
          $(_this._element).one(Event.MOUSEUP_DISMISS, function (event) {
            if ($(event.target).is(_this._element)) {
              _this._ignoreBackdropClick = true;
            }
          });
        });

        this._showBackdrop($.proxy(this._showElement, this, relatedTarget));
      }
    }, {
      key: 'hide',
      value: function hide(event) {
        if (event) {
          event.preventDefault();
        }

        var hideEvent = $.Event(Event.HIDE);

        $(this._element).trigger(hideEvent);

        if (!this._isShown || hideEvent.isDefaultPrevented()) {
          return;
        }

        this._isShown = false;

        this._setEscapeEvent();
        this._setResizeEvent();

        $(document).off(Event.FOCUSIN);

        $(this._element).removeClass(ClassName.IN);

        $(this._element).off(Event.CLICK_DISMISS);
        $(this._dialog).off(Event.MOUSEDOWN_DISMISS);

        if (Util.supportsTransitionEnd() && $(this._element).hasClass(ClassName.FADE)) {

          $(this._element).one(Util.TRANSITION_END, $.proxy(this._hideModal, this)).emulateTransitionEnd(TRANSITION_DURATION);
        } else {
          this._hideModal();
        }
      }
    }, {
      key: 'dispose',
      value: function dispose() {
        $.removeData(this._element, DATA_KEY);

        $(window).off(EVENT_KEY);
        $(document).off(EVENT_KEY);
        $(this._element).off(EVENT_KEY);
        $(this._backdrop).off(EVENT_KEY);

        this._config = null;
        this._element = null;
        this._dialog = null;
        this._backdrop = null;
        this._isShown = null;
        this._isBodyOverflowing = null;
        this._ignoreBackdropClick = null;
        this._originalBodyPadding = null;
        this._scrollbarWidth = null;
      }

      // private

    }, {
      key: '_getConfig',
      value: function _getConfig(config) {
        config = $.extend({}, Default, config);
        Util.typeCheckConfig(NAME, config, DefaultType);
        return config;
      }
    }, {
      key: '_showElement',
      value: function _showElement(relatedTarget) {
        var _this2 = this;

        var transition = Util.supportsTransitionEnd() && $(this._element).hasClass(ClassName.FADE);

        if (!this._element.parentNode || this._element.parentNode.nodeType !== Node.ELEMENT_NODE) {
          // don't move modals dom position
          document.body.appendChild(this._element);
        }

        this._element.style.display = 'block';
        this._element.scrollTop = 0;

        if (transition) {
          Util.reflow(this._element);
        }

        $(this._element).addClass(ClassName.IN);

        if (this._config.focus) {
          this._enforceFocus();
        }

        var shownEvent = $.Event(Event.SHOWN, {
          relatedTarget: relatedTarget
        });

        var transitionComplete = function transitionComplete() {
          if (_this2._config.focus) {
            _this2._element.focus();
          }
          $(_this2._element).trigger(shownEvent);
        };

        if (transition) {
          $(this._dialog).one(Util.TRANSITION_END, transitionComplete).emulateTransitionEnd(TRANSITION_DURATION);
        } else {
          transitionComplete();
        }
      }
    }, {
      key: '_enforceFocus',
      value: function _enforceFocus() {
        var _this3 = this;

        $(document).off(Event.FOCUSIN) // guard against infinite focus loop
        .on(Event.FOCUSIN, function (event) {
          if (_this3._element !== event.target && !$(_this3._element).has(event.target).length) {
            _this3._element.focus();
          }
        });
      }
    }, {
      key: '_setEscapeEvent',
      value: function _setEscapeEvent() {
        var _this4 = this;

        if (this._isShown && this._config.keyboard) {
          $(this._element).on(Event.KEYDOWN_DISMISS, function (event) {
            if (event.which === 27) {
              _this4.hide();
            }
          });
        } else if (!this._isShown) {
          $(this._element).off(Event.KEYDOWN_DISMISS);
        }
      }
    }, {
      key: '_setResizeEvent',
      value: function _setResizeEvent() {
        if (this._isShown) {
          $(window).on(Event.RESIZE, $.proxy(this._handleUpdate, this));
        } else {
          $(window).off(Event.RESIZE);
        }
      }
    }, {
      key: '_hideModal',
      value: function _hideModal() {
        var _this5 = this;

        this._element.style.display = 'none';
        this._showBackdrop(function () {
          $(document.body).removeClass(ClassName.OPEN);
          _this5._resetAdjustments();
          _this5._resetScrollbar();
          $(_this5._element).trigger(Event.HIDDEN);
        });
      }
    }, {
      key: '_removeBackdrop',
      value: function _removeBackdrop() {
        if (this._backdrop) {
          $(this._backdrop).remove();
          this._backdrop = null;
        }
      }
    }, {
      key: '_showBackdrop',
      value: function _showBackdrop(callback) {
        var _this6 = this;

        var animate = $(this._element).hasClass(ClassName.FADE) ? ClassName.FADE : '';

        if (this._isShown && this._config.backdrop) {
          var doAnimate = Util.supportsTransitionEnd() && animate;

          this._backdrop = document.createElement('div');
          this._backdrop.className = ClassName.BACKDROP;

          if (animate) {
            $(this._backdrop).addClass(animate);
          }

          $(this._backdrop).appendTo(document.body);

          $(this._element).on(Event.CLICK_DISMISS, function (event) {
            if (_this6._ignoreBackdropClick) {
              _this6._ignoreBackdropClick = false;
              return;
            }
            if (event.target !== event.currentTarget) {
              return;
            }
            if (_this6._config.backdrop === 'static') {
              _this6._element.focus();
            } else {
              _this6.hide();
            }
          });

          if (doAnimate) {
            Util.reflow(this._backdrop);
          }

          $(this._backdrop).addClass(ClassName.IN);

          if (!callback) {
            return;
          }

          if (!doAnimate) {
            callback();
            return;
          }

          $(this._backdrop).one(Util.TRANSITION_END, callback).emulateTransitionEnd(BACKDROP_TRANSITION_DURATION);
        } else if (!this._isShown && this._backdrop) {
          $(this._backdrop).removeClass(ClassName.IN);

          var callbackRemove = function callbackRemove() {
            _this6._removeBackdrop();
            if (callback) {
              callback();
            }
          };

          if (Util.supportsTransitionEnd() && $(this._element).hasClass(ClassName.FADE)) {
            $(this._backdrop).one(Util.TRANSITION_END, callbackRemove).emulateTransitionEnd(BACKDROP_TRANSITION_DURATION);
          } else {
            callbackRemove();
          }
        } else if (callback) {
          callback();
        }
      }

      // ----------------------------------------------------------------------
      // the following methods are used to handle overflowing modals
      // todo (fat): these should probably be refactored out of modal.js
      // ----------------------------------------------------------------------

    }, {
      key: '_handleUpdate',
      value: function _handleUpdate() {
        this._adjustDialog();
      }
    }, {
      key: '_adjustDialog',
      value: function _adjustDialog() {
        var isModalOverflowing = this._element.scrollHeight > document.documentElement.clientHeight;

        if (!this._isBodyOverflowing && isModalOverflowing) {
          this._element.style.paddingLeft = this._scrollbarWidth + 'px';
        }

        if (this._isBodyOverflowing && !isModalOverflowing) {
          this._element.style.paddingRight = this._scrollbarWidth + 'px~';
        }
      }
    }, {
      key: '_resetAdjustments',
      value: function _resetAdjustments() {
        this._element.style.paddingLeft = '';
        this._element.style.paddingRight = '';
      }
    }, {
      key: '_checkScrollbar',
      value: function _checkScrollbar() {
        var fullWindowWidth = window.innerWidth;
        if (!fullWindowWidth) {
          // workaround for missing window.innerWidth in IE8
          var documentElementRect = document.documentElement.getBoundingClientRect();
          fullWindowWidth = documentElementRect.right - Math.abs(documentElementRect.left);
        }
        this._isBodyOverflowing = document.body.clientWidth < fullWindowWidth;
        this._scrollbarWidth = this._getScrollbarWidth();
      }
    }, {
      key: '_setScrollbar',
      value: function _setScrollbar() {
        var bodyPadding = parseInt($(Selector.FIXED_CONTENT).css('padding-right') || 0, 10);

        this._originalBodyPadding = document.body.style.paddingRight || '';

        if (this._isBodyOverflowing) {
          document.body.style.paddingRight = bodyPadding + this._scrollbarWidth + 'px';
        }
      }
    }, {
      key: '_resetScrollbar',
      value: function _resetScrollbar() {
        document.body.style.paddingRight = this._originalBodyPadding;
      }
    }, {
      key: '_getScrollbarWidth',
      value: function _getScrollbarWidth() {
        // thx d.walsh
        var scrollDiv = document.createElement('div');
        scrollDiv.className = ClassName.SCROLLBAR_MEASURER;
        document.body.appendChild(scrollDiv);
        var scrollbarWidth = scrollDiv.offsetWidth - scrollDiv.clientWidth;
        document.body.removeChild(scrollDiv);
        return scrollbarWidth;
      }

      // static

    }], [{
      key: '_jQueryInterface',
      value: function _jQueryInterface(config, relatedTarget) {
        return this.each(function () {
          var data = $(this).data(DATA_KEY);
          var _config = $.extend({}, Modal.Default, $(this).data(), typeof config === 'object' && config);

          if (!data) {
            data = new Modal(this, _config);
            $(this).data(DATA_KEY, data);
          }

          if (typeof config === 'string') {
            if (data[config] === undefined) {
              throw new Error('No method named "' + config + '"');
            }
            data[config](relatedTarget);
          } else if (_config.show) {
            data.show(relatedTarget);
          }
        });
      }
    }, {
      key: 'VERSION',
      get: function get() {
        return VERSION;
      }
    }, {
      key: 'Default',
      get: function get() {
        return Default;
      }
    }]);

    return Modal;
  })();

  $(document).on(Event.CLICK_DATA_API, Selector.DATA_TOGGLE, function (event) {
    var _this7 = this;

    var target = undefined;
    var selector = Util.getSelectorFromElement(this);

    if (selector) {
      target = $(selector)[0];
    }

    var config = $(target).data(DATA_KEY) ? 'toggle' : $.extend({}, $(target).data(), $(this).data());

    if (this.tagName === 'A') {
      event.preventDefault();
    }

    var $target = $(target).one(Event.SHOW, function (showEvent) {
      if (showEvent.isDefaultPrevented()) {
        // only register focus restorer if modal will actually get shown
        return;
      }

      $target.one(Event.HIDDEN, function () {
        if ($(_this7).is(':visible')) {
          _this7.focus();
        }
      });
    });

    Modal._jQueryInterface.call($(target), config, this);
  });

  /**
   * ------------------------------------------------------------------------
   * jQuery
   * ------------------------------------------------------------------------
   */

  $.fn[NAME] = Modal._jQueryInterface;
  $.fn[NAME].Constructor = Modal;
  $.fn[NAME].noConflict = function () {
    $.fn[NAME] = JQUERY_NO_CONFLICT;
    return Modal._jQueryInterface;
  };

  return Modal;
})(jQuery);
//# sourceMappingURL=modal.js.map

'use strict';

var _createClass = (function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ('value' in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; })();

var _get = function get(_x, _x2, _x3) { var _again = true; _function: while (_again) { var object = _x, property = _x2, receiver = _x3; _again = false; if (object === null) object = Function.prototype; var desc = Object.getOwnPropertyDescriptor(object, property); if (desc === undefined) { var parent = Object.getPrototypeOf(object); if (parent === null) { return undefined; } else { _x = parent; _x2 = property; _x3 = receiver; _again = true; desc = parent = undefined; continue _function; } } else if ('value' in desc) { return desc.value; } else { var getter = desc.get; if (getter === undefined) { return undefined; } return getter.call(receiver); } } };

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError('Cannot call a class as a function'); } }

function _inherits(subClass, superClass) { if (typeof superClass !== 'function' && superClass !== null) { throw new TypeError('Super expression must either be null or a function, not ' + typeof superClass); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, enumerable: false, writable: true, configurable: true } }); if (superClass) Object.setPrototypeOf ? Object.setPrototypeOf(subClass, superClass) : subClass.__proto__ = superClass; }

/**
 * --------------------------------------------------------------------------
 * Bootstrap (v4.0.0-alpha.2): popover.js
 * Licensed under MIT (https://github.com/twbs/bootstrap/blob/master/LICENSE)
 * --------------------------------------------------------------------------
 */

var Popover = (function ($) {

  /**
   * ------------------------------------------------------------------------
   * Constants
   * ------------------------------------------------------------------------
   */

  var NAME = 'popover';
  var VERSION = '4.0.0-alpha';
  var DATA_KEY = 'bs.popover';
  var EVENT_KEY = '.' + DATA_KEY;
  var JQUERY_NO_CONFLICT = $.fn[NAME];

  var Default = $.extend({}, Tooltip.Default, {
    placement: 'right',
    trigger: 'click',
    content: '',
    template: '<div class="popover" role="tooltip">' + '<div class="popover-arrow"></div>' + '<h3 class="popover-title"></h3>' + '<div class="popover-content"></div></div>'
  });

  var DefaultType = $.extend({}, Tooltip.DefaultType, {
    content: '(string|element|function)'
  });

  var ClassName = {
    FADE: 'fade',
    IN: 'in'
  };

  var Selector = {
    TITLE: '.popover-title',
    CONTENT: '.popover-content',
    ARROW: '.popover-arrow'
  };

  var Event = {
    HIDE: 'hide' + EVENT_KEY,
    HIDDEN: 'hidden' + EVENT_KEY,
    SHOW: 'show' + EVENT_KEY,
    SHOWN: 'shown' + EVENT_KEY,
    INSERTED: 'inserted' + EVENT_KEY,
    CLICK: 'click' + EVENT_KEY,
    FOCUSIN: 'focusin' + EVENT_KEY,
    FOCUSOUT: 'focusout' + EVENT_KEY,
    MOUSEENTER: 'mouseenter' + EVENT_KEY,
    MOUSELEAVE: 'mouseleave' + EVENT_KEY
  };

  /**
   * ------------------------------------------------------------------------
   * Class Definition
   * ------------------------------------------------------------------------
   */

  var Popover = (function (_Tooltip) {
    _inherits(Popover, _Tooltip);

    function Popover() {
      _classCallCheck(this, Popover);

      _get(Object.getPrototypeOf(Popover.prototype), 'constructor', this).apply(this, arguments);
    }

    /**
     * ------------------------------------------------------------------------
     * jQuery
     * ------------------------------------------------------------------------
     */

    _createClass(Popover, [{
      key: 'isWithContent',

      // overrides

      value: function isWithContent() {
        return this.getTitle() || this._getContent();
      }
    }, {
      key: 'getTipElement',
      value: function getTipElement() {
        return this.tip = this.tip || $(this.config.template)[0];
      }
    }, {
      key: 'setContent',
      value: function setContent() {
        var $tip = $(this.getTipElement());

        // we use append for html objects to maintain js events
        this.setElementContent($tip.find(Selector.TITLE), this.getTitle());
        this.setElementContent($tip.find(Selector.CONTENT), this._getContent());

        $tip.removeClass(ClassName.FADE).removeClass(ClassName.IN);

        this.cleanupTether();
      }

      // private

    }, {
      key: '_getContent',
      value: function _getContent() {
        return this.element.getAttribute('data-content') || (typeof this.config.content === 'function' ? this.config.content.call(this.element) : this.config.content);
      }

      // static

    }], [{
      key: '_jQueryInterface',
      value: function _jQueryInterface(config) {
        return this.each(function () {
          var data = $(this).data(DATA_KEY);
          var _config = typeof config === 'object' ? config : null;

          if (!data && /destroy|hide/.test(config)) {
            return;
          }

          if (!data) {
            data = new Popover(this, _config);
            $(this).data(DATA_KEY, data);
          }

          if (typeof config === 'string') {
            if (data[config] === undefined) {
              throw new Error('No method named "' + config + '"');
            }
            data[config]();
          }
        });
      }
    }, {
      key: 'VERSION',

      // getters

      get: function get() {
        return VERSION;
      }
    }, {
      key: 'Default',
      get: function get() {
        return Default;
      }
    }, {
      key: 'NAME',
      get: function get() {
        return NAME;
      }
    }, {
      key: 'DATA_KEY',
      get: function get() {
        return DATA_KEY;
      }
    }, {
      key: 'Event',
      get: function get() {
        return Event;
      }
    }, {
      key: 'EVENT_KEY',
      get: function get() {
        return EVENT_KEY;
      }
    }, {
      key: 'DefaultType',
      get: function get() {
        return DefaultType;
      }
    }]);

    return Popover;
  })(Tooltip);

  $.fn[NAME] = Popover._jQueryInterface;
  $.fn[NAME].Constructor = Popover;
  $.fn[NAME].noConflict = function () {
    $.fn[NAME] = JQUERY_NO_CONFLICT;
    return Popover._jQueryInterface;
  };

  return Popover;
})(jQuery);
//# sourceMappingURL=popover.js.map

'use strict';

var _createClass = (function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ('value' in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; })();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError('Cannot call a class as a function'); } }

/**
 * --------------------------------------------------------------------------
 * Bootstrap (v4.0.0-alpha.2): scrollspy.js
 * Licensed under MIT (https://github.com/twbs/bootstrap/blob/master/LICENSE)
 * --------------------------------------------------------------------------
 */

var ScrollSpy = (function ($) {

  /**
   * ------------------------------------------------------------------------
   * Constants
   * ------------------------------------------------------------------------
   */

  var NAME = 'scrollspy';
  var VERSION = '4.0.0-alpha';
  var DATA_KEY = 'bs.scrollspy';
  var EVENT_KEY = '.' + DATA_KEY;
  var DATA_API_KEY = '.data-api';
  var JQUERY_NO_CONFLICT = $.fn[NAME];

  var Default = {
    offset: 10,
    method: 'auto',
    target: ''
  };

  var DefaultType = {
    offset: 'number',
    method: 'string',
    target: '(string|element)'
  };

  var Event = {
    ACTIVATE: 'activate' + EVENT_KEY,
    SCROLL: 'scroll' + EVENT_KEY,
    LOAD_DATA_API: 'load' + EVENT_KEY + DATA_API_KEY
  };

  var ClassName = {
    DROPDOWN_ITEM: 'dropdown-item',
    DROPDOWN_MENU: 'dropdown-menu',
    NAV_LINK: 'nav-link',
    NAV: 'nav',
    ACTIVE: 'active'
  };

  var Selector = {
    DATA_SPY: '[data-spy="scroll"]',
    ACTIVE: '.active',
    LIST_ITEM: '.list-item',
    LI: 'li',
    LI_DROPDOWN: 'li.dropdown',
    NAV_LINKS: '.nav-link',
    DROPDOWN: '.dropdown',
    DROPDOWN_ITEMS: '.dropdown-item',
    DROPDOWN_TOGGLE: '.dropdown-toggle'
  };

  var OffsetMethod = {
    OFFSET: 'offset',
    POSITION: 'position'
  };

  /**
   * ------------------------------------------------------------------------
   * Class Definition
   * ------------------------------------------------------------------------
   */

  var ScrollSpy = (function () {
    function ScrollSpy(element, config) {
      _classCallCheck(this, ScrollSpy);

      this._element = element;
      this._scrollElement = element.tagName === 'BODY' ? window : element;
      this._config = this._getConfig(config);
      this._selector = this._config.target + ' ' + Selector.NAV_LINKS + ',' + (this._config.target + ' ' + Selector.DROPDOWN_ITEMS);
      this._offsets = [];
      this._targets = [];
      this._activeTarget = null;
      this._scrollHeight = 0;

      $(this._scrollElement).on(Event.SCROLL, $.proxy(this._process, this));

      this.refresh();
      this._process();
    }

    /**
     * ------------------------------------------------------------------------
     * Data Api implementation
     * ------------------------------------------------------------------------
     */

    // getters

    _createClass(ScrollSpy, [{
      key: 'refresh',

      // public

      value: function refresh() {
        var _this = this;

        var autoMethod = this._scrollElement !== this._scrollElement.window ? OffsetMethod.POSITION : OffsetMethod.OFFSET;

        var offsetMethod = this._config.method === 'auto' ? autoMethod : this._config.method;

        var offsetBase = offsetMethod === OffsetMethod.POSITION ? this._getScrollTop() : 0;

        this._offsets = [];
        this._targets = [];

        this._scrollHeight = this._getScrollHeight();

        var targets = $.makeArray($(this._selector));

        targets.map(function (element) {
          var target = undefined;
          var targetSelector = Util.getSelectorFromElement(element);

          if (targetSelector) {
            target = $(targetSelector)[0];
          }

          if (target && (target.offsetWidth || target.offsetHeight)) {
            // todo (fat): remove sketch reliance on jQuery position/offset
            return [$(target)[offsetMethod]().top + offsetBase, targetSelector];
          }
        }).filter(function (item) {
          return item;
        }).sort(function (a, b) {
          return a[0] - b[0];
        }).forEach(function (item) {
          _this._offsets.push(item[0]);
          _this._targets.push(item[1]);
        });
      }
    }, {
      key: 'dispose',
      value: function dispose() {
        $.removeData(this._element, DATA_KEY);
        $(this._scrollElement).off(EVENT_KEY);

        this._element = null;
        this._scrollElement = null;
        this._config = null;
        this._selector = null;
        this._offsets = null;
        this._targets = null;
        this._activeTarget = null;
        this._scrollHeight = null;
      }

      // private

    }, {
      key: '_getConfig',
      value: function _getConfig(config) {
        config = $.extend({}, Default, config);

        if (typeof config.target !== 'string') {
          var id = $(config.target).attr('id');
          if (!id) {
            id = Util.getUID(NAME);
            $(config.target).attr('id', id);
          }
          config.target = '#' + id;
        }

        Util.typeCheckConfig(NAME, config, DefaultType);

        return config;
      }
    }, {
      key: '_getScrollTop',
      value: function _getScrollTop() {
        return this._scrollElement === window ? this._scrollElement.scrollY : this._scrollElement.scrollTop;
      }
    }, {
      key: '_getScrollHeight',
      value: function _getScrollHeight() {
        return this._scrollElement.scrollHeight || Math.max(document.body.scrollHeight, document.documentElement.scrollHeight);
      }
    }, {
      key: '_process',
      value: function _process() {
        var scrollTop = this._getScrollTop() + this._config.offset;
        var scrollHeight = this._getScrollHeight();
        var maxScroll = this._config.offset + scrollHeight - this._scrollElement.offsetHeight;

        if (this._scrollHeight !== scrollHeight) {
          this.refresh();
        }

        if (scrollTop >= maxScroll) {
          var target = this._targets[this._targets.length - 1];

          if (this._activeTarget !== target) {
            this._activate(target);
          }
        }

        if (this._activeTarget && scrollTop < this._offsets[0]) {
          this._activeTarget = null;
          this._clear();
          return;
        }

        for (var i = this._offsets.length; i--;) {
          var isActiveTarget = this._activeTarget !== this._targets[i] && scrollTop >= this._offsets[i] && (this._offsets[i + 1] === undefined || scrollTop < this._offsets[i + 1]);

          if (isActiveTarget) {
            this._activate(this._targets[i]);
          }
        }
      }
    }, {
      key: '_activate',
      value: function _activate(target) {
        this._activeTarget = target;

        this._clear();

        var queries = this._selector.split(',');
        queries = queries.map(function (selector) {
          return selector + '[data-target="' + target + '"],' + (selector + '[href="' + target + '"]');
        });

        var $link = $(queries.join(','));

        if ($link.hasClass(ClassName.DROPDOWN_ITEM)) {
          $link.closest(Selector.DROPDOWN).find(Selector.DROPDOWN_TOGGLE).addClass(ClassName.ACTIVE);
          $link.addClass(ClassName.ACTIVE);
        } else {
          // todo (fat) this is kinda sus…
          // recursively add actives to tested nav-links
          $link.parents(Selector.LI).find(Selector.NAV_LINKS).addClass(ClassName.ACTIVE);
        }

        $(this._scrollElement).trigger(Event.ACTIVATE, {
          relatedTarget: target
        });
      }
    }, {
      key: '_clear',
      value: function _clear() {
        $(this._selector).filter(Selector.ACTIVE).removeClass(ClassName.ACTIVE);
      }

      // static

    }], [{
      key: '_jQueryInterface',
      value: function _jQueryInterface(config) {
        return this.each(function () {
          var data = $(this).data(DATA_KEY);
          var _config = typeof config === 'object' && config || null;

          if (!data) {
            data = new ScrollSpy(this, _config);
            $(this).data(DATA_KEY, data);
          }

          if (typeof config === 'string') {
            if (data[config] === undefined) {
              throw new Error('No method named "' + config + '"');
            }
            data[config]();
          }
        });
      }
    }, {
      key: 'VERSION',
      get: function get() {
        return VERSION;
      }
    }, {
      key: 'Default',
      get: function get() {
        return Default;
      }
    }]);

    return ScrollSpy;
  })();

  $(window).on(Event.LOAD_DATA_API, function () {
    var scrollSpys = $.makeArray($(Selector.DATA_SPY));

    for (var i = scrollSpys.length; i--;) {
      var $spy = $(scrollSpys[i]);
      ScrollSpy._jQueryInterface.call($spy, $spy.data());
    }
  });

  /**
   * ------------------------------------------------------------------------
   * jQuery
   * ------------------------------------------------------------------------
   */

  $.fn[NAME] = ScrollSpy._jQueryInterface;
  $.fn[NAME].Constructor = ScrollSpy;
  $.fn[NAME].noConflict = function () {
    $.fn[NAME] = JQUERY_NO_CONFLICT;
    return ScrollSpy._jQueryInterface;
  };

  return ScrollSpy;
})(jQuery);
//# sourceMappingURL=scrollspy.js.map

'use strict';

var _createClass = (function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ('value' in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; })();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError('Cannot call a class as a function'); } }

/**
 * --------------------------------------------------------------------------
 * Bootstrap (v4.0.0-alpha.2): tab.js
 * Licensed under MIT (https://github.com/twbs/bootstrap/blob/master/LICENSE)
 * --------------------------------------------------------------------------
 */

var Tab = (function ($) {

  /**
   * ------------------------------------------------------------------------
   * Constants
   * ------------------------------------------------------------------------
   */

  var NAME = 'tab';
  var VERSION = '4.0.0-alpha';
  var DATA_KEY = 'bs.tab';
  var EVENT_KEY = '.' + DATA_KEY;
  var DATA_API_KEY = '.data-api';
  var JQUERY_NO_CONFLICT = $.fn[NAME];
  var TRANSITION_DURATION = 150;

  var Event = {
    HIDE: 'hide' + EVENT_KEY,
    HIDDEN: 'hidden' + EVENT_KEY,
    SHOW: 'show' + EVENT_KEY,
    SHOWN: 'shown' + EVENT_KEY,
    CLICK_DATA_API: 'click' + EVENT_KEY + DATA_API_KEY
  };

  var ClassName = {
    DROPDOWN_MENU: 'dropdown-menu',
    ACTIVE: 'active',
    FADE: 'fade',
    IN: 'in'
  };

  var Selector = {
    A: 'a',
    LI: 'li',
    DROPDOWN: '.dropdown',
    UL: 'ul:not(.dropdown-menu)',
    FADE_CHILD: '> .nav-item .fade, > .fade',
    ACTIVE: '.active',
    ACTIVE_CHILD: '> .nav-item > .active, > .active',
    DATA_TOGGLE: '[data-toggle="tab"], [data-toggle="pill"]',
    DROPDOWN_TOGGLE: '.dropdown-toggle',
    DROPDOWN_ACTIVE_CHILD: '> .dropdown-menu .active'
  };

  /**
   * ------------------------------------------------------------------------
   * Class Definition
   * ------------------------------------------------------------------------
   */

  var Tab = (function () {
    function Tab(element) {
      _classCallCheck(this, Tab);

      this._element = element;
    }

    /**
     * ------------------------------------------------------------------------
     * Data Api implementation
     * ------------------------------------------------------------------------
     */

    // getters

    _createClass(Tab, [{
      key: 'show',

      // public

      value: function show() {
        var _this = this;

        if (this._element.parentNode && this._element.parentNode.nodeType === Node.ELEMENT_NODE && $(this._element).hasClass(ClassName.ACTIVE)) {
          return;
        }

        var target = undefined;
        var previous = undefined;
        var ulElement = $(this._element).closest(Selector.UL)[0];
        var selector = Util.getSelectorFromElement(this._element);

        if (ulElement) {
          previous = $.makeArray($(ulElement).find(Selector.ACTIVE));
          previous = previous[previous.length - 1];
        }

        var hideEvent = $.Event(Event.HIDE, {
          relatedTarget: this._element
        });

        var showEvent = $.Event(Event.SHOW, {
          relatedTarget: previous
        });

        if (previous) {
          $(previous).trigger(hideEvent);
        }

        $(this._element).trigger(showEvent);

        if (showEvent.isDefaultPrevented() || hideEvent.isDefaultPrevented()) {
          return;
        }

        if (selector) {
          target = $(selector)[0];
        }

        this._activate(this._element, ulElement);

        var complete = function complete() {
          var hiddenEvent = $.Event(Event.HIDDEN, {
            relatedTarget: _this._element
          });

          var shownEvent = $.Event(Event.SHOWN, {
            relatedTarget: previous
          });

          $(previous).trigger(hiddenEvent);
          $(_this._element).trigger(shownEvent);
        };

        if (target) {
          this._activate(target, target.parentNode, complete);
        } else {
          complete();
        }
      }
    }, {
      key: 'dispose',
      value: function dispose() {
        $.removeClass(this._element, DATA_KEY);
        this._element = null;
      }

      // private

    }, {
      key: '_activate',
      value: function _activate(element, container, callback) {
        var active = $(container).find(Selector.ACTIVE_CHILD)[0];
        var isTransitioning = callback && Util.supportsTransitionEnd() && (active && $(active).hasClass(ClassName.FADE) || Boolean($(container).find(Selector.FADE_CHILD)[0]));

        var complete = $.proxy(this._transitionComplete, this, element, active, isTransitioning, callback);

        if (active && isTransitioning) {
          $(active).one(Util.TRANSITION_END, complete).emulateTransitionEnd(TRANSITION_DURATION);
        } else {
          complete();
        }

        if (active) {
          $(active).removeClass(ClassName.IN);
        }
      }
    }, {
      key: '_transitionComplete',
      value: function _transitionComplete(element, active, isTransitioning, callback) {
        if (active) {
          $(active).removeClass(ClassName.ACTIVE);

          var dropdownChild = $(active).find(Selector.DROPDOWN_ACTIVE_CHILD)[0];

          if (dropdownChild) {
            $(dropdownChild).removeClass(ClassName.ACTIVE);
          }

          active.setAttribute('aria-expanded', false);
        }

        $(element).addClass(ClassName.ACTIVE);
        element.setAttribute('aria-expanded', true);

        if (isTransitioning) {
          Util.reflow(element);
          $(element).addClass(ClassName.IN);
        } else {
          $(element).removeClass(ClassName.FADE);
        }

        if (element.parentNode && $(element.parentNode).hasClass(ClassName.DROPDOWN_MENU)) {

          var dropdownElement = $(element).closest(Selector.DROPDOWN)[0];
          if (dropdownElement) {
            $(dropdownElement).find(Selector.DROPDOWN_TOGGLE).addClass(ClassName.ACTIVE);
          }

          element.setAttribute('aria-expanded', true);
        }

        if (callback) {
          callback();
        }
      }

      // static

    }], [{
      key: '_jQueryInterface',
      value: function _jQueryInterface(config) {
        return this.each(function () {
          var $this = $(this);
          var data = $this.data(DATA_KEY);

          if (!data) {
            data = data = new Tab(this);
            $this.data(DATA_KEY, data);
          }

          if (typeof config === 'string') {
            if (data[config] === undefined) {
              throw new Error('No method named "' + config + '"');
            }
            data[config]();
          }
        });
      }
    }, {
      key: 'VERSION',
      get: function get() {
        return VERSION;
      }
    }]);

    return Tab;
  })();

  $(document).on(Event.CLICK_DATA_API, Selector.DATA_TOGGLE, function (event) {
    event.preventDefault();
    Tab._jQueryInterface.call($(this), 'show');
  });

  /**
   * ------------------------------------------------------------------------
   * jQuery
   * ------------------------------------------------------------------------
   */

  $.fn[NAME] = Tab._jQueryInterface;
  $.fn[NAME].Constructor = Tab;
  $.fn[NAME].noConflict = function () {
    $.fn[NAME] = JQUERY_NO_CONFLICT;
    return Tab._jQueryInterface;
  };

  return Tab;
})(jQuery);
//# sourceMappingURL=tab.js.map

/* global Tether */

'use strict';

var _createClass = (function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ('value' in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; })();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError('Cannot call a class as a function'); } }

/**
 * --------------------------------------------------------------------------
 * Bootstrap (v4.0.0-alpha.2): tooltip.js
 * Licensed under MIT (https://github.com/twbs/bootstrap/blob/master/LICENSE)
 * --------------------------------------------------------------------------
 */

var Tooltip = (function ($) {

  /**
   * Check for Tether dependency
   * Tether - http://github.hubspot.com/tether/
   */
  if (window.Tether === undefined) {
    throw new Error('Bootstrap tooltips require Tether (http://github.hubspot.com/tether/)');
  }

  /**
   * ------------------------------------------------------------------------
   * Constants
   * ------------------------------------------------------------------------
   */

  var NAME = 'tooltip';
  var VERSION = '4.0.0-alpha';
  var DATA_KEY = 'bs.tooltip';
  var EVENT_KEY = '.' + DATA_KEY;
  var JQUERY_NO_CONFLICT = $.fn[NAME];
  var TRANSITION_DURATION = 150;
  var CLASS_PREFIX = 'bs-tether';

  var Default = {
    animation: true,
    template: '<div class="tooltip" role="tooltip">' + '<div class="tooltip-arrow"></div>' + '<div class="tooltip-inner"></div></div>',
    trigger: 'hover focus',
    title: '',
    delay: 0,
    html: false,
    selector: false,
    placement: 'top',
    offset: '0 0',
    constraints: []
  };

  var DefaultType = {
    animation: 'boolean',
    template: 'string',
    title: '(string|element|function)',
    trigger: 'string',
    delay: '(number|object)',
    html: 'boolean',
    selector: '(string|boolean)',
    placement: '(string|function)',
    offset: 'string',
    constraints: 'array'
  };

  var AttachmentMap = {
    TOP: 'bottom center',
    RIGHT: 'middle left',
    BOTTOM: 'top center',
    LEFT: 'middle right'
  };

  var HoverState = {
    IN: 'in',
    OUT: 'out'
  };

  var Event = {
    HIDE: 'hide' + EVENT_KEY,
    HIDDEN: 'hidden' + EVENT_KEY,
    SHOW: 'show' + EVENT_KEY,
    SHOWN: 'shown' + EVENT_KEY,
    INSERTED: 'inserted' + EVENT_KEY,
    CLICK: 'click' + EVENT_KEY,
    FOCUSIN: 'focusin' + EVENT_KEY,
    FOCUSOUT: 'focusout' + EVENT_KEY,
    MOUSEENTER: 'mouseenter' + EVENT_KEY,
    MOUSELEAVE: 'mouseleave' + EVENT_KEY
  };

  var ClassName = {
    FADE: 'fade',
    IN: 'in'
  };

  var Selector = {
    TOOLTIP: '.tooltip',
    TOOLTIP_INNER: '.tooltip-inner'
  };

  var TetherClass = {
    element: false,
    enabled: false
  };

  var Trigger = {
    HOVER: 'hover',
    FOCUS: 'focus',
    CLICK: 'click',
    MANUAL: 'manual'
  };

  /**
   * ------------------------------------------------------------------------
   * Class Definition
   * ------------------------------------------------------------------------
   */

  var Tooltip = (function () {
    function Tooltip(element, config) {
      _classCallCheck(this, Tooltip);

      // private
      this._isEnabled = true;
      this._timeout = 0;
      this._hoverState = '';
      this._activeTrigger = {};
      this._tether = null;

      // protected
      this.element = element;
      this.config = this._getConfig(config);
      this.tip = null;

      this._setListeners();
    }

    /**
     * ------------------------------------------------------------------------
     * jQuery
     * ------------------------------------------------------------------------
     */

    // getters

    _createClass(Tooltip, [{
      key: 'enable',

      // public

      value: function enable() {
        this._isEnabled = true;
      }
    }, {
      key: 'disable',
      value: function disable() {
        this._isEnabled = false;
      }
    }, {
      key: 'toggleEnabled',
      value: function toggleEnabled() {
        this._isEnabled = !this._isEnabled;
      }
    }, {
      key: 'toggle',
      value: function toggle(event) {
        if (event) {
          var dataKey = this.constructor.DATA_KEY;
          var context = $(event.currentTarget).data(dataKey);

          if (!context) {
            context = new this.constructor(event.currentTarget, this._getDelegateConfig());
            $(event.currentTarget).data(dataKey, context);
          }

          context._activeTrigger.click = !context._activeTrigger.click;

          if (context._isWithActiveTrigger()) {
            context._enter(null, context);
          } else {
            context._leave(null, context);
          }
        } else {

          if ($(this.getTipElement()).hasClass(ClassName.IN)) {
            this._leave(null, this);
            return;
          }

          this._enter(null, this);
        }
      }
    }, {
      key: 'dispose',
      value: function dispose() {
        clearTimeout(this._timeout);

        this.cleanupTether();

        $.removeData(this.element, this.constructor.DATA_KEY);

        $(this.element).off(this.constructor.EVENT_KEY);

        if (this.tip) {
          $(this.tip).remove();
        }

        this._isEnabled = null;
        this._timeout = null;
        this._hoverState = null;
        this._activeTrigger = null;
        this._tether = null;

        this.element = null;
        this.config = null;
        this.tip = null;
      }
    }, {
      key: 'show',
      value: function show() {
        var _this = this;

        var showEvent = $.Event(this.constructor.Event.SHOW);

        if (this.isWithContent() && this._isEnabled) {
          $(this.element).trigger(showEvent);

          var isInTheDom = $.contains(this.element.ownerDocument.documentElement, this.element);

          if (showEvent.isDefaultPrevented() || !isInTheDom) {
            return;
          }

          var tip = this.getTipElement();
          var tipId = Util.getUID(this.constructor.NAME);

          tip.setAttribute('id', tipId);
          this.element.setAttribute('aria-describedby', tipId);

          this.setContent();

          if (this.config.animation) {
            $(tip).addClass(ClassName.FADE);
          }

          var placement = typeof this.config.placement === 'function' ? this.config.placement.call(this, tip, this.element) : this.config.placement;

          var attachment = this._getAttachment(placement);

          $(tip).data(this.constructor.DATA_KEY, this).appendTo(document.body);

          $(this.element).trigger(this.constructor.Event.INSERTED);

          this._tether = new Tether({
            attachment: attachment,
            element: tip,
            target: this.element,
            classes: TetherClass,
            classPrefix: CLASS_PREFIX,
            offset: this.config.offset,
            constraints: this.config.constraints,
            addTargetClasses: false
          });

          Util.reflow(tip);
          this._tether.position();

          $(tip).addClass(ClassName.IN);

          var complete = function complete() {
            var prevHoverState = _this._hoverState;
            _this._hoverState = null;

            $(_this.element).trigger(_this.constructor.Event.SHOWN);

            if (prevHoverState === HoverState.OUT) {
              _this._leave(null, _this);
            }
          };

          if (Util.supportsTransitionEnd() && $(this.tip).hasClass(ClassName.FADE)) {
            $(this.tip).one(Util.TRANSITION_END, complete).emulateTransitionEnd(Tooltip._TRANSITION_DURATION);
            return;
          }

          complete();
        }
      }
    }, {
      key: 'hide',
      value: function hide(callback) {
        var _this2 = this;

        var tip = this.getTipElement();
        var hideEvent = $.Event(this.constructor.Event.HIDE);
        var complete = function complete() {
          if (_this2._hoverState !== HoverState.IN && tip.parentNode) {
            tip.parentNode.removeChild(tip);
          }

          _this2.element.removeAttribute('aria-describedby');
          $(_this2.element).trigger(_this2.constructor.Event.HIDDEN);
          _this2.cleanupTether();

          if (callback) {
            callback();
          }
        };

        $(this.element).trigger(hideEvent);

        if (hideEvent.isDefaultPrevented()) {
          return;
        }

        $(tip).removeClass(ClassName.IN);

        if (Util.supportsTransitionEnd() && $(this.tip).hasClass(ClassName.FADE)) {

          $(tip).one(Util.TRANSITION_END, complete).emulateTransitionEnd(TRANSITION_DURATION);
        } else {
          complete();
        }

        this._hoverState = '';
      }

      // protected

    }, {
      key: 'isWithContent',
      value: function isWithContent() {
        return Boolean(this.getTitle());
      }
    }, {
      key: 'getTipElement',
      value: function getTipElement() {
        return this.tip = this.tip || $(this.config.template)[0];
      }
    }, {
      key: 'setContent',
      value: function setContent() {
        var $tip = $(this.getTipElement());

        this.setElementContent($tip.find(Selector.TOOLTIP_INNER), this.getTitle());

        $tip.removeClass(ClassName.FADE).removeClass(ClassName.IN);

        this.cleanupTether();
      }
    }, {
      key: 'setElementContent',
      value: function setElementContent($element, content) {
        var html = this.config.html;
        if (typeof content === 'object' && (content.nodeType || content.jquery)) {
          // content is a DOM node or a jQuery
          if (html) {
            if (!$(content).parent().is($element)) {
              $element.empty().append(content);
            }
          } else {
            $element.text($(content).text());
          }
        } else {
          $element[html ? 'html' : 'text'](content);
        }
      }
    }, {
      key: 'getTitle',
      value: function getTitle() {
        var title = this.element.getAttribute('data-original-title');

        if (!title) {
          title = typeof this.config.title === 'function' ? this.config.title.call(this.element) : this.config.title;
        }

        return title;
      }
    }, {
      key: 'cleanupTether',
      value: function cleanupTether() {
        if (this._tether) {
          this._tether.destroy();
        }
      }

      // private

    }, {
      key: '_getAttachment',
      value: function _getAttachment(placement) {
        return AttachmentMap[placement.toUpperCase()];
      }
    }, {
      key: '_setListeners',
      value: function _setListeners() {
        var _this3 = this;

        var triggers = this.config.trigger.split(' ');

        triggers.forEach(function (trigger) {
          if (trigger === 'click') {
            $(_this3.element).on(_this3.constructor.Event.CLICK, _this3.config.selector, $.proxy(_this3.toggle, _this3));
          } else if (trigger !== Trigger.MANUAL) {
            var eventIn = trigger === Trigger.HOVER ? _this3.constructor.Event.MOUSEENTER : _this3.constructor.Event.FOCUSIN;
            var eventOut = trigger === Trigger.HOVER ? _this3.constructor.Event.MOUSELEAVE : _this3.constructor.Event.FOCUSOUT;

            $(_this3.element).on(eventIn, _this3.config.selector, $.proxy(_this3._enter, _this3)).on(eventOut, _this3.config.selector, $.proxy(_this3._leave, _this3));
          }
        });

        if (this.config.selector) {
          this.config = $.extend({}, this.config, {
            trigger: 'manual',
            selector: ''
          });
        } else {
          this._fixTitle();
        }
      }
    }, {
      key: '_fixTitle',
      value: function _fixTitle() {
        var titleType = typeof this.element.getAttribute('data-original-title');
        if (this.element.getAttribute('title') || titleType !== 'string') {
          this.element.setAttribute('data-original-title', this.element.getAttribute('title') || '');
          this.element.setAttribute('title', '');
        }
      }
    }, {
      key: '_enter',
      value: function _enter(event, context) {
        var dataKey = this.constructor.DATA_KEY;

        context = context || $(event.currentTarget).data(dataKey);

        if (!context) {
          context = new this.constructor(event.currentTarget, this._getDelegateConfig());
          $(event.currentTarget).data(dataKey, context);
        }

        if (event) {
          context._activeTrigger[event.type === 'focusin' ? Trigger.FOCUS : Trigger.HOVER] = true;
        }

        if ($(context.getTipElement()).hasClass(ClassName.IN) || context._hoverState === HoverState.IN) {
          context._hoverState = HoverState.IN;
          return;
        }

        clearTimeout(context._timeout);

        context._hoverState = HoverState.IN;

        if (!context.config.delay || !context.config.delay.show) {
          context.show();
          return;
        }

        context._timeout = setTimeout(function () {
          if (context._hoverState === HoverState.IN) {
            context.show();
          }
        }, context.config.delay.show);
      }
    }, {
      key: '_leave',
      value: function _leave(event, context) {
        var dataKey = this.constructor.DATA_KEY;

        context = context || $(event.currentTarget).data(dataKey);

        if (!context) {
          context = new this.constructor(event.currentTarget, this._getDelegateConfig());
          $(event.currentTarget).data(dataKey, context);
        }

        if (event) {
          context._activeTrigger[event.type === 'focusout' ? Trigger.FOCUS : Trigger.HOVER] = false;
        }

        if (context._isWithActiveTrigger()) {
          return;
        }

        clearTimeout(context._timeout);

        context._hoverState = HoverState.OUT;

        if (!context.config.delay || !context.config.delay.hide) {
          context.hide();
          return;
        }

        context._timeout = setTimeout(function () {
          if (context._hoverState === HoverState.OUT) {
            context.hide();
          }
        }, context.config.delay.hide);
      }
    }, {
      key: '_isWithActiveTrigger',
      value: function _isWithActiveTrigger() {
        for (var trigger in this._activeTrigger) {
          if (this._activeTrigger[trigger]) {
            return true;
          }
        }

        return false;
      }
    }, {
      key: '_getConfig',
      value: function _getConfig(config) {
        config = $.extend({}, this.constructor.Default, $(this.element).data(), config);

        if (config.delay && typeof config.delay === 'number') {
          config.delay = {
            show: config.delay,
            hide: config.delay
          };
        }

        Util.typeCheckConfig(NAME, config, this.constructor.DefaultType);

        return config;
      }
    }, {
      key: '_getDelegateConfig',
      value: function _getDelegateConfig() {
        var config = {};

        if (this.config) {
          for (var key in this.config) {
            if (this.constructor.Default[key] !== this.config[key]) {
              config[key] = this.config[key];
            }
          }
        }

        return config;
      }

      // static

    }], [{
      key: '_jQueryInterface',
      value: function _jQueryInterface(config) {
        return this.each(function () {
          var data = $(this).data(DATA_KEY);
          var _config = typeof config === 'object' ? config : null;

          if (!data && /destroy|hide/.test(config)) {
            return;
          }

          if (!data) {
            data = new Tooltip(this, _config);
            $(this).data(DATA_KEY, data);
          }

          if (typeof config === 'string') {
            if (data[config] === undefined) {
              throw new Error('No method named "' + config + '"');
            }
            data[config]();
          }
        });
      }
    }, {
      key: 'VERSION',
      get: function get() {
        return VERSION;
      }
    }, {
      key: 'Default',
      get: function get() {
        return Default;
      }
    }, {
      key: 'NAME',
      get: function get() {
        return NAME;
      }
    }, {
      key: 'DATA_KEY',
      get: function get() {
        return DATA_KEY;
      }
    }, {
      key: 'Event',
      get: function get() {
        return Event;
      }
    }, {
      key: 'EVENT_KEY',
      get: function get() {
        return EVENT_KEY;
      }
    }, {
      key: 'DefaultType',
      get: function get() {
        return DefaultType;
      }
    }]);

    return Tooltip;
  })();

  $.fn[NAME] = Tooltip._jQueryInterface;
  $.fn[NAME].Constructor = Tooltip;
  $.fn[NAME].noConflict = function () {
    $.fn[NAME] = JQUERY_NO_CONFLICT;
    return Tooltip._jQueryInterface;
  };

  return Tooltip;
})(jQuery);
//# sourceMappingURL=tooltip.js.map

/**
 * --------------------------------------------------------------------------
 * Bootstrap (v4.0.0-alpha.2): util.js
 * Licensed under MIT (https://github.com/twbs/bootstrap/blob/master/LICENSE)
 * --------------------------------------------------------------------------
 */

'use strict';

var Util = (function ($) {

  /**
   * ------------------------------------------------------------------------
   * Private TransitionEnd Helpers
   * ------------------------------------------------------------------------
   */

  var transition = false;

  var TransitionEndEvent = {
    WebkitTransition: 'webkitTransitionEnd',
    MozTransition: 'transitionend',
    OTransition: 'oTransitionEnd otransitionend',
    transition: 'transitionend'
  };

  // shoutout AngusCroll (https://goo.gl/pxwQGp)
  function toType(obj) {
    return ({}).toString.call(obj).match(/\s([a-zA-Z]+)/)[1].toLowerCase();
  }

  function isElement(obj) {
    return (obj[0] || obj).nodeType;
  }

  function getSpecialTransitionEndEvent() {
    return {
      bindType: transition.end,
      delegateType: transition.end,
      handle: function handle(event) {
        if ($(event.target).is(this)) {
          return event.handleObj.handler.apply(this, arguments);
        }
      }
    };
  }

  function transitionEndTest() {
    if (window.QUnit) {
      return false;
    }

    var el = document.createElement('bootstrap');

    for (var _name in TransitionEndEvent) {
      if (el.style[_name] !== undefined) {
        return { end: TransitionEndEvent[_name] };
      }
    }

    return false;
  }

  function transitionEndEmulator(duration) {
    var _this = this;

    var called = false;

    $(this).one(Util.TRANSITION_END, function () {
      called = true;
    });

    setTimeout(function () {
      if (!called) {
        Util.triggerTransitionEnd(_this);
      }
    }, duration);

    return this;
  }

  function setTransitionEndSupport() {
    transition = transitionEndTest();

    $.fn.emulateTransitionEnd = transitionEndEmulator;

    if (Util.supportsTransitionEnd()) {
      $.event.special[Util.TRANSITION_END] = getSpecialTransitionEndEvent();
    }
  }

  /**
   * --------------------------------------------------------------------------
   * Public Util Api
   * --------------------------------------------------------------------------
   */

  var Util = {

    TRANSITION_END: 'bsTransitionEnd',

    getUID: function getUID(prefix) {
      do {
        prefix += ~ ~(Math.random() * 1000000); // "~~" acts like a faster Math.floor() here
      } while (document.getElementById(prefix));
      return prefix;
    },

    getSelectorFromElement: function getSelectorFromElement(element) {
      var selector = element.getAttribute('data-target');

      if (!selector) {
        selector = element.getAttribute('href') || '';
        selector = /^#[a-z]/i.test(selector) ? selector : null;
      }

      return selector;
    },

    reflow: function reflow(element) {
      new Function('bs', 'return bs')(element.offsetHeight);
    },

    triggerTransitionEnd: function triggerTransitionEnd(element) {
      $(element).trigger(transition.end);
    },

    supportsTransitionEnd: function supportsTransitionEnd() {
      return Boolean(transition);
    },

    typeCheckConfig: function typeCheckConfig(componentName, config, configTypes) {
      for (var property in configTypes) {
        if (configTypes.hasOwnProperty(property)) {
          var expectedTypes = configTypes[property];
          var value = config[property];
          var valueType = undefined;

          if (value && isElement(value)) {
            valueType = 'element';
          } else {
            valueType = toType(value);
          }

          if (!new RegExp(expectedTypes).test(valueType)) {
            throw new Error(componentName.toUpperCase() + ': ' + ('Option "' + property + '" provided type "' + valueType + '" ') + ('but expected type "' + expectedTypes + '".'));
          }
        }
      }
    }
  };

  setTransitionEndSupport();

  return Util;
})(jQuery);
//# sourceMappingURL=util.js.map

$(function () {
    $("body").on("click", "[itemprop~='RemoveUrl']", function () {
        // don't show the confirm dialog if the link is also UnsafeUrl, as it will already be handled in base.js
        if ($(this).filter("[itemprop~='UnsafeUrl']").length == 1) {
            return false;
        }

        // use a custom message if its set in data-message
        var dataMessage = $(this).data('message');
        if (dataMessage === undefined) {
            dataMessage = confirmRemoveMessage;
        }

        return confirm(dataMessage);
    });
})(jQuery);

$(function () {
    var magicToken = $("input[name=__RequestVerificationToken]").first();
    if (magicToken) {
        $("body").on("click", "a[itemprop~=UnsafeUrl], a[data-unsafe-url]", function () {
            var _this = $(this);
            var hrefParts = _this.attr("href").split("?");
            var form = $("<form action=\"" + hrefParts[0] + "\" method=\"POST\" />");
            form.append(magicToken.clone());
            if (hrefParts.length > 1) {
                var queryParts = hrefParts[1].split("&");
                for (var i = 0; i < queryParts.length; i++) {
                    var queryPartKVP = queryParts[i].split("=");
                    //trusting hrefs in the page here
                    form.append($("<input type=\"hidden\" name=\"" + decodeURIComponent(queryPartKVP[0]) + "\" value=\"" + decodeURIComponent(queryPartKVP[1]) + "\" />"));
                }
            }
            form.css({ "position": "absolute", "left": "-9999em" });
            $("body").append(form);

            var unsafeUrlPrompt = _this.data("unsafe-url");

            if (unsafeUrlPrompt && unsafeUrlPrompt.length > 0) {
                if (!confirm(unsafeUrlPrompt)) {
                    return false;
                }
            }

            if (_this.filter("[itemprop~='RemoveUrl']").length == 1) {
                // use a custom message if its set in data-message
                var dataMessage = _this.data('message');
                if (dataMessage === undefined) {
                    dataMessage = confirmRemoveMessage;
                }

                if (!confirm(dataMessage)) {
                    return false;
                }
            }

            form.submit();
            return false;

        });
    }
})(jQuery);
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImFsZXJ0LmpzIiwiYnV0dG9uLmpzIiwiY2Fyb3VzZWwuanMiLCJjb2xsYXBzZS5qcyIsImRyb3Bkb3duLmpzIiwibW9kYWwuanMiLCJwb3BvdmVyLmpzIiwic2Nyb2xsc3B5LmpzIiwidGFiLmpzIiwidG9vbHRpcC5qcyIsInV0aWwuanMiLCJhZG1pbi5qcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ2hNQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDNUtBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUM5ZEE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQzVXQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUNyU0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDeGhCQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQ3pNQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUNoVUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDdlFBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUMzbUJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUM3SkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSIsImZpbGUiOiJUaGVBZG1pbi5qcyIsInNvdXJjZXNDb250ZW50IjpbIid1c2Ugc3RyaWN0JztcclxuXHJcbnZhciBfY3JlYXRlQ2xhc3MgPSAoZnVuY3Rpb24gKCkgeyBmdW5jdGlvbiBkZWZpbmVQcm9wZXJ0aWVzKHRhcmdldCwgcHJvcHMpIHsgZm9yICh2YXIgaSA9IDA7IGkgPCBwcm9wcy5sZW5ndGg7IGkrKykgeyB2YXIgZGVzY3JpcHRvciA9IHByb3BzW2ldOyBkZXNjcmlwdG9yLmVudW1lcmFibGUgPSBkZXNjcmlwdG9yLmVudW1lcmFibGUgfHwgZmFsc2U7IGRlc2NyaXB0b3IuY29uZmlndXJhYmxlID0gdHJ1ZTsgaWYgKCd2YWx1ZScgaW4gZGVzY3JpcHRvcikgZGVzY3JpcHRvci53cml0YWJsZSA9IHRydWU7IE9iamVjdC5kZWZpbmVQcm9wZXJ0eSh0YXJnZXQsIGRlc2NyaXB0b3Iua2V5LCBkZXNjcmlwdG9yKTsgfSB9IHJldHVybiBmdW5jdGlvbiAoQ29uc3RydWN0b3IsIHByb3RvUHJvcHMsIHN0YXRpY1Byb3BzKSB7IGlmIChwcm90b1Byb3BzKSBkZWZpbmVQcm9wZXJ0aWVzKENvbnN0cnVjdG9yLnByb3RvdHlwZSwgcHJvdG9Qcm9wcyk7IGlmIChzdGF0aWNQcm9wcykgZGVmaW5lUHJvcGVydGllcyhDb25zdHJ1Y3Rvciwgc3RhdGljUHJvcHMpOyByZXR1cm4gQ29uc3RydWN0b3I7IH07IH0pKCk7XHJcblxyXG5mdW5jdGlvbiBfY2xhc3NDYWxsQ2hlY2soaW5zdGFuY2UsIENvbnN0cnVjdG9yKSB7IGlmICghKGluc3RhbmNlIGluc3RhbmNlb2YgQ29uc3RydWN0b3IpKSB7IHRocm93IG5ldyBUeXBlRXJyb3IoJ0Nhbm5vdCBjYWxsIGEgY2xhc3MgYXMgYSBmdW5jdGlvbicpOyB9IH1cclxuXHJcbi8qKlxyXG4gKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gKiBCb290c3RyYXAgKHY0LjAuMC1hbHBoYS4yKTogYWxlcnQuanNcclxuICogTGljZW5zZWQgdW5kZXIgTUlUIChodHRwczovL2dpdGh1Yi5jb20vdHdicy9ib290c3RyYXAvYmxvYi9tYXN0ZXIvTElDRU5TRSlcclxuICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICovXHJcblxyXG52YXIgQWxlcnQgPSAoZnVuY3Rpb24gKCQpIHtcclxuXHJcbiAgLyoqXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICogQ29uc3RhbnRzXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICovXHJcblxyXG4gIHZhciBOQU1FID0gJ2FsZXJ0JztcclxuICB2YXIgVkVSU0lPTiA9ICc0LjAuMC1hbHBoYSc7XHJcbiAgdmFyIERBVEFfS0VZID0gJ2JzLmFsZXJ0JztcclxuICB2YXIgRVZFTlRfS0VZID0gJy4nICsgREFUQV9LRVk7XHJcbiAgdmFyIERBVEFfQVBJX0tFWSA9ICcuZGF0YS1hcGknO1xyXG4gIHZhciBKUVVFUllfTk9fQ09ORkxJQ1QgPSAkLmZuW05BTUVdO1xyXG4gIHZhciBUUkFOU0lUSU9OX0RVUkFUSU9OID0gMTUwO1xyXG5cclxuICB2YXIgU2VsZWN0b3IgPSB7XHJcbiAgICBESVNNSVNTOiAnW2RhdGEtZGlzbWlzcz1cImFsZXJ0XCJdJ1xyXG4gIH07XHJcblxyXG4gIHZhciBFdmVudCA9IHtcclxuICAgIENMT1NFOiAnY2xvc2UnICsgRVZFTlRfS0VZLFxyXG4gICAgQ0xPU0VEOiAnY2xvc2VkJyArIEVWRU5UX0tFWSxcclxuICAgIENMSUNLX0RBVEFfQVBJOiAnY2xpY2snICsgRVZFTlRfS0VZICsgREFUQV9BUElfS0VZXHJcbiAgfTtcclxuXHJcbiAgdmFyIENsYXNzTmFtZSA9IHtcclxuICAgIEFMRVJUOiAnYWxlcnQnLFxyXG4gICAgRkFERTogJ2ZhZGUnLFxyXG4gICAgSU46ICdpbidcclxuICB9O1xyXG5cclxuICAvKipcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKiBDbGFzcyBEZWZpbml0aW9uXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICovXHJcblxyXG4gIHZhciBBbGVydCA9IChmdW5jdGlvbiAoKSB7XHJcbiAgICBmdW5jdGlvbiBBbGVydChlbGVtZW50KSB7XHJcbiAgICAgIF9jbGFzc0NhbGxDaGVjayh0aGlzLCBBbGVydCk7XHJcblxyXG4gICAgICB0aGlzLl9lbGVtZW50ID0gZWxlbWVudDtcclxuICAgIH1cclxuXHJcbiAgICAvKipcclxuICAgICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAgICogRGF0YSBBcGkgaW1wbGVtZW50YXRpb25cclxuICAgICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAgICovXHJcblxyXG4gICAgLy8gZ2V0dGVyc1xyXG5cclxuICAgIF9jcmVhdGVDbGFzcyhBbGVydCwgW3tcclxuICAgICAga2V5OiAnY2xvc2UnLFxyXG5cclxuICAgICAgLy8gcHVibGljXHJcblxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gY2xvc2UoZWxlbWVudCkge1xyXG4gICAgICAgIGVsZW1lbnQgPSBlbGVtZW50IHx8IHRoaXMuX2VsZW1lbnQ7XHJcblxyXG4gICAgICAgIHZhciByb290RWxlbWVudCA9IHRoaXMuX2dldFJvb3RFbGVtZW50KGVsZW1lbnQpO1xyXG4gICAgICAgIHZhciBjdXN0b21FdmVudCA9IHRoaXMuX3RyaWdnZXJDbG9zZUV2ZW50KHJvb3RFbGVtZW50KTtcclxuXHJcbiAgICAgICAgaWYgKGN1c3RvbUV2ZW50LmlzRGVmYXVsdFByZXZlbnRlZCgpKSB7XHJcbiAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB0aGlzLl9yZW1vdmVFbGVtZW50KHJvb3RFbGVtZW50KTtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdkaXNwb3NlJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIGRpc3Bvc2UoKSB7XHJcbiAgICAgICAgJC5yZW1vdmVEYXRhKHRoaXMuX2VsZW1lbnQsIERBVEFfS0VZKTtcclxuICAgICAgICB0aGlzLl9lbGVtZW50ID0gbnVsbDtcclxuICAgICAgfVxyXG5cclxuICAgICAgLy8gcHJpdmF0ZVxyXG5cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX2dldFJvb3RFbGVtZW50JyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9nZXRSb290RWxlbWVudChlbGVtZW50KSB7XHJcbiAgICAgICAgdmFyIHNlbGVjdG9yID0gVXRpbC5nZXRTZWxlY3RvckZyb21FbGVtZW50KGVsZW1lbnQpO1xyXG4gICAgICAgIHZhciBwYXJlbnQgPSBmYWxzZTtcclxuXHJcbiAgICAgICAgaWYgKHNlbGVjdG9yKSB7XHJcbiAgICAgICAgICBwYXJlbnQgPSAkKHNlbGVjdG9yKVswXTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGlmICghcGFyZW50KSB7XHJcbiAgICAgICAgICBwYXJlbnQgPSAkKGVsZW1lbnQpLmNsb3Nlc3QoJy4nICsgQ2xhc3NOYW1lLkFMRVJUKVswXTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHJldHVybiBwYXJlbnQ7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX3RyaWdnZXJDbG9zZUV2ZW50JyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF90cmlnZ2VyQ2xvc2VFdmVudChlbGVtZW50KSB7XHJcbiAgICAgICAgdmFyIGNsb3NlRXZlbnQgPSAkLkV2ZW50KEV2ZW50LkNMT1NFKTtcclxuXHJcbiAgICAgICAgJChlbGVtZW50KS50cmlnZ2VyKGNsb3NlRXZlbnQpO1xyXG4gICAgICAgIHJldHVybiBjbG9zZUV2ZW50O1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19yZW1vdmVFbGVtZW50JyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9yZW1vdmVFbGVtZW50KGVsZW1lbnQpIHtcclxuICAgICAgICAkKGVsZW1lbnQpLnJlbW92ZUNsYXNzKENsYXNzTmFtZS5JTik7XHJcblxyXG4gICAgICAgIGlmICghVXRpbC5zdXBwb3J0c1RyYW5zaXRpb25FbmQoKSB8fCAhJChlbGVtZW50KS5oYXNDbGFzcyhDbGFzc05hbWUuRkFERSkpIHtcclxuICAgICAgICAgIHRoaXMuX2Rlc3Ryb3lFbGVtZW50KGVsZW1lbnQpO1xyXG4gICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgJChlbGVtZW50KS5vbmUoVXRpbC5UUkFOU0lUSU9OX0VORCwgJC5wcm94eSh0aGlzLl9kZXN0cm95RWxlbWVudCwgdGhpcywgZWxlbWVudCkpLmVtdWxhdGVUcmFuc2l0aW9uRW5kKFRSQU5TSVRJT05fRFVSQVRJT04pO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19kZXN0cm95RWxlbWVudCcsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBfZGVzdHJveUVsZW1lbnQoZWxlbWVudCkge1xyXG4gICAgICAgICQoZWxlbWVudCkuZGV0YWNoKCkudHJpZ2dlcihFdmVudC5DTE9TRUQpLnJlbW92ZSgpO1xyXG4gICAgICB9XHJcblxyXG4gICAgICAvLyBzdGF0aWNcclxuXHJcbiAgICB9XSwgW3tcclxuICAgICAga2V5OiAnX2pRdWVyeUludGVyZmFjZScsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBfalF1ZXJ5SW50ZXJmYWNlKGNvbmZpZykge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmVhY2goZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgdmFyICRlbGVtZW50ID0gJCh0aGlzKTtcclxuICAgICAgICAgIHZhciBkYXRhID0gJGVsZW1lbnQuZGF0YShEQVRBX0tFWSk7XHJcblxyXG4gICAgICAgICAgaWYgKCFkYXRhKSB7XHJcbiAgICAgICAgICAgIGRhdGEgPSBuZXcgQWxlcnQodGhpcyk7XHJcbiAgICAgICAgICAgICRlbGVtZW50LmRhdGEoREFUQV9LRVksIGRhdGEpO1xyXG4gICAgICAgICAgfVxyXG5cclxuICAgICAgICAgIGlmIChjb25maWcgPT09ICdjbG9zZScpIHtcclxuICAgICAgICAgICAgZGF0YVtjb25maWddKHRoaXMpO1xyXG4gICAgICAgICAgfVxyXG4gICAgICAgIH0pO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19oYW5kbGVEaXNtaXNzJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9oYW5kbGVEaXNtaXNzKGFsZXJ0SW5zdGFuY2UpIHtcclxuICAgICAgICByZXR1cm4gZnVuY3Rpb24gKGV2ZW50KSB7XHJcbiAgICAgICAgICBpZiAoZXZlbnQpIHtcclxuICAgICAgICAgICAgZXZlbnQucHJldmVudERlZmF1bHQoKTtcclxuICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICBhbGVydEluc3RhbmNlLmNsb3NlKHRoaXMpO1xyXG4gICAgICAgIH07XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnVkVSU0lPTicsXHJcbiAgICAgIGdldDogZnVuY3Rpb24gZ2V0KCkge1xyXG4gICAgICAgIHJldHVybiBWRVJTSU9OO1xyXG4gICAgICB9XHJcbiAgICB9XSk7XHJcblxyXG4gICAgcmV0dXJuIEFsZXJ0O1xyXG4gIH0pKCk7XHJcblxyXG4gICQoZG9jdW1lbnQpLm9uKEV2ZW50LkNMSUNLX0RBVEFfQVBJLCBTZWxlY3Rvci5ESVNNSVNTLCBBbGVydC5faGFuZGxlRGlzbWlzcyhuZXcgQWxlcnQoKSkpO1xyXG5cclxuICAvKipcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKiBqUXVlcnlcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKi9cclxuXHJcbiAgJC5mbltOQU1FXSA9IEFsZXJ0Ll9qUXVlcnlJbnRlcmZhY2U7XHJcbiAgJC5mbltOQU1FXS5Db25zdHJ1Y3RvciA9IEFsZXJ0O1xyXG4gICQuZm5bTkFNRV0ubm9Db25mbGljdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICQuZm5bTkFNRV0gPSBKUVVFUllfTk9fQ09ORkxJQ1Q7XHJcbiAgICByZXR1cm4gQWxlcnQuX2pRdWVyeUludGVyZmFjZTtcclxuICB9O1xyXG5cclxuICByZXR1cm4gQWxlcnQ7XHJcbn0pKGpRdWVyeSk7XHJcbi8vIyBzb3VyY2VNYXBwaW5nVVJMPWFsZXJ0LmpzLm1hcFxyXG4iLCIvKipcclxuICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICogQm9vdHN0cmFwICh2NC4wLjAtYWxwaGEuMik6IGJ1dHRvbi5qc1xyXG4gKiBMaWNlbnNlZCB1bmRlciBNSVQgKGh0dHBzOi8vZ2l0aHViLmNvbS90d2JzL2Jvb3RzdHJhcC9ibG9iL21hc3Rlci9MSUNFTlNFKVxyXG4gKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gKi9cclxuXHJcbid1c2Ugc3RyaWN0JztcclxuXHJcbnZhciBfY3JlYXRlQ2xhc3MgPSAoZnVuY3Rpb24gKCkgeyBmdW5jdGlvbiBkZWZpbmVQcm9wZXJ0aWVzKHRhcmdldCwgcHJvcHMpIHsgZm9yICh2YXIgaSA9IDA7IGkgPCBwcm9wcy5sZW5ndGg7IGkrKykgeyB2YXIgZGVzY3JpcHRvciA9IHByb3BzW2ldOyBkZXNjcmlwdG9yLmVudW1lcmFibGUgPSBkZXNjcmlwdG9yLmVudW1lcmFibGUgfHwgZmFsc2U7IGRlc2NyaXB0b3IuY29uZmlndXJhYmxlID0gdHJ1ZTsgaWYgKCd2YWx1ZScgaW4gZGVzY3JpcHRvcikgZGVzY3JpcHRvci53cml0YWJsZSA9IHRydWU7IE9iamVjdC5kZWZpbmVQcm9wZXJ0eSh0YXJnZXQsIGRlc2NyaXB0b3Iua2V5LCBkZXNjcmlwdG9yKTsgfSB9IHJldHVybiBmdW5jdGlvbiAoQ29uc3RydWN0b3IsIHByb3RvUHJvcHMsIHN0YXRpY1Byb3BzKSB7IGlmIChwcm90b1Byb3BzKSBkZWZpbmVQcm9wZXJ0aWVzKENvbnN0cnVjdG9yLnByb3RvdHlwZSwgcHJvdG9Qcm9wcyk7IGlmIChzdGF0aWNQcm9wcykgZGVmaW5lUHJvcGVydGllcyhDb25zdHJ1Y3Rvciwgc3RhdGljUHJvcHMpOyByZXR1cm4gQ29uc3RydWN0b3I7IH07IH0pKCk7XHJcblxyXG5mdW5jdGlvbiBfY2xhc3NDYWxsQ2hlY2soaW5zdGFuY2UsIENvbnN0cnVjdG9yKSB7IGlmICghKGluc3RhbmNlIGluc3RhbmNlb2YgQ29uc3RydWN0b3IpKSB7IHRocm93IG5ldyBUeXBlRXJyb3IoJ0Nhbm5vdCBjYWxsIGEgY2xhc3MgYXMgYSBmdW5jdGlvbicpOyB9IH1cclxuXHJcbnZhciBCdXR0b24gPSAoZnVuY3Rpb24gKCQpIHtcclxuXHJcbiAgLyoqXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICogQ29uc3RhbnRzXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICovXHJcblxyXG4gIHZhciBOQU1FID0gJ2J1dHRvbic7XHJcbiAgdmFyIFZFUlNJT04gPSAnNC4wLjAtYWxwaGEnO1xyXG4gIHZhciBEQVRBX0tFWSA9ICdicy5idXR0b24nO1xyXG4gIHZhciBFVkVOVF9LRVkgPSAnLicgKyBEQVRBX0tFWTtcclxuICB2YXIgREFUQV9BUElfS0VZID0gJy5kYXRhLWFwaSc7XHJcbiAgdmFyIEpRVUVSWV9OT19DT05GTElDVCA9ICQuZm5bTkFNRV07XHJcblxyXG4gIHZhciBDbGFzc05hbWUgPSB7XHJcbiAgICBBQ1RJVkU6ICdhY3RpdmUnLFxyXG4gICAgQlVUVE9OOiAnYnRuJyxcclxuICAgIEZPQ1VTOiAnZm9jdXMnXHJcbiAgfTtcclxuXHJcbiAgdmFyIFNlbGVjdG9yID0ge1xyXG4gICAgREFUQV9UT0dHTEVfQ0FSUk9UOiAnW2RhdGEtdG9nZ2xlXj1cImJ1dHRvblwiXScsXHJcbiAgICBEQVRBX1RPR0dMRTogJ1tkYXRhLXRvZ2dsZT1cImJ1dHRvbnNcIl0nLFxyXG4gICAgSU5QVVQ6ICdpbnB1dCcsXHJcbiAgICBBQ1RJVkU6ICcuYWN0aXZlJyxcclxuICAgIEJVVFRPTjogJy5idG4nXHJcbiAgfTtcclxuXHJcbiAgdmFyIEV2ZW50ID0ge1xyXG4gICAgQ0xJQ0tfREFUQV9BUEk6ICdjbGljaycgKyBFVkVOVF9LRVkgKyBEQVRBX0FQSV9LRVksXHJcbiAgICBGT0NVU19CTFVSX0RBVEFfQVBJOiAnZm9jdXMnICsgRVZFTlRfS0VZICsgREFUQV9BUElfS0VZICsgJyAnICsgKCdibHVyJyArIEVWRU5UX0tFWSArIERBVEFfQVBJX0tFWSlcclxuICB9O1xyXG5cclxuICAvKipcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKiBDbGFzcyBEZWZpbml0aW9uXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICovXHJcblxyXG4gIHZhciBCdXR0b24gPSAoZnVuY3Rpb24gKCkge1xyXG4gICAgZnVuY3Rpb24gQnV0dG9uKGVsZW1lbnQpIHtcclxuICAgICAgX2NsYXNzQ2FsbENoZWNrKHRoaXMsIEJ1dHRvbik7XHJcblxyXG4gICAgICB0aGlzLl9lbGVtZW50ID0gZWxlbWVudDtcclxuICAgIH1cclxuXHJcbiAgICAvKipcclxuICAgICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAgICogRGF0YSBBcGkgaW1wbGVtZW50YXRpb25cclxuICAgICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAgICovXHJcblxyXG4gICAgLy8gZ2V0dGVyc1xyXG5cclxuICAgIF9jcmVhdGVDbGFzcyhCdXR0b24sIFt7XHJcbiAgICAgIGtleTogJ3RvZ2dsZScsXHJcblxyXG4gICAgICAvLyBwdWJsaWNcclxuXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiB0b2dnbGUoKSB7XHJcbiAgICAgICAgdmFyIHRyaWdnZXJDaGFuZ2VFdmVudCA9IHRydWU7XHJcbiAgICAgICAgdmFyIHJvb3RFbGVtZW50ID0gJCh0aGlzLl9lbGVtZW50KS5jbG9zZXN0KFNlbGVjdG9yLkRBVEFfVE9HR0xFKVswXTtcclxuXHJcbiAgICAgICAgaWYgKHJvb3RFbGVtZW50KSB7XHJcbiAgICAgICAgICB2YXIgaW5wdXQgPSAkKHRoaXMuX2VsZW1lbnQpLmZpbmQoU2VsZWN0b3IuSU5QVVQpWzBdO1xyXG5cclxuICAgICAgICAgIGlmIChpbnB1dCkge1xyXG4gICAgICAgICAgICBpZiAoaW5wdXQudHlwZSA9PT0gJ3JhZGlvJykge1xyXG4gICAgICAgICAgICAgIGlmIChpbnB1dC5jaGVja2VkICYmICQodGhpcy5fZWxlbWVudCkuaGFzQ2xhc3MoQ2xhc3NOYW1lLkFDVElWRSkpIHtcclxuICAgICAgICAgICAgICAgIHRyaWdnZXJDaGFuZ2VFdmVudCA9IGZhbHNlO1xyXG4gICAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgYWN0aXZlRWxlbWVudCA9ICQocm9vdEVsZW1lbnQpLmZpbmQoU2VsZWN0b3IuQUNUSVZFKVswXTtcclxuXHJcbiAgICAgICAgICAgICAgICBpZiAoYWN0aXZlRWxlbWVudCkge1xyXG4gICAgICAgICAgICAgICAgICAkKGFjdGl2ZUVsZW1lbnQpLnJlbW92ZUNsYXNzKENsYXNzTmFtZS5BQ1RJVkUpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgaWYgKHRyaWdnZXJDaGFuZ2VFdmVudCkge1xyXG4gICAgICAgICAgICAgIGlucHV0LmNoZWNrZWQgPSAhJCh0aGlzLl9lbGVtZW50KS5oYXNDbGFzcyhDbGFzc05hbWUuQUNUSVZFKTtcclxuICAgICAgICAgICAgICAkKHRoaXMuX2VsZW1lbnQpLnRyaWdnZXIoJ2NoYW5nZScpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICB9XHJcbiAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgIHRoaXMuX2VsZW1lbnQuc2V0QXR0cmlidXRlKCdhcmlhLXByZXNzZWQnLCAhJCh0aGlzLl9lbGVtZW50KS5oYXNDbGFzcyhDbGFzc05hbWUuQUNUSVZFKSk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBpZiAodHJpZ2dlckNoYW5nZUV2ZW50KSB7XHJcbiAgICAgICAgICAkKHRoaXMuX2VsZW1lbnQpLnRvZ2dsZUNsYXNzKENsYXNzTmFtZS5BQ1RJVkUpO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdkaXNwb3NlJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIGRpc3Bvc2UoKSB7XHJcbiAgICAgICAgJC5yZW1vdmVEYXRhKHRoaXMuX2VsZW1lbnQsIERBVEFfS0VZKTtcclxuICAgICAgICB0aGlzLl9lbGVtZW50ID0gbnVsbDtcclxuICAgICAgfVxyXG5cclxuICAgICAgLy8gc3RhdGljXHJcblxyXG4gICAgfV0sIFt7XHJcbiAgICAgIGtleTogJ19qUXVlcnlJbnRlcmZhY2UnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2pRdWVyeUludGVyZmFjZShjb25maWcpIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5lYWNoKGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgIHZhciBkYXRhID0gJCh0aGlzKS5kYXRhKERBVEFfS0VZKTtcclxuXHJcbiAgICAgICAgICBpZiAoIWRhdGEpIHtcclxuICAgICAgICAgICAgZGF0YSA9IG5ldyBCdXR0b24odGhpcyk7XHJcbiAgICAgICAgICAgICQodGhpcykuZGF0YShEQVRBX0tFWSwgZGF0YSk7XHJcbiAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgaWYgKGNvbmZpZyA9PT0gJ3RvZ2dsZScpIHtcclxuICAgICAgICAgICAgZGF0YVtjb25maWddKCk7XHJcbiAgICAgICAgICB9XHJcbiAgICAgICAgfSk7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnVkVSU0lPTicsXHJcbiAgICAgIGdldDogZnVuY3Rpb24gZ2V0KCkge1xyXG4gICAgICAgIHJldHVybiBWRVJTSU9OO1xyXG4gICAgICB9XHJcbiAgICB9XSk7XHJcblxyXG4gICAgcmV0dXJuIEJ1dHRvbjtcclxuICB9KSgpO1xyXG5cclxuICAkKGRvY3VtZW50KS5vbihFdmVudC5DTElDS19EQVRBX0FQSSwgU2VsZWN0b3IuREFUQV9UT0dHTEVfQ0FSUk9ULCBmdW5jdGlvbiAoZXZlbnQpIHtcclxuICAgIGV2ZW50LnByZXZlbnREZWZhdWx0KCk7XHJcblxyXG4gICAgdmFyIGJ1dHRvbiA9IGV2ZW50LnRhcmdldDtcclxuXHJcbiAgICBpZiAoISQoYnV0dG9uKS5oYXNDbGFzcyhDbGFzc05hbWUuQlVUVE9OKSkge1xyXG4gICAgICBidXR0b24gPSAkKGJ1dHRvbikuY2xvc2VzdChTZWxlY3Rvci5CVVRUT04pO1xyXG4gICAgfVxyXG5cclxuICAgIEJ1dHRvbi5falF1ZXJ5SW50ZXJmYWNlLmNhbGwoJChidXR0b24pLCAndG9nZ2xlJyk7XHJcbiAgfSkub24oRXZlbnQuRk9DVVNfQkxVUl9EQVRBX0FQSSwgU2VsZWN0b3IuREFUQV9UT0dHTEVfQ0FSUk9ULCBmdW5jdGlvbiAoZXZlbnQpIHtcclxuICAgIHZhciBidXR0b24gPSAkKGV2ZW50LnRhcmdldCkuY2xvc2VzdChTZWxlY3Rvci5CVVRUT04pWzBdO1xyXG4gICAgJChidXR0b24pLnRvZ2dsZUNsYXNzKENsYXNzTmFtZS5GT0NVUywgL15mb2N1cyhpbik/JC8udGVzdChldmVudC50eXBlKSk7XHJcbiAgfSk7XHJcblxyXG4gIC8qKlxyXG4gICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAqIGpRdWVyeVxyXG4gICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAqL1xyXG5cclxuICAkLmZuW05BTUVdID0gQnV0dG9uLl9qUXVlcnlJbnRlcmZhY2U7XHJcbiAgJC5mbltOQU1FXS5Db25zdHJ1Y3RvciA9IEJ1dHRvbjtcclxuICAkLmZuW05BTUVdLm5vQ29uZmxpY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAkLmZuW05BTUVdID0gSlFVRVJZX05PX0NPTkZMSUNUO1xyXG4gICAgcmV0dXJuIEJ1dHRvbi5falF1ZXJ5SW50ZXJmYWNlO1xyXG4gIH07XHJcblxyXG4gIHJldHVybiBCdXR0b247XHJcbn0pKGpRdWVyeSk7XHJcbi8vIyBzb3VyY2VNYXBwaW5nVVJMPWJ1dHRvbi5qcy5tYXBcclxuIiwiJ3VzZSBzdHJpY3QnO1xyXG5cclxudmFyIF9jcmVhdGVDbGFzcyA9IChmdW5jdGlvbiAoKSB7IGZ1bmN0aW9uIGRlZmluZVByb3BlcnRpZXModGFyZ2V0LCBwcm9wcykgeyBmb3IgKHZhciBpID0gMDsgaSA8IHByb3BzLmxlbmd0aDsgaSsrKSB7IHZhciBkZXNjcmlwdG9yID0gcHJvcHNbaV07IGRlc2NyaXB0b3IuZW51bWVyYWJsZSA9IGRlc2NyaXB0b3IuZW51bWVyYWJsZSB8fCBmYWxzZTsgZGVzY3JpcHRvci5jb25maWd1cmFibGUgPSB0cnVlOyBpZiAoJ3ZhbHVlJyBpbiBkZXNjcmlwdG9yKSBkZXNjcmlwdG9yLndyaXRhYmxlID0gdHJ1ZTsgT2JqZWN0LmRlZmluZVByb3BlcnR5KHRhcmdldCwgZGVzY3JpcHRvci5rZXksIGRlc2NyaXB0b3IpOyB9IH0gcmV0dXJuIGZ1bmN0aW9uIChDb25zdHJ1Y3RvciwgcHJvdG9Qcm9wcywgc3RhdGljUHJvcHMpIHsgaWYgKHByb3RvUHJvcHMpIGRlZmluZVByb3BlcnRpZXMoQ29uc3RydWN0b3IucHJvdG90eXBlLCBwcm90b1Byb3BzKTsgaWYgKHN0YXRpY1Byb3BzKSBkZWZpbmVQcm9wZXJ0aWVzKENvbnN0cnVjdG9yLCBzdGF0aWNQcm9wcyk7IHJldHVybiBDb25zdHJ1Y3RvcjsgfTsgfSkoKTtcclxuXHJcbmZ1bmN0aW9uIF9jbGFzc0NhbGxDaGVjayhpbnN0YW5jZSwgQ29uc3RydWN0b3IpIHsgaWYgKCEoaW5zdGFuY2UgaW5zdGFuY2VvZiBDb25zdHJ1Y3RvcikpIHsgdGhyb3cgbmV3IFR5cGVFcnJvcignQ2Fubm90IGNhbGwgYSBjbGFzcyBhcyBhIGZ1bmN0aW9uJyk7IH0gfVxyXG5cclxuLyoqXHJcbiAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAqIEJvb3RzdHJhcCAodjQuMC4wLWFscGhhLjIpOiBjYXJvdXNlbC5qc1xyXG4gKiBMaWNlbnNlZCB1bmRlciBNSVQgKGh0dHBzOi8vZ2l0aHViLmNvbS90d2JzL2Jvb3RzdHJhcC9ibG9iL21hc3Rlci9MSUNFTlNFKVxyXG4gKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gKi9cclxuXHJcbnZhciBDYXJvdXNlbCA9IChmdW5jdGlvbiAoJCkge1xyXG5cclxuICAvKipcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKiBDb25zdGFudHNcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKi9cclxuXHJcbiAgdmFyIE5BTUUgPSAnY2Fyb3VzZWwnO1xyXG4gIHZhciBWRVJTSU9OID0gJzQuMC4wLWFscGhhJztcclxuICB2YXIgREFUQV9LRVkgPSAnYnMuY2Fyb3VzZWwnO1xyXG4gIHZhciBFVkVOVF9LRVkgPSAnLicgKyBEQVRBX0tFWTtcclxuICB2YXIgREFUQV9BUElfS0VZID0gJy5kYXRhLWFwaSc7XHJcbiAgdmFyIEpRVUVSWV9OT19DT05GTElDVCA9ICQuZm5bTkFNRV07XHJcbiAgdmFyIFRSQU5TSVRJT05fRFVSQVRJT04gPSA2MDA7XHJcblxyXG4gIHZhciBEZWZhdWx0ID0ge1xyXG4gICAgaW50ZXJ2YWw6IDUwMDAsXHJcbiAgICBrZXlib2FyZDogdHJ1ZSxcclxuICAgIHNsaWRlOiBmYWxzZSxcclxuICAgIHBhdXNlOiAnaG92ZXInLFxyXG4gICAgd3JhcDogdHJ1ZVxyXG4gIH07XHJcblxyXG4gIHZhciBEZWZhdWx0VHlwZSA9IHtcclxuICAgIGludGVydmFsOiAnKG51bWJlcnxib29sZWFuKScsXHJcbiAgICBrZXlib2FyZDogJ2Jvb2xlYW4nLFxyXG4gICAgc2xpZGU6ICcoYm9vbGVhbnxzdHJpbmcpJyxcclxuICAgIHBhdXNlOiAnKHN0cmluZ3xib29sZWFuKScsXHJcbiAgICB3cmFwOiAnYm9vbGVhbidcclxuICB9O1xyXG5cclxuICB2YXIgRGlyZWN0aW9uID0ge1xyXG4gICAgTkVYVDogJ25leHQnLFxyXG4gICAgUFJFVklPVVM6ICdwcmV2J1xyXG4gIH07XHJcblxyXG4gIHZhciBFdmVudCA9IHtcclxuICAgIFNMSURFOiAnc2xpZGUnICsgRVZFTlRfS0VZLFxyXG4gICAgU0xJRDogJ3NsaWQnICsgRVZFTlRfS0VZLFxyXG4gICAgS0VZRE9XTjogJ2tleWRvd24nICsgRVZFTlRfS0VZLFxyXG4gICAgTU9VU0VFTlRFUjogJ21vdXNlZW50ZXInICsgRVZFTlRfS0VZLFxyXG4gICAgTU9VU0VMRUFWRTogJ21vdXNlbGVhdmUnICsgRVZFTlRfS0VZLFxyXG4gICAgTE9BRF9EQVRBX0FQSTogJ2xvYWQnICsgRVZFTlRfS0VZICsgREFUQV9BUElfS0VZLFxyXG4gICAgQ0xJQ0tfREFUQV9BUEk6ICdjbGljaycgKyBFVkVOVF9LRVkgKyBEQVRBX0FQSV9LRVlcclxuICB9O1xyXG5cclxuICB2YXIgQ2xhc3NOYW1lID0ge1xyXG4gICAgQ0FST1VTRUw6ICdjYXJvdXNlbCcsXHJcbiAgICBBQ1RJVkU6ICdhY3RpdmUnLFxyXG4gICAgU0xJREU6ICdzbGlkZScsXHJcbiAgICBSSUdIVDogJ3JpZ2h0JyxcclxuICAgIExFRlQ6ICdsZWZ0JyxcclxuICAgIElURU06ICdjYXJvdXNlbC1pdGVtJ1xyXG4gIH07XHJcblxyXG4gIHZhciBTZWxlY3RvciA9IHtcclxuICAgIEFDVElWRTogJy5hY3RpdmUnLFxyXG4gICAgQUNUSVZFX0lURU06ICcuYWN0aXZlLmNhcm91c2VsLWl0ZW0nLFxyXG4gICAgSVRFTTogJy5jYXJvdXNlbC1pdGVtJyxcclxuICAgIE5FWFRfUFJFVjogJy5uZXh0LCAucHJldicsXHJcbiAgICBJTkRJQ0FUT1JTOiAnLmNhcm91c2VsLWluZGljYXRvcnMnLFxyXG4gICAgREFUQV9TTElERTogJ1tkYXRhLXNsaWRlXSwgW2RhdGEtc2xpZGUtdG9dJyxcclxuICAgIERBVEFfUklERTogJ1tkYXRhLXJpZGU9XCJjYXJvdXNlbFwiXSdcclxuICB9O1xyXG5cclxuICAvKipcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKiBDbGFzcyBEZWZpbml0aW9uXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICovXHJcblxyXG4gIHZhciBDYXJvdXNlbCA9IChmdW5jdGlvbiAoKSB7XHJcbiAgICBmdW5jdGlvbiBDYXJvdXNlbChlbGVtZW50LCBjb25maWcpIHtcclxuICAgICAgX2NsYXNzQ2FsbENoZWNrKHRoaXMsIENhcm91c2VsKTtcclxuXHJcbiAgICAgIHRoaXMuX2l0ZW1zID0gbnVsbDtcclxuICAgICAgdGhpcy5faW50ZXJ2YWwgPSBudWxsO1xyXG4gICAgICB0aGlzLl9hY3RpdmVFbGVtZW50ID0gbnVsbDtcclxuXHJcbiAgICAgIHRoaXMuX2lzUGF1c2VkID0gZmFsc2U7XHJcbiAgICAgIHRoaXMuX2lzU2xpZGluZyA9IGZhbHNlO1xyXG5cclxuICAgICAgdGhpcy5fY29uZmlnID0gdGhpcy5fZ2V0Q29uZmlnKGNvbmZpZyk7XHJcbiAgICAgIHRoaXMuX2VsZW1lbnQgPSAkKGVsZW1lbnQpWzBdO1xyXG4gICAgICB0aGlzLl9pbmRpY2F0b3JzRWxlbWVudCA9ICQodGhpcy5fZWxlbWVudCkuZmluZChTZWxlY3Rvci5JTkRJQ0FUT1JTKVswXTtcclxuXHJcbiAgICAgIHRoaXMuX2FkZEV2ZW50TGlzdGVuZXJzKCk7XHJcbiAgICB9XHJcblxyXG4gICAgLyoqXHJcbiAgICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgICAqIERhdGEgQXBpIGltcGxlbWVudGF0aW9uXHJcbiAgICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgICAqL1xyXG5cclxuICAgIC8vIGdldHRlcnNcclxuXHJcbiAgICBfY3JlYXRlQ2xhc3MoQ2Fyb3VzZWwsIFt7XHJcbiAgICAgIGtleTogJ25leHQnLFxyXG5cclxuICAgICAgLy8gcHVibGljXHJcblxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gbmV4dCgpIHtcclxuICAgICAgICBpZiAoIXRoaXMuX2lzU2xpZGluZykge1xyXG4gICAgICAgICAgdGhpcy5fc2xpZGUoRGlyZWN0aW9uLk5FWFQpO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICduZXh0V2hlblZpc2libGUnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gbmV4dFdoZW5WaXNpYmxlKCkge1xyXG4gICAgICAgIC8vIERvbid0IGNhbGwgbmV4dCB3aGVuIHRoZSBwYWdlIGlzbid0IHZpc2libGVcclxuICAgICAgICBpZiAoIWRvY3VtZW50LmhpZGRlbikge1xyXG4gICAgICAgICAgdGhpcy5uZXh0KCk7XHJcbiAgICAgICAgfVxyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ3ByZXYnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gcHJldigpIHtcclxuICAgICAgICBpZiAoIXRoaXMuX2lzU2xpZGluZykge1xyXG4gICAgICAgICAgdGhpcy5fc2xpZGUoRGlyZWN0aW9uLlBSRVZJT1VTKTtcclxuICAgICAgICB9XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAncGF1c2UnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gcGF1c2UoZXZlbnQpIHtcclxuICAgICAgICBpZiAoIWV2ZW50KSB7XHJcbiAgICAgICAgICB0aGlzLl9pc1BhdXNlZCA9IHRydWU7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBpZiAoJCh0aGlzLl9lbGVtZW50KS5maW5kKFNlbGVjdG9yLk5FWFRfUFJFVilbMF0gJiYgVXRpbC5zdXBwb3J0c1RyYW5zaXRpb25FbmQoKSkge1xyXG4gICAgICAgICAgVXRpbC50cmlnZ2VyVHJhbnNpdGlvbkVuZCh0aGlzLl9lbGVtZW50KTtcclxuICAgICAgICAgIHRoaXMuY3ljbGUodHJ1ZSk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBjbGVhckludGVydmFsKHRoaXMuX2ludGVydmFsKTtcclxuICAgICAgICB0aGlzLl9pbnRlcnZhbCA9IG51bGw7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnY3ljbGUnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gY3ljbGUoZXZlbnQpIHtcclxuICAgICAgICBpZiAoIWV2ZW50KSB7XHJcbiAgICAgICAgICB0aGlzLl9pc1BhdXNlZCA9IGZhbHNlO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgaWYgKHRoaXMuX2ludGVydmFsKSB7XHJcbiAgICAgICAgICBjbGVhckludGVydmFsKHRoaXMuX2ludGVydmFsKTtcclxuICAgICAgICAgIHRoaXMuX2ludGVydmFsID0gbnVsbDtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGlmICh0aGlzLl9jb25maWcuaW50ZXJ2YWwgJiYgIXRoaXMuX2lzUGF1c2VkKSB7XHJcbiAgICAgICAgICB0aGlzLl9pbnRlcnZhbCA9IHNldEludGVydmFsKCQucHJveHkoZG9jdW1lbnQudmlzaWJpbGl0eVN0YXRlID8gdGhpcy5uZXh0V2hlblZpc2libGUgOiB0aGlzLm5leHQsIHRoaXMpLCB0aGlzLl9jb25maWcuaW50ZXJ2YWwpO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICd0bycsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiB0byhpbmRleCkge1xyXG4gICAgICAgIHZhciBfdGhpcyA9IHRoaXM7XHJcblxyXG4gICAgICAgIHRoaXMuX2FjdGl2ZUVsZW1lbnQgPSAkKHRoaXMuX2VsZW1lbnQpLmZpbmQoU2VsZWN0b3IuQUNUSVZFX0lURU0pWzBdO1xyXG5cclxuICAgICAgICB2YXIgYWN0aXZlSW5kZXggPSB0aGlzLl9nZXRJdGVtSW5kZXgodGhpcy5fYWN0aXZlRWxlbWVudCk7XHJcblxyXG4gICAgICAgIGlmIChpbmRleCA+IHRoaXMuX2l0ZW1zLmxlbmd0aCAtIDEgfHwgaW5kZXggPCAwKSB7XHJcbiAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBpZiAodGhpcy5faXNTbGlkaW5nKSB7XHJcbiAgICAgICAgICAkKHRoaXMuX2VsZW1lbnQpLm9uZShFdmVudC5TTElELCBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiBfdGhpcy50byhpbmRleCk7XHJcbiAgICAgICAgICB9KTtcclxuICAgICAgICAgIHJldHVybjtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGlmIChhY3RpdmVJbmRleCA9PT0gaW5kZXgpIHtcclxuICAgICAgICAgIHRoaXMucGF1c2UoKTtcclxuICAgICAgICAgIHRoaXMuY3ljbGUoKTtcclxuICAgICAgICAgIHJldHVybjtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHZhciBkaXJlY3Rpb24gPSBpbmRleCA+IGFjdGl2ZUluZGV4ID8gRGlyZWN0aW9uLk5FWFQgOiBEaXJlY3Rpb24uUFJFVklPVVM7XHJcblxyXG4gICAgICAgIHRoaXMuX3NsaWRlKGRpcmVjdGlvbiwgdGhpcy5faXRlbXNbaW5kZXhdKTtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdkaXNwb3NlJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIGRpc3Bvc2UoKSB7XHJcbiAgICAgICAgJCh0aGlzLl9lbGVtZW50KS5vZmYoRVZFTlRfS0VZKTtcclxuICAgICAgICAkLnJlbW92ZURhdGEodGhpcy5fZWxlbWVudCwgREFUQV9LRVkpO1xyXG5cclxuICAgICAgICB0aGlzLl9pdGVtcyA9IG51bGw7XHJcbiAgICAgICAgdGhpcy5fY29uZmlnID0gbnVsbDtcclxuICAgICAgICB0aGlzLl9lbGVtZW50ID0gbnVsbDtcclxuICAgICAgICB0aGlzLl9pbnRlcnZhbCA9IG51bGw7XHJcbiAgICAgICAgdGhpcy5faXNQYXVzZWQgPSBudWxsO1xyXG4gICAgICAgIHRoaXMuX2lzU2xpZGluZyA9IG51bGw7XHJcbiAgICAgICAgdGhpcy5fYWN0aXZlRWxlbWVudCA9IG51bGw7XHJcbiAgICAgICAgdGhpcy5faW5kaWNhdG9yc0VsZW1lbnQgPSBudWxsO1xyXG4gICAgICB9XHJcblxyXG4gICAgICAvLyBwcml2YXRlXHJcblxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfZ2V0Q29uZmlnJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9nZXRDb25maWcoY29uZmlnKSB7XHJcbiAgICAgICAgY29uZmlnID0gJC5leHRlbmQoe30sIERlZmF1bHQsIGNvbmZpZyk7XHJcbiAgICAgICAgVXRpbC50eXBlQ2hlY2tDb25maWcoTkFNRSwgY29uZmlnLCBEZWZhdWx0VHlwZSk7XHJcbiAgICAgICAgcmV0dXJuIGNvbmZpZztcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfYWRkRXZlbnRMaXN0ZW5lcnMnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2FkZEV2ZW50TGlzdGVuZXJzKCkge1xyXG4gICAgICAgIGlmICh0aGlzLl9jb25maWcua2V5Ym9hcmQpIHtcclxuICAgICAgICAgICQodGhpcy5fZWxlbWVudCkub24oRXZlbnQuS0VZRE9XTiwgJC5wcm94eSh0aGlzLl9rZXlkb3duLCB0aGlzKSk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBpZiAodGhpcy5fY29uZmlnLnBhdXNlID09PSAnaG92ZXInICYmICEoJ29udG91Y2hzdGFydCcgaW4gZG9jdW1lbnQuZG9jdW1lbnRFbGVtZW50KSkge1xyXG4gICAgICAgICAgJCh0aGlzLl9lbGVtZW50KS5vbihFdmVudC5NT1VTRUVOVEVSLCAkLnByb3h5KHRoaXMucGF1c2UsIHRoaXMpKS5vbihFdmVudC5NT1VTRUxFQVZFLCAkLnByb3h5KHRoaXMuY3ljbGUsIHRoaXMpKTtcclxuICAgICAgICB9XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX2tleWRvd24nLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2tleWRvd24oZXZlbnQpIHtcclxuICAgICAgICBldmVudC5wcmV2ZW50RGVmYXVsdCgpO1xyXG5cclxuICAgICAgICBpZiAoL2lucHV0fHRleHRhcmVhL2kudGVzdChldmVudC50YXJnZXQudGFnTmFtZSkpIHtcclxuICAgICAgICAgIHJldHVybjtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHN3aXRjaCAoZXZlbnQud2hpY2gpIHtcclxuICAgICAgICAgIGNhc2UgMzc6XHJcbiAgICAgICAgICAgIHRoaXMucHJldigpO2JyZWFrO1xyXG4gICAgICAgICAgY2FzZSAzOTpcclxuICAgICAgICAgICAgdGhpcy5uZXh0KCk7YnJlYWs7XHJcbiAgICAgICAgICBkZWZhdWx0OlxyXG4gICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgfVxyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19nZXRJdGVtSW5kZXgnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2dldEl0ZW1JbmRleChlbGVtZW50KSB7XHJcbiAgICAgICAgdGhpcy5faXRlbXMgPSAkLm1ha2VBcnJheSgkKGVsZW1lbnQpLnBhcmVudCgpLmZpbmQoU2VsZWN0b3IuSVRFTSkpO1xyXG4gICAgICAgIHJldHVybiB0aGlzLl9pdGVtcy5pbmRleE9mKGVsZW1lbnQpO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19nZXRJdGVtQnlEaXJlY3Rpb24nLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2dldEl0ZW1CeURpcmVjdGlvbihkaXJlY3Rpb24sIGFjdGl2ZUVsZW1lbnQpIHtcclxuICAgICAgICB2YXIgaXNOZXh0RGlyZWN0aW9uID0gZGlyZWN0aW9uID09PSBEaXJlY3Rpb24uTkVYVDtcclxuICAgICAgICB2YXIgaXNQcmV2RGlyZWN0aW9uID0gZGlyZWN0aW9uID09PSBEaXJlY3Rpb24uUFJFVklPVVM7XHJcbiAgICAgICAgdmFyIGFjdGl2ZUluZGV4ID0gdGhpcy5fZ2V0SXRlbUluZGV4KGFjdGl2ZUVsZW1lbnQpO1xyXG4gICAgICAgIHZhciBsYXN0SXRlbUluZGV4ID0gdGhpcy5faXRlbXMubGVuZ3RoIC0gMTtcclxuICAgICAgICB2YXIgaXNHb2luZ1RvV3JhcCA9IGlzUHJldkRpcmVjdGlvbiAmJiBhY3RpdmVJbmRleCA9PT0gMCB8fCBpc05leHREaXJlY3Rpb24gJiYgYWN0aXZlSW5kZXggPT09IGxhc3RJdGVtSW5kZXg7XHJcblxyXG4gICAgICAgIGlmIChpc0dvaW5nVG9XcmFwICYmICF0aGlzLl9jb25maWcud3JhcCkge1xyXG4gICAgICAgICAgcmV0dXJuIGFjdGl2ZUVsZW1lbnQ7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB2YXIgZGVsdGEgPSBkaXJlY3Rpb24gPT09IERpcmVjdGlvbi5QUkVWSU9VUyA/IC0xIDogMTtcclxuICAgICAgICB2YXIgaXRlbUluZGV4ID0gKGFjdGl2ZUluZGV4ICsgZGVsdGEpICUgdGhpcy5faXRlbXMubGVuZ3RoO1xyXG5cclxuICAgICAgICByZXR1cm4gaXRlbUluZGV4ID09PSAtMSA/IHRoaXMuX2l0ZW1zW3RoaXMuX2l0ZW1zLmxlbmd0aCAtIDFdIDogdGhpcy5faXRlbXNbaXRlbUluZGV4XTtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfdHJpZ2dlclNsaWRlRXZlbnQnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX3RyaWdnZXJTbGlkZUV2ZW50KHJlbGF0ZWRUYXJnZXQsIGRpcmVjdGlvbmFsQ2xhc3NuYW1lKSB7XHJcbiAgICAgICAgdmFyIHNsaWRlRXZlbnQgPSAkLkV2ZW50KEV2ZW50LlNMSURFLCB7XHJcbiAgICAgICAgICByZWxhdGVkVGFyZ2V0OiByZWxhdGVkVGFyZ2V0LFxyXG4gICAgICAgICAgZGlyZWN0aW9uOiBkaXJlY3Rpb25hbENsYXNzbmFtZVxyXG4gICAgICAgIH0pO1xyXG5cclxuICAgICAgICAkKHRoaXMuX2VsZW1lbnQpLnRyaWdnZXIoc2xpZGVFdmVudCk7XHJcblxyXG4gICAgICAgIHJldHVybiBzbGlkZUV2ZW50O1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19zZXRBY3RpdmVJbmRpY2F0b3JFbGVtZW50JyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9zZXRBY3RpdmVJbmRpY2F0b3JFbGVtZW50KGVsZW1lbnQpIHtcclxuICAgICAgICBpZiAodGhpcy5faW5kaWNhdG9yc0VsZW1lbnQpIHtcclxuICAgICAgICAgICQodGhpcy5faW5kaWNhdG9yc0VsZW1lbnQpLmZpbmQoU2VsZWN0b3IuQUNUSVZFKS5yZW1vdmVDbGFzcyhDbGFzc05hbWUuQUNUSVZFKTtcclxuXHJcbiAgICAgICAgICB2YXIgbmV4dEluZGljYXRvciA9IHRoaXMuX2luZGljYXRvcnNFbGVtZW50LmNoaWxkcmVuW3RoaXMuX2dldEl0ZW1JbmRleChlbGVtZW50KV07XHJcblxyXG4gICAgICAgICAgaWYgKG5leHRJbmRpY2F0b3IpIHtcclxuICAgICAgICAgICAgJChuZXh0SW5kaWNhdG9yKS5hZGRDbGFzcyhDbGFzc05hbWUuQUNUSVZFKTtcclxuICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX3NsaWRlJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9zbGlkZShkaXJlY3Rpb24sIGVsZW1lbnQpIHtcclxuICAgICAgICB2YXIgX3RoaXMyID0gdGhpcztcclxuXHJcbiAgICAgICAgdmFyIGFjdGl2ZUVsZW1lbnQgPSAkKHRoaXMuX2VsZW1lbnQpLmZpbmQoU2VsZWN0b3IuQUNUSVZFX0lURU0pWzBdO1xyXG4gICAgICAgIHZhciBuZXh0RWxlbWVudCA9IGVsZW1lbnQgfHwgYWN0aXZlRWxlbWVudCAmJiB0aGlzLl9nZXRJdGVtQnlEaXJlY3Rpb24oZGlyZWN0aW9uLCBhY3RpdmVFbGVtZW50KTtcclxuXHJcbiAgICAgICAgdmFyIGlzQ3ljbGluZyA9IEJvb2xlYW4odGhpcy5faW50ZXJ2YWwpO1xyXG5cclxuICAgICAgICB2YXIgZGlyZWN0aW9uYWxDbGFzc05hbWUgPSBkaXJlY3Rpb24gPT09IERpcmVjdGlvbi5ORVhUID8gQ2xhc3NOYW1lLkxFRlQgOiBDbGFzc05hbWUuUklHSFQ7XHJcblxyXG4gICAgICAgIGlmIChuZXh0RWxlbWVudCAmJiAkKG5leHRFbGVtZW50KS5oYXNDbGFzcyhDbGFzc05hbWUuQUNUSVZFKSkge1xyXG4gICAgICAgICAgdGhpcy5faXNTbGlkaW5nID0gZmFsc2U7XHJcbiAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB2YXIgc2xpZGVFdmVudCA9IHRoaXMuX3RyaWdnZXJTbGlkZUV2ZW50KG5leHRFbGVtZW50LCBkaXJlY3Rpb25hbENsYXNzTmFtZSk7XHJcbiAgICAgICAgaWYgKHNsaWRlRXZlbnQuaXNEZWZhdWx0UHJldmVudGVkKCkpIHtcclxuICAgICAgICAgIHJldHVybjtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGlmICghYWN0aXZlRWxlbWVudCB8fCAhbmV4dEVsZW1lbnQpIHtcclxuICAgICAgICAgIC8vIHNvbWUgd2VpcmRuZXNzIGlzIGhhcHBlbmluZywgc28gd2UgYmFpbFxyXG4gICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdGhpcy5faXNTbGlkaW5nID0gdHJ1ZTtcclxuXHJcbiAgICAgICAgaWYgKGlzQ3ljbGluZykge1xyXG4gICAgICAgICAgdGhpcy5wYXVzZSgpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdGhpcy5fc2V0QWN0aXZlSW5kaWNhdG9yRWxlbWVudChuZXh0RWxlbWVudCk7XHJcblxyXG4gICAgICAgIHZhciBzbGlkRXZlbnQgPSAkLkV2ZW50KEV2ZW50LlNMSUQsIHtcclxuICAgICAgICAgIHJlbGF0ZWRUYXJnZXQ6IG5leHRFbGVtZW50LFxyXG4gICAgICAgICAgZGlyZWN0aW9uOiBkaXJlY3Rpb25hbENsYXNzTmFtZVxyXG4gICAgICAgIH0pO1xyXG5cclxuICAgICAgICBpZiAoVXRpbC5zdXBwb3J0c1RyYW5zaXRpb25FbmQoKSAmJiAkKHRoaXMuX2VsZW1lbnQpLmhhc0NsYXNzKENsYXNzTmFtZS5TTElERSkpIHtcclxuXHJcbiAgICAgICAgICAkKG5leHRFbGVtZW50KS5hZGRDbGFzcyhkaXJlY3Rpb24pO1xyXG5cclxuICAgICAgICAgIFV0aWwucmVmbG93KG5leHRFbGVtZW50KTtcclxuXHJcbiAgICAgICAgICAkKGFjdGl2ZUVsZW1lbnQpLmFkZENsYXNzKGRpcmVjdGlvbmFsQ2xhc3NOYW1lKTtcclxuICAgICAgICAgICQobmV4dEVsZW1lbnQpLmFkZENsYXNzKGRpcmVjdGlvbmFsQ2xhc3NOYW1lKTtcclxuXHJcbiAgICAgICAgICAkKGFjdGl2ZUVsZW1lbnQpLm9uZShVdGlsLlRSQU5TSVRJT05fRU5ELCBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICQobmV4dEVsZW1lbnQpLnJlbW92ZUNsYXNzKGRpcmVjdGlvbmFsQ2xhc3NOYW1lKS5yZW1vdmVDbGFzcyhkaXJlY3Rpb24pO1xyXG5cclxuICAgICAgICAgICAgJChuZXh0RWxlbWVudCkuYWRkQ2xhc3MoQ2xhc3NOYW1lLkFDVElWRSk7XHJcblxyXG4gICAgICAgICAgICAkKGFjdGl2ZUVsZW1lbnQpLnJlbW92ZUNsYXNzKENsYXNzTmFtZS5BQ1RJVkUpLnJlbW92ZUNsYXNzKGRpcmVjdGlvbikucmVtb3ZlQ2xhc3MoZGlyZWN0aW9uYWxDbGFzc05hbWUpO1xyXG5cclxuICAgICAgICAgICAgX3RoaXMyLl9pc1NsaWRpbmcgPSBmYWxzZTtcclxuXHJcbiAgICAgICAgICAgIHNldFRpbWVvdXQoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgIHJldHVybiAkKF90aGlzMi5fZWxlbWVudCkudHJpZ2dlcihzbGlkRXZlbnQpO1xyXG4gICAgICAgICAgICB9LCAwKTtcclxuICAgICAgICAgIH0pLmVtdWxhdGVUcmFuc2l0aW9uRW5kKFRSQU5TSVRJT05fRFVSQVRJT04pO1xyXG4gICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAkKGFjdGl2ZUVsZW1lbnQpLnJlbW92ZUNsYXNzKENsYXNzTmFtZS5BQ1RJVkUpO1xyXG4gICAgICAgICAgJChuZXh0RWxlbWVudCkuYWRkQ2xhc3MoQ2xhc3NOYW1lLkFDVElWRSk7XHJcblxyXG4gICAgICAgICAgdGhpcy5faXNTbGlkaW5nID0gZmFsc2U7XHJcbiAgICAgICAgICAkKHRoaXMuX2VsZW1lbnQpLnRyaWdnZXIoc2xpZEV2ZW50KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGlmIChpc0N5Y2xpbmcpIHtcclxuICAgICAgICAgIHRoaXMuY3ljbGUoKTtcclxuICAgICAgICB9XHJcbiAgICAgIH1cclxuXHJcbiAgICAgIC8vIHN0YXRpY1xyXG5cclxuICAgIH1dLCBbe1xyXG4gICAgICBrZXk6ICdfalF1ZXJ5SW50ZXJmYWNlJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9qUXVlcnlJbnRlcmZhY2UoY29uZmlnKSB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZWFjaChmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICB2YXIgZGF0YSA9ICQodGhpcykuZGF0YShEQVRBX0tFWSk7XHJcbiAgICAgICAgICB2YXIgX2NvbmZpZyA9ICQuZXh0ZW5kKHt9LCBEZWZhdWx0LCAkKHRoaXMpLmRhdGEoKSk7XHJcblxyXG4gICAgICAgICAgaWYgKHR5cGVvZiBjb25maWcgPT09ICdvYmplY3QnKSB7XHJcbiAgICAgICAgICAgICQuZXh0ZW5kKF9jb25maWcsIGNvbmZpZyk7XHJcbiAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgdmFyIGFjdGlvbiA9IHR5cGVvZiBjb25maWcgPT09ICdzdHJpbmcnID8gY29uZmlnIDogX2NvbmZpZy5zbGlkZTtcclxuXHJcbiAgICAgICAgICBpZiAoIWRhdGEpIHtcclxuICAgICAgICAgICAgZGF0YSA9IG5ldyBDYXJvdXNlbCh0aGlzLCBfY29uZmlnKTtcclxuICAgICAgICAgICAgJCh0aGlzKS5kYXRhKERBVEFfS0VZLCBkYXRhKTtcclxuICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICBpZiAodHlwZW9mIGNvbmZpZyA9PT0gJ251bWJlcicpIHtcclxuICAgICAgICAgICAgZGF0YS50byhjb25maWcpO1xyXG4gICAgICAgICAgfSBlbHNlIGlmICh0eXBlb2YgYWN0aW9uID09PSAnc3RyaW5nJykge1xyXG4gICAgICAgICAgICBpZiAoZGF0YVthY3Rpb25dID09PSB1bmRlZmluZWQpIHtcclxuICAgICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoJ05vIG1ldGhvZCBuYW1lZCBcIicgKyBhY3Rpb24gKyAnXCInKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBkYXRhW2FjdGlvbl0oKTtcclxuICAgICAgICAgIH0gZWxzZSBpZiAoX2NvbmZpZy5pbnRlcnZhbCkge1xyXG4gICAgICAgICAgICBkYXRhLnBhdXNlKCk7XHJcbiAgICAgICAgICAgIGRhdGEuY3ljbGUoKTtcclxuICAgICAgICAgIH1cclxuICAgICAgICB9KTtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfZGF0YUFwaUNsaWNrSGFuZGxlcicsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBfZGF0YUFwaUNsaWNrSGFuZGxlcihldmVudCkge1xyXG4gICAgICAgIHZhciBzZWxlY3RvciA9IFV0aWwuZ2V0U2VsZWN0b3JGcm9tRWxlbWVudCh0aGlzKTtcclxuXHJcbiAgICAgICAgaWYgKCFzZWxlY3Rvcikge1xyXG4gICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdmFyIHRhcmdldCA9ICQoc2VsZWN0b3IpWzBdO1xyXG5cclxuICAgICAgICBpZiAoIXRhcmdldCB8fCAhJCh0YXJnZXQpLmhhc0NsYXNzKENsYXNzTmFtZS5DQVJPVVNFTCkpIHtcclxuICAgICAgICAgIHJldHVybjtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHZhciBjb25maWcgPSAkLmV4dGVuZCh7fSwgJCh0YXJnZXQpLmRhdGEoKSwgJCh0aGlzKS5kYXRhKCkpO1xyXG4gICAgICAgIHZhciBzbGlkZUluZGV4ID0gdGhpcy5nZXRBdHRyaWJ1dGUoJ2RhdGEtc2xpZGUtdG8nKTtcclxuXHJcbiAgICAgICAgaWYgKHNsaWRlSW5kZXgpIHtcclxuICAgICAgICAgIGNvbmZpZy5pbnRlcnZhbCA9IGZhbHNlO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgQ2Fyb3VzZWwuX2pRdWVyeUludGVyZmFjZS5jYWxsKCQodGFyZ2V0KSwgY29uZmlnKTtcclxuXHJcbiAgICAgICAgaWYgKHNsaWRlSW5kZXgpIHtcclxuICAgICAgICAgICQodGFyZ2V0KS5kYXRhKERBVEFfS0VZKS50byhzbGlkZUluZGV4KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGV2ZW50LnByZXZlbnREZWZhdWx0KCk7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnVkVSU0lPTicsXHJcbiAgICAgIGdldDogZnVuY3Rpb24gZ2V0KCkge1xyXG4gICAgICAgIHJldHVybiBWRVJTSU9OO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ0RlZmF1bHQnLFxyXG4gICAgICBnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcclxuICAgICAgICByZXR1cm4gRGVmYXVsdDtcclxuICAgICAgfVxyXG4gICAgfV0pO1xyXG5cclxuICAgIHJldHVybiBDYXJvdXNlbDtcclxuICB9KSgpO1xyXG5cclxuICAkKGRvY3VtZW50KS5vbihFdmVudC5DTElDS19EQVRBX0FQSSwgU2VsZWN0b3IuREFUQV9TTElERSwgQ2Fyb3VzZWwuX2RhdGFBcGlDbGlja0hhbmRsZXIpO1xyXG5cclxuICAkKHdpbmRvdykub24oRXZlbnQuTE9BRF9EQVRBX0FQSSwgZnVuY3Rpb24gKCkge1xyXG4gICAgJChTZWxlY3Rvci5EQVRBX1JJREUpLmVhY2goZnVuY3Rpb24gKCkge1xyXG4gICAgICB2YXIgJGNhcm91c2VsID0gJCh0aGlzKTtcclxuICAgICAgQ2Fyb3VzZWwuX2pRdWVyeUludGVyZmFjZS5jYWxsKCRjYXJvdXNlbCwgJGNhcm91c2VsLmRhdGEoKSk7XHJcbiAgICB9KTtcclxuICB9KTtcclxuXHJcbiAgLyoqXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICogalF1ZXJ5XHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICovXHJcblxyXG4gICQuZm5bTkFNRV0gPSBDYXJvdXNlbC5falF1ZXJ5SW50ZXJmYWNlO1xyXG4gICQuZm5bTkFNRV0uQ29uc3RydWN0b3IgPSBDYXJvdXNlbDtcclxuICAkLmZuW05BTUVdLm5vQ29uZmxpY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAkLmZuW05BTUVdID0gSlFVRVJZX05PX0NPTkZMSUNUO1xyXG4gICAgcmV0dXJuIENhcm91c2VsLl9qUXVlcnlJbnRlcmZhY2U7XHJcbiAgfTtcclxuXHJcbiAgcmV0dXJuIENhcm91c2VsO1xyXG59KShqUXVlcnkpO1xyXG4vLyMgc291cmNlTWFwcGluZ1VSTD1jYXJvdXNlbC5qcy5tYXBcclxuIiwiJ3VzZSBzdHJpY3QnO1xyXG5cclxudmFyIF9jcmVhdGVDbGFzcyA9IChmdW5jdGlvbiAoKSB7IGZ1bmN0aW9uIGRlZmluZVByb3BlcnRpZXModGFyZ2V0LCBwcm9wcykgeyBmb3IgKHZhciBpID0gMDsgaSA8IHByb3BzLmxlbmd0aDsgaSsrKSB7IHZhciBkZXNjcmlwdG9yID0gcHJvcHNbaV07IGRlc2NyaXB0b3IuZW51bWVyYWJsZSA9IGRlc2NyaXB0b3IuZW51bWVyYWJsZSB8fCBmYWxzZTsgZGVzY3JpcHRvci5jb25maWd1cmFibGUgPSB0cnVlOyBpZiAoJ3ZhbHVlJyBpbiBkZXNjcmlwdG9yKSBkZXNjcmlwdG9yLndyaXRhYmxlID0gdHJ1ZTsgT2JqZWN0LmRlZmluZVByb3BlcnR5KHRhcmdldCwgZGVzY3JpcHRvci5rZXksIGRlc2NyaXB0b3IpOyB9IH0gcmV0dXJuIGZ1bmN0aW9uIChDb25zdHJ1Y3RvciwgcHJvdG9Qcm9wcywgc3RhdGljUHJvcHMpIHsgaWYgKHByb3RvUHJvcHMpIGRlZmluZVByb3BlcnRpZXMoQ29uc3RydWN0b3IucHJvdG90eXBlLCBwcm90b1Byb3BzKTsgaWYgKHN0YXRpY1Byb3BzKSBkZWZpbmVQcm9wZXJ0aWVzKENvbnN0cnVjdG9yLCBzdGF0aWNQcm9wcyk7IHJldHVybiBDb25zdHJ1Y3RvcjsgfTsgfSkoKTtcclxuXHJcbmZ1bmN0aW9uIF9jbGFzc0NhbGxDaGVjayhpbnN0YW5jZSwgQ29uc3RydWN0b3IpIHsgaWYgKCEoaW5zdGFuY2UgaW5zdGFuY2VvZiBDb25zdHJ1Y3RvcikpIHsgdGhyb3cgbmV3IFR5cGVFcnJvcignQ2Fubm90IGNhbGwgYSBjbGFzcyBhcyBhIGZ1bmN0aW9uJyk7IH0gfVxyXG5cclxuLyoqXHJcbiAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAqIEJvb3RzdHJhcCAodjQuMC4wLWFscGhhLjIpOiBjb2xsYXBzZS5qc1xyXG4gKiBMaWNlbnNlZCB1bmRlciBNSVQgKGh0dHBzOi8vZ2l0aHViLmNvbS90d2JzL2Jvb3RzdHJhcC9ibG9iL21hc3Rlci9MSUNFTlNFKVxyXG4gKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gKi9cclxuXHJcbnZhciBDb2xsYXBzZSA9IChmdW5jdGlvbiAoJCkge1xyXG5cclxuICAvKipcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKiBDb25zdGFudHNcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKi9cclxuXHJcbiAgdmFyIE5BTUUgPSAnY29sbGFwc2UnO1xyXG4gIHZhciBWRVJTSU9OID0gJzQuMC4wLWFscGhhJztcclxuICB2YXIgREFUQV9LRVkgPSAnYnMuY29sbGFwc2UnO1xyXG4gIHZhciBFVkVOVF9LRVkgPSAnLicgKyBEQVRBX0tFWTtcclxuICB2YXIgREFUQV9BUElfS0VZID0gJy5kYXRhLWFwaSc7XHJcbiAgdmFyIEpRVUVSWV9OT19DT05GTElDVCA9ICQuZm5bTkFNRV07XHJcbiAgdmFyIFRSQU5TSVRJT05fRFVSQVRJT04gPSA2MDA7XHJcblxyXG4gIHZhciBEZWZhdWx0ID0ge1xyXG4gICAgdG9nZ2xlOiB0cnVlLFxyXG4gICAgcGFyZW50OiAnJ1xyXG4gIH07XHJcblxyXG4gIHZhciBEZWZhdWx0VHlwZSA9IHtcclxuICAgIHRvZ2dsZTogJ2Jvb2xlYW4nLFxyXG4gICAgcGFyZW50OiAnc3RyaW5nJ1xyXG4gIH07XHJcblxyXG4gIHZhciBFdmVudCA9IHtcclxuICAgIFNIT1c6ICdzaG93JyArIEVWRU5UX0tFWSxcclxuICAgIFNIT1dOOiAnc2hvd24nICsgRVZFTlRfS0VZLFxyXG4gICAgSElERTogJ2hpZGUnICsgRVZFTlRfS0VZLFxyXG4gICAgSElEREVOOiAnaGlkZGVuJyArIEVWRU5UX0tFWSxcclxuICAgIENMSUNLX0RBVEFfQVBJOiAnY2xpY2snICsgRVZFTlRfS0VZICsgREFUQV9BUElfS0VZXHJcbiAgfTtcclxuXHJcbiAgdmFyIENsYXNzTmFtZSA9IHtcclxuICAgIElOOiAnaW4nLFxyXG4gICAgQ09MTEFQU0U6ICdjb2xsYXBzZScsXHJcbiAgICBDT0xMQVBTSU5HOiAnY29sbGFwc2luZycsXHJcbiAgICBDT0xMQVBTRUQ6ICdjb2xsYXBzZWQnXHJcbiAgfTtcclxuXHJcbiAgdmFyIERpbWVuc2lvbiA9IHtcclxuICAgIFdJRFRIOiAnd2lkdGgnLFxyXG4gICAgSEVJR0hUOiAnaGVpZ2h0J1xyXG4gIH07XHJcblxyXG4gIHZhciBTZWxlY3RvciA9IHtcclxuICAgIEFDVElWRVM6ICcucGFuZWwgPiAuaW4sIC5wYW5lbCA+IC5jb2xsYXBzaW5nJyxcclxuICAgIERBVEFfVE9HR0xFOiAnW2RhdGEtdG9nZ2xlPVwiY29sbGFwc2VcIl0nXHJcbiAgfTtcclxuXHJcbiAgLyoqXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICogQ2xhc3MgRGVmaW5pdGlvblxyXG4gICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAqL1xyXG5cclxuICB2YXIgQ29sbGFwc2UgPSAoZnVuY3Rpb24gKCkge1xyXG4gICAgZnVuY3Rpb24gQ29sbGFwc2UoZWxlbWVudCwgY29uZmlnKSB7XHJcbiAgICAgIF9jbGFzc0NhbGxDaGVjayh0aGlzLCBDb2xsYXBzZSk7XHJcblxyXG4gICAgICB0aGlzLl9pc1RyYW5zaXRpb25pbmcgPSBmYWxzZTtcclxuICAgICAgdGhpcy5fZWxlbWVudCA9IGVsZW1lbnQ7XHJcbiAgICAgIHRoaXMuX2NvbmZpZyA9IHRoaXMuX2dldENvbmZpZyhjb25maWcpO1xyXG4gICAgICB0aGlzLl90cmlnZ2VyQXJyYXkgPSAkLm1ha2VBcnJheSgkKCdbZGF0YS10b2dnbGU9XCJjb2xsYXBzZVwiXVtocmVmPVwiIycgKyBlbGVtZW50LmlkICsgJ1wiXSwnICsgKCdbZGF0YS10b2dnbGU9XCJjb2xsYXBzZVwiXVtkYXRhLXRhcmdldD1cIiMnICsgZWxlbWVudC5pZCArICdcIl0nKSkpO1xyXG5cclxuICAgICAgdGhpcy5fcGFyZW50ID0gdGhpcy5fY29uZmlnLnBhcmVudCA/IHRoaXMuX2dldFBhcmVudCgpIDogbnVsbDtcclxuXHJcbiAgICAgIGlmICghdGhpcy5fY29uZmlnLnBhcmVudCkge1xyXG4gICAgICAgIHRoaXMuX2FkZEFyaWFBbmRDb2xsYXBzZWRDbGFzcyh0aGlzLl9lbGVtZW50LCB0aGlzLl90cmlnZ2VyQXJyYXkpO1xyXG4gICAgICB9XHJcblxyXG4gICAgICBpZiAodGhpcy5fY29uZmlnLnRvZ2dsZSkge1xyXG4gICAgICAgIHRoaXMudG9nZ2xlKCk7XHJcbiAgICAgIH1cclxuICAgIH1cclxuXHJcbiAgICAvKipcclxuICAgICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAgICogRGF0YSBBcGkgaW1wbGVtZW50YXRpb25cclxuICAgICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAgICovXHJcblxyXG4gICAgLy8gZ2V0dGVyc1xyXG5cclxuICAgIF9jcmVhdGVDbGFzcyhDb2xsYXBzZSwgW3tcclxuICAgICAga2V5OiAndG9nZ2xlJyxcclxuXHJcbiAgICAgIC8vIHB1YmxpY1xyXG5cclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIHRvZ2dsZSgpIHtcclxuICAgICAgICBpZiAoJCh0aGlzLl9lbGVtZW50KS5oYXNDbGFzcyhDbGFzc05hbWUuSU4pKSB7XHJcbiAgICAgICAgICB0aGlzLmhpZGUoKTtcclxuICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgdGhpcy5zaG93KCk7XHJcbiAgICAgICAgfVxyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ3Nob3cnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gc2hvdygpIHtcclxuICAgICAgICB2YXIgX3RoaXMgPSB0aGlzO1xyXG5cclxuICAgICAgICBpZiAodGhpcy5faXNUcmFuc2l0aW9uaW5nIHx8ICQodGhpcy5fZWxlbWVudCkuaGFzQ2xhc3MoQ2xhc3NOYW1lLklOKSkge1xyXG4gICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdmFyIGFjdGl2ZXMgPSB1bmRlZmluZWQ7XHJcbiAgICAgICAgdmFyIGFjdGl2ZXNEYXRhID0gdW5kZWZpbmVkO1xyXG5cclxuICAgICAgICBpZiAodGhpcy5fcGFyZW50KSB7XHJcbiAgICAgICAgICBhY3RpdmVzID0gJC5tYWtlQXJyYXkoJChTZWxlY3Rvci5BQ1RJVkVTKSk7XHJcbiAgICAgICAgICBpZiAoIWFjdGl2ZXMubGVuZ3RoKSB7XHJcbiAgICAgICAgICAgIGFjdGl2ZXMgPSBudWxsO1xyXG4gICAgICAgICAgfVxyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgaWYgKGFjdGl2ZXMpIHtcclxuICAgICAgICAgIGFjdGl2ZXNEYXRhID0gJChhY3RpdmVzKS5kYXRhKERBVEFfS0VZKTtcclxuICAgICAgICAgIGlmIChhY3RpdmVzRGF0YSAmJiBhY3RpdmVzRGF0YS5faXNUcmFuc2l0aW9uaW5nKSB7XHJcbiAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgIH1cclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHZhciBzdGFydEV2ZW50ID0gJC5FdmVudChFdmVudC5TSE9XKTtcclxuICAgICAgICAkKHRoaXMuX2VsZW1lbnQpLnRyaWdnZXIoc3RhcnRFdmVudCk7XHJcbiAgICAgICAgaWYgKHN0YXJ0RXZlbnQuaXNEZWZhdWx0UHJldmVudGVkKCkpIHtcclxuICAgICAgICAgIHJldHVybjtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGlmIChhY3RpdmVzKSB7XHJcbiAgICAgICAgICBDb2xsYXBzZS5falF1ZXJ5SW50ZXJmYWNlLmNhbGwoJChhY3RpdmVzKSwgJ2hpZGUnKTtcclxuICAgICAgICAgIGlmICghYWN0aXZlc0RhdGEpIHtcclxuICAgICAgICAgICAgJChhY3RpdmVzKS5kYXRhKERBVEFfS0VZLCBudWxsKTtcclxuICAgICAgICAgIH1cclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHZhciBkaW1lbnNpb24gPSB0aGlzLl9nZXREaW1lbnNpb24oKTtcclxuXHJcbiAgICAgICAgJCh0aGlzLl9lbGVtZW50KS5yZW1vdmVDbGFzcyhDbGFzc05hbWUuQ09MTEFQU0UpLmFkZENsYXNzKENsYXNzTmFtZS5DT0xMQVBTSU5HKTtcclxuXHJcbiAgICAgICAgdGhpcy5fZWxlbWVudC5zdHlsZVtkaW1lbnNpb25dID0gMDtcclxuICAgICAgICB0aGlzLl9lbGVtZW50LnNldEF0dHJpYnV0ZSgnYXJpYS1leHBhbmRlZCcsIHRydWUpO1xyXG5cclxuICAgICAgICBpZiAodGhpcy5fdHJpZ2dlckFycmF5Lmxlbmd0aCkge1xyXG4gICAgICAgICAgJCh0aGlzLl90cmlnZ2VyQXJyYXkpLnJlbW92ZUNsYXNzKENsYXNzTmFtZS5DT0xMQVBTRUQpLmF0dHIoJ2FyaWEtZXhwYW5kZWQnLCB0cnVlKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHRoaXMuc2V0VHJhbnNpdGlvbmluZyh0cnVlKTtcclxuXHJcbiAgICAgICAgdmFyIGNvbXBsZXRlID0gZnVuY3Rpb24gY29tcGxldGUoKSB7XHJcbiAgICAgICAgICAkKF90aGlzLl9lbGVtZW50KS5yZW1vdmVDbGFzcyhDbGFzc05hbWUuQ09MTEFQU0lORykuYWRkQ2xhc3MoQ2xhc3NOYW1lLkNPTExBUFNFKS5hZGRDbGFzcyhDbGFzc05hbWUuSU4pO1xyXG5cclxuICAgICAgICAgIF90aGlzLl9lbGVtZW50LnN0eWxlW2RpbWVuc2lvbl0gPSAnJztcclxuXHJcbiAgICAgICAgICBfdGhpcy5zZXRUcmFuc2l0aW9uaW5nKGZhbHNlKTtcclxuXHJcbiAgICAgICAgICAkKF90aGlzLl9lbGVtZW50KS50cmlnZ2VyKEV2ZW50LlNIT1dOKTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICBpZiAoIVV0aWwuc3VwcG9ydHNUcmFuc2l0aW9uRW5kKCkpIHtcclxuICAgICAgICAgIGNvbXBsZXRlKCk7XHJcbiAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB2YXIgY2FwaXRhbGl6ZWREaW1lbnNpb24gPSBkaW1lbnNpb25bMF0udG9VcHBlckNhc2UoKSArIGRpbWVuc2lvbi5zbGljZSgxKTtcclxuICAgICAgICB2YXIgc2Nyb2xsU2l6ZSA9ICdzY3JvbGwnICsgY2FwaXRhbGl6ZWREaW1lbnNpb247XHJcblxyXG4gICAgICAgICQodGhpcy5fZWxlbWVudCkub25lKFV0aWwuVFJBTlNJVElPTl9FTkQsIGNvbXBsZXRlKS5lbXVsYXRlVHJhbnNpdGlvbkVuZChUUkFOU0lUSU9OX0RVUkFUSU9OKTtcclxuXHJcbiAgICAgICAgdGhpcy5fZWxlbWVudC5zdHlsZVtkaW1lbnNpb25dID0gdGhpcy5fZWxlbWVudFtzY3JvbGxTaXplXSArICdweCc7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnaGlkZScsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBoaWRlKCkge1xyXG4gICAgICAgIHZhciBfdGhpczIgPSB0aGlzO1xyXG5cclxuICAgICAgICBpZiAodGhpcy5faXNUcmFuc2l0aW9uaW5nIHx8ICEkKHRoaXMuX2VsZW1lbnQpLmhhc0NsYXNzKENsYXNzTmFtZS5JTikpIHtcclxuICAgICAgICAgIHJldHVybjtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHZhciBzdGFydEV2ZW50ID0gJC5FdmVudChFdmVudC5ISURFKTtcclxuICAgICAgICAkKHRoaXMuX2VsZW1lbnQpLnRyaWdnZXIoc3RhcnRFdmVudCk7XHJcbiAgICAgICAgaWYgKHN0YXJ0RXZlbnQuaXNEZWZhdWx0UHJldmVudGVkKCkpIHtcclxuICAgICAgICAgIHJldHVybjtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHZhciBkaW1lbnNpb24gPSB0aGlzLl9nZXREaW1lbnNpb24oKTtcclxuICAgICAgICB2YXIgb2Zmc2V0RGltZW5zaW9uID0gZGltZW5zaW9uID09PSBEaW1lbnNpb24uV0lEVEggPyAnb2Zmc2V0V2lkdGgnIDogJ29mZnNldEhlaWdodCc7XHJcblxyXG4gICAgICAgIHRoaXMuX2VsZW1lbnQuc3R5bGVbZGltZW5zaW9uXSA9IHRoaXMuX2VsZW1lbnRbb2Zmc2V0RGltZW5zaW9uXSArICdweCc7XHJcblxyXG4gICAgICAgIFV0aWwucmVmbG93KHRoaXMuX2VsZW1lbnQpO1xyXG5cclxuICAgICAgICAkKHRoaXMuX2VsZW1lbnQpLmFkZENsYXNzKENsYXNzTmFtZS5DT0xMQVBTSU5HKS5yZW1vdmVDbGFzcyhDbGFzc05hbWUuQ09MTEFQU0UpLnJlbW92ZUNsYXNzKENsYXNzTmFtZS5JTik7XHJcblxyXG4gICAgICAgIHRoaXMuX2VsZW1lbnQuc2V0QXR0cmlidXRlKCdhcmlhLWV4cGFuZGVkJywgZmFsc2UpO1xyXG5cclxuICAgICAgICBpZiAodGhpcy5fdHJpZ2dlckFycmF5Lmxlbmd0aCkge1xyXG4gICAgICAgICAgJCh0aGlzLl90cmlnZ2VyQXJyYXkpLmFkZENsYXNzKENsYXNzTmFtZS5DT0xMQVBTRUQpLmF0dHIoJ2FyaWEtZXhwYW5kZWQnLCBmYWxzZSk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB0aGlzLnNldFRyYW5zaXRpb25pbmcodHJ1ZSk7XHJcblxyXG4gICAgICAgIHZhciBjb21wbGV0ZSA9IGZ1bmN0aW9uIGNvbXBsZXRlKCkge1xyXG4gICAgICAgICAgX3RoaXMyLnNldFRyYW5zaXRpb25pbmcoZmFsc2UpO1xyXG4gICAgICAgICAgJChfdGhpczIuX2VsZW1lbnQpLnJlbW92ZUNsYXNzKENsYXNzTmFtZS5DT0xMQVBTSU5HKS5hZGRDbGFzcyhDbGFzc05hbWUuQ09MTEFQU0UpLnRyaWdnZXIoRXZlbnQuSElEREVOKTtcclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICB0aGlzLl9lbGVtZW50LnN0eWxlW2RpbWVuc2lvbl0gPSAwO1xyXG5cclxuICAgICAgICBpZiAoIVV0aWwuc3VwcG9ydHNUcmFuc2l0aW9uRW5kKCkpIHtcclxuICAgICAgICAgIGNvbXBsZXRlKCk7XHJcbiAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICAkKHRoaXMuX2VsZW1lbnQpLm9uZShVdGlsLlRSQU5TSVRJT05fRU5ELCBjb21wbGV0ZSkuZW11bGF0ZVRyYW5zaXRpb25FbmQoVFJBTlNJVElPTl9EVVJBVElPTik7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnc2V0VHJhbnNpdGlvbmluZycsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBzZXRUcmFuc2l0aW9uaW5nKGlzVHJhbnNpdGlvbmluZykge1xyXG4gICAgICAgIHRoaXMuX2lzVHJhbnNpdGlvbmluZyA9IGlzVHJhbnNpdGlvbmluZztcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdkaXNwb3NlJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIGRpc3Bvc2UoKSB7XHJcbiAgICAgICAgJC5yZW1vdmVEYXRhKHRoaXMuX2VsZW1lbnQsIERBVEFfS0VZKTtcclxuXHJcbiAgICAgICAgdGhpcy5fY29uZmlnID0gbnVsbDtcclxuICAgICAgICB0aGlzLl9wYXJlbnQgPSBudWxsO1xyXG4gICAgICAgIHRoaXMuX2VsZW1lbnQgPSBudWxsO1xyXG4gICAgICAgIHRoaXMuX3RyaWdnZXJBcnJheSA9IG51bGw7XHJcbiAgICAgICAgdGhpcy5faXNUcmFuc2l0aW9uaW5nID0gbnVsbDtcclxuICAgICAgfVxyXG5cclxuICAgICAgLy8gcHJpdmF0ZVxyXG5cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX2dldENvbmZpZycsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBfZ2V0Q29uZmlnKGNvbmZpZykge1xyXG4gICAgICAgIGNvbmZpZyA9ICQuZXh0ZW5kKHt9LCBEZWZhdWx0LCBjb25maWcpO1xyXG4gICAgICAgIGNvbmZpZy50b2dnbGUgPSBCb29sZWFuKGNvbmZpZy50b2dnbGUpOyAvLyBjb2VyY2Ugc3RyaW5nIHZhbHVlc1xyXG4gICAgICAgIFV0aWwudHlwZUNoZWNrQ29uZmlnKE5BTUUsIGNvbmZpZywgRGVmYXVsdFR5cGUpO1xyXG4gICAgICAgIHJldHVybiBjb25maWc7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX2dldERpbWVuc2lvbicsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBfZ2V0RGltZW5zaW9uKCkge1xyXG4gICAgICAgIHZhciBoYXNXaWR0aCA9ICQodGhpcy5fZWxlbWVudCkuaGFzQ2xhc3MoRGltZW5zaW9uLldJRFRIKTtcclxuICAgICAgICByZXR1cm4gaGFzV2lkdGggPyBEaW1lbnNpb24uV0lEVEggOiBEaW1lbnNpb24uSEVJR0hUO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19nZXRQYXJlbnQnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2dldFBhcmVudCgpIHtcclxuICAgICAgICB2YXIgX3RoaXMzID0gdGhpcztcclxuXHJcbiAgICAgICAgdmFyIHBhcmVudCA9ICQodGhpcy5fY29uZmlnLnBhcmVudClbMF07XHJcbiAgICAgICAgdmFyIHNlbGVjdG9yID0gJ1tkYXRhLXRvZ2dsZT1cImNvbGxhcHNlXCJdW2RhdGEtcGFyZW50PVwiJyArIHRoaXMuX2NvbmZpZy5wYXJlbnQgKyAnXCJdJztcclxuXHJcbiAgICAgICAgJChwYXJlbnQpLmZpbmQoc2VsZWN0b3IpLmVhY2goZnVuY3Rpb24gKGksIGVsZW1lbnQpIHtcclxuICAgICAgICAgIF90aGlzMy5fYWRkQXJpYUFuZENvbGxhcHNlZENsYXNzKENvbGxhcHNlLl9nZXRUYXJnZXRGcm9tRWxlbWVudChlbGVtZW50KSwgW2VsZW1lbnRdKTtcclxuICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgcmV0dXJuIHBhcmVudDtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfYWRkQXJpYUFuZENvbGxhcHNlZENsYXNzJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9hZGRBcmlhQW5kQ29sbGFwc2VkQ2xhc3MoZWxlbWVudCwgdHJpZ2dlckFycmF5KSB7XHJcbiAgICAgICAgaWYgKGVsZW1lbnQpIHtcclxuICAgICAgICAgIHZhciBpc09wZW4gPSAkKGVsZW1lbnQpLmhhc0NsYXNzKENsYXNzTmFtZS5JTik7XHJcbiAgICAgICAgICBlbGVtZW50LnNldEF0dHJpYnV0ZSgnYXJpYS1leHBhbmRlZCcsIGlzT3Blbik7XHJcblxyXG4gICAgICAgICAgaWYgKHRyaWdnZXJBcnJheS5sZW5ndGgpIHtcclxuICAgICAgICAgICAgJCh0cmlnZ2VyQXJyYXkpLnRvZ2dsZUNsYXNzKENsYXNzTmFtZS5DT0xMQVBTRUQsICFpc09wZW4pLmF0dHIoJ2FyaWEtZXhwYW5kZWQnLCBpc09wZW4pO1xyXG4gICAgICAgICAgfVxyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG5cclxuICAgICAgLy8gc3RhdGljXHJcblxyXG4gICAgfV0sIFt7XHJcbiAgICAgIGtleTogJ19nZXRUYXJnZXRGcm9tRWxlbWVudCcsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBfZ2V0VGFyZ2V0RnJvbUVsZW1lbnQoZWxlbWVudCkge1xyXG4gICAgICAgIHZhciBzZWxlY3RvciA9IFV0aWwuZ2V0U2VsZWN0b3JGcm9tRWxlbWVudChlbGVtZW50KTtcclxuICAgICAgICByZXR1cm4gc2VsZWN0b3IgPyAkKHNlbGVjdG9yKVswXSA6IG51bGw7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX2pRdWVyeUludGVyZmFjZScsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBfalF1ZXJ5SW50ZXJmYWNlKGNvbmZpZykge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmVhY2goZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgdmFyICR0aGlzID0gJCh0aGlzKTtcclxuICAgICAgICAgIHZhciBkYXRhID0gJHRoaXMuZGF0YShEQVRBX0tFWSk7XHJcbiAgICAgICAgICB2YXIgX2NvbmZpZyA9ICQuZXh0ZW5kKHt9LCBEZWZhdWx0LCAkdGhpcy5kYXRhKCksIHR5cGVvZiBjb25maWcgPT09ICdvYmplY3QnICYmIGNvbmZpZyk7XHJcblxyXG4gICAgICAgICAgaWYgKCFkYXRhICYmIF9jb25maWcudG9nZ2xlICYmIC9zaG93fGhpZGUvLnRlc3QoY29uZmlnKSkge1xyXG4gICAgICAgICAgICBfY29uZmlnLnRvZ2dsZSA9IGZhbHNlO1xyXG4gICAgICAgICAgfVxyXG5cclxuICAgICAgICAgIGlmICghZGF0YSkge1xyXG4gICAgICAgICAgICBkYXRhID0gbmV3IENvbGxhcHNlKHRoaXMsIF9jb25maWcpO1xyXG4gICAgICAgICAgICAkdGhpcy5kYXRhKERBVEFfS0VZLCBkYXRhKTtcclxuICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICBpZiAodHlwZW9mIGNvbmZpZyA9PT0gJ3N0cmluZycpIHtcclxuICAgICAgICAgICAgaWYgKGRhdGFbY29uZmlnXSA9PT0gdW5kZWZpbmVkKSB7XHJcbiAgICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKCdObyBtZXRob2QgbmFtZWQgXCInICsgY29uZmlnICsgJ1wiJyk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgZGF0YVtjb25maWddKCk7XHJcbiAgICAgICAgICB9XHJcbiAgICAgICAgfSk7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnVkVSU0lPTicsXHJcbiAgICAgIGdldDogZnVuY3Rpb24gZ2V0KCkge1xyXG4gICAgICAgIHJldHVybiBWRVJTSU9OO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ0RlZmF1bHQnLFxyXG4gICAgICBnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcclxuICAgICAgICByZXR1cm4gRGVmYXVsdDtcclxuICAgICAgfVxyXG4gICAgfV0pO1xyXG5cclxuICAgIHJldHVybiBDb2xsYXBzZTtcclxuICB9KSgpO1xyXG5cclxuICAkKGRvY3VtZW50KS5vbihFdmVudC5DTElDS19EQVRBX0FQSSwgU2VsZWN0b3IuREFUQV9UT0dHTEUsIGZ1bmN0aW9uIChldmVudCkge1xyXG4gICAgZXZlbnQucHJldmVudERlZmF1bHQoKTtcclxuXHJcbiAgICB2YXIgdGFyZ2V0ID0gQ29sbGFwc2UuX2dldFRhcmdldEZyb21FbGVtZW50KHRoaXMpO1xyXG4gICAgdmFyIGRhdGEgPSAkKHRhcmdldCkuZGF0YShEQVRBX0tFWSk7XHJcbiAgICB2YXIgY29uZmlnID0gZGF0YSA/ICd0b2dnbGUnIDogJCh0aGlzKS5kYXRhKCk7XHJcblxyXG4gICAgQ29sbGFwc2UuX2pRdWVyeUludGVyZmFjZS5jYWxsKCQodGFyZ2V0KSwgY29uZmlnKTtcclxuICB9KTtcclxuXHJcbiAgLyoqXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICogalF1ZXJ5XHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICovXHJcblxyXG4gICQuZm5bTkFNRV0gPSBDb2xsYXBzZS5falF1ZXJ5SW50ZXJmYWNlO1xyXG4gICQuZm5bTkFNRV0uQ29uc3RydWN0b3IgPSBDb2xsYXBzZTtcclxuICAkLmZuW05BTUVdLm5vQ29uZmxpY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAkLmZuW05BTUVdID0gSlFVRVJZX05PX0NPTkZMSUNUO1xyXG4gICAgcmV0dXJuIENvbGxhcHNlLl9qUXVlcnlJbnRlcmZhY2U7XHJcbiAgfTtcclxuXHJcbiAgcmV0dXJuIENvbGxhcHNlO1xyXG59KShqUXVlcnkpO1xyXG4vLyMgc291cmNlTWFwcGluZ1VSTD1jb2xsYXBzZS5qcy5tYXBcclxuIiwiJ3VzZSBzdHJpY3QnO1xyXG5cclxudmFyIF9jcmVhdGVDbGFzcyA9IChmdW5jdGlvbiAoKSB7IGZ1bmN0aW9uIGRlZmluZVByb3BlcnRpZXModGFyZ2V0LCBwcm9wcykgeyBmb3IgKHZhciBpID0gMDsgaSA8IHByb3BzLmxlbmd0aDsgaSsrKSB7IHZhciBkZXNjcmlwdG9yID0gcHJvcHNbaV07IGRlc2NyaXB0b3IuZW51bWVyYWJsZSA9IGRlc2NyaXB0b3IuZW51bWVyYWJsZSB8fCBmYWxzZTsgZGVzY3JpcHRvci5jb25maWd1cmFibGUgPSB0cnVlOyBpZiAoJ3ZhbHVlJyBpbiBkZXNjcmlwdG9yKSBkZXNjcmlwdG9yLndyaXRhYmxlID0gdHJ1ZTsgT2JqZWN0LmRlZmluZVByb3BlcnR5KHRhcmdldCwgZGVzY3JpcHRvci5rZXksIGRlc2NyaXB0b3IpOyB9IH0gcmV0dXJuIGZ1bmN0aW9uIChDb25zdHJ1Y3RvciwgcHJvdG9Qcm9wcywgc3RhdGljUHJvcHMpIHsgaWYgKHByb3RvUHJvcHMpIGRlZmluZVByb3BlcnRpZXMoQ29uc3RydWN0b3IucHJvdG90eXBlLCBwcm90b1Byb3BzKTsgaWYgKHN0YXRpY1Byb3BzKSBkZWZpbmVQcm9wZXJ0aWVzKENvbnN0cnVjdG9yLCBzdGF0aWNQcm9wcyk7IHJldHVybiBDb25zdHJ1Y3RvcjsgfTsgfSkoKTtcclxuXHJcbmZ1bmN0aW9uIF9jbGFzc0NhbGxDaGVjayhpbnN0YW5jZSwgQ29uc3RydWN0b3IpIHsgaWYgKCEoaW5zdGFuY2UgaW5zdGFuY2VvZiBDb25zdHJ1Y3RvcikpIHsgdGhyb3cgbmV3IFR5cGVFcnJvcignQ2Fubm90IGNhbGwgYSBjbGFzcyBhcyBhIGZ1bmN0aW9uJyk7IH0gfVxyXG5cclxuLyoqXHJcbiAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAqIEJvb3RzdHJhcCAodjQuMC4wLWFscGhhLjIpOiBkcm9wZG93bi5qc1xyXG4gKiBMaWNlbnNlZCB1bmRlciBNSVQgKGh0dHBzOi8vZ2l0aHViLmNvbS90d2JzL2Jvb3RzdHJhcC9ibG9iL21hc3Rlci9MSUNFTlNFKVxyXG4gKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gKi9cclxuXHJcbnZhciBEcm9wZG93biA9IChmdW5jdGlvbiAoJCkge1xyXG5cclxuICAvKipcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKiBDb25zdGFudHNcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKi9cclxuXHJcbiAgdmFyIE5BTUUgPSAnZHJvcGRvd24nO1xyXG4gIHZhciBWRVJTSU9OID0gJzQuMC4wLWFscGhhJztcclxuICB2YXIgREFUQV9LRVkgPSAnYnMuZHJvcGRvd24nO1xyXG4gIHZhciBFVkVOVF9LRVkgPSAnLicgKyBEQVRBX0tFWTtcclxuICB2YXIgREFUQV9BUElfS0VZID0gJy5kYXRhLWFwaSc7XHJcbiAgdmFyIEpRVUVSWV9OT19DT05GTElDVCA9ICQuZm5bTkFNRV07XHJcblxyXG4gIHZhciBFdmVudCA9IHtcclxuICAgIEhJREU6ICdoaWRlJyArIEVWRU5UX0tFWSxcclxuICAgIEhJRERFTjogJ2hpZGRlbicgKyBFVkVOVF9LRVksXHJcbiAgICBTSE9XOiAnc2hvdycgKyBFVkVOVF9LRVksXHJcbiAgICBTSE9XTjogJ3Nob3duJyArIEVWRU5UX0tFWSxcclxuICAgIENMSUNLOiAnY2xpY2snICsgRVZFTlRfS0VZLFxyXG4gICAgQ0xJQ0tfREFUQV9BUEk6ICdjbGljaycgKyBFVkVOVF9LRVkgKyBEQVRBX0FQSV9LRVksXHJcbiAgICBLRVlET1dOX0RBVEFfQVBJOiAna2V5ZG93bicgKyBFVkVOVF9LRVkgKyBEQVRBX0FQSV9LRVlcclxuICB9O1xyXG5cclxuICB2YXIgQ2xhc3NOYW1lID0ge1xyXG4gICAgQkFDS0RST1A6ICdkcm9wZG93bi1iYWNrZHJvcCcsXHJcbiAgICBESVNBQkxFRDogJ2Rpc2FibGVkJyxcclxuICAgIE9QRU46ICdvcGVuJ1xyXG4gIH07XHJcblxyXG4gIHZhciBTZWxlY3RvciA9IHtcclxuICAgIEJBQ0tEUk9QOiAnLmRyb3Bkb3duLWJhY2tkcm9wJyxcclxuICAgIERBVEFfVE9HR0xFOiAnW2RhdGEtdG9nZ2xlPVwiZHJvcGRvd25cIl0nLFxyXG4gICAgRk9STV9DSElMRDogJy5kcm9wZG93biBmb3JtJyxcclxuICAgIFJPTEVfTUVOVTogJ1tyb2xlPVwibWVudVwiXScsXHJcbiAgICBST0xFX0xJU1RCT1g6ICdbcm9sZT1cImxpc3Rib3hcIl0nLFxyXG4gICAgTkFWQkFSX05BVjogJy5uYXZiYXItbmF2JyxcclxuICAgIFZJU0lCTEVfSVRFTVM6ICdbcm9sZT1cIm1lbnVcIl0gbGk6bm90KC5kaXNhYmxlZCkgYSwgJyArICdbcm9sZT1cImxpc3Rib3hcIl0gbGk6bm90KC5kaXNhYmxlZCkgYSdcclxuICB9O1xyXG5cclxuICAvKipcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKiBDbGFzcyBEZWZpbml0aW9uXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICovXHJcblxyXG4gIHZhciBEcm9wZG93biA9IChmdW5jdGlvbiAoKSB7XHJcbiAgICBmdW5jdGlvbiBEcm9wZG93bihlbGVtZW50KSB7XHJcbiAgICAgIF9jbGFzc0NhbGxDaGVjayh0aGlzLCBEcm9wZG93bik7XHJcblxyXG4gICAgICB0aGlzLl9lbGVtZW50ID0gZWxlbWVudDtcclxuXHJcbiAgICAgIHRoaXMuX2FkZEV2ZW50TGlzdGVuZXJzKCk7XHJcbiAgICB9XHJcblxyXG4gICAgLyoqXHJcbiAgICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgICAqIERhdGEgQXBpIGltcGxlbWVudGF0aW9uXHJcbiAgICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgICAqL1xyXG5cclxuICAgIC8vIGdldHRlcnNcclxuXHJcbiAgICBfY3JlYXRlQ2xhc3MoRHJvcGRvd24sIFt7XHJcbiAgICAgIGtleTogJ3RvZ2dsZScsXHJcblxyXG4gICAgICAvLyBwdWJsaWNcclxuXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiB0b2dnbGUoKSB7XHJcbiAgICAgICAgaWYgKHRoaXMuZGlzYWJsZWQgfHwgJCh0aGlzKS5oYXNDbGFzcyhDbGFzc05hbWUuRElTQUJMRUQpKSB7XHJcbiAgICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB2YXIgcGFyZW50ID0gRHJvcGRvd24uX2dldFBhcmVudEZyb21FbGVtZW50KHRoaXMpO1xyXG4gICAgICAgIHZhciBpc0FjdGl2ZSA9ICQocGFyZW50KS5oYXNDbGFzcyhDbGFzc05hbWUuT1BFTik7XHJcblxyXG4gICAgICAgIERyb3Bkb3duLl9jbGVhck1lbnVzKCk7XHJcblxyXG4gICAgICAgIGlmIChpc0FjdGl2ZSkge1xyXG4gICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgaWYgKCdvbnRvdWNoc3RhcnQnIGluIGRvY3VtZW50LmRvY3VtZW50RWxlbWVudCAmJiAhJChwYXJlbnQpLmNsb3Nlc3QoU2VsZWN0b3IuTkFWQkFSX05BVikubGVuZ3RoKSB7XHJcblxyXG4gICAgICAgICAgLy8gaWYgbW9iaWxlIHdlIHVzZSBhIGJhY2tkcm9wIGJlY2F1c2UgY2xpY2sgZXZlbnRzIGRvbid0IGRlbGVnYXRlXHJcbiAgICAgICAgICB2YXIgZHJvcGRvd24gPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KCdkaXYnKTtcclxuICAgICAgICAgIGRyb3Bkb3duLmNsYXNzTmFtZSA9IENsYXNzTmFtZS5CQUNLRFJPUDtcclxuICAgICAgICAgICQoZHJvcGRvd24pLmluc2VydEJlZm9yZSh0aGlzKTtcclxuICAgICAgICAgICQoZHJvcGRvd24pLm9uKCdjbGljaycsIERyb3Bkb3duLl9jbGVhck1lbnVzKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHZhciByZWxhdGVkVGFyZ2V0ID0geyByZWxhdGVkVGFyZ2V0OiB0aGlzIH07XHJcbiAgICAgICAgdmFyIHNob3dFdmVudCA9ICQuRXZlbnQoRXZlbnQuU0hPVywgcmVsYXRlZFRhcmdldCk7XHJcblxyXG4gICAgICAgICQocGFyZW50KS50cmlnZ2VyKHNob3dFdmVudCk7XHJcblxyXG4gICAgICAgIGlmIChzaG93RXZlbnQuaXNEZWZhdWx0UHJldmVudGVkKCkpIHtcclxuICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHRoaXMuZm9jdXMoKTtcclxuICAgICAgICB0aGlzLnNldEF0dHJpYnV0ZSgnYXJpYS1leHBhbmRlZCcsICd0cnVlJyk7XHJcblxyXG4gICAgICAgICQocGFyZW50KS50b2dnbGVDbGFzcyhDbGFzc05hbWUuT1BFTik7XHJcbiAgICAgICAgJChwYXJlbnQpLnRyaWdnZXIoJC5FdmVudChFdmVudC5TSE9XTiwgcmVsYXRlZFRhcmdldCkpO1xyXG5cclxuICAgICAgICByZXR1cm4gZmFsc2U7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnZGlzcG9zZScsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBkaXNwb3NlKCkge1xyXG4gICAgICAgICQucmVtb3ZlRGF0YSh0aGlzLl9lbGVtZW50LCBEQVRBX0tFWSk7XHJcbiAgICAgICAgJCh0aGlzLl9lbGVtZW50KS5vZmYoRVZFTlRfS0VZKTtcclxuICAgICAgICB0aGlzLl9lbGVtZW50ID0gbnVsbDtcclxuICAgICAgfVxyXG5cclxuICAgICAgLy8gcHJpdmF0ZVxyXG5cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX2FkZEV2ZW50TGlzdGVuZXJzJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9hZGRFdmVudExpc3RlbmVycygpIHtcclxuICAgICAgICAkKHRoaXMuX2VsZW1lbnQpLm9uKEV2ZW50LkNMSUNLLCB0aGlzLnRvZ2dsZSk7XHJcbiAgICAgIH1cclxuXHJcbiAgICAgIC8vIHN0YXRpY1xyXG5cclxuICAgIH1dLCBbe1xyXG4gICAgICBrZXk6ICdfalF1ZXJ5SW50ZXJmYWNlJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9qUXVlcnlJbnRlcmZhY2UoY29uZmlnKSB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZWFjaChmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICB2YXIgZGF0YSA9ICQodGhpcykuZGF0YShEQVRBX0tFWSk7XHJcblxyXG4gICAgICAgICAgaWYgKCFkYXRhKSB7XHJcbiAgICAgICAgICAgICQodGhpcykuZGF0YShEQVRBX0tFWSwgZGF0YSA9IG5ldyBEcm9wZG93bih0aGlzKSk7XHJcbiAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgaWYgKHR5cGVvZiBjb25maWcgPT09ICdzdHJpbmcnKSB7XHJcbiAgICAgICAgICAgIGlmIChkYXRhW2NvbmZpZ10gPT09IHVuZGVmaW5lZCkge1xyXG4gICAgICAgICAgICAgIHRocm93IG5ldyBFcnJvcignTm8gbWV0aG9kIG5hbWVkIFwiJyArIGNvbmZpZyArICdcIicpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGRhdGFbY29uZmlnXS5jYWxsKHRoaXMpO1xyXG4gICAgICAgICAgfVxyXG4gICAgICAgIH0pO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19jbGVhck1lbnVzJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9jbGVhck1lbnVzKGV2ZW50KSB7XHJcbiAgICAgICAgaWYgKGV2ZW50ICYmIGV2ZW50LndoaWNoID09PSAzKSB7XHJcbiAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB2YXIgYmFja2Ryb3AgPSAkKFNlbGVjdG9yLkJBQ0tEUk9QKVswXTtcclxuICAgICAgICBpZiAoYmFja2Ryb3ApIHtcclxuICAgICAgICAgIGJhY2tkcm9wLnBhcmVudE5vZGUucmVtb3ZlQ2hpbGQoYmFja2Ryb3ApO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdmFyIHRvZ2dsZXMgPSAkLm1ha2VBcnJheSgkKFNlbGVjdG9yLkRBVEFfVE9HR0xFKSk7XHJcblxyXG4gICAgICAgIGZvciAodmFyIGkgPSAwOyBpIDwgdG9nZ2xlcy5sZW5ndGg7IGkrKykge1xyXG4gICAgICAgICAgdmFyIF9wYXJlbnQgPSBEcm9wZG93bi5fZ2V0UGFyZW50RnJvbUVsZW1lbnQodG9nZ2xlc1tpXSk7XHJcbiAgICAgICAgICB2YXIgcmVsYXRlZFRhcmdldCA9IHsgcmVsYXRlZFRhcmdldDogdG9nZ2xlc1tpXSB9O1xyXG5cclxuICAgICAgICAgIGlmICghJChfcGFyZW50KS5oYXNDbGFzcyhDbGFzc05hbWUuT1BFTikpIHtcclxuICAgICAgICAgICAgY29udGludWU7XHJcbiAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgaWYgKGV2ZW50ICYmIGV2ZW50LnR5cGUgPT09ICdjbGljaycgJiYgL2lucHV0fHRleHRhcmVhL2kudGVzdChldmVudC50YXJnZXQudGFnTmFtZSkgJiYgJC5jb250YWlucyhfcGFyZW50LCBldmVudC50YXJnZXQpKSB7XHJcbiAgICAgICAgICAgIGNvbnRpbnVlO1xyXG4gICAgICAgICAgfVxyXG5cclxuICAgICAgICAgIHZhciBoaWRlRXZlbnQgPSAkLkV2ZW50KEV2ZW50LkhJREUsIHJlbGF0ZWRUYXJnZXQpO1xyXG4gICAgICAgICAgJChfcGFyZW50KS50cmlnZ2VyKGhpZGVFdmVudCk7XHJcbiAgICAgICAgICBpZiAoaGlkZUV2ZW50LmlzRGVmYXVsdFByZXZlbnRlZCgpKSB7XHJcbiAgICAgICAgICAgIGNvbnRpbnVlO1xyXG4gICAgICAgICAgfVxyXG5cclxuICAgICAgICAgIHRvZ2dsZXNbaV0uc2V0QXR0cmlidXRlKCdhcmlhLWV4cGFuZGVkJywgJ2ZhbHNlJyk7XHJcblxyXG4gICAgICAgICAgJChfcGFyZW50KS5yZW1vdmVDbGFzcyhDbGFzc05hbWUuT1BFTikudHJpZ2dlcigkLkV2ZW50KEV2ZW50LkhJRERFTiwgcmVsYXRlZFRhcmdldCkpO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfZ2V0UGFyZW50RnJvbUVsZW1lbnQnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2dldFBhcmVudEZyb21FbGVtZW50KGVsZW1lbnQpIHtcclxuICAgICAgICB2YXIgcGFyZW50ID0gdW5kZWZpbmVkO1xyXG4gICAgICAgIHZhciBzZWxlY3RvciA9IFV0aWwuZ2V0U2VsZWN0b3JGcm9tRWxlbWVudChlbGVtZW50KTtcclxuXHJcbiAgICAgICAgaWYgKHNlbGVjdG9yKSB7XHJcbiAgICAgICAgICBwYXJlbnQgPSAkKHNlbGVjdG9yKVswXTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHJldHVybiBwYXJlbnQgfHwgZWxlbWVudC5wYXJlbnROb2RlO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19kYXRhQXBpS2V5ZG93bkhhbmRsZXInLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2RhdGFBcGlLZXlkb3duSGFuZGxlcihldmVudCkge1xyXG4gICAgICAgIGlmICghLygzOHw0MHwyN3wzMikvLnRlc3QoZXZlbnQud2hpY2gpIHx8IC9pbnB1dHx0ZXh0YXJlYS9pLnRlc3QoZXZlbnQudGFyZ2V0LnRhZ05hbWUpKSB7XHJcbiAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBldmVudC5wcmV2ZW50RGVmYXVsdCgpO1xyXG4gICAgICAgIGV2ZW50LnN0b3BQcm9wYWdhdGlvbigpO1xyXG5cclxuICAgICAgICBpZiAodGhpcy5kaXNhYmxlZCB8fCAkKHRoaXMpLmhhc0NsYXNzKENsYXNzTmFtZS5ESVNBQkxFRCkpIHtcclxuICAgICAgICAgIHJldHVybjtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHZhciBwYXJlbnQgPSBEcm9wZG93bi5fZ2V0UGFyZW50RnJvbUVsZW1lbnQodGhpcyk7XHJcbiAgICAgICAgdmFyIGlzQWN0aXZlID0gJChwYXJlbnQpLmhhc0NsYXNzKENsYXNzTmFtZS5PUEVOKTtcclxuXHJcbiAgICAgICAgaWYgKCFpc0FjdGl2ZSAmJiBldmVudC53aGljaCAhPT0gMjcgfHwgaXNBY3RpdmUgJiYgZXZlbnQud2hpY2ggPT09IDI3KSB7XHJcblxyXG4gICAgICAgICAgaWYgKGV2ZW50LndoaWNoID09PSAyNykge1xyXG4gICAgICAgICAgICB2YXIgdG9nZ2xlID0gJChwYXJlbnQpLmZpbmQoU2VsZWN0b3IuREFUQV9UT0dHTEUpWzBdO1xyXG4gICAgICAgICAgICAkKHRvZ2dsZSkudHJpZ2dlcignZm9jdXMnKTtcclxuICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAkKHRoaXMpLnRyaWdnZXIoJ2NsaWNrJyk7XHJcbiAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB2YXIgaXRlbXMgPSAkLm1ha2VBcnJheSgkKFNlbGVjdG9yLlZJU0lCTEVfSVRFTVMpKTtcclxuXHJcbiAgICAgICAgaXRlbXMgPSBpdGVtcy5maWx0ZXIoZnVuY3Rpb24gKGl0ZW0pIHtcclxuICAgICAgICAgIHJldHVybiBpdGVtLm9mZnNldFdpZHRoIHx8IGl0ZW0ub2Zmc2V0SGVpZ2h0O1xyXG4gICAgICAgIH0pO1xyXG5cclxuICAgICAgICBpZiAoIWl0ZW1zLmxlbmd0aCkge1xyXG4gICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdmFyIGluZGV4ID0gaXRlbXMuaW5kZXhPZihldmVudC50YXJnZXQpO1xyXG5cclxuICAgICAgICBpZiAoZXZlbnQud2hpY2ggPT09IDM4ICYmIGluZGV4ID4gMCkge1xyXG4gICAgICAgICAgLy8gdXBcclxuICAgICAgICAgIGluZGV4LS07XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBpZiAoZXZlbnQud2hpY2ggPT09IDQwICYmIGluZGV4IDwgaXRlbXMubGVuZ3RoIC0gMSkge1xyXG4gICAgICAgICAgLy8gZG93blxyXG4gICAgICAgICAgaW5kZXgrKztcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGlmICghIH5pbmRleCkge1xyXG4gICAgICAgICAgaW5kZXggPSAwO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgaXRlbXNbaW5kZXhdLmZvY3VzKCk7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnVkVSU0lPTicsXHJcbiAgICAgIGdldDogZnVuY3Rpb24gZ2V0KCkge1xyXG4gICAgICAgIHJldHVybiBWRVJTSU9OO1xyXG4gICAgICB9XHJcbiAgICB9XSk7XHJcblxyXG4gICAgcmV0dXJuIERyb3Bkb3duO1xyXG4gIH0pKCk7XHJcblxyXG4gICQoZG9jdW1lbnQpLm9uKEV2ZW50LktFWURPV05fREFUQV9BUEksIFNlbGVjdG9yLkRBVEFfVE9HR0xFLCBEcm9wZG93bi5fZGF0YUFwaUtleWRvd25IYW5kbGVyKS5vbihFdmVudC5LRVlET1dOX0RBVEFfQVBJLCBTZWxlY3Rvci5ST0xFX01FTlUsIERyb3Bkb3duLl9kYXRhQXBpS2V5ZG93bkhhbmRsZXIpLm9uKEV2ZW50LktFWURPV05fREFUQV9BUEksIFNlbGVjdG9yLlJPTEVfTElTVEJPWCwgRHJvcGRvd24uX2RhdGFBcGlLZXlkb3duSGFuZGxlcikub24oRXZlbnQuQ0xJQ0tfREFUQV9BUEksIERyb3Bkb3duLl9jbGVhck1lbnVzKS5vbihFdmVudC5DTElDS19EQVRBX0FQSSwgU2VsZWN0b3IuREFUQV9UT0dHTEUsIERyb3Bkb3duLnByb3RvdHlwZS50b2dnbGUpLm9uKEV2ZW50LkNMSUNLX0RBVEFfQVBJLCBTZWxlY3Rvci5GT1JNX0NISUxELCBmdW5jdGlvbiAoZSkge1xyXG4gICAgZS5zdG9wUHJvcGFnYXRpb24oKTtcclxuICB9KTtcclxuXHJcbiAgLyoqXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICogalF1ZXJ5XHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICovXHJcblxyXG4gICQuZm5bTkFNRV0gPSBEcm9wZG93bi5falF1ZXJ5SW50ZXJmYWNlO1xyXG4gICQuZm5bTkFNRV0uQ29uc3RydWN0b3IgPSBEcm9wZG93bjtcclxuICAkLmZuW05BTUVdLm5vQ29uZmxpY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAkLmZuW05BTUVdID0gSlFVRVJZX05PX0NPTkZMSUNUO1xyXG4gICAgcmV0dXJuIERyb3Bkb3duLl9qUXVlcnlJbnRlcmZhY2U7XHJcbiAgfTtcclxuXHJcbiAgcmV0dXJuIERyb3Bkb3duO1xyXG59KShqUXVlcnkpO1xyXG4vLyMgc291cmNlTWFwcGluZ1VSTD1kcm9wZG93bi5qcy5tYXBcclxuIiwiJ3VzZSBzdHJpY3QnO1xyXG5cclxudmFyIF9jcmVhdGVDbGFzcyA9IChmdW5jdGlvbiAoKSB7IGZ1bmN0aW9uIGRlZmluZVByb3BlcnRpZXModGFyZ2V0LCBwcm9wcykgeyBmb3IgKHZhciBpID0gMDsgaSA8IHByb3BzLmxlbmd0aDsgaSsrKSB7IHZhciBkZXNjcmlwdG9yID0gcHJvcHNbaV07IGRlc2NyaXB0b3IuZW51bWVyYWJsZSA9IGRlc2NyaXB0b3IuZW51bWVyYWJsZSB8fCBmYWxzZTsgZGVzY3JpcHRvci5jb25maWd1cmFibGUgPSB0cnVlOyBpZiAoJ3ZhbHVlJyBpbiBkZXNjcmlwdG9yKSBkZXNjcmlwdG9yLndyaXRhYmxlID0gdHJ1ZTsgT2JqZWN0LmRlZmluZVByb3BlcnR5KHRhcmdldCwgZGVzY3JpcHRvci5rZXksIGRlc2NyaXB0b3IpOyB9IH0gcmV0dXJuIGZ1bmN0aW9uIChDb25zdHJ1Y3RvciwgcHJvdG9Qcm9wcywgc3RhdGljUHJvcHMpIHsgaWYgKHByb3RvUHJvcHMpIGRlZmluZVByb3BlcnRpZXMoQ29uc3RydWN0b3IucHJvdG90eXBlLCBwcm90b1Byb3BzKTsgaWYgKHN0YXRpY1Byb3BzKSBkZWZpbmVQcm9wZXJ0aWVzKENvbnN0cnVjdG9yLCBzdGF0aWNQcm9wcyk7IHJldHVybiBDb25zdHJ1Y3RvcjsgfTsgfSkoKTtcclxuXHJcbmZ1bmN0aW9uIF9jbGFzc0NhbGxDaGVjayhpbnN0YW5jZSwgQ29uc3RydWN0b3IpIHsgaWYgKCEoaW5zdGFuY2UgaW5zdGFuY2VvZiBDb25zdHJ1Y3RvcikpIHsgdGhyb3cgbmV3IFR5cGVFcnJvcignQ2Fubm90IGNhbGwgYSBjbGFzcyBhcyBhIGZ1bmN0aW9uJyk7IH0gfVxyXG5cclxuLyoqXHJcbiAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAqIEJvb3RzdHJhcCAodjQuMC4wLWFscGhhLjIpOiBtb2RhbC5qc1xyXG4gKiBMaWNlbnNlZCB1bmRlciBNSVQgKGh0dHBzOi8vZ2l0aHViLmNvbS90d2JzL2Jvb3RzdHJhcC9ibG9iL21hc3Rlci9MSUNFTlNFKVxyXG4gKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gKi9cclxuXHJcbnZhciBNb2RhbCA9IChmdW5jdGlvbiAoJCkge1xyXG5cclxuICAvKipcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKiBDb25zdGFudHNcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKi9cclxuXHJcbiAgdmFyIE5BTUUgPSAnbW9kYWwnO1xyXG4gIHZhciBWRVJTSU9OID0gJzQuMC4wLWFscGhhJztcclxuICB2YXIgREFUQV9LRVkgPSAnYnMubW9kYWwnO1xyXG4gIHZhciBFVkVOVF9LRVkgPSAnLicgKyBEQVRBX0tFWTtcclxuICB2YXIgREFUQV9BUElfS0VZID0gJy5kYXRhLWFwaSc7XHJcbiAgdmFyIEpRVUVSWV9OT19DT05GTElDVCA9ICQuZm5bTkFNRV07XHJcbiAgdmFyIFRSQU5TSVRJT05fRFVSQVRJT04gPSAzMDA7XHJcbiAgdmFyIEJBQ0tEUk9QX1RSQU5TSVRJT05fRFVSQVRJT04gPSAxNTA7XHJcblxyXG4gIHZhciBEZWZhdWx0ID0ge1xyXG4gICAgYmFja2Ryb3A6IHRydWUsXHJcbiAgICBrZXlib2FyZDogdHJ1ZSxcclxuICAgIGZvY3VzOiB0cnVlLFxyXG4gICAgc2hvdzogdHJ1ZVxyXG4gIH07XHJcblxyXG4gIHZhciBEZWZhdWx0VHlwZSA9IHtcclxuICAgIGJhY2tkcm9wOiAnKGJvb2xlYW58c3RyaW5nKScsXHJcbiAgICBrZXlib2FyZDogJ2Jvb2xlYW4nLFxyXG4gICAgZm9jdXM6ICdib29sZWFuJyxcclxuICAgIHNob3c6ICdib29sZWFuJ1xyXG4gIH07XHJcblxyXG4gIHZhciBFdmVudCA9IHtcclxuICAgIEhJREU6ICdoaWRlJyArIEVWRU5UX0tFWSxcclxuICAgIEhJRERFTjogJ2hpZGRlbicgKyBFVkVOVF9LRVksXHJcbiAgICBTSE9XOiAnc2hvdycgKyBFVkVOVF9LRVksXHJcbiAgICBTSE9XTjogJ3Nob3duJyArIEVWRU5UX0tFWSxcclxuICAgIEZPQ1VTSU46ICdmb2N1c2luJyArIEVWRU5UX0tFWSxcclxuICAgIFJFU0laRTogJ3Jlc2l6ZScgKyBFVkVOVF9LRVksXHJcbiAgICBDTElDS19ESVNNSVNTOiAnY2xpY2suZGlzbWlzcycgKyBFVkVOVF9LRVksXHJcbiAgICBLRVlET1dOX0RJU01JU1M6ICdrZXlkb3duLmRpc21pc3MnICsgRVZFTlRfS0VZLFxyXG4gICAgTU9VU0VVUF9ESVNNSVNTOiAnbW91c2V1cC5kaXNtaXNzJyArIEVWRU5UX0tFWSxcclxuICAgIE1PVVNFRE9XTl9ESVNNSVNTOiAnbW91c2Vkb3duLmRpc21pc3MnICsgRVZFTlRfS0VZLFxyXG4gICAgQ0xJQ0tfREFUQV9BUEk6ICdjbGljaycgKyBFVkVOVF9LRVkgKyBEQVRBX0FQSV9LRVlcclxuICB9O1xyXG5cclxuICB2YXIgQ2xhc3NOYW1lID0ge1xyXG4gICAgU0NST0xMQkFSX01FQVNVUkVSOiAnbW9kYWwtc2Nyb2xsYmFyLW1lYXN1cmUnLFxyXG4gICAgQkFDS0RST1A6ICdtb2RhbC1iYWNrZHJvcCcsXHJcbiAgICBPUEVOOiAnbW9kYWwtb3BlbicsXHJcbiAgICBGQURFOiAnZmFkZScsXHJcbiAgICBJTjogJ2luJ1xyXG4gIH07XHJcblxyXG4gIHZhciBTZWxlY3RvciA9IHtcclxuICAgIERJQUxPRzogJy5tb2RhbC1kaWFsb2cnLFxyXG4gICAgREFUQV9UT0dHTEU6ICdbZGF0YS10b2dnbGU9XCJtb2RhbFwiXScsXHJcbiAgICBEQVRBX0RJU01JU1M6ICdbZGF0YS1kaXNtaXNzPVwibW9kYWxcIl0nLFxyXG4gICAgRklYRURfQ09OVEVOVDogJy5uYXZiYXItZml4ZWQtdG9wLCAubmF2YmFyLWZpeGVkLWJvdHRvbSwgLmlzLWZpeGVkJ1xyXG4gIH07XHJcblxyXG4gIC8qKlxyXG4gICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAqIENsYXNzIERlZmluaXRpb25cclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKi9cclxuXHJcbiAgdmFyIE1vZGFsID0gKGZ1bmN0aW9uICgpIHtcclxuICAgIGZ1bmN0aW9uIE1vZGFsKGVsZW1lbnQsIGNvbmZpZykge1xyXG4gICAgICBfY2xhc3NDYWxsQ2hlY2sodGhpcywgTW9kYWwpO1xyXG5cclxuICAgICAgdGhpcy5fY29uZmlnID0gdGhpcy5fZ2V0Q29uZmlnKGNvbmZpZyk7XHJcbiAgICAgIHRoaXMuX2VsZW1lbnQgPSBlbGVtZW50O1xyXG4gICAgICB0aGlzLl9kaWFsb2cgPSAkKGVsZW1lbnQpLmZpbmQoU2VsZWN0b3IuRElBTE9HKVswXTtcclxuICAgICAgdGhpcy5fYmFja2Ryb3AgPSBudWxsO1xyXG4gICAgICB0aGlzLl9pc1Nob3duID0gZmFsc2U7XHJcbiAgICAgIHRoaXMuX2lzQm9keU92ZXJmbG93aW5nID0gZmFsc2U7XHJcbiAgICAgIHRoaXMuX2lnbm9yZUJhY2tkcm9wQ2xpY2sgPSBmYWxzZTtcclxuICAgICAgdGhpcy5fb3JpZ2luYWxCb2R5UGFkZGluZyA9IDA7XHJcbiAgICAgIHRoaXMuX3Njcm9sbGJhcldpZHRoID0gMDtcclxuICAgIH1cclxuXHJcbiAgICAvKipcclxuICAgICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAgICogRGF0YSBBcGkgaW1wbGVtZW50YXRpb25cclxuICAgICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAgICovXHJcblxyXG4gICAgLy8gZ2V0dGVyc1xyXG5cclxuICAgIF9jcmVhdGVDbGFzcyhNb2RhbCwgW3tcclxuICAgICAga2V5OiAndG9nZ2xlJyxcclxuXHJcbiAgICAgIC8vIHB1YmxpY1xyXG5cclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIHRvZ2dsZShyZWxhdGVkVGFyZ2V0KSB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuX2lzU2hvd24gPyB0aGlzLmhpZGUoKSA6IHRoaXMuc2hvdyhyZWxhdGVkVGFyZ2V0KTtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdzaG93JyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIHNob3cocmVsYXRlZFRhcmdldCkge1xyXG4gICAgICAgIHZhciBfdGhpcyA9IHRoaXM7XHJcblxyXG4gICAgICAgIHZhciBzaG93RXZlbnQgPSAkLkV2ZW50KEV2ZW50LlNIT1csIHtcclxuICAgICAgICAgIHJlbGF0ZWRUYXJnZXQ6IHJlbGF0ZWRUYXJnZXRcclxuICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgJCh0aGlzLl9lbGVtZW50KS50cmlnZ2VyKHNob3dFdmVudCk7XHJcblxyXG4gICAgICAgIGlmICh0aGlzLl9pc1Nob3duIHx8IHNob3dFdmVudC5pc0RlZmF1bHRQcmV2ZW50ZWQoKSkge1xyXG4gICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdGhpcy5faXNTaG93biA9IHRydWU7XHJcblxyXG4gICAgICAgIHRoaXMuX2NoZWNrU2Nyb2xsYmFyKCk7XHJcbiAgICAgICAgdGhpcy5fc2V0U2Nyb2xsYmFyKCk7XHJcblxyXG4gICAgICAgICQoZG9jdW1lbnQuYm9keSkuYWRkQ2xhc3MoQ2xhc3NOYW1lLk9QRU4pO1xyXG5cclxuICAgICAgICB0aGlzLl9zZXRFc2NhcGVFdmVudCgpO1xyXG4gICAgICAgIHRoaXMuX3NldFJlc2l6ZUV2ZW50KCk7XHJcblxyXG4gICAgICAgICQodGhpcy5fZWxlbWVudCkub24oRXZlbnQuQ0xJQ0tfRElTTUlTUywgU2VsZWN0b3IuREFUQV9ESVNNSVNTLCAkLnByb3h5KHRoaXMuaGlkZSwgdGhpcykpO1xyXG5cclxuICAgICAgICAkKHRoaXMuX2RpYWxvZykub24oRXZlbnQuTU9VU0VET1dOX0RJU01JU1MsIGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICQoX3RoaXMuX2VsZW1lbnQpLm9uZShFdmVudC5NT1VTRVVQX0RJU01JU1MsIGZ1bmN0aW9uIChldmVudCkge1xyXG4gICAgICAgICAgICBpZiAoJChldmVudC50YXJnZXQpLmlzKF90aGlzLl9lbGVtZW50KSkge1xyXG4gICAgICAgICAgICAgIF90aGlzLl9pZ25vcmVCYWNrZHJvcENsaWNrID0gdHJ1ZTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgfSk7XHJcbiAgICAgICAgfSk7XHJcblxyXG4gICAgICAgIHRoaXMuX3Nob3dCYWNrZHJvcCgkLnByb3h5KHRoaXMuX3Nob3dFbGVtZW50LCB0aGlzLCByZWxhdGVkVGFyZ2V0KSk7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnaGlkZScsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBoaWRlKGV2ZW50KSB7XHJcbiAgICAgICAgaWYgKGV2ZW50KSB7XHJcbiAgICAgICAgICBldmVudC5wcmV2ZW50RGVmYXVsdCgpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdmFyIGhpZGVFdmVudCA9ICQuRXZlbnQoRXZlbnQuSElERSk7XHJcblxyXG4gICAgICAgICQodGhpcy5fZWxlbWVudCkudHJpZ2dlcihoaWRlRXZlbnQpO1xyXG5cclxuICAgICAgICBpZiAoIXRoaXMuX2lzU2hvd24gfHwgaGlkZUV2ZW50LmlzRGVmYXVsdFByZXZlbnRlZCgpKSB7XHJcbiAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB0aGlzLl9pc1Nob3duID0gZmFsc2U7XHJcblxyXG4gICAgICAgIHRoaXMuX3NldEVzY2FwZUV2ZW50KCk7XHJcbiAgICAgICAgdGhpcy5fc2V0UmVzaXplRXZlbnQoKTtcclxuXHJcbiAgICAgICAgJChkb2N1bWVudCkub2ZmKEV2ZW50LkZPQ1VTSU4pO1xyXG5cclxuICAgICAgICAkKHRoaXMuX2VsZW1lbnQpLnJlbW92ZUNsYXNzKENsYXNzTmFtZS5JTik7XHJcblxyXG4gICAgICAgICQodGhpcy5fZWxlbWVudCkub2ZmKEV2ZW50LkNMSUNLX0RJU01JU1MpO1xyXG4gICAgICAgICQodGhpcy5fZGlhbG9nKS5vZmYoRXZlbnQuTU9VU0VET1dOX0RJU01JU1MpO1xyXG5cclxuICAgICAgICBpZiAoVXRpbC5zdXBwb3J0c1RyYW5zaXRpb25FbmQoKSAmJiAkKHRoaXMuX2VsZW1lbnQpLmhhc0NsYXNzKENsYXNzTmFtZS5GQURFKSkge1xyXG5cclxuICAgICAgICAgICQodGhpcy5fZWxlbWVudCkub25lKFV0aWwuVFJBTlNJVElPTl9FTkQsICQucHJveHkodGhpcy5faGlkZU1vZGFsLCB0aGlzKSkuZW11bGF0ZVRyYW5zaXRpb25FbmQoVFJBTlNJVElPTl9EVVJBVElPTik7XHJcbiAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgIHRoaXMuX2hpZGVNb2RhbCgpO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdkaXNwb3NlJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIGRpc3Bvc2UoKSB7XHJcbiAgICAgICAgJC5yZW1vdmVEYXRhKHRoaXMuX2VsZW1lbnQsIERBVEFfS0VZKTtcclxuXHJcbiAgICAgICAgJCh3aW5kb3cpLm9mZihFVkVOVF9LRVkpO1xyXG4gICAgICAgICQoZG9jdW1lbnQpLm9mZihFVkVOVF9LRVkpO1xyXG4gICAgICAgICQodGhpcy5fZWxlbWVudCkub2ZmKEVWRU5UX0tFWSk7XHJcbiAgICAgICAgJCh0aGlzLl9iYWNrZHJvcCkub2ZmKEVWRU5UX0tFWSk7XHJcblxyXG4gICAgICAgIHRoaXMuX2NvbmZpZyA9IG51bGw7XHJcbiAgICAgICAgdGhpcy5fZWxlbWVudCA9IG51bGw7XHJcbiAgICAgICAgdGhpcy5fZGlhbG9nID0gbnVsbDtcclxuICAgICAgICB0aGlzLl9iYWNrZHJvcCA9IG51bGw7XHJcbiAgICAgICAgdGhpcy5faXNTaG93biA9IG51bGw7XHJcbiAgICAgICAgdGhpcy5faXNCb2R5T3ZlcmZsb3dpbmcgPSBudWxsO1xyXG4gICAgICAgIHRoaXMuX2lnbm9yZUJhY2tkcm9wQ2xpY2sgPSBudWxsO1xyXG4gICAgICAgIHRoaXMuX29yaWdpbmFsQm9keVBhZGRpbmcgPSBudWxsO1xyXG4gICAgICAgIHRoaXMuX3Njcm9sbGJhcldpZHRoID0gbnVsbDtcclxuICAgICAgfVxyXG5cclxuICAgICAgLy8gcHJpdmF0ZVxyXG5cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX2dldENvbmZpZycsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBfZ2V0Q29uZmlnKGNvbmZpZykge1xyXG4gICAgICAgIGNvbmZpZyA9ICQuZXh0ZW5kKHt9LCBEZWZhdWx0LCBjb25maWcpO1xyXG4gICAgICAgIFV0aWwudHlwZUNoZWNrQ29uZmlnKE5BTUUsIGNvbmZpZywgRGVmYXVsdFR5cGUpO1xyXG4gICAgICAgIHJldHVybiBjb25maWc7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX3Nob3dFbGVtZW50JyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9zaG93RWxlbWVudChyZWxhdGVkVGFyZ2V0KSB7XHJcbiAgICAgICAgdmFyIF90aGlzMiA9IHRoaXM7XHJcblxyXG4gICAgICAgIHZhciB0cmFuc2l0aW9uID0gVXRpbC5zdXBwb3J0c1RyYW5zaXRpb25FbmQoKSAmJiAkKHRoaXMuX2VsZW1lbnQpLmhhc0NsYXNzKENsYXNzTmFtZS5GQURFKTtcclxuXHJcbiAgICAgICAgaWYgKCF0aGlzLl9lbGVtZW50LnBhcmVudE5vZGUgfHwgdGhpcy5fZWxlbWVudC5wYXJlbnROb2RlLm5vZGVUeXBlICE9PSBOb2RlLkVMRU1FTlRfTk9ERSkge1xyXG4gICAgICAgICAgLy8gZG9uJ3QgbW92ZSBtb2RhbHMgZG9tIHBvc2l0aW9uXHJcbiAgICAgICAgICBkb2N1bWVudC5ib2R5LmFwcGVuZENoaWxkKHRoaXMuX2VsZW1lbnQpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdGhpcy5fZWxlbWVudC5zdHlsZS5kaXNwbGF5ID0gJ2Jsb2NrJztcclxuICAgICAgICB0aGlzLl9lbGVtZW50LnNjcm9sbFRvcCA9IDA7XHJcblxyXG4gICAgICAgIGlmICh0cmFuc2l0aW9uKSB7XHJcbiAgICAgICAgICBVdGlsLnJlZmxvdyh0aGlzLl9lbGVtZW50KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgICQodGhpcy5fZWxlbWVudCkuYWRkQ2xhc3MoQ2xhc3NOYW1lLklOKTtcclxuXHJcbiAgICAgICAgaWYgKHRoaXMuX2NvbmZpZy5mb2N1cykge1xyXG4gICAgICAgICAgdGhpcy5fZW5mb3JjZUZvY3VzKCk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB2YXIgc2hvd25FdmVudCA9ICQuRXZlbnQoRXZlbnQuU0hPV04sIHtcclxuICAgICAgICAgIHJlbGF0ZWRUYXJnZXQ6IHJlbGF0ZWRUYXJnZXRcclxuICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgdmFyIHRyYW5zaXRpb25Db21wbGV0ZSA9IGZ1bmN0aW9uIHRyYW5zaXRpb25Db21wbGV0ZSgpIHtcclxuICAgICAgICAgIGlmIChfdGhpczIuX2NvbmZpZy5mb2N1cykge1xyXG4gICAgICAgICAgICBfdGhpczIuX2VsZW1lbnQuZm9jdXMoKTtcclxuICAgICAgICAgIH1cclxuICAgICAgICAgICQoX3RoaXMyLl9lbGVtZW50KS50cmlnZ2VyKHNob3duRXZlbnQpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIGlmICh0cmFuc2l0aW9uKSB7XHJcbiAgICAgICAgICAkKHRoaXMuX2RpYWxvZykub25lKFV0aWwuVFJBTlNJVElPTl9FTkQsIHRyYW5zaXRpb25Db21wbGV0ZSkuZW11bGF0ZVRyYW5zaXRpb25FbmQoVFJBTlNJVElPTl9EVVJBVElPTik7XHJcbiAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgIHRyYW5zaXRpb25Db21wbGV0ZSgpO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfZW5mb3JjZUZvY3VzJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9lbmZvcmNlRm9jdXMoKSB7XHJcbiAgICAgICAgdmFyIF90aGlzMyA9IHRoaXM7XHJcblxyXG4gICAgICAgICQoZG9jdW1lbnQpLm9mZihFdmVudC5GT0NVU0lOKSAvLyBndWFyZCBhZ2FpbnN0IGluZmluaXRlIGZvY3VzIGxvb3BcclxuICAgICAgICAub24oRXZlbnQuRk9DVVNJTiwgZnVuY3Rpb24gKGV2ZW50KSB7XHJcbiAgICAgICAgICBpZiAoX3RoaXMzLl9lbGVtZW50ICE9PSBldmVudC50YXJnZXQgJiYgISQoX3RoaXMzLl9lbGVtZW50KS5oYXMoZXZlbnQudGFyZ2V0KS5sZW5ndGgpIHtcclxuICAgICAgICAgICAgX3RoaXMzLl9lbGVtZW50LmZvY3VzKCk7XHJcbiAgICAgICAgICB9XHJcbiAgICAgICAgfSk7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX3NldEVzY2FwZUV2ZW50JyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9zZXRFc2NhcGVFdmVudCgpIHtcclxuICAgICAgICB2YXIgX3RoaXM0ID0gdGhpcztcclxuXHJcbiAgICAgICAgaWYgKHRoaXMuX2lzU2hvd24gJiYgdGhpcy5fY29uZmlnLmtleWJvYXJkKSB7XHJcbiAgICAgICAgICAkKHRoaXMuX2VsZW1lbnQpLm9uKEV2ZW50LktFWURPV05fRElTTUlTUywgZnVuY3Rpb24gKGV2ZW50KSB7XHJcbiAgICAgICAgICAgIGlmIChldmVudC53aGljaCA9PT0gMjcpIHtcclxuICAgICAgICAgICAgICBfdGhpczQuaGlkZSgpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICB9KTtcclxuICAgICAgICB9IGVsc2UgaWYgKCF0aGlzLl9pc1Nob3duKSB7XHJcbiAgICAgICAgICAkKHRoaXMuX2VsZW1lbnQpLm9mZihFdmVudC5LRVlET1dOX0RJU01JU1MpO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfc2V0UmVzaXplRXZlbnQnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX3NldFJlc2l6ZUV2ZW50KCkge1xyXG4gICAgICAgIGlmICh0aGlzLl9pc1Nob3duKSB7XHJcbiAgICAgICAgICAkKHdpbmRvdykub24oRXZlbnQuUkVTSVpFLCAkLnByb3h5KHRoaXMuX2hhbmRsZVVwZGF0ZSwgdGhpcykpO1xyXG4gICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAkKHdpbmRvdykub2ZmKEV2ZW50LlJFU0laRSk7XHJcbiAgICAgICAgfVxyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19oaWRlTW9kYWwnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2hpZGVNb2RhbCgpIHtcclxuICAgICAgICB2YXIgX3RoaXM1ID0gdGhpcztcclxuXHJcbiAgICAgICAgdGhpcy5fZWxlbWVudC5zdHlsZS5kaXNwbGF5ID0gJ25vbmUnO1xyXG4gICAgICAgIHRoaXMuX3Nob3dCYWNrZHJvcChmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAkKGRvY3VtZW50LmJvZHkpLnJlbW92ZUNsYXNzKENsYXNzTmFtZS5PUEVOKTtcclxuICAgICAgICAgIF90aGlzNS5fcmVzZXRBZGp1c3RtZW50cygpO1xyXG4gICAgICAgICAgX3RoaXM1Ll9yZXNldFNjcm9sbGJhcigpO1xyXG4gICAgICAgICAgJChfdGhpczUuX2VsZW1lbnQpLnRyaWdnZXIoRXZlbnQuSElEREVOKTtcclxuICAgICAgICB9KTtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfcmVtb3ZlQmFja2Ryb3AnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX3JlbW92ZUJhY2tkcm9wKCkge1xyXG4gICAgICAgIGlmICh0aGlzLl9iYWNrZHJvcCkge1xyXG4gICAgICAgICAgJCh0aGlzLl9iYWNrZHJvcCkucmVtb3ZlKCk7XHJcbiAgICAgICAgICB0aGlzLl9iYWNrZHJvcCA9IG51bGw7XHJcbiAgICAgICAgfVxyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19zaG93QmFja2Ryb3AnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX3Nob3dCYWNrZHJvcChjYWxsYmFjaykge1xyXG4gICAgICAgIHZhciBfdGhpczYgPSB0aGlzO1xyXG5cclxuICAgICAgICB2YXIgYW5pbWF0ZSA9ICQodGhpcy5fZWxlbWVudCkuaGFzQ2xhc3MoQ2xhc3NOYW1lLkZBREUpID8gQ2xhc3NOYW1lLkZBREUgOiAnJztcclxuXHJcbiAgICAgICAgaWYgKHRoaXMuX2lzU2hvd24gJiYgdGhpcy5fY29uZmlnLmJhY2tkcm9wKSB7XHJcbiAgICAgICAgICB2YXIgZG9BbmltYXRlID0gVXRpbC5zdXBwb3J0c1RyYW5zaXRpb25FbmQoKSAmJiBhbmltYXRlO1xyXG5cclxuICAgICAgICAgIHRoaXMuX2JhY2tkcm9wID0gZG9jdW1lbnQuY3JlYXRlRWxlbWVudCgnZGl2Jyk7XHJcbiAgICAgICAgICB0aGlzLl9iYWNrZHJvcC5jbGFzc05hbWUgPSBDbGFzc05hbWUuQkFDS0RST1A7XHJcblxyXG4gICAgICAgICAgaWYgKGFuaW1hdGUpIHtcclxuICAgICAgICAgICAgJCh0aGlzLl9iYWNrZHJvcCkuYWRkQ2xhc3MoYW5pbWF0ZSk7XHJcbiAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgJCh0aGlzLl9iYWNrZHJvcCkuYXBwZW5kVG8oZG9jdW1lbnQuYm9keSk7XHJcblxyXG4gICAgICAgICAgJCh0aGlzLl9lbGVtZW50KS5vbihFdmVudC5DTElDS19ESVNNSVNTLCBmdW5jdGlvbiAoZXZlbnQpIHtcclxuICAgICAgICAgICAgaWYgKF90aGlzNi5faWdub3JlQmFja2Ryb3BDbGljaykge1xyXG4gICAgICAgICAgICAgIF90aGlzNi5faWdub3JlQmFja2Ryb3BDbGljayA9IGZhbHNlO1xyXG4gICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBpZiAoZXZlbnQudGFyZ2V0ICE9PSBldmVudC5jdXJyZW50VGFyZ2V0KSB7XHJcbiAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGlmIChfdGhpczYuX2NvbmZpZy5iYWNrZHJvcCA9PT0gJ3N0YXRpYycpIHtcclxuICAgICAgICAgICAgICBfdGhpczYuX2VsZW1lbnQuZm9jdXMoKTtcclxuICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICBfdGhpczYuaGlkZSgpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICBpZiAoZG9BbmltYXRlKSB7XHJcbiAgICAgICAgICAgIFV0aWwucmVmbG93KHRoaXMuX2JhY2tkcm9wKTtcclxuICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAkKHRoaXMuX2JhY2tkcm9wKS5hZGRDbGFzcyhDbGFzc05hbWUuSU4pO1xyXG5cclxuICAgICAgICAgIGlmICghY2FsbGJhY2spIHtcclxuICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgfVxyXG5cclxuICAgICAgICAgIGlmICghZG9BbmltYXRlKSB7XHJcbiAgICAgICAgICAgIGNhbGxiYWNrKCk7XHJcbiAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAkKHRoaXMuX2JhY2tkcm9wKS5vbmUoVXRpbC5UUkFOU0lUSU9OX0VORCwgY2FsbGJhY2spLmVtdWxhdGVUcmFuc2l0aW9uRW5kKEJBQ0tEUk9QX1RSQU5TSVRJT05fRFVSQVRJT04pO1xyXG4gICAgICAgIH0gZWxzZSBpZiAoIXRoaXMuX2lzU2hvd24gJiYgdGhpcy5fYmFja2Ryb3ApIHtcclxuICAgICAgICAgICQodGhpcy5fYmFja2Ryb3ApLnJlbW92ZUNsYXNzKENsYXNzTmFtZS5JTik7XHJcblxyXG4gICAgICAgICAgdmFyIGNhbGxiYWNrUmVtb3ZlID0gZnVuY3Rpb24gY2FsbGJhY2tSZW1vdmUoKSB7XHJcbiAgICAgICAgICAgIF90aGlzNi5fcmVtb3ZlQmFja2Ryb3AoKTtcclxuICAgICAgICAgICAgaWYgKGNhbGxiYWNrKSB7XHJcbiAgICAgICAgICAgICAgY2FsbGJhY2soKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgfTtcclxuXHJcbiAgICAgICAgICBpZiAoVXRpbC5zdXBwb3J0c1RyYW5zaXRpb25FbmQoKSAmJiAkKHRoaXMuX2VsZW1lbnQpLmhhc0NsYXNzKENsYXNzTmFtZS5GQURFKSkge1xyXG4gICAgICAgICAgICAkKHRoaXMuX2JhY2tkcm9wKS5vbmUoVXRpbC5UUkFOU0lUSU9OX0VORCwgY2FsbGJhY2tSZW1vdmUpLmVtdWxhdGVUcmFuc2l0aW9uRW5kKEJBQ0tEUk9QX1RSQU5TSVRJT05fRFVSQVRJT04pO1xyXG4gICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgY2FsbGJhY2tSZW1vdmUoKTtcclxuICAgICAgICAgIH1cclxuICAgICAgICB9IGVsc2UgaWYgKGNhbGxiYWNrKSB7XHJcbiAgICAgICAgICBjYWxsYmFjaygpO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG5cclxuICAgICAgLy8gLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAgICAvLyB0aGUgZm9sbG93aW5nIG1ldGhvZHMgYXJlIHVzZWQgdG8gaGFuZGxlIG92ZXJmbG93aW5nIG1vZGFsc1xyXG4gICAgICAvLyB0b2RvIChmYXQpOiB0aGVzZSBzaG91bGQgcHJvYmFibHkgYmUgcmVmYWN0b3JlZCBvdXQgb2YgbW9kYWwuanNcclxuICAgICAgLy8gLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG5cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX2hhbmRsZVVwZGF0ZScsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBfaGFuZGxlVXBkYXRlKCkge1xyXG4gICAgICAgIHRoaXMuX2FkanVzdERpYWxvZygpO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19hZGp1c3REaWFsb2cnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2FkanVzdERpYWxvZygpIHtcclxuICAgICAgICB2YXIgaXNNb2RhbE92ZXJmbG93aW5nID0gdGhpcy5fZWxlbWVudC5zY3JvbGxIZWlnaHQgPiBkb2N1bWVudC5kb2N1bWVudEVsZW1lbnQuY2xpZW50SGVpZ2h0O1xyXG5cclxuICAgICAgICBpZiAoIXRoaXMuX2lzQm9keU92ZXJmbG93aW5nICYmIGlzTW9kYWxPdmVyZmxvd2luZykge1xyXG4gICAgICAgICAgdGhpcy5fZWxlbWVudC5zdHlsZS5wYWRkaW5nTGVmdCA9IHRoaXMuX3Njcm9sbGJhcldpZHRoICsgJ3B4JztcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGlmICh0aGlzLl9pc0JvZHlPdmVyZmxvd2luZyAmJiAhaXNNb2RhbE92ZXJmbG93aW5nKSB7XHJcbiAgICAgICAgICB0aGlzLl9lbGVtZW50LnN0eWxlLnBhZGRpbmdSaWdodCA9IHRoaXMuX3Njcm9sbGJhcldpZHRoICsgJ3B4fic7XHJcbiAgICAgICAgfVxyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19yZXNldEFkanVzdG1lbnRzJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9yZXNldEFkanVzdG1lbnRzKCkge1xyXG4gICAgICAgIHRoaXMuX2VsZW1lbnQuc3R5bGUucGFkZGluZ0xlZnQgPSAnJztcclxuICAgICAgICB0aGlzLl9lbGVtZW50LnN0eWxlLnBhZGRpbmdSaWdodCA9ICcnO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19jaGVja1Njcm9sbGJhcicsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBfY2hlY2tTY3JvbGxiYXIoKSB7XHJcbiAgICAgICAgdmFyIGZ1bGxXaW5kb3dXaWR0aCA9IHdpbmRvdy5pbm5lcldpZHRoO1xyXG4gICAgICAgIGlmICghZnVsbFdpbmRvd1dpZHRoKSB7XHJcbiAgICAgICAgICAvLyB3b3JrYXJvdW5kIGZvciBtaXNzaW5nIHdpbmRvdy5pbm5lcldpZHRoIGluIElFOFxyXG4gICAgICAgICAgdmFyIGRvY3VtZW50RWxlbWVudFJlY3QgPSBkb2N1bWVudC5kb2N1bWVudEVsZW1lbnQuZ2V0Qm91bmRpbmdDbGllbnRSZWN0KCk7XHJcbiAgICAgICAgICBmdWxsV2luZG93V2lkdGggPSBkb2N1bWVudEVsZW1lbnRSZWN0LnJpZ2h0IC0gTWF0aC5hYnMoZG9jdW1lbnRFbGVtZW50UmVjdC5sZWZ0KTtcclxuICAgICAgICB9XHJcbiAgICAgICAgdGhpcy5faXNCb2R5T3ZlcmZsb3dpbmcgPSBkb2N1bWVudC5ib2R5LmNsaWVudFdpZHRoIDwgZnVsbFdpbmRvd1dpZHRoO1xyXG4gICAgICAgIHRoaXMuX3Njcm9sbGJhcldpZHRoID0gdGhpcy5fZ2V0U2Nyb2xsYmFyV2lkdGgoKTtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfc2V0U2Nyb2xsYmFyJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9zZXRTY3JvbGxiYXIoKSB7XHJcbiAgICAgICAgdmFyIGJvZHlQYWRkaW5nID0gcGFyc2VJbnQoJChTZWxlY3Rvci5GSVhFRF9DT05URU5UKS5jc3MoJ3BhZGRpbmctcmlnaHQnKSB8fCAwLCAxMCk7XHJcblxyXG4gICAgICAgIHRoaXMuX29yaWdpbmFsQm9keVBhZGRpbmcgPSBkb2N1bWVudC5ib2R5LnN0eWxlLnBhZGRpbmdSaWdodCB8fCAnJztcclxuXHJcbiAgICAgICAgaWYgKHRoaXMuX2lzQm9keU92ZXJmbG93aW5nKSB7XHJcbiAgICAgICAgICBkb2N1bWVudC5ib2R5LnN0eWxlLnBhZGRpbmdSaWdodCA9IGJvZHlQYWRkaW5nICsgdGhpcy5fc2Nyb2xsYmFyV2lkdGggKyAncHgnO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfcmVzZXRTY3JvbGxiYXInLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX3Jlc2V0U2Nyb2xsYmFyKCkge1xyXG4gICAgICAgIGRvY3VtZW50LmJvZHkuc3R5bGUucGFkZGluZ1JpZ2h0ID0gdGhpcy5fb3JpZ2luYWxCb2R5UGFkZGluZztcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfZ2V0U2Nyb2xsYmFyV2lkdGgnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2dldFNjcm9sbGJhcldpZHRoKCkge1xyXG4gICAgICAgIC8vIHRoeCBkLndhbHNoXHJcbiAgICAgICAgdmFyIHNjcm9sbERpdiA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoJ2RpdicpO1xyXG4gICAgICAgIHNjcm9sbERpdi5jbGFzc05hbWUgPSBDbGFzc05hbWUuU0NST0xMQkFSX01FQVNVUkVSO1xyXG4gICAgICAgIGRvY3VtZW50LmJvZHkuYXBwZW5kQ2hpbGQoc2Nyb2xsRGl2KTtcclxuICAgICAgICB2YXIgc2Nyb2xsYmFyV2lkdGggPSBzY3JvbGxEaXYub2Zmc2V0V2lkdGggLSBzY3JvbGxEaXYuY2xpZW50V2lkdGg7XHJcbiAgICAgICAgZG9jdW1lbnQuYm9keS5yZW1vdmVDaGlsZChzY3JvbGxEaXYpO1xyXG4gICAgICAgIHJldHVybiBzY3JvbGxiYXJXaWR0aDtcclxuICAgICAgfVxyXG5cclxuICAgICAgLy8gc3RhdGljXHJcblxyXG4gICAgfV0sIFt7XHJcbiAgICAgIGtleTogJ19qUXVlcnlJbnRlcmZhY2UnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2pRdWVyeUludGVyZmFjZShjb25maWcsIHJlbGF0ZWRUYXJnZXQpIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5lYWNoKGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgIHZhciBkYXRhID0gJCh0aGlzKS5kYXRhKERBVEFfS0VZKTtcclxuICAgICAgICAgIHZhciBfY29uZmlnID0gJC5leHRlbmQoe30sIE1vZGFsLkRlZmF1bHQsICQodGhpcykuZGF0YSgpLCB0eXBlb2YgY29uZmlnID09PSAnb2JqZWN0JyAmJiBjb25maWcpO1xyXG5cclxuICAgICAgICAgIGlmICghZGF0YSkge1xyXG4gICAgICAgICAgICBkYXRhID0gbmV3IE1vZGFsKHRoaXMsIF9jb25maWcpO1xyXG4gICAgICAgICAgICAkKHRoaXMpLmRhdGEoREFUQV9LRVksIGRhdGEpO1xyXG4gICAgICAgICAgfVxyXG5cclxuICAgICAgICAgIGlmICh0eXBlb2YgY29uZmlnID09PSAnc3RyaW5nJykge1xyXG4gICAgICAgICAgICBpZiAoZGF0YVtjb25maWddID09PSB1bmRlZmluZWQpIHtcclxuICAgICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoJ05vIG1ldGhvZCBuYW1lZCBcIicgKyBjb25maWcgKyAnXCInKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBkYXRhW2NvbmZpZ10ocmVsYXRlZFRhcmdldCk7XHJcbiAgICAgICAgICB9IGVsc2UgaWYgKF9jb25maWcuc2hvdykge1xyXG4gICAgICAgICAgICBkYXRhLnNob3cocmVsYXRlZFRhcmdldCk7XHJcbiAgICAgICAgICB9XHJcbiAgICAgICAgfSk7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnVkVSU0lPTicsXHJcbiAgICAgIGdldDogZnVuY3Rpb24gZ2V0KCkge1xyXG4gICAgICAgIHJldHVybiBWRVJTSU9OO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ0RlZmF1bHQnLFxyXG4gICAgICBnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcclxuICAgICAgICByZXR1cm4gRGVmYXVsdDtcclxuICAgICAgfVxyXG4gICAgfV0pO1xyXG5cclxuICAgIHJldHVybiBNb2RhbDtcclxuICB9KSgpO1xyXG5cclxuICAkKGRvY3VtZW50KS5vbihFdmVudC5DTElDS19EQVRBX0FQSSwgU2VsZWN0b3IuREFUQV9UT0dHTEUsIGZ1bmN0aW9uIChldmVudCkge1xyXG4gICAgdmFyIF90aGlzNyA9IHRoaXM7XHJcblxyXG4gICAgdmFyIHRhcmdldCA9IHVuZGVmaW5lZDtcclxuICAgIHZhciBzZWxlY3RvciA9IFV0aWwuZ2V0U2VsZWN0b3JGcm9tRWxlbWVudCh0aGlzKTtcclxuXHJcbiAgICBpZiAoc2VsZWN0b3IpIHtcclxuICAgICAgdGFyZ2V0ID0gJChzZWxlY3RvcilbMF07XHJcbiAgICB9XHJcblxyXG4gICAgdmFyIGNvbmZpZyA9ICQodGFyZ2V0KS5kYXRhKERBVEFfS0VZKSA/ICd0b2dnbGUnIDogJC5leHRlbmQoe30sICQodGFyZ2V0KS5kYXRhKCksICQodGhpcykuZGF0YSgpKTtcclxuXHJcbiAgICBpZiAodGhpcy50YWdOYW1lID09PSAnQScpIHtcclxuICAgICAgZXZlbnQucHJldmVudERlZmF1bHQoKTtcclxuICAgIH1cclxuXHJcbiAgICB2YXIgJHRhcmdldCA9ICQodGFyZ2V0KS5vbmUoRXZlbnQuU0hPVywgZnVuY3Rpb24gKHNob3dFdmVudCkge1xyXG4gICAgICBpZiAoc2hvd0V2ZW50LmlzRGVmYXVsdFByZXZlbnRlZCgpKSB7XHJcbiAgICAgICAgLy8gb25seSByZWdpc3RlciBmb2N1cyByZXN0b3JlciBpZiBtb2RhbCB3aWxsIGFjdHVhbGx5IGdldCBzaG93blxyXG4gICAgICAgIHJldHVybjtcclxuICAgICAgfVxyXG5cclxuICAgICAgJHRhcmdldC5vbmUoRXZlbnQuSElEREVOLCBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgaWYgKCQoX3RoaXM3KS5pcygnOnZpc2libGUnKSkge1xyXG4gICAgICAgICAgX3RoaXM3LmZvY3VzKCk7XHJcbiAgICAgICAgfVxyXG4gICAgICB9KTtcclxuICAgIH0pO1xyXG5cclxuICAgIE1vZGFsLl9qUXVlcnlJbnRlcmZhY2UuY2FsbCgkKHRhcmdldCksIGNvbmZpZywgdGhpcyk7XHJcbiAgfSk7XHJcblxyXG4gIC8qKlxyXG4gICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAqIGpRdWVyeVxyXG4gICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAqL1xyXG5cclxuICAkLmZuW05BTUVdID0gTW9kYWwuX2pRdWVyeUludGVyZmFjZTtcclxuICAkLmZuW05BTUVdLkNvbnN0cnVjdG9yID0gTW9kYWw7XHJcbiAgJC5mbltOQU1FXS5ub0NvbmZsaWN0ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgJC5mbltOQU1FXSA9IEpRVUVSWV9OT19DT05GTElDVDtcclxuICAgIHJldHVybiBNb2RhbC5falF1ZXJ5SW50ZXJmYWNlO1xyXG4gIH07XHJcblxyXG4gIHJldHVybiBNb2RhbDtcclxufSkoalF1ZXJ5KTtcclxuLy8jIHNvdXJjZU1hcHBpbmdVUkw9bW9kYWwuanMubWFwXHJcbiIsIid1c2Ugc3RyaWN0JztcclxuXHJcbnZhciBfY3JlYXRlQ2xhc3MgPSAoZnVuY3Rpb24gKCkgeyBmdW5jdGlvbiBkZWZpbmVQcm9wZXJ0aWVzKHRhcmdldCwgcHJvcHMpIHsgZm9yICh2YXIgaSA9IDA7IGkgPCBwcm9wcy5sZW5ndGg7IGkrKykgeyB2YXIgZGVzY3JpcHRvciA9IHByb3BzW2ldOyBkZXNjcmlwdG9yLmVudW1lcmFibGUgPSBkZXNjcmlwdG9yLmVudW1lcmFibGUgfHwgZmFsc2U7IGRlc2NyaXB0b3IuY29uZmlndXJhYmxlID0gdHJ1ZTsgaWYgKCd2YWx1ZScgaW4gZGVzY3JpcHRvcikgZGVzY3JpcHRvci53cml0YWJsZSA9IHRydWU7IE9iamVjdC5kZWZpbmVQcm9wZXJ0eSh0YXJnZXQsIGRlc2NyaXB0b3Iua2V5LCBkZXNjcmlwdG9yKTsgfSB9IHJldHVybiBmdW5jdGlvbiAoQ29uc3RydWN0b3IsIHByb3RvUHJvcHMsIHN0YXRpY1Byb3BzKSB7IGlmIChwcm90b1Byb3BzKSBkZWZpbmVQcm9wZXJ0aWVzKENvbnN0cnVjdG9yLnByb3RvdHlwZSwgcHJvdG9Qcm9wcyk7IGlmIChzdGF0aWNQcm9wcykgZGVmaW5lUHJvcGVydGllcyhDb25zdHJ1Y3Rvciwgc3RhdGljUHJvcHMpOyByZXR1cm4gQ29uc3RydWN0b3I7IH07IH0pKCk7XHJcblxyXG52YXIgX2dldCA9IGZ1bmN0aW9uIGdldChfeCwgX3gyLCBfeDMpIHsgdmFyIF9hZ2FpbiA9IHRydWU7IF9mdW5jdGlvbjogd2hpbGUgKF9hZ2FpbikgeyB2YXIgb2JqZWN0ID0gX3gsIHByb3BlcnR5ID0gX3gyLCByZWNlaXZlciA9IF94MzsgX2FnYWluID0gZmFsc2U7IGlmIChvYmplY3QgPT09IG51bGwpIG9iamVjdCA9IEZ1bmN0aW9uLnByb3RvdHlwZTsgdmFyIGRlc2MgPSBPYmplY3QuZ2V0T3duUHJvcGVydHlEZXNjcmlwdG9yKG9iamVjdCwgcHJvcGVydHkpOyBpZiAoZGVzYyA9PT0gdW5kZWZpbmVkKSB7IHZhciBwYXJlbnQgPSBPYmplY3QuZ2V0UHJvdG90eXBlT2Yob2JqZWN0KTsgaWYgKHBhcmVudCA9PT0gbnVsbCkgeyByZXR1cm4gdW5kZWZpbmVkOyB9IGVsc2UgeyBfeCA9IHBhcmVudDsgX3gyID0gcHJvcGVydHk7IF94MyA9IHJlY2VpdmVyOyBfYWdhaW4gPSB0cnVlOyBkZXNjID0gcGFyZW50ID0gdW5kZWZpbmVkOyBjb250aW51ZSBfZnVuY3Rpb247IH0gfSBlbHNlIGlmICgndmFsdWUnIGluIGRlc2MpIHsgcmV0dXJuIGRlc2MudmFsdWU7IH0gZWxzZSB7IHZhciBnZXR0ZXIgPSBkZXNjLmdldDsgaWYgKGdldHRlciA9PT0gdW5kZWZpbmVkKSB7IHJldHVybiB1bmRlZmluZWQ7IH0gcmV0dXJuIGdldHRlci5jYWxsKHJlY2VpdmVyKTsgfSB9IH07XHJcblxyXG5mdW5jdGlvbiBfY2xhc3NDYWxsQ2hlY2soaW5zdGFuY2UsIENvbnN0cnVjdG9yKSB7IGlmICghKGluc3RhbmNlIGluc3RhbmNlb2YgQ29uc3RydWN0b3IpKSB7IHRocm93IG5ldyBUeXBlRXJyb3IoJ0Nhbm5vdCBjYWxsIGEgY2xhc3MgYXMgYSBmdW5jdGlvbicpOyB9IH1cclxuXHJcbmZ1bmN0aW9uIF9pbmhlcml0cyhzdWJDbGFzcywgc3VwZXJDbGFzcykgeyBpZiAodHlwZW9mIHN1cGVyQ2xhc3MgIT09ICdmdW5jdGlvbicgJiYgc3VwZXJDbGFzcyAhPT0gbnVsbCkgeyB0aHJvdyBuZXcgVHlwZUVycm9yKCdTdXBlciBleHByZXNzaW9uIG11c3QgZWl0aGVyIGJlIG51bGwgb3IgYSBmdW5jdGlvbiwgbm90ICcgKyB0eXBlb2Ygc3VwZXJDbGFzcyk7IH0gc3ViQ2xhc3MucHJvdG90eXBlID0gT2JqZWN0LmNyZWF0ZShzdXBlckNsYXNzICYmIHN1cGVyQ2xhc3MucHJvdG90eXBlLCB7IGNvbnN0cnVjdG9yOiB7IHZhbHVlOiBzdWJDbGFzcywgZW51bWVyYWJsZTogZmFsc2UsIHdyaXRhYmxlOiB0cnVlLCBjb25maWd1cmFibGU6IHRydWUgfSB9KTsgaWYgKHN1cGVyQ2xhc3MpIE9iamVjdC5zZXRQcm90b3R5cGVPZiA/IE9iamVjdC5zZXRQcm90b3R5cGVPZihzdWJDbGFzcywgc3VwZXJDbGFzcykgOiBzdWJDbGFzcy5fX3Byb3RvX18gPSBzdXBlckNsYXNzOyB9XHJcblxyXG4vKipcclxuICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICogQm9vdHN0cmFwICh2NC4wLjAtYWxwaGEuMik6IHBvcG92ZXIuanNcclxuICogTGljZW5zZWQgdW5kZXIgTUlUIChodHRwczovL2dpdGh1Yi5jb20vdHdicy9ib290c3RyYXAvYmxvYi9tYXN0ZXIvTElDRU5TRSlcclxuICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICovXHJcblxyXG52YXIgUG9wb3ZlciA9IChmdW5jdGlvbiAoJCkge1xyXG5cclxuICAvKipcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKiBDb25zdGFudHNcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKi9cclxuXHJcbiAgdmFyIE5BTUUgPSAncG9wb3Zlcic7XHJcbiAgdmFyIFZFUlNJT04gPSAnNC4wLjAtYWxwaGEnO1xyXG4gIHZhciBEQVRBX0tFWSA9ICdicy5wb3BvdmVyJztcclxuICB2YXIgRVZFTlRfS0VZID0gJy4nICsgREFUQV9LRVk7XHJcbiAgdmFyIEpRVUVSWV9OT19DT05GTElDVCA9ICQuZm5bTkFNRV07XHJcblxyXG4gIHZhciBEZWZhdWx0ID0gJC5leHRlbmQoe30sIFRvb2x0aXAuRGVmYXVsdCwge1xyXG4gICAgcGxhY2VtZW50OiAncmlnaHQnLFxyXG4gICAgdHJpZ2dlcjogJ2NsaWNrJyxcclxuICAgIGNvbnRlbnQ6ICcnLFxyXG4gICAgdGVtcGxhdGU6ICc8ZGl2IGNsYXNzPVwicG9wb3ZlclwiIHJvbGU9XCJ0b29sdGlwXCI+JyArICc8ZGl2IGNsYXNzPVwicG9wb3Zlci1hcnJvd1wiPjwvZGl2PicgKyAnPGgzIGNsYXNzPVwicG9wb3Zlci10aXRsZVwiPjwvaDM+JyArICc8ZGl2IGNsYXNzPVwicG9wb3Zlci1jb250ZW50XCI+PC9kaXY+PC9kaXY+J1xyXG4gIH0pO1xyXG5cclxuICB2YXIgRGVmYXVsdFR5cGUgPSAkLmV4dGVuZCh7fSwgVG9vbHRpcC5EZWZhdWx0VHlwZSwge1xyXG4gICAgY29udGVudDogJyhzdHJpbmd8ZWxlbWVudHxmdW5jdGlvbiknXHJcbiAgfSk7XHJcblxyXG4gIHZhciBDbGFzc05hbWUgPSB7XHJcbiAgICBGQURFOiAnZmFkZScsXHJcbiAgICBJTjogJ2luJ1xyXG4gIH07XHJcblxyXG4gIHZhciBTZWxlY3RvciA9IHtcclxuICAgIFRJVExFOiAnLnBvcG92ZXItdGl0bGUnLFxyXG4gICAgQ09OVEVOVDogJy5wb3BvdmVyLWNvbnRlbnQnLFxyXG4gICAgQVJST1c6ICcucG9wb3Zlci1hcnJvdydcclxuICB9O1xyXG5cclxuICB2YXIgRXZlbnQgPSB7XHJcbiAgICBISURFOiAnaGlkZScgKyBFVkVOVF9LRVksXHJcbiAgICBISURERU46ICdoaWRkZW4nICsgRVZFTlRfS0VZLFxyXG4gICAgU0hPVzogJ3Nob3cnICsgRVZFTlRfS0VZLFxyXG4gICAgU0hPV046ICdzaG93bicgKyBFVkVOVF9LRVksXHJcbiAgICBJTlNFUlRFRDogJ2luc2VydGVkJyArIEVWRU5UX0tFWSxcclxuICAgIENMSUNLOiAnY2xpY2snICsgRVZFTlRfS0VZLFxyXG4gICAgRk9DVVNJTjogJ2ZvY3VzaW4nICsgRVZFTlRfS0VZLFxyXG4gICAgRk9DVVNPVVQ6ICdmb2N1c291dCcgKyBFVkVOVF9LRVksXHJcbiAgICBNT1VTRUVOVEVSOiAnbW91c2VlbnRlcicgKyBFVkVOVF9LRVksXHJcbiAgICBNT1VTRUxFQVZFOiAnbW91c2VsZWF2ZScgKyBFVkVOVF9LRVlcclxuICB9O1xyXG5cclxuICAvKipcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKiBDbGFzcyBEZWZpbml0aW9uXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICovXHJcblxyXG4gIHZhciBQb3BvdmVyID0gKGZ1bmN0aW9uIChfVG9vbHRpcCkge1xyXG4gICAgX2luaGVyaXRzKFBvcG92ZXIsIF9Ub29sdGlwKTtcclxuXHJcbiAgICBmdW5jdGlvbiBQb3BvdmVyKCkge1xyXG4gICAgICBfY2xhc3NDYWxsQ2hlY2sodGhpcywgUG9wb3Zlcik7XHJcblxyXG4gICAgICBfZ2V0KE9iamVjdC5nZXRQcm90b3R5cGVPZihQb3BvdmVyLnByb3RvdHlwZSksICdjb25zdHJ1Y3RvcicsIHRoaXMpLmFwcGx5KHRoaXMsIGFyZ3VtZW50cyk7XHJcbiAgICB9XHJcblxyXG4gICAgLyoqXHJcbiAgICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgICAqIGpRdWVyeVxyXG4gICAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICAgKi9cclxuXHJcbiAgICBfY3JlYXRlQ2xhc3MoUG9wb3ZlciwgW3tcclxuICAgICAga2V5OiAnaXNXaXRoQ29udGVudCcsXHJcblxyXG4gICAgICAvLyBvdmVycmlkZXNcclxuXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBpc1dpdGhDb250ZW50KCkge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldFRpdGxlKCkgfHwgdGhpcy5fZ2V0Q29udGVudCgpO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ2dldFRpcEVsZW1lbnQnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gZ2V0VGlwRWxlbWVudCgpIHtcclxuICAgICAgICByZXR1cm4gdGhpcy50aXAgPSB0aGlzLnRpcCB8fCAkKHRoaXMuY29uZmlnLnRlbXBsYXRlKVswXTtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdzZXRDb250ZW50JyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIHNldENvbnRlbnQoKSB7XHJcbiAgICAgICAgdmFyICR0aXAgPSAkKHRoaXMuZ2V0VGlwRWxlbWVudCgpKTtcclxuXHJcbiAgICAgICAgLy8gd2UgdXNlIGFwcGVuZCBmb3IgaHRtbCBvYmplY3RzIHRvIG1haW50YWluIGpzIGV2ZW50c1xyXG4gICAgICAgIHRoaXMuc2V0RWxlbWVudENvbnRlbnQoJHRpcC5maW5kKFNlbGVjdG9yLlRJVExFKSwgdGhpcy5nZXRUaXRsZSgpKTtcclxuICAgICAgICB0aGlzLnNldEVsZW1lbnRDb250ZW50KCR0aXAuZmluZChTZWxlY3Rvci5DT05URU5UKSwgdGhpcy5fZ2V0Q29udGVudCgpKTtcclxuXHJcbiAgICAgICAgJHRpcC5yZW1vdmVDbGFzcyhDbGFzc05hbWUuRkFERSkucmVtb3ZlQ2xhc3MoQ2xhc3NOYW1lLklOKTtcclxuXHJcbiAgICAgICAgdGhpcy5jbGVhbnVwVGV0aGVyKCk7XHJcbiAgICAgIH1cclxuXHJcbiAgICAgIC8vIHByaXZhdGVcclxuXHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19nZXRDb250ZW50JyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9nZXRDb250ZW50KCkge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmVsZW1lbnQuZ2V0QXR0cmlidXRlKCdkYXRhLWNvbnRlbnQnKSB8fCAodHlwZW9mIHRoaXMuY29uZmlnLmNvbnRlbnQgPT09ICdmdW5jdGlvbicgPyB0aGlzLmNvbmZpZy5jb250ZW50LmNhbGwodGhpcy5lbGVtZW50KSA6IHRoaXMuY29uZmlnLmNvbnRlbnQpO1xyXG4gICAgICB9XHJcblxyXG4gICAgICAvLyBzdGF0aWNcclxuXHJcbiAgICB9XSwgW3tcclxuICAgICAga2V5OiAnX2pRdWVyeUludGVyZmFjZScsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBfalF1ZXJ5SW50ZXJmYWNlKGNvbmZpZykge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmVhY2goZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgdmFyIGRhdGEgPSAkKHRoaXMpLmRhdGEoREFUQV9LRVkpO1xyXG4gICAgICAgICAgdmFyIF9jb25maWcgPSB0eXBlb2YgY29uZmlnID09PSAnb2JqZWN0JyA/IGNvbmZpZyA6IG51bGw7XHJcblxyXG4gICAgICAgICAgaWYgKCFkYXRhICYmIC9kZXN0cm95fGhpZGUvLnRlc3QoY29uZmlnKSkge1xyXG4gICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgaWYgKCFkYXRhKSB7XHJcbiAgICAgICAgICAgIGRhdGEgPSBuZXcgUG9wb3Zlcih0aGlzLCBfY29uZmlnKTtcclxuICAgICAgICAgICAgJCh0aGlzKS5kYXRhKERBVEFfS0VZLCBkYXRhKTtcclxuICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICBpZiAodHlwZW9mIGNvbmZpZyA9PT0gJ3N0cmluZycpIHtcclxuICAgICAgICAgICAgaWYgKGRhdGFbY29uZmlnXSA9PT0gdW5kZWZpbmVkKSB7XHJcbiAgICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKCdObyBtZXRob2QgbmFtZWQgXCInICsgY29uZmlnICsgJ1wiJyk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgZGF0YVtjb25maWddKCk7XHJcbiAgICAgICAgICB9XHJcbiAgICAgICAgfSk7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnVkVSU0lPTicsXHJcblxyXG4gICAgICAvLyBnZXR0ZXJzXHJcblxyXG4gICAgICBnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcclxuICAgICAgICByZXR1cm4gVkVSU0lPTjtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdEZWZhdWx0JyxcclxuICAgICAgZ2V0OiBmdW5jdGlvbiBnZXQoKSB7XHJcbiAgICAgICAgcmV0dXJuIERlZmF1bHQ7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnTkFNRScsXHJcbiAgICAgIGdldDogZnVuY3Rpb24gZ2V0KCkge1xyXG4gICAgICAgIHJldHVybiBOQU1FO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ0RBVEFfS0VZJyxcclxuICAgICAgZ2V0OiBmdW5jdGlvbiBnZXQoKSB7XHJcbiAgICAgICAgcmV0dXJuIERBVEFfS0VZO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ0V2ZW50JyxcclxuICAgICAgZ2V0OiBmdW5jdGlvbiBnZXQoKSB7XHJcbiAgICAgICAgcmV0dXJuIEV2ZW50O1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ0VWRU5UX0tFWScsXHJcbiAgICAgIGdldDogZnVuY3Rpb24gZ2V0KCkge1xyXG4gICAgICAgIHJldHVybiBFVkVOVF9LRVk7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnRGVmYXVsdFR5cGUnLFxyXG4gICAgICBnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcclxuICAgICAgICByZXR1cm4gRGVmYXVsdFR5cGU7XHJcbiAgICAgIH1cclxuICAgIH1dKTtcclxuXHJcbiAgICByZXR1cm4gUG9wb3ZlcjtcclxuICB9KShUb29sdGlwKTtcclxuXHJcbiAgJC5mbltOQU1FXSA9IFBvcG92ZXIuX2pRdWVyeUludGVyZmFjZTtcclxuICAkLmZuW05BTUVdLkNvbnN0cnVjdG9yID0gUG9wb3ZlcjtcclxuICAkLmZuW05BTUVdLm5vQ29uZmxpY3QgPSBmdW5jdGlvbiAoKSB7XHJcbiAgICAkLmZuW05BTUVdID0gSlFVRVJZX05PX0NPTkZMSUNUO1xyXG4gICAgcmV0dXJuIFBvcG92ZXIuX2pRdWVyeUludGVyZmFjZTtcclxuICB9O1xyXG5cclxuICByZXR1cm4gUG9wb3ZlcjtcclxufSkoalF1ZXJ5KTtcclxuLy8jIHNvdXJjZU1hcHBpbmdVUkw9cG9wb3Zlci5qcy5tYXBcclxuIiwiJ3VzZSBzdHJpY3QnO1xyXG5cclxudmFyIF9jcmVhdGVDbGFzcyA9IChmdW5jdGlvbiAoKSB7IGZ1bmN0aW9uIGRlZmluZVByb3BlcnRpZXModGFyZ2V0LCBwcm9wcykgeyBmb3IgKHZhciBpID0gMDsgaSA8IHByb3BzLmxlbmd0aDsgaSsrKSB7IHZhciBkZXNjcmlwdG9yID0gcHJvcHNbaV07IGRlc2NyaXB0b3IuZW51bWVyYWJsZSA9IGRlc2NyaXB0b3IuZW51bWVyYWJsZSB8fCBmYWxzZTsgZGVzY3JpcHRvci5jb25maWd1cmFibGUgPSB0cnVlOyBpZiAoJ3ZhbHVlJyBpbiBkZXNjcmlwdG9yKSBkZXNjcmlwdG9yLndyaXRhYmxlID0gdHJ1ZTsgT2JqZWN0LmRlZmluZVByb3BlcnR5KHRhcmdldCwgZGVzY3JpcHRvci5rZXksIGRlc2NyaXB0b3IpOyB9IH0gcmV0dXJuIGZ1bmN0aW9uIChDb25zdHJ1Y3RvciwgcHJvdG9Qcm9wcywgc3RhdGljUHJvcHMpIHsgaWYgKHByb3RvUHJvcHMpIGRlZmluZVByb3BlcnRpZXMoQ29uc3RydWN0b3IucHJvdG90eXBlLCBwcm90b1Byb3BzKTsgaWYgKHN0YXRpY1Byb3BzKSBkZWZpbmVQcm9wZXJ0aWVzKENvbnN0cnVjdG9yLCBzdGF0aWNQcm9wcyk7IHJldHVybiBDb25zdHJ1Y3RvcjsgfTsgfSkoKTtcclxuXHJcbmZ1bmN0aW9uIF9jbGFzc0NhbGxDaGVjayhpbnN0YW5jZSwgQ29uc3RydWN0b3IpIHsgaWYgKCEoaW5zdGFuY2UgaW5zdGFuY2VvZiBDb25zdHJ1Y3RvcikpIHsgdGhyb3cgbmV3IFR5cGVFcnJvcignQ2Fubm90IGNhbGwgYSBjbGFzcyBhcyBhIGZ1bmN0aW9uJyk7IH0gfVxyXG5cclxuLyoqXHJcbiAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAqIEJvb3RzdHJhcCAodjQuMC4wLWFscGhhLjIpOiBzY3JvbGxzcHkuanNcclxuICogTGljZW5zZWQgdW5kZXIgTUlUIChodHRwczovL2dpdGh1Yi5jb20vdHdicy9ib290c3RyYXAvYmxvYi9tYXN0ZXIvTElDRU5TRSlcclxuICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICovXHJcblxyXG52YXIgU2Nyb2xsU3B5ID0gKGZ1bmN0aW9uICgkKSB7XHJcblxyXG4gIC8qKlxyXG4gICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAqIENvbnN0YW50c1xyXG4gICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAqL1xyXG5cclxuICB2YXIgTkFNRSA9ICdzY3JvbGxzcHknO1xyXG4gIHZhciBWRVJTSU9OID0gJzQuMC4wLWFscGhhJztcclxuICB2YXIgREFUQV9LRVkgPSAnYnMuc2Nyb2xsc3B5JztcclxuICB2YXIgRVZFTlRfS0VZID0gJy4nICsgREFUQV9LRVk7XHJcbiAgdmFyIERBVEFfQVBJX0tFWSA9ICcuZGF0YS1hcGknO1xyXG4gIHZhciBKUVVFUllfTk9fQ09ORkxJQ1QgPSAkLmZuW05BTUVdO1xyXG5cclxuICB2YXIgRGVmYXVsdCA9IHtcclxuICAgIG9mZnNldDogMTAsXHJcbiAgICBtZXRob2Q6ICdhdXRvJyxcclxuICAgIHRhcmdldDogJydcclxuICB9O1xyXG5cclxuICB2YXIgRGVmYXVsdFR5cGUgPSB7XHJcbiAgICBvZmZzZXQ6ICdudW1iZXInLFxyXG4gICAgbWV0aG9kOiAnc3RyaW5nJyxcclxuICAgIHRhcmdldDogJyhzdHJpbmd8ZWxlbWVudCknXHJcbiAgfTtcclxuXHJcbiAgdmFyIEV2ZW50ID0ge1xyXG4gICAgQUNUSVZBVEU6ICdhY3RpdmF0ZScgKyBFVkVOVF9LRVksXHJcbiAgICBTQ1JPTEw6ICdzY3JvbGwnICsgRVZFTlRfS0VZLFxyXG4gICAgTE9BRF9EQVRBX0FQSTogJ2xvYWQnICsgRVZFTlRfS0VZICsgREFUQV9BUElfS0VZXHJcbiAgfTtcclxuXHJcbiAgdmFyIENsYXNzTmFtZSA9IHtcclxuICAgIERST1BET1dOX0lURU06ICdkcm9wZG93bi1pdGVtJyxcclxuICAgIERST1BET1dOX01FTlU6ICdkcm9wZG93bi1tZW51JyxcclxuICAgIE5BVl9MSU5LOiAnbmF2LWxpbmsnLFxyXG4gICAgTkFWOiAnbmF2JyxcclxuICAgIEFDVElWRTogJ2FjdGl2ZSdcclxuICB9O1xyXG5cclxuICB2YXIgU2VsZWN0b3IgPSB7XHJcbiAgICBEQVRBX1NQWTogJ1tkYXRhLXNweT1cInNjcm9sbFwiXScsXHJcbiAgICBBQ1RJVkU6ICcuYWN0aXZlJyxcclxuICAgIExJU1RfSVRFTTogJy5saXN0LWl0ZW0nLFxyXG4gICAgTEk6ICdsaScsXHJcbiAgICBMSV9EUk9QRE9XTjogJ2xpLmRyb3Bkb3duJyxcclxuICAgIE5BVl9MSU5LUzogJy5uYXYtbGluaycsXHJcbiAgICBEUk9QRE9XTjogJy5kcm9wZG93bicsXHJcbiAgICBEUk9QRE9XTl9JVEVNUzogJy5kcm9wZG93bi1pdGVtJyxcclxuICAgIERST1BET1dOX1RPR0dMRTogJy5kcm9wZG93bi10b2dnbGUnXHJcbiAgfTtcclxuXHJcbiAgdmFyIE9mZnNldE1ldGhvZCA9IHtcclxuICAgIE9GRlNFVDogJ29mZnNldCcsXHJcbiAgICBQT1NJVElPTjogJ3Bvc2l0aW9uJ1xyXG4gIH07XHJcblxyXG4gIC8qKlxyXG4gICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAqIENsYXNzIERlZmluaXRpb25cclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKi9cclxuXHJcbiAgdmFyIFNjcm9sbFNweSA9IChmdW5jdGlvbiAoKSB7XHJcbiAgICBmdW5jdGlvbiBTY3JvbGxTcHkoZWxlbWVudCwgY29uZmlnKSB7XHJcbiAgICAgIF9jbGFzc0NhbGxDaGVjayh0aGlzLCBTY3JvbGxTcHkpO1xyXG5cclxuICAgICAgdGhpcy5fZWxlbWVudCA9IGVsZW1lbnQ7XHJcbiAgICAgIHRoaXMuX3Njcm9sbEVsZW1lbnQgPSBlbGVtZW50LnRhZ05hbWUgPT09ICdCT0RZJyA/IHdpbmRvdyA6IGVsZW1lbnQ7XHJcbiAgICAgIHRoaXMuX2NvbmZpZyA9IHRoaXMuX2dldENvbmZpZyhjb25maWcpO1xyXG4gICAgICB0aGlzLl9zZWxlY3RvciA9IHRoaXMuX2NvbmZpZy50YXJnZXQgKyAnICcgKyBTZWxlY3Rvci5OQVZfTElOS1MgKyAnLCcgKyAodGhpcy5fY29uZmlnLnRhcmdldCArICcgJyArIFNlbGVjdG9yLkRST1BET1dOX0lURU1TKTtcclxuICAgICAgdGhpcy5fb2Zmc2V0cyA9IFtdO1xyXG4gICAgICB0aGlzLl90YXJnZXRzID0gW107XHJcbiAgICAgIHRoaXMuX2FjdGl2ZVRhcmdldCA9IG51bGw7XHJcbiAgICAgIHRoaXMuX3Njcm9sbEhlaWdodCA9IDA7XHJcblxyXG4gICAgICAkKHRoaXMuX3Njcm9sbEVsZW1lbnQpLm9uKEV2ZW50LlNDUk9MTCwgJC5wcm94eSh0aGlzLl9wcm9jZXNzLCB0aGlzKSk7XHJcblxyXG4gICAgICB0aGlzLnJlZnJlc2goKTtcclxuICAgICAgdGhpcy5fcHJvY2VzcygpO1xyXG4gICAgfVxyXG5cclxuICAgIC8qKlxyXG4gICAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICAgKiBEYXRhIEFwaSBpbXBsZW1lbnRhdGlvblxyXG4gICAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICAgKi9cclxuXHJcbiAgICAvLyBnZXR0ZXJzXHJcblxyXG4gICAgX2NyZWF0ZUNsYXNzKFNjcm9sbFNweSwgW3tcclxuICAgICAga2V5OiAncmVmcmVzaCcsXHJcblxyXG4gICAgICAvLyBwdWJsaWNcclxuXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiByZWZyZXNoKCkge1xyXG4gICAgICAgIHZhciBfdGhpcyA9IHRoaXM7XHJcblxyXG4gICAgICAgIHZhciBhdXRvTWV0aG9kID0gdGhpcy5fc2Nyb2xsRWxlbWVudCAhPT0gdGhpcy5fc2Nyb2xsRWxlbWVudC53aW5kb3cgPyBPZmZzZXRNZXRob2QuUE9TSVRJT04gOiBPZmZzZXRNZXRob2QuT0ZGU0VUO1xyXG5cclxuICAgICAgICB2YXIgb2Zmc2V0TWV0aG9kID0gdGhpcy5fY29uZmlnLm1ldGhvZCA9PT0gJ2F1dG8nID8gYXV0b01ldGhvZCA6IHRoaXMuX2NvbmZpZy5tZXRob2Q7XHJcblxyXG4gICAgICAgIHZhciBvZmZzZXRCYXNlID0gb2Zmc2V0TWV0aG9kID09PSBPZmZzZXRNZXRob2QuUE9TSVRJT04gPyB0aGlzLl9nZXRTY3JvbGxUb3AoKSA6IDA7XHJcblxyXG4gICAgICAgIHRoaXMuX29mZnNldHMgPSBbXTtcclxuICAgICAgICB0aGlzLl90YXJnZXRzID0gW107XHJcblxyXG4gICAgICAgIHRoaXMuX3Njcm9sbEhlaWdodCA9IHRoaXMuX2dldFNjcm9sbEhlaWdodCgpO1xyXG5cclxuICAgICAgICB2YXIgdGFyZ2V0cyA9ICQubWFrZUFycmF5KCQodGhpcy5fc2VsZWN0b3IpKTtcclxuXHJcbiAgICAgICAgdGFyZ2V0cy5tYXAoZnVuY3Rpb24gKGVsZW1lbnQpIHtcclxuICAgICAgICAgIHZhciB0YXJnZXQgPSB1bmRlZmluZWQ7XHJcbiAgICAgICAgICB2YXIgdGFyZ2V0U2VsZWN0b3IgPSBVdGlsLmdldFNlbGVjdG9yRnJvbUVsZW1lbnQoZWxlbWVudCk7XHJcblxyXG4gICAgICAgICAgaWYgKHRhcmdldFNlbGVjdG9yKSB7XHJcbiAgICAgICAgICAgIHRhcmdldCA9ICQodGFyZ2V0U2VsZWN0b3IpWzBdO1xyXG4gICAgICAgICAgfVxyXG5cclxuICAgICAgICAgIGlmICh0YXJnZXQgJiYgKHRhcmdldC5vZmZzZXRXaWR0aCB8fCB0YXJnZXQub2Zmc2V0SGVpZ2h0KSkge1xyXG4gICAgICAgICAgICAvLyB0b2RvIChmYXQpOiByZW1vdmUgc2tldGNoIHJlbGlhbmNlIG9uIGpRdWVyeSBwb3NpdGlvbi9vZmZzZXRcclxuICAgICAgICAgICAgcmV0dXJuIFskKHRhcmdldClbb2Zmc2V0TWV0aG9kXSgpLnRvcCArIG9mZnNldEJhc2UsIHRhcmdldFNlbGVjdG9yXTtcclxuICAgICAgICAgIH1cclxuICAgICAgICB9KS5maWx0ZXIoZnVuY3Rpb24gKGl0ZW0pIHtcclxuICAgICAgICAgIHJldHVybiBpdGVtO1xyXG4gICAgICAgIH0pLnNvcnQoZnVuY3Rpb24gKGEsIGIpIHtcclxuICAgICAgICAgIHJldHVybiBhWzBdIC0gYlswXTtcclxuICAgICAgICB9KS5mb3JFYWNoKGZ1bmN0aW9uIChpdGVtKSB7XHJcbiAgICAgICAgICBfdGhpcy5fb2Zmc2V0cy5wdXNoKGl0ZW1bMF0pO1xyXG4gICAgICAgICAgX3RoaXMuX3RhcmdldHMucHVzaChpdGVtWzFdKTtcclxuICAgICAgICB9KTtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdkaXNwb3NlJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIGRpc3Bvc2UoKSB7XHJcbiAgICAgICAgJC5yZW1vdmVEYXRhKHRoaXMuX2VsZW1lbnQsIERBVEFfS0VZKTtcclxuICAgICAgICAkKHRoaXMuX3Njcm9sbEVsZW1lbnQpLm9mZihFVkVOVF9LRVkpO1xyXG5cclxuICAgICAgICB0aGlzLl9lbGVtZW50ID0gbnVsbDtcclxuICAgICAgICB0aGlzLl9zY3JvbGxFbGVtZW50ID0gbnVsbDtcclxuICAgICAgICB0aGlzLl9jb25maWcgPSBudWxsO1xyXG4gICAgICAgIHRoaXMuX3NlbGVjdG9yID0gbnVsbDtcclxuICAgICAgICB0aGlzLl9vZmZzZXRzID0gbnVsbDtcclxuICAgICAgICB0aGlzLl90YXJnZXRzID0gbnVsbDtcclxuICAgICAgICB0aGlzLl9hY3RpdmVUYXJnZXQgPSBudWxsO1xyXG4gICAgICAgIHRoaXMuX3Njcm9sbEhlaWdodCA9IG51bGw7XHJcbiAgICAgIH1cclxuXHJcbiAgICAgIC8vIHByaXZhdGVcclxuXHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19nZXRDb25maWcnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2dldENvbmZpZyhjb25maWcpIHtcclxuICAgICAgICBjb25maWcgPSAkLmV4dGVuZCh7fSwgRGVmYXVsdCwgY29uZmlnKTtcclxuXHJcbiAgICAgICAgaWYgKHR5cGVvZiBjb25maWcudGFyZ2V0ICE9PSAnc3RyaW5nJykge1xyXG4gICAgICAgICAgdmFyIGlkID0gJChjb25maWcudGFyZ2V0KS5hdHRyKCdpZCcpO1xyXG4gICAgICAgICAgaWYgKCFpZCkge1xyXG4gICAgICAgICAgICBpZCA9IFV0aWwuZ2V0VUlEKE5BTUUpO1xyXG4gICAgICAgICAgICAkKGNvbmZpZy50YXJnZXQpLmF0dHIoJ2lkJywgaWQpO1xyXG4gICAgICAgICAgfVxyXG4gICAgICAgICAgY29uZmlnLnRhcmdldCA9ICcjJyArIGlkO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgVXRpbC50eXBlQ2hlY2tDb25maWcoTkFNRSwgY29uZmlnLCBEZWZhdWx0VHlwZSk7XHJcblxyXG4gICAgICAgIHJldHVybiBjb25maWc7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX2dldFNjcm9sbFRvcCcsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBfZ2V0U2Nyb2xsVG9wKCkge1xyXG4gICAgICAgIHJldHVybiB0aGlzLl9zY3JvbGxFbGVtZW50ID09PSB3aW5kb3cgPyB0aGlzLl9zY3JvbGxFbGVtZW50LnNjcm9sbFkgOiB0aGlzLl9zY3JvbGxFbGVtZW50LnNjcm9sbFRvcDtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfZ2V0U2Nyb2xsSGVpZ2h0JyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9nZXRTY3JvbGxIZWlnaHQoKSB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuX3Njcm9sbEVsZW1lbnQuc2Nyb2xsSGVpZ2h0IHx8IE1hdGgubWF4KGRvY3VtZW50LmJvZHkuc2Nyb2xsSGVpZ2h0LCBkb2N1bWVudC5kb2N1bWVudEVsZW1lbnQuc2Nyb2xsSGVpZ2h0KTtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfcHJvY2VzcycsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBfcHJvY2VzcygpIHtcclxuICAgICAgICB2YXIgc2Nyb2xsVG9wID0gdGhpcy5fZ2V0U2Nyb2xsVG9wKCkgKyB0aGlzLl9jb25maWcub2Zmc2V0O1xyXG4gICAgICAgIHZhciBzY3JvbGxIZWlnaHQgPSB0aGlzLl9nZXRTY3JvbGxIZWlnaHQoKTtcclxuICAgICAgICB2YXIgbWF4U2Nyb2xsID0gdGhpcy5fY29uZmlnLm9mZnNldCArIHNjcm9sbEhlaWdodCAtIHRoaXMuX3Njcm9sbEVsZW1lbnQub2Zmc2V0SGVpZ2h0O1xyXG5cclxuICAgICAgICBpZiAodGhpcy5fc2Nyb2xsSGVpZ2h0ICE9PSBzY3JvbGxIZWlnaHQpIHtcclxuICAgICAgICAgIHRoaXMucmVmcmVzaCgpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgaWYgKHNjcm9sbFRvcCA+PSBtYXhTY3JvbGwpIHtcclxuICAgICAgICAgIHZhciB0YXJnZXQgPSB0aGlzLl90YXJnZXRzW3RoaXMuX3RhcmdldHMubGVuZ3RoIC0gMV07XHJcblxyXG4gICAgICAgICAgaWYgKHRoaXMuX2FjdGl2ZVRhcmdldCAhPT0gdGFyZ2V0KSB7XHJcbiAgICAgICAgICAgIHRoaXMuX2FjdGl2YXRlKHRhcmdldCk7XHJcbiAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBpZiAodGhpcy5fYWN0aXZlVGFyZ2V0ICYmIHNjcm9sbFRvcCA8IHRoaXMuX29mZnNldHNbMF0pIHtcclxuICAgICAgICAgIHRoaXMuX2FjdGl2ZVRhcmdldCA9IG51bGw7XHJcbiAgICAgICAgICB0aGlzLl9jbGVhcigpO1xyXG4gICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgZm9yICh2YXIgaSA9IHRoaXMuX29mZnNldHMubGVuZ3RoOyBpLS07KSB7XHJcbiAgICAgICAgICB2YXIgaXNBY3RpdmVUYXJnZXQgPSB0aGlzLl9hY3RpdmVUYXJnZXQgIT09IHRoaXMuX3RhcmdldHNbaV0gJiYgc2Nyb2xsVG9wID49IHRoaXMuX29mZnNldHNbaV0gJiYgKHRoaXMuX29mZnNldHNbaSArIDFdID09PSB1bmRlZmluZWQgfHwgc2Nyb2xsVG9wIDwgdGhpcy5fb2Zmc2V0c1tpICsgMV0pO1xyXG5cclxuICAgICAgICAgIGlmIChpc0FjdGl2ZVRhcmdldCkge1xyXG4gICAgICAgICAgICB0aGlzLl9hY3RpdmF0ZSh0aGlzLl90YXJnZXRzW2ldKTtcclxuICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX2FjdGl2YXRlJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9hY3RpdmF0ZSh0YXJnZXQpIHtcclxuICAgICAgICB0aGlzLl9hY3RpdmVUYXJnZXQgPSB0YXJnZXQ7XHJcblxyXG4gICAgICAgIHRoaXMuX2NsZWFyKCk7XHJcblxyXG4gICAgICAgIHZhciBxdWVyaWVzID0gdGhpcy5fc2VsZWN0b3Iuc3BsaXQoJywnKTtcclxuICAgICAgICBxdWVyaWVzID0gcXVlcmllcy5tYXAoZnVuY3Rpb24gKHNlbGVjdG9yKSB7XHJcbiAgICAgICAgICByZXR1cm4gc2VsZWN0b3IgKyAnW2RhdGEtdGFyZ2V0PVwiJyArIHRhcmdldCArICdcIl0sJyArIChzZWxlY3RvciArICdbaHJlZj1cIicgKyB0YXJnZXQgKyAnXCJdJyk7XHJcbiAgICAgICAgfSk7XHJcblxyXG4gICAgICAgIHZhciAkbGluayA9ICQocXVlcmllcy5qb2luKCcsJykpO1xyXG5cclxuICAgICAgICBpZiAoJGxpbmsuaGFzQ2xhc3MoQ2xhc3NOYW1lLkRST1BET1dOX0lURU0pKSB7XHJcbiAgICAgICAgICAkbGluay5jbG9zZXN0KFNlbGVjdG9yLkRST1BET1dOKS5maW5kKFNlbGVjdG9yLkRST1BET1dOX1RPR0dMRSkuYWRkQ2xhc3MoQ2xhc3NOYW1lLkFDVElWRSk7XHJcbiAgICAgICAgICAkbGluay5hZGRDbGFzcyhDbGFzc05hbWUuQUNUSVZFKTtcclxuICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgLy8gdG9kbyAoZmF0KSB0aGlzIGlzIGtpbmRhIHN1c+KAplxyXG4gICAgICAgICAgLy8gcmVjdXJzaXZlbHkgYWRkIGFjdGl2ZXMgdG8gdGVzdGVkIG5hdi1saW5rc1xyXG4gICAgICAgICAgJGxpbmsucGFyZW50cyhTZWxlY3Rvci5MSSkuZmluZChTZWxlY3Rvci5OQVZfTElOS1MpLmFkZENsYXNzKENsYXNzTmFtZS5BQ1RJVkUpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgJCh0aGlzLl9zY3JvbGxFbGVtZW50KS50cmlnZ2VyKEV2ZW50LkFDVElWQVRFLCB7XHJcbiAgICAgICAgICByZWxhdGVkVGFyZ2V0OiB0YXJnZXRcclxuICAgICAgICB9KTtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfY2xlYXInLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2NsZWFyKCkge1xyXG4gICAgICAgICQodGhpcy5fc2VsZWN0b3IpLmZpbHRlcihTZWxlY3Rvci5BQ1RJVkUpLnJlbW92ZUNsYXNzKENsYXNzTmFtZS5BQ1RJVkUpO1xyXG4gICAgICB9XHJcblxyXG4gICAgICAvLyBzdGF0aWNcclxuXHJcbiAgICB9XSwgW3tcclxuICAgICAga2V5OiAnX2pRdWVyeUludGVyZmFjZScsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBfalF1ZXJ5SW50ZXJmYWNlKGNvbmZpZykge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmVhY2goZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgdmFyIGRhdGEgPSAkKHRoaXMpLmRhdGEoREFUQV9LRVkpO1xyXG4gICAgICAgICAgdmFyIF9jb25maWcgPSB0eXBlb2YgY29uZmlnID09PSAnb2JqZWN0JyAmJiBjb25maWcgfHwgbnVsbDtcclxuXHJcbiAgICAgICAgICBpZiAoIWRhdGEpIHtcclxuICAgICAgICAgICAgZGF0YSA9IG5ldyBTY3JvbGxTcHkodGhpcywgX2NvbmZpZyk7XHJcbiAgICAgICAgICAgICQodGhpcykuZGF0YShEQVRBX0tFWSwgZGF0YSk7XHJcbiAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgaWYgKHR5cGVvZiBjb25maWcgPT09ICdzdHJpbmcnKSB7XHJcbiAgICAgICAgICAgIGlmIChkYXRhW2NvbmZpZ10gPT09IHVuZGVmaW5lZCkge1xyXG4gICAgICAgICAgICAgIHRocm93IG5ldyBFcnJvcignTm8gbWV0aG9kIG5hbWVkIFwiJyArIGNvbmZpZyArICdcIicpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGRhdGFbY29uZmlnXSgpO1xyXG4gICAgICAgICAgfVxyXG4gICAgICAgIH0pO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ1ZFUlNJT04nLFxyXG4gICAgICBnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcclxuICAgICAgICByZXR1cm4gVkVSU0lPTjtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdEZWZhdWx0JyxcclxuICAgICAgZ2V0OiBmdW5jdGlvbiBnZXQoKSB7XHJcbiAgICAgICAgcmV0dXJuIERlZmF1bHQ7XHJcbiAgICAgIH1cclxuICAgIH1dKTtcclxuXHJcbiAgICByZXR1cm4gU2Nyb2xsU3B5O1xyXG4gIH0pKCk7XHJcblxyXG4gICQod2luZG93KS5vbihFdmVudC5MT0FEX0RBVEFfQVBJLCBmdW5jdGlvbiAoKSB7XHJcbiAgICB2YXIgc2Nyb2xsU3B5cyA9ICQubWFrZUFycmF5KCQoU2VsZWN0b3IuREFUQV9TUFkpKTtcclxuXHJcbiAgICBmb3IgKHZhciBpID0gc2Nyb2xsU3B5cy5sZW5ndGg7IGktLTspIHtcclxuICAgICAgdmFyICRzcHkgPSAkKHNjcm9sbFNweXNbaV0pO1xyXG4gICAgICBTY3JvbGxTcHkuX2pRdWVyeUludGVyZmFjZS5jYWxsKCRzcHksICRzcHkuZGF0YSgpKTtcclxuICAgIH1cclxuICB9KTtcclxuXHJcbiAgLyoqXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICogalF1ZXJ5XHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICovXHJcblxyXG4gICQuZm5bTkFNRV0gPSBTY3JvbGxTcHkuX2pRdWVyeUludGVyZmFjZTtcclxuICAkLmZuW05BTUVdLkNvbnN0cnVjdG9yID0gU2Nyb2xsU3B5O1xyXG4gICQuZm5bTkFNRV0ubm9Db25mbGljdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICQuZm5bTkFNRV0gPSBKUVVFUllfTk9fQ09ORkxJQ1Q7XHJcbiAgICByZXR1cm4gU2Nyb2xsU3B5Ll9qUXVlcnlJbnRlcmZhY2U7XHJcbiAgfTtcclxuXHJcbiAgcmV0dXJuIFNjcm9sbFNweTtcclxufSkoalF1ZXJ5KTtcclxuLy8jIHNvdXJjZU1hcHBpbmdVUkw9c2Nyb2xsc3B5LmpzLm1hcFxyXG4iLCIndXNlIHN0cmljdCc7XHJcblxyXG52YXIgX2NyZWF0ZUNsYXNzID0gKGZ1bmN0aW9uICgpIHsgZnVuY3Rpb24gZGVmaW5lUHJvcGVydGllcyh0YXJnZXQsIHByb3BzKSB7IGZvciAodmFyIGkgPSAwOyBpIDwgcHJvcHMubGVuZ3RoOyBpKyspIHsgdmFyIGRlc2NyaXB0b3IgPSBwcm9wc1tpXTsgZGVzY3JpcHRvci5lbnVtZXJhYmxlID0gZGVzY3JpcHRvci5lbnVtZXJhYmxlIHx8IGZhbHNlOyBkZXNjcmlwdG9yLmNvbmZpZ3VyYWJsZSA9IHRydWU7IGlmICgndmFsdWUnIGluIGRlc2NyaXB0b3IpIGRlc2NyaXB0b3Iud3JpdGFibGUgPSB0cnVlOyBPYmplY3QuZGVmaW5lUHJvcGVydHkodGFyZ2V0LCBkZXNjcmlwdG9yLmtleSwgZGVzY3JpcHRvcik7IH0gfSByZXR1cm4gZnVuY3Rpb24gKENvbnN0cnVjdG9yLCBwcm90b1Byb3BzLCBzdGF0aWNQcm9wcykgeyBpZiAocHJvdG9Qcm9wcykgZGVmaW5lUHJvcGVydGllcyhDb25zdHJ1Y3Rvci5wcm90b3R5cGUsIHByb3RvUHJvcHMpOyBpZiAoc3RhdGljUHJvcHMpIGRlZmluZVByb3BlcnRpZXMoQ29uc3RydWN0b3IsIHN0YXRpY1Byb3BzKTsgcmV0dXJuIENvbnN0cnVjdG9yOyB9OyB9KSgpO1xyXG5cclxuZnVuY3Rpb24gX2NsYXNzQ2FsbENoZWNrKGluc3RhbmNlLCBDb25zdHJ1Y3RvcikgeyBpZiAoIShpbnN0YW5jZSBpbnN0YW5jZW9mIENvbnN0cnVjdG9yKSkgeyB0aHJvdyBuZXcgVHlwZUVycm9yKCdDYW5ub3QgY2FsbCBhIGNsYXNzIGFzIGEgZnVuY3Rpb24nKTsgfSB9XHJcblxyXG4vKipcclxuICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICogQm9vdHN0cmFwICh2NC4wLjAtYWxwaGEuMik6IHRhYi5qc1xyXG4gKiBMaWNlbnNlZCB1bmRlciBNSVQgKGh0dHBzOi8vZ2l0aHViLmNvbS90d2JzL2Jvb3RzdHJhcC9ibG9iL21hc3Rlci9MSUNFTlNFKVxyXG4gKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gKi9cclxuXHJcbnZhciBUYWIgPSAoZnVuY3Rpb24gKCQpIHtcclxuXHJcbiAgLyoqXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICogQ29uc3RhbnRzXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICovXHJcblxyXG4gIHZhciBOQU1FID0gJ3RhYic7XHJcbiAgdmFyIFZFUlNJT04gPSAnNC4wLjAtYWxwaGEnO1xyXG4gIHZhciBEQVRBX0tFWSA9ICdicy50YWInO1xyXG4gIHZhciBFVkVOVF9LRVkgPSAnLicgKyBEQVRBX0tFWTtcclxuICB2YXIgREFUQV9BUElfS0VZID0gJy5kYXRhLWFwaSc7XHJcbiAgdmFyIEpRVUVSWV9OT19DT05GTElDVCA9ICQuZm5bTkFNRV07XHJcbiAgdmFyIFRSQU5TSVRJT05fRFVSQVRJT04gPSAxNTA7XHJcblxyXG4gIHZhciBFdmVudCA9IHtcclxuICAgIEhJREU6ICdoaWRlJyArIEVWRU5UX0tFWSxcclxuICAgIEhJRERFTjogJ2hpZGRlbicgKyBFVkVOVF9LRVksXHJcbiAgICBTSE9XOiAnc2hvdycgKyBFVkVOVF9LRVksXHJcbiAgICBTSE9XTjogJ3Nob3duJyArIEVWRU5UX0tFWSxcclxuICAgIENMSUNLX0RBVEFfQVBJOiAnY2xpY2snICsgRVZFTlRfS0VZICsgREFUQV9BUElfS0VZXHJcbiAgfTtcclxuXHJcbiAgdmFyIENsYXNzTmFtZSA9IHtcclxuICAgIERST1BET1dOX01FTlU6ICdkcm9wZG93bi1tZW51JyxcclxuICAgIEFDVElWRTogJ2FjdGl2ZScsXHJcbiAgICBGQURFOiAnZmFkZScsXHJcbiAgICBJTjogJ2luJ1xyXG4gIH07XHJcblxyXG4gIHZhciBTZWxlY3RvciA9IHtcclxuICAgIEE6ICdhJyxcclxuICAgIExJOiAnbGknLFxyXG4gICAgRFJPUERPV046ICcuZHJvcGRvd24nLFxyXG4gICAgVUw6ICd1bDpub3QoLmRyb3Bkb3duLW1lbnUpJyxcclxuICAgIEZBREVfQ0hJTEQ6ICc+IC5uYXYtaXRlbSAuZmFkZSwgPiAuZmFkZScsXHJcbiAgICBBQ1RJVkU6ICcuYWN0aXZlJyxcclxuICAgIEFDVElWRV9DSElMRDogJz4gLm5hdi1pdGVtID4gLmFjdGl2ZSwgPiAuYWN0aXZlJyxcclxuICAgIERBVEFfVE9HR0xFOiAnW2RhdGEtdG9nZ2xlPVwidGFiXCJdLCBbZGF0YS10b2dnbGU9XCJwaWxsXCJdJyxcclxuICAgIERST1BET1dOX1RPR0dMRTogJy5kcm9wZG93bi10b2dnbGUnLFxyXG4gICAgRFJPUERPV05fQUNUSVZFX0NISUxEOiAnPiAuZHJvcGRvd24tbWVudSAuYWN0aXZlJ1xyXG4gIH07XHJcblxyXG4gIC8qKlxyXG4gICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAqIENsYXNzIERlZmluaXRpb25cclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKi9cclxuXHJcbiAgdmFyIFRhYiA9IChmdW5jdGlvbiAoKSB7XHJcbiAgICBmdW5jdGlvbiBUYWIoZWxlbWVudCkge1xyXG4gICAgICBfY2xhc3NDYWxsQ2hlY2sodGhpcywgVGFiKTtcclxuXHJcbiAgICAgIHRoaXMuX2VsZW1lbnQgPSBlbGVtZW50O1xyXG4gICAgfVxyXG5cclxuICAgIC8qKlxyXG4gICAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICAgKiBEYXRhIEFwaSBpbXBsZW1lbnRhdGlvblxyXG4gICAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICAgKi9cclxuXHJcbiAgICAvLyBnZXR0ZXJzXHJcblxyXG4gICAgX2NyZWF0ZUNsYXNzKFRhYiwgW3tcclxuICAgICAga2V5OiAnc2hvdycsXHJcblxyXG4gICAgICAvLyBwdWJsaWNcclxuXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBzaG93KCkge1xyXG4gICAgICAgIHZhciBfdGhpcyA9IHRoaXM7XHJcblxyXG4gICAgICAgIGlmICh0aGlzLl9lbGVtZW50LnBhcmVudE5vZGUgJiYgdGhpcy5fZWxlbWVudC5wYXJlbnROb2RlLm5vZGVUeXBlID09PSBOb2RlLkVMRU1FTlRfTk9ERSAmJiAkKHRoaXMuX2VsZW1lbnQpLmhhc0NsYXNzKENsYXNzTmFtZS5BQ1RJVkUpKSB7XHJcbiAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB2YXIgdGFyZ2V0ID0gdW5kZWZpbmVkO1xyXG4gICAgICAgIHZhciBwcmV2aW91cyA9IHVuZGVmaW5lZDtcclxuICAgICAgICB2YXIgdWxFbGVtZW50ID0gJCh0aGlzLl9lbGVtZW50KS5jbG9zZXN0KFNlbGVjdG9yLlVMKVswXTtcclxuICAgICAgICB2YXIgc2VsZWN0b3IgPSBVdGlsLmdldFNlbGVjdG9yRnJvbUVsZW1lbnQodGhpcy5fZWxlbWVudCk7XHJcblxyXG4gICAgICAgIGlmICh1bEVsZW1lbnQpIHtcclxuICAgICAgICAgIHByZXZpb3VzID0gJC5tYWtlQXJyYXkoJCh1bEVsZW1lbnQpLmZpbmQoU2VsZWN0b3IuQUNUSVZFKSk7XHJcbiAgICAgICAgICBwcmV2aW91cyA9IHByZXZpb3VzW3ByZXZpb3VzLmxlbmd0aCAtIDFdO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdmFyIGhpZGVFdmVudCA9ICQuRXZlbnQoRXZlbnQuSElERSwge1xyXG4gICAgICAgICAgcmVsYXRlZFRhcmdldDogdGhpcy5fZWxlbWVudFxyXG4gICAgICAgIH0pO1xyXG5cclxuICAgICAgICB2YXIgc2hvd0V2ZW50ID0gJC5FdmVudChFdmVudC5TSE9XLCB7XHJcbiAgICAgICAgICByZWxhdGVkVGFyZ2V0OiBwcmV2aW91c1xyXG4gICAgICAgIH0pO1xyXG5cclxuICAgICAgICBpZiAocHJldmlvdXMpIHtcclxuICAgICAgICAgICQocHJldmlvdXMpLnRyaWdnZXIoaGlkZUV2ZW50KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgICQodGhpcy5fZWxlbWVudCkudHJpZ2dlcihzaG93RXZlbnQpO1xyXG5cclxuICAgICAgICBpZiAoc2hvd0V2ZW50LmlzRGVmYXVsdFByZXZlbnRlZCgpIHx8IGhpZGVFdmVudC5pc0RlZmF1bHRQcmV2ZW50ZWQoKSkge1xyXG4gICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgaWYgKHNlbGVjdG9yKSB7XHJcbiAgICAgICAgICB0YXJnZXQgPSAkKHNlbGVjdG9yKVswXTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHRoaXMuX2FjdGl2YXRlKHRoaXMuX2VsZW1lbnQsIHVsRWxlbWVudCk7XHJcblxyXG4gICAgICAgIHZhciBjb21wbGV0ZSA9IGZ1bmN0aW9uIGNvbXBsZXRlKCkge1xyXG4gICAgICAgICAgdmFyIGhpZGRlbkV2ZW50ID0gJC5FdmVudChFdmVudC5ISURERU4sIHtcclxuICAgICAgICAgICAgcmVsYXRlZFRhcmdldDogX3RoaXMuX2VsZW1lbnRcclxuICAgICAgICAgIH0pO1xyXG5cclxuICAgICAgICAgIHZhciBzaG93bkV2ZW50ID0gJC5FdmVudChFdmVudC5TSE9XTiwge1xyXG4gICAgICAgICAgICByZWxhdGVkVGFyZ2V0OiBwcmV2aW91c1xyXG4gICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgJChwcmV2aW91cykudHJpZ2dlcihoaWRkZW5FdmVudCk7XHJcbiAgICAgICAgICAkKF90aGlzLl9lbGVtZW50KS50cmlnZ2VyKHNob3duRXZlbnQpO1xyXG4gICAgICAgIH07XHJcblxyXG4gICAgICAgIGlmICh0YXJnZXQpIHtcclxuICAgICAgICAgIHRoaXMuX2FjdGl2YXRlKHRhcmdldCwgdGFyZ2V0LnBhcmVudE5vZGUsIGNvbXBsZXRlKTtcclxuICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgY29tcGxldGUoKTtcclxuICAgICAgICB9XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnZGlzcG9zZScsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBkaXNwb3NlKCkge1xyXG4gICAgICAgICQucmVtb3ZlQ2xhc3ModGhpcy5fZWxlbWVudCwgREFUQV9LRVkpO1xyXG4gICAgICAgIHRoaXMuX2VsZW1lbnQgPSBudWxsO1xyXG4gICAgICB9XHJcblxyXG4gICAgICAvLyBwcml2YXRlXHJcblxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfYWN0aXZhdGUnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2FjdGl2YXRlKGVsZW1lbnQsIGNvbnRhaW5lciwgY2FsbGJhY2spIHtcclxuICAgICAgICB2YXIgYWN0aXZlID0gJChjb250YWluZXIpLmZpbmQoU2VsZWN0b3IuQUNUSVZFX0NISUxEKVswXTtcclxuICAgICAgICB2YXIgaXNUcmFuc2l0aW9uaW5nID0gY2FsbGJhY2sgJiYgVXRpbC5zdXBwb3J0c1RyYW5zaXRpb25FbmQoKSAmJiAoYWN0aXZlICYmICQoYWN0aXZlKS5oYXNDbGFzcyhDbGFzc05hbWUuRkFERSkgfHwgQm9vbGVhbigkKGNvbnRhaW5lcikuZmluZChTZWxlY3Rvci5GQURFX0NISUxEKVswXSkpO1xyXG5cclxuICAgICAgICB2YXIgY29tcGxldGUgPSAkLnByb3h5KHRoaXMuX3RyYW5zaXRpb25Db21wbGV0ZSwgdGhpcywgZWxlbWVudCwgYWN0aXZlLCBpc1RyYW5zaXRpb25pbmcsIGNhbGxiYWNrKTtcclxuXHJcbiAgICAgICAgaWYgKGFjdGl2ZSAmJiBpc1RyYW5zaXRpb25pbmcpIHtcclxuICAgICAgICAgICQoYWN0aXZlKS5vbmUoVXRpbC5UUkFOU0lUSU9OX0VORCwgY29tcGxldGUpLmVtdWxhdGVUcmFuc2l0aW9uRW5kKFRSQU5TSVRJT05fRFVSQVRJT04pO1xyXG4gICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICBjb21wbGV0ZSgpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgaWYgKGFjdGl2ZSkge1xyXG4gICAgICAgICAgJChhY3RpdmUpLnJlbW92ZUNsYXNzKENsYXNzTmFtZS5JTik7XHJcbiAgICAgICAgfVxyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ190cmFuc2l0aW9uQ29tcGxldGUnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX3RyYW5zaXRpb25Db21wbGV0ZShlbGVtZW50LCBhY3RpdmUsIGlzVHJhbnNpdGlvbmluZywgY2FsbGJhY2spIHtcclxuICAgICAgICBpZiAoYWN0aXZlKSB7XHJcbiAgICAgICAgICAkKGFjdGl2ZSkucmVtb3ZlQ2xhc3MoQ2xhc3NOYW1lLkFDVElWRSk7XHJcblxyXG4gICAgICAgICAgdmFyIGRyb3Bkb3duQ2hpbGQgPSAkKGFjdGl2ZSkuZmluZChTZWxlY3Rvci5EUk9QRE9XTl9BQ1RJVkVfQ0hJTEQpWzBdO1xyXG5cclxuICAgICAgICAgIGlmIChkcm9wZG93bkNoaWxkKSB7XHJcbiAgICAgICAgICAgICQoZHJvcGRvd25DaGlsZCkucmVtb3ZlQ2xhc3MoQ2xhc3NOYW1lLkFDVElWRSk7XHJcbiAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgYWN0aXZlLnNldEF0dHJpYnV0ZSgnYXJpYS1leHBhbmRlZCcsIGZhbHNlKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgICQoZWxlbWVudCkuYWRkQ2xhc3MoQ2xhc3NOYW1lLkFDVElWRSk7XHJcbiAgICAgICAgZWxlbWVudC5zZXRBdHRyaWJ1dGUoJ2FyaWEtZXhwYW5kZWQnLCB0cnVlKTtcclxuXHJcbiAgICAgICAgaWYgKGlzVHJhbnNpdGlvbmluZykge1xyXG4gICAgICAgICAgVXRpbC5yZWZsb3coZWxlbWVudCk7XHJcbiAgICAgICAgICAkKGVsZW1lbnQpLmFkZENsYXNzKENsYXNzTmFtZS5JTik7XHJcbiAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICQoZWxlbWVudCkucmVtb3ZlQ2xhc3MoQ2xhc3NOYW1lLkZBREUpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgaWYgKGVsZW1lbnQucGFyZW50Tm9kZSAmJiAkKGVsZW1lbnQucGFyZW50Tm9kZSkuaGFzQ2xhc3MoQ2xhc3NOYW1lLkRST1BET1dOX01FTlUpKSB7XHJcblxyXG4gICAgICAgICAgdmFyIGRyb3Bkb3duRWxlbWVudCA9ICQoZWxlbWVudCkuY2xvc2VzdChTZWxlY3Rvci5EUk9QRE9XTilbMF07XHJcbiAgICAgICAgICBpZiAoZHJvcGRvd25FbGVtZW50KSB7XHJcbiAgICAgICAgICAgICQoZHJvcGRvd25FbGVtZW50KS5maW5kKFNlbGVjdG9yLkRST1BET1dOX1RPR0dMRSkuYWRkQ2xhc3MoQ2xhc3NOYW1lLkFDVElWRSk7XHJcbiAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgZWxlbWVudC5zZXRBdHRyaWJ1dGUoJ2FyaWEtZXhwYW5kZWQnLCB0cnVlKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGlmIChjYWxsYmFjaykge1xyXG4gICAgICAgICAgY2FsbGJhY2soKTtcclxuICAgICAgICB9XHJcbiAgICAgIH1cclxuXHJcbiAgICAgIC8vIHN0YXRpY1xyXG5cclxuICAgIH1dLCBbe1xyXG4gICAgICBrZXk6ICdfalF1ZXJ5SW50ZXJmYWNlJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9qUXVlcnlJbnRlcmZhY2UoY29uZmlnKSB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZWFjaChmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICB2YXIgJHRoaXMgPSAkKHRoaXMpO1xyXG4gICAgICAgICAgdmFyIGRhdGEgPSAkdGhpcy5kYXRhKERBVEFfS0VZKTtcclxuXHJcbiAgICAgICAgICBpZiAoIWRhdGEpIHtcclxuICAgICAgICAgICAgZGF0YSA9IGRhdGEgPSBuZXcgVGFiKHRoaXMpO1xyXG4gICAgICAgICAgICAkdGhpcy5kYXRhKERBVEFfS0VZLCBkYXRhKTtcclxuICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICBpZiAodHlwZW9mIGNvbmZpZyA9PT0gJ3N0cmluZycpIHtcclxuICAgICAgICAgICAgaWYgKGRhdGFbY29uZmlnXSA9PT0gdW5kZWZpbmVkKSB7XHJcbiAgICAgICAgICAgICAgdGhyb3cgbmV3IEVycm9yKCdObyBtZXRob2QgbmFtZWQgXCInICsgY29uZmlnICsgJ1wiJyk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgZGF0YVtjb25maWddKCk7XHJcbiAgICAgICAgICB9XHJcbiAgICAgICAgfSk7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnVkVSU0lPTicsXHJcbiAgICAgIGdldDogZnVuY3Rpb24gZ2V0KCkge1xyXG4gICAgICAgIHJldHVybiBWRVJTSU9OO1xyXG4gICAgICB9XHJcbiAgICB9XSk7XHJcblxyXG4gICAgcmV0dXJuIFRhYjtcclxuICB9KSgpO1xyXG5cclxuICAkKGRvY3VtZW50KS5vbihFdmVudC5DTElDS19EQVRBX0FQSSwgU2VsZWN0b3IuREFUQV9UT0dHTEUsIGZ1bmN0aW9uIChldmVudCkge1xyXG4gICAgZXZlbnQucHJldmVudERlZmF1bHQoKTtcclxuICAgIFRhYi5falF1ZXJ5SW50ZXJmYWNlLmNhbGwoJCh0aGlzKSwgJ3Nob3cnKTtcclxuICB9KTtcclxuXHJcbiAgLyoqXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICogalF1ZXJ5XHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICovXHJcblxyXG4gICQuZm5bTkFNRV0gPSBUYWIuX2pRdWVyeUludGVyZmFjZTtcclxuICAkLmZuW05BTUVdLkNvbnN0cnVjdG9yID0gVGFiO1xyXG4gICQuZm5bTkFNRV0ubm9Db25mbGljdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICQuZm5bTkFNRV0gPSBKUVVFUllfTk9fQ09ORkxJQ1Q7XHJcbiAgICByZXR1cm4gVGFiLl9qUXVlcnlJbnRlcmZhY2U7XHJcbiAgfTtcclxuXHJcbiAgcmV0dXJuIFRhYjtcclxufSkoalF1ZXJ5KTtcclxuLy8jIHNvdXJjZU1hcHBpbmdVUkw9dGFiLmpzLm1hcFxyXG4iLCIvKiBnbG9iYWwgVGV0aGVyICovXHJcblxyXG4ndXNlIHN0cmljdCc7XHJcblxyXG52YXIgX2NyZWF0ZUNsYXNzID0gKGZ1bmN0aW9uICgpIHsgZnVuY3Rpb24gZGVmaW5lUHJvcGVydGllcyh0YXJnZXQsIHByb3BzKSB7IGZvciAodmFyIGkgPSAwOyBpIDwgcHJvcHMubGVuZ3RoOyBpKyspIHsgdmFyIGRlc2NyaXB0b3IgPSBwcm9wc1tpXTsgZGVzY3JpcHRvci5lbnVtZXJhYmxlID0gZGVzY3JpcHRvci5lbnVtZXJhYmxlIHx8IGZhbHNlOyBkZXNjcmlwdG9yLmNvbmZpZ3VyYWJsZSA9IHRydWU7IGlmICgndmFsdWUnIGluIGRlc2NyaXB0b3IpIGRlc2NyaXB0b3Iud3JpdGFibGUgPSB0cnVlOyBPYmplY3QuZGVmaW5lUHJvcGVydHkodGFyZ2V0LCBkZXNjcmlwdG9yLmtleSwgZGVzY3JpcHRvcik7IH0gfSByZXR1cm4gZnVuY3Rpb24gKENvbnN0cnVjdG9yLCBwcm90b1Byb3BzLCBzdGF0aWNQcm9wcykgeyBpZiAocHJvdG9Qcm9wcykgZGVmaW5lUHJvcGVydGllcyhDb25zdHJ1Y3Rvci5wcm90b3R5cGUsIHByb3RvUHJvcHMpOyBpZiAoc3RhdGljUHJvcHMpIGRlZmluZVByb3BlcnRpZXMoQ29uc3RydWN0b3IsIHN0YXRpY1Byb3BzKTsgcmV0dXJuIENvbnN0cnVjdG9yOyB9OyB9KSgpO1xyXG5cclxuZnVuY3Rpb24gX2NsYXNzQ2FsbENoZWNrKGluc3RhbmNlLCBDb25zdHJ1Y3RvcikgeyBpZiAoIShpbnN0YW5jZSBpbnN0YW5jZW9mIENvbnN0cnVjdG9yKSkgeyB0aHJvdyBuZXcgVHlwZUVycm9yKCdDYW5ub3QgY2FsbCBhIGNsYXNzIGFzIGEgZnVuY3Rpb24nKTsgfSB9XHJcblxyXG4vKipcclxuICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICogQm9vdHN0cmFwICh2NC4wLjAtYWxwaGEuMik6IHRvb2x0aXAuanNcclxuICogTGljZW5zZWQgdW5kZXIgTUlUIChodHRwczovL2dpdGh1Yi5jb20vdHdicy9ib290c3RyYXAvYmxvYi9tYXN0ZXIvTElDRU5TRSlcclxuICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICovXHJcblxyXG52YXIgVG9vbHRpcCA9IChmdW5jdGlvbiAoJCkge1xyXG5cclxuICAvKipcclxuICAgKiBDaGVjayBmb3IgVGV0aGVyIGRlcGVuZGVuY3lcclxuICAgKiBUZXRoZXIgLSBodHRwOi8vZ2l0aHViLmh1YnNwb3QuY29tL3RldGhlci9cclxuICAgKi9cclxuICBpZiAod2luZG93LlRldGhlciA9PT0gdW5kZWZpbmVkKSB7XHJcbiAgICB0aHJvdyBuZXcgRXJyb3IoJ0Jvb3RzdHJhcCB0b29sdGlwcyByZXF1aXJlIFRldGhlciAoaHR0cDovL2dpdGh1Yi5odWJzcG90LmNvbS90ZXRoZXIvKScpO1xyXG4gIH1cclxuXHJcbiAgLyoqXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICogQ29uc3RhbnRzXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICovXHJcblxyXG4gIHZhciBOQU1FID0gJ3Rvb2x0aXAnO1xyXG4gIHZhciBWRVJTSU9OID0gJzQuMC4wLWFscGhhJztcclxuICB2YXIgREFUQV9LRVkgPSAnYnMudG9vbHRpcCc7XHJcbiAgdmFyIEVWRU5UX0tFWSA9ICcuJyArIERBVEFfS0VZO1xyXG4gIHZhciBKUVVFUllfTk9fQ09ORkxJQ1QgPSAkLmZuW05BTUVdO1xyXG4gIHZhciBUUkFOU0lUSU9OX0RVUkFUSU9OID0gMTUwO1xyXG4gIHZhciBDTEFTU19QUkVGSVggPSAnYnMtdGV0aGVyJztcclxuXHJcbiAgdmFyIERlZmF1bHQgPSB7XHJcbiAgICBhbmltYXRpb246IHRydWUsXHJcbiAgICB0ZW1wbGF0ZTogJzxkaXYgY2xhc3M9XCJ0b29sdGlwXCIgcm9sZT1cInRvb2x0aXBcIj4nICsgJzxkaXYgY2xhc3M9XCJ0b29sdGlwLWFycm93XCI+PC9kaXY+JyArICc8ZGl2IGNsYXNzPVwidG9vbHRpcC1pbm5lclwiPjwvZGl2PjwvZGl2PicsXHJcbiAgICB0cmlnZ2VyOiAnaG92ZXIgZm9jdXMnLFxyXG4gICAgdGl0bGU6ICcnLFxyXG4gICAgZGVsYXk6IDAsXHJcbiAgICBodG1sOiBmYWxzZSxcclxuICAgIHNlbGVjdG9yOiBmYWxzZSxcclxuICAgIHBsYWNlbWVudDogJ3RvcCcsXHJcbiAgICBvZmZzZXQ6ICcwIDAnLFxyXG4gICAgY29uc3RyYWludHM6IFtdXHJcbiAgfTtcclxuXHJcbiAgdmFyIERlZmF1bHRUeXBlID0ge1xyXG4gICAgYW5pbWF0aW9uOiAnYm9vbGVhbicsXHJcbiAgICB0ZW1wbGF0ZTogJ3N0cmluZycsXHJcbiAgICB0aXRsZTogJyhzdHJpbmd8ZWxlbWVudHxmdW5jdGlvbiknLFxyXG4gICAgdHJpZ2dlcjogJ3N0cmluZycsXHJcbiAgICBkZWxheTogJyhudW1iZXJ8b2JqZWN0KScsXHJcbiAgICBodG1sOiAnYm9vbGVhbicsXHJcbiAgICBzZWxlY3RvcjogJyhzdHJpbmd8Ym9vbGVhbiknLFxyXG4gICAgcGxhY2VtZW50OiAnKHN0cmluZ3xmdW5jdGlvbiknLFxyXG4gICAgb2Zmc2V0OiAnc3RyaW5nJyxcclxuICAgIGNvbnN0cmFpbnRzOiAnYXJyYXknXHJcbiAgfTtcclxuXHJcbiAgdmFyIEF0dGFjaG1lbnRNYXAgPSB7XHJcbiAgICBUT1A6ICdib3R0b20gY2VudGVyJyxcclxuICAgIFJJR0hUOiAnbWlkZGxlIGxlZnQnLFxyXG4gICAgQk9UVE9NOiAndG9wIGNlbnRlcicsXHJcbiAgICBMRUZUOiAnbWlkZGxlIHJpZ2h0J1xyXG4gIH07XHJcblxyXG4gIHZhciBIb3ZlclN0YXRlID0ge1xyXG4gICAgSU46ICdpbicsXHJcbiAgICBPVVQ6ICdvdXQnXHJcbiAgfTtcclxuXHJcbiAgdmFyIEV2ZW50ID0ge1xyXG4gICAgSElERTogJ2hpZGUnICsgRVZFTlRfS0VZLFxyXG4gICAgSElEREVOOiAnaGlkZGVuJyArIEVWRU5UX0tFWSxcclxuICAgIFNIT1c6ICdzaG93JyArIEVWRU5UX0tFWSxcclxuICAgIFNIT1dOOiAnc2hvd24nICsgRVZFTlRfS0VZLFxyXG4gICAgSU5TRVJURUQ6ICdpbnNlcnRlZCcgKyBFVkVOVF9LRVksXHJcbiAgICBDTElDSzogJ2NsaWNrJyArIEVWRU5UX0tFWSxcclxuICAgIEZPQ1VTSU46ICdmb2N1c2luJyArIEVWRU5UX0tFWSxcclxuICAgIEZPQ1VTT1VUOiAnZm9jdXNvdXQnICsgRVZFTlRfS0VZLFxyXG4gICAgTU9VU0VFTlRFUjogJ21vdXNlZW50ZXInICsgRVZFTlRfS0VZLFxyXG4gICAgTU9VU0VMRUFWRTogJ21vdXNlbGVhdmUnICsgRVZFTlRfS0VZXHJcbiAgfTtcclxuXHJcbiAgdmFyIENsYXNzTmFtZSA9IHtcclxuICAgIEZBREU6ICdmYWRlJyxcclxuICAgIElOOiAnaW4nXHJcbiAgfTtcclxuXHJcbiAgdmFyIFNlbGVjdG9yID0ge1xyXG4gICAgVE9PTFRJUDogJy50b29sdGlwJyxcclxuICAgIFRPT0xUSVBfSU5ORVI6ICcudG9vbHRpcC1pbm5lcidcclxuICB9O1xyXG5cclxuICB2YXIgVGV0aGVyQ2xhc3MgPSB7XHJcbiAgICBlbGVtZW50OiBmYWxzZSxcclxuICAgIGVuYWJsZWQ6IGZhbHNlXHJcbiAgfTtcclxuXHJcbiAgdmFyIFRyaWdnZXIgPSB7XHJcbiAgICBIT1ZFUjogJ2hvdmVyJyxcclxuICAgIEZPQ1VTOiAnZm9jdXMnLFxyXG4gICAgQ0xJQ0s6ICdjbGljaycsXHJcbiAgICBNQU5VQUw6ICdtYW51YWwnXHJcbiAgfTtcclxuXHJcbiAgLyoqXHJcbiAgICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICogQ2xhc3MgRGVmaW5pdGlvblxyXG4gICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAqL1xyXG5cclxuICB2YXIgVG9vbHRpcCA9IChmdW5jdGlvbiAoKSB7XHJcbiAgICBmdW5jdGlvbiBUb29sdGlwKGVsZW1lbnQsIGNvbmZpZykge1xyXG4gICAgICBfY2xhc3NDYWxsQ2hlY2sodGhpcywgVG9vbHRpcCk7XHJcblxyXG4gICAgICAvLyBwcml2YXRlXHJcbiAgICAgIHRoaXMuX2lzRW5hYmxlZCA9IHRydWU7XHJcbiAgICAgIHRoaXMuX3RpbWVvdXQgPSAwO1xyXG4gICAgICB0aGlzLl9ob3ZlclN0YXRlID0gJyc7XHJcbiAgICAgIHRoaXMuX2FjdGl2ZVRyaWdnZXIgPSB7fTtcclxuICAgICAgdGhpcy5fdGV0aGVyID0gbnVsbDtcclxuXHJcbiAgICAgIC8vIHByb3RlY3RlZFxyXG4gICAgICB0aGlzLmVsZW1lbnQgPSBlbGVtZW50O1xyXG4gICAgICB0aGlzLmNvbmZpZyA9IHRoaXMuX2dldENvbmZpZyhjb25maWcpO1xyXG4gICAgICB0aGlzLnRpcCA9IG51bGw7XHJcblxyXG4gICAgICB0aGlzLl9zZXRMaXN0ZW5lcnMoKTtcclxuICAgIH1cclxuXHJcbiAgICAvKipcclxuICAgICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAgICogalF1ZXJ5XHJcbiAgICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgICAqL1xyXG5cclxuICAgIC8vIGdldHRlcnNcclxuXHJcbiAgICBfY3JlYXRlQ2xhc3MoVG9vbHRpcCwgW3tcclxuICAgICAga2V5OiAnZW5hYmxlJyxcclxuXHJcbiAgICAgIC8vIHB1YmxpY1xyXG5cclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIGVuYWJsZSgpIHtcclxuICAgICAgICB0aGlzLl9pc0VuYWJsZWQgPSB0cnVlO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ2Rpc2FibGUnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gZGlzYWJsZSgpIHtcclxuICAgICAgICB0aGlzLl9pc0VuYWJsZWQgPSBmYWxzZTtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICd0b2dnbGVFbmFibGVkJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIHRvZ2dsZUVuYWJsZWQoKSB7XHJcbiAgICAgICAgdGhpcy5faXNFbmFibGVkID0gIXRoaXMuX2lzRW5hYmxlZDtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICd0b2dnbGUnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gdG9nZ2xlKGV2ZW50KSB7XHJcbiAgICAgICAgaWYgKGV2ZW50KSB7XHJcbiAgICAgICAgICB2YXIgZGF0YUtleSA9IHRoaXMuY29uc3RydWN0b3IuREFUQV9LRVk7XHJcbiAgICAgICAgICB2YXIgY29udGV4dCA9ICQoZXZlbnQuY3VycmVudFRhcmdldCkuZGF0YShkYXRhS2V5KTtcclxuXHJcbiAgICAgICAgICBpZiAoIWNvbnRleHQpIHtcclxuICAgICAgICAgICAgY29udGV4dCA9IG5ldyB0aGlzLmNvbnN0cnVjdG9yKGV2ZW50LmN1cnJlbnRUYXJnZXQsIHRoaXMuX2dldERlbGVnYXRlQ29uZmlnKCkpO1xyXG4gICAgICAgICAgICAkKGV2ZW50LmN1cnJlbnRUYXJnZXQpLmRhdGEoZGF0YUtleSwgY29udGV4dCk7XHJcbiAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgY29udGV4dC5fYWN0aXZlVHJpZ2dlci5jbGljayA9ICFjb250ZXh0Ll9hY3RpdmVUcmlnZ2VyLmNsaWNrO1xyXG5cclxuICAgICAgICAgIGlmIChjb250ZXh0Ll9pc1dpdGhBY3RpdmVUcmlnZ2VyKCkpIHtcclxuICAgICAgICAgICAgY29udGV4dC5fZW50ZXIobnVsbCwgY29udGV4dCk7XHJcbiAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICBjb250ZXh0Ll9sZWF2ZShudWxsLCBjb250ZXh0KTtcclxuICAgICAgICAgIH1cclxuICAgICAgICB9IGVsc2Uge1xyXG5cclxuICAgICAgICAgIGlmICgkKHRoaXMuZ2V0VGlwRWxlbWVudCgpKS5oYXNDbGFzcyhDbGFzc05hbWUuSU4pKSB7XHJcbiAgICAgICAgICAgIHRoaXMuX2xlYXZlKG51bGwsIHRoaXMpO1xyXG4gICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgdGhpcy5fZW50ZXIobnVsbCwgdGhpcyk7XHJcbiAgICAgICAgfVxyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ2Rpc3Bvc2UnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gZGlzcG9zZSgpIHtcclxuICAgICAgICBjbGVhclRpbWVvdXQodGhpcy5fdGltZW91dCk7XHJcblxyXG4gICAgICAgIHRoaXMuY2xlYW51cFRldGhlcigpO1xyXG5cclxuICAgICAgICAkLnJlbW92ZURhdGEodGhpcy5lbGVtZW50LCB0aGlzLmNvbnN0cnVjdG9yLkRBVEFfS0VZKTtcclxuXHJcbiAgICAgICAgJCh0aGlzLmVsZW1lbnQpLm9mZih0aGlzLmNvbnN0cnVjdG9yLkVWRU5UX0tFWSk7XHJcblxyXG4gICAgICAgIGlmICh0aGlzLnRpcCkge1xyXG4gICAgICAgICAgJCh0aGlzLnRpcCkucmVtb3ZlKCk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB0aGlzLl9pc0VuYWJsZWQgPSBudWxsO1xyXG4gICAgICAgIHRoaXMuX3RpbWVvdXQgPSBudWxsO1xyXG4gICAgICAgIHRoaXMuX2hvdmVyU3RhdGUgPSBudWxsO1xyXG4gICAgICAgIHRoaXMuX2FjdGl2ZVRyaWdnZXIgPSBudWxsO1xyXG4gICAgICAgIHRoaXMuX3RldGhlciA9IG51bGw7XHJcblxyXG4gICAgICAgIHRoaXMuZWxlbWVudCA9IG51bGw7XHJcbiAgICAgICAgdGhpcy5jb25maWcgPSBudWxsO1xyXG4gICAgICAgIHRoaXMudGlwID0gbnVsbDtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdzaG93JyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIHNob3coKSB7XHJcbiAgICAgICAgdmFyIF90aGlzID0gdGhpcztcclxuXHJcbiAgICAgICAgdmFyIHNob3dFdmVudCA9ICQuRXZlbnQodGhpcy5jb25zdHJ1Y3Rvci5FdmVudC5TSE9XKTtcclxuXHJcbiAgICAgICAgaWYgKHRoaXMuaXNXaXRoQ29udGVudCgpICYmIHRoaXMuX2lzRW5hYmxlZCkge1xyXG4gICAgICAgICAgJCh0aGlzLmVsZW1lbnQpLnRyaWdnZXIoc2hvd0V2ZW50KTtcclxuXHJcbiAgICAgICAgICB2YXIgaXNJblRoZURvbSA9ICQuY29udGFpbnModGhpcy5lbGVtZW50Lm93bmVyRG9jdW1lbnQuZG9jdW1lbnRFbGVtZW50LCB0aGlzLmVsZW1lbnQpO1xyXG5cclxuICAgICAgICAgIGlmIChzaG93RXZlbnQuaXNEZWZhdWx0UHJldmVudGVkKCkgfHwgIWlzSW5UaGVEb20pIHtcclxuICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgfVxyXG5cclxuICAgICAgICAgIHZhciB0aXAgPSB0aGlzLmdldFRpcEVsZW1lbnQoKTtcclxuICAgICAgICAgIHZhciB0aXBJZCA9IFV0aWwuZ2V0VUlEKHRoaXMuY29uc3RydWN0b3IuTkFNRSk7XHJcblxyXG4gICAgICAgICAgdGlwLnNldEF0dHJpYnV0ZSgnaWQnLCB0aXBJZCk7XHJcbiAgICAgICAgICB0aGlzLmVsZW1lbnQuc2V0QXR0cmlidXRlKCdhcmlhLWRlc2NyaWJlZGJ5JywgdGlwSWQpO1xyXG5cclxuICAgICAgICAgIHRoaXMuc2V0Q29udGVudCgpO1xyXG5cclxuICAgICAgICAgIGlmICh0aGlzLmNvbmZpZy5hbmltYXRpb24pIHtcclxuICAgICAgICAgICAgJCh0aXApLmFkZENsYXNzKENsYXNzTmFtZS5GQURFKTtcclxuICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICB2YXIgcGxhY2VtZW50ID0gdHlwZW9mIHRoaXMuY29uZmlnLnBsYWNlbWVudCA9PT0gJ2Z1bmN0aW9uJyA/IHRoaXMuY29uZmlnLnBsYWNlbWVudC5jYWxsKHRoaXMsIHRpcCwgdGhpcy5lbGVtZW50KSA6IHRoaXMuY29uZmlnLnBsYWNlbWVudDtcclxuXHJcbiAgICAgICAgICB2YXIgYXR0YWNobWVudCA9IHRoaXMuX2dldEF0dGFjaG1lbnQocGxhY2VtZW50KTtcclxuXHJcbiAgICAgICAgICAkKHRpcCkuZGF0YSh0aGlzLmNvbnN0cnVjdG9yLkRBVEFfS0VZLCB0aGlzKS5hcHBlbmRUbyhkb2N1bWVudC5ib2R5KTtcclxuXHJcbiAgICAgICAgICAkKHRoaXMuZWxlbWVudCkudHJpZ2dlcih0aGlzLmNvbnN0cnVjdG9yLkV2ZW50LklOU0VSVEVEKTtcclxuXHJcbiAgICAgICAgICB0aGlzLl90ZXRoZXIgPSBuZXcgVGV0aGVyKHtcclxuICAgICAgICAgICAgYXR0YWNobWVudDogYXR0YWNobWVudCxcclxuICAgICAgICAgICAgZWxlbWVudDogdGlwLFxyXG4gICAgICAgICAgICB0YXJnZXQ6IHRoaXMuZWxlbWVudCxcclxuICAgICAgICAgICAgY2xhc3NlczogVGV0aGVyQ2xhc3MsXHJcbiAgICAgICAgICAgIGNsYXNzUHJlZml4OiBDTEFTU19QUkVGSVgsXHJcbiAgICAgICAgICAgIG9mZnNldDogdGhpcy5jb25maWcub2Zmc2V0LFxyXG4gICAgICAgICAgICBjb25zdHJhaW50czogdGhpcy5jb25maWcuY29uc3RyYWludHMsXHJcbiAgICAgICAgICAgIGFkZFRhcmdldENsYXNzZXM6IGZhbHNlXHJcbiAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgICBVdGlsLnJlZmxvdyh0aXApO1xyXG4gICAgICAgICAgdGhpcy5fdGV0aGVyLnBvc2l0aW9uKCk7XHJcblxyXG4gICAgICAgICAgJCh0aXApLmFkZENsYXNzKENsYXNzTmFtZS5JTik7XHJcblxyXG4gICAgICAgICAgdmFyIGNvbXBsZXRlID0gZnVuY3Rpb24gY29tcGxldGUoKSB7XHJcbiAgICAgICAgICAgIHZhciBwcmV2SG92ZXJTdGF0ZSA9IF90aGlzLl9ob3ZlclN0YXRlO1xyXG4gICAgICAgICAgICBfdGhpcy5faG92ZXJTdGF0ZSA9IG51bGw7XHJcblxyXG4gICAgICAgICAgICAkKF90aGlzLmVsZW1lbnQpLnRyaWdnZXIoX3RoaXMuY29uc3RydWN0b3IuRXZlbnQuU0hPV04pO1xyXG5cclxuICAgICAgICAgICAgaWYgKHByZXZIb3ZlclN0YXRlID09PSBIb3ZlclN0YXRlLk9VVCkge1xyXG4gICAgICAgICAgICAgIF90aGlzLl9sZWF2ZShudWxsLCBfdGhpcyk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgIH07XHJcblxyXG4gICAgICAgICAgaWYgKFV0aWwuc3VwcG9ydHNUcmFuc2l0aW9uRW5kKCkgJiYgJCh0aGlzLnRpcCkuaGFzQ2xhc3MoQ2xhc3NOYW1lLkZBREUpKSB7XHJcbiAgICAgICAgICAgICQodGhpcy50aXApLm9uZShVdGlsLlRSQU5TSVRJT05fRU5ELCBjb21wbGV0ZSkuZW11bGF0ZVRyYW5zaXRpb25FbmQoVG9vbHRpcC5fVFJBTlNJVElPTl9EVVJBVElPTik7XHJcbiAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICBjb21wbGV0ZSgpO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdoaWRlJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIGhpZGUoY2FsbGJhY2spIHtcclxuICAgICAgICB2YXIgX3RoaXMyID0gdGhpcztcclxuXHJcbiAgICAgICAgdmFyIHRpcCA9IHRoaXMuZ2V0VGlwRWxlbWVudCgpO1xyXG4gICAgICAgIHZhciBoaWRlRXZlbnQgPSAkLkV2ZW50KHRoaXMuY29uc3RydWN0b3IuRXZlbnQuSElERSk7XHJcbiAgICAgICAgdmFyIGNvbXBsZXRlID0gZnVuY3Rpb24gY29tcGxldGUoKSB7XHJcbiAgICAgICAgICBpZiAoX3RoaXMyLl9ob3ZlclN0YXRlICE9PSBIb3ZlclN0YXRlLklOICYmIHRpcC5wYXJlbnROb2RlKSB7XHJcbiAgICAgICAgICAgIHRpcC5wYXJlbnROb2RlLnJlbW92ZUNoaWxkKHRpcCk7XHJcbiAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgX3RoaXMyLmVsZW1lbnQucmVtb3ZlQXR0cmlidXRlKCdhcmlhLWRlc2NyaWJlZGJ5Jyk7XHJcbiAgICAgICAgICAkKF90aGlzMi5lbGVtZW50KS50cmlnZ2VyKF90aGlzMi5jb25zdHJ1Y3Rvci5FdmVudC5ISURERU4pO1xyXG4gICAgICAgICAgX3RoaXMyLmNsZWFudXBUZXRoZXIoKTtcclxuXHJcbiAgICAgICAgICBpZiAoY2FsbGJhY2spIHtcclxuICAgICAgICAgICAgY2FsbGJhY2soKTtcclxuICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG5cclxuICAgICAgICAkKHRoaXMuZWxlbWVudCkudHJpZ2dlcihoaWRlRXZlbnQpO1xyXG5cclxuICAgICAgICBpZiAoaGlkZUV2ZW50LmlzRGVmYXVsdFByZXZlbnRlZCgpKSB7XHJcbiAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICAkKHRpcCkucmVtb3ZlQ2xhc3MoQ2xhc3NOYW1lLklOKTtcclxuXHJcbiAgICAgICAgaWYgKFV0aWwuc3VwcG9ydHNUcmFuc2l0aW9uRW5kKCkgJiYgJCh0aGlzLnRpcCkuaGFzQ2xhc3MoQ2xhc3NOYW1lLkZBREUpKSB7XHJcblxyXG4gICAgICAgICAgJCh0aXApLm9uZShVdGlsLlRSQU5TSVRJT05fRU5ELCBjb21wbGV0ZSkuZW11bGF0ZVRyYW5zaXRpb25FbmQoVFJBTlNJVElPTl9EVVJBVElPTik7XHJcbiAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgIGNvbXBsZXRlKCk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB0aGlzLl9ob3ZlclN0YXRlID0gJyc7XHJcbiAgICAgIH1cclxuXHJcbiAgICAgIC8vIHByb3RlY3RlZFxyXG5cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnaXNXaXRoQ29udGVudCcsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBpc1dpdGhDb250ZW50KCkge1xyXG4gICAgICAgIHJldHVybiBCb29sZWFuKHRoaXMuZ2V0VGl0bGUoKSk7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnZ2V0VGlwRWxlbWVudCcsXHJcbiAgICAgIHZhbHVlOiBmdW5jdGlvbiBnZXRUaXBFbGVtZW50KCkge1xyXG4gICAgICAgIHJldHVybiB0aGlzLnRpcCA9IHRoaXMudGlwIHx8ICQodGhpcy5jb25maWcudGVtcGxhdGUpWzBdO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ3NldENvbnRlbnQnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gc2V0Q29udGVudCgpIHtcclxuICAgICAgICB2YXIgJHRpcCA9ICQodGhpcy5nZXRUaXBFbGVtZW50KCkpO1xyXG5cclxuICAgICAgICB0aGlzLnNldEVsZW1lbnRDb250ZW50KCR0aXAuZmluZChTZWxlY3Rvci5UT09MVElQX0lOTkVSKSwgdGhpcy5nZXRUaXRsZSgpKTtcclxuXHJcbiAgICAgICAgJHRpcC5yZW1vdmVDbGFzcyhDbGFzc05hbWUuRkFERSkucmVtb3ZlQ2xhc3MoQ2xhc3NOYW1lLklOKTtcclxuXHJcbiAgICAgICAgdGhpcy5jbGVhbnVwVGV0aGVyKCk7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnc2V0RWxlbWVudENvbnRlbnQnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gc2V0RWxlbWVudENvbnRlbnQoJGVsZW1lbnQsIGNvbnRlbnQpIHtcclxuICAgICAgICB2YXIgaHRtbCA9IHRoaXMuY29uZmlnLmh0bWw7XHJcbiAgICAgICAgaWYgKHR5cGVvZiBjb250ZW50ID09PSAnb2JqZWN0JyAmJiAoY29udGVudC5ub2RlVHlwZSB8fCBjb250ZW50LmpxdWVyeSkpIHtcclxuICAgICAgICAgIC8vIGNvbnRlbnQgaXMgYSBET00gbm9kZSBvciBhIGpRdWVyeVxyXG4gICAgICAgICAgaWYgKGh0bWwpIHtcclxuICAgICAgICAgICAgaWYgKCEkKGNvbnRlbnQpLnBhcmVudCgpLmlzKCRlbGVtZW50KSkge1xyXG4gICAgICAgICAgICAgICRlbGVtZW50LmVtcHR5KCkuYXBwZW5kKGNvbnRlbnQpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAkZWxlbWVudC50ZXh0KCQoY29udGVudCkudGV4dCgpKTtcclxuICAgICAgICAgIH1cclxuICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgJGVsZW1lbnRbaHRtbCA/ICdodG1sJyA6ICd0ZXh0J10oY29udGVudCk7XHJcbiAgICAgICAgfVxyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ2dldFRpdGxlJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIGdldFRpdGxlKCkge1xyXG4gICAgICAgIHZhciB0aXRsZSA9IHRoaXMuZWxlbWVudC5nZXRBdHRyaWJ1dGUoJ2RhdGEtb3JpZ2luYWwtdGl0bGUnKTtcclxuXHJcbiAgICAgICAgaWYgKCF0aXRsZSkge1xyXG4gICAgICAgICAgdGl0bGUgPSB0eXBlb2YgdGhpcy5jb25maWcudGl0bGUgPT09ICdmdW5jdGlvbicgPyB0aGlzLmNvbmZpZy50aXRsZS5jYWxsKHRoaXMuZWxlbWVudCkgOiB0aGlzLmNvbmZpZy50aXRsZTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHJldHVybiB0aXRsZTtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdjbGVhbnVwVGV0aGVyJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIGNsZWFudXBUZXRoZXIoKSB7XHJcbiAgICAgICAgaWYgKHRoaXMuX3RldGhlcikge1xyXG4gICAgICAgICAgdGhpcy5fdGV0aGVyLmRlc3Ryb3koKTtcclxuICAgICAgICB9XHJcbiAgICAgIH1cclxuXHJcbiAgICAgIC8vIHByaXZhdGVcclxuXHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ19nZXRBdHRhY2htZW50JyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9nZXRBdHRhY2htZW50KHBsYWNlbWVudCkge1xyXG4gICAgICAgIHJldHVybiBBdHRhY2htZW50TWFwW3BsYWNlbWVudC50b1VwcGVyQ2FzZSgpXTtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfc2V0TGlzdGVuZXJzJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9zZXRMaXN0ZW5lcnMoKSB7XHJcbiAgICAgICAgdmFyIF90aGlzMyA9IHRoaXM7XHJcblxyXG4gICAgICAgIHZhciB0cmlnZ2VycyA9IHRoaXMuY29uZmlnLnRyaWdnZXIuc3BsaXQoJyAnKTtcclxuXHJcbiAgICAgICAgdHJpZ2dlcnMuZm9yRWFjaChmdW5jdGlvbiAodHJpZ2dlcikge1xyXG4gICAgICAgICAgaWYgKHRyaWdnZXIgPT09ICdjbGljaycpIHtcclxuICAgICAgICAgICAgJChfdGhpczMuZWxlbWVudCkub24oX3RoaXMzLmNvbnN0cnVjdG9yLkV2ZW50LkNMSUNLLCBfdGhpczMuY29uZmlnLnNlbGVjdG9yLCAkLnByb3h5KF90aGlzMy50b2dnbGUsIF90aGlzMykpO1xyXG4gICAgICAgICAgfSBlbHNlIGlmICh0cmlnZ2VyICE9PSBUcmlnZ2VyLk1BTlVBTCkge1xyXG4gICAgICAgICAgICB2YXIgZXZlbnRJbiA9IHRyaWdnZXIgPT09IFRyaWdnZXIuSE9WRVIgPyBfdGhpczMuY29uc3RydWN0b3IuRXZlbnQuTU9VU0VFTlRFUiA6IF90aGlzMy5jb25zdHJ1Y3Rvci5FdmVudC5GT0NVU0lOO1xyXG4gICAgICAgICAgICB2YXIgZXZlbnRPdXQgPSB0cmlnZ2VyID09PSBUcmlnZ2VyLkhPVkVSID8gX3RoaXMzLmNvbnN0cnVjdG9yLkV2ZW50Lk1PVVNFTEVBVkUgOiBfdGhpczMuY29uc3RydWN0b3IuRXZlbnQuRk9DVVNPVVQ7XHJcblxyXG4gICAgICAgICAgICAkKF90aGlzMy5lbGVtZW50KS5vbihldmVudEluLCBfdGhpczMuY29uZmlnLnNlbGVjdG9yLCAkLnByb3h5KF90aGlzMy5fZW50ZXIsIF90aGlzMykpLm9uKGV2ZW50T3V0LCBfdGhpczMuY29uZmlnLnNlbGVjdG9yLCAkLnByb3h5KF90aGlzMy5fbGVhdmUsIF90aGlzMykpO1xyXG4gICAgICAgICAgfVxyXG4gICAgICAgIH0pO1xyXG5cclxuICAgICAgICBpZiAodGhpcy5jb25maWcuc2VsZWN0b3IpIHtcclxuICAgICAgICAgIHRoaXMuY29uZmlnID0gJC5leHRlbmQoe30sIHRoaXMuY29uZmlnLCB7XHJcbiAgICAgICAgICAgIHRyaWdnZXI6ICdtYW51YWwnLFxyXG4gICAgICAgICAgICBzZWxlY3RvcjogJydcclxuICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICB0aGlzLl9maXhUaXRsZSgpO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfZml4VGl0bGUnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2ZpeFRpdGxlKCkge1xyXG4gICAgICAgIHZhciB0aXRsZVR5cGUgPSB0eXBlb2YgdGhpcy5lbGVtZW50LmdldEF0dHJpYnV0ZSgnZGF0YS1vcmlnaW5hbC10aXRsZScpO1xyXG4gICAgICAgIGlmICh0aGlzLmVsZW1lbnQuZ2V0QXR0cmlidXRlKCd0aXRsZScpIHx8IHRpdGxlVHlwZSAhPT0gJ3N0cmluZycpIHtcclxuICAgICAgICAgIHRoaXMuZWxlbWVudC5zZXRBdHRyaWJ1dGUoJ2RhdGEtb3JpZ2luYWwtdGl0bGUnLCB0aGlzLmVsZW1lbnQuZ2V0QXR0cmlidXRlKCd0aXRsZScpIHx8ICcnKTtcclxuICAgICAgICAgIHRoaXMuZWxlbWVudC5zZXRBdHRyaWJ1dGUoJ3RpdGxlJywgJycpO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfZW50ZXInLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2VudGVyKGV2ZW50LCBjb250ZXh0KSB7XHJcbiAgICAgICAgdmFyIGRhdGFLZXkgPSB0aGlzLmNvbnN0cnVjdG9yLkRBVEFfS0VZO1xyXG5cclxuICAgICAgICBjb250ZXh0ID0gY29udGV4dCB8fCAkKGV2ZW50LmN1cnJlbnRUYXJnZXQpLmRhdGEoZGF0YUtleSk7XHJcblxyXG4gICAgICAgIGlmICghY29udGV4dCkge1xyXG4gICAgICAgICAgY29udGV4dCA9IG5ldyB0aGlzLmNvbnN0cnVjdG9yKGV2ZW50LmN1cnJlbnRUYXJnZXQsIHRoaXMuX2dldERlbGVnYXRlQ29uZmlnKCkpO1xyXG4gICAgICAgICAgJChldmVudC5jdXJyZW50VGFyZ2V0KS5kYXRhKGRhdGFLZXksIGNvbnRleHQpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgaWYgKGV2ZW50KSB7XHJcbiAgICAgICAgICBjb250ZXh0Ll9hY3RpdmVUcmlnZ2VyW2V2ZW50LnR5cGUgPT09ICdmb2N1c2luJyA/IFRyaWdnZXIuRk9DVVMgOiBUcmlnZ2VyLkhPVkVSXSA9IHRydWU7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBpZiAoJChjb250ZXh0LmdldFRpcEVsZW1lbnQoKSkuaGFzQ2xhc3MoQ2xhc3NOYW1lLklOKSB8fCBjb250ZXh0Ll9ob3ZlclN0YXRlID09PSBIb3ZlclN0YXRlLklOKSB7XHJcbiAgICAgICAgICBjb250ZXh0Ll9ob3ZlclN0YXRlID0gSG92ZXJTdGF0ZS5JTjtcclxuICAgICAgICAgIHJldHVybjtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGNsZWFyVGltZW91dChjb250ZXh0Ll90aW1lb3V0KTtcclxuXHJcbiAgICAgICAgY29udGV4dC5faG92ZXJTdGF0ZSA9IEhvdmVyU3RhdGUuSU47XHJcblxyXG4gICAgICAgIGlmICghY29udGV4dC5jb25maWcuZGVsYXkgfHwgIWNvbnRleHQuY29uZmlnLmRlbGF5LnNob3cpIHtcclxuICAgICAgICAgIGNvbnRleHQuc2hvdygpO1xyXG4gICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgY29udGV4dC5fdGltZW91dCA9IHNldFRpbWVvdXQoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgaWYgKGNvbnRleHQuX2hvdmVyU3RhdGUgPT09IEhvdmVyU3RhdGUuSU4pIHtcclxuICAgICAgICAgICAgY29udGV4dC5zaG93KCk7XHJcbiAgICAgICAgICB9XHJcbiAgICAgICAgfSwgY29udGV4dC5jb25maWcuZGVsYXkuc2hvdyk7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX2xlYXZlJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9sZWF2ZShldmVudCwgY29udGV4dCkge1xyXG4gICAgICAgIHZhciBkYXRhS2V5ID0gdGhpcy5jb25zdHJ1Y3Rvci5EQVRBX0tFWTtcclxuXHJcbiAgICAgICAgY29udGV4dCA9IGNvbnRleHQgfHwgJChldmVudC5jdXJyZW50VGFyZ2V0KS5kYXRhKGRhdGFLZXkpO1xyXG5cclxuICAgICAgICBpZiAoIWNvbnRleHQpIHtcclxuICAgICAgICAgIGNvbnRleHQgPSBuZXcgdGhpcy5jb25zdHJ1Y3RvcihldmVudC5jdXJyZW50VGFyZ2V0LCB0aGlzLl9nZXREZWxlZ2F0ZUNvbmZpZygpKTtcclxuICAgICAgICAgICQoZXZlbnQuY3VycmVudFRhcmdldCkuZGF0YShkYXRhS2V5LCBjb250ZXh0KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGlmIChldmVudCkge1xyXG4gICAgICAgICAgY29udGV4dC5fYWN0aXZlVHJpZ2dlcltldmVudC50eXBlID09PSAnZm9jdXNvdXQnID8gVHJpZ2dlci5GT0NVUyA6IFRyaWdnZXIuSE9WRVJdID0gZmFsc2U7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBpZiAoY29udGV4dC5faXNXaXRoQWN0aXZlVHJpZ2dlcigpKSB7XHJcbiAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBjbGVhclRpbWVvdXQoY29udGV4dC5fdGltZW91dCk7XHJcblxyXG4gICAgICAgIGNvbnRleHQuX2hvdmVyU3RhdGUgPSBIb3ZlclN0YXRlLk9VVDtcclxuXHJcbiAgICAgICAgaWYgKCFjb250ZXh0LmNvbmZpZy5kZWxheSB8fCAhY29udGV4dC5jb25maWcuZGVsYXkuaGlkZSkge1xyXG4gICAgICAgICAgY29udGV4dC5oaWRlKCk7XHJcbiAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBjb250ZXh0Ll90aW1lb3V0ID0gc2V0VGltZW91dChmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICBpZiAoY29udGV4dC5faG92ZXJTdGF0ZSA9PT0gSG92ZXJTdGF0ZS5PVVQpIHtcclxuICAgICAgICAgICAgY29udGV4dC5oaWRlKCk7XHJcbiAgICAgICAgICB9XHJcbiAgICAgICAgfSwgY29udGV4dC5jb25maWcuZGVsYXkuaGlkZSk7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX2lzV2l0aEFjdGl2ZVRyaWdnZXInLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2lzV2l0aEFjdGl2ZVRyaWdnZXIoKSB7XHJcbiAgICAgICAgZm9yICh2YXIgdHJpZ2dlciBpbiB0aGlzLl9hY3RpdmVUcmlnZ2VyKSB7XHJcbiAgICAgICAgICBpZiAodGhpcy5fYWN0aXZlVHJpZ2dlclt0cmlnZ2VyXSkge1xyXG4gICAgICAgICAgICByZXR1cm4gdHJ1ZTtcclxuICAgICAgICAgIH1cclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdfZ2V0Q29uZmlnJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9nZXRDb25maWcoY29uZmlnKSB7XHJcbiAgICAgICAgY29uZmlnID0gJC5leHRlbmQoe30sIHRoaXMuY29uc3RydWN0b3IuRGVmYXVsdCwgJCh0aGlzLmVsZW1lbnQpLmRhdGEoKSwgY29uZmlnKTtcclxuXHJcbiAgICAgICAgaWYgKGNvbmZpZy5kZWxheSAmJiB0eXBlb2YgY29uZmlnLmRlbGF5ID09PSAnbnVtYmVyJykge1xyXG4gICAgICAgICAgY29uZmlnLmRlbGF5ID0ge1xyXG4gICAgICAgICAgICBzaG93OiBjb25maWcuZGVsYXksXHJcbiAgICAgICAgICAgIGhpZGU6IGNvbmZpZy5kZWxheVxyXG4gICAgICAgICAgfTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIFV0aWwudHlwZUNoZWNrQ29uZmlnKE5BTUUsIGNvbmZpZywgdGhpcy5jb25zdHJ1Y3Rvci5EZWZhdWx0VHlwZSk7XHJcblxyXG4gICAgICAgIHJldHVybiBjb25maWc7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnX2dldERlbGVnYXRlQ29uZmlnJyxcclxuICAgICAgdmFsdWU6IGZ1bmN0aW9uIF9nZXREZWxlZ2F0ZUNvbmZpZygpIHtcclxuICAgICAgICB2YXIgY29uZmlnID0ge307XHJcblxyXG4gICAgICAgIGlmICh0aGlzLmNvbmZpZykge1xyXG4gICAgICAgICAgZm9yICh2YXIga2V5IGluIHRoaXMuY29uZmlnKSB7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLmNvbnN0cnVjdG9yLkRlZmF1bHRba2V5XSAhPT0gdGhpcy5jb25maWdba2V5XSkge1xyXG4gICAgICAgICAgICAgIGNvbmZpZ1trZXldID0gdGhpcy5jb25maWdba2V5XTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgfVxyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcmV0dXJuIGNvbmZpZztcclxuICAgICAgfVxyXG5cclxuICAgICAgLy8gc3RhdGljXHJcblxyXG4gICAgfV0sIFt7XHJcbiAgICAgIGtleTogJ19qUXVlcnlJbnRlcmZhY2UnLFxyXG4gICAgICB2YWx1ZTogZnVuY3Rpb24gX2pRdWVyeUludGVyZmFjZShjb25maWcpIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5lYWNoKGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgIHZhciBkYXRhID0gJCh0aGlzKS5kYXRhKERBVEFfS0VZKTtcclxuICAgICAgICAgIHZhciBfY29uZmlnID0gdHlwZW9mIGNvbmZpZyA9PT0gJ29iamVjdCcgPyBjb25maWcgOiBudWxsO1xyXG5cclxuICAgICAgICAgIGlmICghZGF0YSAmJiAvZGVzdHJveXxoaWRlLy50ZXN0KGNvbmZpZykpIHtcclxuICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgfVxyXG5cclxuICAgICAgICAgIGlmICghZGF0YSkge1xyXG4gICAgICAgICAgICBkYXRhID0gbmV3IFRvb2x0aXAodGhpcywgX2NvbmZpZyk7XHJcbiAgICAgICAgICAgICQodGhpcykuZGF0YShEQVRBX0tFWSwgZGF0YSk7XHJcbiAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgaWYgKHR5cGVvZiBjb25maWcgPT09ICdzdHJpbmcnKSB7XHJcbiAgICAgICAgICAgIGlmIChkYXRhW2NvbmZpZ10gPT09IHVuZGVmaW5lZCkge1xyXG4gICAgICAgICAgICAgIHRocm93IG5ldyBFcnJvcignTm8gbWV0aG9kIG5hbWVkIFwiJyArIGNvbmZpZyArICdcIicpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGRhdGFbY29uZmlnXSgpO1xyXG4gICAgICAgICAgfVxyXG4gICAgICAgIH0pO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ1ZFUlNJT04nLFxyXG4gICAgICBnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcclxuICAgICAgICByZXR1cm4gVkVSU0lPTjtcclxuICAgICAgfVxyXG4gICAgfSwge1xyXG4gICAgICBrZXk6ICdEZWZhdWx0JyxcclxuICAgICAgZ2V0OiBmdW5jdGlvbiBnZXQoKSB7XHJcbiAgICAgICAgcmV0dXJuIERlZmF1bHQ7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnTkFNRScsXHJcbiAgICAgIGdldDogZnVuY3Rpb24gZ2V0KCkge1xyXG4gICAgICAgIHJldHVybiBOQU1FO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ0RBVEFfS0VZJyxcclxuICAgICAgZ2V0OiBmdW5jdGlvbiBnZXQoKSB7XHJcbiAgICAgICAgcmV0dXJuIERBVEFfS0VZO1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ0V2ZW50JyxcclxuICAgICAgZ2V0OiBmdW5jdGlvbiBnZXQoKSB7XHJcbiAgICAgICAgcmV0dXJuIEV2ZW50O1xyXG4gICAgICB9XHJcbiAgICB9LCB7XHJcbiAgICAgIGtleTogJ0VWRU5UX0tFWScsXHJcbiAgICAgIGdldDogZnVuY3Rpb24gZ2V0KCkge1xyXG4gICAgICAgIHJldHVybiBFVkVOVF9LRVk7XHJcbiAgICAgIH1cclxuICAgIH0sIHtcclxuICAgICAga2V5OiAnRGVmYXVsdFR5cGUnLFxyXG4gICAgICBnZXQ6IGZ1bmN0aW9uIGdldCgpIHtcclxuICAgICAgICByZXR1cm4gRGVmYXVsdFR5cGU7XHJcbiAgICAgIH1cclxuICAgIH1dKTtcclxuXHJcbiAgICByZXR1cm4gVG9vbHRpcDtcclxuICB9KSgpO1xyXG5cclxuICAkLmZuW05BTUVdID0gVG9vbHRpcC5falF1ZXJ5SW50ZXJmYWNlO1xyXG4gICQuZm5bTkFNRV0uQ29uc3RydWN0b3IgPSBUb29sdGlwO1xyXG4gICQuZm5bTkFNRV0ubm9Db25mbGljdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICQuZm5bTkFNRV0gPSBKUVVFUllfTk9fQ09ORkxJQ1Q7XHJcbiAgICByZXR1cm4gVG9vbHRpcC5falF1ZXJ5SW50ZXJmYWNlO1xyXG4gIH07XHJcblxyXG4gIHJldHVybiBUb29sdGlwO1xyXG59KShqUXVlcnkpO1xyXG4vLyMgc291cmNlTWFwcGluZ1VSTD10b29sdGlwLmpzLm1hcFxyXG4iLCIvKipcclxuICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICogQm9vdHN0cmFwICh2NC4wLjAtYWxwaGEuMik6IHV0aWwuanNcclxuICogTGljZW5zZWQgdW5kZXIgTUlUIChodHRwczovL2dpdGh1Yi5jb20vdHdicy9ib290c3RyYXAvYmxvYi9tYXN0ZXIvTElDRU5TRSlcclxuICogLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICovXHJcblxyXG4ndXNlIHN0cmljdCc7XHJcblxyXG52YXIgVXRpbCA9IChmdW5jdGlvbiAoJCkge1xyXG5cclxuICAvKipcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS1cclxuICAgKiBQcml2YXRlIFRyYW5zaXRpb25FbmQgSGVscGVyc1xyXG4gICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAqL1xyXG5cclxuICB2YXIgdHJhbnNpdGlvbiA9IGZhbHNlO1xyXG5cclxuICB2YXIgVHJhbnNpdGlvbkVuZEV2ZW50ID0ge1xyXG4gICAgV2Via2l0VHJhbnNpdGlvbjogJ3dlYmtpdFRyYW5zaXRpb25FbmQnLFxyXG4gICAgTW96VHJhbnNpdGlvbjogJ3RyYW5zaXRpb25lbmQnLFxyXG4gICAgT1RyYW5zaXRpb246ICdvVHJhbnNpdGlvbkVuZCBvdHJhbnNpdGlvbmVuZCcsXHJcbiAgICB0cmFuc2l0aW9uOiAndHJhbnNpdGlvbmVuZCdcclxuICB9O1xyXG5cclxuICAvLyBzaG91dG91dCBBbmd1c0Nyb2xsIChodHRwczovL2dvby5nbC9weHdRR3ApXHJcbiAgZnVuY3Rpb24gdG9UeXBlKG9iaikge1xyXG4gICAgcmV0dXJuICh7fSkudG9TdHJpbmcuY2FsbChvYmopLm1hdGNoKC9cXHMoW2EtekEtWl0rKS8pWzFdLnRvTG93ZXJDYXNlKCk7XHJcbiAgfVxyXG5cclxuICBmdW5jdGlvbiBpc0VsZW1lbnQob2JqKSB7XHJcbiAgICByZXR1cm4gKG9ialswXSB8fCBvYmopLm5vZGVUeXBlO1xyXG4gIH1cclxuXHJcbiAgZnVuY3Rpb24gZ2V0U3BlY2lhbFRyYW5zaXRpb25FbmRFdmVudCgpIHtcclxuICAgIHJldHVybiB7XHJcbiAgICAgIGJpbmRUeXBlOiB0cmFuc2l0aW9uLmVuZCxcclxuICAgICAgZGVsZWdhdGVUeXBlOiB0cmFuc2l0aW9uLmVuZCxcclxuICAgICAgaGFuZGxlOiBmdW5jdGlvbiBoYW5kbGUoZXZlbnQpIHtcclxuICAgICAgICBpZiAoJChldmVudC50YXJnZXQpLmlzKHRoaXMpKSB7XHJcbiAgICAgICAgICByZXR1cm4gZXZlbnQuaGFuZGxlT2JqLmhhbmRsZXIuYXBwbHkodGhpcywgYXJndW1lbnRzKTtcclxuICAgICAgICB9XHJcbiAgICAgIH1cclxuICAgIH07XHJcbiAgfVxyXG5cclxuICBmdW5jdGlvbiB0cmFuc2l0aW9uRW5kVGVzdCgpIHtcclxuICAgIGlmICh3aW5kb3cuUVVuaXQpIHtcclxuICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgfVxyXG5cclxuICAgIHZhciBlbCA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoJ2Jvb3RzdHJhcCcpO1xyXG5cclxuICAgIGZvciAodmFyIF9uYW1lIGluIFRyYW5zaXRpb25FbmRFdmVudCkge1xyXG4gICAgICBpZiAoZWwuc3R5bGVbX25hbWVdICE9PSB1bmRlZmluZWQpIHtcclxuICAgICAgICByZXR1cm4geyBlbmQ6IFRyYW5zaXRpb25FbmRFdmVudFtfbmFtZV0gfTtcclxuICAgICAgfVxyXG4gICAgfVxyXG5cclxuICAgIHJldHVybiBmYWxzZTtcclxuICB9XHJcblxyXG4gIGZ1bmN0aW9uIHRyYW5zaXRpb25FbmRFbXVsYXRvcihkdXJhdGlvbikge1xyXG4gICAgdmFyIF90aGlzID0gdGhpcztcclxuXHJcbiAgICB2YXIgY2FsbGVkID0gZmFsc2U7XHJcblxyXG4gICAgJCh0aGlzKS5vbmUoVXRpbC5UUkFOU0lUSU9OX0VORCwgZnVuY3Rpb24gKCkge1xyXG4gICAgICBjYWxsZWQgPSB0cnVlO1xyXG4gICAgfSk7XHJcblxyXG4gICAgc2V0VGltZW91dChmdW5jdGlvbiAoKSB7XHJcbiAgICAgIGlmICghY2FsbGVkKSB7XHJcbiAgICAgICAgVXRpbC50cmlnZ2VyVHJhbnNpdGlvbkVuZChfdGhpcyk7XHJcbiAgICAgIH1cclxuICAgIH0sIGR1cmF0aW9uKTtcclxuXHJcbiAgICByZXR1cm4gdGhpcztcclxuICB9XHJcblxyXG4gIGZ1bmN0aW9uIHNldFRyYW5zaXRpb25FbmRTdXBwb3J0KCkge1xyXG4gICAgdHJhbnNpdGlvbiA9IHRyYW5zaXRpb25FbmRUZXN0KCk7XHJcblxyXG4gICAgJC5mbi5lbXVsYXRlVHJhbnNpdGlvbkVuZCA9IHRyYW5zaXRpb25FbmRFbXVsYXRvcjtcclxuXHJcbiAgICBpZiAoVXRpbC5zdXBwb3J0c1RyYW5zaXRpb25FbmQoKSkge1xyXG4gICAgICAkLmV2ZW50LnNwZWNpYWxbVXRpbC5UUkFOU0lUSU9OX0VORF0gPSBnZXRTcGVjaWFsVHJhbnNpdGlvbkVuZEV2ZW50KCk7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAvKipcclxuICAgKiAtLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLVxyXG4gICAqIFB1YmxpYyBVdGlsIEFwaVxyXG4gICAqIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXHJcbiAgICovXHJcblxyXG4gIHZhciBVdGlsID0ge1xyXG5cclxuICAgIFRSQU5TSVRJT05fRU5EOiAnYnNUcmFuc2l0aW9uRW5kJyxcclxuXHJcbiAgICBnZXRVSUQ6IGZ1bmN0aW9uIGdldFVJRChwcmVmaXgpIHtcclxuICAgICAgZG8ge1xyXG4gICAgICAgIHByZWZpeCArPSB+IH4oTWF0aC5yYW5kb20oKSAqIDEwMDAwMDApOyAvLyBcIn5+XCIgYWN0cyBsaWtlIGEgZmFzdGVyIE1hdGguZmxvb3IoKSBoZXJlXHJcbiAgICAgIH0gd2hpbGUgKGRvY3VtZW50LmdldEVsZW1lbnRCeUlkKHByZWZpeCkpO1xyXG4gICAgICByZXR1cm4gcHJlZml4O1xyXG4gICAgfSxcclxuXHJcbiAgICBnZXRTZWxlY3RvckZyb21FbGVtZW50OiBmdW5jdGlvbiBnZXRTZWxlY3RvckZyb21FbGVtZW50KGVsZW1lbnQpIHtcclxuICAgICAgdmFyIHNlbGVjdG9yID0gZWxlbWVudC5nZXRBdHRyaWJ1dGUoJ2RhdGEtdGFyZ2V0Jyk7XHJcblxyXG4gICAgICBpZiAoIXNlbGVjdG9yKSB7XHJcbiAgICAgICAgc2VsZWN0b3IgPSBlbGVtZW50LmdldEF0dHJpYnV0ZSgnaHJlZicpIHx8ICcnO1xyXG4gICAgICAgIHNlbGVjdG9yID0gL14jW2Etel0vaS50ZXN0KHNlbGVjdG9yKSA/IHNlbGVjdG9yIDogbnVsbDtcclxuICAgICAgfVxyXG5cclxuICAgICAgcmV0dXJuIHNlbGVjdG9yO1xyXG4gICAgfSxcclxuXHJcbiAgICByZWZsb3c6IGZ1bmN0aW9uIHJlZmxvdyhlbGVtZW50KSB7XHJcbiAgICAgIG5ldyBGdW5jdGlvbignYnMnLCAncmV0dXJuIGJzJykoZWxlbWVudC5vZmZzZXRIZWlnaHQpO1xyXG4gICAgfSxcclxuXHJcbiAgICB0cmlnZ2VyVHJhbnNpdGlvbkVuZDogZnVuY3Rpb24gdHJpZ2dlclRyYW5zaXRpb25FbmQoZWxlbWVudCkge1xyXG4gICAgICAkKGVsZW1lbnQpLnRyaWdnZXIodHJhbnNpdGlvbi5lbmQpO1xyXG4gICAgfSxcclxuXHJcbiAgICBzdXBwb3J0c1RyYW5zaXRpb25FbmQ6IGZ1bmN0aW9uIHN1cHBvcnRzVHJhbnNpdGlvbkVuZCgpIHtcclxuICAgICAgcmV0dXJuIEJvb2xlYW4odHJhbnNpdGlvbik7XHJcbiAgICB9LFxyXG5cclxuICAgIHR5cGVDaGVja0NvbmZpZzogZnVuY3Rpb24gdHlwZUNoZWNrQ29uZmlnKGNvbXBvbmVudE5hbWUsIGNvbmZpZywgY29uZmlnVHlwZXMpIHtcclxuICAgICAgZm9yICh2YXIgcHJvcGVydHkgaW4gY29uZmlnVHlwZXMpIHtcclxuICAgICAgICBpZiAoY29uZmlnVHlwZXMuaGFzT3duUHJvcGVydHkocHJvcGVydHkpKSB7XHJcbiAgICAgICAgICB2YXIgZXhwZWN0ZWRUeXBlcyA9IGNvbmZpZ1R5cGVzW3Byb3BlcnR5XTtcclxuICAgICAgICAgIHZhciB2YWx1ZSA9IGNvbmZpZ1twcm9wZXJ0eV07XHJcbiAgICAgICAgICB2YXIgdmFsdWVUeXBlID0gdW5kZWZpbmVkO1xyXG5cclxuICAgICAgICAgIGlmICh2YWx1ZSAmJiBpc0VsZW1lbnQodmFsdWUpKSB7XHJcbiAgICAgICAgICAgIHZhbHVlVHlwZSA9ICdlbGVtZW50JztcclxuICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgIHZhbHVlVHlwZSA9IHRvVHlwZSh2YWx1ZSk7XHJcbiAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgaWYgKCFuZXcgUmVnRXhwKGV4cGVjdGVkVHlwZXMpLnRlc3QodmFsdWVUeXBlKSkge1xyXG4gICAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoY29tcG9uZW50TmFtZS50b1VwcGVyQ2FzZSgpICsgJzogJyArICgnT3B0aW9uIFwiJyArIHByb3BlcnR5ICsgJ1wiIHByb3ZpZGVkIHR5cGUgXCInICsgdmFsdWVUeXBlICsgJ1wiICcpICsgKCdidXQgZXhwZWN0ZWQgdHlwZSBcIicgKyBleHBlY3RlZFR5cGVzICsgJ1wiLicpKTtcclxuICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICAgIH1cclxuICAgIH1cclxuICB9O1xyXG5cclxuICBzZXRUcmFuc2l0aW9uRW5kU3VwcG9ydCgpO1xyXG5cclxuICByZXR1cm4gVXRpbDtcclxufSkoalF1ZXJ5KTtcclxuLy8jIHNvdXJjZU1hcHBpbmdVUkw9dXRpbC5qcy5tYXBcclxuIiwiJChmdW5jdGlvbiAoKSB7XHJcbiAgICAkKFwiYm9keVwiKS5vbihcImNsaWNrXCIsIFwiW2l0ZW1wcm9wfj0nUmVtb3ZlVXJsJ11cIiwgZnVuY3Rpb24gKCkge1xyXG4gICAgICAgIC8vIGRvbid0IHNob3cgdGhlIGNvbmZpcm0gZGlhbG9nIGlmIHRoZSBsaW5rIGlzIGFsc28gVW5zYWZlVXJsLCBhcyBpdCB3aWxsIGFscmVhZHkgYmUgaGFuZGxlZCBpbiBiYXNlLmpzXHJcbiAgICAgICAgaWYgKCQodGhpcykuZmlsdGVyKFwiW2l0ZW1wcm9wfj0nVW5zYWZlVXJsJ11cIikubGVuZ3RoID09IDEpIHtcclxuICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgLy8gdXNlIGEgY3VzdG9tIG1lc3NhZ2UgaWYgaXRzIHNldCBpbiBkYXRhLW1lc3NhZ2VcclxuICAgICAgICB2YXIgZGF0YU1lc3NhZ2UgPSAkKHRoaXMpLmRhdGEoJ21lc3NhZ2UnKTtcclxuICAgICAgICBpZiAoZGF0YU1lc3NhZ2UgPT09IHVuZGVmaW5lZCkge1xyXG4gICAgICAgICAgICBkYXRhTWVzc2FnZSA9IGNvbmZpcm1SZW1vdmVNZXNzYWdlO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcmV0dXJuIGNvbmZpcm0oZGF0YU1lc3NhZ2UpO1xyXG4gICAgfSk7XHJcbn0pKGpRdWVyeSk7XHJcblxyXG4kKGZ1bmN0aW9uICgpIHtcclxuICAgIHZhciBtYWdpY1Rva2VuID0gJChcImlucHV0W25hbWU9X19SZXF1ZXN0VmVyaWZpY2F0aW9uVG9rZW5dXCIpLmZpcnN0KCk7XHJcbiAgICBpZiAobWFnaWNUb2tlbikge1xyXG4gICAgICAgICQoXCJib2R5XCIpLm9uKFwiY2xpY2tcIiwgXCJhW2l0ZW1wcm9wfj1VbnNhZmVVcmxdLCBhW2RhdGEtdW5zYWZlLXVybF1cIiwgZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICB2YXIgX3RoaXMgPSAkKHRoaXMpO1xyXG4gICAgICAgICAgICB2YXIgaHJlZlBhcnRzID0gX3RoaXMuYXR0cihcImhyZWZcIikuc3BsaXQoXCI/XCIpO1xyXG4gICAgICAgICAgICB2YXIgZm9ybSA9ICQoXCI8Zm9ybSBhY3Rpb249XFxcIlwiICsgaHJlZlBhcnRzWzBdICsgXCJcXFwiIG1ldGhvZD1cXFwiUE9TVFxcXCIgLz5cIik7XHJcbiAgICAgICAgICAgIGZvcm0uYXBwZW5kKG1hZ2ljVG9rZW4uY2xvbmUoKSk7XHJcbiAgICAgICAgICAgIGlmIChocmVmUGFydHMubGVuZ3RoID4gMSkge1xyXG4gICAgICAgICAgICAgICAgdmFyIHF1ZXJ5UGFydHMgPSBocmVmUGFydHNbMV0uc3BsaXQoXCImXCIpO1xyXG4gICAgICAgICAgICAgICAgZm9yICh2YXIgaSA9IDA7IGkgPCBxdWVyeVBhcnRzLmxlbmd0aDsgaSsrKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIHF1ZXJ5UGFydEtWUCA9IHF1ZXJ5UGFydHNbaV0uc3BsaXQoXCI9XCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIC8vdHJ1c3RpbmcgaHJlZnMgaW4gdGhlIHBhZ2UgaGVyZVxyXG4gICAgICAgICAgICAgICAgICAgIGZvcm0uYXBwZW5kKCQoXCI8aW5wdXQgdHlwZT1cXFwiaGlkZGVuXFxcIiBuYW1lPVxcXCJcIiArIGRlY29kZVVSSUNvbXBvbmVudChxdWVyeVBhcnRLVlBbMF0pICsgXCJcXFwiIHZhbHVlPVxcXCJcIiArIGRlY29kZVVSSUNvbXBvbmVudChxdWVyeVBhcnRLVlBbMV0pICsgXCJcXFwiIC8+XCIpKTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBmb3JtLmNzcyh7IFwicG9zaXRpb25cIjogXCJhYnNvbHV0ZVwiLCBcImxlZnRcIjogXCItOTk5OWVtXCIgfSk7XHJcbiAgICAgICAgICAgICQoXCJib2R5XCIpLmFwcGVuZChmb3JtKTtcclxuXHJcbiAgICAgICAgICAgIHZhciB1bnNhZmVVcmxQcm9tcHQgPSBfdGhpcy5kYXRhKFwidW5zYWZlLXVybFwiKTtcclxuXHJcbiAgICAgICAgICAgIGlmICh1bnNhZmVVcmxQcm9tcHQgJiYgdW5zYWZlVXJsUHJvbXB0Lmxlbmd0aCA+IDApIHtcclxuICAgICAgICAgICAgICAgIGlmICghY29uZmlybSh1bnNhZmVVcmxQcm9tcHQpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICBpZiAoX3RoaXMuZmlsdGVyKFwiW2l0ZW1wcm9wfj0nUmVtb3ZlVXJsJ11cIikubGVuZ3RoID09IDEpIHtcclxuICAgICAgICAgICAgICAgIC8vIHVzZSBhIGN1c3RvbSBtZXNzYWdlIGlmIGl0cyBzZXQgaW4gZGF0YS1tZXNzYWdlXHJcbiAgICAgICAgICAgICAgICB2YXIgZGF0YU1lc3NhZ2UgPSBfdGhpcy5kYXRhKCdtZXNzYWdlJyk7XHJcbiAgICAgICAgICAgICAgICBpZiAoZGF0YU1lc3NhZ2UgPT09IHVuZGVmaW5lZCkge1xyXG4gICAgICAgICAgICAgICAgICAgIGRhdGFNZXNzYWdlID0gY29uZmlybVJlbW92ZU1lc3NhZ2U7XHJcbiAgICAgICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAgICAgaWYgKCFjb25maXJtKGRhdGFNZXNzYWdlKSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgZm9ybS5zdWJtaXQoKTtcclxuICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG5cclxuICAgICAgICB9KTtcclxuICAgIH1cclxufSkoalF1ZXJ5KTsiXSwic291cmNlUm9vdCI6Ii9zb3VyY2UvIn0=
