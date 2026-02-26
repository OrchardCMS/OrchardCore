var contentTypePickerApp;
var contentTypePickerInitialized;
var sharedContentTypePickerModal;

function initializeContentTypePickerApplication(pathBase) {
    if (contentTypePickerInitialized) {
        return;
    }

    // Check if the Vue app element exists in the DOM
    var appElement = document.getElementById("contentTypePickerApp");
    if (!appElement) {
        // Element not found - don't set initialized flag so it can be retried
        return;
    }

    contentTypePickerInitialized = true;

    contentTypePickerApp = new Vue({
        el: "#contentTypePickerApp",
        data: {
            contentTypes: [],
            categories: [],
            searchFilter: "",
            selectedCategory: "All",
            pathBase: pathBase || "",
            currentConfig: null,
            hasError: false,
        },
        computed: {
            filteredContentTypes: function () {
                var self = this;
                return this.contentTypes.filter(function (contentType) {
                    // Filter by category
                    // Items without a category are only shown when "All" is selected
                    if (self.selectedCategory !== "All") {
                        if (!contentType.category || contentType.category !== self.selectedCategory) {
                            return false;
                        }
                    }
                    // Filter by search term
                    if (self.searchFilter) {
                        var searchLower = self.searchFilter.toLowerCase();
                        var nameMatch = contentType.displayName.toLowerCase().indexOf(searchLower) >= 0;
                        var descMatch = contentType.description && contentType.description.toLowerCase().indexOf(searchLower) >= 0;
                        return nameMatch || descMatch;
                    }
                    return true;
                });
            },
        },
        methods: {
            configure: function (options) {
                this.currentConfig = options;
                this.contentTypes = options.contentTypes || [];

                // Build unique categories list
                var categorySet = {};
                this.contentTypes.forEach(function (contentType) {
                    if (contentType.category) {
                        categorySet[contentType.category] = true;
                    }
                });
                this.categories = ["All"].concat(Object.keys(categorySet).sort());

                // Reset filters and error state
                this.searchFilter = "";
                this.selectedCategory = "All";
                this.hasError = false;
            },
            selectCategory: function (category) {
                this.selectedCategory = category;
            },
            selectContentType: function (contentType) {
                if (this.currentConfig && this.currentConfig.onContentTypeSelected) {
                    this.currentConfig.onContentTypeSelected(contentType, this.currentConfig);
                }
            },
            getThumbnailUrl: function (contentType) {
                if (contentType.thumbnail) {
                    // Handle tilde paths
                    if (contentType.thumbnail.startsWith("~/")) {
                        return this.pathBase + contentType.thumbnail.substring(1);
                    }
                    return contentType.thumbnail;
                }
                return null;
            },
        },
    });

    // Set up shared modal
    var modalElement = document.getElementById("contentTypePickerModal");
    if (modalElement) {
        sharedContentTypePickerModal = new bootstrap.Modal(modalElement);

        // Blur focused element before modal hides to prevent aria-hidden warning
        modalElement.addEventListener("hide.bs.modal", function () {
            if (document.activeElement && modalElement.contains(document.activeElement)) {
                document.activeElement.blur();
            }
        });

        // Reset state when modal is hidden
        modalElement.addEventListener("hidden.bs.modal", function () {
            if (contentTypePickerApp) {
                contentTypePickerApp.searchFilter = "";
                contentTypePickerApp.selectedCategory = "All";
                contentTypePickerApp.currentConfig = null;
                contentTypePickerApp.hasError = false;
            }
        });
    }
}

// Remove any duplicate modals that may have been injected via AJAX
function removeDuplicateModals() {
    var modals = document.querySelectorAll("#contentTypePickerModal");
    for (var i = 1; i < modals.length; i++) {
        modals[i].remove();
    }
}

// Show the shared modal with configuration
function showContentTypePicker(config) {
    // Remove any duplicate modals first
    removeDuplicateModals();

    // Try to initialize if not already done (handles late DOM injection)
    if (!contentTypePickerInitialized) {
        initializeContentTypePickerApplication(config.pathBase || "");
    }

    if (contentTypePickerApp && sharedContentTypePickerModal) {
        contentTypePickerApp.configure(config);

        // Update modal title if provided
        if (config.modalTitle) {
            var titleElement = document.getElementById("contentTypePickerModalLabel");

            if (titleElement) {
                titleElement.textContent = config.modalTitle;
            }
        }

        sharedContentTypePickerModal.show();
    }
}

// Hide the shared modal
function hideContentTypePicker() {
    if (sharedContentTypePickerModal) {
        // Blur any focused element to prevent aria-hidden warning
        if (document.activeElement) {
            document.activeElement.blur();
        }
        sharedContentTypePickerModal.hide();
    }
}

// Direct widget creation via AJAX
function contentTypePickerSelectContentType(contentType, config) {
    var targetId = config.targetId;
    var htmlFieldPrefix = config.htmlFieldPrefix;
    var $target = $("#" + targetId);
    var createEditorUrl = $target.data("buildeditorurl");

    // Calculate next prefix index
    var indexes = $target
        .closest("form")
        .find("input[name*='Prefixes']")
        .filter(function (i, e) {
            return $(e).val().substring(0, $(e).val().lastIndexOf("-")) === htmlFieldPrefix;
        })
        .map(function (i, e) {
            return (
                parseInt(
                    $(e)
                        .val()
                        .substring($(e).val().lastIndexOf("-") + 1),
                ) || 0
            );
        });

    var index = indexes.length ? Math.max(...indexes) + 1 : 0;
    var prefix = htmlFieldPrefix + "-" + index.toString();

    // Build URL with proper encoding
    var params = new URLSearchParams({
        id: contentType.name,
        prefix: prefix,
        prefixesName: config.prefixesName,
        contentTypesName: config.contentTypesName,
        contentItemsName: config.contentItemsName,
        targetId: targetId,
        flowmetadata: config.flowmetadata || false,
        parentContentType: config.parentContentType,
        partName: config.partName,
    });

    // Add optional cardCollectionType
    if (config.cardCollectionType) {
        params.append("cardCollectionType", config.cardCollectionType);
    }

    $.ajax({
        url: createEditorUrl + "?" + params.toString(),
    }).done(function (data) {
        var result = JSON.parse(data);

        // Insert before element if specified, otherwise append to target
        if (config.insertBefore) {
            $(result.Content).insertBefore(config.insertBefore);
        } else {
            $target.append(result.Content);
        }

        // Execute scripts
        $(result.Scripts)
            .filter("script")
            .each(function () {
                $.globalEval(this.text || this.textContent || this.innerHTML || "");
            });

        hideContentTypePicker();
    }).fail(function () {
        if (contentTypePickerApp) {
            contentTypePickerApp.hasError = true;
        }
    });
}
