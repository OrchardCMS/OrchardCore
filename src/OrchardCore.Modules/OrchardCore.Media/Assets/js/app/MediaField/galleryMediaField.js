const MEDIA_FIELD_GALLERY = "mediaFieldGallery";

Vue.component("mediaFieldGalleryListItem", {
    template:
    /*html*/
    `
        <li class="list-group-item d-flex p-0 overflow-hidden align-items-center" v-if="!media.isRemoved" :class="media.mediaPath=='not-found' ? 'text-danger' : ''">
            <div class="media-preview flex-shrink-0">
                <img
                    v-if="media.mime.startsWith('image')"
                    :src="buildMediaUrl(media.url, media.anchor)"
                    :data-mime="media.mime"
                    class="w-100 object-fit-scale"
                />
                <i v-else-if="media.mediaPath=='not-found'" :title="media.name" class="fa-solid fa-triangle-exclamation"></i>
                <i v-else :class="$parent.getfontAwesomeClassNameForFileName(media.name, 'fa-4x card-text')" :data-mime="media.mime"></i>
            </div>
            <div class="me-auto flex-shrink-1">
                <span v-if="media.mediaPath=='not-found'" class="media-filename card-text small">{{ $parent.T.mediaNotFound }} - {{ $parent.T.discardWarning }}</span>
                <span v-else class="media-filename card-text small" :title="media.name">{{ media.name }}</span>
            </div>
            <div class="media-field-gallery-list-actions flex-shrink-0">
                <a
                    v-show="allowMediaText && media.mediaPath!=='not-found'"
                    class="btn btn-light btn-sm inline-media-button view-button"
                    v-on:click.prevent.stop="$parent.showMediaTextModal(media)"
                    href="javascript:;"
                    title="Edit media text"
                >
                    <span v-show="!media.mediaText">
                        <i class="far fa-comment"></i>
                    </span>
                    <span v-show="media.mediaText">
                        <i class="fa-solid fa-comment"></i>
                    </span>
                </a>
                <a
                    href="javascript:;"
                    v-show="allowAnchors && media.mime.startsWith('image') && media.mediaPath!=='not-found'"
                    v-on:click="$parent.showAnchorModal(media)"
                    class="btn btn-light btn-sm inline-media-button view-button"
                    title="Set anchor"
                >
                    <i class="fa-solid fa-crosshairs" aria-hidden="true"></i>
                </a>
                <a
                    :href="media.url"
                    target="_blank"
                    v-show="media.mediaPath!=='not-found'"
                    class="btn btn-light btn-sm inline-media-button view-button"
                    title="View media"
                >
                    <i class="fa-solid fa-download" aria-hidden="true"></i>
                </a>
                <a
                    href="javascript:;"
                    class="btn btn-light btn-sm inline-media-button view-button"
                    v-on:click.stop="$parent.selectAndDeleteMedia(media)"
                    title="Remove media"
                >
                    <i class="fa-solid fa-trash" aria-hidden="true"></i>
                </a>
            </div>
        </li>
    `,
    props: {
        media: Object,
        canAddMedia: Boolean,
        allowMediaText: Boolean,
        allowAnchors: Boolean,
        cols: Number,
    },
    methods: {
        fileSize: function fileSize(rawSize) {
            return Math.round(rawSize / 1024);
        },
        buildMediaUrl: function buildMediaUrl(url) {
            return url + (url.indexOf("?") == -1 ? "?" : "&") + "width=32&height=32";
        },
    },
});

