/*
	defines the class "AdminSettignsViewModel"
		with the flag field "DisplayMenuFilter"
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
