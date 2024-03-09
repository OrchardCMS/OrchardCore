import mitt from "mitt";
import dbg from "debug";

const debug = dbg("notifier");

declare global {
  interface Window {
    notificationBus: any;
  }
}

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
  debug(message);
  window.notificationBus.emit("notify", message);
}

/**
 * Adds close notifier(s) functionality
 */
export function notifiersClose() {
  const notifiers = document.querySelectorAll<HTMLElement>('.p-message[role="alert"]');

  if (notifiers.length > 0) {
    notifiers.forEach((notifier) => {
      let btnClose = notifier.querySelector(":scope > .p-message-wrapper > .p-message-close");

      if (btnClose) {
        btnClose.addEventListener("click", (e: Event) => notifier.classList.add("hidden"));
      }

      setInterval(function () {
        notifier.style.transition = "0.8s";
        notifier.style.opacity = "0";
      }, 5000);
    });
  }
}

/**
 * Parses an error object and return proper message.
 * Use with TSClient.
 * @param error
 * @returns string
 */
export async function tryGetErrorMessage(error) {
  debug(error);

  if (error.response) {
    if (error.response.constructor.name === "Blob" && error.response.type === "application/problem+json") {
      // Here we received an error from a Problem interface.
      const message = JSON.parse(await error.response.text());
      return message.detail;
    } else if (error.response.constructor.name === "Blob" && error.response.type === "text/html") {
      // Here for example we could have received a 404 error which would be returned as HTML
      return error.message;
    } else {
      // Here the Problem interface did not return a blob but only a message.
      return error.response.detail ?? error.response.title;
    }
  } else {
    // Here for example we have an API connection error which would return
    // a global server error.
    return error.message;
  }
}
