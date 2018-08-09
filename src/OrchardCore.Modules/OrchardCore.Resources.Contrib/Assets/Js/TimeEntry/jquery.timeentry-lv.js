/* http://keith-wood.name/timeEntry.html
   Latvian (UTF-8) initialisation for the jQuery $.timeEntry extension.
   Written by Rihards Prieditis (rprieditis@gmail.com). */
(function($) {
	$.timeEntry.regionalOptions['lv'] = {show24Hours: true, separator: ':',
		ampmPrefix: '', ampmNames: ['AM', 'PM'],
		spinnerTexts: ['Pašlaik', 'Iepriekšējais lauks', 'Nākamais lauks', 'Palielināt', 'Samazināt']};
	$.timeEntry.setDefaults($.timeEntry.regionalOptions['lv']);
})(jQuery);