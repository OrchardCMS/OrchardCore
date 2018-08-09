/* http://keith-wood.name/calendars.html
   Romansh localisation for calendars datepicker for jQuery.
   Yvonne Gienal (yvonne.gienal@educa.ch). */
(function($) {
	$.calendarsPicker.regionalOptions['rm'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '&#x3c;Suandant', prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'Precedent&#x3e;', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'Actual', currentStatus: '',
		todayText: 'Actual', todayStatus: '',
		clearText: 'X', clearStatus: '',
		closeText: 'Serrar', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'emna', weekStatus: '',
		dayStatus: 'DD d MM', defaultStatus: '',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['rm']);
})(jQuery);
