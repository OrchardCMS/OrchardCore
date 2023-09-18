// Each time the sidebar status is modified, that is persisted to localStorage.
// When the page is loaded again, userPreferencesLoader.js will read that info to 
// restore the sidebar to the previous state.
function persistAdminPreferences(theme) {
    setTimeout(function () {
        var adminPreferences = {};
        adminPreferences.leftSidebarCompact = document.body.classList.contains('left-sidebar-compact') ? true : false;
        adminPreferences.isCompactExplicit = isCompactExplicit;
        adminPreferences.darkMode = (theme || document.documentElement.getAttribute('data-bs-theme')) === darkThemeName ? true : false;
        setAdminPreferences(adminPreferences);
    }, 200);
}
