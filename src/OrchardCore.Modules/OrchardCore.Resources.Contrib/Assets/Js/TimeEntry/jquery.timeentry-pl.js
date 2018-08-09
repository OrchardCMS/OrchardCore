/* http://keith-wood.name/timeEntry.html
   Polish initialisation for the jQuery time entry extension. 
   Polish translation by Jacek Wysocki (jacek.wysocki@gmail.com). */
(function($) {
	$.timeEntry.regionalOptions['pl'] = {show24Hours: true, separator: ':',
		ampmPrefix: '', ampmNames: ['AM', 'PM'],
		spinnerTexts: ['Teraz', 'Poprzednie pole', 'Następne pole', 'Zwiększ wartość', 'Zmniejsz wartość']};
	$.timeEntry.setDefaults($.timeEntry.regionalOptions['pl']);
})(jQuery);
