/* http://keith-wood.name/timeEntry.html
   Slovak initialisation for the jQuery time entry extension
   Written by Vojtech Rinik (vojto@hmm.sk)  */
(function($) {
	$.timeEntry.regionalOptions['sk'] = {show24Hours: false, separator: ':',
		ampmPrefix: '', ampmNames: ['AM', 'PM'],
		spinnerTexts: ['Teraz', 'Predchádzajúce pole', 'Nasledujúce pole', 'Zvýšiť', 'Znížiť']};
	$.timeEntry.setDefaults($.timeEntry.regionalOptions['sk']);
})(jQuery);
