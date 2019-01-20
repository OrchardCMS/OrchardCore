// This component receives a list of all the items, unpaged.
// As the user interacts with the pager, it raises succesive events with the items in the current page.
// It's the parent's responsibility to listen for that events and display the items received
// <pager> component
Vue.component('pager', {
    template: '\
        <nav id="media-pager" aria-label="Pagination Navigation" role="navigation" :data-computed-trigger="itemsInCurrentPage.length">\
            <ul class= "pagination  pagination-sm"> \
                <li class="page-item media-first-button" :class="{disabled : !canDoFirst}"> \
                    <a class="page-link" href="#" :tabindex="canDoFirst ? 0 : -1" v-on:click="goFirst">First</a> \
                </li> \
                <li class="page-item" :class="{disabled : !canDoPrev}"> \
                    <a class="page-link" href="#" :tabindex="canDoPrev ? 0 : -1" v-on:click="previous">Previous</a> \
                </li> \
                <li class="page-item page-number"  :class="{active : current == link - 1}" v-for="link in pageLinks"> \
                    <a class="page-link" href="#" v-on:click="goTo(link - 1)" :aria-label="\'Goto Page \' + link">\
                        {{link}} \
                        <span v-if="current == link -1" class="sr-only">(current)</span>\
                    </a> \
                </li> \
                <li class="page-item" :class="{disabled : !canDoNext}"> \
                    <a class="page-link" href="#" :tabindex="canDoNext ? 0 : -1" v-on:click="next">Next</a> \
                </li> \
                <li class="page-item media-last-button" :class="{disabled : !canDoLast}"> \
                    <a class="page-link" href="#" :tabindex="canDoLast ? 0 : -1" v-on:click="goLast">Last</a> \
                </li> \
                <li class="page-item ml-4">\
                    <div style="display: flex; ">\
                        <span class="page-link text-muted page-size-label">Page Size</span>\
                        <select id="pageSizeSelect" class="page-link" v-model="pageSize"> \
                            <option v-for="option in pageSizeOptions" v-bind:value="option"> \
                                {{option}} \
                            </option> \
                        </select> \
                    </div>\
                </li> \
                <li class="page-item ml-4 page-info"> \
                    <span class="page-link"> {{current + 1}} - {{totalPages}}</span> \
                </li> \
            </ul> \
        </nav>\
        ',
    props: {
        sourceItems: Array
    },
    data: function () {
        return {
            pageSize: 5,
            pageSizeOptions: [5, 10, 30, 50, 100],
            current: 0
        };
    },
    methods: {
        next: function () {
            this.current = this.current + 1;
        },
        previous: function () {
            this.current = this.current - 1;
        },
        goFirst: function () {
            this.current = 0;
        },
        goLast: function () {
            this.current = this.totalPages - 1;
        },
        goTo: function (targetPage) {
            this.current = targetPage;
        }
    },
    computed: {
        total: function () {
            return this.sourceItems ? this.sourceItems.length : 0;
        },
        totalPages: function () {
            return Math.ceil(this.total / this.pageSize);
        },
        isLastPage: function () {
            return this.current + 1 >= this.totalPages;
        },
        isFirstPage: function () {
            return this.current === 0;
        },
        canDoNext: function () {
            return !this.isLastPage;
        },
        canDoPrev: function () {
            return !this.isFirstPage;
        },
        canDoFirst: function () {
            return !this.isFirstPage;
        },
        canDoLast: function () {
            return !this.isLastPage;
        },
        // this computed is only to have a central place where we detect changes and leverage Vue JS reactivity to raise our event.
        // That event will be handled by the parent media app to display the items in the page.
        // this logic will not run if the computed property is not used in the template. We use a dummy "data-computed-trigger" attribute for that.
        itemsInCurrentPage: function () {
            var start = this.pageSize * this.current;
            var end = start + this.pageSize;
            var result = this.sourceItems.slice(start, end);
            bus.$emit('pagerEvent', result);
            return result;
        },
        pageLinks: function () {

            var links = [];
            var linksCount = 4;

            var nextItem = this.current - linksCount;
            if (nextItem < 0) {
                nextItem = -1;
            }

            for (var i = 0; i < linksCount; i++) {
                if (nextItem >= this.totalPages) {
                    break;
                }
                nextItem = nextItem + 1;
                links.push(nextItem + 1);

            }

            return links;

        }
    },
    watch: {
        sourceItems: function () {
            this.current = 0; // resetting current page after receiving a new list of unpaged items
        },
        pageSize: function () {
            this.current = 0;
        }
    }
});
