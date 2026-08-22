// Defined by this module's own Assets/Admin/cors-admin.js (classic global, a Vue 2 instance).
declare const corsApp: { policies: unknown };

// #corsSettings already carries this same JSON as its value attribute (read by the classic
// script's own submit handler), so this reads it back rather than duplicating the serialization.
const corsSettings = document.getElementById("corsSettings") as HTMLInputElement | null;

if (corsSettings) {
    corsApp.policies = JSON.parse(corsSettings.value);
}
