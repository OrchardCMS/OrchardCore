import { createApp } from "vue";
import App from "./App.vue";
import CompareApp from "./compare/CompareApp.vue";
import { readConfig } from "./config";
import { loadTranslations } from "./i18n";
import { createDesignerApi } from "./api/designerApi";
import "./styles/designer.css";

// Mounts the designer on #workflow-designer (Views/WorkflowType/Edit.cshtml, Version.cshtml and CompareVersions.cshtml,
// Views/Workflow/Details.cshtml).
const element = document.getElementById("workflow-designer");

if (element) {
    const config = readConfig(element);

    loadTranslations(config.translations);
    // The compare page shows two definitions; every other page is the designer, read-only or not.
    createApp(config.mode === "compare" ? CompareApp : App, { config, api: createDesignerApi(config.urls) }).mount(element);
}
