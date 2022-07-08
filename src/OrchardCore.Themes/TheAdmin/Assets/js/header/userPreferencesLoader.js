// We add some classes to the body tag to restore the sidebar to the state is was before reload.
// That state was saved to localstorage by userPreferencesPersistor.js
// We need to apply the classes BEFORE the page is rendered. 
// That is why we use a MutationObserver instead of document.Ready().
var observer = new MutationObserver(function (mutations) {
    var html = document.querySelector("html");
    var tenant = html.getAttribute('data-tenant');
    var key = tenant + '-adminPreferences';
    var adminPreferences = JSON.parse(localStorage.getItem(key));

    for (var i = 0; i < mutations.length; i++) {
        for (var j = 0; j < mutations[i].addedNodes.length; j++) {
            
            if (mutations[i].addedNodes[j].tagName == 'BODY') {
                var body = mutations[i].addedNodes[j];

                if (adminPreferences != null) {
                    if (adminPreferences.leftSidebarCompact == true) {
                        body.classList.add('left-sidebar-compact');
                    }
                    isCompactExplicit = adminPreferences.isCompactExplicit;

                    if(html.getAttribute('data-darkmode') === 'True')
                    {
                        if (adminPreferences.darkMode){
                            html.setAttribute('data-theme', 'darkmode');
                        }
                        else
                        {
                            html.setAttribute('data-theme', 'default');
                        }
                    }
                } 
                else 
                {
                    body.classList.add('no-admin-preferences');

                    if(html.getAttribute('data-darkmode') === 'True')
                    {
                        // Automatically sets darkmode based on OS preferences
                        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches)
                        {
                            html.setAttribute('data-theme', 'darkmode');
                        }
                        else
                        {
                            html.setAttribute('data-theme', 'default');
                        }
                    }
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
