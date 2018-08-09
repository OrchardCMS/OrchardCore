/* http://keith-wood.name/calendars.html
   Bosnian localisation for Gregorian/Julian calendars for jQuery.
   Kenan Konjo. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['bs'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Januar','Februar','Mart','April','Maj','Juni',
		'Juli','August','Septembar','Oktobar','Novembar','Decembar'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','Maj','Jun',
		'Jul','Aug','Sep','Okt','Nov','Dec'],
		dayNames: ['Nedelja','Ponedeljak','Utorak','Srijeda','Četvrtak','Petak','Subota'],
		dayNamesShort: ['Ned','Pon','Uto','Sri','Čet','Pet','Sub'],
		dayNamesMin: ['Ne','Po','Ut','Sr','Če','Pe','Su'],
		dateFormat: 'dd.mm.yy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['bs'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['bs'];
	}
})(jQuery);
