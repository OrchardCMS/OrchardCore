notificationManager = function () {

    const initialize = (readUrl, wrapperSelector) => {

        if (!readUrl) {
            console.log('No readUrl was provided.');

            return;
        }

        var elements = document.getElementsByClassName('mark-notification-as-read');

        for (let i = 0; i < elements.length; i++) {
            let element = elements[i];
            element.addEventListener('click', () => {

                if (element.getAttribute('data-is-read') != "false") {
                    return;
                }

                var messageId = element.getAttribute('data-message-id');

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
                                var wrapper = element.closest(wrapperSelector);
                                if (wrapper) {
                                    wrapper.classList.remove('notification-is-unread');
                                    wrapper.classList.add('notification-is-read');
                                    wrapper.setAttribute('data-is-read', true);
                                }
                            } else {
                                element.classList.remove('notification-is-unread');
                                element.classList.add('notification-is-read');
                                element.setAttribute('data-is-read', true);
                            }
                        }

                        var targetUrl = element.getAttribute('data-target-url');

                        if (targetUrl) {
                            window.location.href = targetUrl;
                        }
                    });
            });
        }
    }

    return {
        initializeContainer: initialize
    };
}();
