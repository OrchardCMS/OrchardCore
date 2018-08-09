/* http://keith-wood.name/calendars.html
   Montenegrin localisation for calendars datepicker for jQuery.
   Written by Miloš Milošević - fleka d.o.o. */
(function($) {
	$.calendarsPicker.regionalOptions['me'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '&#x3c;', prevStatus: 'Прикажи претходни мјесец',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: 'Прикажи претходну годину',
		nextText: '&#x3e;', nextStatus: 'Прикажи сљедећи мјесец',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: 'Прикажи сљедећу годину',
		currentText: 'Данас', currentStatus: 'Текући мјесец',
		todayText: 'Данас', todayStatus: 'Текући мјесец',
		clearText: 'Обриши', clearStatus: 'Обриши тренутни датум',
		closeText: 'Затвори', closeStatus: 'Затвори календар',
		yearStatus: 'Прикажи године', monthStatus: 'Прикажи мјесеце',
		weekText: 'Сед', weekStatus: 'Седмица',
		dayStatus: '\'Датум\' DD d MM', defaultStatus: 'Одабери датум',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['me']);
})(jQuery);
