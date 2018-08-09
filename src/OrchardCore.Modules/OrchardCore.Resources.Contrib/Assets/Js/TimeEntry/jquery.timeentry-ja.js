/* http://keith-wood.name/timeEntry.html
   Japanese initialisation for the jQuery time entry extension
   Written by Yuuki Takahashi (yuuki&#64fb69.jp) */
(function($) {
	$.timeEntry.regionalOptions['ja'] = {show24Hours: true, separator: ':',
		ampmPrefix: '', ampmNames: ['AM', 'PM'],
		spinnerTexts: ['現在時刻', '前へ', '次へ', '増やす', '減らす']};
	$.timeEntry.setDefaults($.timeEntry.regionalOptions['ja']);
})(jQuery);
