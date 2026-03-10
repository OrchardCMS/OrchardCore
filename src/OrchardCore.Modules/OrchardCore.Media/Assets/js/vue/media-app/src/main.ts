declare global {
  var __VUE_PROD_DEVTOOLS__: boolean; // eslint-disable-line no-var
}

globalThis.__VUE_PROD_DEVTOOLS__ = false;

import { createApp } from "vue";
import App from "./App.vue";
import router from './router';
/* import the fontawesome core */
import { library } from "@fortawesome/fontawesome-svg-core";
/* import font awesome icon component */
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";
/* import specific icons */
import { fas } from "@fortawesome/free-solid-svg-icons";
import { far } from "@fortawesome/free-regular-svg-icons";
import { createVfm } from "vue-final-modal";
import { registerNotificationBus } from "@bloom/services/notifications/notifier";
import "primeflex/primeflex.scss";
import "primevue/resources/primevue.css";
import "primevue/resources/themes/lara-light-blue/theme.css";
import PrimeVue from 'primevue/config';
import Menu from 'primevue/menu';
import TreeSelect from 'primevue/treeselect';

const vfm = createVfm();

/* add icons to the library */
library.add(fas);
library.add(far);

const app = createApp({ name: "media-library" });
app.component("media-app", App);
app.component("fa-icon", FontAwesomeIcon);
app.component('p-menu', Menu);
app.component('p-treeselect', TreeSelect);

registerNotificationBus();

app.use(PrimeVue);
app.use(vfm);
app.use(router)
app.mount("#media-app");
