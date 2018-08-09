/* http://keith-wood.name/timeEntry.html
   Hungarian initialisation for the jQuery time entry extension
   Written by Karaszi Istvan (raszi@spam.raszi.hu)  */
(function($) {
	$.timeEntry.regionalOptions['hu'] = {show24Hours: true,  separator: ':',
		ampmPrefix: '', ampmNames: ['DE', 'DU'],
		spinnerTexts: ['Most', 'Előző mező', 'Következő mező', 'Növel', 'Csökkent']};
	$.timeEntry.setDefaults($.timeEntry.regionalOptions['hu']);
})(jQuery);
