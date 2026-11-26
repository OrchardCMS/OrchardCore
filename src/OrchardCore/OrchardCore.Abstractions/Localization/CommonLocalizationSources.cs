namespace OrchardCore.Localization;

/// <summary>
/// Untranslated strings that several modules use with the same meaning, such as the categories of deployment steps
/// that they share, or that are general to every module.
/// </summary>
/// <remarks>
/// Reuse these sources instead of declaring the same text in each module, so that it is translated once. Add a
/// source only when it is used by more than one project, or is general to every module, such as "Settings".
/// The root sources use the PO context <c>OrchardCore.Localization.CommonLocalizationSources</c>.
/// Nested classes separate meanings that have the same English text. Property names and translator notes
/// alone do not distinguish translations: the source type and value identify a PO entry.
/// Resolve sources with the current tenant's localizer factory; never cache translated values statically.
/// Declare the strings
/// that are specific to a module in an internal class of the module, and keep the strings that are
/// specific to a view in the view, with its localizer.
/// </remarks>
public static class CommonLocalizationSources
{
    /// <summary>The "Configuration" category for configuration and deployment settings.</summary>
    public static readonly LocalizationSource Configuration = LocalizationSource.Create("Configuration", typeof(CommonLocalizationSources),
        note: "Category noun for application configuration and configuration deployment steps.");

    /// <summary>The "Content" category for site content, not the body of a field or message.</summary>
    public static readonly LocalizationSource Content = LocalizationSource.Create("Content", typeof(CommonLocalizationSources),
        note: "Category noun for the site's content, not a field value or message body.");

    /// <summary>The "Content Management" category for managing site content.</summary>
    public static readonly LocalizationSource ContentManagement = LocalizationSource.Create("Content Management", typeof(CommonLocalizationSources),
        note: "Category for managing site content and deploying content definitions or items.");

    /// <summary>The "Development" category for developer tools.</summary>
    public static readonly LocalizationSource Development = LocalizationSource.Create("Development", typeof(CommonLocalizationSources),
        note: "Category noun for developer tools and related deployment steps.");

    /// <summary>The "Infrastructure" category for application infrastructure.</summary>
    public static readonly LocalizationSource Infrastructure = LocalizationSource.Create("Infrastructure", typeof(CommonLocalizationSources),
        note: "Category noun for application infrastructure and related deployment steps.");

    /// <summary>The "Search" category noun. Use <see cref="Commands.Search"/> for a search command.</summary>
    public static readonly LocalizationSource Search = LocalizationSource.Create("Search", typeof(CommonLocalizationSources),
        note: "Category noun for search functionality, indexing and search-related deployment steps; not a command.");

    /// <summary>The "Security" category for application security.</summary>
    public static readonly LocalizationSource Security = LocalizationSource.Create("Security", typeof(CommonLocalizationSources),
        note: "Category noun for application security and related deployment steps.");

    /// <summary>The "Settings" heading for configuration menus, editor sections and categories.</summary>
    public static readonly LocalizationSource Settings = LocalizationSource.Create("Settings", typeof(CommonLocalizationSources),
        note: "Plural noun heading for configuration settings in menus, editor sections and categories.");

    /// <summary>The "General" heading for an editor's general settings section or menu.</summary>
    public static readonly LocalizationSource General = LocalizationSource.Create("General", typeof(CommonLocalizationSources),
        note: "Standalone heading for general settings in an editor section or configuration menu.");

    /// <summary>The "Cancel" command that dismisses an editor or dialog without accepting changes. Use <see cref="RunningOperations.Cancel"/> to stop execution.</summary>
    public static readonly LocalizationSource Cancel = LocalizationSource.Create("Cancel", typeof(CommonLocalizationSources),
        note: "Command to dismiss an editor or dialog without accepting changes; not to stop a running operation.");

    /// <summary>The "Delete" command that deletes an entity, including a bulk action or confirmation title.</summary>
    public static readonly LocalizationSource Delete = LocalizationSource.Create("Delete", typeof(CommonLocalizationSources),
        note: "Command to delete an entity or entities; also used as the title of its confirmation dialog.");

    /// <summary>The "Edit" command that opens an existing entity's editor.</summary>
    public static readonly LocalizationSource Edit = LocalizationSource.Create("Edit", typeof(CommonLocalizationSources),
        note: "Command to open the editor of an existing entity.");

