/* http://keith-wood.name/calendars.html
   Russian localisation for Gregorian/Julian calendars for jQuery.
   Written by Andrew Stromnov (stromnov@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ru'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Январь','Февраль','Март','Апрель','Май','Июнь',
		'Июль','Август','Сентябрь','Октябрь','Ноябрь','Декабрь'],
		monthNamesShort: ['Янв','Фев','Мар','Апр','Май','Июн',
		'Июл','Авг','Сен','Окт','Ноя','Дек'],
		dayNames: ['воскресенье','понедельник','вторник','среда','четверг','пятница','суббота'],
		dayNamesShort: ['вск','пнд','втр','срд','чтв','птн','сбт'],
		dayNamesMin: ['Вс','Пн','Вт','Ср','Чт','Пт','Сб'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ru'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ru'];
	}
})(jQuery);
