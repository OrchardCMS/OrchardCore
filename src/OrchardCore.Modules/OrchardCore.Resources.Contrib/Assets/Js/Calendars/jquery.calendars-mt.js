/* http://keith-wood.name/calendars.html
   Maltese localisation for Gregorian/Julian calendars for jQuery.
   Written by Chritian Sciberras (uuf6429@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['mt'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Jannar','Frar','Marzu','April','Mejju','Ġunju',
		'Lulju','Awissu','Settembru','Ottubru','Novembru','Diċembru'],
		monthNamesShort: ['Jan', 'Fra', 'Mar', 'Apr', 'Mej', 'Ġun',
		'Lul', 'Awi', 'Set', 'Ott', 'Nov', 'Diċ'],
		dayNames: ['Il-Ħadd', 'It-Tnejn', 'It-Tlieta', 'L-Erbgħa', 'Il-Ħamis', 'Il-Ġimgħa', 'Is-Sibt'],
		dayNamesShort: ['Ħad', 'Tne', 'Tli', 'Erb', 'Ħam', 'Ġim', 'Sib'],
		dayNamesMin: ['Ħ','T','T','E','Ħ','Ġ','S'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['mt'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['mt'];
	}
})(jQuery);
