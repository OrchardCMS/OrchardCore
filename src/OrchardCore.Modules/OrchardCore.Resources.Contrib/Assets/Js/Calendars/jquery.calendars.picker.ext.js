/* http://keith-wood.name/calendars.html
   Calendars date picker extensions for jQuery v2.0.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) August 2009.
   Available under the MIT (http://keith-wood.name/licence.html) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

	var themeRollerRenderer = {
		picker: '<div{popup:start} id="ui-datepicker-div"{popup:end} class="ui-datepicker ui-widget ' +
		'ui-widget-content ui-helper-clearfix ui-corner-all{inline:start} ui-datepicker-inline{inline:end}">' +
		'<div class="ui-datepicker-header ui-widget-header ui-helper-clearfix ui-corner-all">' +
		'{link:prev}{link:today}{link:next}</div>{months}' +
		'{popup:start}<div class="ui-datepicker-header ui-widget-header ui-helper-clearfix ' +
		'ui-corner-all">{button:clear}{button:close}</div>{popup:end}' +
		'<div class="ui-helper-clearfix"></div></div>',
		monthRow: '<div class="ui-datepicker-row-break">{months}</div>',
		month: '<div class="ui-datepicker-group">' +
		'<div class="ui-datepicker-header ui-widget-header ui-helper-clearfix ui-corner-all">{monthHeader:MM yyyy}</div>' +
		'<table class="ui-datepicker-calendar"><thead>{weekHeader}</thead><tbody>{weeks}</tbody></table></div>',
		weekHeader: '<tr>{days}</tr>',
		dayHeader: '<th>{day}</th>',
		week: '<tr>{days}</tr>',
		day: '<td>{day}</td>',
		monthSelector: '.ui-datepicker-group',
		daySelector: 'td',
		rtlClass: 'ui-datepicker-rtl',
		multiClass: 'ui-datepicker-multi',
		defaultClass: 'ui-state-default',
		selectedClass: 'ui-state-active',
		highlightedClass: 'ui-state-hover',
		todayClass: 'ui-state-highlight',
		otherMonthClass: 'ui-datepicker-other-month',
		weekendClass: 'ui-datepicker-week-end',
		commandClass: 'ui-datepicker-cmd',
		commandButtonClass: 'ui-state-default ui-corner-all',
		commandLinkClass: '',
		disabledClass: 'ui-datepicker-disabled'
	};

	$.extend($.calendarsPicker, {

		/** Template for generating a calendar picker showing week of year.
			Found in the <code>jquery.calendars.picker.ext.js</code> module.
			@memberof CalendarsPicker */
		weekOfYearRenderer: $.extend({}, $.calendarsPicker.defaultRenderer, {
			weekHeader: '<tr><th class="calendars-week">' +
			'<span title="{l10n:weekStatus}">{l10n:weekText}</span></th>{days}</tr>',
			week: '<tr><td class="calendars-week">{weekOfYear}</td>{days}</tr>'
		}),

		/** ThemeRoller template for generating a calendar picker.
			Found in the <code>jquery.calendars.picker.ext.js</code> module.
			@memberof CalendarsPicker */
		themeRollerRenderer: themeRollerRenderer,

		/** ThemeRoller template for generating a calendar picker showing week of year.
			Found in the <code>jquery.calendars.picker.ext.js</code> module.
			@memberof CalendarsPicker */
		themeRollerWeekOfYearRenderer: $.extend({}, themeRollerRenderer, {
			weekHeader: '<tr><th class="ui-state-hover"><span>{l10n:weekText}</span></th>{days}</tr>',
			week: '<tr><td class="ui-state-hover">{weekOfYear}</td>{days}</tr>'
		}),

		/** Don't allow weekends to be selected.
			Found in the <code>jquery.calendars.picker.ext.js</code> module.
			@memberof CalendarsPicker
			@param date {CDate} The current date.
			@return {object} Information about this date.
			@example onDate: $.calendarsPicker.noWeekends */
		noWeekends: function(date) {
			return {selectable: date.weekDay()};
		},

		/** Change the first day of the week by clicking on the day header.
			Found in the <code>jquery.calendars.picker.ext.js</code> module.
			@memberof CalendarsPicker
			@param picker {jQuery} The completed datepicker division.
			@param calendar {BaseCalendar} The calendar implementation.
			@param inst {object} The current instance settings.
			@example onShow: $.calendarsPicker.changeFirstDay */
		changeFirstDay: function(picker, calendar, inst) {
			var target = $(this);
			picker.find('th span').each(function() {
				if (this.parentNode.className.match(/.*calendars-week.*/)) {
					return;
				}
				$('<a href="javascript:void(0)" class="' + this.className +
						'" title="Change first day of the week">' + $(this).text() + '</a>').
					click(function() {
						var dow = parseInt(this.className.replace(/^.*calendars-dow-(\d+).*$/, '$1'), 10);
						target.calendarsPicker('option', {firstDay: dow});
					}).
					replaceAll(this);
			});
		},

		/** A function to call when a date is hovered.
			@callback CalendarsPickerOnHover
			@param date {CDate} The date being hovered or <code>null</code> on exit.
			@param selectable {boolean} <code>true</code> if this date is selectable, <code>false</code> if not.
			@example function showHovered(date, selectable) {
	$('#feedback').text('You are viewing ' + (date ? date.formatDate() : 'nothing'));
 } */

		/** Add a callback when hovering over dates.
			Found in the <code>jquery.calendars.picker.ext.js</code> module.
			@memberof CalendarsPicker
			@param onHover {CalendarsPickerOnHover} The callback when hovering.
			@example onShow: $.calendarsPicker.hoverCallback(showHovered) */
		hoverCallback: function(onHover) {
			return function(picker, calendar, inst) {
				if ($.isFunction(onHover)) {
					var target = this;
					var renderer = inst.options.renderer;
					picker.find(renderer.daySelector + ' a, ' + renderer.daySelector + ' span').
						hover(function() {
							onHover.apply(target, [$(target).calendarsPicker('retrieveDate', this),
								this.nodeName.toLowerCase() === 'a']);
						},
						function() { onHover.apply(target, []); });
				}
			};
		},

		/** Highlight the entire week when hovering over it.
			Found in the <code>jquery.calendars.picker.ext.js</code> module.
			@memberof CalendarsPicker
			@param picker {jQuery} The completed datepicker division.
			@param calendar {BaseCalendar} The calendar implementation.
			@param inst {object} The current instance settings.
			@example onShow: $.calendarsPicker.highlightWeek */
		highlightWeek: function(picker, calendar, inst) {
			var target = this;
			var renderer = inst.options.renderer;
			picker.find(renderer.daySelector + ' a, ' + renderer.daySelector + ' span').
				hover(function() {
					$(this).parents('tr').find(renderer.daySelector + ' *').
						addClass(renderer.highlightedClass);
				},
				function() {
					$(this).parents('tr').find(renderer.daySelector + ' *').
						removeClass(renderer.highlightedClass);
				});
		},

		/** Show a status bar with messages.
			Found in the <code>jquery.calendars.picker.ext.js</code> module.
			@memberof CalendarsPicker
			@param picker {jQuery} The completed datepicker division.
			@param calendar {BaseCalendar} The calendar implementation.
			@param inst {object} The current instance settings.
			@example onShow: $.calendarsPicker.showStatus */
		showStatus: function(picker, calendar, inst) {
			var isTR = (inst.options.renderer.selectedClass === 'ui-state-active');
			var defaultStatus = inst.options.defaultStatus || '&#160;';
			var status = $('<div class="' + (!isTR ? 'calendars-status' :
				'ui-datepicker-status ui-widget-header ui-helper-clearfix ui-corner-all') + '">' +
				defaultStatus + '</div>').
				insertAfter(picker.find('.calendars-month-row:last,.ui-datepicker-row-break:last'));
			picker.find('*[title]').each(function() {
					var title = $(this).attr('title');
					$(this).removeAttr('title').hover(
						function() { status.text(title || defaultStatus); },
						function() { status.text(defaultStatus); });
				});
		},

		/** Allow easier navigation by month.
			Found in the <code>jquery.calendars.picker.ext.js</code> module.
			@memberof CalendarsPicker
			@param picker {jQuery} The completed datepicker division.
			@param calendar {BaseCalendar} The calendar implementation.
			@param inst {object} The current instance settings.
			@example onShow: $.calendarsPicker.monthNavigation */
		monthNavigation: function(picker, calendar, inst) {
			var target = $(this);
			var isTR = (inst.options.renderer.selectedClass === 'ui-state-active');
			var minDate = inst.curMinDate();
			var maxDate = inst.get('maxDate');
			var year = inst.drawDate.year();
			var html = '<div class="' + (!isTR ? 'calendars-month-nav' : 'ui-datepicker-month-nav') + '">';
			for (var i = 0; i < calendar.monthsInYear(year); i++) {
				var ord = calendar.fromMonthOfYear(year, i + calendar.minMonth) - calendar.minMonth;
				var inRange = ((!minDate || calendar.newDate(year, i + calendar.minMonth,
					calendar.daysInMonth(year, i + calendar.minMonth)).compareTo(minDate) > -1) && (!maxDate ||
					calendar.newDate(year, i + calendar.minMonth, calendar.minDay).compareTo(maxDate) < +1));
				html += '<div>' + (inRange ? '<a href="#" class="jd' +
					calendar.newDate(year, i + calendar.minMonth, calendar.minDay).toJD() + '"' : '<span') +
					' title="' + calendar.local.monthNames[ord] + '">' + calendar.local.monthNamesShort[ord] +
					(inRange ? '</a>' : '</span>') + '</div>';
			}
			html += '</div>';
			$(html).insertAfter(picker.find('div.calendars-nav,div.ui-datepicker-header:first')).
				find('a').click(function() {
					var date = target.calendarsPicker('retrieveDate', this);
					target.calendarsPicker('showMonth', date.year(), date.month());
					return false;
				});
		},

		/** Select an entire week when clicking on a week number.
			Use in conjunction with <code>weekOfYearRenderer</code>.
			Found in the <code>jquery.calendars.picker.ext.js</code> module.
			@memberof CalendarsPicker
			@param picker {jQuery} The completed datepicker division.
			@param calendar {BaseCalendar} The calendar implementation.
			@param inst {object} The current instance settings.
			@example onShow: $.calendarsPicker.selectWeek */
		selectWeek: function(picker, calendar, inst) {
			var target = $(this);
			picker.find('td.calendars-week span').each(function() {
				$('<a href="javascript:void(0)" class="' +
						this.className + '" title="Select the entire week">' +
						$(this).text() + '</a>').
					click(function() {
						var date = target.calendarsPicker('retrieveDate', this);
						var dates = [date];
						for (var i = 1; i < calendar.daysInWeek(); i++) {
							dates.push(date = date.newDate().add(1, 'd'));
						}
						if (inst.options.rangeSelect) {
							dates.splice(1, dates.length - 2);
						}
						target.calendarsPicker('setDate', dates).calendarsPicker('hide');
					}).
					replaceAll(this);
			});
		},

		/** Select an entire month when clicking on the week header.
			Use in conjunction with <code>weekOfYearRenderer</code>.
			Found in the <code>jquery.calendars.picker.ext.js</code> module.
			@memberof CalendarsPicker
			@param picker {jQuery} The completed datepicker division.
			@param calendar {BaseCalendar} The calendar implementation.
			@param inst {object} The current instance settings.
			@example onShow: $.calendarsPicker.selectMonth */
		selectMonth: function(picker, calendar, inst) {
			var target = $(this);
			picker.find('th.calendars-week').each(function() {
				$('<a href="javascript:void(0)" title="Select the entire month">' +
						$(this).text() + '</a>').
					click(function() {
						var date = target.calendarsPicker('retrieveDate', $(this).parents('table').
							find('td:not(.calendars-week) *:not(.calendars-other-month)')[0]);
						var dates = [date.day(1)];
						var dim = calendar.daysInMonth(date);
						for (var i = 1; i < dim; i++) {
							dates.push(date = date.newDate().add(1, 'd'));
						}
						if (inst.options.rangeSelect) {
							dates.splice(1, dates.length - 2);
						}
						target.calendarsPicker('setDate', dates).calendarsPicker('hide');
					}).
					appendTo(this);
			});
		},

		/** Select a month only instead of a single day.
			Found in the <code>jquery.calendars.picker.ext.js</code> module.
			@memberof CalendarsPicker
			@param picker {jQuery} The completed datepicker division.
			@param calendar {BaseCalendar} The calendar implementation.
			@param inst {object} The current instance settings.
			@example onShow: $.calendarsPicker.monthOnly */
		monthOnly: function(picker, calendar, inst) {
			var target = $(this);
			var selectMonth = $('<div style="text-align: center;"><button type="button">Select</button></div>').
				insertAfter(picker.find('.calendars-month-row:last,.ui-datepicker-row-break:last')).
				children().click(function() {
					var monthYear = picker.find('.calendars-month-year:first').val().split('/');
					target.calendarsPicker('setDate', calendar.newDate(
						parseInt(monthYear[1], 10), parseInt(monthYear[0], 10), calendar.minDay)).
						calendarsPicker('hide');
				});
			picker.find('.calendars-month-row table,.ui-datepicker-row-break table').remove();
		}
	});

})(jQuery);
