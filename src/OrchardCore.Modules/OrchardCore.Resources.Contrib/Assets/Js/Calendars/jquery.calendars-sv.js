/* http://keith-wood.name/calendars.html
   Swedish localisation for Gregorian/Julian calendars for jQuery.
   Written by Anders Ekdahl (anders@nomadiz.se). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['sv'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
        monthNames: ['Januari','Februari','Mars','April','Maj','Juni',
        'Juli','Augusti','September','Oktober','November','December'],
        monthNamesShort: ['Jan','Feb','Mar','Apr','Maj','Jun',
        'Jul','Aug','Sep','Okt','Nov','Dec'],
		dayNames: ['Söndag','Måndag','Tisdag','Onsdag','Torsdag','Fredag','Lördag'],
		dayNamesShort: ['Sön','Mån','Tis','Ons','Tor','Fre','Lör'],
		dayNamesMin: ['Sö','Må','Ti','On','To','Fr','Lö'],
        dateFormat: 'yyyy-mm-dd',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['sv'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['sv'];
	}
})(jQuery);
