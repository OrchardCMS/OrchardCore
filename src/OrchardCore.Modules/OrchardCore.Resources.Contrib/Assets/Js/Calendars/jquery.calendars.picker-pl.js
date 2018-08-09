/* http://keith-wood.name/calendars.html
   Polish localisation for calendars datepicker for jQuery.
   Written by Jacek Wysocki (jacek.wysocki@gmail.com). */
(function($) {
	$.calendarsPicker.regionalOptions['pl'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '&#x3c;Poprzedni', prevStatus: 'Pokaż poprzedni miesiąc',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'Następny&#x3e;', nextStatus: 'Pokaż następny miesiąc',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'Dziś', currentStatus: 'Pokaż aktualny miesiąc',
		todayText: 'Dziś', todayStatus: 'Pokaż aktualny miesiąc',
		clearText: 'Wyczyść', clearStatus: 'Wyczyść obecną datę',
		closeText: 'Zamknij', closeStatus: 'Zamknij bez zapisywania',
		yearStatus: 'Pokaż inny rok', monthStatus: 'Pokaż inny miesiąc',
		weekText: 'Tydz', weekStatus: 'Tydzień roku',
		dayStatus: '\'Wybierz\' DD, M d', defaultStatus: 'Wybierz datę',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['pl']);
})(jQuery);
