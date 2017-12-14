/*!
 * Start Bootstrap - Agency v4.0.0-beta (http://startbootstrap.com/template-overviews/agency)
 * Copyright 2013-2017 Start Bootstrap
 * Licensed under MIT (https://github.com/BlackrockDigital/startbootstrap-agency/blob/master/LICENSE)
 */
!function(a){"use strict";a('a.js-scroll-trigger[href*="#"]:not([href="#"])').click(function(){if(location.pathname.replace(/^\//,"")==this.pathname.replace(/^\//,"")&&location.hostname==this.hostname){var e=a(this.hash);if((e=e.length?e:a("[name="+this.hash.slice(1)+"]")).length)return a("html, body").animate({scrollTop:e.offset().top-54},1e3,"easeInOutExpo"),!1}}),a(".js-scroll-trigger").click(function(){a(".navbar-collapse").collapse("hide")}),a("body").scrollspy({target:"#mainNav",offset:54}),a(window).scroll(function(){a("#mainNav").offset().top>100?a("#mainNav").addClass("navbar-shrink"):a("#mainNav").removeClass("navbar-shrink")})}(jQuery);