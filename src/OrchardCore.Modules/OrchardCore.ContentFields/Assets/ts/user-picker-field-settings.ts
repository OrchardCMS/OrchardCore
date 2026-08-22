interface UserRolesVueInstance {
    $el: HTMLElement;
    displayAllUsers: boolean;
}

declare const Vue: new (options: {
    el: string;
    data(): { displayAllUsers: boolean };
    mounted(this: UserRolesVueInstance): void;
}) => UserRolesVueInstance;

new Vue({
    el: "#userRolesVue",
    data() {
        return { displayAllUsers: false };
    },
    mounted() {
        this.displayAllUsers = this.$el.dataset.displayAllUsers === "true";
    },
});
