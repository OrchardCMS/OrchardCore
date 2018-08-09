/* http://keith-wood.name/calendars.html
   Simplified Chinese localisation for calendars datepicker for jQuery.
   Written by Cloudream (cloudream@gmail.com). */
(function($) {
	$.calendarsPicker.regionalOptions['zh-CN'] = {
		renderer: $.extend({}, $.calendarsPicker.defaultRenderer,
			{month: $.calendarsPicker.defaultRenderer.month.
				replace(/monthHeader/, 'monthHeader:MM yyyy年')}),
		prevText: '&#x3c;上月', prevStatus: '显示上月',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: '显示上一年',
		nextText: '下月&#x3e;', nextStatus: '显示下月',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: '显示下一年',
		currentText: '今天', currentStatus: '显示本月',
		todayText: '今天', todayStatus: '显示本月',
		clearText: '清除', clearStatus: '清除已选日期',
		closeText: '关闭', closeStatus: '不改变当前选择',
		yearStatus: '选择年份', monthStatus: '选择月份',
		weekText: '周', weekStatus: '年内周次',
		dayStatus: '选择 m月 d日, DD', defaultStatus: '请选择日期',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['zh-CN']);
})(jQuery);
