/* http://keith-wood.name/calendars.html
   Greek localisation for calendars datepicker for jQuery.
   Written by Alex Cicovic (http://www.alexcicovic.com). */
(function($) {
	$.calendarsPicker.regionalOptions['el'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: 'Προηγούμενος', prevStatus: 'Επισκόπηση προηγούμενου μήνα',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'Επόμενος', nextStatus: 'Επισκόπηση επόμενου μήνα',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'Τρέχων Μήνας', currentStatus: 'Επισκόπηση τρέχοντος μήνα',
		todayText: 'Τρέχων Μήνας', todayStatus: 'Επισκόπηση τρέχοντος μήνα',
		clearText: 'Σβήσιμο', clearStatus: 'Σβήσιμο της επιλεγμένης ημερομηνίας',
		closeText: 'Κλείσιμο', closeStatus: 'Κλείσιμο χωρίς αλλαγή',
		yearStatus: 'Επισκόπηση άλλου έτους', monthStatus: 'Επισκόπηση άλλου μήνα',
		weekText: 'Εβδ', weekStatus: '',
		dayStatus: 'Επιλογή DD d MM', defaultStatus: 'Επιλέξτε μια ημερομηνία',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['el']);
})(jQuery);
