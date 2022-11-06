import { createApp } from 'vue'
//import FolderComponent from './components/folderComponent.vue';
//import MediaItemsGridComponent from './components/mediaItemsGridComponent.vue'
//import MediaItemsTableComponent from './components/mediaItemsTableComponent.vue'
//import PagerComponent from './components/pagerComponent.vue'
//import SortIndicatorComponent from './components/sortIndicatorComponent.vue'
//import '../node_modules/bootstrap/dist/css/bootstrap.css'
import './style.css'
import App from './App.vue'
import mitt from 'mitt';

const emitter = mitt();
const mountEl = document.querySelector("#app") as HTMLElement | null;

const app = createApp(App, { ...mountEl?.dataset })
app.config.globalProperties.emitter = emitter;

app.mount('#app')
