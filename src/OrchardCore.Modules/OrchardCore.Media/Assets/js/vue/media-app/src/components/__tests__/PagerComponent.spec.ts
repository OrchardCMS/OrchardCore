// pagerComponent.spec.ts
import { mount } from '@vue/test-utils';
import PagerComponent from '../PagerComponent.vue';
import { describe, expect, it } from 'vitest';
import { nextTick } from 'vue';

/**
 * Helper to extract visible page numbers from rendered page-number elements.
 */
const getPageNumbers = (wrapper: ReturnType<typeof mount>) => {
  return wrapper.findAll('.page-number .page-link').map(el => Number(el.text().replace(/\(current\)/, '').trim()));
};

/**
 * Helper to get the active page number.
 */
const getActivePage = (wrapper: ReturnType<typeof mount>) => {
  const active = wrapper.find('.page-number.active .page-link');
  if (!active.exists()) return null;
  return Number(active.text().replace(/\(current\)/, '').trim());
};

/**
 * Access internal setup state.
 */
const ss = (wrapper: ReturnType<typeof mount>) => {
  return (wrapper.vm.$ as any).setupState; // eslint-disable-line @typescript-eslint/no-explicit-any
};

/**
 * Call a pager function and wait for DOM update.
 */
const callAndUpdate = async (wrapper: ReturnType<typeof mount>, fn: () => void) => {
  fn();
  wrapper.vm.$forceUpdate();
  await nextTick();
  await nextTick();
};

describe('PagerComponent', () => {
  it('renders page links correctly', async () => {
    const wrapper = mount(PagerComponent, {
      props: { sourceItems: Array(30).fill(null) },
    });
    await nextTick();

    const pageNumbers = getPageNumbers(wrapper);
    expect(pageNumbers).toEqual([1, 2, 3]);
    expect(getActivePage(wrapper)).toBe(1);
    expect(ss(wrapper).current).toBe(0);
  });

  it('renders page links correctly when no items', async () => {
    const wrapper = mount(PagerComponent, {
      props: { sourceItems: [] },
    });
    await nextTick();

    const pageNumbers = getPageNumbers(wrapper);
    expect(pageNumbers).toEqual([1]);
    expect(getActivePage(wrapper)).toBe(1);
  });

  it('first page has disabled first/previous buttons', async () => {
    const wrapper = mount(PagerComponent, {
      props: { sourceItems: Array(30).fill(null) },
    });
    await nextTick();

    expect(wrapper.find('.file-first-button').classes()).toContain('disabled');
    expect(getActivePage(wrapper)).toBe(1);
  });

  it('next increments current page', async () => {
    const wrapper = mount(PagerComponent, {
      props: { sourceItems: Array(30).fill(null) },
    });
    await nextTick();

    await callAndUpdate(wrapper, () => ss(wrapper).next());

    expect(ss(wrapper).current).toBe(1);
    expect(getActivePage(wrapper)).toBe(2);
  });

  it('goTo navigates to target page', async () => {
    const wrapper = mount(PagerComponent, {
      props: { sourceItems: Array(30).fill(null) },
    });
    await nextTick();

    await callAndUpdate(wrapper, () => ss(wrapper).goTo(1));

    expect(ss(wrapper).current).toBe(1);
    expect(getActivePage(wrapper)).toBe(2);
  });

  it('goFirst returns to first page', async () => {
    const wrapper = mount(PagerComponent, {
      props: { sourceItems: Array(30).fill(null) },
    });
    await nextTick();

    await callAndUpdate(wrapper, () => ss(wrapper).next());
    expect(ss(wrapper).current).toBe(1);

    await callAndUpdate(wrapper, () => ss(wrapper).goFirst());
    expect(ss(wrapper).current).toBe(0);
    expect(getActivePage(wrapper)).toBe(1);
  });

  it('goLast goes to last page', async () => {
    const wrapper = mount(PagerComponent, {
      props: { sourceItems: Array(30).fill(null) },
    });
    await nextTick();

    await callAndUpdate(wrapper, () => ss(wrapper).goLast());

    expect(ss(wrapper).current).toBe(2);
    expect(getActivePage(wrapper)).toBe(3);
  });
});
