/* http://keith-wood.name/calendars.html
   Catalan localisation for calendars datepicker for jQuery.
   Writers: (joan.leon@gmail.com). */
(function($) {
	$.calendarsPicker.regionalOptions['ca'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '&#x3c;Ant', prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'Seg&#x3e;', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'Avui', currentStatus: '',
		todayText: 'Avui', todayStatus: '',
		clearText: 'Netejar', clearStatus: '',
		closeText: 'Tancar', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Wk', weekStatus: '',
		dayStatus: 'DD, M d', defaultStatus: '',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['ca']);
})(jQuery);
