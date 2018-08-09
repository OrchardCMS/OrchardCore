/* http://keith-wood.name/timeEntry.html
   Portuguese initialisation for the jQuery time entry extension
   Written by Dino Sane (dino@asttra.com.br). */
(function($) {
	$.timeEntry.regionalOptions['pt'] = {show24Hours: true, separator: ':',
		ampmPrefix: '', ampmNames: ['AM', 'PM'],
		spinnerTexts: ['Agora', 'Campo anterior', 'Campo Seguinte', 'Aumentar', 'Diminuir']};
	$.timeEntry.setDefaults($.timeEntry.regionalOptions['pt']);
})(jQuery);
