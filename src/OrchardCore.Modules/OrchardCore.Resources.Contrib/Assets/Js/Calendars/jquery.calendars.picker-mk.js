/* http://keith-wood.name/calendars.html
   Македонски MK localisation for calendars datepicker for jQuery.
   Hajan Selmani (hajan [at] live [dot] com). */
(function($) {
	$.calendarsPicker.regionalOptions['mk'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: 'Претх.', prevStatus: 'Прикажи го претходниот месец',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: 'Прикажи ја претходната година',
		nextText: 'Следен', nextStatus: 'Прикажи го следниот месец',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: 'Прикажи ја следната година',
		currentText: 'Тековен', currentStatus: 'Прикажи го тековниот месец',
		todayText: 'Денес', todayStatus: 'Прикажи го денешниот месец',
		clearText: 'Бриши', clearStatus: 'Избриши го тековниот датум',
		closeText: 'Затвори', closeStatus: 'Затвори без промени',
		yearStatus: 'Избери друга година', monthStatus: 'Избери друг месец',
		weekText: 'Нед', weekStatus: 'Недела во годината',
		dayStatus: 'Избери DD, M d', defaultStatus: 'Избери датум',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['mk']);
})(jQuery);
