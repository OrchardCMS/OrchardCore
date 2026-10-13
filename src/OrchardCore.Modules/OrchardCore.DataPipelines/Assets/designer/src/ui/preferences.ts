import { createPreferences } from "@bloom/designer/ui/preferences";

// Per-browser conveniences of the designer (panel width, collapsed panes).
export const { readPreference, writePreference } = createPreferences("orchardcore:data-pipelines-designer:");
