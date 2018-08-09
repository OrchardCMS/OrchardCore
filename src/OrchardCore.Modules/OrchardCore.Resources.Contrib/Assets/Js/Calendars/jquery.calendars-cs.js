/* http://keith-wood.name/calendars.html
   Czech localisation for Gregorian/Julian calendars for jQuery.
   Written by Tomas Muller (tomas@tomas-muller.net). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['cs'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['leden','únor','březen','duben','květen','červen',
        'červenec','srpen','září','říjen','listopad','prosinec'],
		monthNamesShort: ['led','úno','bře','dub','kvě','čer',
		'čvc','srp','zář','říj','lis','pro'],
		dayNames: ['neděle', 'pondělí', 'úterý', 'středa', 'čtvrtek', 'pátek', 'sobota'],
		dayNamesShort: ['ne', 'po', 'út', 'st', 'čt', 'pá', 'so'],
		dayNamesMin: ['ne','po','út','st','čt','pá','so'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['cs'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['cs'];
	}
})(jQuery);
