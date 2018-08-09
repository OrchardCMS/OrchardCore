/* http://keith-wood.name/calendars.html
   Bulgarian localisation for Gregorian/Julian calendars for jQuery.
   Written by Stoyan Kyosev (http://svest.org). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['bg'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
        monthNames: ['Януари','Февруари','Март','Април','Май','Юни',
        'Юли','Август','Септември','Октомври','Ноември','Декември'],
        monthNamesShort: ['Яну','Фев','Мар','Апр','Май','Юни',
        'Юли','Авг','Сеп','Окт','Нов','Дек'],
        dayNames: ['Неделя','Понеделник','Вторник','Сряда','Четвъртък','Петък','Събота'],
        dayNamesShort: ['Нед','Пон','Вто','Сря','Чет','Пет','Съб'],
        dayNamesMin: ['Не','По','Вт','Ср','Че','Пе','Съ'],
        dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
        isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['bg'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['bg'];
	}
})(jQuery);