    /// <summary>The "Save" command that persists editor changes.</summary>
    public static readonly LocalizationSource Save = LocalizationSource.Create("Save", typeof(CommonLocalizationSources),
        note: "Command to persist changes made in an editor, including a preview of that button.");

    /// <summary>The "Save and continue" command that saves changes and stays in the editor.</summary>
    public static readonly LocalizationSource SaveAndContinue = LocalizationSource.Create("Save and continue", typeof(CommonLocalizationSources),
        note: "Command to save changes and continue editing the same entity.");

    /// <summary>The "Actions" heading for commands on items, including bulk actions. Use <see cref="Workflow.Actions"/> for workflow action definitions.</summary>
    public static readonly LocalizationSource Actions = LocalizationSource.Create("Actions", typeof(CommonLocalizationSources),
        note: "Plural noun for a menu, toolbar or column of commands available on items, including bulk actions.");

    /// <summary>The "Save actions" accessible label for a group of save commands.</summary>
    public static readonly LocalizationSource SaveActions = LocalizationSource.Create("Save actions", typeof(CommonLocalizationSources),
        note: "Accessible label for a button group offering save commands.");

    /// <summary>The "Toggle Dropdown" accessible command that opens or closes a dropdown.</summary>
    public static readonly LocalizationSource ToggleDropdown = LocalizationSource.Create("Toggle Dropdown", typeof(CommonLocalizationSources),
        note: "Accessible command label for opening or closing a dropdown menu.");

    /// <summary>The "Status" heading for an entity's state or an operation's execution state.</summary>
    public static readonly LocalizationSource Status = LocalizationSource.Create("Status", typeof(CommonLocalizationSources),
        note: "Noun heading for an entity's current state or an operation's execution state; not an HTTP status code.");

    /// <summary>The "See documentation" command that opens documentation.</summary>
    public static readonly LocalizationSource SeeDocumentation = LocalizationSource.Create("See documentation", typeof(CommonLocalizationSources),
        note: "Link command to open documentation about the current feature or editor.");

    /// <summary>The "Ok" command that confirms a dialog, preserving the original capitalization.</summary>
    public static readonly LocalizationSource Ok = LocalizationSource.Create("Ok", typeof(CommonLocalizationSources),
        note: "Dialog confirmation command; the English spelling is intentionally 'Ok'.");

    /// <summary>The "Yes" affirmative response to a confirmation question. Use <see cref="BooleanValues.Yes"/> for a boolean value.</summary>
    public static readonly LocalizationSource Yes = LocalizationSource.Create("Yes", typeof(CommonLocalizationSources),
        note: "Affirmative response to a confirmation question in a dialog.");

    /// <summary>The "No" negative response to a confirmation question. Use <see cref="BooleanValues.No"/> for a boolean value.</summary>
    public static readonly LocalizationSource No = LocalizationSource.Create("No", typeof(CommonLocalizationSources),
        note: "Negative response to a confirmation question in a dialog.");

    /// <summary>The singular "Content item" noun for an Orchard content entity, including editor headings and workflow value types.</summary>
    public static readonly LocalizationSource ContentItem = LocalizationSource.Create("Content item", typeof(CommonLocalizationSources),
        note: "Singular noun for an Orchard content entity, used as a heading, picker label or workflow value type.");

    /// <summary>The title-case "Content Item" heading for an Orchard content entity. Use <see cref="ContentItem"/> for sentence case.</summary>
    public static readonly LocalizationSource ContentItemTitle = LocalizationSource.Create("Content Item", typeof(CommonLocalizationSources),
        note: "Title-case heading identifying an Orchard content entity in deployment summaries.");

    /// <summary>The plural "Content types" noun for Orchard content-type definitions, in sentence case.</summary>
    public static readonly LocalizationSource ContentTypes = LocalizationSource.Create("Content types", typeof(CommonLocalizationSources),
        note: "Plural noun for Orchard content-type definitions, used in editor section headings and selection labels.");

    /// <summary>The title-case "Content Types" noun for Orchard content-type definitions. Use <see cref="ContentTypes"/> for sentence case.</summary>
    public static readonly LocalizationSource ContentTypesTitle = LocalizationSource.Create("Content Types", typeof(CommonLocalizationSources),
        note: "Title-case plural noun for Orchard content-type definitions, used in navigation, headings and selection labels.");

    /// <summary>The "Name" label for the name of an entity. Use <see cref="TechnicalNames.Name"/> for a technical identifier and <see cref="UserNames.Name"/> for a user's name.</summary>
    public static readonly LocalizationSource Name = LocalizationSource.Create("Name", typeof(CommonLocalizationSources),
        note: "Noun label for the name of an entity, such as a template, role, tenant, menu, file or pipeline; not a user's name or a technical field identifier.");

