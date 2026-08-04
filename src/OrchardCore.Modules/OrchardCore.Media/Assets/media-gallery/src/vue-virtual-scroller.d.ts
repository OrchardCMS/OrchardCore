declare module 'vue-virtual-scroller' {
  import { DefineComponent } from 'vue';

  export const RecycleScroller: DefineComponent<{
    items: any[];
    itemSize: number | null;
    keyField?: string;
    buffer?: number;
    direction?: 'vertical' | 'horizontal';
  }>;

  export const DynamicScroller: DefineComponent<any>;
  export const DynamicScrollerItem: DefineComponent<any>;
}
