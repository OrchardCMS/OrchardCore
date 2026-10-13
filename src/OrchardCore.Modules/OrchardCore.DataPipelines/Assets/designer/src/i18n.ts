import { loadTranslations as loadSharedTranslations, setDefaultTexts, t } from "@bloom/designer/core/i18n";

// The UI strings are rendered by the server into config.translations (a key/value dictionary). Every call names its
// English text, used when the dictionary has no value for the key, so the designer works without translations. The
// shared components (@bloom/designer) speak of a draft; the designer names its pipeline instead.
const PIPELINE_TEXTS: Record<string, string> = {
    ConflictTitle: "The pipeline was changed elsewhere",
    PublishBlockedTitle: "The pipeline cannot be published",
    PublishTitle: "Publish the pipeline?",
    PublishWarningsMessage: "The pipeline has {0} warnings. Runs will use this version once it is published.",
    NoIssues: "No issues: the pipeline can be published.",
    VersionsIntro: "Each publish creates a version. Runs use the published version; restoring a version copies it into the draft.",
    NoVersions: "The pipeline was never published.",
    FitToContent: "Fit to the steps",
    LoadingForm: "Loading…",
};

export const loadTranslations = (values: Record<string, string> | null | undefined) => {
    setDefaultTexts(PIPELINE_TEXTS);
    loadSharedTranslations(values);
};

export { t };
