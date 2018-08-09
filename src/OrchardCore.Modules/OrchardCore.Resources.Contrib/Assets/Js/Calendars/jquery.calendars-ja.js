/* http://keith-wood.name/calendars.html
   Japanese localisation for Gregorian/Julian calendars for jQuery.
   Written by Kentaro SATO (kentaro@ranvis.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ja'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['1月','2月','3月','4月','5月','6月',
		'7月','8月','9月','10月','11月','12月'],
		monthNamesShort: ['1月','2月','3月','4月','5月','6月',
		'7月','8月','9月','10月','11月','12月'],
		dayNames: ['日曜日','月曜日','火曜日','水曜日','木曜日','金曜日','土曜日'],
		dayNamesShort: ['日','月','火','水','木','金','土'],
		dayNamesMin: ['日','月','火','水','木','金','土'],
		dateFormat: 'yyyy/mm/dd',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ja'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ja'];
	}
})(jQuery);