Vue.component("mediaFieldGalleryCardItem", {
    template:
    /*html*/
    `
        <li class="media-field-gallery-item" v-if="!media.isRemoved">
            <div class="card ratio ratio-1x1 overflow-hidden" :class="media.mediaPath=='not-found' ? 'text-danger border-danger' : ''">
                <div class="d-flex flex-column h-100">
                    <div class="flex-grow-1 media-preview d-flex justify-content-center align-items-center">
                        <div class="update-media" v-if="!$parent.allowMultiple" v-on:click="$parent.showMediaModal">
                            + Media Library
                        </div>
                        <div class="image-wrapper" v-if="media.mime.startsWith('image')">
                            <img
                                :src="buildMediaUrl(media.url)"
                                :data-mime="media.mime"
                                class="w-100 h-100 object-fit-scale"
                            />
                        </div>
                        <div v-else-if="media.mediaPath=='not-found'" class="d-flex flex-column justify-content-center align-items-center h-100 bg-body file-icon not-found" :title="media.name">
                            <i class="fa-solid fa-triangle-exclamation fa-2x card-text'"></i>
                            <span class="card-text small pt-2" :title="media.name">{{ $parent.T.mediaNotFound }}</span>
                            <span class="card-text small pt-2 px-2 text-center" :title="media.name">{{ $parent.T.discardWarning }}</span>
                        </div>
                        <div v-else class="d-flex flex-column justify-content-center align-items-center h-100 bg-body file-icon">
                            <i :class="$parent.getfontAwesomeClassNameForFileName(media.name, 'fa-4x card-text')" :data-mime="media.mime"></i>
                            <span class="media-filename card-text small pt-2" :title="media.name">{{ media.name }}</span>
                        </div>
                    </div>

                    <div class="media-field-gallery-card-actions flex-shrink-0">
                        <a
                            v-show="allowMediaText && media.mediaPath!=='not-found'"
                            class="btn btn-light btn-sm inline-media-button view-button"
                            v-on:click.prevent.stop="$parent.showMediaTextModal(media)"
                            href="javascript:;"
                            title="Edit media text"
                        >
                            <span v-show="!media.mediaText">
                                <i class="far fa-comment"></i>
                            </span>
                            <span v-show="media.mediaText">
                                <i class="fa-solid fa-comment"></i>
                            </span>
                        </a>
                        <a
                            href="javascript:;"
                            v-show="allowAnchors && media.mime.startsWith('image') && media.mediaPath!=='not-found'"
                            v-on:click="$parent.showAnchorModal(media)"
                            class="btn btn-light btn-sm inline-media-button view-button"
                            title="Set anchor"
                        >
                            <i class="fa-solid fa-crosshairs" aria-hidden="true"></i>
                        </a>
                        <a
                            :href="media.url"
                            target="_blank"
                            v-show="media.mediaPath!=='not-found'"
                            class="btn btn-light btn-sm inline-media-button view-button"
                            title="View media"
                        >
                            <i class="fa-solid fa-download" aria-hidden="true"></i>
                        </a>
                        <a
                            href="javascript:;"
                            class="btn btn-light btn-sm inline-media-button view-button"
                            v-on:click.stop="$parent.selectAndDeleteMedia(media)"
                            title="Remove media"
                        >
                            <i class="fa-solid fa-trash" aria-hidden="true"></i>
                        </a>
                    </div>
                </div>
            </div>
        </li>
    `,
    props: {
        media: Object,
        canAddMedia: Boolean,
        allowAnchors: Boolean,
        allowMediaText: Boolean,
    },
    methods: {
        fileSize: function fileSize(rawSize) {
            return Math.round(rawSize / 1024);
        },
        buildMediaUrl: function buildMediaUrl(url) {
            return (url + (url.indexOf("?") == -1 ? "?" : "&") + "width=240&height=240");
        },
    },
});

