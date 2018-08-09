/* http://keith-wood.name/calendars.html
   Ukrainian localisation for calendars datepicker for jQuery.
   Written by Maxim Drogobitskiy (maxdao@gmail.com). */
(function($) {
	$.calendarsPicker.regionalOptions['uk'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '&#x3c;',  prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: '&#x3e;', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'Сьогодні', currentStatus: '',
		todayText: 'Сьогодні', todayStatus: '',
		clearText: 'Очистити', clearStatus: '',
		closeText: 'Закрити', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Не', weekStatus: '',
		dayStatus: 'DD, M d', defaultStatus: '',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['uk']);
})(jQuery);
