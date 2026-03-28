import { createApp } from 'vue'
import { setTranslations } from '@bloom/helpers/localizations'
import App from './App.vue'

const mountEl = document.getElementById('vue-openapi-settings')

if (mountEl) {
    const localizationsAttr = mountEl.getAttribute('localizations')
    if (localizationsAttr) {
        try {
            setTranslations(JSON.parse(localizationsAttr))
        }
        catch {
            // Use defaults if JSON is invalid.
        }
    }

    const app = createApp(App, {
        settingsData: mountEl.getAttribute('settings-data'),
    })
    app.mount(mountEl)
}
