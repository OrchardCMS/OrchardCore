var _this2 = this;

/*-----------------------------------------------
|   Utilities
-----------------------------------------------*/
var spUtils = function ($) {
  var Utils = {
    $window: $(window),
    $document: $(document),
    $html: $('html'),
    $body: $('body'),
    $main: $('main'),
    isRTL: function isRTL() {
      return this.$html.attr('dir') === 'rtl';
    },
    location: window.location,
    nua: navigator.userAgent,
    breakpoints: {
      xs: 0,
      sm: 576,
      md: 768,
      lg: 992,
      xl: 1200
    },
    offset: function offset(element) {
      var rect = element.getBoundingClientRect();
      var scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;
      var scrollTop = window.pageYOffset || document.documentElement.scrollTop;
      return {
        top: rect.top + scrollTop,
        left: rect.left + scrollLeft
      };
    },
    isScrolledIntoViewJS: function isScrolledIntoViewJS(element) {
      var windowHeight = window.innerHeight;
      var elemTop = this.offset(element).top;
      var elemHeight = element.offsetHeight;
      var windowScrollTop = window.scrollY;
      return elemTop <= windowScrollTop + windowHeight && windowScrollTop <= elemTop + elemHeight;
    },
    isScrolledIntoView: function isScrolledIntoView(el) {
      var $el = $(el);
      var windowHeight = this.$window.height();
      var elemTop = $el.offset().top;
      var elemHeight = $el.height();
      var windowScrollTop = this.$window.scrollTop();
      return elemTop <= windowScrollTop + windowHeight && windowScrollTop <= elemTop + elemHeight;
    },
    getCurrentScreanBreakpoint: function getCurrentScreanBreakpoint() {
      var _this = this;

      var currentScrean = '';
      var windowWidth = this.$window.width();
      $.each(this.breakpoints, function (index, value) {
        if (windowWidth >= value) {
          currentScrean = index;
        } else if (windowWidth >= _this.breakpoints.xl) {
          currentScrean = 'xl';
        }
      });
      return {
        currentScrean: currentScrean,
        currentBreakpoint: this.breakpoints[currentScrean]
      };
    }
  };
  return Utils;
}(jQuery);
/*-----------------------------------------------
|  Hover dir for Gallery
-----------------------------------------------*/


spUtils.$document.ready(function () {
  var toTopBtn = $('.btn-back-to-top');
  spUtils.$window.scroll(function () {
    var scrollTop = spUtils.$window.scrollTop();

    if (scrollTop > 100) {
      toTopBtn.fadeIn();
    } else {
      toTopBtn.fadeOut();
    }
  });
});
/*-----------------------------------------------
|   Top navigation opacity on scroll
-----------------------------------------------*/

spUtils.$document.ready(function () {
  var $navbar = $('.navbar-theme');
  var $headerOverlay = $('.header-overlay');
  var $headerContent = $('.header-text, .header-indicator-down');

  if ($navbar.length) {
    var windowHeight = spUtils.$window.height();
    spUtils.$window.scroll(function () {
      var scrollTop = spUtils.$window.scrollTop();
      var alpha = scrollTop / windowHeight * 2;
      var beta = scrollTop / windowHeight;
      alpha >= 1 && (alpha = 1);
      $navbar.css({
        'background-color': "rgba(0, 0, 0, " + alpha + ")"
      }); // Reduce header content opacity on scroll

      alpha >= 1 && (alpha = 1);
      $headerOverlay.css({
        'background-color': "rgba(0, 0, 0, " + beta + ")"
      });
      $headerContent.css({
        opacity: 1 - beta
      }); // Reduce border bottom opacity on scroll

      $navbar.css({
        'border-color': "rgba(255, 255, 255, " + (0.15 - alpha) + ")"
      });
    }); // Fix navbar background color [after and before expand]

    var classList = $navbar.attr('class').split(' ');
    var breakpoint = classList.filter(function (c) {
      return c.indexOf('navbar-expand-') >= 0;
    })[0].split('navbar-expand-')[1];
    spUtils.$window.resize(function () {
      if (spUtils.$window.width() > spUtils.breakpoints[breakpoint]) {
        return $navbar.removeClass('bg-dark');
      }

      if (!$navbar.find('.navbar-toggler').hasClass('collapsed')) {
        return $navbar.addClass('bg-dark');
      }

      return null;
    }); // Top navigation background toggle on mobile

    $navbar.on('show.bs.collapse hide.bs.collapse', function (e) {
      $(e.currentTarget).toggleClass('bg-dark');
    });
  }
});
/*-----------------------------------------------
|   Select menu [bootstrap 4]
-----------------------------------------------*/

spUtils.$document.ready(function () {
  // https://getbootstrap.com/docs/4.0/getting-started/browsers-devices/#select-menu
  // https://github.com/twbs/bootstrap/issues/26183
  spDetector.isAndroid && $('select.form-control').removeClass('form-control').css('width', '100%');
});
/*-----------------------------------------------
|   Count Up
-----------------------------------------------*/

spUtils.$document.ready(function () {
  var $counters = $('[data-countup]');

  if ($counters.length) {
    $counters.each(function (index, value) {
      var $counter = $(value);
      var counter = $counter.data('countup');

      var toAlphanumeric = function toAlphanumeric(num) {
        var number = num;
        var abbreviations = {
          k: 1000,
          m: 1000000,
          b: 1000000000,
          t: 1000000000000
        };

        if (num < abbreviations.m) {
          number = (num / abbreviations.k).toFixed(2) + "k";
        } else if (num < abbreviations.b) {
          number = (num / abbreviations.m).toFixed(2) + "m";
        } else if (num < abbreviations.t) {
          number = (num / abbreviations.b).toFixed(2) + "b";
        } else {
          number = (num / abbreviations.t).toFixed(2) + "t";
        }

        return number;
      };

      var toComma = function toComma(num) {
        return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
      };

      var toSpace = function toSpace(num) {
        return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
      };

      var playCountUpTriggered = false;

      var countUP = function countUP() {
        if (spUtils.isScrolledIntoView(value) && !playCountUpTriggered) {
          if (!playCountUpTriggered) {
            $({
              countNum: 0
            }).animate({
              countNum: counter.count
            }, {
              duration: counter.duration || 1000,
              easing: 'linear',
              step: function step() {
                $counter.text((counter.prefix ? counter.prefix : '') + Math.floor(this.countNum));
              },
              complete: function complete() {
                switch (counter.format) {
                  case 'comma':
                    $counter.text((counter.prefix ? counter.prefix : '') + toComma(this.countNum));
                    break;

                  case 'space':
                    $counter.text((counter.prefix ? counter.prefix : '') + toSpace(this.countNum));
                    break;

                  case 'alphanumeric':
                    $counter.text((counter.prefix ? counter.prefix : '') + toAlphanumeric(this.countNum));
                    break;

                  default:
                    $counter.text((counter.prefix ? counter.prefix : '') + this.countNum);
                }
              }
            });
            playCountUpTriggered = true;
          }
        }

        return playCountUpTriggered;
      };

      countUP();
      spUtils.$window.scroll(function () {
        countUP();
      });
    });
  }
});
/*-----------------------------------------------
|   Countdown
-----------------------------------------------*/

spUtils.$document.ready(function () {
  var $dataCountdowns = $('[data-countdown]');
  var DATA_KEY = {
    FALLBACK: 'countdown-fallback',
    COUNTDOWN: 'countdown'
  };

  if ($dataCountdowns.length) {
    $dataCountdowns.each(function (index, value) {
      var $dateCountdown = $(value);
      var date = $dateCountdown.data(DATA_KEY.COUNTDOWN);
      var fallback;

      if (typeof $dateCountdown.data(DATA_KEY.FALLBACK) !== typeof undefined) {
        fallback = $dateCountdown.data(DATA_KEY.FALLBACK);
      }

      $dateCountdown.countdown(date, function (event) {
        if (event.elapsed) {
          $dateCountdown.text(fallback);
        } else {
          $dateCountdown.text(event.strftime('%D days %H:%M:%S'));
        }
      });
    });
  }
});
/*-----------------------------------------------
|   Variables
-----------------------------------------------*/

/*
  global opr, safari
*/

