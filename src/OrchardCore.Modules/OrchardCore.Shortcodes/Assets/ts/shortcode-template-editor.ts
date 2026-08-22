// Defined by this module's own Assets/js/shortcode-templates.js (classic global). Content/Usage/
// Name/Hint are top-level ShortcodeTemplateViewModel properties, so their ids are always the
// fixed literal property names - shared as-is by both Create.cshtml and Edit.cshtml.
declare function initializeShortcodesTemplateEditor(
    categoriesElement: Element | null,
    contentElement: Element | null,
    usageElement: Element | null,
    previewElement: Element | null,
    nameElement: Element | null,
    hintElement: Element | null,
): void;

initializeShortcodesTemplateEditor(
    document.getElementById("shortcodeCategories"),
    document.getElementById("Content"),
    document.getElementById("Usage"),
    document.getElementById("shortcodePreview"),
    document.getElementById("Name"),
    document.getElementById("Hint"),
);
