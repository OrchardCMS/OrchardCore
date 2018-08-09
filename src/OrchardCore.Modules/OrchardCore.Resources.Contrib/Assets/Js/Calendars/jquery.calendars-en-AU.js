/* http://keith-wood.name/calendars.html
   English/Australia localisation for Gregorian/Julian calendars for jQuery.
   Based on en-GB. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['en-AU'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['January','February','March','April','May','June',
		'July','August','September','October','November','December'],
		monthNamesShort: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
		'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
		dayNames: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
		dayNamesShort: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
		dayNamesMin: ['Su','Mo','Tu','We','Th','Fr','Sa'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['en-AU'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['en-AU'];
	}
})(jQuery);
