/* http://keith-wood.name/calendars.html
   Swiss-German localisation for Gregorian/Julian calendars for jQuery.
   Written by Douglas Jose & Juerg Meier. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['de-CH'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Januar','Februar','März','April','Mai','Juni',
		'Juli','August','September','Oktober','November','Dezember'],
		monthNamesShort: ['Jan','Feb','Mär','Apr','Mai','Jun',
		'Jul','Aug','Sep','Okt','Nov','Dez'],
		dayNames: ['Sonntag','Montag','Dienstag','Mittwoch','Donnerstag','Freitag','Samstag'],
		dayNamesShort: ['So','Mo','Di','Mi','Do','Fr','Sa'],
		dayNamesMin: ['So','Mo','Di','Mi','Do','Fr','Sa'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['de-CH'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['de-CH'];
	}
})(jQuery);
