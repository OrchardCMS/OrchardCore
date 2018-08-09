/* http://keith-wood.name/calendars.html
   Farsi/Persian localisation for calendars datepicker for jQuery.
   Javad Mowlanezhad -- jmowla@gmail.com. */
(function($) {
	$.calendarsPicker.regionalOptions['fa'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '&#x3c;قبلي', prevStatus: 'نمايش ماه قبل',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '',
		nextText: 'بعدي&#x3e;', nextStatus: 'نمايش ماه بعد',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '',
		currentText: 'امروز', currentStatus: 'نمايش ماه جاري',
		todayText: 'امروز', todayStatus: 'نمايش ماه جاري',
		clearText: 'حذف تاريخ', clearStatus: 'پاک کردن تاريخ جاري',
		closeText: 'بستن', closeStatus: 'بستن بدون اعمال تغييرات',
		yearStatus: 'نمايش سال متفاوت', monthStatus: 'نمايش ماه متفاوت',
		weekText: 'هف', weekStatus: 'هفتهِ سال',
		dayStatus: 'انتخاب D, M d', defaultStatus: 'انتخاب تاريخ',
		isRTL: true
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['fa']);
})(jQuery);