/*-----------------------------------------------
|   Detector
-----------------------------------------------*/

var spDetector = function () {
  var Detector = {
    isMobile: /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(spUtils.nua),
    isOSX: spUtils.nua.match(/(iPad|iPhone|iPod|Macintosh)/g),
    isOpera: !!window.opr && !!opr.addons || !!window.opera || navigator.userAgent.indexOf(' OPR/') >= 0,
    isFirefox: typeof InstallTrigger !== 'undefined',
    isSafari: /constructor/i.test(window.HTMLElement) || function (p) {
      return p.toString() === '[object SafariRemoteNotification]';
    }(!window.safari || safari.pushNotification),
    isNewerIE: spUtils.nua.match(/msie (9|([1-9][0-9]))/i),
    isOlderIE: spUtils.nua.match(/msie/i) && !_this2.isNewerIE,
    isAncientIE: spUtils.nua.match(/msie 6/i),
    isIE: _this2.isAncientIE || _this2.isOlderIE || _this2.isNewerIE,
    isIE11: !!window.MSInputMethodContext && !!document.documentMode,
    isEdge: !_this2.isIE11 && !_this2.isIE && !!window.StyleMedia,
    isChrome: !!window.chrome && !!window.chrome.webstore,
    isBlink: (_this2.isChrome || _this2.isOpera) && !!window.CSS,
    isPuppeteer: spUtils.nua.match(/puppeteer/i),
    isIOS: parseFloat((/CPU.*OS ([0-9_]{1,5})|(CPU like).*AppleWebKit.*Mobile/i.exec(spUtils.nua) || [0, ''])[1].replace('undefined', '3_2').replace('_', '.').replace('_', '')) || false,
    iPadiPhoneFirefox: spUtils.nua.match(/iPod|iPad|iPhone/g) && spUtils.nua.match(/Gecko/g),
    macFirefox: spUtils.nua.match(/Macintosh/g) && spUtils.nua.match(/Gecko/g) && spUtils.nua.match(/rv:/g),
    isAndroid: spUtils.nua.indexOf('Mozilla/5.0') > -1 && spUtils.nua.indexOf('Android ') > -1 && spUtils.nua.indexOf('AppleWebKit') > -1
  };
  spUtils.$document.ready(function () {
    if (Detector.isOpera) spUtils.$html.addClass('opera');
    if (Detector.isMobile) spUtils.$html.addClass('mobile');
    if (Detector.isOSX) spUtils.$html.addClass('osx');
    if (Detector.isFirefox) spUtils.$html.addClass('firefox');
    if (Detector.isSafari) spUtils.$html.addClass('safari');
    if (Detector.isIOS) spUtils.$html.addClass('ios');
    if (Detector.isIE || Detector.isIE11) spUtils.$html.addClass('ie');
    if (Detector.isEdge) spUtils.$html.addClass('edge');
    if (Detector.isChrome) spUtils.$html.addClass('chrome');
    if (Detector.isBlink) spUtils.$html.addClass('blink');
    if (Detector.isPuppeteer) spUtils.$html.addClass('puppeteer');
  });
  return Detector;
}();
/*-----------------------------------------------
|   Documentation and Component Navigation
-----------------------------------------------*/


spUtils.$document.ready(function () {
  var $componentNav = $('#components-nav');

  if ($componentNav.length) {
    var loc = window.location.href;

    var _loc$split = loc.split('#');

    loc = _loc$split[0];
    var location = loc.split('/');
    var url = location[location.length - 2] + "/" + location.pop();
    var urls = $componentNav.children('li').children('a');

    for (var i = 0, max = urls.length; i < max; i += 1) {
      var dom = urls[i].href.split('/');
      var domURL = dom[dom.length - 2] + "/" + dom.pop();

      if (domURL === url) {
        var $targetedElement = $(urls[i]);
        $targetedElement.removeClass('text-600');
        $targetedElement.addClass('text-black');
        break;
      }
    }
  }
});
/*-----------------------------------------------
|   Draw SVG
-----------------------------------------------*/

/*
  global TimelineMax, TweenMax
*/

(function ($) {
  /*-----------------------------------------------
  |   SVG animation
  -----------------------------------------------*/
  var zanimSVG = $('[data-zanim-svg]');

  if (zanimSVG.length && !spDetector.isPuppeteer) {
    zanimSVG.each(function (index, value) {
      var $this = $(value);
      var path = $this.find('path');
      var controller = $this.data('zanim-svg');
      controller.delay || (controller.delay = 0);
      controller.duration || (controller.duration = 2);
      controller.ease || (controller.ease = 'Expo.easeOut');
      var tl = new TimelineMax();
      var DrawSvgInit = tl.from(path, controller.duration, {
        drawSVG: 0,
        delay: controller.delay,
        ease: controller.ease
      }).pause();

      function svgTrigger() {
        if (spUtils.isScrolledIntoView($this) && controller.trigger === 'scroll') {
          DrawSvgInit.play();
          TweenMax.set(path, {
            visibility: 'visible'
          });
          controller.trigger = false;
        }
      }

      spUtils.$document.ready(function () {
        svgTrigger();
        spUtils.$window.on('scroll', function () {
          svgTrigger();
        });
      });
    });
  }
})(jQuery);
/*-----------------------------------------------
|   Documentation and Component Navigation
-----------------------------------------------*/


spUtils.$document.ready(function () {
  var $navbarDropdown = $('.navbar-theme .dropdown');

  if (!spDetector.isMobile) {
    $navbarDropdown.addClass('dropdown-on-hover');
  } else {
    $navbarDropdown.removeClass('dropdown-on-hover');
  }

  var toggleDropdown = function toggleDropdown(e) {
    var $el = $(e.target);
    var dropdown = $el.closest('.dropdown-on-hover');
    var dropdownMenu = $('.dropdown-menu', dropdown);
    setTimeout(function () {
      var shouldOpen = e.type !== 'click' && dropdown.is(':hover');
      dropdownMenu.toggleClass('show', shouldOpen);
      dropdown.toggleClass('show', shouldOpen);
      $('[data-toggle="dropdown"]', dropdown).attr('aria-expanded', shouldOpen);
    }, e.type === 'mouseleave' ? 100 : 0);
  };

  $('body').on('mouseenter mouseleave', '.dropdown-on-hover', toggleDropdown).on('click', '.dropdown-menu a', toggleDropdown);
}); // import { on } from 'cluster';

/*-----------------------------------------------
|   On page scroll for #id targets
-----------------------------------------------*/

