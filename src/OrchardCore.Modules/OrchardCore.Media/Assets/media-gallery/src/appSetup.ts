import { type App as VueApp, type Plugin } from "vue";
import router from "./router";
/* import the fontawesome core */
import { library } from "@fortawesome/fontawesome-svg-core";
/* import font awesome icon component */
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";
/* import specific icons */
import { fas } from "@fortawesome/free-solid-svg-icons";
import { far } from "@fortawesome/free-regular-svg-icons";
import { createVfm } from "vue-final-modal";
import PrimeVue, { type PrimeVueConfiguration } from "primevue/config";
import Aura from "@primevue/themes/aura";
import Menu from "primevue/menu";
import TreeSelect from "primevue/treeselect";

/* add icons to the library (once) */
library.add(fas);
library.add(far);

/**
 * Configure a Vue app instance with the shared plugins the media app needs. Used by both the
 * embedded entry (main.ts — gallery + picker) and the standalone entry (standalone.ts).
 */
export function configureMediaApp(app: VueApp) {
  const vfm = createVfm();

  app.component("fa-icon", FontAwesomeIcon);
  app.component("p-menu", Menu);
  app.component("p-treeselect", TreeSelect);

  // PrimeVue's exported plugin type (ObjectPlugin<any[]>) doesn't structurally match Vue's
  // Plugin<[Options]> overload, so cast it; the options object is still checked.
  app.use(PrimeVue as unknown as Plugin<[PrimeVueConfiguration]>, {
    theme: {
      preset: Aura,
      options: {
        prefix: "p",
        darkModeSelector: '[data-bs-theme="dark"]',
        cssLayer: {
          name: "primevue",
          order: "theme, base, primevue, utilities",
        },
      },
    },
  });
  app.use(vfm);
  app.use(router);
}
