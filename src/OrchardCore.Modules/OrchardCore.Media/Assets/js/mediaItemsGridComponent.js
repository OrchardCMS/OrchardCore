// <media-items-grid> component
Vue.component('mediaItemsGrid', {
    template: '\
        <ol class="row">\
                <li v-for="media in filteredMediaItems" \
                    :key="media.name" \
                    class="media-item media-container-main-list-item card" \
                    :style="{width: thumbSize + 2 + \'px\'}" \
                    :class="{selected: isMediaSelected(media)}" \
                    v-on:click.stop="toggleSelectionOfMedia(media)" \
                    draggable="true" v-on:dragstart="dragStart(media, $event)"> \
                    <div class="thumb-container" :style="{height: thumbSize + \'px\'}"> \
                            <img draggable="false" :src="media.url + \'?width=\' + thumbSize + \'&height=\' + thumbSize" :style="{ maxHeight: thumbSize + \'px\' , maxWidth: thumbSize + \'px\' }"/> \
                    </div> \
                    <div class="media-container-main-item-title card-body"> \
                        <a href="javascript:;" class="btn btn-light btn-sm float-right inline-media-button edit-button mr-4" v-on:click.stop="renameMedia(media)"><i class="fa fa-edit"></i></a> \
                        <a href="javascript:;" class="btn btn-light btn-sm float-right inline-media-button delete-button" v-on:click.stop="deleteMedia(media)"><i class="fa fa-trash"></i></a> \
                        <span class="media-filename card-text small" :title="media.name">{{ media.name }}</span> \
                    </div> \
                 </li> \
        </ol>\
        \
        ',
    data: function () {
        return {
            T: {}
        }
    },
    props: {
        filteredMediaItems: Array,
        selectedMedias: Array,
        thumbSize: Number
    },
    created: function () {
        var self = this;
        // retrieving localized strings from view
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
