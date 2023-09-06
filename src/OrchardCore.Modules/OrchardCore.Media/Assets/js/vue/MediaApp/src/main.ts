import { createApp } from 'vue'
//import FolderComponent from './components/folderComponent.vue';
//import MediaItemsGridComponent from './components/mediaItemsGridComponent.vue'
//import MediaItemsTableComponent from './components/mediaItemsTableComponent.vue'
//import PagerComponent from './components/pagerComponent.vue'
//import SortIndicatorComponent from './components/sortIndicatorComponent.vue'
//import '../node_modules/bootstrap/dist/css/bootstrap.css'
import './style.css'
import App from './App.vue'
import mitt from 'mitt'
/* import the fontawesome core */
import { library } from '@fortawesome/fontawesome-svg-core'
/* import font awesome icon component */
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
/* import specific icons */
import { fas } from '@fortawesome/free-solid-svg-icons'

const emitter = mitt();
const mountEl = document.querySelector("#mediaApp") as HTMLElement | null;

/* add icons to the library */
library.add(fas)

const app = createApp(App, { ...mountEl?.dataset })
app.component('fa-icon', FontAwesomeIcon)
app.config.globalProperties.emitter = emitter;

app.mount('#mediaApp')
