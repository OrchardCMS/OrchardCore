/* http://keith-wood.name/calendars.html
   Turkish localisation for Gregorian/Julian calendars for jQuery.
   Written by Izzet Emre Erkan (kara@karalamalar.net). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['tr'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Ocak','Şubat','Mart','Nisan','Mayıs','Haziran',
		'Temmuz','Ağustos','Eylül','Ekim','Kasım','Aralık'],
		monthNamesShort: ['Oca','Şub','Mar','Nis','May','Haz',
		'Tem','Ağu','Eyl','Eki','Kas','Ara'],
		dayNames: ['Pazar','Pazartesi','Salı','Çarşamba','Perşembe','Cuma','Cumartesi'],
		dayNamesShort: ['Pz','Pt','Sa','Ça','Pe','Cu','Ct'],
		dayNamesMin: ['Pz','Pt','Sa','Ça','Pe','Cu','Ct'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['tr'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['tr'];
	}
})(jQuery);