spUtils.$document.ready(function ($) {
  $(document).on('click', 'a[data-fancyscroll]', function scrollTo(e) {
    // const $this = $(e.currentTarget);
    var $this = $(this);

    if (spUtils.location.pathname === $this[0].pathname && spUtils.location.pathname.replace(/^\//, '') === this.pathname.replace(/^\//, '') && spUtils.location.hostname === this.hostname) {
      e.preventDefault();
      var target = $(this.hash);
      target = target.length ? target : $("[name=" + this.hash.slice(1) + "]");

      if (target.length) {
        $('html,body').animate({
          scrollTop: target.offset().top - ($this.data('offset') || 0)
        }, 500, 'swing', function () {
          var hash = $this.attr('href');
          window.history.pushState ? window.history.pushState(null, null, hash) : window.location.hash = hash;
        });
        return false;
      }
    }

    return true;
  });
  var hash = window.location.hash;

  if (document.getElementById(hash.slice(1))) {
    var $this = $(hash);
    $('html,body').animate({
      scrollTop: $this.offset().top - $("a[href='" + hash + "']").data('offset')
    }, 800, 'swing', function () {
      window.history.pushState ? window.history.pushState(null, null, hash) : window.location.hash = hash;
    });
  }
});
/*-----------------------------------------------
|  Flex slider
-----------------------------------------------*/

spUtils.$document.ready(function () {
  var flexslider = $('.flexslider');

  if (flexslider.length) {
    var flexSliderZanim = function flexSliderZanim(slider, target) {
      if ($(slider).find('*[data-zanim-timeline]').length === 0) return;
      $(slider).find('*[data-zanim-timeline]').zanimation(function (animation) {
        return animation.kill();
      });
      $(target).zanimation(function (animation) {
        return animation.play();
      });
    };

    flexslider.each(function (item, value) {
      var $this = $(value);
      $this.flexslider($.extend($this.data('options') || {
        prevText: '<span class="indicator-arrow indicator-arrow-left"></span>',
        nextText: '<span class="indicator-arrow indicator-arrow-right"></span>'
      }, {
        start: function start(slider) {
          flexSliderZanim(slider, slider.find('*[data-zanim-timeline].flex-active-slide'));
        },
        before: function before(slider) {
          flexSliderZanim(slider, slider.find("ul.slides > li:nth-child(" + (slider.getTarget(slider.direction) + 1) + ")")[0]);
        }
      }));
    });
  }
}); // //////////////////////////////////////
//
// Universal contact form ajax submission
//
// //////////////////////////////////////

$(document).ready(function () {
  if ($('.zform').length) {
    var submitButtonValue = {
      set: function set($elm, value) {
        if ($elm.prop('tagName') === 'BUTTON') {
          $elm.html(value);
          return;
        }

        $elm.val(value);
      },
      get: function get($elm) {
        var value = $elm.val();

        if (value === '') {
          return $elm.html();
        }

        return value;
      }
    };
    $('.zform').each(function (index, value) {
      var $form = $(value);
      $form.on('submit', function (e) {
        e.preventDefault();

        if ($('#g-recaptcha-response').val() === '') {
          $form.find('.zform-feedback').html('<div class="alert alert-danger alert-dismissible fade show" role="alert"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>Please, verify you are a human!</div>');
          return;
        }

        var $submit = $form.find(':submit');
        var submitText = submitButtonValue.get($submit);
        submitButtonValue.set($submit, 'Sending...');
        $.ajax({
          type: 'post',
          url: '/assets/php/form-processor.php',
          data: $form.serialize() // again, keep generic so this applies to any form

        }).done(function (result) {
          // if(result.status ==)
          $form.find('.zform-feedback').html(result);
          submitButtonValue.set($submit, submitText);
          window.grecaptcha.reset();
          $form.trigger('reset');
        }).fail(function (xhr) {
          $form.find('.zform-feedback').html(xhr.responseText);
          submitButtonValue.set($submit, submitText);
        });
      });
    });
  }
});
/*-----------------------------------------------
|   Bootstrap validation
-----------------------------------------------*/

(function () {
  window.addEventListener('load', function () {
    // Fetch all the forms we want to apply reign Bootstrap validation styles to
    var forms = document.getElementsByClassName('needs-validation'); // Loop over them and prevent submission

    Array.prototype.filter.call(forms, function (form) {
      form.addEventListener('submit', function (event) {
        if (form.checkValidity() === false) {
          event.preventDefault();
          event.stopPropagation();
        }

        form.classList.add('was-validated');
      }, false);
    });
  }, false);
})();
/*-----------------------------------------------
|   Gooogle Map
-----------------------------------------------*/

/*
  global google
*/


function initMap() {
  var $googlemaps = $('.googlemap');

  if ($googlemaps.length) {
    // Visit https://snazzymaps.com/ for more themes
    var mapStyles = {
      Default: [{
        featureType: 'water',
        elementType: 'geometry',
        stylers: [{
          color: '#e9e9e9'
        }, {
          lightness: 17
        }]
      }, {
        featureType: 'landscape',
        elementType: 'geometry',
        stylers: [{
          color: '#f5f5f5'
        }, {
          lightness: 20
        }]
      }, {
        featureType: 'road.highway',
        elementType: 'geometry.fill',
        stylers: [{
          color: '#ffffff'
        }, {
          lightness: 17
        }]
      }, {
        featureType: 'road.highway',
        elementType: 'geometry.stroke',
        stylers: [{
          color: '#ffffff'
        }, {
          lightness: 29
        }, {
          weight: 0.2
        }]
      }, {
        featureType: 'road.arterial',
        elementType: 'geometry',
        stylers: [{
          color: '#ffffff'
        }, {
          lightness: 18
        }]
      }, {
        featureType: 'road.local',
        elementType: 'geometry',
        stylers: [{
          color: '#ffffff'
        }, {
          lightness: 16
        }]
      }, {
        featureType: 'poi',
        elementType: 'geometry',
        stylers: [{
          color: '#f5f5f5'
        }, {
          lightness: 21
        }]
      }, {
        featureType: 'poi.park',
        elementType: 'geometry',
        stylers: [{
          color: '#dedede'
        }, {
          lightness: 21
        }]
      }, {
        elementType: 'labels.text.stroke',
        stylers: [{
          visibility: 'on'
        }, {
          color: '#ffffff'
        }, {
          lightness: 16
        }]
      }, {
        elementType: 'labels.text.fill',
        stylers: [{
          saturation: 36
        }, {
          color: '#333333'
        }, {
          lightness: 40
        }]
      }, {
        elementType: 'labels.icon',
        stylers: [{
          visibility: 'off'
        }]
      }, {
        featureType: 'transit',
        elementType: 'geometry',
        stylers: [{
          color: '#f2f2f2'
        }, {
          lightness: 19
        }]
      }, {
        featureType: 'administrative',
        elementType: 'geometry.fill',
        stylers: [{
          color: '#fefefe'
        }, {
          lightness: 20
        }]
      }, {
        featureType: 'administrative',
        elementType: 'geometry.stroke',
        stylers: [{
          color: '#fefefe'
        }, {
          lightness: 17
        }, {
          weight: 1.2
        }]
      }],
      Gray: [{
        featureType: 'all',
        elementType: 'labels.text.fill',
        stylers: [{
          saturation: 36
        }, {
          color: '#000000'
        }, {
          lightness: 40
        }]
      }, {
        featureType: 'all',
        elementType: 'labels.text.stroke',
        stylers: [{
          visibility: 'on'
        }, {
          color: '#000000'
        }, {
          lightness: 16
        }]
      }, {
        featureType: 'all',
        elementType: 'labels.icon',
        stylers: [{
          visibility: 'off'
        }]
      }, {
        featureType: 'administrative',
        elementType: 'geometry.fill',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 20
        }]
      }, {
        featureType: 'administrative',
        elementType: 'geometry.stroke',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 17
        }, {
          weight: 1.2
        }]
      }, {
        featureType: 'landscape',
        elementType: 'geometry',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 20
        }]
      }, {
        featureType: 'poi',
        elementType: 'geometry',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 21
        }]
      }, {
        featureType: 'road.highway',
        elementType: 'geometry.fill',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 17
        }]
      }, {
        featureType: 'road.highway',
        elementType: 'geometry.stroke',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 29
        }, {
          weight: 0.2
        }]
      }, {
        featureType: 'road.arterial',
        elementType: 'geometry',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 18
        }]
      }, {
        featureType: 'road.local',
        elementType: 'geometry',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 16
        }]
      }, {
        featureType: 'transit',
        elementType: 'geometry',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 19
        }]
      }, {
        featureType: 'water',
        elementType: 'geometry',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 17
        }]
      }],
      Midnight: [{
        featureType: 'all',
        elementType: 'labels.text.fill',
        stylers: [{
          color: '#ffffff'
        }]
      }, {
        featureType: 'all',
        elementType: 'labels.text.stroke',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 13
        }]
      }, {
        featureType: 'administrative',
        elementType: 'geometry.fill',
        stylers: [{
          color: '#000000'
        }]
      }, {
        featureType: 'administrative',
        elementType: 'geometry.stroke',
        stylers: [{
          color: '#144b53'
        }, {
          lightness: 14
        }, {
          weight: 1.4
        }]
      }, {
        featureType: 'landscape',
        elementType: 'all',
        stylers: [{
          color: '#08304b'
        }]
      }, {
        featureType: 'poi',
        elementType: 'geometry',
        stylers: [{
          color: '#0c4152'
        }, {
          lightness: 5
        }]
      }, {
        featureType: 'road.highway',
        elementType: 'geometry.fill',
        stylers: [{
          color: '#000000'
        }]
      }, {
        featureType: 'road.highway',
        elementType: 'geometry.stroke',
        stylers: [{
          color: '#0b434f'
        }, {
          lightness: 25
        }]
      }, {
        featureType: 'road.arterial',
        elementType: 'geometry.fill',
        stylers: [{
          color: '#000000'
        }]
      }, {
        featureType: 'road.arterial',
        elementType: 'geometry.stroke',
        stylers: [{
          color: '#0b3d51'
        }, {
          lightness: 16
        }]
      }, {
        featureType: 'road.local',
        elementType: 'geometry',
        stylers: [{
          color: '#000000'
        }]
      }, {
        featureType: 'transit',
        elementType: 'all',
        stylers: [{
          color: '#146474'
        }]
      }, {
        featureType: 'water',
        elementType: 'all',
        stylers: [{
          color: '#021019'
        }]
      }],
      Hopper: [{
        featureType: 'water',
        elementType: 'geometry',
        stylers: [{
          hue: '#165c64'
        }, {
          saturation: 34
        }, {
          lightness: -69
        }, {
          visibility: 'on'
        }]
      }, {
        featureType: 'landscape',
        elementType: 'geometry',
        stylers: [{
          hue: '#b7caaa'
        }, {
          saturation: -14
        }, {
          lightness: -18
        }, {
          visibility: 'on'
        }]
      }, {
        featureType: 'landscape.man_made',
        elementType: 'all',
        stylers: [{
          hue: '#cbdac1'
        }, {
          saturation: -6
        }, {
          lightness: -9
        }, {
          visibility: 'on'
        }]
      }, {
        featureType: 'road',
        elementType: 'geometry',
        stylers: [{
          hue: '#8d9b83'
        }, {
          saturation: -89
        }, {
          lightness: -12
        }, {
          visibility: 'on'
        }]
      }, {
        featureType: 'road.highway',
        elementType: 'geometry',
        stylers: [{
          hue: '#d4dad0'
        }, {
          saturation: -88
        }, {
          lightness: 54
        }, {
          visibility: 'simplified'
        }]
      }, {
        featureType: 'road.arterial',
        elementType: 'geometry',
        stylers: [{
          hue: '#bdc5b6'
        }, {
          saturation: -89
        }, {
          lightness: -3
        }, {
          visibility: 'simplified'
        }]
      }, {
        featureType: 'road.local',
        elementType: 'geometry',
        stylers: [{
          hue: '#bdc5b6'
        }, {
          saturation: -89
        }, {
          lightness: -26
        }, {
          visibility: 'on'
        }]
      }, {
        featureType: 'poi',
        elementType: 'geometry',
        stylers: [{
          hue: '#c17118'
        }, {
          saturation: 61
        }, {
          lightness: -45
        }, {
          visibility: 'on'
        }]
      }, {
        featureType: 'poi.park',
        elementType: 'all',
        stylers: [{
          hue: '#8ba975'
        }, {
          saturation: -46
        }, {
          lightness: -28
        }, {
          visibility: 'on'
        }]
      }, {
        featureType: 'transit',
        elementType: 'geometry',
        stylers: [{
          hue: '#a43218'
        }, {
          saturation: 74
        }, {
          lightness: -51
        }, {
          visibility: 'simplified'
        }]
      }, {
        featureType: 'administrative.province',
        elementType: 'all',
        stylers: [{
          hue: '#ffffff'
        }, {
          saturation: 0
        }, {
          lightness: 100
        }, {
          visibility: 'simplified'
        }]
      }, {
        featureType: 'administrative.neighborhood',
        elementType: 'all',
        stylers: [{
          hue: '#ffffff'
        }, {
          saturation: 0
        }, {
          lightness: 100
        }, {
          visibility: 'off'
        }]
      }, {
        featureType: 'administrative.locality',
        elementType: 'labels',
        stylers: [{
          hue: '#ffffff'
        }, {
          saturation: 0
        }, {
          lightness: 100
        }, {
          visibility: 'off'
        }]
      }, {
        featureType: 'administrative.land_parcel',
        elementType: 'all',
        stylers: [{
          hue: '#ffffff'
        }, {
          saturation: 0
        }, {
          lightness: 100
        }, {
          visibility: 'off'
        }]
      }, {
        featureType: 'administrative',
        elementType: 'all',
        stylers: [{
          hue: '#3a3935'
        }, {
          saturation: 5
        }, {
          lightness: -57
        }, {
          visibility: 'off'
        }]
      }, {
        featureType: 'poi.medical',
        elementType: 'geometry',
        stylers: [{
          hue: '#cba923'
        }, {
          saturation: 50
        }, {
          lightness: -46
        }, {
          visibility: 'on'
        }]
      }],
      Beard: [{
        featureType: 'poi.business',
        elementType: 'labels.text',
        stylers: [{
          visibility: 'on'
        }, {
          color: '#333333'
        }]
      }],
      AssassianCreed: [{
        featureType: 'all',
        elementType: 'all',
        stylers: [{
          visibility: 'on'
        }]
      }, {
        featureType: 'all',
        elementType: 'labels',
        stylers: [{
          visibility: 'off'
        }, {
          saturation: '-100'
        }]
      }, {
        featureType: 'all',
        elementType: 'labels.text.fill',
        stylers: [{
          saturation: 36
        }, {
          color: '#000000'
        }, {
          lightness: 40
        }, {
          visibility: 'off'
        }]
      }, {
        featureType: 'all',
        elementType: 'labels.text.stroke',
        stylers: [{
          visibility: 'off'
        }, {
          color: '#000000'
        }, {
          lightness: 16
        }]
      }, {
        featureType: 'all',
        elementType: 'labels.icon',
        stylers: [{
          visibility: 'off'
        }]
      }, {
        featureType: 'administrative',
        elementType: 'geometry.fill',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 20
        }]
      }, {
        featureType: 'administrative',
        elementType: 'geometry.stroke',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 17
        }, {
          weight: 1.2
        }]
      }, {
        featureType: 'landscape',
        elementType: 'geometry',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 20
        }]
      }, {
        featureType: 'landscape',
        elementType: 'geometry.fill',
        stylers: [{
          color: '#4d6059'
        }]
      }, {
        featureType: 'landscape',
        elementType: 'geometry.stroke',
        stylers: [{
          color: '#4d6059'
        }]
      }, {
        featureType: 'landscape.natural',
        elementType: 'geometry.fill',
        stylers: [{
          color: '#4d6059'
        }]
      }, {
        featureType: 'poi',
        elementType: 'geometry',
        stylers: [{
          lightness: 21
        }]
      }, {
        featureType: 'poi',
        elementType: 'geometry.fill',
        stylers: [{
          color: '#4d6059'
        }]
      }, {
        featureType: 'poi',
        elementType: 'geometry.stroke',
        stylers: [{
          color: '#4d6059'
        }]
      }, {
        featureType: 'road',
        elementType: 'geometry',
        stylers: [{
          visibility: 'on'
        }, {
          color: '#7f8d89'
        }]
      }, {
        featureType: 'road',
        elementType: 'geometry.fill',
        stylers: [{
          color: '#7f8d89'
        }]
      }, {
        featureType: 'road.highway',
        elementType: 'geometry.fill',
        stylers: [{
          color: '#7f8d89'
        }, {
          lightness: 17
        }]
      }, {
        featureType: 'road.highway',
        elementType: 'geometry.stroke',
        stylers: [{
          color: '#7f8d89'
        }, {
          lightness: 29
        }, {
          weight: 0.2
        }]
      }, {
        featureType: 'road.arterial',
        elementType: 'geometry',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 18
        }]
      }, {
        featureType: 'road.arterial',
        elementType: 'geometry.fill',
        stylers: [{
          color: '#7f8d89'
        }]
      }, {
        featureType: 'road.arterial',
        elementType: 'geometry.stroke',
        stylers: [{
          color: '#7f8d89'
        }]
      }, {
        featureType: 'road.local',
        elementType: 'geometry',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 16
        }]
      }, {
        featureType: 'road.local',
        elementType: 'geometry.fill',
        stylers: [{
          color: '#7f8d89'
        }]
      }, {
        featureType: 'road.local',
        elementType: 'geometry.stroke',
        stylers: [{
          color: '#7f8d89'
        }]
      }, {
        featureType: 'transit',
        elementType: 'geometry',
        stylers: [{
          color: '#000000'
        }, {
          lightness: 19
        }]
      }, {
        featureType: 'water',
        elementType: 'all',
        stylers: [{
          color: '#2b3638'
        }, {
          visibility: 'on'
        }]
      }, {
        featureType: 'water',
        elementType: 'geometry',
        stylers: [{
          color: '#2b3638'
        }, {
          lightness: 17
        }]
      }, {
        featureType: 'water',
        elementType: 'geometry.fill',
        stylers: [{
          color: '#24282b'
        }]
      }, {
        featureType: 'water',
        elementType: 'geometry.stroke',
        stylers: [{
          color: '#24282b'
        }]
      }, {
        featureType: 'water',
        elementType: 'labels',
        stylers: [{
          visibility: 'off'
        }]
      }, {
        featureType: 'water',
        elementType: 'labels.text',
        stylers: [{
          visibility: 'off '
        }]
      }, {
        featureType: 'water',
        elementType: 'labels.text.fill',
        stylers: [{
          visibility: 'off'
        }]
      }, {
        featureType: 'water',
        elementType: 'labels.text.stroke',
        stylers: [{
          visibility: 'off'
        }]
      }, {
        featureType: 'water',
        elementType: 'labels.icon',
        stylers: [{
          visibility: 'off'
        }]
      }],
      SubtleGray: [{
        featureType: 'administrative',
        elementType: 'all',
        stylers: [{
          saturation: '-100'
        }]
      }, {
        featureType: 'administrative.province',
        elementType: 'all',
        stylers: [{
          visibility: 'off'
        }]
      }, {
        featureType: 'landscape',
        elementType: 'all',
        stylers: [{
          saturation: -100
        }, {
          lightness: 65
        }, {
          visibility: 'on'
        }]
      }, {
        featureType: 'poi',
        elementType: 'all',
        stylers: [{
          saturation: -100
        }, {
          lightness: '50'
        }, {
          visibility: 'simplified'
        }]
      }, {
        featureType: 'road',
        elementType: 'all',
        stylers: [{
          saturation: -100
        }]
      }, {
        featureType: 'road.highway',
        elementType: 'all',
        stylers: [{
          visibility: 'simplified'
        }]
      }, {
        featureType: 'road.arterial',
        elementType: 'all',
        stylers: [{
          lightness: '30'
        }]
      }, {
        featureType: 'road.local',
        elementType: 'all',
        stylers: [{
          lightness: '40'
        }]
      }, {
        featureType: 'transit',
        elementType: 'all',
        stylers: [{
          saturation: -100
        }, {
          visibility: 'simplified'
        }]
      }, {
        featureType: 'water',
        elementType: 'geometry',
        stylers: [{
          hue: '#ffff00'
        }, {
          lightness: -25
        }, {
          saturation: -97
        }]
      }, {
        featureType: 'water',
        elementType: 'labels',
        stylers: [{
          lightness: -25
        }, {
          saturation: -100
        }]
      }],
      Tripitty: [{
        featureType: 'all',
        elementType: 'labels',
        stylers: [{
          visibility: 'off'
        }]
      }, {
        featureType: 'administrative',
        elementType: 'all',
        stylers: [{
          visibility: 'off'
        }]
      }, {
        featureType: 'landscape',
        elementType: 'all',
        stylers: [{
          color: '#2c5ca5'
        }]
      }, {
        featureType: 'poi',
        elementType: 'all',
        stylers: [{
          color: '#2c5ca5'
        }]
      }, {
        featureType: 'road',
        elementType: 'all',
        stylers: [{
          visibility: 'off'
        }]
      }, {
        featureType: 'transit',
        elementType: 'all',
        stylers: [{
          visibility: 'off'
        }]
      }, {
        featureType: 'water',
        elementType: 'all',
        stylers: [{
          color: '#193a70'
        }, {
          visibility: 'on'
        }]
      }]
    };
    $googlemaps.each(function (index, value) {
      var $googlemap = $(value);
      var latLng = $googlemap.data('latlng').split(',');
      var markerPopup = $googlemap.html();
      var icon = $googlemap.data('icon') ? $googlemap.data('icon') : 'https://maps.gstatic.com/mapfiles/api-3/images/spotlight-poi.png';
      var zoom = $googlemap.data('zoom');
      var mapStyle = $googlemap.data('theme');
      var mapElement = value;

      if ($googlemap.data('theme') === 'streetview') {
        var pov = $googlemap.data('pov');
        var _mapOptions = {
          position: {
            lat: Number(latLng[0]),
            lng: Number(latLng[1])
          },
          pov: pov,
          zoom: zoom,
          gestureHandling: 'none',
          scrollwheel: false
        };
        return new google.maps.StreetViewPanorama(mapElement, _mapOptions);
      }

      var mapOptions = {
        zoom: zoom,
        scrollwheel: $googlemap.data('scrollwheel'),
        center: new google.maps.LatLng(latLng[0], latLng[1]),
        styles: mapStyles[mapStyle]
      };
      var map = new google.maps.Map(mapElement, mapOptions);
      var infowindow = new google.maps.InfoWindow({
        content: markerPopup
      });
      var marker = new google.maps.Marker({
        position: new google.maps.LatLng(latLng[0], latLng[1]),
        icon: icon,
        map: map
      });
      marker.addListener('click', function () {
        infowindow.open(map, marker);
      });
      return null;
    });
  }
}
/*-----------------------------------------------
|  Hover dir for Gallery
-----------------------------------------------*/


