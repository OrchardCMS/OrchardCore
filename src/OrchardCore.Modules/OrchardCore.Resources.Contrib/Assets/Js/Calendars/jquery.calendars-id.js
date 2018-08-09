/* http://keith-wood.name/calendars.html
   Indonesian localisation for Gregorian/Julian calendars for jQuery.
   Written by Deden Fathurahman (dedenf@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['id'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Januari','Februari','Maret','April','Mei','Juni',
		'Juli','Agustus','September','Oktober','Nopember','Desember'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','Mei','Jun',
		'Jul','Agus','Sep','Okt','Nop','Des'],
		dayNames: ['Minggu','Senin','Selasa','Rabu','Kamis','Jumat','Sabtu'],
		dayNamesShort: ['Min','Sen','Sel','Rab','kam','Jum','Sab'],
		dayNamesMin: ['Mg','Sn','Sl','Rb','Km','jm','Sb'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['id'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['id'];
	}
})(jQuery);
