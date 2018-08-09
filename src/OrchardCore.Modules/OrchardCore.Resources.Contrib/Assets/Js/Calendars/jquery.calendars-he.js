/* http://keith-wood.name/calendars.html
   Hebrew localisation for Gregorian/Julian calendars for jQuery.
   Written by Amir Hardon (ahardon at gmail dot com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['he'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['ינואר','פברואר','מרץ','אפריל','מאי','יוני',
		'יולי','אוגוסט','ספטמבר','אוקטובר','נובמבר','דצמבר'],
		monthNamesShort: ['1','2','3','4','5','6',
		'7','8','9','10','11','12'],
		dayNames: ['ראשון','שני','שלישי','רביעי','חמישי','שישי','שבת'],
		dayNamesShort: ['א\'','ב\'','ג\'','ד\'','ה\'','ו\'','שבת'],
		dayNamesMin: ['א\'','ב\'','ג\'','ד\'','ה\'','ו\'','שבת'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 0,
		isRTL: true
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['he'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['he'];
	}
})(jQuery);
