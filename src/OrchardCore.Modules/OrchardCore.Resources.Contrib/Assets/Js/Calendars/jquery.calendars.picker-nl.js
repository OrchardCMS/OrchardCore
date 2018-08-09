/* http://keith-wood.name/calendars.html
   Dutch localisation for calendars datepicker for jQuery.
   Written by Mathias Bynens <http://mathiasbynens.be/>. */
(function($) {
	$.calendarsPicker.regionalOptions['nl'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '←', prevStatus: 'Bekijk de vorige maand',
		prevJumpText: '«', nextJumpStatus: 'Bekijk het vorige jaar',
		nextText: '→', nextStatus: 'Bekijk de volgende maand',
		nextJumpText: '»', nextJumpStatus: 'Bekijk het volgende jaar',
		currentText: 'Vandaag', currentStatus: 'Bekijk de huidige maand',
		todayText: 'Vandaag', todayStatus: 'Bekijk de huidige maand',
		clearText: 'Wissen', clearStatus: 'Wis de huidige datum',
		closeText: 'Sluiten', closeStatus: 'Sluit zonder verandering',
		yearStatus: 'Bekijk een ander jaar', monthStatus: 'Bekijk een andere maand',
		weekText: 'Wk', weekStatus: 'Week van het jaar',
		dayStatus: 'dd-mm-yyyy', defaultStatus: 'Kies een datum',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['nl']);
})(jQuery);
