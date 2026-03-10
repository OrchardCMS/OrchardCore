<template>
  <div>
    <nav id="file-pager" class="tw-mb-3" aria-label="Pagination Navigation" role="navigation"
      :data-computed-trigger="itemsInCurrentPage?.length">
      <ul class="ma-pagination">
        <li class="ma-page-item file-first-button" :class="{ 'is-disabled': !canDoFirst }">
          <a class="ma-page-link first" href="#" :tabindex="canDoFirst ? 0 : -1" v-on:click="goFirst">
            {{ t.PagerFirstButton }}
          </a>
        </li>
        <li class="ma-page-item" :class="{ 'is-disabled': !canDoPrev }">
          <a class="ma-page-link previous" href="#" :tabindex="canDoPrev ? 0 : -1" v-on:click="previous">
            {{ t.PagerPreviousButton }}
          </a>
        </li>
        <template v-for="(link, index) in pageLinks" v-bind:key="index">
          <li v-if="link != -1" class="ma-page-item ma-page-number" :class="{ active: current == link - 1 }">
            <a class="ma-page-link" href="#" v-on:click="goTo(link - 1)" :aria-label="'Goto Page ' + link">
              {{ link }}
              <span v-if="current == link - 1" style="display:none">(current)</span>
            </a>
          </li>
        </template>
        <li class="ma-page-item" :class="{ 'is-disabled': !canDoNext }">
          <a class="ma-page-link next" href="#" :tabindex="canDoNext ? 0 : -1" v-on:click="next">
            {{ t.PagerNextButton }}
          </a>
        </li>
        <li class="ma-page-item file-last-button" :class="{ 'is-disabled': !canDoLast }">
          <a class="ma-page-link last" href="#" :tabindex="canDoLast ? 0 : -1" v-on:click="goLast">
            {{ t.PagerLastButton }}
          </a>
        </li>
      </ul>
    </nav>
  </div>
</template>

<script setup lang="ts">
/**
 * Pager component
 *
 * This component receives a list of all the items, unpaged.
 * As the user interacts with the pager, it raises events with the items in the current page.
 * It's the parent's responsibility to listen for these events and display the received items
 */
import { IFileLibraryItemDto } from '../interfaces/interfaces';
import { computed, ref, watch } from 'vue'
import { useEventBus } from '../services/UseEventBus'
import { useLocalizations } from '../services/Localizations';
const { emit } = useEventBus();
const { translations } = useLocalizations();
const t = translations.value

const props = defineProps({
  sourceItems: {
    type: Array<IFileLibraryItemDto>,
    required: true
  }
});

/**
 * Number of items to show per page
 */
const pageSize = ref(10);

/**
 * The current page number, 0-indexed
 */
const current = ref(0);

/**
 * Sets the current page to the next page, 0-indexed.
 */
const next = () => {
  current.value = current.value + 1;
}


/**
 * Sets the current page to the previous page, 0-indexed.
 */
const previous = () => {
  current.value = current.value - 1;
}


/**
 * Sets the current page to the first page, 0-indexed.
 */
const goFirst = () => {
  current.value = 0;
}


/**
 * Sets the current page to the last page, 0-indexed.
 */
const goLast = () => {
  current.value = totalPages.value - 1;
}


/**
 * Sets the current page to the target page number, 0-indexed.
 * @param {number} targetPage the 0-indexed target page number
 */
const goTo = (targetPage: number) => {
  current.value = targetPage;
}

/**
 * The total number of items
 */
const total = computed(() => {
  return props.sourceItems.length;
});

/**
 * The total number of pages
 */
const totalPages = computed(() => {
  let pages = Math.ceil(total.value / pageSize.value);
  return pages > 0 ? pages : 1;
});

/**
 * Whether the current page is the last page
 */
const isLastPage = computed(() => {
  return current.value + 1 >= totalPages.value;
});

/**
 * Whether the current page is the first page
 */
const isFirstPage = computed(() => {
  return current.value === 0;
});

/**
 * Whether the previous page button should be enabled
 */
const canDoPrev = computed(() => {
  return !isFirstPage.value;
});

/**
 * Whether the next page button should be enabled
 */
const canDoNext = computed(() => {
  return !isLastPage.value;
});

/**
 * Whether the first page button should be enabled
 */
const canDoFirst = computed(() => {
  return !isFirstPage.value;
});

/**
 * Whether the last page button should be enabled
 */
const canDoLast = computed(() => {
  return !isLastPage.value;
});

// this computed is only to have a central place where we detect changes and leverage Vue JS reactivity to raise our event.
// That event will be handled by the parent file app to display the items in the page.
// this logic will not run if the computed property is not used in the template. We use a dummy "data-computed-trigger" attribute for that.
const itemsInCurrentPage = computed(() => {
  let start = pageSize.value * current.value;
  let end = start + pageSize.value;
  let result = props.sourceItems.slice(start, end);

  emit("PagerEvent", result);

  return result;
});

/**
 * The links to show in the pager
 */
const pageLinks = computed(() => {
  let links = [];

  links.push(current.value + 1);

  // Add 2 items before current
  let beforeCurrent = current.value > 0 ? current.value : -1;
  links.unshift(beforeCurrent);

  let beforeBeforeCurrent = current.value > 1 ? current.value - 1 : -1;
  links.unshift(beforeBeforeCurrent);

  // Add 2 items after current
  let afterCurrent = totalPages.value - current.value > 1 ? current.value + 2 : -1;
  links.push(afterCurrent);

  let afterAfterCurrent = totalPages.value - current.value > 2 ? current.value + 3 : -1;
  links.push(afterAfterCurrent);

  return links;
});

watch(props.sourceItems, () => {
  current.value = 0; // resetting current page after receiving a new list of unpaged items
});
</script>
