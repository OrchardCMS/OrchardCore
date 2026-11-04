import type { Library } from "../../api/types";

export const library: Library = {
    categories: [
        {
            name: "Contenu",
            activities: [
                { name: "ContentPublishedEvent", displayText: "Contenu publié", category: "Contenu", isEvent: true, hasEditor: true, thumbnailHtml: "<h4>Contenu publié</h4>", icon: "fa-solid fa-file-lines" },
                { name: "CreateContentTask", displayText: "Créer un contenu", category: "Contenu", isEvent: false, hasEditor: true, thumbnailHtml: "<h4>Créer</h4>", icon: null },
            ],
        },
        {
            name: "HTTP",
            activities: [
                { name: "HttpRequestEvent", displayText: "Http Request Event", category: "HTTP", isEvent: true, hasEditor: true, thumbnailHtml: "<h4>Http Request</h4>", icon: "fa-solid fa-globe" },
                { name: "HttpRedirectTask", displayText: "Http Redirect Task", category: "HTTP", isEvent: false, hasEditor: true, thumbnailHtml: "<h4>Redirect</h4>", icon: "fa-solid fa-share" },
            ],
        },
        {
            name: "Primitives",
            activities: [{ name: "NotifyTask", displayText: "Notify Task", category: "Primitives", isEvent: false, hasEditor: true, thumbnailHtml: "<h4>Notify</h4>", icon: "fa-solid fa-bell" }],
        },
    ],
};
