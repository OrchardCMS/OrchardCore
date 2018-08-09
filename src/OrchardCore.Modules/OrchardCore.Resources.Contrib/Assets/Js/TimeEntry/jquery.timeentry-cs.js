/* http://keith-wood.name/timeEntry.html
   Czech initialisation for the jQuery time entry extension
   Written by Stanislav Kurinec (stenly.kurinec@gmail.com)  */
(function($) {
	$.timeEntry.regionalOptions['cs'] = {show24Hours: true, separator: ':',
		ampmPrefix: '', ampmNames: ['Dop', 'Odp'],
		spinnerTexts: ['Nyní', 'Předchozí pole', 'Následující pole', 'Zvýšit', 'Snížit']};
	$.timeEntry.setDefaults($.timeEntry.regionalOptions['cs']);
})(jQuery);