spUtils.$document.ready(function () {
  var $gridder = $('.gridder');

  if ($gridder.length) {
    $gridder.each(function (index, value) {
      var $this = $(value);
      var options = $.extend({
        scroll: true,
        scrollOffset: 75,
        scrollTo: 'listitem',
        // panel or listitem
        animationSpeed: 400,
        animationEasing: 'easeInOutExpo',
        showNav: true,
        // Show Navigation
        nextText: "<span class='fas fa-angle-right'></span>",
        // Next button text
        prevText: "<span class='fas fa-angle-left'></span>",
        // Previous button text
        closeText: "<span class='fas fa-times'></span>" // Close button text

      }, $this.data('options'));
      $this.gridderExpander(options);
    });
  }
});
/*-----------------------------------------------
|  Hover dir for Gallery
-----------------------------------------------*/

spUtils.$document.ready(function () {
  var hoverdir = $('.hoverdir-grid');

  if (hoverdir.length) {
    hoverdir.each(function (index, value) {
      var $this = $(value);
      var options = $.extend({
        speed: 300,
        easing: 'ease',
        hoverDelay: 0,
        inverse: false,
        hoverElem: '.hoverdir-item-content'
      }, $this.data('options'));
      $this.find('.hoverdir-item').hoverdir(options);
    });
  }
});
/*-----------------------------------------------
|   Masonry with isotope
-----------------------------------------------*/

