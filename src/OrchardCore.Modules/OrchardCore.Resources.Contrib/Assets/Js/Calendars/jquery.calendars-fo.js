/* http://keith-wood.name/calendars.html
   Faroese localisation for Gregorian/Julian calendars for jQuery.
   Written by Sverri Mohr Olsen, sverrimo@gmail.com */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['fo'] = {
		name: 'Gregorianskur',
		epochs: ['BCE', 'CE'],
		monthNames: ['Januar','Februar','Mars','Apríl','Mei','Juni',
		'Juli','August','September','Oktober','November','Desember'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','Mei','Jun',
		'Jul','Aug','Sep','Okt','Nov','Des'],
		dayNames: ['Sunnudagur','Mánadagur','Týsdagur','Mikudagur','Hósdagur','Fríggjadagur','Leyardagur'],
		dayNamesShort: ['Sun','Mán','Týs','Mik','Hós','Frí','Ley'],
		dayNamesMin: ['Su','Má','Tý','Mi','Hó','Fr','Le'],
		dateFormat: 'dd-mm-yyyy',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['fo'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['fo'];
	}
})(jQuery);
