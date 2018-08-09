/* http://keith-wood.name/calendars.html
   Persian calendar for jQuery v2.0.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) August 2009.
   Available under the MIT (http://keith-wood.name/licence.html) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

	/** Implementation of the Persian or Jalali calendar.
		Based on code from <a href="http://www.iranchamber.com/calendar/converter/iranian_calendar_converter.php">http://www.iranchamber.com/calendar/converter/iranian_calendar_converter.php</a>.
		See also <a href="http://en.wikipedia.org/wiki/Iranian_calendar">http://en.wikipedia.org/wiki/Iranian_calendar</a>.
		@class PersianCalendar
		@param [language=''] {string} The language code (default English) for localisation. */
	function PersianCalendar(language) {
		this.local = this.regionalOptions[language || ''] || this.regionalOptions[''];
	}

	PersianCalendar.prototype = new $.calendars.baseCalendar;

	$.extend(PersianCalendar.prototype, {
		/** The calendar name.
			@memberof PersianCalendar */
		name: 'Persian',
		/** Julian date of start of Persian epoch: 19 March 622 CE.
			@memberof PersianCalendar */
		jdEpoch: 1948320.5,
		/** Days per month in a common year.
			@memberof PersianCalendar */
		daysPerMonth: [31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 29],
		/** <code>true</code> if has a year zero, <code>false</code> if not.
			@memberof PersianCalendar */
		hasYearZero: false,
		/** The minimum month number.
			@memberof PersianCalendar */
		minMonth: 1,
		/** The first month in the year.
			@memberof PersianCalendar */
		firstMonth: 1,
		/** The minimum day number.
			@memberof PersianCalendar */
		minDay: 1,

		/** Localisations for the plugin.
			Entries are objects indexed by the language code ('' being the default US/English).
			Each object has the following attributes.
			@memberof PersianCalendar
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
				name: 'Persian',
				epochs: ['BP', 'AP'],
				monthNames: ['Farvardin', 'Ordibehesht', 'Khordad', 'Tir', 'Mordad', 'Shahrivar',
				'Mehr', 'Aban', 'Azar', 'Day', 'Bahman', 'Esfand'],
				monthNamesShort: ['Far', 'Ord', 'Kho', 'Tir', 'Mor', 'Sha', 'Meh', 'Aba', 'Aza', 'Day', 'Bah', 'Esf'],
				dayNames: ['Yekshambe', 'Doshambe', 'Seshambe', 'Chæharshambe', 'Panjshambe', 'Jom\'e', 'Shambe'],
				dayNamesShort: ['Yek', 'Do', 'Se', 'Chæ', 'Panj', 'Jom', 'Sha'],
				dayNamesMin: ['Ye','Do','Se','Ch','Pa','Jo','Sh'],
				dateFormat: 'yyyy/mm/dd',
				firstDay: 6,
				isRTL: false
			}
		},

		/** Determine whether this date is in a leap year.
			@memberof PersianCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@return {boolean} <code>true</code> if this is a leap year, <code>false</code> if not.
			@throws Error if an invalid year or a different calendar used. */
		leapYear: function(year) {
			var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
			return (((((date.year() - (date.year() > 0 ? 474 : 473)) % 2820) +
				474 + 38) * 682) % 2816) < 682;
		},

		/** Determine the week of the year for a date.
			@memberof PersianCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@param [month] {number} The month to examine.
			@param [day] {number} The day to examine.
			@return {number} The week of the year.
			@throws Error if an invalid date or a different calendar used. */
		weekOfYear: function(year, month, day) {
			// Find Saturday of this week starting on Saturday
			var checkDate = this.newDate(year, month, day);
			checkDate.add(-((checkDate.dayOfWeek() + 1) % 7), 'd');
			return Math.floor((checkDate.dayOfYear() - 1) / 7) + 1;
		},

		/** Retrieve the number of days in a month.
			@memberof PersianCalendar
			@param year {CDate|number} The date to examine or the year of the month.
			@param [month] {number} The month.
			@return {number} The number of days in this month.
			@throws Error if an invalid month/year or a different calendar used. */
		daysInMonth: function(year, month) {
			var date = this._validate(year, month, this.minDay, $.calendars.local.invalidMonth);
			return this.daysPerMonth[date.month() - 1] +
				(date.month() === 12 && this.leapYear(date.year()) ? 1 : 0);
		},

		/** Determine whether this date is a week day.
			@memberof PersianCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@param [month] {number} The month to examine.
			@param [day] {number} The day to examine.
			@return {boolean} <code>true</code> if a week day, <code>false</code> if not.
			@throws Error if an invalid date or a different calendar used. */
		weekDay: function(year, month, day) {
			return this.dayOfWeek(year, month, day) !== 5;
		},

		/** Retrieve the Julian date equivalent for this date,
			i.e. days since January 1, 4713 BCE Greenwich noon.
			@memberof PersianCalendar
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
			var epBase = year - (year >= 0 ? 474 : 473);
			var epYear = 474 + mod(epBase, 2820);
			return day + (month <= 7 ? (month - 1) * 31 : (month - 1) * 30 + 6) +
				Math.floor((epYear * 682 - 110) / 2816) + (epYear - 1) * 365 +
				Math.floor(epBase / 2820) * 1029983 + this.jdEpoch - 1;
		},

		/** Create a new date from a Julian date.
			@memberof PersianCalendar
			@param jd {number} The Julian date to convert.
			@return {CDate} The equivalent date. */
		fromJD: function(jd) {
			jd = Math.floor(jd) + 0.5;
			var depoch = jd - this.toJD(475, 1, 1);
			var cycle = Math.floor(depoch / 1029983);
			var cyear = mod(depoch, 1029983);
			var ycycle = 2820;
			if (cyear !== 1029982) {
				var aux1 = Math.floor(cyear / 366);
				var aux2 = mod(cyear, 366);
				ycycle = Math.floor(((2134 * aux1) + (2816 * aux2) + 2815) / 1028522) + aux1 + 1;
			}
			var year = ycycle + (2820 * cycle) + 474;
			year = (year <= 0 ? year - 1 : year);
			var yday = jd - this.toJD(year, 1, 1) + 1;
			var month = (yday <= 186 ? Math.ceil(yday / 31) : Math.ceil((yday - 6) / 30));
			var day = jd - this.toJD(year, month, 1) + 1;
			return this.newDate(year, month, day);
		}
	});

	// Modulus function which works for non-integers.
	function mod(a, b) {
		return a - (b * Math.floor(a / b));
	}

	// Persian (Jalali) calendar implementation
	$.calendars.calendars.persian = PersianCalendar;
	$.calendars.calendars.jalali = PersianCalendar;

})(jQuery);