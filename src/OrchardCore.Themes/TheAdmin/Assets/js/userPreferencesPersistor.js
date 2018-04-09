// Each time the sidebar status is modified, that is persisted to localStorage.
// When the page is loaded again, userPreferencesLoader.js will read that info to 
// restore the sidebar to the previous state.
function persistAdminPreferences() {
    setTimeout(function () {
        var adminPreferences = {};        
        adminPreferences.leftSidebarHidden = $('body').hasClass('left-sidebar-hidden') ? true : false;
        adminPreferences.leftbarVisibleOnSmall = $('body').hasClass('leftbar-visible-on-small') ? true : false;
        adminPreferences.leftSidebarCompact = $('body').hasClass('left-sidebar-compact') ? true : false;
        
        localStorage.setItem('adminPreferences', JSON.stringify(adminPreferences));
    }, 200);
}
