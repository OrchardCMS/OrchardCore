import { describe, expect, it } from 'vitest';
import { mount } from '@vue/test-utils';
import SortIndicatorComponent from '../SortIndicatorComponent.vue';

describe('SortIndicatorComponent', () => {
  it('renders sort indicator correctly', async () => {
    const wrapper = mount(SortIndicatorComponent, {
      props: {
        colname: 'name',
        selectedcolname: 'name',
        asc: true,
      },
    });

    expect(wrapper.find('.sort-indicator').exists()).toBe(true);
    expect(wrapper.find('.fa-chevron-up').exists()).toBe(true);
    expect(wrapper.find('.fa-chevron-down').exists()).toBe(false);
  });

  it('renders sort indicator correctly when asc is false', async () => {
    const wrapper = mount(SortIndicatorComponent, {
      props: {
        colname: 'name',
        selectedcolname: 'name',
        asc: false,
      },
    });

    expect(wrapper.find('.sort-indicator').exists()).toBe(true);
    expect(wrapper.find('.fa-chevron-up').exists()).toBe(false);
    expect(wrapper.find('.fa-chevron-down').exists()).toBe(true);
  });

  it('does not render sort indicator when colname does not match selectedcolname', async () => {
    const wrapper = mount(SortIndicatorComponent, {
      props: {
        colname: 'name',
        selectedcolname: 'other',
        asc: true,
      },
    });

    expect(wrapper.find('.fa-chevron-up').exists()).toBe(false);
  });
});