/* http://keith-wood.name/calendars.html
   Japanese localisation for calendars datepicker for jQuery.
   Written by Kentaro SATO (kentaro@ranvis.com). */
(function($) {
	$.calendarsPicker.regionalOptions['ja'] = {
		renderer: $.extend({}, $.calendarsPicker.defaultRenderer,
			{month: $.calendarsPicker.defaultRenderer.month.
				replace(/monthHeader/, 'monthHeader:yyyy年 MM')}),
		prevText: '&#x3c;前', prevStatus: '前月を表示します',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '前年を表示します',
		nextText: '次&#x3e;', nextStatus: '翌月を表示します',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '翌年を表示します',
		currentText: '今日', currentStatus: '今月を表示します',
		todayText: '今日', todayStatus: '今月を表示します',
		clearText: 'クリア', clearStatus: '日付をクリアします',
		closeText: '閉じる', closeStatus: '変更せずに閉じます',
		yearStatus: '表示する年を変更します', monthStatus: '表示する月を変更します',
		weekText: '週', weekStatus: '暦週で第何週目かを表します',
		dayStatus: 'yyyy/mm/dd', defaultStatus: '日付を選択します',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['ja']);
})(jQuery);
