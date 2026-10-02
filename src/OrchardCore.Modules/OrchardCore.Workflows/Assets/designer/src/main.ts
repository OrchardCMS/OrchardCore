import { createApp } from "vue";
import App from "./App.vue";
import { readConfig } from "./config";
import { loadTranslations } from "./i18n";
import { createDesignerApi } from "./api/designerApi";
import "./styles/designer.css";

// Mounts the designer on #workflow-designer (Views/WorkflowType/Designer.cshtml).
const element = document.getElementById("workflow-designer");

if (element) {
    const config = readConfig(element);

    loadTranslations(config.translations);
    createApp(App, { config, api: createDesignerApi(config.urls) }).mount(element);
}
