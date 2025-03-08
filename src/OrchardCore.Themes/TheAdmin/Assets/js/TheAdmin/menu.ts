import { isCompactExplicit, setCompactExplicit } from "../constants";
import { persistAdminPreferences } from "./userPreferencesPersistor";
// When we load compact status from preferences we need to do some other tasks besides adding the class to the body.
// UserPreferencesLoader has already added the needed class.
document.addEventListener("DOMContentLoaded", function () {
    // We set leftbar to compact if :
    // 1. That preference was stored by the user the last time he was on the page
    // 2. Or it's the first time on page and page is small.
    //
    if (document.body.classList.contains("left-sidebar-compact") || (document.body.classList.contains("no-admin-preferences") && window.innerWidth < 768)) {
        setCompactStatus(false);
    }
});

const labels = document.querySelectorAll("#left-nav ul.menu-admin > li > figure > figcaption > .item-label");
labels.forEach(function (label) {
    label.parentElement?.previousElementSibling?.setAttribute("title", label.textContent ?? "");
});

const leftbarCompactor = document.querySelector(".leftbar-compactor");
leftbarCompactor?.addEventListener("click", function () {
    document.body.classList.toggle("left-sidebar-compact") ? unSetCompactStatus() : setCompactStatus(true);
});

const leftNav = document.getElementById("left-nav");
const leftNavMenuItems = leftNav?.querySelectorAll("li.has-items");
leftNavMenuItems?.forEach(function (menuItem) {
    menuItem.addEventListener("click", function () {
        for (const item of leftNavMenuItems) {
            item.classList.remove("visible");
        }
        this.classList.add("visible");
    });
});

document.addEventListener("click", function (event) {
    const target = event.target;
    const trigger = document.querySelector("#left-nav li.has-items");
    if (trigger != null && trigger !== target && !trigger?.contains(target)) {
        for (const item of leftNavMenuItems) {
            item.classList.remove("visible");
        }
    }
});

var subMenuArray = new Array();

const setCompactStatus = (explicit: boolean) => {
    // This if is to avoid that when sliding from expanded to compact the
    // underliyng ul is visible while shrinking. It is ugly.
    if (!document.body.classList.contains("left-sidebar-compact")) {
        var labels = document.querySelectorAll("#left-nav ul.menu-admin > li > figure > figcaption > .item-label");
        labels.forEach(function (label) {
            label.style.backgroundColor = "transparent";
        });
        setTimeout(function () {
            labels.forEach(function (label) {
                label.style.backgroundColor = "";
            });
        }, 200);
    }

    document.body.classList.add("left-sidebar-compact");

    // When leftbar is expanded  all ul tags are collapsed.
    // When leftbar is compacted we don't want the first level collapsed.
    // We want it expanded so that hovering over the root buttons shows the full submenu
    const firstLevelMenuItems = leftNav?.querySelectorAll("ul.menu-admin > li > figure > ul");
    firstLevelMenuItems?.forEach(function (menuItem) {
        menuItem.classList.remove("collapse");
    });
    // When hovering, don't want toggling when clicking on label
    const firstLevelMenuLinks = leftNav?.querySelectorAll("ul.menu-admin > li > figure > figcaption > a");
    firstLevelMenuLinks?.forEach(function (menuLink) {
        menuLink.setAttribute("data-bs-toggle", "");
    });
    leftNavMenuItems?.forEach(function (menuItem) {
        menuItem.classList.remove("visible");
    });

    //after menu has collapsed we set the transitions to none so that we don't do any transition
    //animation when open a sub-menu
    setTimeout(function () {
        leftNavMenuItems.forEach(function (menuItem) {
            menuItem.style.transition = "none";
        });
    }, 200);

    if (explicit == true) {
        setCompactExplicit(true);
    }
    persistAdminPreferences();
};

const unSetCompactStatus = () => {
    document.body.classList.remove("left-sidebar-compact");

    // resetting what we disabled for compact state
    const firstLevelMenuItems = leftNav?.querySelectorAll("ul.menu-admin > li > figure > ul");
    firstLevelMenuItems?.forEach(function (menuItem) {
        menuItem.classList.add("collapse");
    });
    const firstLevelMenuLinks = leftNav?.querySelectorAll("ul.menu-admin > li > figure > figcaption > a");
    firstLevelMenuLinks?.forEach(function (menuLink) {
        menuLink.setAttribute("data-bs-toggle", "collapse");
    });
    leftNavMenuItems?.forEach(function (menuItem) {
        menuItem.classList.remove("visible");
    });
    leftNavMenuItems?.forEach(function (menuItem) {
        menuItem.style.transition = "";
    });

    setCompactExplicit(false);
    persistAdminPreferences();
};

// create an Observer instance
const resizeObserver = new ResizeObserver((entries) => {
    if (isCompactExplicit) {
        if (leftNav && leftNav.scrollHeight > leftNav.clientHeight) {
            document.body.classList.add("scroll");
        } else {
            document.body.classList.remove("scroll");
        }
    } else {
        document.body.classList.remove("scroll");
    }
});

// start observing a DOM node
if (leftNav != null) {
    resizeObserver.observe(leftNav);
}

export { setCompactStatus, unSetCompactStatus };
