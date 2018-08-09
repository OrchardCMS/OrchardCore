/* http://keith-wood.name/calendars.html
   Slovak localisation for Gregorian/Julian calendars for jQuery.
   Written by Vojtech Rinik (vojto@hmm.sk). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['sk'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Január','Február','Marec','Apríl','Máj','Jún',
		'Júl','August','September','Október','November','December'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','Máj','Jún',
		'Júl','Aug','Sep','Okt','Nov','Dec'],
		dayNames: ['Nedel\'a','Pondelok','Utorok','Streda','Štvrtok','Piatok','Sobota'],
		dayNamesShort: ['Ned','Pon','Uto','Str','Štv','Pia','Sob'],
		dayNamesMin: ['Ne','Po','Ut','St','Št','Pia','So'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['sk'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['sk'];
	}
})(jQuery);
