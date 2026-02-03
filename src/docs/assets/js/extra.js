(function () {
  // Configuration
  var DOCS_HOSTNAME = 'docs.orchardcore.net';

  // Initialize when DOM is ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initVersionSelector);
  } else {
    // DOM is already ready
    initVersionSelector();
  }

  // ReadTheDocs-specific search fix - only runs if jQuery is available (loaded by RTD)
  if (typeof $ !== 'undefined') {
    $(document).ready(function () {
      fixSearch();
    });
  }

  /*
   * RTD messes up MkDocs' search feature by tinkering with the search box defined in the theme, see
   * https://github.com/rtfd/readthedocs.org/issues/1088. This function sets up a DOM4 MutationObserver
   * to react to changes to the search form (triggered by RTD on doc ready). It then reverts everything
   * the RTD JS code modified.
   */
  function fixSearch() {
    var target = document.getElementById('mkdocs-search-form');
    if (!target) return;

    var config = {attributes: true, childList: true};

    var observer = new MutationObserver(function(mutations) {
      // if it isn't disconnected it'll loop infinitely because the observed element is modified
      observer.disconnect();
      var form = $('#mkdocs-search-form');
      form.empty();
      form.attr('action', 'https://' + window.location.hostname + '/en/' + determineSelectedBranch() + '/search.html');
      $('<input>').attr({
        type: "text",
        name: "q",
        placeholder: "Search docs"
      }).appendTo(form);
    });

    if (window.location.origin.indexOf('readthedocs') > -1) {
      observer.observe(target, config);
    }
  }

  /**
   * Analyzes the URL of the current page to find out what the selected GitHub branch is. It's usually
   * part of the location path. The code needs to distinguish between running MkDocs standalone
   * and docs served from RTD. If no valid branch could be determined 'latest' returned.
   *
   * @returns GitHub branch name
   */
  function determineSelectedBranch() {
    var branch = 'latest', path = window.location.pathname;
    if (window.location.origin.indexOf('readthedocs') > -1) {
      // path is like /en/<branch>/<lang>/build/ -> extract 'lang'
      // split[0] is an '' because the path starts with the separator
      var thirdPathSegment = path.split('/')[2];
      if (thirdPathSegment != 'latest') {
        branch = thirdPathSegment;
      }
    }
    return branch;
  }

  /**
   * Initializes the version selector dropdown functionality.
   * Handles opening/closing the dropdown and switching between versions.
   */
  function initVersionSelector() {
    var versionContainer = document.querySelector('.md-version');
    var versionButton = document.querySelector('.md-version__current');
    var versionLabel = document.getElementById('current-version-label');
    var versionLinks = document.querySelectorAll('.md-version__link');

    if (!versionContainer || !versionButton) {
      return;
    }

    // Prevent double initialization
    if (versionContainer.dataset.initialized) {
      return;
    }
    versionContainer.dataset.initialized = 'true';

    // Determine current version from URL
    var currentVersionSlug = determineSelectedBranch();

    // Update the label and mark the current version
    versionLinks.forEach(function(link) {
      var slug = link.getAttribute('data-version-slug');
      var name = link.getAttribute('data-version-name');

      if (slug === currentVersionSlug) {
        link.classList.add('is-current');
        if (versionLabel) {
          versionLabel.textContent = name;
        }
      }
    });

    // Toggle dropdown on button click
    versionButton.addEventListener('click', function(e) {
      e.preventDefault();
      e.stopPropagation();
      versionContainer.classList.toggle('is-open');
    });

    // Handle keyboard navigation
    versionButton.addEventListener('keydown', function(e) {
      if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault();
        versionContainer.classList.toggle('is-open');
      } else if (e.key === 'Escape') {
        versionContainer.classList.remove('is-open');
      }
    });

    // Close dropdown when clicking outside
    document.addEventListener('click', function(e) {
      if (!versionContainer.contains(e.target)) {
        versionContainer.classList.remove('is-open');
      }
    });

    // Handle version link clicks
    versionLinks.forEach(function(link) {
      link.addEventListener('click', function(e) {
        e.preventDefault();
        var targetVersionSlug = link.getAttribute('data-version-slug');

        if (targetVersionSlug === currentVersionSlug) {
          // Already on this version, just close the dropdown
          versionContainer.classList.remove('is-open');
          return;
        }

        // Navigate to the same page on the selected version
        navigateToVersion(targetVersionSlug);
      });

      // Keyboard support for links
      link.addEventListener('keydown', function(e) {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          link.click();
        }
      });
    });
  }

  /**
   * Navigates to the same page on a different documentation version.
   * Preserves the current page path when switching versions.
   *
   * @param {string} targetVersionSlug - The version slug to navigate to
   */
  function navigateToVersion(targetVersionSlug) {
    var currentPath = window.location.pathname;
    var currentVersionSlug = determineSelectedBranch();
    var newPath;

    if (window.location.origin.indexOf('readthedocs') > -1) {
      // On ReadTheDocs: URL format is /en/<version>/path/to/page/
      // Replace the version segment in the URL
      var pathParts = currentPath.split('/');
      // pathParts[0] = '', pathParts[1] = 'en', pathParts[2] = version
      if (pathParts.length >= 3) {
        pathParts[2] = targetVersionSlug;
        newPath = pathParts.join('/');
      } else {
        // Fallback to root of target version
        newPath = '/en/' + targetVersionSlug + '/';
      }
    } else {
      // Local development or standalone MkDocs
      // Redirect to the target version's root on RTD
      newPath = 'https://' + DOCS_HOSTNAME + '/en/' + targetVersionSlug + '/';
      window.location.href = newPath;
      return;
    }

    window.location.href = window.location.origin + newPath;
  }
}());
