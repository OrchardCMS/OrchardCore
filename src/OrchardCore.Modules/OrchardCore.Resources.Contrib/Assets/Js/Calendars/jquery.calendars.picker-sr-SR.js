/* http://keith-wood.name/calendars.html
   Serbian localisation for calendars datepicker for jQuery.
   Written by Dejan Dimić. */
(function($) {
	$.calendarsPicker.regionalOptions['sr-SR'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '&#x3c;', prevStatus: 'Prikaži predhodni mesec',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: 'Prikaži predhodnu godinu',
		nextText: '&#x3e;', nextStatus: 'Prikaži sledeći mesec',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: 'Prikaži sledeću godinu',
		currentText: 'Danas', currentStatus: 'Tekući mesec',
		todayText: 'Danas', todayStatus: 'Tekući mesec',
		clearText: 'Obriši', clearStatus: 'Obriši trenutni datum',
		closeText: 'Zatvori', closeStatus: 'Zatvori kalendar',
		yearStatus: 'Prikaži godine', monthStatus: 'Prikaži mesece',
		weekText: 'Sed', weekStatus: 'Sedmica',
		dayStatus: '\'Datum\' DD, M d', defaultStatus: 'Odaberi datum',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['sr-SR']);
})(jQuery);
