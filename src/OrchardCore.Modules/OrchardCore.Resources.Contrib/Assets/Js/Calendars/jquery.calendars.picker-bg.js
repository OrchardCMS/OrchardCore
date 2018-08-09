/* http://keith-wood.name/calendars.html
   Bulgarian localisation for calendars datepicker for jQuery.
   Written by Stoyan Kyosev (http://svest.org). */
(function($) {
	$.calendarsPicker.regionalOptions['bg'] = {
		renderer: $.calendarsPicker.defaultRenderer,
        prevText: '&#x3c;назад', prevStatus: 'покажи последния месец',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
        nextText: 'напред&#x3e;', nextStatus: 'покажи следващия месец',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
        currentText: 'днес', currentStatus: '',
		todayText: 'днес', todayStatus: '',
		clearText: 'изчисти', clearStatus: 'изчисти актуалната дата',
        closeText: 'затвори', closeStatus: 'затвори без промени',
		yearStatus: 'покажи друга година', monthStatus: 'покажи друг месец',
		weekText: 'Wk', weekStatus: 'седмица от месеца',
		dayStatus: 'Избери D, M d', defaultStatus: 'Избери дата',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['bg']);
})(jQuery);
