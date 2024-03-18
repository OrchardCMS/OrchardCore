// We add some classes to the body tag to restore the sidebar to the state is was before reload.
// That state was saved to localstorage by userPreferencesPersistor.js
// We need to apply the classes BEFORE the page is rendered.
// That is why we use a MutationObserver instead of document.Ready().
let isCompactExplicit = false;

const observer = new MutationObserver(function (mutations) {
    for (let i = 0; i < mutations.length; i++) {
        for (let j = 0; j < mutations[i].addedNodes.length; j++) {

            if (mutations[i].addedNodes[j].tagName == 'BODY') {
                let body = mutations[i].addedNodes[j];
                let adminPreferences = getAdminPreferences();

                if (adminPreferences) {
                    isCompactExplicit = adminPreferences.isCompactExplicit;
                    if (adminPreferences != null && adminPreferences.leftSidebarCompact == true) {
                        body.classList.add('left-sidebar-compact');
                    }
                }

                observer.disconnect();
            };
        }
    }
});

observer.observe(document.documentElement, {
    childList: true,
    subtree: true
});
