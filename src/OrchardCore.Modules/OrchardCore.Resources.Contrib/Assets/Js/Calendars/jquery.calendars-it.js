/* http://keith-wood.name/calendars.html
   Italian localisation for Gregorian/Julian calendars for jQuery.
   Written by Apaella (apaella@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['it'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Gennaio','Febbraio','Marzo','Aprile','Maggio','Giugno',
		'Luglio','Agosto','Settembre','Ottobre','Novembre','Dicembre'],
		monthNamesShort: ['Gen','Feb','Mar','Apr','Mag','Giu',
		'Lug','Ago','Set','Ott','Nov','Dic'],
		dayNames: ['Domenica','Lunedì','Martedì','Mercoledì','Giovedì','Venerdì','Sabato'],
		dayNamesShort: ['Dom','Lun','Mar','Mer','Gio','Ven','Sab'],
		dayNamesMin: ['Do','Lu','Ma','Me','Gio','Ve','Sa'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['it'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['it'];
	}
})(jQuery);
