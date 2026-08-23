declare const Vue: {
    createApp(options: Record<string, unknown>): { mount(selector: string | Element): void };
};

const element = document.getElementById("userRolesVue");

if (element) {
    Vue.createApp({
        data() {
            return { displayAllUsers: element.dataset.displayAllUsers === "true" };
        },
    }).mount(element);
}
