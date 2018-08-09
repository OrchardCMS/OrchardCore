/* http://keith-wood.name/calendars.html
   Nepali calendar for jQuery v2.0.1.
   Written by Artur Neumann (ict.projects{at}nepal.inf.org) April 2013.
   Available under the MIT (http://keith-wood.name/licence.html) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

	/** Implementation of the Nepali civil calendar.
		Based on the ideas from 
		<a href="http://codeissue.com/articles/a04e050dea7468f/algorithm-to-convert-english-date-to-nepali-date-using-c-net">http://codeissue.com/articles/a04e050dea7468f/algorithm-to-convert-english-date-to-nepali-date-using-c-net</a>
		and <a href="http://birenj2ee.blogspot.com/2011/04/nepali-calendar-in-java.html">http://birenj2ee.blogspot.com/2011/04/nepali-calendar-in-java.html</a>
		See also <a href="http://en.wikipedia.org/wiki/Nepali_calendar">http://en.wikipedia.org/wiki/Nepali_calendar</a>
		and <a href="https://en.wikipedia.org/wiki/Bikram_Samwat">https://en.wikipedia.org/wiki/Bikram_Samwat</a>.
		@class NepaliCalendar
		@param [language=''] {string} The language code (default English) for localisation. */
	function NepaliCalendar(language) {
		this.local = this.regionalOptions[language || ''] || this.regionalOptions[''];
	}

	NepaliCalendar.prototype = new $.calendars.baseCalendar;

	$.extend(NepaliCalendar.prototype, {
		/** The calendar name.
			@memberof NepaliCalendar */
		name: 'Nepali',
		/** Julian date of start of Nepali epoch: 14 April 57 BCE.
			@memberof NepaliCalendar */
		jdEpoch: 1700709.5,
		/** Days per month in a common year.
			@memberof NepaliCalendar */
		daysPerMonth: [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
		/** <code>true</code> if has a year zero, <code>false</code> if not.
			@memberof NepaliCalendar */
		hasYearZero: false,
		/** The minimum month number.
			@memberof NepaliCalendar */
		minMonth: 1,
		/** The first month in the year.
			@memberof NepaliCalendar */
		firstMonth: 1,
		/** The minimum day number.
			@memberof NepaliCalendar */
		minDay: 1, 
		/** The number of days in the year.
			@memberof NepaliCalendar */
		daysPerYear: 365,

		/** Localisations for the plugin.
			Entries are objects indexed by the language code ('' being the default US/English).
			Each object has the following attributes.
			@memberof NepaliCalendar
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
				name: 'Nepali',
				epochs: ['BBS', 'ABS'],
				monthNames: ['Baisakh', 'Jestha', 'Ashadh', 'Shrawan', 'Bhadra', 'Ashwin',
				'Kartik', 'Mangsir', 'Paush', 'Mangh', 'Falgun', 'Chaitra'],
				monthNamesShort: ['Bai', 'Je', 'As', 'Shra', 'Bha', 'Ash', 'Kar', 'Mang', 'Pau', 'Ma', 'Fal', 'Chai'],
				dayNames: ['Aaitabaar', 'Sombaar', 'Manglbaar', 'Budhabaar', 'Bihibaar', 'Shukrabaar', 'Shanibaar'],
				dayNamesShort: ['Aaita', 'Som', 'Mangl', 'Budha', 'Bihi', 'Shukra', 'Shani'],
				dayNamesMin: ['Aai', 'So', 'Man', 'Bu', 'Bi', 'Shu', 'Sha'],
				dateFormat: 'dd/mm/yyyy',
				firstDay: 1,
				isRTL: false
			}
		},

		/** Determine whether this date is in a leap year.
			@memberof NepaliCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@return {boolean} <code>true</code> if this is a leap year, <code>false</code> if not.
			@throws Error if an invalid year or a different calendar used. */
		leapYear: function(year) {
			return this.daysInYear(year) !== this.daysPerYear;
		},

		/** Determine the week of the year for a date.
			@memberof NepaliCalendar
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
			@memberof NepaliCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@return {number} The number of days.
			@throws Error if an invalid year or a different calendar used. */
		daysInYear: function(year) {
			var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
			year = date.year();
			if (typeof this.NEPALI_CALENDAR_DATA[year] === 'undefined') {
				return this.daysPerYear;
			}
			var daysPerYear = 0;
			for (var month_number = this.minMonth; month_number <= 12; month_number++) {
				daysPerYear += this.NEPALI_CALENDAR_DATA[year][month_number];
			}
			return daysPerYear;
		},

		/** Retrieve the number of days in a month.
			@memberof NepaliCalendar
			@param year {CDate|number| The date to examine or the year of the month.
			@param [month] {number} The month.
			@return {number} The number of days in this month.
			@throws Error if an invalid month/year or a different calendar used. */
		daysInMonth: function(year, month) {
			if (year.year) {
				month = year.month();
				year = year.year();
			}
			this._validate(year, month, this.minDay, $.calendars.local.invalidMonth);
			return (typeof this.NEPALI_CALENDAR_DATA[year] === 'undefined' ?
				this.daysPerMonth[month - 1] : this.NEPALI_CALENDAR_DATA[year][month]);
		},

		/** Determine whether this date is a week day.
			@memberof NepaliCalendar
			@param year {CDate|number} The date to examine or the year to examine.
			@param [month] {number} The month to examine.
			@param [day] {number} The day to examine.
			@return {boolean} <code>true</code> if a week day, <code>false</code> if not.
			@throws Error if an invalid date or a different calendar used. */
		weekDay: function(year, month, day) {
			return this.dayOfWeek(year, month, day) !== 6;
		},

		/** Retrieve the Julian date equivalent for this date,
			i.e. days since January 1, 4713 BCE Greenwich noon.
			@memberof NepaliCalendar
			@param year {CDate|number} The date to convert or the year to convert.
			@param [month] {number} The month to convert.
			@param [day] {number} The day to convert.
			@return {number} The equivalent Julian date.
			@throws Error if an invalid date or a different calendar used. */
		toJD: function(nepaliYear, nepaliMonth, nepaliDay) {
			var date = this._validate(nepaliYear, nepaliMonth, nepaliDay, $.calendars.local.invalidDate);
			nepaliYear = date.year();
			nepaliMonth = date.month();
			nepaliDay = date.day();
			var gregorianCalendar = $.calendars.instance();
			var gregorianDayOfYear = 0; // We will add all the days that went by since
			// the 1st. January and then we can get the Gregorian Date
			var nepaliMonthToCheck = nepaliMonth;
			var nepaliYearToCheck = nepaliYear;
			this._createMissingCalendarData(nepaliYear);
			// Get the correct year
			var gregorianYear = nepaliYear - (nepaliMonthToCheck > 9 || (nepaliMonthToCheck === 9 &&
				nepaliDay >= this.NEPALI_CALENDAR_DATA[nepaliYearToCheck][0]) ? 56 : 57);
			// First we add the amount of days in the actual Nepali month as the day of year in the
			// Gregorian one because at least this days are gone since the 1st. Jan. 
			if (nepaliMonth !== 9) {
				gregorianDayOfYear = nepaliDay;
				nepaliMonthToCheck--;
			}
			// Now we loop throw all Nepali month and add the amount of days to gregorianDayOfYear 
			// we do this till we reach Paush (9th month). 1st. January always falls in this month  
			while (nepaliMonthToCheck !== 9) {
				if (nepaliMonthToCheck <= 0) {
					nepaliMonthToCheck = 12;
					nepaliYearToCheck--;
				}				
				gregorianDayOfYear += this.NEPALI_CALENDAR_DATA[nepaliYearToCheck][nepaliMonthToCheck];
				nepaliMonthToCheck--;
			}		
			// If the date that has to be converted is in Paush (month no. 9) we have to do some other calculation
			if (nepaliMonth === 9) {
				// Add the days that are passed since the first day of Paush and substract the
				// amount of days that lie between 1st. Jan and 1st Paush
				gregorianDayOfYear += nepaliDay - this.NEPALI_CALENDAR_DATA[nepaliYearToCheck][0];
				// For the first days of Paush we are now in negative values,
				// because in the end of the gregorian year we substract
				// 365 / 366 days (P.S. remember math in school + - gives -)
				if (gregorianDayOfYear <= 0) {
					gregorianDayOfYear += (gregorianCalendar.leapYear(gregorianYear) ? 366 : 365);
				}
			}
			else {
				gregorianDayOfYear += this.NEPALI_CALENDAR_DATA[nepaliYearToCheck][9] -
					this.NEPALI_CALENDAR_DATA[nepaliYearToCheck][0];
			}		
			return gregorianCalendar.newDate(gregorianYear, 1 ,1).add(gregorianDayOfYear, 'd').toJD();
		},
		
		/** Create a new date from a Julian date.
			@memberof NepaliCalendar
			@param jd {number} The Julian date to convert.
			@return {CDate} The equivalent date. */
		fromJD: function(jd) {
			var gregorianCalendar =  $.calendars.instance();
			var gregorianDate = gregorianCalendar.fromJD(jd);
			var gregorianYear = gregorianDate.year();
			var gregorianDayOfYear = gregorianDate.dayOfYear();
			var nepaliYear = gregorianYear + 56; //this is not final, it could be also +57 but +56 is always true for 1st Jan.
			this._createMissingCalendarData(nepaliYear);
			var nepaliMonth = 9; // Jan 1 always fall in Nepali month Paush which is the 9th month of Nepali calendar.
			// Get the Nepali day in Paush (month 9) of 1st January 
			var dayOfFirstJanInPaush = this.NEPALI_CALENDAR_DATA[nepaliYear][0];
			// Check how many days are left of Paush .
			// Days calculated from 1st Jan till the end of the actual Nepali month, 
			// we use this value to check if the gregorian Date is in the actual Nepali month.
			var daysSinceJanFirstToEndOfNepaliMonth =
				this.NEPALI_CALENDAR_DATA[nepaliYear][nepaliMonth] - dayOfFirstJanInPaush + 1;
			// If the gregorian day-of-year is smaller o equal than the sum of days between the 1st January and 
			// the end of the actual nepali month we found the correct nepali month.
			// Example: 
			// The 4th February 2011 is the gregorianDayOfYear 35 (31 days of January + 4)
			// 1st January 2011 is in the nepali year 2067, where 1st. January is in the 17th day of Paush (9th month)
			// In 2067 Paush has 30days, This means (30-17+1=14) there are 14days between 1st January and end of Paush 
			// (including 17th January)
			// The gregorianDayOfYear (35) is bigger than 14, so we check the next month
			// The next nepali month (Mangh) has 29 days 
			// 29+14=43, this is bigger than gregorianDayOfYear(35) so, we found the correct nepali month
			while (gregorianDayOfYear > daysSinceJanFirstToEndOfNepaliMonth) {
				nepaliMonth++;
				if (nepaliMonth > 12) {
					nepaliMonth = 1;
					nepaliYear++;
				}	
				daysSinceJanFirstToEndOfNepaliMonth += this.NEPALI_CALENDAR_DATA[nepaliYear][nepaliMonth];
			}
			// The last step is to calculate the nepali day-of-month
			// to continue our example from before:
			// we calculated there are 43 days from 1st. January (17 Paush) till end of Mangh (29 days)
			// when we subtract from this 43 days the day-of-year of the the Gregorian date (35),
			// we know how far the searched day is away from the end of the Nepali month.
			// So we simply subtract this number from the amount of days in this month (30) 
			var nepaliDayOfMonth = this.NEPALI_CALENDAR_DATA[nepaliYear][nepaliMonth] -
				(daysSinceJanFirstToEndOfNepaliMonth - gregorianDayOfYear);		
			return this.newDate(nepaliYear, nepaliMonth, nepaliDayOfMonth);
		},
		
		/** Creates missing data in the NEPALI_CALENDAR_DATA table.
			This data will not be correct but just give an estimated result. Mostly -/+ 1 day
			@private
			@param nepaliYear {number} The missing year number. */
		_createMissingCalendarData: function(nepaliYear) {
			var tmp_calendar_data = this.daysPerMonth.slice(0);
			tmp_calendar_data.unshift(17);
			for (var nepaliYearToCreate = (nepaliYear - 1); nepaliYearToCreate < (nepaliYear + 2); nepaliYearToCreate++) {
				if (typeof this.NEPALI_CALENDAR_DATA[nepaliYearToCreate] === 'undefined') {
					this.NEPALI_CALENDAR_DATA[nepaliYearToCreate] = tmp_calendar_data;
				}
			}
		},
		
		NEPALI_CALENDAR_DATA:  {
			// These data are from http://www.ashesh.com.np
			1970: [18, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			1971: [18, 31, 31, 32, 31, 32, 30, 30, 29, 30, 29, 30, 30],
			1972: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30],
			1973: [19, 30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
			1974: [19, 31, 31, 32, 30, 31, 31, 30, 29, 30, 29, 30, 30],
			1975: [18, 31, 31, 32, 32, 30, 31, 30, 29, 30, 29, 30, 30],
			1976: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			1977: [18, 31, 32, 31, 32, 31, 31, 29, 30, 29, 30, 29, 31],
			1978: [18, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			1979: [18, 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			1980: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			1981: [18, 31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30],
			1982: [18, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			1983: [18, 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			1984: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			1985: [18, 31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30],
			1986: [18, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			1987: [18, 31, 32, 31, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			1988: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			1989: [18, 31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30],
			1990: [18, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			1991: [18, 31, 32, 31, 32, 31, 30, 30, 29, 30, 29, 30, 30],	
			// These data are from http://nepalicalendar.rat32.com/index.php
			1992: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
			1993: [18, 31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30],
			1994: [18, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			1995: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30],
			1996: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
			1997: [18, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			1998: [18, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			1999: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			2000: [17, 30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
			2001: [18, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2002: [18, 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			2003: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			2004: [17, 30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
			2005: [18, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2006: [18, 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			2007: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			2008: [17, 31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 29, 31],
			2009: [18, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2010: [18, 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			2011: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			2012: [17, 31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30],
			2013: [18, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2014: [18, 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			2015: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			2016: [17, 31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30],
			2017: [18, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2018: [18, 31, 32, 31, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			2019: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
			2020: [17, 31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30],
			2021: [18, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2022: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30],
			2023: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
			2024: [17, 31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30],
			2025: [18, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2026: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			2027: [17, 30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
			2028: [17, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2029: [18, 31, 31, 32, 31, 32, 30, 30, 29, 30, 29, 30, 30],
			2030: [17, 31, 32, 31, 32, 31, 30, 30, 30, 30, 30, 30, 31],
			2031: [17, 31, 32, 31, 32, 31, 31, 31, 31, 31, 31, 31, 31],
			2032: [17, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32],
			2033: [18, 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			2034: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			2035: [17, 30, 32, 31, 32, 31, 31, 29, 30, 30, 29, 29, 31],
			2036: [17, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2037: [18, 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			2038: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			2039: [17, 31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30],
			2040: [17, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2041: [18, 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			2042: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			2043: [17, 31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30],
			2044: [17, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2045: [18, 31, 32, 31, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			2046: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			2047: [17, 31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30],
			2048: [17, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2049: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30],
			2050: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
			2051: [17, 31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30],
			2052: [17, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2053: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30],
			2054: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
			2055: [17, 31, 31, 32, 31, 31, 31, 30, 29, 30, 30, 29, 30],
			2056: [17, 31, 31, 32, 31, 32, 30, 30, 29, 30, 29, 30, 30],
			2057: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			2058: [17, 30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
			2059: [17, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2060: [17, 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			2061: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			2062: [17, 30, 32, 31, 32, 31, 31, 29, 30, 29, 30, 29, 31],
			2063: [17, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2064: [17, 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			2065: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			2066: [17, 31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 29, 31],
			2067: [17, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2068: [17, 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			2069: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			2070: [17, 31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30],
			2071: [17, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2072: [17, 31, 32, 31, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			2073: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
			2074: [17, 31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30],
			2075: [17, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2076: [16, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30],
			2077: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
			2078: [17, 31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30],
			2079: [17, 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
			2080: [16, 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30],
			// These data are from http://www.ashesh.com.np/nepali-calendar/
			2081: [17, 31, 31, 32, 32, 31, 30, 30, 30, 29, 30, 30, 30],
			2082: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 30, 30],
			2083: [17, 31, 31, 32, 31, 31, 30, 30, 30, 29, 30, 30, 30],
			2084: [17, 31, 31, 32, 31, 31, 30, 30, 30, 29, 30, 30, 30],
			2085: [17, 31, 32, 31, 32, 31, 31, 30, 30, 29, 30, 30, 30],
			2086: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 30, 30],
			2087: [16, 31, 31, 32, 31, 31, 31, 30, 30, 29, 30, 30, 30],
			2088: [16, 30, 31, 32, 32, 30, 31, 30, 30, 29, 30, 30, 30],
			2089: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 30, 30],
			2090: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 30, 30],
			2091: [16, 31, 31, 32, 31, 31, 31, 30, 30, 29, 30, 30, 30],
			2092: [16, 31, 31, 32, 32, 31, 30, 30, 30, 29, 30, 30, 30],
			2093: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 30, 30],
			2094: [17, 31, 31, 32, 31, 31, 30, 30, 30, 29, 30, 30, 30],
			2095: [17, 31, 31, 32, 31, 31, 31, 30, 29, 30, 30, 30, 30],
			2096: [17, 30, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
			2097: [17, 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 30, 30],
			2098: [17, 31, 31, 32, 31, 31, 31, 29, 30, 29, 30, 30, 31],
			2099: [17, 31, 31, 32, 31, 31, 31, 30, 29, 29, 30, 30, 30],
			2100: [17, 31, 32, 31, 32, 30, 31, 30, 29, 30, 29, 30, 30]	
		}
	});	

	// Nepali calendar implementation
	$.calendars.calendars.nepali = NepaliCalendar;

})(jQuery);
