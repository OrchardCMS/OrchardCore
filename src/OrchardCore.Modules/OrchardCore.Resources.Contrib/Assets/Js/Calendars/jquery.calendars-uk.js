/* http://keith-wood.name/calendars.html
   Ukrainian localisation for Gregorian/Julian calendars for jQuery.
   Written by Maxim Drogobitskiy (maxdao@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['uk'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Січень','Лютий','Березень','Квітень','Травень','Червень',
		'Липень','Серпень','Вересень','Жовтень','Листопад','Грудень'],
		monthNamesShort: ['Січ','Лют','Бер','Кві','Тра','Чер',
		'Лип','Сер','Вер','Жов','Лис','Гру'],
		dayNames: ['неділя','понеділок','вівторок','середа','четвер','п\'ятниця','субота'],
		dayNamesShort: ['нед','пнд','вів','срд','чтв','птн','сбт'],
		dayNamesMin: ['Нд','Пн','Вт','Ср','Чт','Пт','Сб'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['uk'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['uk'];
	}
})(jQuery);
