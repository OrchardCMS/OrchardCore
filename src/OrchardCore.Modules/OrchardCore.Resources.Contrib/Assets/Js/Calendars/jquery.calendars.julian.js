/* http://keith-wood.name/calendars.html
   Julian calendar for jQuery v2.0.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) August 2009.
   Available under the MIT (http://keith-wood.name/licence.html) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

	/** Implementation of the Julian calendar.
		Based on code from <a href="http://www.fourmilab.ch/documents/calendar/">http://www.fourmilab.ch/documents/calendar/</a>.
		See also <a href="http://en.wikipedia.org/wiki/Julian_calendar">http://en.wikipedia.org/wiki/Julian_calendar</a>.
		@class JulianCalendar
		@augments BaseCalendar
		@param [language=''] {string} The language code (default English) for localisation. */
	function JulianCalendar(language) {
		this.local = this.regionalOptions[language || ''] || this.regionalOptions[''];
	}

	JulianCalendar.prototype = new $.calendars.baseCalendar;

	$.extend(JulianCalendar.prototype, {
		/** The calendar name.
			@memberof JulianCalendar */
		name: 'Julian',
		/** Julian date of start of Persian epoch: 1 January 0001 AD = 30 December 0001 BCE.
			@memberof JulianCalendar */
		jdEpoch: 1721423.5,
		/** Days per month in a common year.
			@memberof JulianCalendar */
		daysPerMonth: [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31],
		/** <code>true</code> if has a year zero, <code>false</code> if not.
			@memberof JulianCalendar */
		hasYearZero: false,
		/** The minimum month number.
			@memberof JulianCalendar */
		minMonth: 1,
		/** The first month in the year.
			@memberof JulianCalendar */
		firstMonth: 1,
		/** The minimum day number.
			@memberof JulianCalendar */
		minDay: 1,

		/** Localisations for the plugin.
			Entries are objects indexed by the language code ('' being the default US/English).
			Each object has the following attributes.
			@memberof JulianCalendar
			@property name {string} The calendar name.
			@property epochs {string[]} The epoch names.
			@property monthNames {string[]} The long names of the months of the year.
			@property monthNamesShort {string[]} The short names of the months of the year.
			@property dayNames {string[]} The long names of the days of the week.
			@property dayNamesShort {string[]} The short names of the days of the week.
			@property dayNamesMin {string[]} The minimal names of the days of the week.
			@property dateFormat {string} The date format for this calendar.
					See the options on <a href="BaseCalendar.html#formatDate"><code>formatDate</code></a> for details.
			@property firstDay {number} The number of the first day of the week, starting at 0.
			@property isRTL {number} <code>true</code> if this localisation reads right-to-left. */
		regionalOptions: { // Localisations
			'': {
				name: 'Julian',
				epochs: ['BC', 'AD'],
				monthNames: ['January', 'February', 'March', 'April', 'May', 'June',
				'July', 'August', 'September', 'October', 'November', 'December'],
				monthNamesShort: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
				dayNames: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
				dayNamesShort: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
				dayNamesMin: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
				dateFormat: 'mm/dd/yyyy',
				firstDay: 0,
				isRTL: false
			}
		},

		/** Determine whether this date is in a leap year.
			@memberof JulianCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@return {boolean} <code>true</code> if this is a leap year, <code>false</code> if not.
			@throws Error if an invalid year or a different calendar used. */
		leapYear: function(year) {
			var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
			var year = (date.year() < 0 ? date.year() + 1 : date.year()); // No year zero
			return (year % 4) === 0;
		},

		/** Determine the week of the year for a date - ISO 8601.
			@memberof JulianCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@param [month] {number} The month to examine.
			@param [day] {number} The day to examine.
			@return {number} The week of the year.
			@throws Error if an invalid date or a different calendar used. */
		weekOfYear: function(year, month, day) {
			// Find Thursday of this week starting on Monday
			var checkDate = this.newDate(year, month, day);
			checkDate.add(4 - (checkDate.dayOfWeek() || 7), 'd');
			return Math.floor((checkDate.dayOfYear() - 1) / 7) + 1;
		},

		/** Retrieve the number of days in a month.
			@memberof JulianCalendar
			@param year {CDate|number} The date to examine or the year of the month.
			@param [month] {number} The month.
			@return {number} The number of days in this month.
			@throws Error if an invalid month/year or a different calendar used. */
		daysInMonth: function(year, month) {
			var date = this._validate(year, month, this.minDay, $.calendars.local.invalidMonth);
			return this.daysPerMonth[date.month() - 1] +
				(date.month() === 2 && this.leapYear(date.year()) ? 1 : 0);
		},

		/** Determine whether this date is a week day.
			@memberof JulianCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@param [month] {number} The month to examine.
			@param [day] {number} The day to examine.
			@return {boolean} True if a week day, false if not.
			@throws Error if an invalid date or a different calendar used. */
		weekDay: function(year, month, day) {
			return (this.dayOfWeek(year, month, day) || 7) < 6;
		},

		/** Retrieve the Julian date equivalent for this date,
			i.e. days since January 1, 4713 BCE Greenwich noon.
			@memberof JulianCalendar
			@param year {CDate|number} The date to convert or the year to convert.
			@param [month] {number} The month to convert.
			@param [day] {number} The day to convert.
			@return {number} The equivalent Julian date.
			@throws Error if an invalid date or a different calendar used. */
		toJD: function(year, month, day) {
			var date = this._validate(year, month, day, $.calendars.local.invalidDate);
			year = date.year();
			month = date.month();
			day = date.day();
			if (year < 0) { year++; } // No year zero
			// Jean Meeus algorithm, "Astronomical Algorithms", 1991
			if (month <= 2) {
				year--;
				month += 12;
			}
			return Math.floor(365.25 * (year + 4716)) +
				Math.floor(30.6001 * (month + 1)) + day - 1524.5;
		},

		/** Create a new date from a Julian date.
			@memberof JulianCalendar
			@param jd {number} The Julian date to convert.
			@return {CDate} The equivalent date. */
		fromJD: function(jd) {
			// Jean Meeus algorithm, "Astronomical Algorithms", 1991
			var a = Math.floor(jd + 0.5);
			var b = a + 1524;
			var c = Math.floor((b - 122.1) / 365.25);
			var d = Math.floor(365.25 * c);
			var e = Math.floor((b - d) / 30.6001);
			var month = e - Math.floor(e < 14 ? 1 : 13);
			var year = c - Math.floor(month > 2 ? 4716 : 4715);
			var day = b - d - Math.floor(30.6001 * e);
			if (year <= 0) { year--; } // No year zero
			return this.newDate(year, month, day);
		}
	});

	// Julian calendar implementation
	$.calendars.calendars.julian = JulianCalendar;

})(jQuery);