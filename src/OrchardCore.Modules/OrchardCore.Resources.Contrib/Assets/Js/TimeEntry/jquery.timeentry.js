/* http://keith-wood.name/timeEntry.html
   Time entry for jQuery v2.0.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) June 2007.
   Available under the MIT (https://github.com/jquery/jquery/blob/master/MIT-LICENSE.txt) license.
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

	var pluginName = 'timeEntry';

	/** Create the time entry plugin.
		<p>Sets an input field to add a spinner for time entry.</p>
		<p>The time can be entered via directly typing the value,
		via the arrow keys, or via spinner buttons.
		It is configurable to show 12 or 24-hour time, to show or hide seconds,
		to enforce a minimum and/or maximum time, to change the spinner image,
		and to constrain the time to steps, e.g. only on the quarter hours.</p>
		<p>Expects HTML like:</p>
		<pre>&lt;input type="text"></pre>
		<p>Provide inline configuration like:</p>
		<pre>&lt;input type="text" data-timeEntry="name: 'value'"></pre>
	 	@module TimeEntry
		@augments JQPlugin
		@example $(selector).timeEntry()
 $(selector).timeEntry({showSeconds: true, minTime: new Date(0, 0, 0, 12, 0, 0)}) */
	$.JQPlugin.createPlugin({
	
		/** The name of the plugin. */
		name: pluginName,
			
		/** Time entry before show callback.
			Triggered when the input field is focussed.
			@callback beforeShowCallback
			@param input {Element} The current input field.
			@return {object} Any changes to the instance settings.
			@example beforeShow: function(input) {
	// Cross-populate minimum/maximum times for a range
 	return {minTime: (input.id === 'timeTo' ?
 		$('#timeFrom').timeEntry('getTime') : null), 
 		maxTime: (input.id === 'timeFrom' ?
 		$('#timeTo').timeEntry('getTime') : null)};
 } */
			
		/** Time entry before set time callback.
			Triggered when the input field value is to be changed.
			@callback beforeSetTimeCallback
			@param current {string} The current time value entered.
			@param newTime {string} The new time value to use.
			@param minTime {Date} The minimum time value allowed.
			@param maxTime {Date} The maximum time value allowed.
			@return {Date} The actual time value to set.
			@example beforeSetTime: function(oldTime, newTime, minTime, maxTime) {
 	var increment = (newTime - (oldTime || newTime)) > 0;
 	if (newTime.getMinutes() > 30) { // First half of hour only
 		newTime.setMinutes(increment ? 0 : 30);
 		newTime.setHours(newTime.getHours() + (increment ? 1 : 0));
 	}
 	return newTime;
 } */
			
		/** Default settings for the plugin.
			@property [appendText=''] {string} Display text following the input box, e.g. showing the format.
			@property [showSeconds=false] {boolean} <code>true</code> to show seconds as well,
						<code>false</code> for hours/minutes only.
			@property [unlimitedHours=false] {boolean} <code>true</code> to allow entry of more than 24 hours,
						<code>false</code> to restrict to one day.
			@property [timeSteps=[1,1,1]] {number[]} Steps for each of hours/minutes/seconds when incrementing/decrementing.
			@property [initialField=null] {number} The field to highlight initially (0 = hours, 1 = minutes, ...),
						or <code>null</code> for none.
			@property [noSeparatorEntry=false] {boolean} <code>true</code> to move to next sub-field after two digits entry.
			@property [tabToExit=false] {boolean} <code>true</code> for tab key to go to next element,
						<code>false</code> for tab key to step through internal fields.
			@property [useMouseWheel=true] {boolean} <code>true</code> to use mouse wheel for increment/decrement if possible,
						<code>false</code> to never use it.
			@property [defaultTime=null] {Date|number|string} The time to use if none has been set,
						or <code>null</code> for now. Specify as a <code>Date</code> object, as a number of seconds
						offset from now, or as a string of offsets from now, using 'H' for hours,
						'M' for minutes, 'S' for seconds.
			@property [minTime=null] {Date|number|string|number[]} The earliest selectable time,
						or <code>null</code> for no limit. See <code>defaultTime</code> for possible formats,
						use array of hours, minutes, seconds for <code>unlimitedHours</code>.
			@property [maxTime=null] {Date|number|string|number[]} The latest selectable time,
						or <code>null</code> for no limit. See <code>defaultTime</code> for possible formats,
						use array of hours, minutes, seconds for <code>unlimitedHours</code>.
			@property [spinnerImage='spinnerDefault.png'] {string} The URL of the images to use for the time spinner -
						seven images packed horizontally for normal, each button pressed
						(centre, previous, next, increment, decrement), and disabled.
			@property [spinnerSize=[20,20,8]] {number[]} The width and height of the spinner image,
						and size of centre button for current time.
			@property [spinnerBigImage=''] {string} The URL of the images to use for the expanded time spinner -
						seven images packed horizontally for normal, each button pressed
						(centre, previous, next, increment, decrement), and disabled.
			@property [spinnerBigSize=[40,40,16]] {number[]} The width and height of the expanded spinner image,
						and size of centre button for current time.
			@property [spinnerIncDecOnly=false] {boolean} <code>true</code> for increment/decrement buttons only, <code>false</code> for all.
			@property [spinnerRepeat=[500,250]] {number[]} Initial and subsequent waits in milliseconds
						for repeats on the spinner buttons.
			@property [beforeShow=null] {beforeShowCallback} Function that takes an input field and
						returns a set of custom settings for the time entry.
			@property [beforeSetTime=null] {beforeSetTimeCallback} Function that runs before updating the time,
						takes the old and new times, and minimum and maximum times as parameters,
						and returns an adjusted time if necessary.
			@example {defaultTime: new Date(0, 0, 0, 8, 30, 0), minTime: -300, maxTime: '+2H +30M'} */
		defaultOptions: {
			appendText: '',
			showSeconds: false,
			unlimitedHours: false,
			timeSteps: [1, 1, 1],
			initialField: null,
			noSeparatorEntry: false,
			tabToExit: false,
			useMouseWheel: true,
			defaultTime: null,
			minTime: null,
			maxTime: null,
			spinnerImage: 'spinnerDefault.png',
			spinnerSize: [20, 20, 8],
			spinnerBigImage: '',
			spinnerBigSize: [40, 40, 16],
			spinnerIncDecOnly: false,
			spinnerRepeat: [500, 250],
			beforeShow: null,
			beforeSetTime: null
		},

		/** Localisations for the plugin.
			Entries are objects indexed by the language code ('' being the default US/English).
			Each object has the following attributes.
			@property [show24Hours=false] {boolean} <code>true</code> to use 24 hour time, <code>false</code> for 12 hour (AM/PM).
			@property [separator=':'] {string} The separator between time fields.
			@property [ampmPrefix=''] {string} The separator before the AM/PM text.
			@property [ampmNames=['AM','PM']] {string[]} Names of morning/evening markers.
			@property [spinnerTexts=['Now','Previous&nbsp;field','Next&nbsp;field','Increment','Decrement']] {string[]}
						The popup texts for the spinner image areas. */
		regionalOptions: { // Available regional settings, indexed by language/country code
			'': { // Default regional settings - English/US
				show24Hours: false,
				separator: ':',
				ampmPrefix: '',
				ampmNames: ['AM', 'PM'],
				spinnerTexts: ['Now', 'Previous field', 'Next field', 'Increment', 'Decrement']
			}
		},
		
		_getters: ['getOffset', 'getTime', 'isDisabled'],

		_appendClass: pluginName + '-append', // Class name for the appended content
		_controlClass: pluginName + '-control', // Class name for the date entry control
		_expandClass: pluginName + '-expand', // Class name for the expanded spinner

		_disabledInputs: [], // List of time inputs that have been disabled

		_instSettings: function(elem, options) {
			return {_field: 0, _selectedHour: 0, _selectedMinute: 0, _selectedSecond: 0};
		},
		
		_postAttach: function(elem, inst) {
			elem.on('focus.' + inst.name, this._doFocus).
				on('blur.' + inst.name, this._doBlur).
				on('click.' + inst.name, this._doClick).
				on('keydown.' + inst.name, this._doKeyDown).
				on('keypress.' + inst.name, this._doKeyPress).
				on('paste.' + inst.name, function(event) { // Check pastes
					setTimeout(function() { plugin._parseTime(inst); }, 1);
				});
		},

		_optionsChanged: function(elem, inst, options) {
			var currentTime = this._extractTime(inst);
			$.extend(inst.options, options);
			inst.options.show24Hours = inst.options.show24Hours || inst.options.unlimitedHours;
			inst._field = 0;
			if (currentTime) {
				this._setTime(inst, new Date(0, 0, 0, currentTime[0], currentTime[1], currentTime[2]));
			}
			// Remove stuff dependent on old settings
			elem.next('span.' + this._appendClass).remove();
			elem.parent().find('span.' + this._controlClass).remove();
			if ($.fn.mousewheel) {
				elem.unmousewheel();
			}
			// And re-add if requested
			var spinner = (!inst.options.spinnerImage ? null :
				$('<span class="' + this._controlClass + '" style="display: inline-block; ' +
				'background: url(\'' + inst.options.spinnerImage + '\') 0 0 no-repeat; width: ' + 
				inst.options.spinnerSize[0] + 'px; height: ' + inst.options.spinnerSize[1] + 'px;"></span>'));
			elem.after(inst.options.appendText ? '<span class="' + this._appendClass + '">' +
				inst.options.appendText + '</span>' : '').after(spinner || '');
			// Allow mouse wheel usage
			if (inst.options.useMouseWheel && $.fn.mousewheel) {
				elem.mousewheel(this._doMouseWheel);
			}
			if (spinner) {
				spinner.mousedown(this._handleSpinner).mouseup(this._endSpinner).
					mouseover(this._expandSpinner).mouseout(this._endSpinner).
					mousemove(this._describeSpinner);
			}
		},

		/** Enable a time entry input and any associated spinner.
			@param elem {Element} The single input field.
			@example $(selector).timeEntry('enable') */
		enable: function(elem) {
			this._enableDisable(elem, false);
		},

		/** Disable a time entry input and any associated spinner.
			@param elem {Element} The single input field.
			@example $(selector).timeEntry('disable') */
		disable: function(elem) {
			this._enableDisable(elem, true);
		},

		/** Enable or disable a time entry input and any associated spinner.
			@private
			@param elem {Element} The single input field.
			@param disable {boolean} <code>true</code> to disable, <code>false</code> to enable. */
		_enableDisable: function(elem, disable) {
			var inst = this._getInst(elem);
			if (!inst) {
				return;
			}
			elem.disabled = disable;
			if (elem.nextSibling && elem.nextSibling.nodeName.toLowerCase() === 'span') {
				this._changeSpinner(inst, elem.nextSibling, (disable ? 5 : -1));
			}
			this._disabledInputs = $.map(this._disabledInputs,
				function(value) { return (value === elem ? null : value); }); // Delete entry
			if (disable) {
				this._disabledInputs.push(elem);
			}
		},

		/** Check whether an input field has been disabled.
			@param elem {Element} The input field to check.
			@return {boolean} <code>true</code> if this field has been disabled, <code>false</code> if it is enabled.
			@example if ($(selector).dateEntry('isDisabled')) {...} */
		isDisabled: function(elem) {
			return $.inArray(elem, this._disabledInputs) > -1;
		},

		_preDestroy: function(elem, inst) {
			elem = $(elem).off('.' + pluginName);
			if ($.fn.mousewheel) {
				elem.unmousewheel();
			}
			this._disabledInputs = $.map(this._disabledInputs,
				function(value) { return (value === elem[0] ? null : value); }); // Delete entry
			elem.siblings('.' + this._appendClass + ',.' + this._controlClass).remove();
		},

		/** Initialise the current time for a time entry input field.
			@param elem {Element} The input field to update.
			@param time {Date|number|string} The new time or offset or <code>null</code> to clear.
					An actual time or offset in seconds from now or units and periods of offsets from now.
			@example $(selector).timeEntry('setTime', new Date(0, 0, 0, 11, 22, 33))
 $(selector).timeEntry('setTime', +300)
 $(selector).timeEntry('setTime', '+1H +30M')
 $(selector).timeEntry('setTime', null) */
		setTime: function(elem, time) {
			var inst = this._getInst(elem);
			if (inst) {
				if (time === null || time === '') {
					$(elem).val('');
				}
				else {
					this._setTime(inst, time ? ($.isArray(time) ? time :
						(typeof time === 'object' ? new Date(time.getTime()) : time)) : null);
				}
			}
		},

		/** Retrieve the current time for a time entry input field.
			@param elem {Element} The input field to update.
			@return {Date} The current time or <code>null</code> if none.
			@example var time = $(selector).timeEntry('getTime') */
		getTime: function(elem) {
			var inst = this._getInst(elem);
			var currentTime = (inst ? this._extractTime(inst) : null);
			return (!currentTime ? null :
				new Date(0, 0, 0, currentTime[0], currentTime[1], currentTime[2]));
		},

		/** Retrieve the millisecond offset for the current time.
			@param elem {Element} The input field to examine.
			@return {number} The time as milliseconds offset or zero if none.
			@example var offset = $(selector).timeEntry('getOffset') */
		getOffset: function(elem) {
			var inst = this._getInst(elem);
			var currentTime = (inst ? this._extractTime(inst) : null);
			return (!currentTime ? 0 :
				(currentTime[0] * 3600 + currentTime[1] * 60 + currentTime[2]) * 1000);
		},

		/** Initialise date entry.
			@private
			@param elem {Element|Event} The input field or the focus event. */
		_doFocus: function(elem) {
			var input = (elem.nodeName && elem.nodeName.toLowerCase() === 'input' ? elem : this);
			if (plugin._lastInput === input || plugin.isDisabled(input)) {
				plugin._focussed = false;
				return;
			}
			var inst = plugin._getInst(input);
			plugin._focussed = true;
			plugin._lastInput = input;
			plugin._blurredInput = null;
			$.extend(inst.options, ($.isFunction(inst.options.beforeShow) ?
				inst.options.beforeShow.apply(input, [input]) : {}));
				plugin._parseTime(inst, elem.nodeName ? null : elem);
			setTimeout(function() { plugin._showField(inst); }, 10);
		},

		/** Note that the field has been exited.
			@private
			@param event {Event} The blur event. */
		_doBlur: function(event) {
			plugin._blurredInput = plugin._lastInput;
			plugin._lastInput = null;
		},

		/** Select appropriate field portion on click, if already in the field.
			@private
			@param event {Event} The click event. */
		_doClick: function(event) {
			var input = event.target;
			var inst = plugin._getInst(input);
			var prevField = inst._field;
			if (!plugin._focussed) {
				inst._field = plugin._getSelection(inst, input, event);
			}
			if (prevField !== inst._field) {
				inst._lastChr = '';
			}
			plugin._showField(inst);
			plugin._focussed = false;
		},

		/** Find the selected subfield within the control.
			@private
			@param inst {object} The current instance settings.
			@param input {Element} The input control.
			@param event {Event} The triggering event.
			@return {number} The selected subfield. */
		_getSelection: function(inst, input, event) {
			var select = 0;
			var fieldSizes = [inst.elem.val().split(inst.options.separator)[0].length, 2, 2];
			if (input.selectionStart !== null) { // Use input select range
				var end = 0;
				for (var field = 0; field <= Math.max(1, inst._secondField, inst._ampmField); field++) {
					end += (field !== inst._ampmField ? fieldSizes[field] + inst.options.separator.length :
						inst.options.ampmPrefix.length + inst.options.ampmNames[0].length);
					select = field;
					if (input.selectionStart < end) {
						break;
					}
				}
			}
			else if (input.createTextRange && event != null) { // Check against bounding boxes
				var src = $(event.srcElement);
				var range = input.createTextRange();
				var convert = function(value) {
					return {thin: 2, medium: 4, thick: 6}[value] || value;
				};
				var offsetX = event.clientX + document.documentElement.scrollLeft -
					(src.offset().left + parseInt(convert(src.css('border-left-width')), 10)) -
					range.offsetLeft; // Position - left edge - alignment
				for (var field = 0; field <= Math.max(1, inst._secondField, inst._ampmField); field++) {
					var end = (field !== inst._ampmField ? (field * fieldSize) + 2 :
						(inst._ampmField * fieldSize) + inst.options.ampmPrefix.length +
						inst.options.ampmNames[0].length);
					range.collapse();
					range.moveEnd('character', end);
					select = field;
					if (offsetX < range.boundingWidth) { // And compare
						break;
					}
				}
			}
			return select;
		},

		/** Handle keystrokes in the field.
			@private
			@param event {Event} The keydown event.
			@return {boolean} <code>true</code> to continue, <code>false</code> to stop processing. */
		_doKeyDown: function(event) {
			if (event.keyCode >= 48) { // >= '0'
				return true;
			}
			var inst = plugin._getInst(event.target);
			switch (event.keyCode) {
				case 9: return (inst.options.tabToExit ? true : (event.shiftKey ?
							// Move to previous time field, or out if at the beginning
							plugin._changeField(inst, -1, true) :
							// Move to next time field, or out if at the end
							plugin._changeField(inst, +1, true)));
				case 35: if (event.ctrlKey) { // Clear time on ctrl+end
							plugin._setValue(inst, '');
						}
						else { // Last field on end
							inst._field = Math.max(1, inst._secondField, inst._ampmField);
							plugin._adjustField(inst, 0);
						}
						break;
				case 36: if (event.ctrlKey) { // Current time on ctrl+home
							plugin._setTime(inst);
						}
						else { // First field on home
							inst._field = 0;
							plugin._adjustField(inst, 0);
						}
						break;
				case 37: plugin._changeField(inst, -1, false); break; // Previous field on left
				case 38: plugin._adjustField(inst, +1); break; // Increment time field on up
				case 39: plugin._changeField(inst, +1, false); break; // Next field on right
				case 40: plugin._adjustField(inst, -1); break; // Decrement time field on down
				case 46: plugin._setValue(inst, ''); break; // Clear time on delete
				case 8: inst._lastChr = ''; // Fall through
				default: return true;
			}
			return false;
		},

		/** Disallow unwanted characters.
			@private
			@param event {Event} The keypress event.
			@return {boolean} <code>true</code> to continue, <code>false</code> to stop processing. */
		_doKeyPress: function(event) {
			var chr = String.fromCharCode(event.charCode === undefined ? event.keyCode : event.charCode);
			if (chr < ' ') {
				return true;
			}
			var inst = plugin._getInst(event.target);
			plugin._handleKeyPress(inst, chr);
			return false;
		},

		/** Update date based on keystroke entered.
			@private
			@param inst {object} The instance settings.
			@param chr {string} The new character. */
		_handleKeyPress: function(inst, chr) {
			if (chr === inst.options.separator) {
				this._changeField(inst, +1, false);
			}
			else if (chr >= '0' && chr <= '9') { // Allow direct entry of date
				var key = parseInt(chr, 10);
				var value = parseInt(inst._lastChr + chr, 10);
				var hour = (inst._field !== 0 ? inst._selectedHour :
					(inst.options.unlimitedHours ? value :
					(inst.options.show24Hours ? (value < 24 ? value : key) :
					(value >= 1 && value <= 12 ? value :
					(key > 0 ? key : inst._selectedHour)) % 12 +
					(inst._selectedHour >= 12 ? 12 : 0))));
				var minute = (inst._field !== 1 ? inst._selectedMinute :
					(value < 60 ? value : key));
				var second = (inst._field !== inst._secondField ? inst._selectedSecond :
					(value < 60 ? value : key));
				var fields = this._constrainTime(inst, [hour, minute, second]);
				this._setTime(inst, (inst.options.unlimitedHours ? fields :
					new Date(0, 0, 0, fields[0], fields[1], fields[2])));
				if (inst.options.noSeparatorEntry && inst._lastChr) {
					this._changeField(inst, +1, false);
				}
				else {
					inst._lastChr = (inst.options.unlimitedHours && inst._field === 0 ? inst._lastChr + chr : chr);
				}
			}
			else if (!inst.options.show24Hours) { // Set am/pm based on first char of names
				chr = chr.toLowerCase();
				if ((chr === inst.options.ampmNames[0].substring(0, 1).toLowerCase() &&
						inst._selectedHour >= 12) ||
						(chr === inst.options.ampmNames[1].substring(0, 1).toLowerCase() &&
						inst._selectedHour < 12)) {
					var saveField = inst._field;
					inst._field = inst._ampmField;
					this._adjustField(inst, +1);
					inst._field = saveField;
					this._showField(inst);
				}
			}
		},

		/** Increment/decrement on mouse wheel activity.
			@private
			@param event {Event} The mouse wheel event.
			@param delta {number} The amount of change. */
		_doMouseWheel: function(event, delta) {
			if (plugin.isDisabled(event.target)) {
				return;
			}
			var inst = plugin._getInst(event.target);
			inst.elem.focus();
			if (!inst.elem.val()) {
				plugin._parseTime(inst);
			}
			plugin._adjustField(inst, delta);
			event.preventDefault();
		},

		/** Expand the spinner, if possible, to make it easier to use.
			@private
			@param event {Event} The mouse over event. */
		_expandSpinner: function(event) {
			var spinner = plugin._getSpinnerTarget(event);
			var inst = plugin._getInst(plugin._getInput(spinner));
			if (plugin.isDisabled(inst.elem[0])) {
				return;
			}
			if (inst.options.spinnerBigImage) {
				inst._expanded = true;
				var offset = $(spinner).offset();
				var relative = null;
				$(spinner).parents().each(function() {
					var parent = $(this);
					if (parent.css('position') === 'relative' || parent.css('position') === 'absolute') {
						relative = parent.offset();
					}
					return !relative;
				});
				$('<div class="' + plugin._expandClass + '" style="position: absolute; left: ' +
					(offset.left - (inst.options.spinnerBigSize[0] - inst.options.spinnerSize[0]) / 2 -
					(relative ? relative.left : 0)) + 'px; top: ' +
					(offset.top - (inst.options.spinnerBigSize[1] - inst.options.spinnerSize[1]) / 2 -
					(relative ? relative.top : 0)) + 'px; width: ' +
					inst.options.spinnerBigSize[0] + 'px; height: ' +
					inst.options.spinnerBigSize[1] + 'px; background: transparent url(' +
					inst.options.spinnerBigImage + ') no-repeat 0px 0px; z-index: 10;"></div>').
					mousedown(plugin._handleSpinner).mouseup(plugin._endSpinner).
					mouseout(plugin._endExpand).mousemove(plugin._describeSpinner).
					insertAfter(spinner);
			}
		},

		/** Locate the actual input field from the spinner.
			@private
			@param spinner {Element} The current spinner.
			@return {Element} The corresponding input. */
		_getInput: function(spinner) {
			return $(spinner).siblings('.' + this._getMarker())[0];
		},

		/** Change the title based on position within the spinner.
			@private
			@param event {Event} The mouse move event. */
		_describeSpinner: function(event) {
			var spinner = plugin._getSpinnerTarget(event);
			var inst = plugin._getInst(plugin._getInput(spinner));
			spinner.title = inst.options.spinnerTexts[plugin._getSpinnerRegion(inst, event)];
		},

		/** Handle a click on the spinner.
			@private
			@param event {Event} The mouse click event. */
		_handleSpinner: function(event) {
			var spinner = plugin._getSpinnerTarget(event);
			var input = plugin._getInput(spinner);
			if (plugin.isDisabled(input)) {
				return;
			}
			if (input === plugin._blurredInput) {
				plugin._lastInput = input;
				plugin._blurredInput = null;
			}
			var inst = plugin._getInst(input);
			plugin._doFocus(input);
			var region = plugin._getSpinnerRegion(inst, event);
			plugin._changeSpinner(inst, spinner, region);
			plugin._actionSpinner(inst, region);
			plugin._timer = null;
			plugin._handlingSpinner = true;
			if (region >= 3 && inst.options.spinnerRepeat[0]) { // Repeat increment/decrement
				plugin._timer = setTimeout(
					function() { plugin._repeatSpinner(inst, region); },
					inst.options.spinnerRepeat[0]);
				$(spinner).one('mouseout', plugin._releaseSpinner).
					one('mouseup', plugin._releaseSpinner);
			}
		},

		/** Action a click on the spinner.
			@private
			@param inst {object} The instance settings.
			@param region {number} The spinner "button". */
		_actionSpinner: function(inst, region) {
			if (!inst.elem.val()) {
				plugin._parseTime(inst);
			}
			switch (region) {
				case 0: this._setTime(inst); break;
				case 1: this._changeField(inst, -1, false); break;
				case 2: this._changeField(inst, +1, false); break;
				case 3: this._adjustField(inst, +1); break;
				case 4: this._adjustField(inst, -1); break;
			}
		},

		/** Repeat a click on the spinner.
			@private
			@param inst {object} The instance settings.
			@param region {number} The spinner "button". */
		_repeatSpinner: function(inst, region) {
			if (!plugin._timer) {
				return;
			}
			plugin._lastInput = plugin._blurredInput;
			this._actionSpinner(inst, region);
			this._timer = setTimeout(
				function() { plugin._repeatSpinner(inst, region); },
				inst.options.spinnerRepeat[1]);
		},

		/** Stop a spinner repeat.
			@private
			@param event {Event} The mouse event. */
		_releaseSpinner: function(event) {
			clearTimeout(plugin._timer);
			plugin._timer = null;
		},

		/** Tidy up after an expanded spinner.
			@private
			@param event {Event} The mouse event. */
		_endExpand: function(event) {
			plugin._timer = null;
			var spinner = plugin._getSpinnerTarget(event);
			var input = plugin._getInput(spinner);
			var inst = plugin._getInst(input);
			$(spinner).remove();
			inst._expanded = false;
		},

		/** Tidy up after a spinner click.
			@private
			@param event {Event} The mouse event. */
		_endSpinner: function(event) {
			plugin._timer = null;
			var spinner = plugin._getSpinnerTarget(event);
			var input = plugin._getInput(spinner);
			var inst = plugin._getInst(input);
			if (!plugin.isDisabled(input)) {
				plugin._changeSpinner(inst, spinner, -1);
			}
			if (plugin._handlingSpinner) {
				plugin._lastInput = plugin._blurredInput;
			}
			if (plugin._lastInput && plugin._handlingSpinner) {
				plugin._showField(inst);
			}
			plugin._handlingSpinner = false;
		},

		/** Retrieve the spinner from the event.
			@private
			@param event {Event} The mouse click event.
			@return {Element} The target field. */
		_getSpinnerTarget: function(event) {
			return event.target || event.srcElement;
		},

		/** Determine which "button" within the spinner was clicked.
			@private
			@param inst {object} The instance settings.
			@param event {Event} The mouse event.
			@return {number} The spinner "button" number. */
		_getSpinnerRegion: function(inst, event) {
			var spinner = this._getSpinnerTarget(event);
			var pos = $(spinner).offset();
			var scrolled = [document.documentElement.scrollLeft || document.body.scrollLeft,
				document.documentElement.scrollTop || document.body.scrollTop];
			var left = (inst.options.spinnerIncDecOnly ? 99 : event.clientX + scrolled[0] - pos.left);
			var top = event.clientY + scrolled[1] - pos.top;
			var spinnerSize = inst.options[inst._expanded ? 'spinnerBigSize' : 'spinnerSize'];
			var right = (inst.options.spinnerIncDecOnly ? 99 : spinnerSize[0] - 1 - left);
			var bottom = spinnerSize[1] - 1 - top;
			if (spinnerSize[2] > 0 && Math.abs(left - right) <= spinnerSize[2] &&
					Math.abs(top - bottom) <= spinnerSize[2]) {
				return 0; // Centre button
			}
			var min = Math.min(left, top, right, bottom);
			return (min === left ? 1 : (min === right ? 2 : (min === top ? 3 : 4))); // Nearest edge
		},

		/** Change the spinner image depending on the button clicked.
			@private
			@param inst {object} The instance settings.
			@param spinner {Element} The spinner control.
			@param region {number} The spinner "button". */
		_changeSpinner: function(inst, spinner, region) {
			$(spinner).css('background-position', '-' + ((region + 1) *
				inst.options[inst._expanded ? 'spinnerBigSize' : 'spinnerSize'][0]) + 'px 0px');
		},

		/** Extract the time value from the input field, or default to now.
			@private
			@param inst {object} The instance settings.
			@param event {Event} The triggering event or <code>null</code>. */
		_parseTime: function(inst, event) {
			var currentTime = this._extractTime(inst);
			if (currentTime) {
				inst._selectedHour = currentTime[0];
				inst._selectedMinute = currentTime[1];
				inst._selectedSecond = currentTime[2];
			}
			else {
				var now = this._constrainTime(inst);
				inst._selectedHour = now[0];
				inst._selectedMinute = now[1];
				inst._selectedSecond = (inst.options.showSeconds ? now[2] : 0);
			}
			inst._secondField = (inst.options.showSeconds ? 2 : -1);
			inst._ampmField = (inst.options.show24Hours ? -1 : (inst.options.showSeconds ? 3 : 2));
			inst._lastChr = '';
			var postProcess = function() {
				if (inst.elem.val() !== '') {
					plugin._showTime(inst);
				}
			};
			if (typeof inst.options.initialField === 'number') {
				inst._field = Math.max(0, Math.min(
					Math.max(1, inst._secondField, inst._ampmField), inst.options.initialField));
				postProcess();
			}
			else {
				setTimeout(function() {
					inst._field = plugin._getSelection(inst, inst.elem[0], event);
					postProcess();
				}, 0);
			}
		},

		/** Extract the time value from a string as an array of values, or default to <code>null</code>.
			@private
			@param value {string} The date text.
			@param inst {object} The instance settings.
			@return {number[]} The retrieved time components (hours, minutes, seconds) or
					<code>null</code> if no value. */
		_extractTime: function(inst, value) {
			value = value || inst.elem.val();
			var currentTime = value.split(inst.options.separator);
			if (inst.options.separator === '' && value !== '') {
				currentTime[0] = value.substring(0, 2);
				currentTime[1] = value.substring(2, 4);
				currentTime[2] = value.substring(4, 6);
			}
			if (currentTime.length >= 2) {
				var isAM = !inst.options.show24Hours && (value.indexOf(inst.options.ampmNames[0]) > -1);
				var isPM = !inst.options.show24Hours && (value.indexOf(inst.options.ampmNames[1]) > -1);
				var hour = parseInt(currentTime[0], 10);
				hour = (isNaN(hour) ? 0 : hour);
				hour = ((isAM || isPM) && hour === 12 ? 0 : hour) + (isPM ? 12 : 0);
				var minute = parseInt(currentTime[1], 10);
				minute = (isNaN(minute) ? 0 : minute);
				var second = (currentTime.length >= 3 ? parseInt(currentTime[2], 10) : 0);
				second = (isNaN(second) || !inst.options.showSeconds ? 0 : second);
				return this._constrainTime(inst, [hour, minute, second]);
			} 
			return null;
		},

		/** Constrain the given/current time to the time steps.
			@private
			@param inst {object} The instance settings.
			@param fields {number[]} The current time components (hours, minutes, seconds).
			@return {number[]} The constrained time components (hours, minutes, seconds). */
		_constrainTime: function(inst, fields) {
			var specified = (fields !== null && fields !== undefined);
			if (!specified) {
				var now = this._determineTime(inst.options.defaultTime, inst) || new Date();
				fields = [now.getHours(), now.getMinutes(), now.getSeconds()];
			}
			var reset = false;
			for (var i = 0; i < inst.options.timeSteps.length; i++) {
				if (reset) {
					fields[i] = 0;
				}
				else if (inst.options.timeSteps[i] > 1) {
					fields[i] = Math.round(fields[i] / inst.options.timeSteps[i]) *
						inst.options.timeSteps[i];
					reset = true;
				}
			}
			return fields;
		},

		/** Set the selected time into the input field.
			@private
			@param inst {object} The instance settings. */
		_showTime: function(inst) {
			var currentTime = (inst.options.unlimitedHours ? inst._selectedHour :
				this._formatNumber(inst.options.show24Hours ? inst._selectedHour :
				((inst._selectedHour + 11) % 12) + 1)) + inst.options.separator +
				this._formatNumber(inst._selectedMinute) +
				(inst.options.showSeconds ? inst.options.separator +
				this._formatNumber(inst._selectedSecond) : '') +
				(inst.options.show24Hours ?  '' : inst.options.ampmPrefix +
				inst.options.ampmNames[(inst._selectedHour < 12 ? 0 : 1)]);
			this._setValue(inst, currentTime);
			this._showField(inst);
		},

		/** Highlight the current date field.
			@private
			@param inst {object} The instance settings. */
		_showField: function(inst) {
			var input = inst.elem[0];
			if (inst.elem.is(':hidden') || plugin._lastInput !== input) {
				return;
			}
			var fieldSizes = [inst.elem.val().split(inst.options.separator)[0].length, 2, 2];
			var start = 0;
			var field = 0;
			while (field < inst._field) {
				start += fieldSizes[field] +
					(field === Math.max(1, inst._secondField) ? 0 : inst.options.separator.length);
				field++;
			}
			var end = start + (inst._field !== inst._ampmField ? fieldSizes[field] :
				inst.options.ampmPrefix.length + inst.options.ampmNames[0].length);
			if (input.setSelectionRange) { // Mozilla
				input.setSelectionRange(start, end);
			}
			else if (input.createTextRange) { // IE
				var range = input.createTextRange();
				range.moveStart('character', start);
				range.moveEnd('character', end - inst.elem.val().length);
				range.select();
			}
			if (!input.disabled) {
				input.focus();
			}
		},

		/** Ensure displayed single number has a leading zero.
			@private
			@param value {number} The current value.
			@return {string} Number with at least two digits. */
		_formatNumber: function(value) {
			return (value < 10 ? '0' : '') + value;
		},

		/** Update the input field and notify listeners.
			@private
			@param inst {object} The instance settings.
			@param value {string} The new value. */
		_setValue: function(inst, value) {
			if (value !== inst.elem.val()) {
				inst.elem.val(value).trigger('change');
			}
		},

		/** Move to previous/next field, or out of field altogether if appropriate.
			@private
			@param inst {object} The instance settings.
			@param offset {number} The direction of change (-1, +1).
			@param moveOut {boolean} <code>true</code> if can move out of the field.
			@return {boolean} <code>true</code> if exiting the field, <code>false</code> if not. */
		_changeField: function(inst, offset, moveOut) {
			var atFirstLast = (inst.elem.val() === '' ||
				inst._field === (offset === -1 ? 0 : Math.max(1, inst._secondField, inst._ampmField)));
			if (!atFirstLast) {
				inst._field += offset;
			}
			this._showField(inst);
			inst._lastChr = '';
			return (atFirstLast && moveOut);
		},

		/** Update the current field in the direction indicated.
			@private
			@param inst {object} The instance settings.
			@param offset {number} The amount to change by. */
		_adjustField: function(inst, offset) {
			if (inst.elem.val() === '') {
				offset = 0;
			}
			if (inst.options.unlimitedHours) {
				this._setTime(inst, [inst._selectedHour + (inst._field === 0 ? offset * inst.options.timeSteps[0] : 0),
					inst._selectedMinute + (inst._field === 1 ? offset * inst.options.timeSteps[1] : 0),
					inst._selectedSecond + (inst._field === inst._secondField ? offset * inst.options.timeSteps[2] : 0)]);
			}
			else {
			this._setTime(inst, new Date(0, 0, 0,
				inst._selectedHour + (inst._field === 0 ? offset * inst.options.timeSteps[0] : 0) +
				(inst._field === inst._ampmField ? offset * 12 : 0),
				inst._selectedMinute + (inst._field === 1 ? offset * inst.options.timeSteps[1] : 0),
					inst._selectedSecond + (inst._field === inst._secondField ? offset * inst.options.timeSteps[2] : 0)));
			}
		},

		/** Check against minimum/maximum and display time.
			@private
			@param inst {object} The instance settings.
			@param time {Date|number|string|number[]} The actual time or offset in seconds from now or
					units and periods of offsets from now or numeric period values. */
		_setTime: function(inst, time) {
			if (inst.options.unlimitedHours && $.isArray(time)) {
				var fields = time;
			}
			else {
			time = this._determineTime(time, inst);
				var fields = (time ? [time.getHours(), time.getMinutes(), time.getSeconds()] : null);
			}
			fields = this._constrainTime(inst, fields);
			time = new Date(0, 0, 0, fields[0], fields[1], fields[2]);
			// Normalise to base date
			var time = this._normaliseTime(time);
			var minTime = this._normaliseTime(this._determineTime(inst.options.minTime, inst));
			var maxTime = this._normaliseTime(this._determineTime(inst.options.maxTime, inst));
			// Ensure it is within the bounds set
			if (inst.options.unlimitedHours) {
				while (fields[2] < 0) {
					fields[2] += 60;
					fields[1]--;
				}
				while (fields[2] > 59) {
					fields[2] -= 60;
					fields[1]++;
				}
				while (fields[1] < 0) {
					fields[1] += 60;
					fields[0]--;
				}
				while (fields[1] > 59) {
					fields[1] -= 60;
					fields[0]++;
				}
				minTime = (inst.options.minTime != null && $.isArray(inst.options.minTime)) ?
					inst.options.minTime : [0, 0, 0];
				if (fields[0] < minTime[0]) {
					fields = minTime.slice(0, 3);
				}
				else if (fields[0] === minTime[0]) {
					if (fields[1] < minTime[1]) {
						fields[1] = minTime[1];
						fields[2] = minTime[2];
					}
					else if (fields[1] === minTime[1]) {
						if (fields[2] < minTime[2]) {
							fields[2] = minTime[2];
						}
					}
				}
				if (inst.options.maxTime != null && $.isArray(inst.options.maxTime)) {
					if (fields[0] > inst.options.maxTime[0]) {
						fields = inst.options.maxTime.slice(0, 3);
					}
					else if (fields[0] === inst.options.maxTime[0]) {
						if (fields[1] > inst.options.maxTime[1]) {
							fields[1] = inst.options.maxTime[1];
							fields[2] = inst.options.maxTime[2];
						}
						else if (fields[1] === inst.options.maxTime[1]) {
							if (fields[2] > inst.options.maxTime[2]) {
								fields[2] = inst.options.maxTime[2];
							}
						}
					}
				}
			}
			else {
			if (minTime && maxTime && minTime > maxTime) {
				if (time < minTime && time > maxTime) {
					time = (Math.abs(time - minTime) < Math.abs(time - maxTime) ? minTime : maxTime);
				}
			}
			else {
				time = (minTime && time < minTime ? minTime :
					(maxTime && time > maxTime ? maxTime : time));
			}
				fields[0] = time.getHours();
				fields[1] = time.getMinutes();
				fields[2] = time.getSeconds();
			}
			// Perform further restrictions if required
			if ($.isFunction(inst.options.beforeSetTime)) {
				time = inst.options.beforeSetTime.apply(inst.elem[0],
					[this.getTime(inst.elem[0]), time, minTime, maxTime]);
				fields[0] = time.getHours();
				fields[1] = time.getMinutes();
				fields[2] = time.getSeconds();
			}
			inst._selectedHour = fields[0];
			inst._selectedMinute = fields[1];
			inst._selectedSecond = fields[2];
			this._showTime(inst);
		},

		/** A time may be specified as an exact value or a relative one.
			@private
			@param setting {Date|number|string|number[]} The actual time or offset in seconds from now or
					units and periods of offsets from now or numeric period values.
			@param inst {object} The instance settings.
			@return {Date} The calculated time. */
		_determineTime: function(setting, inst) {
			var offsetNumeric = function(offset) { // E.g. +300, -2
				var time = new Date();
				time.setTime(time.getTime() + offset * 1000);
				return time;
			};
			var offsetString = function(offset) { // E.g. '+2m', '-4h', '+3h +30m' or '12:34:56PM'
				var fields = plugin._extractTime(inst, offset); // Actual time?
				var time = new Date();
				var hour = (fields ? fields[0] : time.getHours());
				var minute = (fields ? fields[1] : time.getMinutes());
				var second = (fields ? fields[2] : time.getSeconds());
				if (!fields) {
					var pattern = /([+-]?[0-9]+)\s*(s|S|m|M|h|H)?/g;
					var matches = pattern.exec(offset);
					while (matches) {
						switch (matches[2] || 's') {
							case 's' : case 'S' :
								second += parseInt(matches[1], 10); break;
							case 'm' : case 'M' :
								minute += parseInt(matches[1], 10); break;
							case 'h' : case 'H' :
								hour += parseInt(matches[1], 10); break;
						}
						matches = pattern.exec(offset);
					}
				}
				time = new Date(0, 0, 10, hour, minute, second, 0);
				if (/^!/.test(offset)) { // No wrapping
					if (time.getDate() > 10) {
						time = new Date(0, 0, 10, 23, 59, 59);
					}
					else if (time.getDate() < 10) {
						time = new Date(0, 0, 10, 0, 0, 0);
					}
				}
				return time;
			};
			var offsetArray = function(setting) {
				return new Date(0, 0, 0, setting[0], setting[1] || 0, setting[2] || 0, 0);
			};
			return (setting ? (typeof setting === 'string' ? offsetString(setting) :
				(typeof setting === 'number' ? offsetNumeric(setting) :
				($.isArray(setting) ? offsetArray(setting) : setting))) : null);
		},

		/** Normalise time object to a common date.
			@private
			@param time {Date} The original time.
			@return {Date} The normalised time. */
		_normaliseTime: function(time) {
			if (!time) {
				return null;
			}
			time.setFullYear(1900);
			time.setMonth(0);
			time.setDate(0);
			return time;
		}
	});
	
	var plugin = $.timeEntry;

})(jQuery);
