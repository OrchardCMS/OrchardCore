/* http://keith-wood.name/calendars.html
   Danish localisation for calendars datepicker for jQuery.
   Written by Jan Christensen ( deletestuff@gmail.com). */
(function($) {
	$.calendarsPicker.regionalOptions['da'] = {
		renderer: $.calendarsPicker.defaultRenderer,
        prevText: '&#x3c;Forrige', prevStatus: 'Vis forrige måned',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'Næste&#x3e;', nextStatus: 'Vis næste måned',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'Idag', currentStatus: 'Vis aktuel måned',
		todayText: 'Idag', todayStatus: 'Vis aktuel måned',
		clearText: 'Nulstil', clearStatus: 'Nulstil den aktuelle dato',
		closeText: 'Luk', closeStatus: 'Luk uden ændringer',
		yearStatus: 'Vis et andet år', monthStatus: 'Vis en anden måned',
		weekText: 'Uge', weekStatus: 'Årets uge',
		dayStatus: 'Vælg D, M d', defaultStatus: 'Vælg en dato',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['da']);
})(jQuery);
