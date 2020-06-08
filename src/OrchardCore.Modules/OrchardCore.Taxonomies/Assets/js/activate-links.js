// This script is used to add a class on the active link of a menu.
// Because Menus are often cached, the class needs to be set dynamically using JavaScript.

(function ($) {

    $.fn.activateLinks = function (options) {

        var settings = $.extend({
            // class to add to the selector
            class: "active",
            // custom selector based on the parent of the link
            selector: null
        }, options);

        var currentUrl = window.location.href.replace(window.location.protocol + '//' + window.location.host, '');

        var items = $(this).find('a[href="' + currentUrl + '"]').parent();

        if (settings.selector) {
            items = items.find(settings.selector);
        }

        items.addClass(settings.class)

        return this;
    };
}(jQuery));