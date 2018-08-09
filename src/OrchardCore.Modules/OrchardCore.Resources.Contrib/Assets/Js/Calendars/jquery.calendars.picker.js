/* http://keith-wood.name/calendars.html
   Calendars date picker for jQuery v2.0.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) August 2009.
   Available under the MIT (http://keith-wood.name/licence.html) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

	var pluginName = 'calendarsPicker';


	/** Create the calendars datepicker plugin.
		<p>Sets an input field to popup a calendar for date entry,
			or a <code>div</code> or <code>span</code> to show an inline calendar.</p>
		<p>Expects HTML like:</p>
		<pre>&lt;input type="text"> or &lt;div>&lt;/div></pre>
		<p>Provide inline configuration like:</p>
		<pre>&lt;input type="text" data-calendarsPicker="name: 'value'"/></pre>
	 	@class CalendarsPicker
		@augments JQPlugin
		@example $(selector).calendarsPicker()
 $(selector).calendarsPicker({minDate: 0, maxDate: '+1m +1w'}) */
	$.JQPlugin.createPlugin({
	
		/** The name of the plugin.
			@memberof CalendarsPicker */
		name: pluginName,
		
		/** Default template for generating a datepicker.
			Insert anywhere:
			<ul>
			<li>'{l10n:name}' to insert localised value for name,</li>
			<li>'{link:name}' to insert a link trigger for command name,</li>
			<li>'{button:name}' to insert a button trigger for command name,</li>
			<li>'{popup:start}...{popup:end}' to mark a section for inclusion in a popup datepicker only,</li>
			<li>'{inline:start}...{inline:end}' to mark a section for inclusion in an inline datepicker only.</li>
			</ul>
			@memberof CalendarsPicker
			@property picker {string} Overall structure: '{months}' to insert calendar months.
			@property monthRow {string} One row of months: '{months}' to insert calendar months.
			@property month {string} A single month: '{monthHeader<em>:dateFormat</em>}' to insert the month header -
						<em>dateFormat</em> is optional and defaults to 'MM yyyy',
						'{weekHeader}' to insert a week header, '{weeks}' to insert the month's weeks.
			@property weekHeader {string} A week header: '{days}' to insert individual day names.
			@property dayHeader {string} Individual day header: '{day}' to insert day name.
			@property week {string} One week of the month: '{days}' to insert the week's days,
						'{weekOfYear}' to insert week of year.
			@property day {string} An individual day: '{day}' to insert day value.
			@property monthSelector {string} jQuery selector, relative to picker, for a single month.
			@property daySelector {string} jQuery selector, relative to picker, for individual days.
			@property rtlClass {string} Class for right-to-left (RTL) languages.
			@property multiClass {string} Class for multi-month datepickers.
			@property defaultClass {string} Class for selectable dates.
			@property selectedClass {string} Class for currently selected dates.
			@property highlightedClass {string} Class for highlighted dates.
			@property todayClass {string} Class for today.
			@property otherMonthClass {string} Class for days from other months.
			@property weekendClass {string} Class for days on weekends.
			@property commandClass {string} Class prefix for commands.
			@property commandButtonClass {string} Extra class(es) for commands that are buttons.
			@property commandLinkClass {string} Extra class(es) for commands that are links.
			@property disabledClass {string} Class for disabled commands. */
		defaultRenderer: {
			picker: '<div class="calendars">' +
			'<div class="calendars-nav">{link:prev}{link:today}{link:next}</div>{months}' +
			'{popup:start}<div class="calendars-ctrl">{link:clear}{link:close}</div>{popup:end}' +
			'<div class="calendars-clear-fix"></div></div>',
			monthRow: '<div class="calendars-month-row">{months}</div>',
			month: '<div class="calendars-month"><div class="calendars-month-header">{monthHeader}</div>' +
			'<table><thead>{weekHeader}</thead><tbody>{weeks}</tbody></table></div>',
			weekHeader: '<tr>{days}</tr>',
			dayHeader: '<th>{day}</th>',
			week: '<tr>{days}</tr>',
			day: '<td>{day}</td>',
			monthSelector: '.calendars-month',
			daySelector: 'td',
			rtlClass: 'calendars-rtl',
			multiClass: 'calendars-multi',
			defaultClass: '',
			selectedClass: 'calendars-selected',
			highlightedClass: 'calendars-highlight',
			todayClass: 'calendars-today',
			otherMonthClass: 'calendars-other-month',
			weekendClass: 'calendars-weekend',
			commandClass: 'calendars-cmd',
			commandButtonClass: '',
			commandLinkClass: '',
			disabledClass: 'calendars-disabled'
		},
	
		/** Command actions that may be added to a layout by name.
			<ul>
			<li>prev - Show the previous month (based on <code>monthsToStep</code> option) - <em>PageUp</em></li>
			<li>prevJump - Show the previous year (based on <code>monthsToJump</code> option) - <em>Ctrl+PageUp</em></li>
			<li>next - Show the next month (based on <code>monthsToStep</code> option) - <em>PageDown</em></li>
			<li>nextJump - Show the next year (based on <code>monthsToJump</code> option) - <em>Ctrl+PageDown</em></li>
			<li>current - Show the currently selected month or today's if none selected - <em>Ctrl+Home</em></li>
			<li>today - Show today's month - <em>Ctrl+Home</em></li>
			<li>clear - Erase the date and close the datepicker popup - <em>Ctrl+End</em></li>
			<li>close - Close the datepicker popup - <em>Esc</em></li>
			<li>prevWeek - Move the cursor to the previous week - <em>Ctrl+Up</em></li>
			<li>prevDay - Move the cursor to the previous day - <em>Ctrl+Left</em></li>
			<li>nextDay - Move the cursor to the next day - <em>Ctrl+Right</em></li>
			<li>nextWeek - Move the cursor to the next week - <em>Ctrl+Down</em></li>
			</ul>
			The command name is the key name and is used to add the command to a layout
			with '{button:name}' or '{link:name}'. Each has the following attributes.
			@memberof CalendarsPicker
			@property text {string} The field in the regional settings for the displayed text.
			@property status {string} The field in the regional settings for the status text.
			@property keystroke {object} The keystroke to trigger the action, with attributes:
				<code>keyCode</code> {number} the code for the keystroke,
				<code>ctrlKey</code> {boolean} <code>true</code> if <em>Ctrl</em> is required,
				<code>altKey</code> {boolean} <code>true</code> if <em>Alt</em> is required,
				<code>shiftKey</code> {boolean} <code>true</code> if <em>Shift</em> is required.
			@property enabled {CalendarsPickerCommandEnabled} The function that indicates the command is enabled.
			@property date {CalendarsPickerCommandDate} The function to get the date associated with this action.
			@property action {CalendarsPickerCommandAction} The function that implements the action. */
		commands: {
			prev: {text: 'prevText', status: 'prevStatus', // Previous month
				keystroke: {keyCode: 33}, // Page up
				enabled: function(inst) {
					var minDate = inst.curMinDate();
					return (!minDate || inst.drawDate.newDate().
						add(1 - inst.options.monthsToStep - inst.options.monthsOffset, 'm').
						day(inst.options.calendar.minDay).add(-1, 'd').compareTo(minDate) !== -1); },
				date: function(inst) {
					return inst.drawDate.newDate().
						add(-inst.options.monthsToStep - inst.options.monthsOffset, 'm').
						day(inst.options.calendar.minDay); },
				action: function(inst) {
					plugin.changeMonth(this, -inst.options.monthsToStep); }
			},
			prevJump: {text: 'prevJumpText', status: 'prevJumpStatus', // Previous year
				keystroke: {keyCode: 33, ctrlKey: true}, // Ctrl + Page up
				enabled: function(inst) {
					var minDate = inst.curMinDate();
					return (!minDate || inst.drawDate.newDate().
						add(1 - inst.options.monthsToJump - inst.options.monthsOffset, 'm').
						day(inst.options.calendar.minDay).add(-1, 'd').compareTo(minDate) !== -1); },
				date: function(inst) {
					return inst.drawDate.newDate().
						add(-inst.options.monthsToJump - inst.options.monthsOffset, 'm').
						day(inst.options.calendar.minDay); },
				action: function(inst) {
					plugin.changeMonth(this, -inst.options.monthsToJump); }
			},
			next: {text: 'nextText', status: 'nextStatus', // Next month
				keystroke: {keyCode: 34}, // Page down
				enabled: function(inst) {
					var maxDate = inst.get('maxDate');
					return (!maxDate || inst.drawDate.newDate().
						add(inst.options.monthsToStep - inst.options.monthsOffset, 'm').
						day(inst.options.calendar.minDay).compareTo(maxDate) !== +1); },
				date: function(inst) {
					return inst.drawDate.newDate().
						add(inst.options.monthsToStep - inst.options.monthsOffset, 'm').
						day(inst.options.calendar.minDay); },
				action: function(inst) {
					plugin.changeMonth(this, inst.options.monthsToStep); }
			},
			nextJump: {text: 'nextJumpText', status: 'nextJumpStatus', // Next year
				keystroke: {keyCode: 34, ctrlKey: true}, // Ctrl + Page down
				enabled: function(inst) {
					var maxDate = inst.get('maxDate');
					return (!maxDate || inst.drawDate.newDate().
						add(inst.options.monthsToJump - inst.options.monthsOffset, 'm').
						day(inst.options.calendar.minDay).compareTo(maxDate) !== +1);	},
				date: function(inst) {
					return inst.drawDate.newDate().
						add(inst.options.monthsToJump - inst.options.monthsOffset, 'm').
						day(inst.options.calendar.minDay); },
				action: function(inst) {
					plugin.changeMonth(this, inst.options.monthsToJump); }
			},
			current: {text: 'currentText', status: 'currentStatus', // Current month
				keystroke: {keyCode: 36, ctrlKey: true}, // Ctrl + Home
				enabled: function(inst) {
					var minDate = inst.curMinDate();
					var maxDate = inst.get('maxDate');
					var curDate = inst.selectedDates[0] || inst.options.calendar.today();
					return (!minDate || curDate.compareTo(minDate) !== -1) &&
						(!maxDate || curDate.compareTo(maxDate) !== +1); },
				date: function(inst) {
					return inst.selectedDates[0] || inst.options.calendar.today(); },
				action: function(inst) {
					var curDate = inst.selectedDates[0] || inst.options.calendar.today();
					plugin.showMonth(this, curDate.year(), curDate.month()); }
			},
			today: {text: 'todayText', status: 'todayStatus', // Today's month
				keystroke: {keyCode: 36, ctrlKey: true}, // Ctrl + Home
				enabled: function(inst) {
					var minDate = inst.curMinDate();
					var maxDate = inst.get('maxDate');
					return (!minDate || inst.options.calendar.today().compareTo(minDate) !== -1) &&
						(!maxDate || inst.options.calendar.today().compareTo(maxDate) !== +1); },
				date: function(inst) { return inst.options.calendar.today(); },
				action: function(inst) { plugin.showMonth(this); }
			},
			clear: {text: 'clearText', status: 'clearStatus', // Clear the datepicker
				keystroke: {keyCode: 35, ctrlKey: true}, // Ctrl + End
				enabled: function(inst) { return true; },
				date: function(inst) { return null; },
				action: function(inst) { plugin.clear(this); }
			},
			close: {text: 'closeText', status: 'closeStatus', // Close the datepicker
				keystroke: {keyCode: 27}, // Escape
				enabled: function(inst) { return true; },
				date: function(inst) { return null; },
				action: function(inst) { plugin.hide(this); }
			},
			prevWeek: {text: 'prevWeekText', status: 'prevWeekStatus', // Previous week
				keystroke: {keyCode: 38, ctrlKey: true}, // Ctrl + Up
				enabled: function(inst) {
					var minDate = inst.curMinDate();
					return (!minDate || inst.drawDate.newDate().
						add(-inst.options.calendar.daysInWeek(), 'd').compareTo(minDate) !== -1); },
				date: function(inst) { return inst.drawDate.newDate().
					add(-inst.options.calendar.daysInWeek(), 'd'); },
				action: function(inst) { plugin.changeDay(this, -inst.options.calendar.daysInWeek()); }
			},
			prevDay: {text: 'prevDayText', status: 'prevDayStatus', // Previous day
				keystroke: {keyCode: 37, ctrlKey: true}, // Ctrl + Left
				enabled: function(inst) {
					var minDate = inst.curMinDate();
					return (!minDate || inst.drawDate.newDate().add(-1, 'd').
						compareTo(minDate) !== -1); },
				date: function(inst) { return inst.drawDate.newDate().add(-1, 'd'); },
				action: function(inst) { plugin.changeDay(this, -1); }
			},
			nextDay: {text: 'nextDayText', status: 'nextDayStatus', // Next day
				keystroke: {keyCode: 39, ctrlKey: true}, // Ctrl + Right
				enabled: function(inst) {
					var maxDate = inst.get('maxDate');
					return (!maxDate || inst.drawDate.newDate().add(1, 'd').
						compareTo(maxDate) !== +1); },
				date: function(inst) { return inst.drawDate.newDate().add(1, 'd'); },
				action: function(inst) { plugin.changeDay(this, 1); }
			},
			nextWeek: {text: 'nextWeekText', status: 'nextWeekStatus', // Next week
				keystroke: {keyCode: 40, ctrlKey: true}, // Ctrl + Down
				enabled: function(inst) {
					var maxDate = inst.get('maxDate');
					return (!maxDate || inst.drawDate.newDate().
						add(inst.options.calendar.daysInWeek(), 'd').compareTo(maxDate) !== +1); },
				date: function(inst) { return inst.drawDate.newDate().
					add(inst.options.calendar.daysInWeek(), 'd'); },
				action: function(inst) { plugin.changeDay(this, inst.options.calendar.daysInWeek()); }
			}
		},

		/** Determine whether a command is enabled.
			@callback CalendarsPickerCommandEnabled
			@param inst {object} The current instance settings.
			@return {boolean} <code>true</code> if this command is enabled, <code>false</code> if not.
			@example enabled: function(inst) {
	return !!inst.curMinDate();
 } */

		/** Calculate the representative date for a command.
			@callback CalendarsPickerCommandDate
			@param inst {object} The current instance settings.
			@return {CDate} A date appropriate for this command.
			@example date: function(inst) {
	return inst.curMinDate();
 } */

		/** Perform the action for a command.
			@callback CalendarsPickerCommandAction
			@param inst {object} The current instance settings.
			@example date: function(inst) {
	$.datepick.setDate(inst.elem, inst.curMinDate());
 } */

		/** Calculate the week of the year for a date.
			@callback CalendarsPickerCalculateWeek
			@param date {CDate} The date to evaluate.
			@return {number} The week of the year.
			@example calculateWeek: function(date) {
	var startYear = $.calendars.newDate(date.year(), 1, 1);
	return Math.floor((date.dayOfYear() - startYear.dayOfYear()) / 7) + 1;
 } */

		/** Provide information about an individual date shown in the calendar.
			@callback CalendarsPickerOnDate
			@param date {CDate} The date to evaluate.
			@return {object} Information about that date, with the properties above.
			@property selectable {boolean} <code>true</code> if this date can be selected.
			@property dateClass {string} Class(es) to be applied to the date.
			@property content {string} The date cell content.
			@property tooltip {string} A popup tooltip for the date.
			@example onDate: function(date) {
	return {selectable: date.day() > 0 && date.day() &lt; 5,
		dateClass: date.day() === 4 ? 'last-day' : ''};
 } */

		/** Update the datepicker display.
			@callback CalendarsPickerOnShow
			@param picker {jQuery} The datepicker <code>div</code> to be shown.
			@param inst {object} The current instance settings.
			@example onShow: function(picker, inst) {
	picker.append('&lt;button type="button">Hi&lt;/button>').
		find('button:last').click(function() {
			alert('Hi!');
		});
 } */

		/** React to navigating through the months/years.
			@callback CalendarsPickerOnChangeMonthYear
			@param year {number} The new year.
			@param month {number} The new month (1 to 12).
			@example onChangeMonthYear: function(year, month) {
	alert('Now in ' + month + '/' + year);
 } */
			
		/** Datepicker on select callback.
			Triggered when a date is selected.
			@callback CalendarsPickerOnSelect
			@param dates {CDate[]} The selected date(s).
			@example onSelect: function(dates) {
 	alert('Selected ' + dates);
 } */
			
		/** Datepicker on close callback.
			Triggered when a popup calendar is closed.
			@callback CalendarsPickerOnClose
			@param dates {CDate[]} The selected date(s).
			@example onClose: function(dates) {
 	alert('Selected ' + dates);
 } */
		
		/** Default settings for the plugin.
			@memberof CalendarsPicker
			@property [calendar=$.calendars.instance()] {Calendar} The calendar for this datepicker.
			@property [pickerClass=''] {string} CSS class to add to this instance of the datepicker.
			@property [showOnFocus=true] {boolean} <code>true</code> for popup on focus, <code>false</code> for not.
			@property [showTrigger=null] {string|Element|jQuery} Element to be cloned for a trigger, <code>null</code> for none.
			@property [showAnim='show'] {string} Name of jQuery animation for popup, '' for no animation.
			@property [showOptions=null] {object} Options for enhanced animations.
			@property [showSpeed='normal'] {string} Duration of display/closure.
			@property [popupContainer=null] {string|Element|jQuery} The element to which a popup calendar is added, <code>null</code> for body.
			@property [alignment='bottom'] {string} Alignment of popup - with nominated corner of input:
						'top' or 'bottom' aligns depending on language direction,
						'topLeft', 'topRight', 'bottomLeft', 'bottomRight'.
			@property [fixedWeeks=false] {boolean} <code>true</code> to always show 6 weeks, <code>false</code> to only show as many as are needed.
			@property [firstDay=null] {number} First day of the week, 0 = Sunday, 1 = Monday, etc., <code>null</code> for <code>calendar</code> default.
			@property [calculateWeek=null] {CalendarsPickerCalculateWeek} Calculate week of the year from a date, <code>null</code> for <code>calendar</code> default.
			@property [monthsToShow=1] {number|number[]} How many months to show, cols or [rows, cols].
			@property [monthsOffset=0] {number} How many months to offset the primary month by;
						may be a function that takes the date and returns the offset.
			@property [monthsToStep=1] {number} How many months to move when prev/next clicked.
			@property [monthsToJump=12] {number} How many months to move when large prev/next clicked.
			@property [useMouseWheel=true] {boolean} <code>true</code> to use mousewheel if available, <code>false</code> to never use it.
			@property [changeMonth=true] {boolean} <code>true</code> to change month/year via drop-down, <code>false</code> for navigation only.
			@property [yearRange='c-10:c+10'] {string} Range of years to show in drop-down: 'any' for direct text entry
						or 'start:end', where start/end are '+-nn' for relative to today
						or 'c+-nn' for relative to the currently selected date
						or 'nnnn' for an absolute year.
			@property [showOtherMonths=false] {boolean} <code>true</code> to show dates from other months, <code>false</code> to not show them.
			@property [selectOtherMonths=false] {boolean} <code>true</code> to allow selection of dates from other months too.
			@property [defaultDate=null] {string|number|CDate} Date to show if no other selected.
			@property [selectDefaultDate=false] {boolean} <code>true</code> to pre-select the default date if no other is chosen.
			@property [minDate=null] {string|number|CDate} The minimum selectable date.
			@property [maxDate=null] {string|number|CDate} The maximum selectable date.
			@property [dateFormat='mm/dd/yyyy'] {string} Format for dates.
			@property [autoSize=false] {boolean} <code>true</code> to size the input field according to the date format.
			@property [rangeSelect=false] {boolean} Allows for selecting a date range on one date picker.
			@property [rangeSeparator=' - '] {string} Text between two dates in a range.
			@property [multiSelect=0] {number} Maximum number of selectable dates, zero for single select.
			@property [multiSeparator=','] {string} Text between multiple dates.
			@property [onDate=null] {CalendarsPickerOnDate} Callback as a date is added to the datepicker.
			@property [onShow=null] {CalendarsPickerOnShow} Callback just before a datepicker is shown.
			@property [onChangeMonthYear=null] {CalendarsPickerOnChangeMonthYear} Callback when a new month/year is selected.
			@property [onSelect=null] {CalendarsPickerOnSelect} Callback when a date is selected.
			@property [onClose=null] {CalendarsPickerOnClose} Callback when a datepicker is closed.
			@property [altField=null] {string|Element|jQuery} Alternate field to update in synch with the datepicker.
			@property [altFormat=null] {string} Date format for alternate field, defaults to <code>dateFormat</code>.
			@property [constrainInput=true] {boolean} <code>true</code> to constrain typed input to <code>dateFormat</code> allowed characters.
			@property [commandsAsDateFormat=false] {boolean} <code>true</code> to apply
						<code><a href="#formatDate">formatDate</a></code> to the command texts.
			@property [commands=this.commands] {object} Command actions that may be added to a layout by name. */
		defaultOptions: {
			calendar: $.calendars.instance(),
			pickerClass: '',
			showOnFocus: true,
			showTrigger: null,
			showAnim: 'show',
			showOptions: {},
			showSpeed: 'normal',
			popupContainer: null,
			alignment: 'bottom',
			fixedWeeks: false,
			firstDay: null,
			calculateWeek: null,
			monthsToShow: 1,
			monthsOffset: 0,
			monthsToStep: 1,
			monthsToJump: 12,
			useMouseWheel: true,
			changeMonth: true,
			yearRange: 'c-10:c+10',
			showOtherMonths: false,
			selectOtherMonths: false,
			defaultDate: null,
			selectDefaultDate: false,
			minDate: null,
			maxDate: null,
			dateFormat: null,
			autoSize: false,
			rangeSelect: false,
			rangeSeparator: ' - ',
			multiSelect: 0,
			multiSeparator: ',',
			onDate: null,
			onShow: null,
			onChangeMonthYear: null,
			onSelect: null,
			onClose: null,
			altField: null,
			altFormat: null,
			constrainInput: true,
			commandsAsDateFormat: false,
			commands: {} // this.commands
		},

		/** Localisations for the plugin.
			Entries are objects indexed by the language code ('' being the default US/English).
			Each object has the following attributes.
			@memberof CalendarsPicker
			@property [renderer=this.defaultRenderer] {string} The rendering templates.
			@property [prevText='&lt;Prev'] {string} Text for the previous month command.
			@property [prevStatus='Show the previous month'] {string} Status text for the previous month command.
			@property [prevJumpText='&lt;&lt;'] {string} Text for the previous year command.
			@property [prevJumpStatus='Show the previous year'] {string} Status text for the previous year command.
			@property [nextText='Next&gt;'] {string} Text for the next month command.
			@property [nextStatus='Show the next month'] {string} Status text for the next month command.
			@property [nextJumpText='&gt;&gt;'] {string} Text for the next year command.
			@property [nextJumpStatus='Show the next year'] {string} Status text for the next year command.
			@property [currentText='Current'] {string} Text for the current month command.
			@property [currentStatus='Show the current month'] {string} Status text for the current month command.
			@property [todayText='Today'] {string} Text for the today's month command.
			@property [todayStatus='Show today\'s month'] {string} Status text for the today's month command.
			@property [clearText='Clear'] {string} Text for the clear command.
			@property [clearStatus='Clear all the dates'] {string} Status text for the clear command.
			@property [closeText='Close'] {string} Text for the close command.
			@property [closeStatus='Close the datepicker'] {string} Status text for the close command.
			@property [yearStatus='Change the year'] {string} Status text for year selection.
			@property [earlierText='&#160;&#160;▲'] {string} Text for earlier years.
			@property [laterText='&#160;&#160;▼'] {string} Text for later years.
			@property [monthStatus='Change the month'] {string} Status text for month selection.
			@property [weekText='Wk'] {string} Text for week of the year column header.
			@property [weekStatus='Week of the year'] {string} Status text for week of the year column header.
			@property [dayStatus='Select DD,&#160;M&#160;d,&#160;yyyy'] {string} Status text for selectable days.
			@property [defaultStatus='Select a date'] {string} Status text shown by default.
			@property [isRTL=false] {boolean} <code>true</code> if language is right-to-left. */
		regionalOptions: { // Available regional settings, indexed by language/country code
			'': { // Default regional settings - English/US
				renderer: {}, // this.defaultRenderer
				prevText: '&lt;Prev',
				prevStatus: 'Show the previous month',
				prevJumpText: '&lt;&lt;',
				prevJumpStatus: 'Show the previous year',
				nextText: 'Next&gt;',
				nextStatus: 'Show the next month',
				nextJumpText: '&gt;&gt;',
				nextJumpStatus: 'Show the next year',
				currentText: 'Current',
				currentStatus: 'Show the current month',
				todayText: 'Today',
				todayStatus: 'Show today\'s month',
				clearText: 'Clear',
				clearStatus: 'Clear all the dates',
				closeText: 'Close',
				closeStatus: 'Close the datepicker',
				yearStatus: 'Change the year',
				earlierText: '&#160;&#160;▲',
				laterText: '&#160;&#160;▼',
				monthStatus: 'Change the month',
				weekText: 'Wk',
				weekStatus: 'Week of the year',
				dayStatus: 'Select DD, M d, yyyy',
				defaultStatus: 'Select a date',
				isRTL: false
			}
		},
		
		/** Names of getter methods - those that can't be chained.
			@memberof CalendarsPicker */
		_getters: ['getDate', 'isDisabled', 'isSelectable', 'retrieveDate'],

		_disabled: [],
		
		_popupClass: 'calendars-popup', // Marker for popup division
		_triggerClass: 'calendars-trigger', // Marker for trigger element
		_disableClass: 'calendars-disable', // Marker for disabled element
		_monthYearClass: 'calendars-month-year', // Marker for month/year inputs
		_curMonthClass: 'calendars-month-', // Marker for current month/year
		_anyYearClass: 'calendars-any-year', // Marker for year direct input
		_curDoWClass: 'calendars-dow-', // Marker for day of week

		_init: function() {
			this.defaultOptions.commands = this.commands;
			this.regionalOptions[''].renderer = this.defaultRenderer;
			this._super();
		},

		_instSettings: function(elem, options) {
			return {selectedDates: [], drawDate: null, pickingRange: false,
				inline: ($.inArray(elem[0].nodeName.toLowerCase(), ['div', 'span']) > -1),
				get: function(name) { // Get a setting value, computing if necessary
					if ($.inArray(name, ['defaultDate', 'minDate', 'maxDate']) > -1) { // Decode date settings
						return this.options.calendar.determineDate(this.options[name], null,
							this.selectedDates[0], this.get('dateFormat'), this.getConfig());
					}
					if (name === 'dateFormat') {
						return this.options.dateFormat || this.options.calendar.local.dateFormat;
					}
					return this.options[name];
				},
				curMinDate: function() {
					return (this.pickingRange ? this.selectedDates[0] : this.get('minDate'));
				},
				getConfig: function() {
					return {dayNamesShort: this.options.dayNamesShort, dayNames: this.options.dayNames,
						monthNamesShort: this.options.monthNamesShort, monthNames: this.options.monthNames,
						calculateWeek: this.options.calculateWeek, shortYearCutoff: this.options.shortYearCutoff};
				}
			};
		},

		_postAttach: function(elem, inst) {
			if (inst.inline) {
				inst.drawDate = plugin._checkMinMax((inst.selectedDates[0] ||
					inst.get('defaultDate') || inst.options.calendar.today()).newDate(), inst);
				inst.prevDate = inst.drawDate.newDate();
				this._update(elem[0]);
				if ($.fn.mousewheel) {
					elem.mousewheel(this._doMouseWheel);
				}
			}
			else {
				this._attachments(elem, inst);
				elem.on('keydown.' + inst.name, this._keyDown).on('keypress.' + inst.name, this._keyPress).
					on('keyup.' + inst.name, this._keyUp);
				if (elem.attr('disabled')) {
					this.disable(elem[0]);
				}
			}
		},

		_optionsChanged: function(elem, inst, options) {
			if (options.calendar && options.calendar !== inst.options.calendar) {
				var discardDate = function(name) {
					return (typeof inst.options[name] === 'object' ? null : inst.options[name]);
				};
				options = $.extend({defaultDate: discardDate('defaultDate'),
					minDate: discardDate('minDate'), maxDate: discardDate('maxDate')}, options);
				inst.selectedDates = [];
				inst.drawDate = null;
			}
			var dates = inst.selectedDates;
			$.extend(inst.options, options);
			this.setDate(elem[0], dates, null, false, true);
			inst.pickingRange = false;
			var calendar = inst.options.calendar;
			var defaultDate = inst.get('defaultDate');
			inst.drawDate = this._checkMinMax((defaultDate ? defaultDate : inst.drawDate) ||
				defaultDate || calendar.today(), inst).newDate();
			if (!inst.inline) {
				this._attachments(elem, inst);
			}
			if (inst.inline || inst.div) {
				this._update(elem[0]);
			}
		},

		/** Attach events and trigger, if necessary.
			@memberof CalendarsPicker
			@private
			@param elem {jQuery} The control to affect.
			@param inst {object} The current instance settings. */
		_attachments: function(elem, inst) {
			elem.off('focus.' + inst.name);
			if (inst.options.showOnFocus) {
				elem.on('focus.' + inst.name, this.show);
			}
			if (inst.trigger) {
				inst.trigger.remove();
			}
			var trigger = inst.options.showTrigger;
			inst.trigger = (!trigger ? $([]) :
				$(trigger).clone().removeAttr('id').addClass(this._triggerClass)
					[inst.options.isRTL ? 'insertBefore' : 'insertAfter'](elem).
					click(function() {
						if (!plugin.isDisabled(elem[0])) {
							plugin[plugin.curInst === inst ? 'hide' : 'show'](elem[0]);
						}
					}));
			this._autoSize(elem, inst);
			var dates = this._extractDates(inst, elem.val());
			if (dates) {
				this.setDate(elem[0], dates, null, true);
			}
			var defaultDate = inst.get('defaultDate');
			if (inst.options.selectDefaultDate && defaultDate && inst.selectedDates.length === 0) {
				this.setDate(elem[0], (defaultDate || inst.options.calendar.today()).newDate());
			}
		},

		/** Apply the maximum length for the date format.
			@memberof CalendarsPicker
			@private
			@param elem {jQuery} The control to affect.
			@param inst {object} The current instance settings. */
		_autoSize: function(elem, inst) {
			if (inst.options.autoSize && !inst.inline) {
				var calendar = inst.options.calendar;
				var date = calendar.newDate(2009, 10, 20); // Ensure double digits
				var dateFormat = inst.get('dateFormat');
				if (dateFormat.match(/[DM]/)) {
					var findMax = function(names) {
						var max = 0;
						var maxI = 0;
						for (var i = 0; i < names.length; i++) {
							if (names[i].length > max) {
								max = names[i].length;
								maxI = i;
							}
						}
						return maxI;
					};
					date.month(findMax(calendar.local[dateFormat.match(/MM/) ? // Longest month
						'monthNames' : 'monthNamesShort']) + 1);
					date.day(findMax(calendar.local[dateFormat.match(/DD/) ? // Longest day
						'dayNames' : 'dayNamesShort']) + 20 - date.dayOfWeek());
				}
				inst.elem.attr('size', date.formatDate(dateFormat).length);
			}
		},

		_preDestroy: function(elem, inst) {
			if (inst.trigger) {
				inst.trigger.remove();
			}
			elem.empty().off('.' + inst.name);
			if (inst.inline && $.fn.mousewheel) {
				elem.unmousewheel();
			}
			if (!inst.inline && inst.options.autoSize) {
				elem.removeAttr('size');
			}
		},

		/** Apply multiple event functions.
			@memberof CalendarsPicker
			@param fns {function} The functions to apply.
			@example onShow: multipleEvents(fn1, fn2, ...) */
		multipleEvents: function(fns) {
			var funcs = arguments;
			return function(args) {
				for (var i = 0; i < funcs.length; i++) {
					funcs[i].apply(this, arguments);
				}
			};
		},

		/** Enable the control.
			@memberof CalendarsPicker
			@param elem {Element} The control to affect.
			@example $(selector).datepick('enable') */
		enable: function(elem) {
			elem = $(elem);
			if (!elem.hasClass(this._getMarker())) {
				return;
			}
			var inst = this._getInst(elem);
			if (inst.inline) {
				elem.children('.' + this._disableClass).remove().end().
					find('button,select').prop('disabled', false).end().
					find('a').attr('href', 'javascript:void(0)');
			}
			else {
				elem.prop('disabled', false);
				inst.trigger.filter('button.' + this._triggerClass).prop('disabled', false).end().
					filter('img.' + this._triggerClass).css({opacity: '1.0', cursor: ''});
			}
			this._disabled = $.map(this._disabled,
				function(value) { return (value === elem[0] ? null : value); }); // Delete entry
		},

		/** Disable the control.
			@memberof CalendarsPicker
			@param elem {Element} The control to affect.
			@example $(selector).datepick('disable') */
		disable: function(elem) {
			elem = $(elem);
			if (!elem.hasClass(this._getMarker())) {
				return;
			}
			var inst = this._getInst(elem);
			if (inst.inline) {
				var inline = elem.children(':last');
				var offset = inline.offset();
				var relOffset = {left: 0, top: 0};
				inline.parents().each(function() {
					if ($(this).css('position') === 'relative') {
						relOffset = $(this).offset();
						return false;
					}
				});
				var zIndex = elem.css('zIndex');
				zIndex = (zIndex === 'auto' ? 0 : parseInt(zIndex, 10)) + 1;
				elem.prepend('<div class="' + this._disableClass + '" style="' +
					'width: ' + inline.outerWidth() + 'px; height: ' + inline.outerHeight() +
					'px; left: ' + (offset.left - relOffset.left) + 'px; top: ' +
					(offset.top - relOffset.top) + 'px; z-index: ' + zIndex + '"></div>').
					find('button,select').prop('disabled', true).end().
					find('a').removeAttr('href');
			}
			else {
				elem.prop('disabled', true);
				inst.trigger.filter('button.' + this._triggerClass).prop('disabled', true).end().
					filter('img.' + this._triggerClass).css({opacity: '0.5', cursor: 'default'});
			}
			this._disabled = $.map(this._disabled,
				function(value) { return (value === elem[0] ? null : value); }); // Delete entry
			this._disabled.push(elem[0]);
		},

		/** Is the first field in a jQuery collection disabled as a datepicker?
			@memberof CalendarsPicker
			@param elem {Element} The control to examine.
			@return {boolean} <code>true</code> if disabled, <code>false</code> if enabled.
			@example if ($(selector).datepick('isDisabled')) {...} */
		isDisabled: function(elem) {
			return (elem && $.inArray(elem, this._disabled) > -1);
		},

		/** Show a popup datepicker.
			@memberof CalendarsPicker
			@param elem {Event|Element} a focus event or the control to use.
			@example $(selector).datepick('show') */
		show: function(elem) {
			elem = $(elem.target || elem);
			var inst = plugin._getInst(elem);
			if (plugin.curInst === inst) {
				return;
			}
			if (plugin.curInst) {
				plugin.hide(plugin.curInst, true);
			}
			if (!$.isEmptyObject(inst)) {
				// Retrieve existing date(s)
				inst.lastVal = null;
				inst.selectedDates = plugin._extractDates(inst, elem.val());
				inst.pickingRange = false;
				inst.drawDate = plugin._checkMinMax((inst.selectedDates[0] ||
					inst.get('defaultDate') || inst.options.calendar.today()).newDate(), inst);
				inst.prevDate = inst.drawDate.newDate();
				plugin.curInst = inst;
				// Generate content
				plugin._update(elem[0], true);
				// Adjust position before showing
				var offset = plugin._checkOffset(inst);
				inst.div.css({left: offset.left, top: offset.top});
				// And display
				var showAnim = inst.options.showAnim;
				var showSpeed = inst.options.showSpeed;
				showSpeed = (showSpeed === 'normal' && $.ui &&
					parseInt($.ui.version.substring(2)) >= 8 ? '_default' : showSpeed);
				if ($.effects && ($.effects[showAnim] || ($.effects.effect && $.effects.effect[showAnim]))) {
					var data = inst.div.data(); // Update old effects data
					for (var key in data) {
						if (key.match(/^ec\.storage\./)) {
							data[key] = inst._mainDiv.css(key.replace(/ec\.storage\./, ''));
						}
					}
					inst.div.data(data).show(showAnim, inst.options.showOptions, showSpeed);
				}
				else {
					inst.div[showAnim || 'show'](showAnim ? showSpeed : 0);
				}
			}
		},

		/** Extract possible dates from a string.
			@memberof CalendarsPicker
			@private
			@param inst {object} The current instance settings.
			@param text {string} The text to extract from.
			@return {CDate[]} The extracted dates. */
		_extractDates: function(inst, datesText) {
			if (datesText === inst.lastVal) {
				return;
			}
			inst.lastVal = datesText;
			datesText = datesText.split(inst.options.multiSelect ? inst.options.multiSeparator :
				(inst.options.rangeSelect ? inst.options.rangeSeparator : '\x00'));
			var dates = [];
			for (var i = 0; i < datesText.length; i++) {
				try {
					var date = inst.options.calendar.parseDate(inst.get('dateFormat'), datesText[i]);
					if (date) {
						var found = false;
						for (var j = 0; j < dates.length; j++) {
							if (dates[j].compareTo(date) === 0) {
								found = true;
								break;
							}
						}
						if (!found) {
							dates.push(date);
						}
					}
				}
				catch (e) {
					// Ignore
				}
			}
			dates.splice(inst.options.multiSelect || (inst.options.rangeSelect ? 2 : 1), dates.length);
			if (inst.options.rangeSelect && dates.length === 1) {
				dates[1] = dates[0];
			}
			return dates;
		},

		/** Update the datepicker display.
			@memberof CalendarsPicker
			@private
			@param elem {Event|Element} a focus event or the control to use.
			@param hidden {boolean} <code>true</code> to initially hide the datepicker. */
		_update: function(elem, hidden) {
			elem = $(elem.target || elem);
			var inst = plugin._getInst(elem);
			if (!$.isEmptyObject(inst)) {
				if (inst.inline || plugin.curInst === inst) {
					if ($.isFunction(inst.options.onChangeMonthYear) && (!inst.prevDate ||
							inst.prevDate.year() !== inst.drawDate.year() ||
							inst.prevDate.month() !== inst.drawDate.month())) {
						inst.options.onChangeMonthYear.apply(elem[0],
							[inst.drawDate.year(), inst.drawDate.month()]);
					}
				}
				if (inst.inline) {
					var index = $('a, :input', elem).index($(':focus', elem));
					elem.html(this._generateContent(elem[0], inst));
					var focus = elem.find('a, :input');
					focus.eq(Math.max(Math.min(index, focus.length - 1), 0)).focus();
				}
				else if (plugin.curInst === inst) {
					if (!inst.div) {
						inst.div = $('<div></div>').addClass(this._popupClass).
							css({display: (hidden ? 'none' : 'static'), position: 'absolute',
								left: elem.offset().left, top: elem.offset().top + elem.outerHeight()}).
							appendTo($(inst.options.popupContainer || 'body'));
						if ($.fn.mousewheel) {
							inst.div.mousewheel(this._doMouseWheel);
						}
					}
					inst.div.html(this._generateContent(elem[0], inst));
					elem.focus();
				}
			}
		},

		/** Update the input field and any alternate field with the current dates.
			@memberof CalendarsPicker
			@private
			@param elem {Element} The control to use.
			@param keyUp {boolean} <code>true</code> if coming from <code>keyUp</code> processing (internal). */
		_updateInput: function(elem, keyUp) {
			var inst = this._getInst(elem);
			if (!$.isEmptyObject(inst)) {
				var value = '';
				var altValue = '';
				var sep = (inst.options.multiSelect ? inst.options.multiSeparator :
					inst.options.rangeSeparator);
				var calendar = inst.options.calendar;
				var dateFormat = inst.get('dateFormat');
				var altFormat = inst.options.altFormat || dateFormat;
				for (var i = 0; i < inst.selectedDates.length; i++) {
					value += (keyUp ? '' : (i > 0 ? sep : '') +
						calendar.formatDate(dateFormat, inst.selectedDates[i]));
					altValue += (i > 0 ? sep : '') +
						calendar.formatDate(altFormat, inst.selectedDates[i]);
				}
				if (!inst.inline && !keyUp) {
					$(elem).val(value);
				}
				$(inst.options.altField).val(altValue);
				if ($.isFunction(inst.options.onSelect) && !keyUp && !inst.inSelect) {
					inst.inSelect = true; // Prevent endless loops
					inst.options.onSelect.apply(elem, [inst.selectedDates]);
					inst.inSelect = false;
				}
			}
		},

		/** Retrieve the size of left and top borders for an element.
			@memberof CalendarsPicker
			@private
			@param elem {jQuery} The element of interest.
			@return {number[]} The left and top borders. */
		_getBorders: function(elem) {
			var convert = function(value) {
				return {thin: 1, medium: 3, thick: 5}[value] || value;
			};
			return [parseFloat(convert(elem.css('border-left-width'))),
				parseFloat(convert(elem.css('border-top-width')))];
		},

		/** Check positioning to remain on the screen.
			@memberof CalendarsPicker
			@private
			@param inst {object} The current instance settings.
			@return {object} The updated offset for the datepicker. */
		_checkOffset: function(inst) {
			var base = (inst.elem.is(':hidden') && inst.trigger ? inst.trigger : inst.elem);
			var offset = base.offset();
			var browserWidth = $(window).width();
			var browserHeight = $(window).height();
			if (browserWidth === 0) {
				return offset;
			}
			var isFixed = false;
			$(inst.elem).parents().each(function() {
				isFixed |= $(this).css('position') === 'fixed';
				return !isFixed;
			});
			var scrollX = document.documentElement.scrollLeft || document.body.scrollLeft;
			var scrollY = document.documentElement.scrollTop || document.body.scrollTop;
			var above = offset.top - (isFixed ? scrollY : 0) - inst.div.outerHeight();
			var below = offset.top - (isFixed ? scrollY : 0) + base.outerHeight();
			var alignL = offset.left - (isFixed ? scrollX : 0);
			var alignR = offset.left - (isFixed ? scrollX : 0) + base.outerWidth() - inst.div.outerWidth();
			var tooWide = (offset.left - scrollX + inst.div.outerWidth()) > browserWidth;
			var tooHigh = (offset.top - scrollY + inst.elem.outerHeight() +
				inst.div.outerHeight()) > browserHeight;
			inst.div.css('position', isFixed ? 'fixed' : 'absolute');
			var alignment = inst.options.alignment;
			if (alignment === 'topLeft') {
				offset = {left: alignL, top: above};
			}
			else if (alignment === 'topRight') {
				offset = {left: alignR, top: above};
			}
			else if (alignment === 'bottomLeft') {
				offset = {left: alignL, top: below};
			}
			else if (alignment === 'bottomRight') {
				offset = {left: alignR, top: below};
			}
			else if (alignment === 'top') {
				offset = {left: (inst.options.isRTL || tooWide ? alignR : alignL), top: above};
			}
			else { // bottom
				offset = {left: (inst.options.isRTL || tooWide ? alignR : alignL),
					top: (tooHigh ? above : below)};
			}
			offset.left = Math.max((isFixed ? 0 : scrollX), offset.left);
			offset.top = Math.max((isFixed ? 0 : scrollY), offset.top);
			return offset;
		},

		/** Close date picker if clicked elsewhere.
			@memberof CalendarsPicker
			@private
			@param event {MouseEvent} The mouse click to check. */
		_checkExternalClick: function(event) {
			if (!plugin.curInst) {
				return;
			}
			var elem = $(event.target);
			if (elem.closest('.' + plugin._popupClass + ',.' + plugin._triggerClass).length === 0 &&
					!elem.hasClass(plugin._getMarker())) {
				plugin.hide(plugin.curInst);
			}
		},

		/** Hide a popup datepicker.
			@memberof CalendarsPicker
			@param elem {Element|object} The control to use or the current instance settings.
			@param immediate {boolean} <code>true</code> to close immediately without animation (internal).
			@example $(selector).datepick('hide') */
		hide: function(elem, immediate) {
			if (!elem) {
				return;
			}
			var inst = this._getInst(elem);
			if ($.isEmptyObject(inst)) {
				inst = elem;
			}
			if (inst && inst === plugin.curInst) {
				var showAnim = (immediate ? '' : inst.options.showAnim);
				var showSpeed = inst.options.showSpeed;
				showSpeed = (showSpeed === 'normal' && $.ui &&
					parseInt($.ui.version.substring(2)) >= 8 ? '_default' : showSpeed);
				var postProcess = function() {
					if (!inst.div) {
						return;
					}
					inst.div.remove();
					inst.div = null;
					plugin.curInst = null;
					if ($.isFunction(inst.options.onClose)) {
						inst.options.onClose.apply(elem, [inst.selectedDates]);
					}
				};
				inst.div.stop();
				if ($.effects && ($.effects[showAnim] || ($.effects.effect && $.effects.effect[showAnim]))) {
					inst.div.hide(showAnim, inst.options.showOptions, showSpeed, postProcess);
				}
				else {
					var hideAnim = (showAnim === 'slideDown' ? 'slideUp' :
						(showAnim === 'fadeIn' ? 'fadeOut' : 'hide'));
					inst.div[hideAnim]((showAnim ? showSpeed : ''), postProcess);
				}
				if (!showAnim) {
					postProcess();
				}
			}
		},

		/** Handle keystrokes in the datepicker.
			@memberof CalendarsPicker
			@private
			@param event {KeyEvent} The keystroke.
			@return {boolean} <code>true</code> if not handled, <code>false</code> if handled. */
		_keyDown: function(event) {
			var elem = (event.data && event.data.elem) || event.target;
			var inst = plugin._getInst(elem);
			var handled = false;
			if (inst.inline || inst.div) {
				if (event.keyCode === 9) { // Tab - close
					plugin.hide(elem);
				}
				else if (event.keyCode === 13) { // Enter - select
					plugin.selectDate(elem,
						$('a.' + inst.options.renderer.highlightedClass, inst.div)[0]);
					handled = true;
				}
				else { // Command keystrokes
					var commands = inst.options.commands;
					for (var name in commands) {
						var command = commands[name];
						if (command.keystroke.keyCode === event.keyCode &&
								!!command.keystroke.ctrlKey === !!(event.ctrlKey || event.metaKey) &&
								!!command.keystroke.altKey === event.altKey &&
								!!command.keystroke.shiftKey === event.shiftKey) {
							plugin.performAction(elem, name);
							handled = true;
							break;
						}
					}
				}
			}
			else { // Show on 'current' keystroke
				var command = inst.options.commands.current;
				if (command.keystroke.keyCode === event.keyCode &&
						!!command.keystroke.ctrlKey === !!(event.ctrlKey || event.metaKey) &&
						!!command.keystroke.altKey === event.altKey &&
						!!command.keystroke.shiftKey === event.shiftKey) {
					plugin.show(elem);
					handled = true;
				}
			}
			inst.ctrlKey = ((event.keyCode < 48 && event.keyCode !== 32) || event.ctrlKey || event.metaKey);
			if (handled) {
				event.preventDefault();
				event.stopPropagation();
			}
			return !handled;
		},

		/** Filter keystrokes in the datepicker.
			@memberof CalendarsPicker
			@private
			@param event {KeyEvent} The keystroke.
			@return {boolean} <code>true</code> if allowed, <code>false</code> if not allowed. */
		_keyPress: function(event) {
			var inst = plugin._getInst((event.data && event.data.elem) || event.target);
			if (!$.isEmptyObject(inst) && inst.options.constrainInput) {
				var ch = String.fromCharCode(event.keyCode || event.charCode);
				var allowedChars = plugin._allowedChars(inst);
				return (event.metaKey || inst.ctrlKey || ch < ' ' ||
					!allowedChars || allowedChars.indexOf(ch) > -1);
			}
			return true;
		},

		/** Determine the set of characters allowed by the date format.
			@memberof CalendarsPicker
			@private
			@param inst {object} The current instance settings.
			@return {string} The set of allowed characters, or <code>null</code> if anything allowed. */
		_allowedChars: function(inst) {
			var allowedChars = (inst.options.multiSelect ? inst.options.multiSeparator :
				(inst.options.rangeSelect ? inst.options.rangeSeparator : ''));
			var literal = false;
			var hasNum = false;
			var dateFormat = inst.get('dateFormat');
			for (var i = 0; i < dateFormat.length; i++) {
				var ch = dateFormat.charAt(i);
				if (literal) {
					if (ch === "'" && dateFormat.charAt(i + 1) !== "'") {
						literal = false;
					}
					else {
						allowedChars += ch;
					}
				}
				else {
					switch (ch) {
						case 'd': case 'm': case 'o': case 'w':
							allowedChars += (hasNum ? '' : '0123456789'); hasNum = true; break;
						case 'y': case '@': case '!':
							allowedChars += (hasNum ? '' : '0123456789') + '-'; hasNum = true; break;
						case 'J':
							allowedChars += (hasNum ? '' : '0123456789') + '-.'; hasNum = true; break;
						case 'D': case 'M': case 'Y':
							return null; // Accept anything
						case "'":
							if (dateFormat.charAt(i + 1) === "'") {
								allowedChars += "'";
							}
							else {
								literal = true;
							}
							break;
						default:
							allowedChars += ch;
					}
				}
			}
			return allowedChars;
		},

		/** Synchronise datepicker with the field.
			@memberof CalendarsPicker
			@private
			@param event {KeyEvent} The keystroke.
			@return {boolean} <code>true</code> if allowed, <code>false</code> if not allowed. */
		_keyUp: function(event) {
			var elem = (event.data && event.data.elem) || event.target;
			var inst = plugin._getInst(elem);
			if (!$.isEmptyObject(inst) && !inst.ctrlKey && inst.lastVal !== inst.elem.val()) {
				try {
					var dates = plugin._extractDates(inst, inst.elem.val());
					if (dates.length > 0) {
						plugin.setDate(elem, dates, null, true);
					}
				}
				catch (event) {
					// Ignore
				}
			}
			return true;
		},

		/** Increment/decrement month/year on mouse wheel activity.
			@memberof CalendarsPicker
			@private
			@param event {event} The mouse wheel event.
			@param delta {number} The amount of change. */
		_doMouseWheel: function(event, delta) {
			var elem = (plugin.curInst && plugin.curInst.elem[0]) ||
				$(event.target).closest('.' + plugin._getMarker())[0];
			if (plugin.isDisabled(elem)) {
				return;
			}
			var inst = plugin._getInst(elem);
			if (inst.options.useMouseWheel) {
				delta = (delta < 0 ? -1 : +1);
				plugin.changeMonth(elem, -inst.options[event.ctrlKey ? 'monthsToJump' : 'monthsToStep'] * delta);
			}
			event.preventDefault();
		},

		/** Clear an input and close a popup datepicker.
			@memberof CalendarsPicker
			@param elem {Element} The control to use.
			@example $(selector).datepick('clear') */
		clear: function(elem) {
			var inst = this._getInst(elem);
			if (!$.isEmptyObject(inst)) {
				inst.selectedDates = [];
				this.hide(elem);
				var defaultDate = inst.get('defaultDate');
				if (inst.options.selectDefaultDate && defaultDate) {
					this.setDate(elem, (defaultDate || inst.options.calendar.today()).newDate());
				}
				else {
					this._updateInput(elem);
				}
			}
		},

		/** Retrieve the selected date(s) for a datepicker.
			@memberof CalendarsPicker
			@param elem {Element} The control to examine.
			@return {CDate[]} The selected date(s).
			@example var dates = $(selector).datepick('getDate') */
		getDate: function(elem) {
			var inst = this._getInst(elem);
			return (!$.isEmptyObject(inst) ? inst.selectedDates : []);
		},

		/** Set the selected date(s) for a datepicker.
			@memberof CalendarsPicker
			@param elem {Element} the control to examine.
			@param dates {CDate|number|string|array} the selected date(s).
			@param [endDate] {CDate|number|string} the ending date for a range.
			@param [keyUp] {boolean} <code>true</code> if coming from <code>keyUp</code> processing (internal).
			@param [setOpt] {boolean} <code>true</code> if coming from option processing (internal).
			@example $(selector).datepick('setDate', new Date(2014, 12-1, 25))
 $(selector).datepick('setDate', '12/25/2014', '01/01/2015')
 $(selector).datepick('setDate', [date1, date2, date3]) */
		setDate: function(elem, dates, endDate, keyUp, setOpt) {
			var inst = this._getInst(elem);
			if (!$.isEmptyObject(inst)) {
				if (!$.isArray(dates)) {
					dates = [dates];
					if (endDate) {
						dates.push(endDate);
					}
				}
				var minDate = inst.get('minDate');
				var maxDate = inst.get('maxDate');
				var curDate = inst.selectedDates[0];
				inst.selectedDates = [];
				for (var i = 0; i < dates.length; i++) {
					var date = inst.options.calendar.determineDate(
						dates[i], null, curDate, inst.get('dateFormat'), inst.getConfig());
					if (date) {
						if ((!minDate || date.compareTo(minDate) !== -1) &&
								(!maxDate || date.compareTo(maxDate) !== +1)) {
							var found = false;
							for (var j = 0; j < inst.selectedDates.length; j++) {
								if (inst.selectedDates[j].compareTo(date) === 0) {
									found = true;
									break;
								}
							}
							if (!found) {
								inst.selectedDates.push(date);
							}
						}
					}
				}
				inst.selectedDates.splice(inst.options.multiSelect ||
					(inst.options.rangeSelect ? 2 : 1), inst.selectedDates.length);
				if (inst.options.rangeSelect) {
					switch (inst.selectedDates.length) {
						case 1: inst.selectedDates[1] = inst.selectedDates[0]; break;
						case 2: inst.selectedDates[1] =
							(inst.selectedDates[0].compareTo(inst.selectedDates[1]) === +1 ?
							inst.selectedDates[0] : inst.selectedDates[1]); break;
					}
					inst.pickingRange = false;
				}
				inst.prevDate = (inst.drawDate ? inst.drawDate.newDate() : null);
				inst.drawDate = this._checkMinMax((inst.selectedDates[0] ||
					inst.get('defaultDate') || inst.options.calendar.today()).newDate(), inst);
				if (!setOpt) {
					this._update(elem);
					this._updateInput(elem, keyUp);
				}
			}
		},

		/** Determine whether a date is selectable for this datepicker.
			@memberof CalendarsPicker
			@private
			@param elem {Element} The control to check.
			@param date {CDate|string|number} The date to check.
			@return {boolean} <code>true</code> if selectable, <code>false</code> if not.
			@example var selectable = $(selector).datepick('isSelectable', date) */
		isSelectable: function(elem, date) {
			var inst = this._getInst(elem);
			if ($.isEmptyObject(inst)) {
				return false;
			}
			date = inst.options.calendar.determineDate(date,
				inst.selectedDates[0] || inst.options.calendar.today(), null,
				inst.options.dateFormat, inst.getConfig());
			return this._isSelectable(elem, date, inst.options.onDate,
				inst.get('minDate'), inst.get('maxDate'));
		},

		/** Internally determine whether a date is selectable for this datepicker.
			@memberof CalendarsPicker
			@private
			@param elem {Element} the control to check.
			@param date {CDate} The date to check.
			@param onDate {function|boolean} Any <code>onDate</code> callback or <code>callback.selectable</code>.
			@param minDate {CDate} The minimum allowed date.
			@param maxDate {CDate} The maximum allowed date.
			@return {boolean} <code>true</code> if selectable, <code>false</code> if not. */
		_isSelectable: function(elem, date, onDate, minDate, maxDate) {
			var dateInfo = (typeof onDate === 'boolean' ? {selectable: onDate} :
				(!$.isFunction(onDate) ? {} : onDate.apply(elem, [date, true])));
			return (dateInfo.selectable !== false) &&
				(!minDate || date.toJD() >= minDate.toJD()) && (!maxDate || date.toJD() <= maxDate.toJD());
		},

		/** Perform a named action for a datepicker.
			@memberof CalendarsPicker
			@param elem {element} The control to affect.
			@param action {string} The name of the action. */
		performAction: function(elem, action) {
			var inst = this._getInst(elem);
			if (!$.isEmptyObject(inst) && !this.isDisabled(elem)) {
				var commands = inst.options.commands;
				if (commands[action] && commands[action].enabled.apply(elem, [inst])) {
					commands[action].action.apply(elem, [inst]);
				}
			}
		},

		/** Set the currently shown month, defaulting to today's.
			@memberof CalendarsPicker
			@param elem {Element} The control to affect.
			@param [year] {number} The year to show.
			@param [month] {number} The month to show (1-12).
			@param [day] {number} The day to show.
			@example $(selector).datepick('showMonth', 2014, 12, 25) */
		showMonth: function(elem, year, month, day) {
			var inst = this._getInst(elem);
			if (!$.isEmptyObject(inst) && (day != null ||
					(inst.drawDate.year() !== year || inst.drawDate.month() !== month))) {
				inst.prevDate = inst.drawDate.newDate();
				var calendar = inst.options.calendar;
				var show = this._checkMinMax((year != null ?
					calendar.newDate(year, month, 1) : calendar.today()), inst);
				inst.drawDate.date(show.year(), show.month(), 
					(day != null ? day : Math.min(inst.drawDate.day(),
					calendar.daysInMonth(show.year(), show.month()))));
				this._update(elem);
			}
		},

		/** Adjust the currently shown month.
			@memberof CalendarsPicker
			@param elem {Element} The control to affect.
			@param offset {number} The number of months to change by.
			@example $(selector).datepick('changeMonth', 2)*/
		changeMonth: function(elem, offset) {
			var inst = this._getInst(elem);
			if (!$.isEmptyObject(inst)) {
				var date = inst.drawDate.newDate().add(offset, 'm');
				this.showMonth(elem, date.year(), date.month());
			}
		},

		/** Adjust the currently shown day.
			@memberof CalendarsPicker
			@param elem {Element} The control to affect.
			@param offset {number} The number of days to change by.
			@example $(selector).datepick('changeDay', 7)*/
		changeDay: function(elem, offset) {
			var inst = this._getInst(elem);
			if (!$.isEmptyObject(inst)) {
				var date = inst.drawDate.newDate().add(offset, 'd');
				this.showMonth(elem, date.year(), date.month(), date.day());
			}
		},

		/** Restrict a date to the minimum/maximum specified.
			@memberof CalendarsPicker
			@private
			@param date {CDate} The date to check.
			@param inst {object} The current instance settings. */
		_checkMinMax: function(date, inst) {
			var minDate = inst.get('minDate');
			var maxDate = inst.get('maxDate');
			date = (minDate && date.compareTo(minDate) === -1 ? minDate.newDate() : date);
			date = (maxDate && date.compareTo(maxDate) === +1 ? maxDate.newDate() : date);
			return date;
		},

		/** Retrieve the date associated with an entry in the datepicker.
			@memberof CalendarsPicker
			@param elem {Element} The control to examine.
			@param target {Element} The selected datepicker element.
			@return {CDate} The corresponding date, or <code>null</code>.			
			@example var date = $(selector).datepick('retrieveDate', $('div.datepick-popup a:contains(10)')[0]) */
		retrieveDate: function(elem, target) {
			var inst = this._getInst(elem);
			return ($.isEmptyObject(inst) ? null : inst.options.calendar.fromJD(
				parseFloat(target.className.replace(/^.*jd(\d+\.5).*$/, '$1'))));
		},

		/** Select a date for this datepicker.
			@memberof CalendarsPicker
			@param elem {Element} The control to examine.
			@param target {Element} The selected datepicker element.
			@example $(selector).datepick('selectDate', $('div.datepick-popup a:contains(10)')[0]) */
		selectDate: function(elem, target) {
			var inst = this._getInst(elem);
			if (!$.isEmptyObject(inst) && !this.isDisabled(elem)) {
				var date = this.retrieveDate(elem, target);
				if (inst.options.multiSelect) {
					var found = false;
					for (var i = 0; i < inst.selectedDates.length; i++) {
						if (date.compareTo(inst.selectedDates[i]) === 0) {
							inst.selectedDates.splice(i, 1);
							found = true;
							break;
						}
					}
					if (!found && inst.selectedDates.length < inst.options.multiSelect) {
						inst.selectedDates.push(date);
					}
				}
				else if (inst.options.rangeSelect) {
					if (inst.pickingRange) {
						inst.selectedDates[1] = date;
					}
					else {
						inst.selectedDates = [date, date];
					}
					inst.pickingRange = !inst.pickingRange;
				}
				else {
					inst.selectedDates = [date];
				}
				inst.prevDate = inst.drawDate = date.newDate();
				this._updateInput(elem);
				if (inst.inline || inst.pickingRange || inst.selectedDates.length <
						(inst.options.multiSelect || (inst.options.rangeSelect ? 2 : 1))) {
					this._update(elem);
				}
				else {
					this.hide(elem);
				}
			}
		},

		/** Generate the datepicker content for this control.
			@memberof CalendarsPicker
			@private
			@param elem {Element} The control to affect.
			@param inst {object} The current instance settings.
			@return {jQuery} The datepicker content */
		_generateContent: function(elem, inst) {
			var monthsToShow = inst.options.monthsToShow;
			monthsToShow = ($.isArray(monthsToShow) ? monthsToShow : [1, monthsToShow]);
			inst.drawDate = this._checkMinMax(
				inst.drawDate || inst.get('defaultDate') || inst.options.calendar.today(), inst);
			var drawDate = inst.drawDate.newDate().add(-inst.options.monthsOffset, 'm');
			// Generate months
			var monthRows = '';
			for (var row = 0; row < monthsToShow[0]; row++) {
				var months = '';
				for (var col = 0; col < monthsToShow[1]; col++) {
					months += this._generateMonth(elem, inst, drawDate.year(),
						drawDate.month(), inst.options.calendar, inst.options.renderer, (row === 0 && col === 0));
					drawDate.add(1, 'm');
				}
				monthRows += this._prepare(inst.options.renderer.monthRow, inst).replace(/\{months\}/, months);
			}
			var picker = this._prepare(inst.options.renderer.picker, inst).replace(/\{months\}/, monthRows).
				replace(/\{weekHeader\}/g, this._generateDayHeaders(inst, inst.options.calendar, inst.options.renderer));
			// Add commands
			var addCommand = function(type, open, close, name, classes) {
				if (picker.indexOf('{' + type + ':' + name + '}') === -1) {
					return;
				}
				var command = inst.options.commands[name];
				var date = (inst.options.commandsAsDateFormat ? command.date.apply(elem, [inst]) : null);
				picker = picker.replace(new RegExp('\\{' + type + ':' + name + '\\}', 'g'),
					'<' + open + (command.status ? ' title="' + inst.options[command.status] + '"' : '') +
					' class="' + inst.options.renderer.commandClass + ' ' +
					inst.options.renderer.commandClass + '-' + name + ' ' + classes +
					(command.enabled(inst) ? '' : ' ' + inst.options.renderer.disabledClass) + '">' +
					(date ? date.formatDate(inst.options[command.text]) :
					inst.options[command.text]) + '</' + close + '>');
			};
			for (var name in inst.options.commands) {
				addCommand('button', 'button type="button"', 'button', name,
					inst.options.renderer.commandButtonClass);
				addCommand('link', 'a href="javascript:void(0)"', 'a', name,
					inst.options.renderer.commandLinkClass);
			}
			picker = $(picker);
			if (monthsToShow[1] > 1) {
				var count = 0;
				$(inst.options.renderer.monthSelector, picker).each(function() {
					var nth = ++count % monthsToShow[1];
					$(this).addClass(nth === 1 ? 'first' : (nth === 0 ? 'last' : ''));
				});
			}
			// Add datepicker behaviour
			var self = this;
			function removeHighlight() {
				(inst.inline ? $(this).closest('.' + self._getMarker()) : inst.div).
					find(inst.options.renderer.daySelector + ' a').
					removeClass(inst.options.renderer.highlightedClass);
			}
			picker.find(inst.options.renderer.daySelector + ' a').hover(
					function() {
						removeHighlight.apply(this);
						$(this).addClass(inst.options.renderer.highlightedClass);
					},
					removeHighlight).
				click(function() {
					self.selectDate(elem, this);
				}).end().
				find('select.' + this._monthYearClass + ':not(.' + this._anyYearClass + ')').
				change(function() {
					var monthYear = $(this).val().split('/');
					self.showMonth(elem, parseInt(monthYear[1], 10), parseInt(monthYear[0], 10));
				}).end().
				find('select.' + this._anyYearClass).click(function() {
					$(this).css('visibility', 'hidden').
						next('input').css({left: this.offsetLeft, top: this.offsetTop,
						width: this.offsetWidth, height: this.offsetHeight}).show().focus();
				}).end().
				find('input.' + self._monthYearClass).change(function() {
					try {
						var year = parseInt($(this).val(), 10);
						year = (isNaN(year) ? inst.drawDate.year() : year);
						self.showMonth(elem, year, inst.drawDate.month(), inst.drawDate.day());
					}
					catch (e) {
						alert(e);
					}
				}).keydown(function(event) {
					if (event.keyCode === 13) { // Enter
						$(event.elem).change();
					}
					else if (event.keyCode === 27) { // Escape
						$(event.elem).hide().prev('select').css('visibility', 'visible');
						inst.elem.focus();
					}
				});
			// Add keyboard handling
			var data = {elem: inst.elem[0]};
			picker.keydown(data, this._keyDown).keypress(data, this._keyPress).keyup(data, this._keyUp);
			// Add command behaviour
			picker.find('.' + inst.options.renderer.commandClass).click(function() {
					if (!$(this).hasClass(inst.options.renderer.disabledClass)) {
						var action = this.className.replace(
							new RegExp('^.*' + inst.options.renderer.commandClass + '-([^ ]+).*$'), '$1');
						plugin.performAction(elem, action);
					}
				});
			// Add classes
			if (inst.options.isRTL) {
				picker.addClass(inst.options.renderer.rtlClass);
			}
			if (monthsToShow[0] * monthsToShow[1] > 1) {
				picker.addClass(inst.options.renderer.multiClass);
			}
			if (inst.options.pickerClass) {
				picker.addClass(inst.options.pickerClass);
			}
			// Resize
			$('body').append(picker);
			var width = 0;
			picker.find(inst.options.renderer.monthSelector).each(function() {
				width += $(this).outerWidth();
			});
			picker.width(width / monthsToShow[0]);
			// Pre-show customisation
			if ($.isFunction(inst.options.onShow)) {
				inst.options.onShow.apply(elem, [picker, inst.options.calendar, inst]);
			}
			return picker;
		},

		/** Generate the content for a single month.
			@memberof CalendarsPicker
			@private
			@param elem {Element} The control to affect.
			@param inst {object} The current instance settings.
			@param year {number} The year to generate.
			@param month {number} The month to generate.
			@param calendar {BaseCalendar} The current calendar.
			@param renderer {object} The rendering templates.
			@param first {boolean} <code>true</code> if first of multiple months.
			@return {string} The month content. */
		_generateMonth: function(elem, inst, year, month, calendar, renderer, first) {
			var daysInMonth = calendar.daysInMonth(year, month);
			var monthsToShow = inst.options.monthsToShow;
			monthsToShow = ($.isArray(monthsToShow) ? monthsToShow : [1, monthsToShow]);
			var fixedWeeks = inst.options.fixedWeeks || (monthsToShow[0] * monthsToShow[1] > 1);
			var firstDay = inst.options.firstDay;
			firstDay = (firstDay == null ? calendar.local.firstDay : firstDay);
			var leadDays = (calendar.dayOfWeek(year, month, calendar.minDay) -
				firstDay + calendar.daysInWeek()) % calendar.daysInWeek();
			var numWeeks = (fixedWeeks ? 6 : Math.ceil((leadDays + daysInMonth) / calendar.daysInWeek()));
			var selectOtherMonths = inst.options.selectOtherMonths && inst.options.showOtherMonths;
			var minDate = (inst.pickingRange ? inst.selectedDates[0] : inst.get('minDate'));
			var maxDate = inst.get('maxDate');
			var showWeeks = renderer.week.indexOf('{weekOfYear}') > -1;
			var today = calendar.today();
			var drawDate = calendar.newDate(year, month, calendar.minDay);
			drawDate.add(-leadDays - (fixedWeeks &&
				(drawDate.dayOfWeek() === firstDay || drawDate.daysInMonth() < calendar.daysInWeek())?
				calendar.daysInWeek() : 0), 'd');
			var jd = drawDate.toJD();
			// Generate weeks
			var weeks = '';
			for (var week = 0; week < numWeeks; week++) {
				var weekOfYear = (!showWeeks ? '' : '<span class="jd' + jd + '">' +
					($.isFunction(inst.options.calculateWeek) ?
					inst.options.calculateWeek(drawDate) : drawDate.weekOfYear()) + '</span>');
				var days = '';
				for (var day = 0; day < calendar.daysInWeek(); day++) {
					var selected = false;
					if (inst.options.rangeSelect && inst.selectedDates.length > 0) {
						selected = (drawDate.compareTo(inst.selectedDates[0]) !== -1 &&
							drawDate.compareTo(inst.selectedDates[1]) !== +1)
					}
					else {
						for (var i = 0; i < inst.selectedDates.length; i++) {
							if (inst.selectedDates[i].compareTo(drawDate) === 0) {
								selected = true;
								break;
							}
						}
					}
					var dateInfo = (!$.isFunction(inst.options.onDate) ? {} :
						inst.options.onDate.apply(elem, [drawDate, drawDate.month() === month]));
					var selectable = (selectOtherMonths || drawDate.month() === month) &&
						this._isSelectable(elem, drawDate, dateInfo.selectable, minDate, maxDate);
					days += this._prepare(renderer.day, inst).replace(/\{day\}/g,
						(selectable ? '<a href="javascript:void(0)"' : '<span') +
						' class="jd' + jd + ' ' + (dateInfo.dateClass || '') +
						(selected && (selectOtherMonths || drawDate.month() === month) ?
						' ' + renderer.selectedClass : '') +
						(selectable ? ' ' + renderer.defaultClass : '') +
						(drawDate.weekDay() ? '' : ' ' + renderer.weekendClass) +
						(drawDate.month() === month ? '' : ' ' + renderer.otherMonthClass) +
						(drawDate.compareTo(today) === 0 && drawDate.month() === month ?
						' ' + renderer.todayClass : '') +
						(drawDate.compareTo(inst.drawDate) === 0 && drawDate.month() === month ?
						' ' + renderer.highlightedClass : '') + '"' +
						(dateInfo.title || (inst.options.dayStatus && selectable) ? ' title="' +
						(dateInfo.title || drawDate.formatDate(inst.options.dayStatus)) + '"' : '') + '>' +
						(inst.options.showOtherMonths || drawDate.month() === month ?
						dateInfo.content || drawDate.day() : '&#160;') +
						(selectable ? '</a>' : '</span>'));
					drawDate.add(1, 'd');
					jd++;
				}
				weeks += this._prepare(renderer.week, inst).replace(/\{days\}/g, days).
					replace(/\{weekOfYear\}/g, weekOfYear);
			}
			var monthHeader = this._prepare(renderer.month, inst).match(/\{monthHeader(:[^\}]+)?\}/);
			monthHeader = (monthHeader[0].length <= 13 ? 'MM yyyy' :
				monthHeader[0].substring(13, monthHeader[0].length - 1));
			monthHeader = (first ? this._generateMonthSelection(
				inst, year, month, minDate, maxDate, monthHeader, calendar, renderer) :
				calendar.formatDate(monthHeader, calendar.newDate(year, month, calendar.minDay)));
			var weekHeader = this._prepare(renderer.weekHeader, inst).
				replace(/\{days\}/g, this._generateDayHeaders(inst, calendar, renderer));
			return this._prepare(renderer.month, inst).replace(/\{monthHeader(:[^\}]+)?\}/g, monthHeader).
				replace(/\{weekHeader\}/g, weekHeader).replace(/\{weeks\}/g, weeks);
		},

		/** Generate the HTML for the day headers.
			@memberof CalendarsPicker
			@private
			@param inst {object} The current instance settings.
			@param calendar {BaseCalendar} The current calendar.
			@param renderer {object} The rendering templates.
			@return {string} A week's worth of day headers. */
		_generateDayHeaders: function(inst, calendar, renderer) {
			var firstDay = inst.options.firstDay;
			firstDay = (firstDay == null ? calendar.local.firstDay : firstDay);
			var header = '';
			for (var day = 0; day < calendar.daysInWeek(); day++) {
				var dow = (day + firstDay) % calendar.daysInWeek();
				header += this._prepare(renderer.dayHeader, inst).replace(/\{day\}/g,
					'<span class="' + this._curDoWClass + dow + '" title="' +
					calendar.local.dayNames[dow] + '">' + calendar.local.dayNamesMin[dow] + '</span>');
			}
			return header;
		},

		/** Generate selection controls for month.
			@memberof CalendarsPicker
			@private
			@param inst {object} The current instance settings.
			@param year {number} The year to generate.
			@param month {number} The month to generate.
			@param minDate {CDate} The minimum date allowed.
			@param maxDate {CDate} The maximum date allowed.
			@param monthHeader {string} The month/year format.
			@param calendar {BaseCalendar} The current calendar.
			@return {string} The month selection content. */
		_generateMonthSelection: function(inst, year, month, minDate, maxDate, monthHeader, calendar) {
			if (!inst.options.changeMonth) {
				return calendar.formatDate(monthHeader, calendar.newDate(year, month, 1));
			}
			// Months
			var monthNames = calendar.local[
				'monthNames' + (monthHeader.match(/mm/i) ? '' : 'Short')];
			var html = monthHeader.replace(/m+/i, '\\x2E').replace(/y+/i, '\\x2F');
			var selector = '<select class="' + this._monthYearClass +
				'" title="' + inst.options.monthStatus + '">';
			var maxMonth = calendar.monthsInYear(year) + calendar.minMonth;
			for (var m = calendar.minMonth; m < maxMonth; m++) {
				if ((!minDate || calendar.newDate(year, m,
						calendar.daysInMonth(year, m) - 1 + calendar.minDay).
						compareTo(minDate) !== -1) &&
						(!maxDate || calendar.newDate(year, m, calendar.minDay).
						compareTo(maxDate) !== +1)) {
					selector += '<option value="' + m + '/' + year + '"' +
						(month === m ? ' selected="selected"' : '') + '>' +
						monthNames[m - calendar.minMonth] + '</option>';
				}
			}
			selector += '</select>';
			html = html.replace(/\\x2E/, selector);
			// Years
			var yearRange = inst.options.yearRange;
			if (yearRange === 'any') {
				selector = '<select class="' + this._monthYearClass + ' ' + this._anyYearClass +
					'" title="' + inst.options.yearStatus + '">' +
					'<option>' + year + '</option></select>' +
					'<input class="' + this._monthYearClass + ' ' + this._curMonthClass +
					month + '" value="' + year + '">';
			}
			else {
				yearRange = yearRange.split(':');
				var todayYear = calendar.today().year();
				var start = (yearRange[0].match('c[+-].*') ? year + parseInt(yearRange[0].substring(1), 10) :
					((yearRange[0].match('[+-].*') ? todayYear : 0) + parseInt(yearRange[0], 10)));
				var end = (yearRange[1].match('c[+-].*') ? year + parseInt(yearRange[1].substring(1), 10) :
					((yearRange[1].match('[+-].*') ? todayYear : 0) + parseInt(yearRange[1], 10)));
				selector = '<select class="' + this._monthYearClass +
					'" title="' + inst.options.yearStatus + '">';
				start = calendar.newDate(start + 1, calendar.firstMonth, calendar.minDay).add(-1, 'd');
				end = calendar.newDate(end, calendar.firstMonth, calendar.minDay);
				var addYear = function(y, yDisplay) {
					if (y !== 0 || calendar.hasYearZero) {
						selector += '<option value="' +
							Math.min(month, calendar.monthsInYear(y) - 1 + calendar.minMonth) +
							'/' + y + '"' + (year === y ? ' selected="selected"' : '') + '>' +
							(yDisplay || y) + '</option>';
					}
				};
				if (start.toJD() < end.toJD()) {
					start = (minDate && minDate.compareTo(start) === +1 ? minDate : start).year();
					end = (maxDate && maxDate.compareTo(end) === -1 ? maxDate : end).year();
					var earlierLater = Math.floor((end - start) / 2);
					if (!minDate || minDate.year() < start) {
						addYear(start - earlierLater, inst.options.earlierText);
					}
					for (var y = start; y <= end; y++) {
						addYear(y);
					}
					if (!maxDate || maxDate.year() > end) {
						addYear(end + earlierLater, inst.options.laterText);
					}
				}
				else {
					start = (maxDate && maxDate.compareTo(start) === -1 ? maxDate : start).year();
					end = (minDate && minDate.compareTo(end) === +1 ? minDate : end).year();
					var earlierLater = Math.floor((start - end) / 2);
					if (!maxDate || maxDate.year() > start) {
						addYear(start + earlierLater, inst.options.earlierText);
					}
					for (var y = start; y >= end; y--) {
						addYear(y);
					}
					if (!minDate || minDate.year() < end) {
						addYear(end - earlierLater, inst.options.laterText);
					}
				}
				selector += '</select>';
			}
			html = html.replace(/\\x2F/, selector);
			return html;
		},

		/** Prepare a render template for use.
			Exclude popup/inline sections that are not applicable.
			Localise text of the form: {l10n:name}.
			@memberof CalendarsPicker
			@private
			@param text {string} The text to localise.
			@param inst {object} The current instance settings.
			@return {string} The localised text. */
		_prepare: function(text, inst) {
			var replaceSection = function(type, retain) {
				while (true) {
					var start = text.indexOf('{' + type + ':start}');
					if (start === -1) {
						return;
					}
					var end = text.substring(start).indexOf('{' + type + ':end}');
					if (end > -1) {
						text = text.substring(0, start) +
							(retain ? text.substr(start + type.length + 8, end - type.length - 8) : '') +
							text.substring(start + end + type.length + 6);
					}
				}
			};
			replaceSection('inline', inst.inline);
			replaceSection('popup', !inst.inline);
			var pattern = /\{l10n:([^\}]+)\}/;
			var matches = null;
			while (matches = pattern.exec(text)) {
				text = text.replace(matches[0], inst.options[matches[1]]);
			}
			return text;
		}
	});

	var plugin = $.calendarsPicker; // Singleton instance

	$(function() {
		$(document).on('mousedown.' + pluginName, plugin._checkExternalClick).
			on('resize.' + pluginName, function() { plugin.hide(plugin.curInst); });
	});

})(jQuery);
