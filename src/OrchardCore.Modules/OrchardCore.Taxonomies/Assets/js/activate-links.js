// This script is used to add a class on the active link of a menu.
// Because Menus are often cached, the class needs to be set dynamically using JavaScript.

window.activateLinks = function (container, options) {
    var settings = Object.assign({
        // class to add to the selector
        class: "active",
        // custom selector based on the parent of the link
        selector: null
    }, options);

    var currentUrl = window.location.href.replace(window.location.protocol + '//' + window.location.host, '');

    var items = Array.from(container.querySelectorAll('a[href="' + currentUrl + '"]')).map(function (a) {
        return a.parentElement;
    });

    if (settings.selector) {
        items = items.flatMap(function (item) {
            return Array.from(item.querySelectorAll(settings.selector));
        });
    }

    items.forEach(function (item) {
        item.classList.add(settings.class);
    });

    return container;
};
