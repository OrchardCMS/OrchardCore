/* http://keith-wood.name/calendars.html
   Hindi INDIA localisation for Gregorian/Julian calendars for jQuery.
   Written by Pawan Kumar Singh. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['hi-IN'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['जनवरी',' फरवरी', 'मार्च', 'अप्रैल', 'मई', 'जून','जुलाई', 'अगस्त', 'सितम्बर', 'अक्टूबर', 'नवम्बर', 'दिसम्बर'],
		monthNamesShort: ['जन', 'फर', 'मार्च','अप्रै', 'मई', 'जून','जुलाई', 'अग', 'सित', 'अक्टू', 'नव', 'दिस'],
		dayNames: ['रविवार', 'सोमवार', 'मंगलवार', 'बुधवार', 'गुरुवार', 'शुक्रवार', 'शनिवार'],
		dayNamesShort: ['रवि', 'सोम', 'मंगल', 'बुध', 'गुरु', 'शुक्र', 'शनि'],
		dayNamesMin: ['र','सो','मं','बु','गु','शु','श'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['hi-IN'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['hi-IN'];
	}
})(jQuery);
