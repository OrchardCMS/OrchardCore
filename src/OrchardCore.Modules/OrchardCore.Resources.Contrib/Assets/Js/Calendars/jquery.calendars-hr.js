/* http://keith-wood.name/calendars.html
   Croatian localisation for Gregorian/Julian calendars for jQuery.
   Written by Vjekoslav Nesek. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['hr'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Siječanj','Veljača','Ožujak','Travanj','Svibanj','Lipanj',
		'Srpanj','Kolovoz','Rujan','Listopad','Studeni','Prosinac'],
		monthNamesShort: ['Sij','Velj','Ožu','Tra','Svi','Lip',
		'Srp','Kol','Ruj','Lis','Stu','Pro'],
		dayNames: ['Nedjelja','Ponedjeljak','Utorak','Srijeda','Četvrtak','Petak','Subota'],
		dayNamesShort: ['Ned','Pon','Uto','Sri','Čet','Pet','Sub'],
		dayNamesMin: ['Ne','Po','Ut','Sr','Če','Pe','Su'],
		dateFormat: 'dd.mm.yyyy.',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['hr'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['hr'];
	}
})(jQuery);
