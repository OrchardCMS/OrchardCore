/* http://keith-wood.name/calendars.html
   Brazilian Portuguese localisation for Gregorian/Julian calendars for jQuery.
   Written by Leonildo Costa Silva (leocsilva@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['pt-BR'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Janeiro','Fevereiro','Março','Abril','Maio','Junho',
		'Julho','Agosto','Setembro','Outubro','Novembro','Dezembro'],
		monthNamesShort: ['Jan','Fev','Mar','Abr','Mai','Jun',
		'Jul','Ago','Set','Out','Nov','Dez'],
		dayNames: ['Domingo','Segunda-feira','Terça-feira','Quarta-feira','Quinta-feira','Sexta-feira','Sábado'],
		dayNamesShort: ['Dom','Seg','Ter','Qua','Qui','Sex','Sáb'],
		dayNamesMin: ['Dom','Seg','Ter','Qua','Qui','Sex','Sáb'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['pt-BR'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['pt-BR'];
	}
})(jQuery);
