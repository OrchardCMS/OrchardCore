// We add some classes to the body tag to restore the sidebar to the state is was before reload.
// That state was saved to localstorage by userPreferencesPersistor.js
// We need to apply the classes BEFORE the page is rendered. 
// That is why we use a MutationObserver instead of document.Ready().
var observer = new MutationObserver(function (mutations) {
    var adminPreferences = JSON.parse(localStorage.getItem('adminPreferences'));

    for (var i = 0; i < mutations.length; i++) {
        for (var j = 0; j < mutations[i].addedNodes.length; j++) {
            
            if (mutations[i].addedNodes[j].tagName == 'BODY') {

                var body = mutations[i].addedNodes[j];
                var defaultCSS = document.getElementById('admin-default');
                var darkModeCSS = document.getElementById('admin-darkmode');
                var btnDarkMode = document.getElementById('btn-darkmode');

                if (adminPreferences != null) {
                    if (adminPreferences.leftSidebarCompact == true) {
                        body.classList.add('left-sidebar-compact');
                    }
                    isCompactExplicit = adminPreferences.isCompactExplicit;

                    if (darkModeCSS) {
                        if (adminPreferences.darkMode){
                            darkModeCSS.setAttribute('media', 'all');
                            defaultCSS.setAttribute('media', 'not all');
                            body.classList.add('darkmode');
                            
                            btnDarkMode.firstChild.classList.remove('fa-moon');
                            btnDarkMode.firstChild.classList.add('fa-sun');
                        }
                        else
                        {
                            darkModeCSS.setAttribute('media', 'not all');
                            defaultCSS.setAttribute('media', 'all');
                            body.classList.remove('darkmode');

                            btnDarkMode.firstChild.classList.remove('fa-sun');
                            btnDarkMode.firstChild.classList.add('fa-moon');
                        }
                    }
                } else {
                    body.classList.add('no-admin-preferences');

                    if(darkModeCSS) {
                        // Automatically sets darkmode based on OS preferences
                        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches)
                        {
                            darkModeCSS.setAttribute('media', 'all');
                            defaultCSS.setAttribute('media', 'not all');
                            body.classList.add('darkmode');

                            btnDarkMode.firstChild.classList.remove('fa-moon');
                            btnDarkMode.firstChild.classList.add('fa-sun');
                        }
                        else
                        {
                            body.classList.remove('darkmode');

                            btnDarkMode.firstChild.classList.remove('fa-sun');
                            btnDarkMode.firstChild.classList.add('fa-moon');
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
