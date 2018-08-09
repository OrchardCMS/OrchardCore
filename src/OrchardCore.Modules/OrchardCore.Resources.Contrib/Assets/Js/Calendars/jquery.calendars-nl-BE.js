/* http://keith-wood.name/calendars.html
   Dutch/Belgian localisation for Gregorian/Julian calendars for jQuery.
   Written by Mathias Bynens <http://mathiasbynens.be/>. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['nl-BE'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['januari', 'februari', 'maart', 'april', 'mei', 'juni',
		'juli', 'augustus', 'september', 'oktober', 'november', 'december'],
		monthNamesShort: ['jan', 'feb', 'maa', 'apr', 'mei', 'jun',
		'jul', 'aug', 'sep', 'okt', 'nov', 'dec'],
		dayNames: ['zondag', 'maandag', 'dinsdag', 'woensdag', 'donderdag', 'vrijdag', 'zaterdag'],
		dayNamesShort: ['zon', 'maa', 'din', 'woe', 'don', 'vri', 'zat'],
		dayNamesMin: ['zo', 'ma', 'di', 'wo', 'do', 'vr', 'za'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['nl-BE'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['nl-BE'];
	}
})(jQuery);
