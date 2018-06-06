// We add some classes to the body tag to restore the sidebar to the state is was before reload.
// That state was saved to localstorage by userPreferencesPersistor.js
// We need to apply the classes BEFORE the page is rendered. 
// That is why we use a MutationObserver instead of document.Ready().
var observer = new MutationObserver(function (mutations) {
    for (var i = 0; i < mutations.length; i++) {
        for (var j = 0; j < mutations[i].addedNodes.length; j++) {
            if (mutations[i].addedNodes[j].tagName == 'BODY') {

                var body = mutations[i].addedNodes[j];

                var adminPreferences = JSON.parse(localStorage.getItem('adminPreferences'));
                if (adminPreferences != null) {
                    if (adminPreferences.leftSidebarCompact == true) {
                        body.className += ' left-sidebar-compact';
                    }
                    isCompactExplicit = adminPreferences.isCompactExplicit;
                } else {
                    body.className += ' no-admin-preferences';
                }
                // we're done: 
                observer.disconnect();
            };
        }
    }
});

observer.observe(document.documentElement, {
    childList: true,
    subtree: true
});
