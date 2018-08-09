/* http://keith-wood.name/calendars.html
   Urdu localisation for Gregorian/Julian calendars for jQuery.
   Mansoor Munib -- mansoormunib@gmail.com <http://www.mansoor.co.nr/mansoor.html>
   Thanks to Habib Ahmed, ObaidUllah Anwar. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ur'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['جنوری','فروری','مارچ','اپریل','مئی','جون',
		'جولائی','اگست','ستمبر','اکتوبر','نومبر','دسمبر'],
		monthNamesShort: ['1','2','3','4','5','6',
		'7','8','9','10','11','12'],
		dayNames: ['اتوار','پير','منگل','بدھ','جمعرات','جمعہ','ہفتہ'],
		dayNamesShort: ['اتوار','پير','منگل','بدھ','جمعرات','جمعہ','ہفتہ'],
		dayNamesMin: ['اتوار','پير','منگل','بدھ','جمعرات','جمعہ','ہفتہ'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 0,
		firstDay: 1,
		isRTL: true
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ur'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ur'];
	}
})(jQuery);
