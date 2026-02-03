(function () {
  // Configuration
  var DOCS_HOSTNAME = 'docs.orchardcore.net';
  // URL to fetch versions from - always points to the latest/main branch version
  // This ensures all documentation versions show the same, up-to-date version list
  var VERSIONS_URL = 'https://' + DOCS_HOSTNAME + '/en/latest/assets/versions.json';

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
   * Fetches the version list from the main branch and sets up event handlers.
   */
  function initVersionSelector() {
    var versionContainer = document.querySelector('.md-version');
    var versionButton = document.querySelector('.md-version__current');
    var versionLabel = document.getElementById('current-version-label');
    var versionList = document.getElementById('version-list');

    if (!versionContainer || !versionButton || !versionList) {
      return;
    }

    // Prevent double initialization
    if (versionContainer.dataset.initialized) {
      return;
    }
    versionContainer.dataset.initialized = 'true';

    // Determine current version from URL
    var currentVersionSlug = determineSelectedBranch();

    // Fetch the version list from the main branch
    fetchVersions(function(versions) {
      if (!versions || !versions.versions) {
        // Fallback: show current version only
        versionLabel.textContent = currentVersionSlug;
        return;
      }

      // Populate the version list
      populateVersionList(versionList, versions.versions, currentVersionSlug, versionLabel);

      // Set up event handlers after populating the list
      setupEventHandlers(versionContainer, versionButton, currentVersionSlug);
    });

    // Set up basic toggle functionality immediately (before fetch completes)
    versionButton.addEventListener('click', function(e) {
      e.preventDefault();
      e.stopPropagation();
      versionContainer.classList.toggle('is-open');
    });

    versionButton.addEventListener('keydown', function(e) {
      if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault();
        versionContainer.classList.toggle('is-open');
      } else if (e.key === 'Escape') {
        versionContainer.classList.remove('is-open');
      }
    });

    document.addEventListener('click', function(e) {
      if (!versionContainer.contains(e.target)) {
        versionContainer.classList.remove('is-open');
      }
    });
  }

  /**
   * Fetches the version list from the main branch.
   * Falls back to local versions if fetch fails.
   *
   * @param {function} callback - Called with the versions data
   */
  function fetchVersions(callback) {
    // For local development, try to use local versions file first
    var isLocal = window.location.origin.indexOf('readthedocs') === -1 &&
                  window.location.hostname !== DOCS_HOSTNAME;

    var url = isLocal ? 'assets/versions.json' : VERSIONS_URL;

    fetch(url)
      .then(function(response) {
        if (!response.ok) {
          throw new Error('Failed to fetch versions');
        }
        return response.json();
      })
      .then(function(data) {
        callback(data);
      })
      .catch(function(error) {
        console.warn('Could not fetch versions from ' + url + ':', error);
        // If fetching from main branch failed and we're not local, no fallback
        callback(null);
      });
  }

  /**
   * Populates the version dropdown list with version items.
   *
   * @param {HTMLElement} listElement - The UL element to populate
   * @param {Array} versions - Array of version objects
   * @param {string} currentVersionSlug - The current version slug
   * @param {HTMLElement} labelElement - The element showing the current version
   */
  function populateVersionList(listElement, versions, currentVersionSlug, labelElement) {
    listElement.innerHTML = '';

    versions.forEach(function(version) {
      var li = document.createElement('li');
      li.className = 'md-version__item';

      var a = document.createElement('a');
      a.href = '#';
      a.className = 'md-version__link';
      a.setAttribute('data-version-slug', version.slug);
      a.setAttribute('data-version-name', version.name);
      a.textContent = version.name;

      if (version.slug === currentVersionSlug) {
        a.classList.add('is-current');
        if (labelElement) {
          labelElement.textContent = version.name;
        }
      }

      li.appendChild(a);
      listElement.appendChild(li);
    });
  }

  /**
   * Sets up event handlers for version link clicks.
   *
   * @param {HTMLElement} versionContainer - The version selector container
   * @param {HTMLElement} versionButton - The toggle button
   * @param {string} currentVersionSlug - The current version slug
   */
  function setupEventHandlers(versionContainer, versionButton, currentVersionSlug) {
    var versionLinks = document.querySelectorAll('.md-version__link');

    versionLinks.forEach(function(link) {
      link.addEventListener('click', function(e) {
        e.preventDefault();
        var targetVersionSlug = link.getAttribute('data-version-slug');

        if (targetVersionSlug === currentVersionSlug) {
          versionContainer.classList.remove('is-open');
          return;
        }

        navigateToVersion(targetVersionSlug);
      });

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
