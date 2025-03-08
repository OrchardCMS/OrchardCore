sortingListManager = function () {

    const saveOrders = (evt, url, errorMessage) => {

        const data = {
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

        const element = document.querySelector(selector);

        if (!element) {
            console.log('Unable to find the sortable element. The given selector is: ' + selector);

            return;
        }

        let orderUrl = sortUrl;

        if (!orderUrl) {
            orderUrl = element.getAttribute('data-sort-uri');

            if (!orderUrl) {
                console.log('Unable to determine the sort post URI. Either pass it to the create function or set it as data-sort-uri to the sorting element.');

                return;
            }
        }

        Sortable.create(element, {
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
