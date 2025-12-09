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
                    if (self.selectedCategory !== "All" && contentType.category !== self.selectedCategory) {
                        return false;
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
            getPreviewImageUrl: function (contentType) {
                if (contentType.previewImage) {
                    // Handle tilde paths
                    if (contentType.previewImage.startsWith("~/")) {
                        return this.pathBase + contentType.previewImage.substring(1);
                    }
                    return contentType.previewImage;
                }
                return this.pathBase + "/OrchardCore.Flows/Images/content-type-placeholder.png";
            },
            handleImageError: function (event, contentType) {
                // Replace the broken image with a placeholder div
                var img = event.target;
                var placeholder = document.createElement("div");
                placeholder.className = "card-img-top img-placeholder";
                placeholder.innerHTML = '<i class="fa fa-cube" aria-hidden="true"></i>';
                img.parentNode.replaceChild(placeholder, img);
            },
        },
    });

    // Set up shared modal
    var modalElement = document.getElementById('contentTypePickerModal');
    if (modalElement) {
        sharedContentTypePickerModal = new bootstrap.Modal(modalElement);

        // Reset state when modal is hidden
        modalElement.addEventListener('hidden.bs.modal', function () {
            if (contentTypePickerApp) {
                contentTypePickerApp.searchFilter = '';
                contentTypePickerApp.selectedCategory = 'All';
                contentTypePickerApp.currentConfig = null;
            }
        });
    }
}

// Show the shared modal with configuration
function showContentTypePicker(config) {
    if (contentTypePickerApp && sharedContentTypePickerModal) {
        contentTypePickerApp.configure(config);
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
        .data("flowmetadata", false)
        .data("parent-content-type", config.parentContentType)
        .data("part-name", config.partName);

    // Append to body so delegated event handler can find it, trigger click, then remove
    $trigger.appendTo('body').trigger("click").remove();

    // Close the shared modal
    hideContentTypePicker();
}
