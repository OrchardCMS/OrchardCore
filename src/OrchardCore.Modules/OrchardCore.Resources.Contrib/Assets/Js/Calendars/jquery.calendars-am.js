/* http://keith-wood.name/calendars.html
   Amharic (አማርኛ) localisation for Gregorian/Julian calendars for jQuery.
   Leyu Sisay. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['am'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['ጃንዋሪ','ፈብርዋሪ','ማርች','አፕሪል','ሜይ','ጁን',
		'ጁላይ','ኦገስት','ሴፕቴምበር','ኦክቶበር','ኖቬምበር','ዲሴምበር'],
		monthNamesShort: ['ጃንዋ', 'ፈብር', 'ማርች', 'አፕሪ', 'ሜይ', 'ጁን',
		'ጁላይ', 'ኦገስ', 'ሴፕቴ', 'ኦክቶ', 'ኖቬም', 'ዲሴም'],
		dayNames: ['ሰንዴይ', 'መንዴይ', 'ትዩስዴይ', 'ዌንስዴይ', 'ተርሰዴይ', 'ፍራይዴይ', 'ሳተርዴይ'],
		dayNamesShort: ['ሰንዴ', 'መንዴ', 'ትዩስ', 'ዌንስ', 'ተርሰ', 'ፍራይ', 'ሳተር'],
		dayNamesMin: ['ሰን', 'መን', 'ትዩ', 'ዌን', 'ተር', 'ፍራ', 'ሳተ'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['am'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['am'];
	}
})(jQuery);
