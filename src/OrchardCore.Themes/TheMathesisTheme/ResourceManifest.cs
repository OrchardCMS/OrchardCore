using OrchardCore.ResourceManagement;

namespace TheMathesisTheme
{
	public class ResourceManifest : IResourceManifestProvider
	{
		public void BuildManifests(IResourceManifestBuilder builder)
		{
			var manifest = builder.Add();

			manifest
			 .DefineStyle("TheMathesisTheme-Roboto")
			 .SetCdn("https://fonts.googleapis.com/css?family=Roboto:300,400,500,700");

			manifest.DefineStyle("TheMathesisTheme-Roboto-Slab")
			 .SetCdn("https://fonts.googleapis.com/css?family=Roboto+Slab:400,700");

			manifest
			 .DefineStyle("TheMathesisTheme-Material-Icons")
			 .SetCdn("https://fonts.googleapis.com/css?family=Material+Icons");

			manifest
			 .DefineStyle("TheMathesisTheme-Montserrat")
			 .SetCdn("https://fonts.googleapis.com/css?family=Montserrat:400,700");

			manifest
			 .DefineStyle("TheMathesisTheme-Droid-Serif")
			 .SetCdn("https://fonts.googleapis.com/css?family=Droid+Serif:400,700,400italic,700italic");

			manifest
			 .DefineStyle("TheMathesisTheme-Font-Awesome")
			 .SetCdn("https://maxcdn.bootstrapcdn.com/font-awesome/latest/css/font-awesome.min.css");

			manifest
			 .DefineStyle("TheMathesisTheme-Material-Kit")
			 .SetUrl("~/TheMathesisTheme/assets/css/material-kit.css?v=2.2.0");

			manifest
			 .DefineStyle("TheMathesisTheme-Mathesis-Kit")
			 .SetDependencies("TheMathesisTheme-Material-Kit")
			 .SetUrl("~/TheMathesisTheme/assets/css/mathesis-kit.css");

			manifest
			 .DefineStyle("TheMathesisTheme-FancyTree-Css")
			 .SetDependencies("TheMathesisTheme-Mathesis-Kit")
			 .SetUrl("~/TheMathesisTheme/assets/css/ui.fancytree.css");

			manifest
			 .DefineStyle("TheMathesisTheme-JqCloud-Css")
			 .SetDependencies("TheMathesisTheme-FancyTree-Css")
			 .SetUrl("~/TheMathesisTheme/assets/css/jqcloud.css");

			manifest
			 .DefineScript("TheMathesisTheme-jQuery")
			 .SetUrl("~/TheMathesisTheme/assets/js/core/jquery.min.js");

			manifest
			 .DefineScript("TheMathesisTheme-jQuery-Ui")
			 .SetDependencies("TheMathesisTheme-jQuery")
			 .SetUrl("~/TheMathesisTheme/assets/js/core/jquery-ui.min.js");

			manifest
			 .DefineScript("TheMathesisTheme-Popper")
			 .SetDependencies("TheMathesisTheme-jQuery")
			 .SetUrl("~/TheMathesisTheme/assets/js/core/popper.min.js");

			manifest
			 .DefineScript("TheMathesisTheme-Bootstrap-Material")
			 .SetDependencies("TheMathesisTheme-jQuery")
			 .SetDependencies("TheMathesisTheme-Popper")
			 .SetUrl("~/TheMathesisTheme/assets/js/core/bootstrap-material-design.min.js");

			manifest
			 .DefineScript("TheMathesisTheme-Moment")
			 .SetDependencies("TheMathesisTheme-jQuery")
			 .SetUrl("~/TheMathesisTheme/assets/js/plugins/moment.min.js");

			manifest
			 .DefineScript("TheMathesisTheme-Bootstrap-Datepicker")
			 .SetDependencies("TheMathesisTheme-jQuery")
			 .SetUrl("~/TheMathesisTheme/assets/js/plugins/bootstrap-datetimepicker.js");

			manifest
			 .DefineScript("TheMathesisTheme-Nouslider")
			 .SetDependencies("TheMathesisTheme-jQuery")
			 .SetUrl("~/TheMathesisTheme/assets/js/plugins/nouislider.min.js");

			manifest
			 .DefineScript("TheMathesisTheme-Bootstrap-Tags-Input")
			 .SetDependencies("TheMathesisTheme-jQuery")
			 .SetUrl("~/TheMathesisTheme/assets/js/plugins/bootstrap-tagsinput.js");

			manifest
			 .DefineScript("TheMathesisTheme-Bootstrap-Select-Picker")
			 .SetDependencies("TheMathesisTheme-jQuery")
			 .SetUrl("~/TheMathesisTheme/assets/js/plugins/bootstrap-selectpicker.js");

			manifest
			 .DefineScript("TheMathesisTheme-Bootstrap-Jasny")
			 .SetDependencies("TheMathesisTheme-jQuery")
			 .SetUrl("~/TheMathesisTheme/assets/js/plugins/jasny-bootstrap.min.js");

			manifest
			 .DefineScript("TheMathesisTheme-Buttons")
			 .SetDependencies("TheMathesisTheme-jQuery")
			 .SetCdn("https://buttons.github.io/buttons.js");

			manifest
			 .DefineScript("TheMathesisTheme-Material-Kit-Script")
			 .SetDependencies("TheMathesisTheme-Bootstrap-Material")
			 .SetUrl("~/TheMathesisTheme/assets/js/material-kit.js?v=2.1.0");

			manifest
			 .DefineScript("TheMathesisTheme-Mathesis-Script")
			 .SetDependencies("TheMathesisTheme-FancyTree-Filter")
			 .SetUrl("~/TheMathesisTheme/assets/js/mathesis.js");

			manifest
			 .DefineScript("TheMathesisTheme-FancyTree")
			 .SetDependencies("TheMathesisTheme-Material-Kit-Script")
			 .SetUrl("~/TheMathesisTheme/assets/js/modules/jquery.fancytree.js");

			manifest
			 .DefineScript("TheMathesisTheme-FancyTree-Glyph")
			 .SetDependencies("TheMathesisTheme-FancyTree")
			 .SetUrl("~/TheMathesisTheme/assets/js/modules/jquery.fancytree.glyph.js");

			manifest
			 .DefineScript("TheMathesisTheme-FancyTree-Wide")
			 .SetDependencies("TheMathesisTheme-FancyTree.Gliph")
			 .SetUrl("~/TheMathesisTheme/assets/js/modules/jquery.fancytree.wide.js");

			manifest
			 .DefineScript("TheMathesisTheme-FancyTree-Filter")
			 .SetDependencies("TheMathesisTheme-FancyTree.Wide")
			 .SetUrl("~/TheMathesisTheme/assets/js/modules/jquery.fancytree.filter.js");

			manifest
			 .DefineScript("TheMathesisTheme-JqCloud")
			 .SetDependencies("TheMathesisTheme-Material-Kit-Script")
			 .SetUrl("~/TheMathesisTheme/assets/js/plugins/jqcloud.js");
		}
	}
}