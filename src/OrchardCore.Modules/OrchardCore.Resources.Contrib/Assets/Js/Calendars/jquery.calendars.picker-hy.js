/* http://keith-wood.name/calendars.html
   Armenian localisation for calendars datepicker for jQuery.
   Written by Levon Zakaryan (levon.zakaryan@gmail.com) */
(function($) {
	$.calendarsPicker.regionalOptions['hy'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '&#x3c;Նախ.',  prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'Հաջ.&#x3e;', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'Այսօր', currentStatus: '',
		todayText: 'Այսօր', todayStatus: '',
		clearText: 'Մաքրել', clearStatus: '',
		closeText: 'Փակել', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'ՇԲՏ', weekStatus: '',
		dayStatus: 'DD, M d', defaultStatus: '',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['hy']);
})(jQuery);
