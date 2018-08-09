/* http://keith-wood.name/timeEntry.html
   Catalan initialisation for the jQuery time entry extension
   Written by Gabriel Guzman (gabriel@josoft.com.ar). */
(function($) {
	$.timeEntry.regionalOptions['ca'] = {show24Hours: true, separator: ':',
		ampmPrefix: '', ampmNames: ['AM', 'PM'],
		spinnerTexts: ['Ara', 'Camp anterior', 'Següent camp', 'Augmentar', 'Disminuir']};
	$.timeEntry.setDefaults($.timeEntry.regionalOptions['ca']);
})(jQuery);
