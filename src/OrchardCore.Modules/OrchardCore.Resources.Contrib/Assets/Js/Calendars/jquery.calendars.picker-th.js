/* http://keith-wood.name/calendars.html
   Thai localisation for calendars datepicker for jQuery.
   Written by pipo (pipo@sixhead.com). */
(function($) {
	$.calendarsPicker.regionalOptions['th'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '&laquo;&nbsp;ย้อน', prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'ถัดไป&nbsp;&raquo;', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'วันนี้', currentStatus: '',
		todayText: 'วันนี้', todayStatus: '',
		clearText: 'ลบ', clearStatus: '',
		closeText: 'ปิด', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Wk', weekStatus: '',
		dayStatus: 'DD, M d', defaultStatus: '',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['th']);
})(jQuery);
