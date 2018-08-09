/* http://keith-wood.name/calendars.html
   Esperanto localisation for calendars datepicker for jQuery.
   Written by Olivier M. (olivierweb@ifrance.com). */
(function($) {
	$.calendarsPicker.regionalOptions['eo'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '&lt;Anta', prevStatus: 'Vidi la antaŭan monaton',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'Sekv&gt;', nextStatus: 'Vidi la sekvan monaton',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'Nuna', currentStatus: 'Vidi la nunan monaton',
		todayText: 'Nuna', todayStatus: 'Vidi la nunan monaton',
		clearText: 'Vakigi', clearStatus: '',
		closeText: 'Fermi', closeStatus: 'Fermi sen modifi',
		yearStatus: 'Vidi alian jaron', monthStatus: 'Vidi alian monaton',
		weekText: 'Sb', weekStatus: '',
		dayStatus: 'Elekti DD, MM d', defaultStatus: 'Elekti la daton',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['eo']);
})(jQuery);