spUtils.$window.on('load', function () {
  var $sortables = $('.sortable');

  if ($sortables.length) {
    var Selector = {
      SORTABLE_ITEM: '.sortable-item',
      SORTABLE_CONTAINER: '.sortable-container',
      MENU: '.menu',
      ITEM: '.item'
    };
    var ClassName = {
      ACTIVE: 'active'
    };
    var DATA_KEY = {
      OPTIONS: 'options',
      FILTER_GROUP: 'filter-group',
      FILTER: 'filter'
    };
    $sortables.each(function (index, value) {
      var $sortable = $(value);
      var $masonryContainer = $sortable.find(Selector.SORTABLE_CONTAINER);
      var $menu = $sortable.find(Selector.MENU);
      $masonryContainer.isotope($.extend($sortable.data(DATA_KEY.OPTIONS) || {}, {
        itemSelector: Selector.SORTABLE_ITEM,
        masonry: {
          columnWidth: Selector.SORTABLE_ITEM
        }
      }));
      /*-----------------------------------------------
      |   Flatten object by concatting values
      -----------------------------------------------*/

      var concatValues = function concatValues(obj) {
        return Object.keys(obj).map(function (key) {
          return obj[key];
        }).join();
      };
      /*-----------------------------------------------
      |   Store filter for each group
      -----------------------------------------------*/


      var filters = {};
      $menu.on('click', Selector.ITEM, function (e) {
        var $masonryFilter = $(e.target);
        filters[$masonryFilter.parent().data(DATA_KEY.FILTER_GROUP) || 0] = $masonryFilter.data(DATA_KEY.FILTER);
        var filterValue = concatValues(filters);
        $masonryFilter.siblings().removeClass(ClassName.ACTIVE);
        $masonryFilter.addClass(ClassName.ACTIVE);
        $masonryContainer.isotope({
          filter: filterValue
        });
      });
    });
  }
});
/*-----------------------------------------------
|   Lightbox
-----------------------------------------------*/

