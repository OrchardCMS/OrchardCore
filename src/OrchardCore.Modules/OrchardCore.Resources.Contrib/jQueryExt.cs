using OrchardCore.ResourceManagement;

namespace OrchardCore.Resources.Contrib
{
    public class jQueryExt : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();
            // jQuery UI (full package).
            manifest.DefineScript("jQueryUI").SetUrl("/OrchardCore.Resources.Contrib/Scripts/jquery-ui.min.js", "/OrchardCore.Resources.Contrib/Scripts/jquery-ui.js")
                .SetVersion("1.11.4")
                .SetDependencies("jQuery")
                .SetCdn("//ajax.aspnetcdn.com/ajax/jquery.ui/1.11.4/jquery-ui.min.js", "//ajax.aspnetcdn.com/ajax/jquery.ui/1.11.4/jquery-ui.js", true);
            manifest.DefineStyle("jQueryUI").SetUrl("/OrchardCore.Resources.Contrib/Styles/jquery-ui.min.css", "/OrchardCore.Resources.Contrib/Styles/jquery-ui.css").SetVersion("1.11.4");
            manifest.DefineStyle("jQueryUI_Orchard").SetDependencies("jQueryUI"); // Right now no customization in the styles, but the resource might be used later.

            manifest.DefineScript("jQueryUI_Sortable").SetDependencies("jQueryUI");
            manifest.DefineScript("jQueryUI_Widget").SetDependencies("jQueryUI");

            // Additional utilities and plugins.
            manifest.DefineScript("jQueryUtils").SetUrl("/OrchardCore.Resources.Contrib/Scripts/jquery.utils.min.js", "/OrchardCore.Resources.Contrib/Scripts/jquery.utils.js")
                .SetDependencies("jQuery");
            manifest.DefineScript("jQueryPlugin").SetUrl("/OrchardCore.Resources.Contrib/Styles/jquery.plugin.min.js", "/OrchardCore.Resources.Contrib/Styles/jquery.plugin.js")
                .SetDependencies("jQuery");
            // jQuery Calendars.
            manifest.DefineScript("jQueryCalendars").SetUrl("/OrchardCore.Resources.Contrib/Scripts/Calendars/jquery.calendars.all.min.js", "/OrchardCore.Resources.Contrib/Scripts/Calendars/jquery.calendars.all.js")
                .SetDependencies("jQueryPlugin").SetVersion("2.0.1");
            manifest.DefineScript("jQueryCalendars_Picker")
                .SetUrl("/OrchardCore.Resources.Contrib/Scripts/Calendars/jquery.calendars.picker.full.min.js", "/OrchardCore.Resources.Contrib/Scripts/Calendars/jquery.calendars.picker.full.js")
                .SetDependencies("jQueryCalendars").SetVersion("2.0.1");
            manifest.DefineStyle("jQueryCalendars_Picker")
                .SetUrl("/OrchardCore.Resources.Contrib/Styles/Calendars/jquery.calendars.picker.full.min.css", "/OrchardCore.Resources.Contrib/Styles/Calendars/jquery.calendars.picker.full.css")
                .SetDependencies("jQueryUI_Orchard").SetVersion("2.0.1");

            // jQuery Time Entry.
            manifest.DefineScript("jQueryTimeEntry").SetUrl("/OrchardCore.Resources.Contrib/Scripts/TimeEntry/jquery.timeentry.min.js", "/OrchardCore.Resources.Contrib/Scripts/TimeEntry/jquery.timeentry.js")
                .SetDependencies("jQueryPlugin").SetVersion("2.0.1");
            manifest.DefineStyle("jQueryTimeEntry").SetUrl("/OrchardCore.Resources.Contrib/Styles/TimeEntry/jquery.timeentry.min.css", "/OrchardCore.Resources.Contrib/Styles/TimeEntry/jquery.timeentry.css").SetVersion("2.0.1");

            manifest.DefineStyle("DateTimeEditor").SetUrl("/OrchardCore.Resources.Contrib/Styles/datetime-editor.css");

            // jQuery Date/Time Editor Enhancements.
            manifest.DefineStyle("jQueryDateTimeEditor").SetUrl("/OrchardCore.Resources.Contrib/Styles/jquery-datetime-editor.min.css", "/OrchardCore.Resources.Contrib/Styles/jquery-datetime-editor.css").
                SetDependencies("DateTimeEditor");

            // jQuery File Upload.
            manifest.DefineScript("jQueryFileUpload").SetUrl("/OrchardCore.Resources.Contrib/Scripts/jquery.fileupload-full.min.js", "/OrchardCore.Resources.Contrib/Scripts/jquery.fileupload-full.js").SetVersion("9.11.2")
                .SetDependencies("jQueryUI_Widget");

            // jQuery Color Box.
            manifest.DefineScript("jQueryColorBox").SetUrl("/OrchardCore.Resources.Contrib/Scripts/jquery.colorbox.min.js", "/OrchardCore.Resources.Contrib/Scripts/jquery.colorbox.js").SetVersion("1.6.3").SetDependencies("jQuery");
            manifest.DefineStyle("jQueryColorBox").SetUrl("/OrchardCore.Resources.Contrib/Styles/jquery.colorbox.min.css", "/OrchardCore.Resources.Contrib/Styles/jquery.colorbox.css").SetVersion("1.6.3");

            // jQuery Cookie.
            manifest.DefineScript("jQueryCookie").SetUrl("/OrchardCore.Resources.Contrib/Scripts/jquery.cookie.min.js", "/OrchardCore.Resources.Contrib/Scripts/jquery.cookie.js").SetVersion("1.4.1")
                .SetDependencies("jQuery");

        }
    }
}
