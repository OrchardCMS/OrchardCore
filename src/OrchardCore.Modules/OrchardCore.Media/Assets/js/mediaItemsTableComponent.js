// <media-items-table> component
Vue.component('mediaItemsTable', {
    template: '\
        <table class="table media-items-table"> \
            <thead> \
                <tr class="header-row"> \
                    <th scope="col" class="thumbnail-column" style="padding-left:16px;">{{ T.imageHeader }}</th> \
                    <th scope="col" v-on:click="changeSort(\'name\')"> \
                       {{ T.nameHeader }} \
                         <sort-indicator colname="name" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator> \
                    </th> \
                    <th scope="col" v-on:click="changeSort(\'size\')"> \
                        <span class="optional-col"> \
                            {{ T.sizeHeader }} \
                         <sort-indicator colname="size" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator> \
                        </span> \
                    </th> \
                    <th scope="col" v-on:click="changeSort(\'mime\')"> \
                        <span class="optional-col"> \
                           {{ T.typeHeader }} \
                         <sort-indicator colname="mime" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator> \
                        </span> \
                    </th> \
                </tr>\
            </thead>\
            <tbody> \
                    <tr v-for="media in filteredMediaItems" \
                          class="media-item" \
                          :class="{selected: isMediaSelected(media)}" \
                          v-on:click.stop="toggleSelectionOfMedia(media)" \
                          draggable="true" v-on:dragstart="dragStart(media, $event)" \
                          :key="media.name" style="height: 80px;"> \
                             <td class="thumbnail-column"> \
                                <div class="img-wrapper"> \
                                    <img draggable="false" :src="media.url + \'?width=\' + thumbSize + \'&height=\' + thumbSize" /> \
                                </div> \
                            </td> \
                            <td> \
                                <div class="media-name-cell"> \
                                    {{ media.name }} \
                                    <div class="buttons-container"> \
                                        <a href="javascript:;" class="btn btn-link btn-sm mr-1 edit-button" v-on:click.stop="renameMedia(media)"> {{ T.editButton }} </a > \
                                        <a href="javascript:;" class="btn btn-link btn-sm delete-button" v-on:click.stop="deleteMedia(media)"> {{ T.deleteButton }} </a> \
                                    </div> \
                                </div> \
                            </td> \
                            <td> \
                                <div class="text-col optional-col"> {{ isNaN(media.size)? 0 : Math.round(media.size / 1024) }} KB</div> \
                            </td> \
                            <td> \
                                <div class="text-col optional-col">{{ media.mime }}</div> \
                            </td> \
                   </tr>\
            </tbody>\
        </table> \
        ',
    data: function () {
        return {
            T: {}
        }
    },
    props: {
        sortBy: String,
        sortAsc: Boolean,
        filteredMediaItems: Array,
        selectedMedias: Array,
        thumbSize: Number
    },
    created: function () {
        var self = this;
        // retrieving localized strings from view
        self.T.imageHeader = $('#t-image-header').val();
        self.T.nameHeader = $('#t-name-header').val();
        self.T.sizeHeader = $('#t-size-header').val();
        self.T.typeHeader = $('#t-type-header').val();
        self.T.editButton = $('#t-edit-button').val();
        self.T.deleteButton = $('#t-delete-button').val();
    },
    methods: {
        isMediaSelected: function (media) {
            var result = this.selectedMedias.some(function (element, index, array) {
                return element.url.toLowerCase() === media.url.toLowerCase();
            });
            return result;
        },

        changeSort: function (newSort) {
            bus.$emit('sortChangeRequested', newSort);
        },

        toggleSelectionOfMedia: function (media) {
            bus.$emit('mediaToggleRequested', media);
        },
        renameMedia: function (media) {
            bus.$emit('renameMediaRequested', media);            
        },
        deleteMedia: function (media) {
            bus.$emit('deleteMediaRequested', media);
        },
        dragStart: function (media, e) {
            bus.$emit('mediaDragStartRequested', media, e);
        }
    }
});
