// This script is used to add a class on the active link of a menu.
// Because Menus are often cached, the class needs to be set dynamically using JavaScript.

window.activateLinks = function (container, options, cb) {
    var settings = Object.assign({
        // class to add to the selector
        class: "active",
        // custom selector based on the parent of the link
        selector: null,
        // how many segments to remove from url in order to find active link in menu
        traverse: 0
    }, options);

    var segments = window.location.href.replace(window.location.protocol + '//' + window.location.host, '').split("/");
    var level = segments.length;
    var minLevel = settings.traverse <= 0 ? level : level >= settings.traverse ? level - settings.traverse : level;

    while (level >= minLevel) {
        var currentUrl = segments.join('/');
        var items = Array.from(container.querySelectorAll('a[href="' + currentUrl + '"]')).map(function (a) {
            return a.parentElement;
        });

        if (settings.selector) {
            items = items.flatMap(function (item) {
                return Array.from(item.querySelectorAll(settings.selector));
            });
        }

        if (items.length > 0) {
            items.forEach(function (item) {
                item.classList.add(settings.class);
            });
            if (cb) {
                cb(items);
            }
            return container;
        }

        level -= 1;
        segments = segments.slice(0, level);
    }
    return container;
};
