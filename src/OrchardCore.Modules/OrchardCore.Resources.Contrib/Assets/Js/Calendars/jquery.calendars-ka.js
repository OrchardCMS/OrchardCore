/* http://keith-wood.name/calendars.html
   Georgian localisation for Gregorian/Julian calendars for jQuery.
   Andrei Gorbushkin. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ka'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['იანვარი','თებერვალი','მარტი','აპრილი','მაისი','ივნისი',
		'ივლისი','აგვისტო','სექტემბერი','ოქტომბერი','ნოემბერი','დეკემბერი'],
		monthNamesShort: ['იან', 'თებ', 'მარ', 'აპრ', 'მაისი', 'ივნ',
		'ივლ', 'აგვ', 'სექ', 'ოქტ', 'ნოე', 'დეკ'],
		dayNames: ['კვირა', 'ორშაბათი', 'სამშაბათი', 'ოთხშაბათი', 'ხუთშაბათი', 'პარასკევი', 'შაბათი'],
		dayNamesShort: ['კვ', 'ორშ', 'სამ', 'ოთხ', 'ხუთ', 'პარ', 'შაბ'],
		dayNamesMin: ['კვ','ორ','სმ','ოთ', 'ხშ', 'პრ','შბ'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ka'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ka'];
	}
})(jQuery);
