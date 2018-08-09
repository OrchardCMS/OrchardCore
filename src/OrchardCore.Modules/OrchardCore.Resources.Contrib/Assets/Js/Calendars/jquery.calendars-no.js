/* http://keith-wood.name/calendars.html
   Norwegian localisation for Gregorian/Julian calendars for jQuery.
   Written by Naimdjon Takhirov (naimdjon@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['no'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Januar','Februar','Mars','April','Mai','Juni',
		'Juli','August','September','Oktober','November','Desember'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','Mai','Jun',
		'Jul','Aug','Sep','Okt','Nov','Des'],
		dayNamesShort: ['Søn','Man','Tir','Ons','Tor','Fre','Lør'],
		dayNames: ['Søndag','Mandag','Tirsdag','Onsdag','Torsdag','Fredag','Lørdag'],
		dayNamesMin: ['Sø','Ma','Ti','On','To','Fr','Lø'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['no'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['no'];
	}
})(jQuery);
