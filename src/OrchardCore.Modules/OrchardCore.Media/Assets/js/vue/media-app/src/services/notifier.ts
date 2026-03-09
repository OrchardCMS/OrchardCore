import dbg from "debug";
import {
  notify as bloomNotify,
  notifiersClose,
  registerNotificationBus,
} from "@bloom/services/notifications/notifier";

const debug = dbg("notifier");

/**
 * Notify a message through a mitt even
 * @param { { summary, detail, severity } } message The notification metadata
 */
export function notify(message) {
  debug(message);
  bloomNotify(message);
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
  } else if (error.detail || error.title) {
    // ProblemDetails object thrown directly by the API client (e.g. 401, 403, 404).
    return error.detail ?? error.title;
  } else {
    // Here for example we have an API connection error which would return
    // a global server error.
    return error.message;
  }
}
