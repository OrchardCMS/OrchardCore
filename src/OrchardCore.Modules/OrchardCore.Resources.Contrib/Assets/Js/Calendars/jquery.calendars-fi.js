/* http://keith-wood.name/calendars.html
   Finnish localisation for Gregorian/Julian calendars for jQuery.
   Written by Harri Kilpiö (harrikilpio@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['fi'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
        monthNames: ['Tammikuu','Helmikuu','Maaliskuu','Huhtikuu','Toukokuu','Kes&auml;kuu',
        'Hein&auml;kuu','Elokuu','Syyskuu','Lokakuu','Marraskuu','Joulukuu'],
        monthNamesShort: ['Tammi','Helmi','Maalis','Huhti','Touko','Kes&auml;',
        'Hein&auml;','Elo','Syys','Loka','Marras','Joulu'],
		dayNamesShort: ['Su','Ma','Ti','Ke','To','Pe','Su'],
		dayNames: ['Sunnuntai','Maanantai','Tiistai','Keskiviikko','Torstai','Perjantai','Lauantai'],
		dayNamesMin: ['Su','Ma','Ti','Ke','To','Pe','La'],
        dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['fi'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['fi'];
	}
})(jQuery);
