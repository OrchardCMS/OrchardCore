// <media-field-thumbs-container> component
// different media field editors share this component to present the thumbs.
Vue.component('mediaFieldThumbsContainer', {
    template:
    /* html */
    `
    <div :id="idPrefix + '_mediaContainerMain'">
         <div v-if="mediaItems.length < 1" class="card text-center">
            <div class= "card-body">
                <span class="hint">{{T.noImages}}</span>
            </div>
         </div>
         <ol ref="multiDragContainer" class="media-items-grid d-flex flex-row align-items-start flex-wrap" >
            <li v-for="media in mediaItems"
                :key="media.vuekey"
                class="media-thumb-item media-container-main-list-item card overflow"
                :style="{width: thumbSize + 2 + 'px'}"
                v-on:click="selectMedia(media)"
                v-if="!media.isRemoved">
                    <div v-if="media.mediaPath!== 'not-found'">
                        <div class="thumb-container" :style="{height: thumbSize + 'px'}" >
                            <img v-if="media.mime.startsWith('image')"
                                :src="buildMediaUrl(media.url, thumbSize)"
                                :data-mime="media.mime"
                                width="100%"
                            />
                            <i v-else :class="getfontAwesomeClassNameForFileName(media.name, 'fa-4x')" :data-mime="media.mime"></i>
                        </div>
                        <div class="media-container-main-item-title card-body">
                            <a href="javascript:;" class="btn btn-light btn-sm float-end inline-media-button delete-button"
                                v-on:click.stop="selectAndDeleteMedia(media)"><i class="fa-solid fa-trash" aria-hidden="true"></i></a>
                            <a :href="media.url" target="_blank" class="btn btn-light btn-sm float-end inline-media-button view-button""><i class="fa-solid fa-download" aria-hidden="true"></i></a>
                            <span class="media-filename card-text small" :title="media.mediaPath">{{ media.isNew ? media.name.substr(36) : media.name }}</span>
                        </div>
                    </div>
                    <div v-else>
                        <div class="thumb-container flex-column" :style="{height: thumbSize + 'px'}">
                            <i class="fa-solid fa-ban text-danger d-block" aria-hidden="true"></i>
                            <span class="text-danger small d-block">{{ T.mediaNotFound }}</span>
                            <span class="text-danger small d-block text-center">{{ T.discardWarning }}</span>
                        </div>
                        <div class="media-container-main-item-title card-body">
                            <a href="javascript:;" class="btn btn-light btn-sm float-end inline-media-button delete-button"
                                v-on:click.stop="selectAndDeleteMedia(media)"><i class="fa-solid fa-trash" aria-hidden="true"></i></a>
                            <span class="media-filename card-text small text-danger" :title="media.name">{{ media.name }}</span>
                        </div>
                  </div>
            </li>
         </ol>
    </div>
    `,
    data: function () {
        return {
            T: {},
            sortableInstance: null,
            multiSelectedItems: [],
        };
    },
    model: {
        prop: 'mediaItems',
        event: 'changed',
    },
    props: {
        mediaItems: Array,
        selectedMedia: Object,
        thumbSize: Number,
        idPrefix: String,
        allowMultiple: Boolean,
    },
    created: function () {
        var self = this;

        // retrieving localized strings from view
        self.T.mediaNotFound = $('#t-media-not-found').val();
        self.T.discardWarning = $('#t-discard-warning').val();
        self.T.noImages = $('#t-no-images').val();
    },
    mounted: function () {
        if (this.allowMultiple) this.initSortable();
    },
    methods: {
        initSortable: function () {
            if (this.$refs.multiDragContainer) {
                var self = this;

                this.sortableInstance = Sortable.create(this.$refs.multiDragContainer, {
                animation: 150,
                ghostClass: 'sortable-ghost',
                multiDrag: true,
                selectedClass: 'sortable-selected',
                preventOnFilter: true,
                forceHelperSize: true,
                onUpdate: function (evt) {
                    let newOrder = [...self.mediaItems];

                    if (evt.oldIndicies && evt.oldIndicies.length > 0) {
                        let oldIndices = evt.oldIndicies.sort((a, b) => a.index - b.index);
                        let newIndices = evt.newIndicies.sort((a, b) => a.index - b.index);

                        const itemsToMove = oldIndices.map(oldIdx => self.mediaItems[oldIdx.index]);

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

                    self.$emit('changed', newOrder);

                    const selectedElements = self.$refs.multiDragContainer.querySelectorAll('.sortable-selected');
                    selectedElements.forEach(el => {
                        Sortable.utils.deselect(el);
                    });
                    self.multiSelectedItems = [];
                }
                });
            }
        },
        selectAndDeleteMedia: function (media) {
            this.$parent.$emit('selectAndDeleteMediaRequested', media);
        },
        selectMedia: function (media) {
            this.$parent.$emit('selectMediaRequested', media);
        },
        buildMediaUrl: function (url, thumbSize) {
            return url + (url.indexOf('?') == -1 ? '?' : '&') + 'width=' + thumbSize + '&height=' + thumbSize;
        },
        getfontAwesomeClassNameForFileName:function getfontAwesomeClassNameForFilename(filename, thumbsize){
            return getClassNameForFilename(filename) + ' ' + thumbsize;
        }
    }
});
