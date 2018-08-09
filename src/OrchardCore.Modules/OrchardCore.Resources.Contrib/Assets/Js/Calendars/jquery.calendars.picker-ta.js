/* http://keith-wood.name/calendars.html
   Tamil (UTF-8) localisation for calendars datepicker for jQuery.
   Written by S A Sureshkumar (saskumar@live.com). */
(function($) {
	$.calendarsPicker.regionalOptions['ta'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: 'முன்னையது',  prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'அடுத்தது', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'இன்று', currentStatus: '',
		todayText: 'இன்று', todayStatus: '',
		clearText: 'அழி', clearStatus: '',
		closeText: 'மூடு', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Wk', weekStatus: '',
		dayStatus: 'D, M d', defaultStatus: '',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['ta']);
})(jQuery);
