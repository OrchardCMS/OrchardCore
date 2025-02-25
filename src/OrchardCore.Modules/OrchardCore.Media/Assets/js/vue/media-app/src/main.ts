import { createApp } from "vue";
import App from "./App.vue";
import mitt from "mitt";
/* import the fontawesome core */
import { library } from "@fortawesome/fontawesome-svg-core";
/* import font awesome icon component */
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";
/* import specific icons */
import { fas } from "@fortawesome/free-solid-svg-icons";
import { far } from "@fortawesome/free-regular-svg-icons";
import { createVfm } from "vue-final-modal";

const emitter = mitt();
const vfm = createVfm();

/* add icons to the library */
library.add(fas);
library.add(far);

const app = createApp({ name: "media-library" });
app.component("media-app", App);
app.component("fa-icon", FontAwesomeIcon);
app.config.globalProperties.emitter = emitter;

app.use(vfm);
app.mount("#media-app");
export default emitter;
