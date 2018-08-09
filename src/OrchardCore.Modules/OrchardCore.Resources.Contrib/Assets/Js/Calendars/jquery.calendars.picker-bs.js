/* http://keith-wood.name/calendars.html
   Bosnian localisation for calendars datepicker for jQuery.
   Kenan Konjo. */
(function($) {
	$.calendarsPicker.regionalOptions['bs'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '&#x3c;', prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: '&#x3e;', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'Danas', currentStatus: '',
		todayText: 'Danas', todayStatus: '',
		clearText: 'X', clearStatus: '',
		closeText: 'Zatvori', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Wk', weekStatus: '',
		dayStatus: '', defaultStatus: '',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['bs']);
})(jQuery);
