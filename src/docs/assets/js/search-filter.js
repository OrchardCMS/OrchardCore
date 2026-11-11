// Lets readers search the User Manual, the Technical Manual or both, with the checkboxes added to the search panel in
// theme/partials/search.html. The choice is kept in the browser.
(function () {
  "use strict";

  var storageKey = "oc-search-manuals";
  var names = { user: "User Manual", technical: "Technical Manual" };

  function load() {
    try {
      var value = JSON.parse(localStorage.getItem(storageKey));
      if (value && (value.user || value.technical)) {
        return { user: !!value.user, technical: !!value.technical };
      }
    } catch (e) {
      // Storage can be unavailable, for example in a private window.
    }

    return { user: true, technical: true };
  }

  function save(state) {
    try {
      localStorage.setItem(storageKey, JSON.stringify(state));
    } catch (e) {
      // Ignore: the choice then lasts for this page only.
    }
  }

  var state = load();

  function manualOf(link) {
    try {
      return new URL(link.getAttribute("href"), location.href).pathname.indexOf("/user-manual/") !== -1 ? "user" : "technical";
    } catch (e) {
      return "technical";
    }
  }

  function apply() {
    document.querySelectorAll("[data-oc-manual]").forEach(function (checkbox) {
      checkbox.checked = state[checkbox.getAttribute("data-oc-manual")];
    });

    var note = document.querySelector(".oc-search-filter__note");
    var text = state.user && state.technical ? "" : "Showing " + names[state.user ? "user" : "technical"] + " results only";
    // Only write when it changes: writing the text is itself a change the observer below reacts to.
    if (note && note.textContent !== text) {
      note.textContent = text;
    }

    var list = document.querySelector(".md-search-result__list");
    if (!list) {
      return;
    }

    list.querySelectorAll(":scope > .md-search-result__item").forEach(function (item) {
      var link = item.querySelector("a[href]");
      var visible = state[link ? manualOf(link) : "technical"];
      item.classList.toggle("oc-search-result--hidden", !visible);
    });

    // Material renders more results when the list is scrolled to its end. Hidden results can leave the list too short
    // to scroll, so ask for the next results until the panel is filled or there are no more.
    var scrollwrap = list.closest(".md-search__scrollwrap");
    if (scrollwrap && scrollwrap.scrollHeight <= scrollwrap.clientHeight + 16) {
      scrollwrap.dispatchEvent(new Event("scroll"));
    }
  }

  document.addEventListener("change", function (event) {
    var checkbox = event.target;
    if (!checkbox.matches || !checkbox.matches("[data-oc-manual]")) {
      return;
    }

    var manual = checkbox.getAttribute("data-oc-manual");
    state[manual] = checkbox.checked;

    // Keep at least one manual: the last ticked box stays ticked.
    if (!state.user && !state.technical) {
      state[manual] = true;
    }

    save(state);
    apply();
  });

  var scheduled = false;
  new MutationObserver(function (mutations) {
    if (scheduled) {
      return;
    }

    for (var i = 0; i < mutations.length; i++) {
      var target = mutations[i].target;
      if (target.nodeType === 1 && (target.matches(".md-search-result__list, .md-search-result__list *, .md-search__output, .md-search__output *"))) {
        scheduled = true;
        requestAnimationFrame(function () {
          scheduled = false;
          apply();
        });
        return;
      }
    }
  }).observe(document.body, { childList: true, subtree: true });

  apply();
})();
