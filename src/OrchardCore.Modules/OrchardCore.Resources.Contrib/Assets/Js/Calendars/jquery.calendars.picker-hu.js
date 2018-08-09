/* http://keith-wood.name/calendars.html
   Hungarian localisation for calendars datepicker for jQuery.
   Written by Istvan Karaszi (jquerycalendar@spam.raszi.hu). */
(function($) {
	$.calendarsPicker.regionalOptions['hu'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '&laquo;&nbsp;vissza', prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'előre&nbsp;&raquo;', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'ma', currentStatus: '',
		todayText: 'ma', todayStatus: '',
		clearText: 'törlés', clearStatus: '',
		closeText: 'bezárás', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Hé', weekStatus: '',
		dayStatus: 'DD, M d', defaultStatus: '',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['hu']);
})(jQuery);
