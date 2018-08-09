/* http://keith-wood.name/calendars.html
   Tatar localisation for calendars datepicker for jQuery.
   Written by Irek Khaziev (khazirek@gmail.com). */
(function($) {
	$.calendarsPicker.regionalOptions['tt'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: 'Алдагы',  prevStatus: 'Алдагы айны күрсәтү',
		prevJumpText: '&lt;&lt;', prevJumpStatus: 'Алдагы елны күрсәтү',
		nextText: 'Киләсе', nextStatus: 'Киләсе айны күрсәтү',
		nextJumpText: '&gt;&gt;', nextJumpStatus: 'Киләсе елны күрсәтү',
		currentText: 'Хәзер', currentStatus: 'Хәзерге айны күрсәтү',
		todayText: 'Бүген', todayStatus: 'Бүгенге айны күрсәтү',
		clearText: 'Чистарту', clearStatus: 'Барлык көннәрне чистарту',
		closeText: 'Ябарга', closeStatus: 'Көн сайлауны ябарга',
		yearStatus: 'Елны кертегез', monthStatus: 'Айны кертегез',
		weekText: 'Атна', weekStatus: 'Елда атна саны',
		dayStatus: 'DD, M d', defaultStatus: 'Көнне сайлагыз',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['tt']);
})(jQuery);
