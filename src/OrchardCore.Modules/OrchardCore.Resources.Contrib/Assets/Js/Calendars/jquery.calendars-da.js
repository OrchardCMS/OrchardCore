/* http://keith-wood.name/calendars.html
   Danish localisation for Gregorian/Julian calendars for jQuery.
   Written by Jan Christensen ( deletestuff@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['da'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
        monthNames: ['Januar','Februar','Marts','April','Maj','Juni',
        'Juli','August','September','Oktober','November','December'],
        monthNamesShort: ['Jan','Feb','Mar','Apr','Maj','Jun',
        'Jul','Aug','Sep','Okt','Nov','Dec'],
		dayNames: ['Søndag','Mandag','Tirsdag','Onsdag','Torsdag','Fredag','Lørdag'],
		dayNamesShort: ['Søn','Man','Tir','Ons','Tor','Fre','Lør'],
		dayNamesMin: ['Sø','Ma','Ti','On','To','Fr','Lø'],
        dateFormat: 'dd-mm-yyyy',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['da'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['da'];
	}
})(jQuery);
