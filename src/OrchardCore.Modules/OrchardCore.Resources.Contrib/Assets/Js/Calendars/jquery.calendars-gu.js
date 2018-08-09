/* http://keith-wood.name/calendars.html
   Gujarati (ગુજરાતી) localisation for Gregorian/Julian calendars for jQuery.
   Naymesh Mistry (naymesh@yahoo.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['gu'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['જાન્યુઆરી','ફેબ્રુઆરી','માર્ચ','એપ્રિલ','મે','જૂન',
		'જુલાઈ','ઑગસ્ટ','સપ્ટેમ્બર','ઑક્ટોબર','નવેમ્બર','ડિસેમ્બર'],
		monthNamesShort: ['જાન્યુ','ફેબ્રુ','માર્ચ','એપ્રિલ','મે','જૂન',
		'જુલાઈ','ઑગસ્ટ','સપ્ટે','ઑક્ટો','નવે','ડિસે'],
		dayNames: ['રવિવાર','સોમવાર','મંગળવાર','બુધવાર','ગુરુવાર','શુક્રવાર','શનિવાર'],
		dayNamesShort: ['રવિ','સોમ','મંગળ','બુધ','ગુરુ','શુક્ર','શનિ'],
		dayNamesMin: ['ર','સો','મં','બુ','ગુ','શુ','શ'],
		dateFormat: 'dd-M-yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['gu'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['gu'];
	}
})(jQuery);
