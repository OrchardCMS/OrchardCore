// <sort-indicator> component
Vue.component('sortIndicator', {
    template: `
        <div v-show="isActive" class="sort-indicator">
            <span v-show="asc"><i class="small fa fa-chevron-up"></i></span>
            <span v-show="!asc"><i class="small fa fa-chevron-down"></i></span>
        </div>
        `,
    props: {
        colname: String,
        selectedcolname: String,
        asc: Boolean
    },
    computed: {
        isActive: function () {
            return this.colname.toLowerCase() == this.selectedcolname.toLowerCase();
        }
    }
});
