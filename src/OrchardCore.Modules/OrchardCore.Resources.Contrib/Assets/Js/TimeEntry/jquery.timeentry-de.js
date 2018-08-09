/* http://keith-wood.name/timeEntry.html
   German initialisation for the jQuery time entry extension
   Written by Eyk Schulz (eyk.schulz@gmx.net)  */
(function($) {
	$.timeEntry.regionalOptions['de'] = {show24Hours: true, separator: ':',
		ampmPrefix: '', ampmNames: ['AM', 'PM'],
		spinnerTexts: ['Jetzt', 'vorheriges Feld', 'nächstes Feld', 'hoch', 'runter']};
	$.timeEntry.setDefaults($.timeEntry.regionalOptions['de']);
})(jQuery);
