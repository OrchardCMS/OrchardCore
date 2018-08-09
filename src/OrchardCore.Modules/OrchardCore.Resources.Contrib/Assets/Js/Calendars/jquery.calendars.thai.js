/* http://keith-wood.name/calendars.html
   Thai calendar for jQuery v2.0.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) February 2010.
   Available under the MIT (http://keith-wood.name/licence.html) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

	var gregorianCalendar = $.calendars.instance();

	/** Implementation of the Thai calendar.
		See http://en.wikipedia.org/wiki/Thai_calendar.
		@class ThaiCalendar
		@param [language=''] {string} The language code (default English) for localisation. */
	function ThaiCalendar(language) {
		this.local = this.regionalOptions[language || ''] || this.regionalOptions[''];
	}

	ThaiCalendar.prototype = new $.calendars.baseCalendar;

	$.extend(ThaiCalendar.prototype, {
		/** The calendar name.
			@memberof ThaiCalendar */
		name: 'Thai',
		/** Julian date of start of Thai epoch: 1 January 543 BCE (Gregorian).
			@memberof ThaiCalendar */
		jdEpoch: 1523098.5,
		/** Difference in years between Thai and Gregorian calendars.
			@memberof ThaiCalendar */
		yearsOffset: 543, 
		/** Days per month in a common year.
			@memberof ThaiCalendar */
		daysPerMonth: [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31],
		/** <code>true</code> if has a year zero, <code>false</code> if not.
			@memberof ThaiCalendar */
		hasYearZero: false,
		/** The minimum month number.
			@memberof ThaiCalendar */
		minMonth: 1,
		/** The first month in the year.
			@memberof ThaiCalendar */
		firstMonth: 1,
		/** The minimum day number.
			@memberof ThaiCalendar */
		minDay: 1,

		/** Localisations for the plugin.
			Entries are objects indexed by the language code ('' being the default US/English).
			Each object has the following attributes.
			@memberof ThaiCalendar
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
				name: 'Thai',
				epochs: ['BBE', 'BE'],
				monthNames: ['January', 'February', 'March', 'April', 'May', 'June',
				'July', 'August', 'September', 'October', 'November', 'December'],
				monthNamesShort: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
				dayNames: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
				dayNamesShort: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
				dayNamesMin: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
				dateFormat: 'dd/mm/yyyy',
				firstDay: 0,
				isRTL: false
			}
		},

		/** Determine whether this date is in a leap year.
			@memberof ThaiCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@return {boolean} <code>true</code> if this is a leap year, <code>false</code> if not.
			@throws Error if an invalid year or a different calendar used. */
		leapYear: function(year) {
			var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
			var year = this._t2gYear(date.year());
			return gregorianCalendar.leapYear(year);
		},

		/** Determine the week of the year for a date - ISO 8601.
			@memberof ThaiCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@param [month] {number} The month to examine.
			@param [day] {number} The day to examine.
			@return {number} The week of the year.
			@throws Error if an invalid date or a different calendar used. */
		weekOfYear: function(year, month, day) {
			var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
			var year = this._t2gYear(date.year());
			return gregorianCalendar.weekOfYear(year, date.month(), date.day());
		},

		/** Retrieve the number of days in a month.
			@memberof ThaiCalendar
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
			@memberof ThaiCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@param [month] {number} The month to examine.
			@param [day] {number} The day to examine.
			@return {boolean} <code>true</code> if a week day, <code>false</code> if not.
			@throws Error if an invalid date or a different calendar used. */
		weekDay: function(year, month, day) {
			return (this.dayOfWeek(year, month, day) || 7) < 6;
		},

		/** Retrieve the Julian date equivalent for this date,
			i.e. days since January 1, 4713 BCE Greenwich noon.
			@memberof ThaiCalendar
			@param year {CDate|number} The date to convert or the year to convert.
			@param [month] {number} The month to convert.
			@param [day] {number} The day to convert.
			@return {number} The equivalent Julian date.
			@throws Error if an invalid date or a different calendar used. */
		toJD: function(year, month, day) {
			var date = this._validate(year, month, day, $.calendars.local.invalidDate);
			var year = this._t2gYear(date.year());
			return gregorianCalendar.toJD(year, date.month(), date.day());
		},

		/** Create a new date from a Julian date.
			@memberof ThaiCalendar
			@param jd {number} The Julian date to convert.
			@return {CDate} The equivalent date. */
		fromJD: function(jd) {
			var date = gregorianCalendar.fromJD(jd);
			var year = this._g2tYear(date.year());
			return this.newDate(year, date.month(), date.day());
		},

		/** Convert Thai to Gregorian year.
			@memberof ThaiCalendar
			@private
			@param year {number} The Thai year.
			@return {number} The corresponding Gregorian year. */
		_t2gYear: function(year) {
			return year - this.yearsOffset - (year >= 1 && year <= this.yearsOffset ? 1 : 0);
		},

		/** Convert Gregorian to Thai year.
			@memberof ThaiCalendar
			@private
			@param year {number} The Gregorian year.
			@return {number} The corresponding Thai year. */
		_g2tYear: function(year) {
			return year + this.yearsOffset + (year >= -this.yearsOffset && year <= -1 ? 1 : 0);
		}
	});

	// Thai calendar implementation
	$.calendars.calendars.thai = ThaiCalendar;

})(jQuery);