/*
  global lightbox
*/

spUtils.$document.ready(function () {
  if ($('[data-lightbox]').length) {
    lightbox.option({
      resizeDuration: 400,
      wrapAround: true,
      fadeDuration: 300,
      imageFadeDuration: 300
    });
  }
});
/*-----------------------------------------------
|   Owl Carousel
-----------------------------------------------*/

var $carousel = $('.owl-carousel-theme');
spUtils.$document.ready(function () {
  if ($carousel.length) {
    var Selector = {
      ALL_TIMELINE: '*[data-zanim-timeline]',
      ACTIVE_ITEM: '.owl-item.active'
    };
    var owlZanim = {
      zanimTimeline: function zanimTimeline($el) {
        return $el.find(Selector.ALL_TIMELINE);
      },
      play: function play($el) {
        if (this.zanimTimeline($el).length === 0) return;
        $el.find(Selector.ACTIVE_ITEM + " > " + Selector.ALL_TIMELINE).zanimation(function (animation) {
          animation.play();
        });
      },
      kill: function kill($el) {
        if (this.zanimTimeline($el).length === 0) return;
        this.zanimTimeline($el).zanimation(function (animation) {
          animation.kill();
        });
      }
    };
    $carousel.each(function (index, value) {
      var $this = $(value);
      var options = $this.data('options') || {};

      if (spUtils.isRTL()) {
        options.rtl = true;
        options.navText || (options.navText = ['<span class="fas fa-angle-right"></span>', '<span class="fas fa-angle-left"></span>']);
      } else {
        options.navText || (options.navText = ['<span class="fas fa-angle-left"></span>', '<span class="fas fa-angle-right"></span>']);
      }

      options.touchDrag = true;
      $this.owlCarousel($.extend(options || {}, {
        onInitialized: function onInitialized(event) {
          owlZanim.play($(event.target));
        },
        onTranslate: function onTranslate(event) {
          owlZanim.kill($(event.target));
        },
        onTranslated: function onTranslated(event) {
          owlZanim.play($(event.target));
        }
      }));
    });
  }
});
/*-----------------------------------------------
|   Inline Player [plyr]
-----------------------------------------------*/

/*
  global Plyr
*/

spUtils.$document.ready(function () {
  var $players = $('.player');

  if ($players.length) {
    $players.each(function (index, value) {
      return new Plyr($(value), {
        captions: {
          active: true
        }
      });
    });
  }

  return false;
});
/*-----------------------------------------------
|   Pre-loader
-----------------------------------------------*/

$.holdReady(true);
$($('main section')[0]).imagesLoaded({
  background: '.bg-holder'
}, function () {
  $.holdReady(false);
});
spUtils.$document.ready(function () {
  var $preloader = $('#preloader');
  $preloader.addClass('loaded');
  setTimeout(function () {
    $preloader.hide();
  }, 800);
});
/*
 global ProgressBar
*/

spUtils.$document.ready(function () {
  // progressbar.js@1.0.0 version is used
  // Docs: http://progressbarjs.readthedocs.org/en/1.0.0/

  /*-----------------------------------------------
  |   Progress Circle
  -----------------------------------------------*/
  var progresCircle = $('.progress-circle');

  if (progresCircle.length) {
    progresCircle.each(function (index, value) {
      var $this = $(value);
      var options = $this.data('options');
      var bar = new ProgressBar.Circle(value, $.extend({
        color: '#aaa',
        // This has to be the same size as the maximum width to
        // prevent clipping
        strokeWidth: 1.5,
        trailWidth: 1.4,
        easing: 'easeInOut',
        duration: 3000,
        svgStyle: {
          'stroke-linecap': 'round',
          display: 'block',
          width: '100%'
        },
        text: {
          autoStyleContainer: false
        },
        from: {
          color: '#aaa',
          width: 1
        },
        to: {
          color: '#333',
          width: 1
        },
        // Set default step function for all animate calls
        step: function step(state, circle) {
          circle.path.setAttribute('stroke', state.color); // circle.path.setAttribute('stroke-width', state.width);

          var percentage = Math.round(circle.value() * 100);
          circle.setText("<span class='value'>" + percentage + "<b>%</b></span> <span class=\"pt-2\">" + options.text + "</span>");
        }
      }, options.css));
      var playProgressTriggered = false;

      var progressCircleAnimation = function progressCircleAnimation() {
        if (!playProgressTriggered) {
          if (spUtils.isScrolledIntoView(value) || spDetector.isPuppeteer) {
            bar.animate(options.progress / 100);
            playProgressTriggered = true;
          }
        }

        return playProgressTriggered;
      };

      progressCircleAnimation();
      spUtils.$window.scroll(function () {
        progressCircleAnimation();
      });
    });
  }
  /*-----------------------------------------------
  |   Progress Line
  -----------------------------------------------*/


  var progressLine = $('.progress-line');

  if (progressLine.length) {
    progressLine.each(function (index, value) {
      var $this = $(value);
      var options = $this.data('options');
      var bar = new ProgressBar.Line(value, $.extend({
        strokeWidth: 1,
        easing: 'easeInOut',
        duration: 3000,
        color: '#333',
        trailColor: '#eee',
        trailWidth: 1,
        svgStyle: {
          width: '100%',
          height: '0.25rem',
          'stroke-linecap': 'round',
          'border-radius': '0.125rem'
        },
        text: {
          style: {
            transform: null
          },
          autoStyleContainer: false
        },
        from: {
          color: '#aaa'
        },
        to: {
          color: '#111'
        },
        step: function step(state, line) {
          line.setText("<span class='value'>" + Math.round(line.value() * 100) + "<b>%</b></span> <span>" + options.text + "</span>");
        }
      }, options.css));
      var playProgressTriggered = false;

      var progressLineAnimation = function progressLineAnimation() {
        if (!playProgressTriggered) {
          if (spUtils.isScrolledIntoView(value) || spDetector.isPuppeteer) {
            bar.animate(options.progress / 100);
            playProgressTriggered = true;
          }
        }

        return playProgressTriggered;
      };

      progressLineAnimation();
      spUtils.$window.scroll(function () {
        progressLineAnimation();
      });
    });
  }
});
/*-----------------------------------------------
|   Rellax [Parallax]
-----------------------------------------------*/

/*
  global Rellax
*/

spUtils.$document.ready(function () {
  var Selector = {
    PARALLAX: '.parallax'
  };

  if ($(Selector.PARALLAX).length) {
    var callRellax = function callRellax() {
      return new Rellax(Selector.PARALLAX, {
        speed: -3
      });
    };

    if (!spDetector.isIE && !spDetector.isIE11 && !spDetector.isPuppeteer) callRellax();
  }
});
/*-----------------------------------------------
|   Remodal [video lightbox]
-----------------------------------------------*/

spUtils.$document.ready(function () {
  var $videoModals = $('.video-modal');

  if ($videoModals.length) {
    spUtils.$body.after("\n      <div id='videoModal' class='remodal remodal-video'>\n        <button data-remodal-action='close' class='remodal-close'></button>\n        <div class='embed-responsive embed-responsive-16by9'>\n          <div id='videoModalIframeWrapper'></div>\n        </div>\n      </div>\n    ");
    var $videoModal = $('#videoModal').remodal();
    var $videoModalIframeWrapper = $('#videoModalIframeWrapper');
    $videoModals.each(function (index, value) {
      $(value).on('click', function (e) {
        e.preventDefault();
        var $this = $(e.currentTarget);
        var ytId = $this.attr('href').split('/');
        var start = $this.data('start');
        var end = $this.data('end');

        if (ytId[2] === 'www.youtube.com') {
          $videoModalIframeWrapper.html("<iframe id='videoModalIframe' src='//www.youtube.com/embed/" + ytId[3].split('?v=')[1] + "?rel=0&amp;autoplay=1&amp;enablejsapi=0&amp;start=" + start + "&ampend=" + end + "' allowfullscreen' frameborder='0' class='embed-responsive-item hide'></iframe>");
        } else if (ytId[2] === 'vimeo.com') {
          $videoModalIframeWrapper.html("<iframe id='videoModalIframe' src='https://player.vimeo.com/video/" + ytId[3] + "?autoplay=1&title=0&byline=0&portrait=0 ?autoplay=1&title=0&byline=0&portrait=0 hide'></iframe>");
        }

        $videoModal.open();
      });
    });
    spUtils.$document.on('closed', '.remodal', function (e) {
      var $this = $(e.currentTarget);

      if ($this.attr('id') === 'videoModal') {
        $videoModalIframeWrapper.html('');
      }
    });
  }
});
/*-----------------------------------------------
|   Sementic UI
-----------------------------------------------*/

