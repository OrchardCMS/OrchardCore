import type { LibraryActivity } from "./types";

// What a toolbox card adds: an activity type by its name, or a preset (LibraryActivity.preset) by its id after this
// prefix. The key is what the toolbox emits and drags, and what addActivity takes.
export const PRESET_KEY_PREFIX = "preset:";

export const toolboxKey = (activity: Pick<LibraryActivity, "name" | "preset">) => (activity.preset ? PRESET_KEY_PREFIX + activity.preset : activity.name);
