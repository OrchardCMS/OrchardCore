// Defined by OrchardCore.Notifications (Assets/js/notification-manager.js), consumed here.
declare const notificationManager: {
    initializeContainer(markAsReadUrl: string, badgeSelector: string): void;
};

const container = document.querySelector<HTMLElement>(".notification-navbar-container");

if (container) {
    notificationManager.initializeContainer(container.dataset.markAsReadUrl ?? "", "#UnreadNotificationBadge");
}
