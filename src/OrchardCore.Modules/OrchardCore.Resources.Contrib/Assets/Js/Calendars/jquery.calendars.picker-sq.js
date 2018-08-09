/* http://keith-wood.name/calendars.html
   Albanian localisation for calendars datepicker for jQuery.
   Written by Flakron Bytyqi (flakron@gmail.com). */
(function($) {
	$.calendarsPicker.regionalOptions['sq'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '&#x3c;mbrapa', prevStatus: 'trego muajin e fundit',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'Përpara&#x3e;', nextStatus: 'trego muajin tjetër',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'sot', currentStatus: '',
		todayText: 'sot', todayStatus: '',
		clearText: 'fshije', clearStatus: 'fshije datën aktuale',
		closeText: 'mbylle', closeStatus: 'mbylle pa ndryshime',
		yearStatus: 'trego tjetër vit', monthStatus: 'trego muajin tjetër',
		weekText: 'Ja', weekStatus: 'Java e muajit',
		dayStatus: '\'Zgjedh\' D, M d', defaultStatus: 'Zgjedhe një datë',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['sq']);
})(jQuery);
