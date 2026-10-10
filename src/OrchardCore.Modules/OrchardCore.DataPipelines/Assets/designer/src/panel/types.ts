import type { FormFragment } from "../api/types";

/**
 * The result of posting a server-rendered form: either applied (and, with `reloadEditor`, to be loaded again because
 * the applied settings change the form), or the form rendered again with errors.
 */
export type FormApplyResult = { valid: true; reloadEditor?: boolean } | (FormFragment & { valid: false });

export const AUTO_APPLY_DELAY = 600;
