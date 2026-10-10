import { createApp } from "vue";
import App from "./App.vue";
import { readConfig } from "./config";
import { loadTranslations } from "./i18n";
import { createDesignerApi } from "./api/designerApi";
import "./styles/designer.css";

// Mounts the designer on #data-pipeline-designer (Views/DataPipeline/Designer.cshtml). The requests carry the
// antiforgery token of the page's #data-pipeline-designer-antiforgery form (see @bloom/services/api-service).
const element = document.getElementById("data-pipeline-designer");

if (element) {
    const config = readConfig(element);

    loadTranslations(config.translations);
    createApp(App, { config, api: createDesignerApi(config.urls) }).mount(element);
}