    /// <summary>The "Standard" option for a default content-part or field editor. Use <see cref="DisplayModes.Standard"/> for a display mode.</summary>
    public static readonly LocalizationSource StandardEditor = LocalizationSource.Create("Standard", typeof(CommonLocalizationSources),
        note: "Option selecting the default editor of a content part or field; not a display mode or form validation option.");

    /// <summary>The "Enabled" state of an entity or feature. Use <see cref="BooleanFields.Enabled"/> for a boolean field label and <see cref="AuditEvents.Enabled"/> for an event.</summary>
    public static readonly LocalizationSource EnabledState = LocalizationSource.Create("Enabled", typeof(CommonLocalizationSources),
        note: "Standalone state of an enabled entity or feature, shown as a badge or state option; not a boolean field caption or past event.");

    /// <summary>The "Disabled" state of an entity or feature. Use <see cref="BooleanFields.Disabled"/> for a boolean field label and <see cref="AuditEvents.Disabled"/> for an event.</summary>
    public static readonly LocalizationSource DisabledState = LocalizationSource.Create("Disabled", typeof(CommonLocalizationSources),
        note: "Standalone state of a disabled entity or feature, shown as a badge or state option; not a boolean field caption or past event.");

    /// <summary>The "Remove" command that removes a selection or relationship without deleting the referenced entity. Use <see cref="Commands.Remove"/> for deletion.</summary>
    public static readonly LocalizationSource Remove = LocalizationSource.Create("Remove", typeof(CommonLocalizationSources),
        note: "Command to remove a selection, relationship or editor row without deleting the referenced entity.");

    /// <summary>Command meanings that differ from the root categories or relationship labels.</summary>
    public static class Commands
    {
        /// <summary>The "Search" command or prompt to enter search terms. Use <see cref="CommonLocalizationSources.Search"/> for the search category.</summary>
        public static readonly LocalizationSource Search = LocalizationSource.Create("Search", typeof(Commands),
            note: "Command to perform a search, or the prompt on a search box; not the search category or an HTML input type.");

        /// <summary>The "Remove" command that deletes a record or credential. Use <see cref="CommonLocalizationSources.Remove"/> for deselection.</summary>
        public static readonly LocalizationSource Remove = LocalizationSource.Create("Remove", typeof(Commands),
            note: "Command to delete a stored record or credential, such as a notification, tenant, run or passkey; not merely to deselect it.");
    }

    /// <summary>Commands for cancelling execution, separate from dismissing an editor or dialog.</summary>
    public static class RunningOperations
    {
        /// <summary>The "Cancel" command that stops a running workflow or pipeline. Use <see cref="CommonLocalizationSources.Cancel"/> to dismiss an editor or dialog.</summary>
        public static readonly LocalizationSource Cancel = LocalizationSource.Create("Cancel", typeof(RunningOperations),
            note: "Command to stop execution of a running workflow or data pipeline; not to dismiss an editor or dialog.");
    }

    /// <summary>Workflow outcomes and user-task definitions, separate from general UI labels.</summary>
    public static class Workflow
    {
        /// <summary>The "Done" completion outcome of a workflow activity; completion does not necessarily mean validation succeeded.</summary>
        public static readonly LocalizationSource Done = LocalizationSource.Create("Done", typeof(Workflow),
            note: "Workflow activity completion outcome. Completion does not necessarily mean that validation succeeded.");

        /// <summary>The "Actions" label for the names of the actions defined by a workflow user task, not an item-action menu.</summary>
        public static readonly LocalizationSource Actions = LocalizationSource.Create("Actions", typeof(Workflow),
            note: "Plural noun for the action names defined by a workflow user task, which determine its possible outcomes.");

        /// <summary>The "Enabled" outcome after an activity enables a tenant, not the tenant's current state.</summary>
        public static readonly LocalizationSource Enabled = LocalizationSource.Create("Enabled", typeof(Workflow),
            note: "Workflow outcome indicating that the activity enabled a tenant; not a current-state badge.");

        /// <summary>The "Disabled" outcome after an activity disables a tenant, not the tenant's current state.</summary>
        public static readonly LocalizationSource Disabled = LocalizationSource.Create("Disabled", typeof(Workflow),
            note: "Workflow outcome indicating that the activity disabled a tenant; not a current-state badge.");
    }

