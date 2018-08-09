/* http://keith-wood.name/calendars.html
   English/Australia localisation for calendars datepicker for jQuery.
   Based on en-GB. */
(function($) {
	$.calendarsPicker.regionalOptions['en-AU'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: 'Prev', prevStatus: 'Show the previous month',
		prevJumpText: '&lt;&lt;', prevJumpStatus: 'Show the previous year',
		nextText: 'Next', nextStatus: 'Show the next month',
		nextJumpText: '&gt;&gt;', nextJumpStatus: 'Show the next year',
		currentText: 'Current', currentStatus: 'Show the current month',
		todayText: 'Today', todayStatus: 'Show today\'s month',
		clearText: 'Clear', clearStatus: 'Clear all the dates',
		closeText: 'Done', closeStatus: 'Close the datepicker',
		yearStatus: 'Change the year', monthStatus: 'Change the month',
		weekText: 'Wk', weekStatus: 'Week of the year',
		dayStatus: 'Select DD, M d, yyyy', defaultStatus: 'Select a date',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['en-AU']);
})(jQuery);
