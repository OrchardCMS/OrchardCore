/* http://keith-wood.name/calendars.html
   Lithuanian localisation for Gregorian/Julian calendars for jQuery.
   Arturas Paleicikas <arturas@avalon.lt>. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['lt'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Sausis','Vasaris','Kovas','Balandis','Gegužė','Birželis',
		'Liepa','Rugpjūtis','Rugsėjis','Spalis','Lapkritis','Gruodis'],
		monthNamesShort: ['Sau','Vas','Kov','Bal','Geg','Bir',
		'Lie','Rugp','Rugs','Spa','Lap','Gru'],
		dayNames: ['sekmadienis','pirmadienis','antradienis','trečiadienis','ketvirtadienis','penktadienis','šeštadienis'],
		dayNamesShort: ['sek','pir','ant','tre','ket','pen','šeš'],
		dayNamesMin: ['Se','Pr','An','Tr','Ke','Pe','Še'],
		dateFormat: 'yyyy-mm-dd',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['lt'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['lt'];
	}
})(jQuery);
