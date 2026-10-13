import type { FormFragment } from "../core/types";

/**
 * The result of posting a server-rendered form: either applied (and, with `reloadEditor`, to be loaded again because
 * the applied settings change the form), or the form rendered again with errors.
 */
export type FormApplyResult = { valid: true; reloadEditor?: boolean } | (FormFragment & { valid: false });

/**
 * How long a form waits after a field changed before applying it, in milliseconds.
 */
export const AUTO_APPLY_DELAY = 600;
