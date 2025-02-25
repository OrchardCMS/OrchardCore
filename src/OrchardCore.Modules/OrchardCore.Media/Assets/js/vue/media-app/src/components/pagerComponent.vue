<!--
    This component receives a list of all the items, unpaged.
    As the user interacts with the pager, it raises events with the items in the current page.
    It's the parent's responsibility to listen for these events and display the received items
    <pager> component
-->

<template>
  <div>
    <nav id="file-pager" class="mb-3" aria-label="Pagination Navigation" role="navigation"
      :data-computed-trigger="itemsInCurrentPage?.length">
      <ul class="pagination pagination-sm">
        <li class="page-item file-first-button" :class="{ disabled: !canDoFirst }">
          <a class="page-link" href="#" :tabindex="canDoFirst ? 0 : -1" v-on:click="goFirst">
            {{ t.PagerFirstButton }}
          </a>
        </li>
        <li class="page-item" :class="{ disabled: !canDoPrev }">
          <a class="page-link" href="#" :tabindex="canDoPrev ? 0 : -1" v-on:click="previous">
            {{ t.PagerPreviousButton }}
          </a>
        </li>
        <template v-for="link in pageLinks">
          <li v-if="link != -1" class="page-item page-number" :class="{ active: current == link - 1 }">
            <a class="page-link" href="#" v-on:click="goTo(link - 1)" :aria-label="'Goto Page ' + link">
              {{ link }}
              <span v-if="current == link - 1" style="display:none">(current)</span>
            </a>
          </li>
        </template>
        <li class="page-item" :class="{ disabled: !canDoNext }">
          <a class="page-link" href="#" :tabindex="canDoNext ? 0 : -1" v-on:click="next">
            {{ t.PagerNextButton }}
          </a>
        </li>
        <li class="page-item file-last-button" :class="{ disabled: !canDoLast }">
          <a class="page-link" href="#" :tabindex="canDoLast ? 0 : -1" v-on:click="goLast">
            {{ t.PagerLastButton }}
          </a>
        </li>
      </ul>
    </nav>
  </div>
</template>

<script lang="ts">
import { IFileStoreEntry } from '../services/MediaApiClient';
import { defineComponent } from 'vue'
import dbg from 'debug';

const debug = dbg("aptix:file-app");

export default defineComponent({
  name: "pager",
  props: {
    sourceItems: Array<IFileStoreEntry>,
    t: {
      type: Object,
      required: true,
    }
  },
  data() {
    return {
      pageSize: 10,
      pageSizeOptions: [10, 30, 50, 100],
      current: 0,
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
    goTo: function (targetPage: number) {
      this.current = targetPage;
    }
  },
  computed: {
    total: function () {
      return this.sourceItems ? this.sourceItems.length : 0;
    },
    totalPages: function () {
      let pages = Math.ceil(this.total / this.pageSize);
      return pages > 0 ? pages : 1;
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
    // That event will be handled by the parent file app to display the items in the page.
    // this logic will not run if the computed property is not used in the template. We use a dummy "data-computed-trigger" attribute for that.
    itemsInCurrentPage: function () {
      let emitter = this.emitter;
      let start = this.pageSize * this.current;
      let end = start + this.pageSize;
      let result = this.sourceItems?.slice(start, end);

      //debug("pagerEvent itemsInCurrentPage", result)
      emitter.emit('pagerEvent', result);

      return result;
    },
    pageLinks: function () {
      let links = [];

      links.push(this.current + 1);

      // Add 2 items before current
      let beforeCurrent = this.current > 0 ? this.current : -1;
      links.unshift(beforeCurrent);

      let beforeBeforeCurrent = this.current > 1 ? this.current - 1 : -1;
      links.unshift(beforeBeforeCurrent);

      // Add 2 items after current
      let afterCurrent = this.totalPages - this.current > 1 ? this.current + 2 : -1;
      links.push(afterCurrent);

      let afterAfterCurrent = this.totalPages - this.current > 2 ? this.current + 3 : -1;
      links.push(afterAfterCurrent);

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
</script>
