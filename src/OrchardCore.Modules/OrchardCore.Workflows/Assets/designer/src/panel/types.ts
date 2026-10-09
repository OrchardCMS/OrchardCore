import type { FormFragment } from "../api/types";

/**
 * The result of posting a server-rendered form: either applied, or the form rendered again with errors.
 */
export type FormApplyResult = { valid: true } | (FormFragment & { valid: false });

export const AUTO_APPLY_DELAY = 600;
