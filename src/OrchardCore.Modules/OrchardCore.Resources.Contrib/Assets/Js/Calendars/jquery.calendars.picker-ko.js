/* http://keith-wood.name/calendars.html
   Korean localisation for calendars datepicker for jQuery.
   Written by DaeKwon Kang (ncrash.dk@gmail.com), Edited by Genie. */
(function($) {
	$.calendarsPicker.regionalOptions['ko'] = {
		renderer: $.extend({}, $.calendarsPicker.defaultRenderer,
			{month: $.calendarsPicker.defaultRenderer.month.
				replace(/monthHeader/, 'monthHeader:yyyy년 MM')}),
		prevText: '이전달', prevStatus: '이전달을 표시합니다',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '이전 연도를 표시합니다',
		nextText: '다음달', nextStatus: '다음달을 표시합니다',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '다음 연도를 표시합니다',
		currentText: '현재', currentStatus: '입력한 달을 표시합니다',
		todayText: '오늘', todayStatus: '이번달을 표시합니다',
		clearText: '지우기', clearStatus: '입력한 날짜를 지웁니다',
		closeText: '닫기', closeStatus: '',
		yearStatus: '표시할 연도를 변경합니다', monthStatus: '표시할 월을 변경합니다',
		weekText: 'Wk', weekStatus: '해당 연도의 주차',
		dayStatus: 'M d일 (D)', defaultStatus: '날짜를 선택하세요',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['ko']);
})(jQuery);
