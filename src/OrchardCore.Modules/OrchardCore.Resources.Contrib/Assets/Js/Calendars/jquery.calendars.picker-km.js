/* http://keith-wood.name/calendars.html
   Khmer initialisation for calendars datepicker for jQuery.
   Written by Sovichet Tep (sovichet.tep@gmail.com). */
(function($) {
	$.calendarsPicker.regionalOptions['km'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: 'ថយ​ក្រោយ', prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'ទៅ​មុខ', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'ថ្ងៃ​នេះ', currentStatus: '',
		todayText: 'ថ្ងៃ​នេះ', todayStatus: '',
		clearText: 'X', clearStatus: '',
		closeText: 'រួច​រាល់', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Wk', weekStatus: '',
		dayStatus: 'DD d MM', defaultStatus: '',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['km']);
})(jQuery);
