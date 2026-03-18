import { createApp } from 'vue'
import App from './App.vue'

const mountEl = document.getElementById('vue-openapi-settings')

if (mountEl) {
    const app = createApp(App)
    app.mount(mountEl)
}
