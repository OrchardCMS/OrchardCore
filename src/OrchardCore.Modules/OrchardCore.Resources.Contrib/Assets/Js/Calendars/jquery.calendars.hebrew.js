/* http://keith-wood.name/calendars.html
   Hebrew calendar for jQuery v2.0.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) August 2009.
   Available under the MIT (http://keith-wood.name/licence.html) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

	/** Implementation of the Hebrew civil calendar.
		Based on code from <a href="http://www.fourmilab.ch/documents/calendar/">http://www.fourmilab.ch/documents/calendar/</a>.
		See also <a href="http://en.wikipedia.org/wiki/Hebrew_calendar">http://en.wikipedia.org/wiki/Hebrew_calendar</a>.
		@class HebrewCalendar
		@param [language=''] {string} The language code (default English) for localisation. */
	function HebrewCalendar(language) {
		this.local = this.regionalOptions[language || ''] || this.regionalOptions[''];
	}

	HebrewCalendar.prototype = new $.calendars.baseCalendar;

	$.extend(HebrewCalendar.prototype, {
		/** The calendar name.
			@memberof HebrewCalendar */
		name: 'Hebrew',
		/** Julian date of start of Hebrew epoch: 7 October 3761 BCE.
			@memberof HebrewCalendar */
		jdEpoch: 347995.5,
		/** Days per month in a common year.
			@memberof HebrewCalendar */
		daysPerMonth: [30, 29, 30, 29, 30, 29, 30, 29, 30, 29, 30, 29, 29],
		/** <code>true</code> if has a year zero, <code>false</code> if not.
			@memberof HebrewCalendar */
		hasYearZero: false,
		/** The minimum month number.
			@memberof HebrewCalendar */
		minMonth: 1,
		/** The first month in the year.
			@memberof HebrewCalendar */
		firstMonth: 7,
		/** The minimum day number.
			@memberof HebrewCalendar */
		minDay: 1,

		/** Localisations for the plugin.
			Entries are objects indexed by the language code ('' being the default US/English).
			Each object has the following attributes.
			@memberof HebrewCalendar
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
				name: 'Hebrew',
				epochs: ['BAM', 'AM'],
				monthNames: ['Nisan', 'Iyar', 'Sivan', 'Tammuz', 'Av', 'Elul',
				'Tishrei', 'Cheshvan', 'Kislev', 'Tevet', 'Shevat', 'Adar', 'Adar II'],
				monthNamesShort: ['Nis', 'Iya', 'Siv', 'Tam', 'Av', 'Elu', 'Tis', 'Che', 'Kis', 'Tev', 'She', 'Ada', 'Ad2'],
				dayNames: ['Yom Rishon', 'Yom Sheni', 'Yom Shlishi', 'Yom Revi\'i', 'Yom Chamishi', 'Yom Shishi', 'Yom Shabbat'],
				dayNamesShort: ['Ris', 'She', 'Shl', 'Rev', 'Cha', 'Shi', 'Sha'],
				dayNamesMin: ['Ri','She','Shl','Re','Ch','Shi','Sha'],
				dateFormat: 'dd/mm/yyyy',
				firstDay: 0,
				isRTL: false
			}
		},

		/** Determine whether this date is in a leap year.
			@memberof HebrewCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@return {boolean} <code>true</code> if this is a leap year, <code>false</code> if not.
			@throws Error if an invalid year or a different calendar used. */
		leapYear: function(year) {
			var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
			return this._leapYear(date.year());
		},

		/** Determine whether this date is in a leap year.
			@memberof HebrewCalendar
			@private
			@param year {number} The year to examine.
			@return {boolean} <code>true</code> if this is a leap year, <code>false</code> if not.
			@throws Error if an invalid year or a different calendar used. */
		_leapYear: function(year) {
			year = (year < 0 ? year + 1 : year);
			return mod(year * 7 + 1, 19) < 7;
		},

		/** Retrieve the number of months in a year.
			@memberof HebrewCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@return {number} The number of months.
			@throws Error if an invalid year or a different calendar used. */
		monthsInYear: function(year) {
			this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
			return this._leapYear(year.year ? year.year() : year) ? 13 : 12;
		},

		/** Determine the week of the year for a date.
			@memberof HebrewCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@param [month] {number} The month to examine.
			@param [day] {number} The day to examine.
			@return {number} The week of the year.
			@throws Error if an invalid date or a different calendar used. */
		weekOfYear: function(year, month, day) {
			// Find Sunday of this week starting on Sunday
			var checkDate = this.newDate(year, month, day);
			checkDate.add(-checkDate.dayOfWeek(), 'd');
			return Math.floor((checkDate.dayOfYear() - 1) / 7) + 1;
		},

		/** Retrieve the number of days in a year.
			@memberof HebrewCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@return {number} The number of days.
			@throws Error if an invalid year or a different calendar used. */
		daysInYear: function(year) {
			var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
			year = date.year();
			return this.toJD((year === -1 ? +1 : year + 1), 7, 1) - this.toJD(year, 7, 1);
		},

		/** Retrieve the number of days in a month.
			@memberof HebrewCalendar
			@param year {CDate|number} The date to examine or the year of the month.
			@param [month] {number} The month.
			@return {number} The number of days in this month.
			@throws Error if an invalid month/year or a different calendar used. */
		daysInMonth: function(year, month) {
			if (year.year) {
				month = year.month();
				year = year.year();
			}
			this._validate(year, month, this.minDay, $.calendars.local.invalidMonth);
			return (month === 12 && this.leapYear(year) ? 30 : // Adar I
					(month === 8 && mod(this.daysInYear(year), 10) === 5 ? 30 : // Cheshvan in shlemah year
					(month === 9 && mod(this.daysInYear(year), 10) === 3 ? 29 : // Kislev in chaserah year
					this.daysPerMonth[month - 1])));
		},

		/** Determine whether this date is a week day.
			@memberof HebrewCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@param [month] {number} The month to examine.
			@param [day] {number} The day to examine.
			@return {boolean} <code>true</code> if a week day, <code>false</code> if not.
			@throws Error if an invalid date or a different calendar used. */
		weekDay: function(year, month, day) {
			return this.dayOfWeek(year, month, day) !== 6;
		},

		/** Retrieve additional information about a date - year type.
			@memberof HebrewCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@param [month] {number} The month to examine.
			@param [day] {number} The day to examine.
			@return {object} Additional information - contents depends on calendar.
			@throws Error if an invalid date or a different calendar used. */
		extraInfo: function(year, month, day) {
			var date = this._validate(year, month, day, $.calendars.local.invalidDate);
			return {yearType: (this.leapYear(date) ? 'embolismic' : 'common') + ' ' +
				['deficient', 'regular', 'complete'][this.daysInYear(date) % 10 - 3]};
		},

		/** Retrieve the Julian date equivalent for this date,
			i.e. days since January 1, 4713 BCE Greenwich noon.
			@memberof HebrewCalendar
			@param year {CDate)|number} The date to convert or the year to convert.
			@param [month] {number} The month to convert.
			@param [day] {number} The day to convert.
			@return {number} The equivalent Julian date.
			@throws Error if an invalid date or a different calendar used. */
		toJD: function(year, month, day) {
			var date = this._validate(year, month, day, $.calendars.local.invalidDate);
			year = date.year();
			month = date.month();
			day = date.day();
			var adjYear = (year <= 0 ? year + 1 : year);
			var jd = this.jdEpoch + this._delay1(adjYear) +
				this._delay2(adjYear) + day + 1;
			if (month < 7) {
				for (var m = 7; m <= this.monthsInYear(year); m++) {
					jd += this.daysInMonth(year, m);
				}
				for (var m = 1; m < month; m++) {
					jd += this.daysInMonth(year, m);
				}
			}
			else {
				for (var m = 7; m < month; m++) {
					jd += this.daysInMonth(year, m);
				}
			}
			return jd;
		},

		/** Test for delay of start of new year and to avoid
			Sunday, Wednesday, or Friday as start of the new year.
			@memberof HebrewCalendar
			@private
			@param year {number} The year to examine.
			@return {number} The days to offset by. */
		_delay1: function(year) {
			var months = Math.floor((235 * year - 234) / 19);
			var parts = 12084 + 13753 * months;
			var day = months * 29 + Math.floor(parts / 25920);
			if (mod(3 * (day + 1), 7) < 3) {
				day++;
			}
			return day;
		},

		/** Check for delay in start of new year due to length of adjacent years.
			@memberof HebrewCalendar
			@private
			@param year {number} The year to examine.
			@return {number} The days to offset by. */
		_delay2: function(year) {
			var last = this._delay1(year - 1);
			var present = this._delay1(year);
			var next = this._delay1(year + 1);
			return ((next - present) === 356 ? 2 : ((present - last) === 382 ? 1 : 0));
		},

		/** Create a new date from a Julian date.
			@memberof HebrewCalendar
			@param jd {number} The Julian date to convert.
			@return {CDate} The equivalent date. */
		fromJD: function(jd) {
			jd = Math.floor(jd) + 0.5;
			var year = Math.floor(((jd - this.jdEpoch) * 98496.0) / 35975351.0) - 1;
			while (jd >= this.toJD((year === -1 ? +1 : year + 1), 7, 1)) {
				year++;
			}
			var month = (jd < this.toJD(year, 1, 1)) ? 7 : 1;
			while (jd > this.toJD(year, month, this.daysInMonth(year, month))) {
				month++;
			}
			var day = jd - this.toJD(year, month, 1) + 1;
			return this.newDate(year, month, day);
		}
	});

	// Modulus function which works for non-integers.
	function mod(a, b) {
		return a - (b * Math.floor(a / b));
	}

	// Hebrew calendar implementation
	$.calendars.calendars.hebrew = HebrewCalendar;

})(jQuery);