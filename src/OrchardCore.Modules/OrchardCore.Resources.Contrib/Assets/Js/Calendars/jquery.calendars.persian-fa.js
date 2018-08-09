/* http://keith-wood.name/calendars.html
   Farsi/Persian localisation for Persian calendar for jQuery v2.0.1.
   Written by Sajjad Servatjoo (sajjad.servatjoo{at}gmail.com) April 2011. */
(function($) {
	$.calendars.calendars.persian.prototype.regionalOptions['fa'] = {
		name: 'Persian',
		epochs: ['BP', 'AP'],
		monthNames: ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
		'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'],
		monthNamesShort: ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
		'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'],
		dayNames: ['يک شنبه', 'دوشنبه', 'سه شنبه', 'چهار شنبه', 'پنج شنبه', 'جمعه', 'شنبه'],
		dayNamesShort: ['يک', 'دو', 'سه', 'چهار', 'پنج', 'جمعه', 'شنبه'],
		dayNamesMin: ['ي', 'د', 'س', 'چ', 'پ', 'ج', 'ش'],
		dateFormat: 'yyyy/mm/dd',
		firstDay: 6,
		isRTL: true
	};
})(jQuery);