Vue.component("mediaFieldGalleryContainer", {
    template:
    /*html*/
    `
        <div class="media-field-gallery" :id="idPrefix + '_mediaContainerMain'" v-cloak>
            <div v-if="allowMultiple" class="pb-2" v-cloak>
                <div class="me-auto">
                    <a
                        type="button"
                        class="btn btn-sm btn-primary"
                        href="javascript:;"
                        v-show="$parent.canAddMedia"
                        v-on:click="$parent.showModal"
                    >
                        <i class="fa-solid fa-plus" aria-hidden="true"></i>
                        Media Library
                    </a>
                    <a
                        type="button"
                        class="btn btn-sm btn-secondary draft me-2"
                        :class="selectedItemCount == 0 ? 'disabled' : ''"
                        :disabled="selectedItemCount == 0"
                        href="javascript:;"
                        v-on:click="deselectAll"
                    >
                        Deselect All
                    </a>

                    <div class="btn-group">
                        <button type="button" class="btn btn-sm" :class="!gridView ? 'text-primary': ''" v-on:click="gridView = false;">
                            <span title="Gridview"></span>
                            <i class="fa-solid fa-th-list"></i>
                        </button>
                        <button type="button" class="btn btn-sm" :class="gridView ? 'text-primary': ''" v-on:click="gridView = true;">
                            <span title="List"></span>
                            <i class="fa-solid fa-th-large"></i>
                        </button>
                        <button type="button" class="btn btn-sm" :class="size == 'sm' ? 'text-primary': ''" v-on:click="size = 'sm';" v-show="gridView">
                            <span title="Small Thumbs"></span>
                            <i class="fa-solid fa-compress"></i>
                        </button>
                        <button type="button" class="btn btn-sm" :class="size == 'lg' ? 'text-primary': ''" v-on:click="size = 'lg';" v-show="gridView">
                            <span title="Large Thumbs"></span>
                            <i class="fa-solid fa-expand"></i>
                        </button>
                    </div>
                </div>
            </div>

            <ol ref="mediaContainer" v-if="!gridView" :class="'media-field-gallery-list list-group list-unstyled size-' + size">
                <media-field-gallery-list-item
                    v-for= "media in mediaItemsNoDuplicates"
                    :key="media.vuekey ?? media.name"
                    :media="media"
                    :canAddMedia="$parent.canAddMedia"
                    :allowMediaText="$parent.allowMediaText"
                    :allowAnchors="$parent.allowAnchors"
                    :size="size"
                />

                <li v-if="!mediaItems || mediaItems.length < 1" class="list-group-item text-center hint media-field-gallery-choose-btn" v-on:click="showMediaModal">
                    + Media Library
                </li>
            </ol>

            <ol ref="mediaContainer" v-if="gridView" :class="'media-field-gallery-cards list-unstyled size-' + size">
                <media-field-gallery-card-item
                    v-for= "media in mediaItemsNoDuplicates"
                    :key="media.vuekey ?? media.name"
                    :media="media"
                    :canAddMedia="$parent.canAddMedia"
                    :allowMediaText="$parent.allowMediaText"
                    :allowAnchors="$parent.allowAnchors"
                    :size="size"
                />

                <li class="add-media-card" v-if="allowMultiple || mediaItems.length < 1" v-on:click="showMediaModal">
                    <div class="card media-field-gallery-choose-btn text-center overflow-hidden">
                        <div class="ratio ratio-1x1 text-center">
                            <div class="hint d-flex align-items-center justify-content-center w-100">+ Media Library</div>
                        </div>
                    </div>
                </li>
            </ol>
        </div>
    `,
    data: function data() {
        return {
            T: {},
            sortableInstance: null,
            multiSelectedItems: [],
            selectedItemCount: 0,
            gridView: true,
            size: "lg",
        };
    },
    props: {
        mediaItems: Array,
        selectedMedia: Object,
        idPrefix: String,
        modalId: String,
        allowMultiple: Boolean
    },
    created: function created() {
        var self = this;

        // retrieving localized strings from view
        self.T.mediaNotFound = $("#t-media-not-found").val();
        self.T.discardWarning = $("#t-discard-warning").val();
        self.T.noImages = $("#t-no-images").val();
    },
    mounted: function mounted() {
        this.getLocalStorageState();
        this.initSortable();
    },
    beforeDestroy: function beforeDestroy() {
        if (this.sortableInstance) {
            this.sortableInstance.destroy();
        }
    },
    watch: {
        mediaItems: {
            handler() {
                if (this.sortableInstance) {
                    this.sortableInstance.destroy();
                }
                this.$nextTick(() => {
                    this.initSortable();
                    // as this component is now responsible for inforcing one item only when allowMultiple is false, we emit the last item in the list
                    if (!this.allowMultiple && this.mediaItems.length > 1) {
                        this.$emit("updated", [this.mediaItems[this.mediaItems.length - 1]]);
                    }
                });
            },
            deep: true,
        },
        size: {
            handler() {
                this.setLocalStorageState();
            },
        },
        gridView: {
            handler() {
                this.setLocalStorageState();
            },
        },
    },
    computed: {
        mediaItemsNoDuplicates: function mediaItemsNoDuplicates() {
          return this.removeDuplicates(this.mediaItems);
        }
    },
    methods: {
        getLocalStorageState: function getLocalStorageState() {
            if (localStorage.getItem(MEDIA_FIELD_GALLERY)) {
                try {
                    const state = JSON.parse(localStorage.getItem(MEDIA_FIELD_GALLERY));
                    this.size = state.size || "lg";
                    this.gridView = state.gridView ?? false;
                } catch (e) {
                    localStorage.removeItem("cats");
                }
            }
        },
        setLocalStorageState: function setLocalStorageState() {
            const parsed = JSON.stringify({
                size: this.size,
                gridView: this.gridView,
            });
            localStorage.setItem(MEDIA_FIELD_GALLERY, parsed);
        },
        initSortable: function initSortable() {
            if (this.allowMultiple && this.$refs.mediaContainer && this.mediaItemsNoDuplicates.length > 0) {
                var self = this;
                this.sortableInstance = Sortable.create(this.$refs.mediaContainer, {
                    animation: 150,
                    ghostClass: "sortable-ghost",
                    multiDrag: true,
                    selectedClass: "sortable-selected",
                    filter: "a, button, .add-media-card",
                    preventOnFilter: false,
                    onSelect: () => this.updateSelectedCount(),
                    onDeselect: () => this.updateSelectedCount(),
                    onEnd: () => this.updateSelectedCount(),
                    onMove: function (evt) {
                        // Prevent moving the .add-media-card or placing anything after it
                        const dragged = evt.dragged;
                        const related = evt.related;

                        // If trying to move the "add" card or drop something after it
                        if (
                            dragged.classList.contains("add-media-card") ||
                            related.classList.contains("add-media-card")
                        ) {
                            return false; // Cancel the move
                        }

                        return true;
                    },
                    onUpdate: function (evt) {
                        let newOrder = [...self.mediaItemsNoDuplicates];

                        if (evt.oldIndicies && evt.oldIndicies.length > 0) {
                        let oldIndices = evt.oldIndicies.sort(
                            (a, b) => a.index - b.index
                        );
                        let newIndices = evt.newIndicies.sort(
                            (a, b) => a.index - b.index
                        );

                        const itemsToMove = oldIndices.map(
                            (oldIdx) => self.mediaItems[oldIdx.index]
                        );

                        // old out
                        for (let i = oldIndices.length - 1; i >= 0; i--) {
                            newOrder.splice(oldIndices[i].index, 1);
                        }

                        // new in
                        for (let i = 0; i < newIndices.length; i++) {
                            newOrder.splice(newIndices[i].index, 0, itemsToMove[i]);
                        }
                        } else {
                            // Single item move
                            const [movedItem] = newOrder.splice(evt.oldIndex, 1);
                            newOrder.splice(evt.newIndex, 0, movedItem);
                        }

                        self.$emit("updated", newOrder);

                        const selectedElements = self.$refs.mediaContainer.querySelectorAll(".sortable-selected");
                        selectedElements.forEach((el) => {
                            Sortable.utils.deselect(el);
                        });
                        self.multiSelectedItems = [];
                    },
                });
            }
        },
        selectAndDeleteMedia: function selectAndDeleteMedia(media) {
            this.$parent.$emit("selectAndDeleteMediaRequested", media);
        },
        showMediaTextModal: function showMediaTextModal(media) {
            this.$parent.$emit("selectMediaRequested", media);
            this.$nextTick(() => this.$emit("updatemediatext"));
        },
        showAnchorModal: function showAnchorModal(media) {
            this.$parent.$emit("selectMediaRequested", media);
            this.$nextTick(() => this.$emit("updateanchor"));
        },
        updateSelectedCount() {
            this.selectedItemCount = this.$refs.mediaContainer.querySelectorAll(".sortable-selected").length;
        },
        getfontAwesomeClassNameForFileName: function getfontAwesomeClassNameForFilename(filename, thumbsize) {
            return getClassNameForFilename(filename) + " " + thumbsize;
        },
        deselectAll: function deselectAll() {
            this.$refs.mediaContainer
                .querySelectorAll(".sortable-selected")
                .forEach((el) => {
                    Sortable.utils.deselect(el);
                });
            this.multiSelectedItems = [];
        },
        showMediaModal: function () {
            this.$parent.showModal();
        },
        removeDuplicates: function (array) {
            return array.filter((obj, index, self) =>
                index === self.findIndex((o) => o.mediaPath === obj.mediaPath)
            );
        },
    }
});
