/* http://keith-wood.name/calendars.html
   Catalan localisation for Gregorian/Julian calendars for jQuery.
   Writers: (joan.leon@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ca'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Gener','Febrer','Mar&ccedil;','Abril','Maig','Juny',
		'Juliol','Agost','Setembre','Octubre','Novembre','Desembre'],
		monthNamesShort: ['Gen','Feb','Mar','Abr','Mai','Jun',
		'Jul','Ago','Set','Oct','Nov','Des'],
		dayNames: ['Diumenge','Dilluns','Dimarts','Dimecres','Dijous','Divendres','Dissabte'],
		dayNamesShort: ['Dug','Dln','Dmt','Dmc','Djs','Dvn','Dsb'],
		dayNamesMin: ['Dg','Dl','Dt','Dc','Dj','Dv','Ds'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ca'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ca'];
	}
})(jQuery);
