sortingListManager = function () {

    const saveOrders = (evt, url, errorMessage) => {

        var data = {
            oldIndex: evt.oldIndex,
            newIndex: evt.newIndex
        };
        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        }).catch(error => {
            console.log(error);
            alert(errorMessage || 'Unable to sort the list');
        });
    }

    const create = (selector, sortUrl, errorMessage) => {

        var sortable = document.querySelector(selector);

        if (!sortable) {
            console.log('Unable to find the sortable element. The given selector is: ' + selector);

            return;
        }

        if (sortUrl) {
            orderUrl = sortUrl;
        } else {
            orderUrl = sortable.getAttribute('data-sort-uri');
        }

        if (!orderUrl) {
            console.log('Unable to determine the sort post URI. Either pass it to the create function or set it as data-sort-uri to the sorting element.');

            return;
        }

        var sortable = Sortable.create(sortable, {
            handle: ".ui-sortable-handle",
            animation: 150,
            filter: ".ignore-elements",
            draggable: ".item",
            onUpdate: function (evt) {
                saveOrders(evt, orderUrl, errorMessage);
            }
        });
    };

    return {
        create: create
    }
}();
