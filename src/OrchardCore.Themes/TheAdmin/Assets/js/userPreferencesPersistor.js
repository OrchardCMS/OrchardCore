// Each time the sidebar status is modified, that is persisted to localStorage.
// When the page is loaded again, userPreferencesLoader.js will read that info to 
// restore the sidebar to the previous state.
function persistAdminPreferences() {
    setTimeout(function () {
        var adminPreferences = {};
        adminPreferences.leftSidebarCompact = document.body.classList.contains('left-sidebar-compact') ? true : false;
        adminPreferences.isCompactExplicit = isCompactExplicit;
        setAdminPreferences(adminPreferences);
    }, 200);
}
