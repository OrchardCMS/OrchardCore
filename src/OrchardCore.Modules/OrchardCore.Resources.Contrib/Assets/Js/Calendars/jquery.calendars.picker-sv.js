/* http://keith-wood.name/calendars.html
   Swedish localisation for calendars datepicker for jQuery.
   Written by Anders Ekdahl ( anders@nomadiz.se). */
(function($) {
	$.calendarsPicker.regionalOptions['sv'] = {
		renderer: $.calendarsPicker.defaultRenderer,
        prevText: '&laquo;Förra',  prevStatus: '',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'Nästa&raquo;', nextStatus: '',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'Idag', currentStatus: '',
		todayText: 'Idag', todayStatus: '',
		clearText: 'Rensa', clearStatus: '',
		closeText: 'Stäng', closeStatus: '',
		yearStatus: '', monthStatus: '',
		weekText: 'Ve', weekStatus: '',
		dayStatus: 'DD, M d', defaultStatus: '',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['sv']);
})(jQuery);
