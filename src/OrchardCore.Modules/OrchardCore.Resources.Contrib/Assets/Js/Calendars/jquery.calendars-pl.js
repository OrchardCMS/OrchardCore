/* http://keith-wood.name/calendars.html
   Polish localisation for Gregorian/Julian calendars for jQuery.
   Written by Jacek Wysocki (jacek.wysocki@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['pl'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Styczeń','Luty','Marzec','Kwiecień','Maj','Czerwiec',
		'Lipiec','Sierpień','Wrzesień','Październik','Listopad','Grudzień'],
		monthNamesShort: ['Sty','Lu','Mar','Kw','Maj','Cze',
		'Lip','Sie','Wrz','Pa','Lis','Gru'],
		dayNames: ['Niedziela','Poniedzialek','Wtorek','Środa','Czwartek','Piątek','Sobota'],
		dayNamesShort: ['Nie','Pn','Wt','Śr','Czw','Pt','So'],
		dayNamesMin: ['N','Pn','Wt','Śr','Cz','Pt','So'],
		dateFormat: 'yyyy-mm-dd',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['pl'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['pl'];
	}
})(jQuery);
