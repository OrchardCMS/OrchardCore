/* http://keith-wood.name/calendars.html
   Maltese localisation for calendars datepicker for jQuery.
   Written by Chritian Sciberras (uuf6429@gmail.com). */
(function($) {
	$.calendarsPicker.regionalOptions['mt'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: 'Ta Qabel', prevStatus: 'Ix-xahar ta qabel',
 		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: 'Is-sena ta qabel',
 		nextText: 'Li Jmiss', nextStatus: 'Ix-xahar li jmiss',
 		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: 'Is-sena li jmiss',
 		currentText: 'Illum', currentStatus: 'Ix-xahar ta llum',
 		todayText: 'Illum', todayStatus: 'Uri ix-xahar ta llum',
 		clearText: 'Ħassar', clearStatus: 'Ħassar id-data',
 		closeText: 'Lest', closeStatus: 'Għalaq mingħajr tibdiliet',
 		yearStatus: 'Uri sena differenti', monthStatus: 'Uri xahar differenti',
		weekText: 'Ġm', weekStatus: 'Il-Ġimgħa fis-sena',
		dayStatus: 'Għazel DD, M d', defaultStatus: 'Għazel data',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['mt']);
})(jQuery);
