var contentTypePickerApp;
var contentTypePickerInitialized;
var sharedContentTypePickerModal;

function initializeContentTypePickerApplication(pathBase) {
    if (contentTypePickerInitialized) {
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

                // Reset filters
                this.searchFilter = "";
                this.selectedCategory = "All";
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

        // Reset state when modal is hidden
        modalElement.addEventListener("hidden.bs.modal", function () {
            if (contentTypePickerApp) {
                contentTypePickerApp.searchFilter = "";
                contentTypePickerApp.selectedCategory = "All";
                contentTypePickerApp.currentConfig = null;
            }
        });
    }
}

// Show the shared modal with configuration
function showContentTypePicker(config) {
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
        sharedContentTypePickerModal.hide();
    }
}

// Integration function to trigger existing add-widget functionality
function contentTypePickerSelectContentType(contentType, config) {
    // Create a synthetic element with the required data attributes
    var $trigger = $('<a class="add-widget" style="display:none;">')
        .data("target-id", config.targetId)
        .data("html-field-prefix", config.htmlFieldPrefix)
        .data("prefixes-name", config.prefixesName)
        .data("contenttypes-name", config.contentTypesName)
        .data("contentitems-name", config.contentItemsName)
        .data("widget-type", contentType.name)
        .data("flowmetadata", config.flowmetadata || false)
        .data("parent-content-type", config.parentContentType)
        .data("part-name", config.partName);

    // Append to body so delegated event handler can find it, trigger click, then remove
    $trigger.appendTo("body").trigger("click").remove();

    // Close the shared modal
    hideContentTypePicker();
}
