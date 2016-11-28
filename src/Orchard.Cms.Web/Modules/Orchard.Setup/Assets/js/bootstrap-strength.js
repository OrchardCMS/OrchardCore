/*
 * bootstrap-strength.js
 * Original author: @rivalex
 * Modified by @agriffard for Bootstrap 4
 * Licensed under the MIT license
 */
;(function(window, $, undefined) {
	
	"use strict";
	
	$.bootstrapStrength = function bootstrapStrength(options,element) {
		this.element		= element;
		this.$elem			= $(this.element);
		this.options		= $.extend({}, $.bootstrapStrength.defaults, options);
		var _self	 		= this;
		_self._init();
	}
	
	$.bootstrapStrength.defaults = {
		striped: true,
		active: true,
		slimBar: false,
		minLength: 8,
		upperCase: 1,
		upperReg: "[A-Z]",
		lowerCase: 1,
		lowerReg: "[a-z]",
		numbers: 1,
		numberReg: "[0-9]",
		specialchars: 1,
		specialReg: "[!,%,&,@,#,$,^,*,?,_,~]",
		topMargin: "5px;",
		meterClasses: {
			weak: "progress-danger",
			medium: "progress-warning",
			good: "progress-success"
		}
	};
	
	$.bootstrapStrength.prototype = {
		
		_init: function() {
			
			var _self			= this,
				options			= _self.options,
				progressClass	= options.meterClasses,
				element			= _self.$elem,
				elementID		= element.attr('id'),
				stringLength = 0,
				capitals = 0,
				lowers = 0,
				numbers = 0,
				special = 0,
				progress;
			
			function calcPercentage(full, partial) {
				return ((partial / full) * 100).toFixed(0);
			}
			
			function matchString(string, range, limit) {
				var searchPattern, rx;
				searchPattern = "(.*" + range + ".*){" + limit + ",}";
				rx = new RegExp( searchPattern, "g" );
				return (string.match(rx))?1:0;
			}
			
			function checkStrength(string, elementID){
				stringLength = (string.length >= options.minLength)?1:0;
				capitals = (options.upperCase > 0) ? matchString(string, options.upperReg, options.upperCase) : 1;
				lowers = (options.lowerCase > 0) ? matchString(string, options.lowerReg, options.lowerCase) : 1;
				numbers = (options.numbers > 0) ? matchString(string, options.numberReg, options.numbers) : 1;
				special = (options.specialchars > 0) ? matchString(string, options.specialReg, options.specialchars) : 1;
				var totalpercent = calcPercentage(5, (stringLength + capitals + lowers + numbers + special));
				displayStrength(totalpercent,elementID);
			}
			
			function displayStrength(total, elementID){
				var meter = $('progress[data-meter="'+elementID+'"]'),
					meterClass;
				switch(parseInt(total)) {
					case 0: meterClass = progressClass.weak; break;
					case 20: meterClass = progressClass.weak; break;
					case 40: meterClass = progressClass.weak; break;
					case 60: meterClass = progressClass.medium; break;
					case 80: meterClass = progressClass.medium; break;
					case 100: meterClass = progressClass.good; break;
				}
				meter.removeClass(progressClass.weak + " " + progressClass.medium + " " + progressClass.good).val(total).addClass(meterClass);
			}
			
			progress = $("<progress/>", {
			    class: "progress" + ((options.striped) ? " progress-striped" : ""),
			    style: "margin-top:" + options.topMargin + ((options.slimBar) ? "height: 7px;border-radius: 7px" : ""),
			    "data-meter": elementID,
				"value": 0,
				"max": 100
			})
			
			if(this.$elem.parent().hasClass("input-group")) {
				this.$elem.parent().after(progress);
			} else {
				this.$elem.after(progress);
			}
			 
			this.$elem.bind('keyup keydown', function(e) {
				var thisString = $('#'+elementID).val();
				checkStrength(thisString,elementID);
			});
		}
	};

	$.fn.bootstrapStrength = function (options) {
		if ($.data(this, "bootstrapStrength")) return;
		return $(this).each(function() {
			$.data(this, "bootstrapStrength",new $.bootstrapStrength(options, this));
		})
	}
})(window, jQuery);