    /// <summary>Boolean values, separate from answers to confirmation questions.</summary>
    public static class BooleanValues
    {
        /// <summary>The "Yes" value for a true boolean, not a dialog confirmation.</summary>
        public static readonly LocalizationSource Yes = LocalizationSource.Create("Yes", typeof(BooleanValues),
            note: "Value representing true in a boolean selector or read-only property; not a dialog response.");

        /// <summary>The "No" value for a false boolean, not a dialog response.</summary>
        public static readonly LocalizationSource No = LocalizationSource.Create("No", typeof(BooleanValues),
            note: "Value representing false in a boolean selector or read-only property; not a dialog response.");
    }

    /// <summary>Captions of boolean fields, separate from states and past events.</summary>
    public static class BooleanFields
    {
        /// <summary>The "Enabled" caption of a boolean field that controls whether something is enabled.</summary>
        public static readonly LocalizationSource Enabled = LocalizationSource.Create("Enabled", typeof(BooleanFields),
            note: "Caption of a boolean field controlling whether an entity or feature is enabled; not its current-state badge or a past event.");

        /// <summary>The "Disabled" caption of a boolean field that controls whether something is disabled.</summary>
        public static readonly LocalizationSource Disabled = LocalizationSource.Create("Disabled", typeof(BooleanFields),
            note: "Caption of a boolean field controlling whether functionality is disabled; not its current-state badge or a past event.");
    }

    /// <summary>Past events recorded in an audit trail, separate from current states.</summary>
    public static class AuditEvents
    {
        /// <summary>The "Enabled" event indicating that a user was enabled.</summary>
        public static readonly LocalizationSource Enabled = LocalizationSource.Create("Enabled", typeof(AuditEvents),
            note: "Past audit event indicating that a user was enabled; not a current-state badge or boolean field.");

        /// <summary>The "Disabled" event indicating that a user was disabled.</summary>
        public static readonly LocalizationSource Disabled = LocalizationSource.Create("Disabled", typeof(AuditEvents),
            note: "Past audit event indicating that a user was disabled; not a current-state badge or boolean field.");
    }

    /// <summary>Labels of technical identifiers, separate from entity display names.</summary>
    public static class TechnicalNames
    {
        /// <summary>The "Name" label for a property, variable, field, header, scope or other technical identifier.</summary>
        public static readonly LocalizationSource Name = LocalizationSource.Create("Name", typeof(TechnicalNames),
            note: "Noun label for a technical identifier, such as a property, variable, field, HTTP header, OpenID scope or time zone; not an entity's display name.");
    }

    /// <summary>Labels of user names, separate from entity and technical names.</summary>
    public static class UserNames
    {
        /// <summary>The "Name" label or sort option for a user's name.</summary>
        public static readonly LocalizationSource Name = LocalizationSource.Create("Name", typeof(UserNames),
            note: "Noun label or sort option for a user's name; not the name of a template, tenant or technical field.");
    }

    /// <summary>Content display-mode options, separate from editor options.</summary>
    public static class DisplayModes
    {
        /// <summary>The "Standard" option selecting the default display mode of a content part or field.</summary>
        public static readonly LocalizationSource Standard = LocalizationSource.Create("Standard", typeof(DisplayModes),
            note: "Option selecting the default display mode of a content part or field; not its editor or form validation.");
    }

    /// <summary>Form-label rendering options, separate from content editors and validation.</summary>
    public static class FormLabels
    {
        /// <summary>The "Standard" option selecting normal label rendering for a form element.</summary>
        public static readonly LocalizationSource Standard = LocalizationSource.Create("Standard", typeof(FormLabels),
            note: "Option selecting normal label rendering for a form element; not a content editor or validation mode.");
    }

    /// <summary>Form-validation options, separate from content editors and label rendering.</summary>
    public static class FormValidation
    {
        /// <summary>The "Standard" option selecting normal validation rendering for a form element.</summary>
        public static readonly LocalizationSource Standard = LocalizationSource.Create("Standard", typeof(FormValidation),
            note: "Option selecting normal validation rendering for a form element; not a content editor or label mode.");
    }

    /// <summary>HTML input-type names, separate from UI commands.</summary>
    public static class InputTypes
    {
        /// <summary>The "Search" name of the HTML search input type, not a command or category.</summary>
        public static readonly LocalizationSource Search = LocalizationSource.Create("Search", typeof(InputTypes),
            note: "Name of the HTML search input type in a form-element type selector; not a command or search category.");
    }
}
