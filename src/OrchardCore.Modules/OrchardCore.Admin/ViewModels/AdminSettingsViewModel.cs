/*
	defines the class "AdminSettignsViewModel"
		with a flag field "DisplayMenuFilter"
*/

using System;

namespace OrchardCore.Admin.ViewModels
{
	public class AdminSettingsViewModel
	{
		// flag field, (apparently) to display the menu filter
		public bool DisplayMenuFilter {get; set; }
	}
}
