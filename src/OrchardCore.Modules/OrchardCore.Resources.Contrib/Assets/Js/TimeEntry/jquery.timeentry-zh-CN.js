/* http://keith-wood.name/timeEntry.html
   Simplified Chinese initialisation for the jQuery time entry extension.
   By Cloudream(cloudream@gmail.com) */
(function($) {
	$.timeEntry.regionalOptions['zh-CN'] = {show24Hours: false, separator: ':',
		ampmPrefix: '', ampmNames: ['AM', 'PM'],
		spinnerTexts: ['当前', '左移', '右移', '加一', '减一']};
	$.timeEntry.setDefaults($.timeEntry.regionalOptions['zh-CN']);
})(jQuery);
