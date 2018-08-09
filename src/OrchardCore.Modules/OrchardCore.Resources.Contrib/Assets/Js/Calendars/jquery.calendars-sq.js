/* http://keith-wood.name/calendars.html
   Albanian localisation for Gregorian/Julian calendars for jQuery.
   Written by Flakron Bytyqi (flakron@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['sq'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Janar','Shkurt','Mars','Prill','Maj','Qershor',
		'Korrik','Gusht','Shtator','Tetor','Nëntor','Dhjetor'],
		monthNamesShort: ['Jan','Shk','Mar','Pri','Maj','Qer',
		'Kor','Gus','Sht','Tet','Nën','Dhj'],
		dayNames: ['E Diel','E Hënë','E Martë','E Mërkurë','E Enjte','E Premte','E Shtune'],
		dayNamesShort: ['Di','Hë','Ma','Më','En','Pr','Sh'],
		dayNamesMin: ['Di','Hë','Ma','Më','En','Pr','Sh'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['sq'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['sq'];
	}
})(jQuery);
