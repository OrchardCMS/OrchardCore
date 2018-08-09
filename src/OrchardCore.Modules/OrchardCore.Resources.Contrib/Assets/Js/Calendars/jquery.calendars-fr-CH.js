/* http://keith-wood.name/calendars.html
   Swiss French localisation for Gregorian/Julian calendars for jQuery.
   Written by Martin Voelkle (martin.voelkle@e-tc.ch). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['fr-CH'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Janvier','Février','Mars','Avril','Mai','Juin',
		'Juillet','Août','Septembre','Octobre','Novembre','Décembre'],
		monthNamesShort: ['Jan','Fév','Mar','Avr','Mai','Jun',
		'Jul','Aoû','Sep','Oct','Nov','Déc'],
		dayNames: ['Dimanche','Lundi','Mardi','Mercredi','Jeudi','Vendredi','Samedi'],
		dayNamesShort: ['Dim','Lun','Mar','Mer','Jeu','Ven','Sam'],
		dayNamesMin: ['Di','Lu','Ma','Me','Je','Ve','Sa'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['fr-CH'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['fr-CH'];
	}
})(jQuery);