spUtils.$document.ready(function () {
  var uiDropdown = $('.ui.dropdown');
  var uiAccordion = $('.ui.accordion');
  /*-----------------------------------------------
  |   Dropdown
  -----------------------------------------------*/

  if (uiDropdown.length) {
    uiDropdown.dropdown();
  }
  /*-----------------------------------------------
  |   Accordion
  -----------------------------------------------*/


  if (uiAccordion.length) {
    uiAccordion.each(function (index, value) {
      var $this = $(value);
      $this.accordion($.extend({
        exclusive: false
      }, $this.data('options') || {}));
    });
  }
});
/*
  global Stickyfill
*/

/*-----------------------------------------------
|   Sticky fill
-----------------------------------------------*/

spUtils.$document.ready(function () {
  Stickyfill.add($('.sticky-top'));
});
/*-----------------------------------------------
|   Sticky Kit
-----------------------------------------------*/

spUtils.$document.ready(function () {
  var stickyKit = $('.sticky-kit');

  if (stickyKit.length) {
    stickyKit.each(function (index, value) {
      var options = $(value).data('options');
      options = $.extend({}, options);
      $(value).stick_in_parent(options);
    });
  }
});
/*-----------------------------------------------
|   Tabs
-----------------------------------------------*/

spUtils.$document.ready(function () {
  var $fancyTabs = $('.fancy-tab');

  if ($fancyTabs.length) {
    var Selector = {
      TAB_BAR: '.nav-bar',
      TAB_BAR_ITEM: '.nav-bar-item',
      TAB_CONTENTS: '.tab-contents'
    };
    var ClassName = {
      ACTIVE: 'active',
      TRANSITION_REVERSE: 'transition-reverse',
      TAB_INDICATOR: 'tab-indicator'
    };
    /*-----------------------------------------------
    |   Function for active tab indicator change
    -----------------------------------------------*/

    var updateIncicator = function updateIncicator($indicator, $tabs, $tabnavCurrentItem) {
      var _$tabnavCurrentItem$p = $tabnavCurrentItem.position(),
          left = _$tabnavCurrentItem$p.left;

      var right = $tabs.children(Selector.TAB_BAR).outerWidth() - (left + $tabnavCurrentItem.outerWidth());
      $indicator.css({
        left: left,
        right: right
      });
    };

    $fancyTabs.each(function (index, value) {
      var $tabs = $(value);
      var $navBar = $tabs.children(Selector.TAB_BAR);
      var $tabnavCurrentItem = $navBar.children(Selector.TAB_BAR_ITEM + "." + ClassName.ACTIVE);
      $navBar.append("\n        <div class=" + ClassName.TAB_INDICATOR + "></div>\n      ");
      var $indicator = $navBar.children("." + ClassName.TAB_INDICATOR);
      var $preIndex = $tabnavCurrentItem.index();
      updateIncicator($indicator, $tabs, $tabnavCurrentItem);
      $navBar.children(Selector.TAB_BAR_ITEM).click(function (e) {
        $tabnavCurrentItem = $(e.currentTarget);
        var $currentIndex = $tabnavCurrentItem.index();
        var $tabContent = $tabs.children(Selector.TAB_CONTENTS).children().eq($currentIndex);
        $tabnavCurrentItem.siblings().removeClass(ClassName.ACTIVE);
        $tabnavCurrentItem.addClass(ClassName.ACTIVE);
        $tabContent.siblings().removeClass(ClassName.ACTIVE);
        $tabContent.addClass(ClassName.ACTIVE);
        /*-----------------------------------------------
        |   Indicator Transition
        -----------------------------------------------*/

        updateIncicator($indicator, $tabs, $tabnavCurrentItem);

        if ($currentIndex - $preIndex <= 0) {
          $indicator.addClass(ClassName.TRANSITION_REVERSE);
        } else {
          $indicator.removeClass(ClassName.TRANSITION_REVERSE);
        }

        $preIndex = $currentIndex;
      });
      spUtils.$window.on('resize', function () {
        updateIncicator($indicator, $tabs, $tabnavCurrentItem);
      });
    });
  }
});
/*-----------------------------------------------
|  Hover dir for Gallery
-----------------------------------------------*/

var $carouselSynced = $('.owl-carousel-synced');
spUtils.$document.ready(function () {
  if ($carouselSynced.length) {
    var Selector = {
      ALL_TIMELINE: '*[data-zanim-timeline]',
      ACTIVE_ITEM: '.owl-item.active'
    };
    var owlZanim = {
      zanimTimeline: function zanimTimeline($el) {
        return $el.find(Selector.ALL_TIMELINE);
      },
      play: function play($el) {
        if (this.zanimTimeline($el).length === 0) return;
        $el.find(Selector.ACTIVE_ITEM + " > " + Selector.ALL_TIMELINE).zanimation(function (animation) {
          animation.play();
        });
      },
      kill: function kill($el) {
        if (this.zanimTimeline($el).length === 0) return;
        this.zanimTimeline($el).zanimation(function (animation) {
          animation.kill();
        });
      }
    };
    $carouselSynced.each(function (index, value) {
      var $this = $(value);
      var options = $this.data('options') || {};

      if (spUtils.isRTL()) {
        options.rtl = true;
        options.navText || (options.navText = ['<span class="fas fa-angle-right"></span>', '<span class="fas fa-angle-left"></span>']);
      } else {
        options.navText || (options.navText = ['<span class="fas fa-angle-left"></span>', '<span class="fas fa-angle-right"></span>']);
      }

      options.touchDrag = true;
      $this.owlCarousel($.extend(options || {}, {
        onInitialized: function onInitialized(event) {
          owlZanim.play($(event.target));
          var avatar = $($carouselSynced.find('.item')[event.item.index]).data('avatar');
          $('.testimonial-avatar').css('background-image', "url(" + avatar + ")");
        },
        onTranslate: function onTranslate(event) {
          owlZanim.kill($(event.target));
        },
        onTranslated: function onTranslated(event) {
          owlZanim.play($(event.target));
        },
        onChange: function onChange(event) {
          var avatar = $($carouselSynced.find('.item')[event.item.index + 1]).data('avatar');
          $('.testimonial-avatar').css('background-image', "url(" + avatar + ")");
        }
      }));
    });
  }
});
/*-----------------------------------------------
|   Tooltip
-----------------------------------------------*/

spUtils.$document.ready(function () {
  // https://getbootstrap.com/docs/4.0/components/tooltips/#example-enable-tooltips-everywhere
  $('[data-toggle="tooltip"]').tooltip();
});
/*-----------------------------------------------
|   Typed Text
-----------------------------------------------*/

/*
  global Typed
 */

spUtils.$document.ready(function () {
  var typedText = $('.typed-text');

  if (typedText.length) {
    typedText.each(function (index, value) {
      return new Typed(value, {
        strings: $(value).data('typed-text'),
        typeSpeed: 100,
        loop: true,
        backDelay: 1500
      });
    });
  }
});
/*-----------------------------------------------
|   YTPlayer
-----------------------------------------------*/

