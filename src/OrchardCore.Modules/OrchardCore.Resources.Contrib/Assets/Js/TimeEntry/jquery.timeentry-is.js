/* http://keith-wood.name/timeEntry.html
   Icelandic initialisation for the jQuery time entry extension
   Written by Már Örlygsson (http://mar.anomy.net/) */
(function($) {
	$.timeEntry.regionalOptions['is'] = {show24Hours: true, separator: ':',
		ampmPrefix: '', ampmNames: ['fh', 'eh'],
		spinnerTexts: ['Núna', 'Fyrra svæði', 'Næsta svæði', 'Hækka', 'Lækka']};
	$.timeEntry.setDefaults($.timeEntry.regionalOptions['is']);
})(jQuery);
