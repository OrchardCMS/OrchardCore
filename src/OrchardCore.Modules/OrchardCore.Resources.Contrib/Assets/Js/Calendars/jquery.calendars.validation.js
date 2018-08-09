/* http://keith-wood.name/calendars.html
   Calendars Validation extension for jQuery 2.0.1.
   Requires Jörn Zaefferer's Validation plugin (http://plugins.jquery.com/project/validate).
   Written by Keith Wood (kbwood{at}iinet.com.au).
   Available under the MIT (http://keith-wood.name/licence.html) license. 
   Please attribute the author if you use it. */

(function($) { // Hide the namespace

	/* Add validation methods if validation plugin available. */
	if ($.fn.validate) {

		$.calendarsPicker.selectDateOrig = $.calendarsPicker.selectDate;
		
		$.extend($.calendarsPicker.regionalOptions[''], {
			validateDate: 'Please enter a valid date',
			validateDateMin: 'Please enter a date on or after {0}',
			validateDateMax: 'Please enter a date on or before {0}',
			validateDateMinMax: 'Please enter a date between {0} and {1}',
			validateDateCompare: 'Please enter a date {0} {1}',
			validateDateToday: 'today',
			validateDateOther: 'the other date',
			validateDateEQ: 'equal to',
			validateDateNE: 'not equal to',
			validateDateLT: 'before',
			validateDateGT: 'after',
			validateDateLE: 'not after',
			validateDateGE: 'not before'
		});
		
		$.extend($.calendarsPicker.defaultOptions, $.calendarsPicker.regionalOptions['']);

		$.extend($.calendarsPicker, {

			/** Trigger a validation after updating the input field with the selected date.
				@param elem {Element} The control to examine.
				@param target {Element} The selected datepicker element. */
			selectDate: function(elem, target) {
				this.selectDateOrig(elem, target);
				var inst = $.calendarsPicker._getInst(elem);
				if (!inst.inline && $.fn.validate) {
					var validation = $(elem).parents('form').validate();
					if (validation) {
						validation.element('#' + elem.id);
					}
				}
			},

			/** Correct error placement for validation errors - after any trigger.
				@param error {jQuery} The error message.
				@param elem {jQuery} The field in error. */
			errorPlacement: function(error, elem) {
				var inst = $.calendarsPicker._getInst(elem);
				if (inst) {
					error[inst.options.isRTL ? 'insertBefore' : 'insertAfter'](
						inst.trigger.length > 0 ? inst.trigger : elem);
				}
				else {
					error.insertAfter(elem);
				}
			},

			/** Format a validation error message involving dates.
				@param source {string} The error message.
				@param params {Date[]} The dates.
				@return {string} The formatted message. */
			errorFormat: function(source, params) {
				var format = ($.calendarsPicker.curInst ?
					$.calendarsPicker.curInst.get('dateFormat') :
					$.calendarsPicker.defaultOptions.dateFormat);
				$.each(params, function(index, value) {
					source = source.replace(new RegExp('\\{' + index + '\\}', 'g'),
						value.formatDate(format) || 'nothing');
				});
				return source;
			}
		});

		var lastElem = null;

		/* Validate date field. */
		$.validator.addMethod('cpDate', function(value, elem) {
				lastElem = elem;
				return this.optional(elem) || validateEach(value, elem);
			},
			function(params) {
				var inst = $.calendarsPicker._getInst(lastElem);
				var minDate = inst.get('minDate');
				var maxDate = inst.get('maxDate');
				var messages = $.calendarsPicker.defaultOptions;
				return (minDate && maxDate ?
					$.calendarsPicker.errorFormat(messages.validateDateMinMax, [minDate, maxDate]) :
					(minDate ? $.calendarsPicker.errorFormat(messages.validateDateMin, [minDate]) :
					(maxDate ? $.calendarsPicker.errorFormat(messages.validateDateMax, [maxDate]) :
					messages.validateDate)));
			});

		/** Apply a validation test to each date provided.
			@private
			@param value {string} The current field value.
			@param elem {Element} The field control.
			@return {boolean} <code>true</code> if OK, <code>false</code> if failed validation. */
		function validateEach(value, elem) {
			var inst = $.calendarsPicker._getInst(elem);
			var dates = (inst.options.multiSelect ? value.split(inst.options.multiSeparator) :
				(inst.options.rangeSelect ? value.split(inst.options.rangeSeparator) : [value]));
			var ok = (inst.options.multiSelect && dates.length <= inst.options.multiSelect) ||
				(!inst.options.multiSelect && inst.options.rangeSelect && dates.length === 2) ||
				(!inst.options.multiSelect && !inst.options.rangeSelect && dates.length === 1);
			if (ok) {
				try {
					var dateFormat = inst.get('dateFormat');
					var minDate = inst.get('minDate');
					var maxDate = inst.get('maxDate');
					var cp = $(elem);
					$.each(dates, function(i, v) {
						dates[i] = inst.options.calendar.parseDate(dateFormat, v);
						ok = ok && (!dates[i] || (cp.calendarsPicker('isSelectable', dates[i]) &&
							(!minDate || dates[i].compareTo(minDate) !== -1) &&
							(!maxDate || dates[i].compareTo(maxDate) !== +1)));
					});
				}
				catch (e) {
					ok = false;
				}
			}
			if (ok && inst.options.rangeSelect) {
				ok = (dates[0].compareTo(dates[1]) !== +1);
			}
			return ok;
		}

		/* And allow as a class rule. */
		$.validator.addClassRules('cpDate', {cpDate: true});

		var comparisons = {equal: 'eq', same: 'eq', notEqual: 'ne', notSame: 'ne',
			lessThan: 'lt', before: 'lt', greaterThan: 'gt', after: 'gt',
			notLessThan: 'ge', notBefore: 'ge', notGreaterThan: 'le', notAfter: 'le'};

		/** Cross-validate date fields.
			params should be an array with [0] comparison type eq/ne/lt/gt/le/ge or synonyms,
			[1] 'today' or date string or CDate or other field selector/element/jQuery OR
			an object with one attribute with name eq/ne/lt/gt/le/ge or synonyms
			and value 'today' or date string or CDate or other field selector/element/jQuery OR
			a string with eq/ne/lt/gt/le/ge or synonyms followed by 'today' or date string or jQuery selector. */
		$.validator.addMethod('cpCompareDate', function(value, elem, params) {
				if (this.optional(elem)) {
					return true;
				}
				params = normaliseParams(params);
				var thisDate = $(elem).calendarsPicker('getDate');
				var thatDate = extractOtherDate(elem, params[1]);
				if (thisDate.length === 0 || thatDate.length === 0) {
					return true;
				}
				lastElem = elem;
				var finalResult = true;
				for (var i = 0; i < thisDate.length; i++) {
					var result = thisDate[i].compareTo(thatDate[0]);
					switch (comparisons[params[0]] || params[0]) {
						case 'eq': finalResult = (result === 0); break;
						case 'ne': finalResult = (result !== 0); break;
						case 'lt': finalResult = (result < 0); break;
						case 'gt': finalResult = (result > 0); break;
						case 'le': finalResult = (result <= 0); break;
						case 'ge': finalResult = (result >= 0); break;
						default:   finalResult = true;
					}
					if (!finalResult) {
						break;
					}
				}
				return finalResult;
			},
			function(params) {
				var messages = $.calendarsPicker.defaultOptions;
				params = normaliseParams(params);
				var thatDate = extractOtherDate(lastElem, params[1], true);
				thatDate = (params[1] === 'today' ? messages.validateDateToday : 
					(thatDate.length ? thatDate[0].formatDate() : messages.validateDateOther));
				return messages.validateDateCompare.replace(/\{0\}/,
					messages['validateDate' + (comparisons[params[0]] || params[0]).toUpperCase()]).
					replace(/\{1\}/, thatDate);
			});

		/** Normalise the comparison parameters to an array.
			@param params {Array|object|string} The original parameters.
			@return {Array} The normalised parameters. */
		function normaliseParams(params) {
			if (typeof params === 'string') {
				params = params.split(' ');
			}
			else if (!$.isArray(params)) {
				var opts = [];
				for (var name in params) {
					opts[0] = name;
					opts[1] = params[name];
				}
				params = opts;
			}
			return params;
		}

		/** Determine the comparison date.
			@param elem {Element} The current datepicker element.
			@param source {string|CDate|jQueryElement} The source of the other date.
			@param noOther {boolean} <code>true</code> to not get the date from another field.
			@return {CDate[]} The date for comparison. */
		function extractOtherDate(elem, source, noOther) {
			if (source.newDate && source.extraInfo) { // Already a CDate
				return [source];
			}
			var inst = $.calendarsPicker._getInst(elem);
			var thatDate = null;
			try {
				if (typeof source === 'string' && source !== 'today') {
					thatDate = inst.options.calendar.parseDate(inst.get('dateFormat'), source);
				}
			}
			catch (e) {
				// Ignore
			}
			thatDate = (thatDate ? [thatDate] : (source === 'today' ?
				[inst.options.calendar.today()] : (noOther ? [] : $(source).calendarsPicker('getDate'))));
			return thatDate;
		}
	}

})(jQuery);