spUtils.$document.ready(function () {
  var Selector = {
    BG_YOUTUBE: '.bg-youtube',
    BG_HOLDER: '.bg-holder'
  };
  var DATA_KEY = {
    PROPERTY: 'property'
  };
  var $youtubeBackground = $(Selector.BG_YOUTUBE);

  if ($youtubeBackground.length) {
    $youtubeBackground.each(function (index, value) {
      var $this = $(value);
      $this.data(DATA_KEY.PROPERTY, $.extend($this.data(DATA_KEY.PROPERTY), {
        showControls: false,
        loop: true,
        autoPlay: true,
        containment: $this.parent(Selector.BG_HOLDER)
      }));
      $this.YTPlayer();
    });
  }
});
/*-----------------------------------------------
|   Global Functions
-----------------------------------------------*/

/*
global TimelineMax, TweenMax, CustomEase
*/

CustomEase.create('CubicBezier', '.77,0,.18,1');

var filterBlur = function filterBlur() {
  var blur = 'blur(5px)';

  if (spDetector.iPadiPhoneFirefox || spDetector.macFirefox) {
    blur = 'blur(0px)';
  }

  return blur;
};

var zanimationEffects = {
  default: {
    from: {
      opacity: 0,
      y: 60
    },
    to: {
      opacity: 1,
      y: 0
    },
    ease: 'CubicBezier',
    duration: 0.8,
    delay: 0
  },
  'slide-down': {
    from: {
      opacity: 0,
      y: -70
    },
    to: {
      opacity: 1,
      y: 0
    },
    ease: 'CubicBezier',
    duration: 0.8,
    delay: 0
  },
  'slide-left': {
    from: {
      opacity: 0,
      x: 70
    },
    to: {
      opacity: 1,
      x: 0
    },
    ease: 'CubicBezier',
    duration: 0.8,
    delay: 0
  },
  'slide-right': {
    from: {
      opacity: 0,
      x: -70
    },
    to: {
      opacity: 1,
      x: 0
    },
    ease: 'CubicBezier',
    duration: 0.8,
    delay: 0
  },
  'zoom-in': {
    from: {
      scale: 0.9,
      opacity: 0,
      filter: filterBlur()
    },
    to: {
      scale: 1,
      opacity: 1,
      filter: 'blur(0px)'
    },
    delay: 0,
    ease: 'CubicBezier',
    duration: 0.8
  },
  'zoom-out': {
    from: {
      scale: 1.1,
      opacity: 1,
      filter: filterBlur()
    },
    to: {
      scale: 1,
      opacity: 1,
      filter: 'blur(0px)'
    },
    delay: 0,
    ease: 'CubicBezier',
    duration: 0.8
  },
  'zoom-out-slide-right': {
    from: {
      scale: 1.1,
      opacity: 1,
      x: -70,
      filter: filterBlur()
    },
    to: {
      scale: 1,
      opacity: 1,
      x: 0,
      filter: 'blur(0px)'
    },
    delay: 0,
    ease: 'CubicBezier',
    duration: 0.8
  },
  'zoom-out-slide-left': {
    from: {
      scale: 1.1,
      opacity: 1,
      x: 70,
      filter: filterBlur()
    },
    to: {
      scale: 1,
      opacity: 1,
      x: 0,
      filter: 'blur(0px)'
    },
    delay: 0,
    ease: 'CubicBezier',
    duration: 0.8
  },
  'blur-in': {
    from: {
      opacity: 0,
      filter: filterBlur()
    },
    to: {
      opacity: 1,
      filter: 'blur(0px)'
    },
    delay: 0,
    ease: 'CubicBezier',
    duration: 0.8
  }
};

if (spUtils.isRTL()) {
  Object.keys(zanimationEffects).forEach(function (key) {
    if (zanimationEffects[key].from.x) {
      zanimationEffects[key].from.x = -zanimationEffects[key].from.x;
    }
  });
}

var breakPointConst = spUtils.getCurrentScreanBreakpoint();
/*-----------------------------------------------
|   Zanimation
-----------------------------------------------*/

(function zanimation($) {
  /*-----------------------------------------------
  |   Get Controller
  -----------------------------------------------*/
  var controllerZanim;

  var getController = function getController(el) {
    var $this = $(el);
    var options = {};
    var controller = {};
    $.each($this, function (index, value) {
      if (value.hasAttribute("data-zanim-" + breakPointConst.currentScrean)) {
        controllerZanim = "zanim-" + breakPointConst.currentScrean;
      } else {
        /*-----------------------------------------------
        |   Set the mobile first Animation
        -----------------------------------------------*/
        var animationBreakpoints = [];
        $.each(value.attributes, function (i, attribute) {
          if (attribute.name !== 'data-zanim-trigger' && (spDetector.isIE || spDetector.isIE11 || spDetector.isEdge ? attribute.name.match('^data-zanim-') : attribute.name.startsWith('data-zanim-'))) {
            var breakPoint = spUtils.breakpoints[attribute.name.split('data-zanim-')[1]];

            if (breakPoint < breakPointConst.currentBreakpoint) {
              animationBreakpoints.push({
                name: attribute.name.split('data-zanim-')[1],
                size: breakPoint
              });
            }
          }

          return i;
        });
        controllerZanim = undefined;

        if (animationBreakpoints.length !== 0) {
          animationBreakpoints = animationBreakpoints.sort(function (a, b) {
            return a.size - b.size;
          });
          var activeBreakpoint = animationBreakpoints.pop();
          controllerZanim = "zanim-" + activeBreakpoint.name;
        }
      }

      return index;
    });
    controller = $.extend(true, {}, options, $this.data(controllerZanim));

    if (!(controllerZanim === undefined)) {
      if ($this.data(controllerZanim).animation) {
        options = zanimationEffects[$this.data(controllerZanim).animation];
      } else {
        options = zanimationEffects.default;
      }
    }

    if (controllerZanim === undefined) {
      options = {
        delay: 0,
        duration: 0,
        ease: 'Expo.easeOut',
        from: {},
        to: {}
      };
    }
    /*-----------------------------------------------
    |   populating the controller
    -----------------------------------------------*/


    controller.delay || (controller.delay = options.delay);
    controller.duration || (controller.duration = options.duration);
    controller.from || (controller.from = options.from);
    controller.to || (controller.to = options.to);
    controller.ease && (controller.to.ease = controller.ease) && controller.to.ease || (controller.to.ease = options.ease);
    return controller;
  };
  /*-----------------------------------------------
  |   End of Get Controller
  -----------------------------------------------*/


  jQuery.fn.zanimation = function zanim(callback) {
    var $this = $(this);
    /*-----------------------------------------------
    |   For Timeline
    -----------------------------------------------*/

    var zanimTimeline = $this.data('zanim-timeline');

    if (zanimTimeline) {
      var timeline = new TimelineMax(zanimTimeline);
      var timelineElements = $this.find('[data-zanim-xs], [data-zanim-sm], [data-zanim-md], [data-zanim-lg], [data-zanim-xl]');
      timelineElements.map(function (index, value) {
        var controller = getController(value);
        timeline.fromTo($(value), controller.duration, controller.from, controller.to, controller.delay).pause();
        return index;
      });
      $this.imagesLoaded(function () {
        return callback(timeline);
      });
    } else if (!$this.parents('[data-zanim-timeline]').length) {
      /*-----------------------------------------------
      |   For single elements outside timeline
      -----------------------------------------------*/
      var controller = getController($this);
      callback(TweenMax.fromTo($this, controller.duration, controller.from, controller.to).delay(controller.delay).pause());
    }

    callback(new TimelineMax());
  };
})(jQuery);
/*-----------------------------------------------
|   Triggering zanimation when the element enters in the view
-----------------------------------------------*/


(function triggeringZanimation($) {
  var triggerZanimation = function triggerZanimation($this) {
    if (spUtils.isScrolledIntoView($this) && $this.attr('data-zanim-trigger') === 'scroll') {
      $this.zanimation(function (animation) {
        return animation.play();
      });
      if (!$this.data('zanim-repeat')) $this.removeAttr('data-zanim-trigger');
    }
  };

  spUtils.$document.ready(function () {
    /*-----------------------------------------------
    |   Playing zanimation for scroll triggers
    -----------------------------------------------*/
    $("*[data-zanim-trigger = 'scroll']").map(function (index, value) {
      triggerZanimation($(value));
      spUtils.$window.on('scroll', function () {
        triggerZanimation($(value));
      });
      return index;
    });
  });
})(jQuery);