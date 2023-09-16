// We add some classes to the body tag to restore the sidebar to the state is was before reload.
// That state was saved to localstorage by userPreferencesPersistor.js
// We need to apply the classes BEFORE the page is rendered. 
// That is why we use a MutationObserver instead of document.Ready().
const observer = new MutationObserver(function (mutations) {
    const html = document.documentElement || document.body;
    const tenant = html.getAttribute('data-tenant');
    const key = tenant + '-adminPreferences';
    let adminPreferences = JSON.parse(localStorage.getItem(key));

    for (let i = 0; i < mutations.length; i++) {
        for (let j = 0; j < mutations[i].addedNodes.length; j++) {

            if (mutations[i].addedNodes[j].tagName == 'BODY') {
                let body = mutations[i].addedNodes[j];

                if (adminPreferences != null && adminPreferences.leftSidebarCompact == true) {
                    body.classList.add('left-sidebar-compact');
                }

                observer.disconnect();
            };
        }
    }
});

observer.observe(document.documentElement || document.body, {
    childList: true,
    subtree: true
});
