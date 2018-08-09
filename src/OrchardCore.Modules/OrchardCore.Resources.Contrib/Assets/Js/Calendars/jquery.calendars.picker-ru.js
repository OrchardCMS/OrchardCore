/* http://keith-wood.name/calendars.html
   Russian localisation for calendars datepicker for jQuery.
   Written by Andrew Stromnov (stromnov@gmail.com). */
(function($) {
	$.calendarsPicker.regionalOptions['ru'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '&#x3c;Пред',  prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'След&#x3e;', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'Сегодня', currentStatus: '',
		todayText: 'Сегодня', todayStatus: '',
		clearText: 'Очистить', clearStatus: '',
		closeText: 'Закрыть', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Не', weekStatus: '',
		dayStatus: 'DD, M d', defaultStatus: '',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['ru']);
})(jQuery);
