/* http://keith-wood.name/calendars.html
   Malaysian localisation for calendars datepicker for jQuery.
   Written by Mohd Nawawi Mohamad Jamili (nawawi@ronggeng.net). */
(function($) {
	$.calendarsPicker.regionalOptions['ms'] = {
		renderer: $.calendarsPicker.defaultRenderer,
		prevText: '&#x3c;Sebelum', prevStatus: 'Tunjukkan bulan lepas',
		prevJumpText: '&#x3c;&#x3c;', prevJumpStatus: 'Tunjukkan tahun lepas',
		nextText: 'Selepas&#x3e;', nextStatus: 'Tunjukkan bulan depan',
		nextJumpText: '&#x3e;&#x3e;', nextJumpStatus: 'Tunjukkan tahun depan',
		currentText: 'hari ini', currentStatus: 'Tunjukkan bulan terkini',
		todayText: 'hari ini', todayStatus: 'Tunjukkan bulan terkini',
		clearText: 'Padam', clearStatus: 'Padamkan tarikh terkini',
		closeText: 'Tutup', closeStatus: 'Tutup tanpa perubahan',
		yearStatus: 'Tunjukkan tahun yang lain', monthStatus: 'Tunjukkan bulan yang lain',
		weekText: 'Mg', weekStatus: 'Minggu bagi tahun ini',
		dayStatus: 'DD, d MM', defaultStatus: 'Sila pilih tarikh',
		isRTL: false
	};
	$.calendarsPicker.setDefaults($.calendarsPicker.regionalOptions['ms']);
})(jQuery);
