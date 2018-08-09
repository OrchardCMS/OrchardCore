/* http://keith-wood.name/calendars.html
   Azerbaijani localisation for Gregorian/Julian calendars for jQuery.
   Written by Jamil Najafov (necefov33@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['az'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Yanvar','Fevral','Mart','Aprel','May','İyun',
		'İyul','Avqust','Sentyabr','Oktyabr','Noyabr','Dekabr'],
		monthNamesShort: ['Yan','Fev','Mar','Apr','May','İyun',
		'İyul','Avq','Sen','Okt','Noy','Dek'],
		dayNames: ['Bazar','Bazar ertəsi','Çərşənbə axşamı','Çərşənbə','Cümə axşamı','Cümə','Şənbə'],
		dayNamesShort: ['B','Be','Ça','Ç','Ca','C','Ş'],
		dayNamesMin: ['B','B','Ç','С','Ç','C','Ş'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['az'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['az'];
	}
})(jQuery);
