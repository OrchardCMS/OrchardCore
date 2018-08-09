/* http://keith-wood.name/timeEntry.html
   Italian initialisation for the jQuery time entry extension
   Written by Apaella (apaella@gmail.com) June 2007. */
(function($) {
	$.timeEntry.regionalOptions['it'] = {show24Hours: true, separator: ':',
		ampmPrefix: '', ampmNames: ['AM', 'PM'],
		spinnerTexts: ['Adesso', 'Precedente', 'Successivo', 'Aumenta', 'Diminuisci']};
	$.timeEntry.setDefaults($.timeEntry.regionalOptions['it']);
})(jQuery);
