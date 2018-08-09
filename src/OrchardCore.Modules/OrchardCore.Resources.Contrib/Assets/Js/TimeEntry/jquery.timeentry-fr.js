/* http://keith-wood.name/timeEntry.html
   French initialisation for the jQuery time entry extension
   Written by Keith Wood (kbwood@iprimus.com.au) June 2007. */
(function($) {
	$.timeEntry.regionalOptions['fr'] = {show24Hours: true, separator: ':',
		ampmPrefix: '', ampmNames: ['AM', 'PM'],
		spinnerTexts: ['Maintenant', 'Précédent', 'Suivant', 'Augmenter', 'Diminuer']};
	$.timeEntry.setDefaults($.timeEntry.regionalOptions['fr']);
})(jQuery);
