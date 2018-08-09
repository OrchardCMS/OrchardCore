/* http://keith-wood.name/calendars.html
   Latvian localisation for Gregorian/Julian calendars for jQuery.
   Arturas Paleicikas <arturas.paleicikas@metasite.net>. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['lv'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Janvāris','Februāris','Marts','Aprīlis','Maijs','Jūnijs',
		'Jūlijs','Augusts','Septembris','Oktobris','Novembris','Decembris'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','Mai','Jūn',
		'Jūl','Aug','Sep','Okt','Nov','Dec'],
		dayNames: ['svētdiena','pirmdiena','otrdiena','trešdiena','ceturtdiena','piektdiena','sestdiena'],
		dayNamesShort: ['svt','prm','otr','tre','ctr','pkt','sst'],
		dayNamesMin: ['Sv','Pr','Ot','Tr','Ct','Pk','Ss'],
		dateFormat: 'dd-mm-yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['lv'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['lv'];
	}
})(jQuery);
