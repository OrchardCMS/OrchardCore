import { createApp, h } from 'vue'
import App from './App.vue'

const mountEl = document.getElementById('vue-openapi-settings')

if (mountEl) {
    const settingsEl = mountEl.querySelector('openapi-settings')

    const app = createApp({
        render: () => h(App, {
            settingsData: settingsEl?.getAttribute('settings-data'),
            translations: settingsEl?.getAttribute('translations'),
        }),
    })
    app.mount(mountEl)
}
