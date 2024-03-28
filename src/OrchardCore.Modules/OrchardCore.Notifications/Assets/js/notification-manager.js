notificationManager = function () {

    const initialize = (readUrl, wrapperSelector) => {

        if (!readUrl) {
            console.log('No readUrl was provided.');

            return;
        }

        var elements = document.getElementsByClassName('mark-notification-as-read');

        for (let i = 0; i < elements.length; i++) {

            ['click', 'mouseover'].forEach((evt) => {
                elements[i].addEventListener(evt, (e) => {

                    if (e.target.getAttribute('data-is-read') != "false") {
                        return;
                    }

                    var messageId = e.target.getAttribute('data-message-id');

                    if (!messageId) {
                        return;
                    }

                    fetch(readUrl, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({ messageId: messageId })
                    }).then(response => response.json())
                        .then(result => {
                            if (result.updated) {
                                if (wrapperSelector) {
                                    var wrapper = e.target.closest(wrapperSelector);
                                    if (wrapper) {
                                        wrapper.classList.remove('notification-is-unread');
                                        wrapper.classList.add('notification-is-read');
                                        wrapper.setAttribute('data-is-read', true);
                                    }
                                } else {
                                    e.target.classList.remove('notification-is-unread');
                                    e.target.classList.add('notification-is-read');
                                    e.target.setAttribute('data-is-read', true);
                                }
                            }

                            var targetUrl = e.target.getAttribute('data-target-url');

                            if (targetUrl) {
                                window.location.href = targetUrl;
                            }
                        });
                });
            })
        }
    }

    return {
        initializeContainer: initialize
    };
}();
