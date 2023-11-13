import mitt from "mitt";

/**
 * Registers the mitt object on the window
 * @returns the notificationBus mitt object
 */
export function registerNotificationBus() {
  if (window.notificationBus === undefined) {
    window.notificationBus = mitt();
  }

  return window.notificationBus;
}

/**
 * Notify a message through a mitt even
 * @param { { summary, detail, severity } } message The notification metadata
 */
export function notify(message) {
  window.notificationBus.emit('notify', message);
}