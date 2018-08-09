/* http://keith-wood.name/calendars.html
   Calendars localisations for jQuery v2.0.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) August 2009.
   Available under the MIT (http://keith-wood.name/licence.html) license. 
   Please attribute the author if you use it. */
/* http://keith-wood.name/calendars.html
   Afrikaans localisation for Gregorian/Julian calendars for jQuery.
   Written by Renier Pretorius and Ruediger Thiede. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['af'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Januarie','Februarie','Maart','April','Mei','Junie',
		'Julie','Augustus','September','Oktober','November','Desember'],
		monthNamesShort: ['Jan', 'Feb', 'Mrt', 'Apr', 'Mei', 'Jun',
		'Jul', 'Aug', 'Sep', 'Okt', 'Nov', 'Des'],
		dayNames: ['Sondag', 'Maandag', 'Dinsdag', 'Woensdag', 'Donderdag', 'Vrydag', 'Saterdag'],
		dayNamesShort: ['Son', 'Maan', 'Dins', 'Woens', 'Don', 'Vry', 'Sat'],
		dayNamesMin: ['So','Ma','Di','Wo','Do','Vr','Sa'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['af'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['af'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Amharic (አማርኛ) localisation for Gregorian/Julian calendars for jQuery.
   Leyu Sisay. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['am'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['ጃንዋሪ','ፈብርዋሪ','ማርች','አፕሪል','ሜይ','ጁን',
		'ጁላይ','ኦገስት','ሴፕቴምበር','ኦክቶበር','ኖቬምበር','ዲሴምበር'],
		monthNamesShort: ['ጃንዋ', 'ፈብር', 'ማርች', 'አፕሪ', 'ሜይ', 'ጁን',
		'ጁላይ', 'ኦገስ', 'ሴፕቴ', 'ኦክቶ', 'ኖቬም', 'ዲሴም'],
		dayNames: ['ሰንዴይ', 'መንዴይ', 'ትዩስዴይ', 'ዌንስዴይ', 'ተርሰዴይ', 'ፍራይዴይ', 'ሳተርዴይ'],
		dayNamesShort: ['ሰንዴ', 'መንዴ', 'ትዩስ', 'ዌንስ', 'ተርሰ', 'ፍራይ', 'ሳተር'],
		dayNamesMin: ['ሰን', 'መን', 'ትዩ', 'ዌን', 'ተር', 'ፍራ', 'ሳተ'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['am'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['am'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Algerian (and Tunisian) Arabic localisation for Gregorian/Julian calendars for jQuery.
   Mohamed Cherif BOUCHELAGHEM -- cherifbouchelaghem@yahoo.fr */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ar-DZ'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['جانفي', 'فيفري', 'مارس', 'أفريل', 'ماي', 'جوان',
		'جويلية', 'أوت', 'سبتمبر','أكتوبر', 'نوفمبر', 'ديسمبر'],
		monthNamesShort: ['1', '2', '3', '4', '5', '6', '7', '8', '9', '10', '11', '12'],
		dayNames: ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'],
		dayNamesShort: ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'],
		dayNamesMin: ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 6,
		isRTL: true
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ar-DZ'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ar-DZ'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Arabic localisation for Gregorian/Julian calendars for jQuery.
   Mahmoud Khaled -- mahmoud.khaled@badrit.com
   NOTE: monthNames are the new months names */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ar-EG'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['يناير', 'فبراير', 'مارس', 'إبريل', 'مايو', 'يونية',
		'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'],
		monthNamesShort: ['1', '2', '3', '4', '5', '6', '7', '8', '9', '10', '11', '12'],
		dayNames:  ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'],
		dayNamesShort: ['أحد', 'اثنين', 'ثلاثاء', 'أربعاء', 'خميس', 'جمعة', 'سبت'],
		dayNamesMin: ['أحد', 'اثنين', 'ثلاثاء', 'أربعاء', 'خميس', 'جمعة', 'سبت'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 6,
		isRTL: true
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ar-EG'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ar-EG'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Arabic localisation for Gregorian/Julian calendars for jQuery.
   Khaled Al Horani -- خالد الحوراني -- koko.dw@gmail.com. */
/* NOTE: monthNames are the original months names and they are the Arabic names,
   not the new months name فبراير - يناير and there isn't any Arabic roots for these months */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ar'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['كانون الثاني', 'شباط', 'آذار', 'نيسان', 'آذار', 'حزيران',
		'تموز', 'آب', 'أيلول', 'تشرين الأول', 'تشرين الثاني', 'كانون الأول'],
		monthNamesShort: ['1', '2', '3', '4', '5', '6', '7', '8', '9', '10', '11', '12'],
		dayNames: ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'],
		dayNamesShort: ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'],
		dayNamesMin: ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 6,
		isRTL: true
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ar'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ar'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Azerbaijani localisation for Gregorian/Julian calendars for jQuery.
   Written by Jamil Najafov (necefov33@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['az'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Yanvar','Fevral','Mart','Aprel','May','İyun',
		'İyul','Avqust','Sentyabr','Oktyabr','Noyabr','Dekabr'],
		monthNamesShort: ['Yan','Fev','Mar','Apr','May','İyun',
		'İyul','Avq','Sen','Okt','Noy','Dek'],
		dayNames: ['Bazar','Bazar ertəsi','Çərşənbə axşamı','Çərşənbə','Cümə axşamı','Cümə','Şənbə'],
		dayNamesShort: ['B','Be','Ça','Ç','Ca','C','Ş'],
		dayNamesMin: ['B','B','Ç','С','Ç','C','Ş'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['az'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['az'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Bulgarian localisation for Gregorian/Julian calendars for jQuery.
   Written by Stoyan Kyosev (http://svest.org). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['bg'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
        monthNames: ['Януари','Февруари','Март','Април','Май','Юни',
        'Юли','Август','Септември','Октомври','Ноември','Декември'],
        monthNamesShort: ['Яну','Фев','Мар','Апр','Май','Юни',
        'Юли','Авг','Сеп','Окт','Нов','Дек'],
        dayNames: ['Неделя','Понеделник','Вторник','Сряда','Четвъртък','Петък','Събота'],
        dayNamesShort: ['Нед','Пон','Вто','Сря','Чет','Пет','Съб'],
        dayNamesMin: ['Не','По','Вт','Ср','Че','Пе','Съ'],
        dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
        isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['bg'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['bg'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Bosnian localisation for Gregorian/Julian calendars for jQuery.
   Kenan Konjo. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['bs'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Januar','Februar','Mart','April','Maj','Juni',
		'Juli','August','Septembar','Oktobar','Novembar','Decembar'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','Maj','Jun',
		'Jul','Aug','Sep','Okt','Nov','Dec'],
		dayNames: ['Nedelja','Ponedeljak','Utorak','Srijeda','Četvrtak','Petak','Subota'],
		dayNamesShort: ['Ned','Pon','Uto','Sri','Čet','Pet','Sub'],
		dayNamesMin: ['Ne','Po','Ut','Sr','Če','Pe','Su'],
		dateFormat: 'dd.mm.yy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['bs'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['bs'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
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
﻿/* http://keith-wood.name/calendars.html
   Czech localisation for Gregorian/Julian calendars for jQuery.
   Written by Tomas Muller (tomas@tomas-muller.net). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['cs'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['leden','únor','březen','duben','květen','červen',
        'červenec','srpen','září','říjen','listopad','prosinec'],
		monthNamesShort: ['led','úno','bře','dub','kvě','čer',
		'čvc','srp','zář','říj','lis','pro'],
		dayNames: ['neděle', 'pondělí', 'úterý', 'středa', 'čtvrtek', 'pátek', 'sobota'],
		dayNamesShort: ['ne', 'po', 'út', 'st', 'čt', 'pá', 'so'],
		dayNamesMin: ['ne','po','út','st','čt','pá','so'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['cs'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['cs'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Danish localisation for Gregorian/Julian calendars for jQuery.
   Written by Jan Christensen ( deletestuff@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['da'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
        monthNames: ['Januar','Februar','Marts','April','Maj','Juni',
        'Juli','August','September','Oktober','November','December'],
        monthNamesShort: ['Jan','Feb','Mar','Apr','Maj','Jun',
        'Jul','Aug','Sep','Okt','Nov','Dec'],
		dayNames: ['Søndag','Mandag','Tirsdag','Onsdag','Torsdag','Fredag','Lørdag'],
		dayNamesShort: ['Søn','Man','Tir','Ons','Tor','Fre','Lør'],
		dayNamesMin: ['Sø','Ma','Ti','On','To','Fr','Lø'],
        dateFormat: 'dd-mm-yyyy',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['da'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['da'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Swiss-German localisation for Gregorian/Julian calendars for jQuery.
   Written by Douglas Jose & Juerg Meier. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['de-CH'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Januar','Februar','März','April','Mai','Juni',
		'Juli','August','September','Oktober','November','Dezember'],
		monthNamesShort: ['Jan','Feb','Mär','Apr','Mai','Jun',
		'Jul','Aug','Sep','Okt','Nov','Dez'],
		dayNames: ['Sonntag','Montag','Dienstag','Mittwoch','Donnerstag','Freitag','Samstag'],
		dayNamesShort: ['So','Mo','Di','Mi','Do','Fr','Sa'],
		dayNamesMin: ['So','Mo','Di','Mi','Do','Fr','Sa'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['de-CH'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['de-CH'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   German localisation for Gregorian/Julian calendars for jQuery.
   Written by Milian Wolff (mail@milianw.de). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['de'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Januar','Februar','März','April','Mai','Juni',
		'Juli','August','September','Oktober','November','Dezember'],
		monthNamesShort: ['Jan','Feb','Mär','Apr','Mai','Jun',
		'Jul','Aug','Sep','Okt','Nov','Dez'],
		dayNames: ['Sonntag','Montag','Dienstag','Mittwoch','Donnerstag','Freitag','Samstag'],
		dayNamesShort: ['So','Mo','Di','Mi','Do','Fr','Sa'],
		dayNamesMin: ['So','Mo','Di','Mi','Do','Fr','Sa'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['de'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['de'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Greek localisation for Gregorian/Julian calendars for jQuery.
   Written by Alex Cicovic (http://www.alexcicovic.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['el'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Ιανουάριος','Φεβρουάριος','Μάρτιος','Απρίλιος','Μάιος','Ιούνιος',
		'Ιούλιος','Αύγουστος','Σεπτέμβριος','Οκτώβριος','Νοέμβριος','Δεκέμβριος'],
		monthNamesShort: ['Ιαν','Φεβ','Μαρ','Απρ','Μαι','Ιουν',
		'Ιουλ','Αυγ','Σεπ','Οκτ','Νοε','Δεκ'],
		dayNames: ['Κυριακή','Δευτέρα','Τρίτη','Τετάρτη','Πέμπτη','Παρασκευή','Σάββατο'],
		dayNamesShort: ['Κυρ','Δευ','Τρι','Τετ','Πεμ','Παρ','Σαβ'],
		dayNamesMin: ['Κυ','Δε','Τρ','Τε','Πε','Πα','Σα'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['el'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['el'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   English/Australia localisation for Gregorian/Julian calendars for jQuery.
   Based on en-GB. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['en-AU'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['January','February','March','April','May','June',
		'July','August','September','October','November','December'],
		monthNamesShort: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
		'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
		dayNames: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
		dayNamesShort: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
		dayNamesMin: ['Su','Mo','Tu','We','Th','Fr','Sa'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['en-AU'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['en-AU'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   English/UK localisation for Gregorian/Julian calendars for jQuery.
   Stuart. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['en-GB'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['January','February','March','April','May','June',
		'July','August','September','October','November','December'],
		monthNamesShort: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
		'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
		dayNames: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
		dayNamesShort: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
		dayNamesMin: ['Su','Mo','Tu','We','Th','Fr','Sa'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['en-GB'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['en-GB'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   English/New Zealand localisation for Gregorian/Julian calendars for jQuery.
   Based on en-GB. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['en-NZ'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['January','February','March','April','May','June',
		'July','August','September','October','November','December'],
		monthNamesShort: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
		'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
		dayNames: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
		dayNamesShort: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
		dayNamesMin: ['Su','Mo','Tu','We','Th','Fr','Sa'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['en-NZ'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['en-NZ'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Esperanto localisation for Gregorian/Julian calendars for jQuery.
   Written by Olivier M. (olivierweb@ifrance.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['eo'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Januaro','Februaro','Marto','Aprilo','Majo','Junio',
		'Julio','Aŭgusto','Septembro','Oktobro','Novembro','Decembro'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','Maj','Jun',
		'Jul','Aŭg','Sep','Okt','Nov','Dec'],
		dayNames: ['Dimanĉo','Lundo','Mardo','Merkredo','Ĵaŭdo','Vendredo','Sabato'],
		dayNamesShort: ['Dim','Lun','Mar','Mer','Ĵaŭ','Ven','Sab'],
		dayNamesMin: ['Di','Lu','Ma','Me','Ĵa','Ve','Sa'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['eo'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['eo'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Spanish/Argentina localisation for Gregorian/Julian calendars for jQuery.
   Written by Esteban Acosta Villafane (esteban.acosta@globant.com) of Globant (http://www.globant.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['es-AR'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Enero','Febrero','Marzo','Abril','Mayo','Junio',
		'Julio','Agosto','Septiembre','Octubre','Noviembre','Diciembre'],
		monthNamesShort: ['Ene','Feb','Mar','Abr','May','Jun',
		'Jul','Ago','Sep','Oct','Nov','Dic'],
		dayNames: ['Domingo','Lunes','Martes','Miércoles','Jueves','Viernes','Sábado'],
		dayNamesShort: ['Dom','Lun','Mar','Mié','Juv','Vie','Sáb'],
		dayNamesMin: ['Do','Lu','Ma','Mi','Ju','Vi','Sá'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['es-AR'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['es-AR'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Spanish/Perú localisation for Gregorian/Julian calendars for jQuery.
   Written by Fischer Tirado (fishdev@globant.com) of ASIX (http://www.asixonline.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['es-PE'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Enero','Febrero','Marzo','Abril','Mayo','Junio',
		'Julio','Agosto','Septiembre','Octubre','Noviembre','Diciembre'],
		monthNamesShort: ['Ene','Feb','Mar','Abr','May','Jun',
		'Jul','Ago','Sep','Oct','Nov','Dic'],
		dayNames: ['Domingo','Lunes','Martes','Miércoles','Jueves','Viernes','Sábado'],
		dayNamesShort: ['Dom','Lun','Mar','Mié','Jue','Vie','Sab'],
		dayNamesMin: ['Do','Lu','Ma','Mi','Ju','Vi','Sa'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['es-PE'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['es-PE'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Spanish localisation for Gregorian/Julian calendars for jQuery.
   Traducido por Vester (xvester@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['es'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Enero','Febrero','Marzo','Abril','Mayo','Junio',
		'Julio','Agosto','Septiembre','Octubre','Noviembre','Diciembre'],
		monthNamesShort: ['Ene','Feb','Mar','Abr','May','Jun',
		'Jul','Ago','Sep','Oct','Nov','Dic'],
		dayNames: ['Domingo','Lunes','Martes','Miércoles','Jueves','Viernes','Sábado'],
		dayNamesShort: ['Dom','Lun','Mar','Mié','Juv','Vie','Sáb'],
		dayNamesMin: ['Do','Lu','Ma','Mi','Ju','Vi','Sá'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['es'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['es'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Estonian localisation for Gregorian/Julian calendars for jQuery.
   Written by Mart Sõmermaa (mrts.pydev at gmail com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['et'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Jaanuar','Veebruar','Märts','Aprill','Mai','Juuni', 
			'Juuli','August','September','Oktoober','November','Detsember'],
		monthNamesShort: ['Jaan', 'Veebr', 'Märts', 'Apr', 'Mai', 'Juuni',
			'Juuli', 'Aug', 'Sept', 'Okt', 'Nov', 'Dets'],
		dayNames: ['Pühapäev', 'Esmaspäev', 'Teisipäev', 'Kolmapäev', 'Neljapäev', 'Reede', 'Laupäev'],
		dayNamesShort: ['Pühap', 'Esmasp', 'Teisip', 'Kolmap', 'Neljap', 'Reede', 'Laup'],
		dayNamesMin: ['P','E','T','K','N','R','L'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['et'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['et'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Basque localisation for Gregorian/Julian calendars for jQuery.
   Karrikas-ek itzulia (karrikas@karrikas.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['eu'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Urtarrila','Otsaila','Martxoa','Apirila','Maiatza','Ekaina',
		'Uztaila','Abuztua','Iraila','Urria','Azaroa','Abendua'],
		monthNamesShort: ['Urt','Ots','Mar','Api','Mai','Eka',
		'Uzt','Abu','Ira','Urr','Aza','Abe'],
		dayNames: ['Igandea','Astelehena','Asteartea','Asteazkena','Osteguna','Ostirala','Larunbata'],
		dayNamesShort: ['Iga','Ast','Ast','Ast','Ost','Ost','Lar'],
		dayNamesMin: ['Ig','As','As','As','Os','Os','La'],
		dateFormat: 'yyyy/mm/dd',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['eu'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['eu'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Farsi/Persian localisation for Gregorian/Julian calendars for jQuery.
   Javad Mowlanezhad -- jmowla@gmail.com */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['fa'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['فروردين','ارديبهشت','خرداد','تير','مرداد','شهريور',
		'مهر','آبان','آذر','دي','بهمن','اسفند'],
		monthNamesShort: ['1','2','3','4','5','6','7','8','9','10','11','12'],
		dayNames: ['يکشنبه','دوشنبه','سه‌شنبه','چهارشنبه','پنجشنبه','جمعه','شنبه'],
		dayNamesShort: ['ي','د','س','چ','پ','ج', 'ش'],
		dayNamesMin: ['ي','د','س','چ','پ','ج', 'ش'],
		dateFormat: 'yyyy/mm/dd',
		firstDay: 6,
		isRTL: true
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['fa'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['fa'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Finnish localisation for Gregorian/Julian calendars for jQuery.
   Written by Harri Kilpiö (harrikilpio@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['fi'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
        monthNames: ['Tammikuu','Helmikuu','Maaliskuu','Huhtikuu','Toukokuu','Kes&auml;kuu',
        'Hein&auml;kuu','Elokuu','Syyskuu','Lokakuu','Marraskuu','Joulukuu'],
        monthNamesShort: ['Tammi','Helmi','Maalis','Huhti','Touko','Kes&auml;',
        'Hein&auml;','Elo','Syys','Loka','Marras','Joulu'],
		dayNamesShort: ['Su','Ma','Ti','Ke','To','Pe','Su'],
		dayNames: ['Sunnuntai','Maanantai','Tiistai','Keskiviikko','Torstai','Perjantai','Lauantai'],
		dayNamesMin: ['Su','Ma','Ti','Ke','To','Pe','La'],
        dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['fi'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['fi'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Faroese localisation for Gregorian/Julian calendars for jQuery.
   Written by Sverri Mohr Olsen, sverrimo@gmail.com */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['fo'] = {
		name: 'Gregorianskur',
		epochs: ['BCE', 'CE'],
		monthNames: ['Januar','Februar','Mars','Apríl','Mei','Juni',
		'Juli','August','September','Oktober','November','Desember'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','Mei','Jun',
		'Jul','Aug','Sep','Okt','Nov','Des'],
		dayNames: ['Sunnudagur','Mánadagur','Týsdagur','Mikudagur','Hósdagur','Fríggjadagur','Leyardagur'],
		dayNamesShort: ['Sun','Mán','Týs','Mik','Hós','Frí','Ley'],
		dayNamesMin: ['Su','Má','Tý','Mi','Hó','Fr','Le'],
		dateFormat: 'dd-mm-yyyy',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['fo'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['fo'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
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
﻿/* http://keith-wood.name/calendars.html
   French localisation for Gregorian/Julian calendars for jQuery.
   Stéphane Nahmani (sholby@sholby.net). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['fr'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Janvier','Février','Mars','Avril','Mai','Juin',
		'Juillet','Août','Septembre','Octobre','Novembre','Décembre'],
		monthNamesShort: ['Jan','Fév','Mar','Avr','Mai','Jun',
		'Jul','Aoû','Sep','Oct','Nov','Déc'],
		dayNames: ['Dimanche','Lundi','Mardi','Mercredi','Jeudi','Vendredi','Samedi'],
		dayNamesShort: ['Dim','Lun','Mar','Mer','Jeu','Ven','Sam'],
		dayNamesMin: ['Di','Lu','Ma','Me','Je','Ve','Sa'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['fr'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['fr'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Iniciacion en galego para a extensión 'UI date picker' para jQuery.
   Traducido por Manuel (McNuel@gmx.net). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['gl'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Xaneiro','Febreiro','Marzo','Abril','Maio','Xuño',
		'Xullo','Agosto','Setembro','Outubro','Novembro','Decembro'],
		monthNamesShort: ['Xan','Feb','Mar','Abr','Mai','Xuñ',
		'Xul','Ago','Set','Out','Nov','Dec'],
		dayNames: ['Domingo','Luns','Martes','Mércores','Xoves','Venres','Sábado'],
		dayNamesShort: ['Dom','Lun','Mar','Mér','Xov','Ven','Sáb'],
		dayNamesMin: ['Do','Lu','Ma','Me','Xo','Ve','Sá'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['gl'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['gl'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Gujarati (ગુજરાતી) localisation for Gregorian/Julian calendars for jQuery.
   Naymesh Mistry (naymesh@yahoo.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['gu'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['જાન્યુઆરી','ફેબ્રુઆરી','માર્ચ','એપ્રિલ','મે','જૂન',
		'જુલાઈ','ઑગસ્ટ','સપ્ટેમ્બર','ઑક્ટોબર','નવેમ્બર','ડિસેમ્બર'],
		monthNamesShort: ['જાન્યુ','ફેબ્રુ','માર્ચ','એપ્રિલ','મે','જૂન',
		'જુલાઈ','ઑગસ્ટ','સપ્ટે','ઑક્ટો','નવે','ડિસે'],
		dayNames: ['રવિવાર','સોમવાર','મંગળવાર','બુધવાર','ગુરુવાર','શુક્રવાર','શનિવાર'],
		dayNamesShort: ['રવિ','સોમ','મંગળ','બુધ','ગુરુ','શુક્ર','શનિ'],
		dayNamesMin: ['ર','સો','મં','બુ','ગુ','શુ','શ'],
		dateFormat: 'dd-M-yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['gu'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['gu'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Hebrew localisation for Gregorian/Julian calendars for jQuery.
   Written by Amir Hardon (ahardon at gmail dot com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['he'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['ינואר','פברואר','מרץ','אפריל','מאי','יוני',
		'יולי','אוגוסט','ספטמבר','אוקטובר','נובמבר','דצמבר'],
		monthNamesShort: ['1','2','3','4','5','6',
		'7','8','9','10','11','12'],
		dayNames: ['ראשון','שני','שלישי','רביעי','חמישי','שישי','שבת'],
		dayNamesShort: ['א\'','ב\'','ג\'','ד\'','ה\'','ו\'','שבת'],
		dayNamesMin: ['א\'','ב\'','ג\'','ד\'','ה\'','ו\'','שבת'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 0,
		isRTL: true
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['he'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['he'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
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
﻿/* http://keith-wood.name/calendars.html
   Croatian localisation for Gregorian/Julian calendars for jQuery.
   Written by Vjekoslav Nesek. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['hr'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Siječanj','Veljača','Ožujak','Travanj','Svibanj','Lipanj',
		'Srpanj','Kolovoz','Rujan','Listopad','Studeni','Prosinac'],
		monthNamesShort: ['Sij','Velj','Ožu','Tra','Svi','Lip',
		'Srp','Kol','Ruj','Lis','Stu','Pro'],
		dayNames: ['Nedjelja','Ponedjeljak','Utorak','Srijeda','Četvrtak','Petak','Subota'],
		dayNamesShort: ['Ned','Pon','Uto','Sri','Čet','Pet','Sub'],
		dayNamesMin: ['Ne','Po','Ut','Sr','Če','Pe','Su'],
		dateFormat: 'dd.mm.yyyy.',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['hr'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['hr'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Hungarian localisation for Gregorian/Julian calendars for jQuery.
   Written by Istvan Karaszi (jquerycalendar@spam.raszi.hu). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['hu'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Január', 'Február', 'Március', 'Április', 'Május', 'Június',
		'Július', 'Augusztus', 'Szeptember', 'Október', 'November', 'December'],
		monthNamesShort: ['Jan', 'Feb', 'Már', 'Ápr', 'Máj', 'Jún',
		'Júl', 'Aug', 'Szep', 'Okt', 'Nov', 'Dec'],
		dayNames: ['Vasárnap', 'Hétfö', 'Kedd', 'Szerda', 'Csütörtök', 'Péntek', 'Szombat'],
		dayNamesShort: ['Vas', 'Hét', 'Ked', 'Sze', 'Csü', 'Pén', 'Szo'],
		dayNamesMin: ['V', 'H', 'K', 'Sze', 'Cs', 'P', 'Szo'],
		dateFormat: 'yyyy-mm-dd',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['hu'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['hu'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Armenian localisation for Gregorian/Julian calendars for jQuery.
   Written by Levon Zakaryan (levon.zakaryan@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['hy'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Հունվար','Փետրվար','Մարտ','Ապրիլ','Մայիս','Հունիս',
		'Հուլիս','Օգոստոս','Սեպտեմբեր','Հոկտեմբեր','Նոյեմբեր','Դեկտեմբեր'],
		monthNamesShort: ['Հունվ','Փետր','Մարտ','Ապր','Մայիս','Հունիս',
		'Հուլ','Օգս','Սեպ','Հոկ','Նոյ','Դեկ'],
		dayNames: ['կիրակի','եկուշաբթի','երեքշաբթի','չորեքշաբթի','հինգշաբթի','ուրբաթ','շաբաթ'],
		dayNamesShort: ['կիր','երկ','երք','չրք','հնգ','ուրբ','շբթ'],
		dayNamesMin: ['կիր','երկ','երք','չրք','հնգ','ուրբ','շբթ'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['hy'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['hy'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Indonesian localisation for Gregorian/Julian calendars for jQuery.
   Written by Deden Fathurahman (dedenf@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['id'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Januari','Februari','Maret','April','Mei','Juni',
		'Juli','Agustus','September','Oktober','Nopember','Desember'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','Mei','Jun',
		'Jul','Agus','Sep','Okt','Nop','Des'],
		dayNames: ['Minggu','Senin','Selasa','Rabu','Kamis','Jumat','Sabtu'],
		dayNamesShort: ['Min','Sen','Sel','Rab','kam','Jum','Sab'],
		dayNamesMin: ['Mg','Sn','Sl','Rb','Km','jm','Sb'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['id'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['id'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Icelandic localisation for Gregorian/Julian calendars for jQuery.
   Written by Haukur H. Thorsson (haukur@eskill.is). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['is'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Janúar','Febrúar','Mars','Apríl','Maí','Júní',
		'Júlí','Ágúst','September','Október','Nóvember','Desember'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','Maí','Jún',
		'Júl','Ágú','Sep','Okt','Nóv','Des'],
		dayNames: ['Sunnudagur','Mánudagur','Þriðjudagur','Miðvikudagur','Fimmtudagur','Föstudagur','Laugardagur'],
		dayNamesShort: ['Sun','Mán','Þri','Mið','Fim','Fös','Lau'],
		dayNamesMin: ['Su','Má','Þr','Mi','Fi','Fö','La'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['is'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['is'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
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
﻿/* http://keith-wood.name/calendars.html
   Japanese localisation for Gregorian/Julian calendars for jQuery.
   Written by Kentaro SATO (kentaro@ranvis.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ja'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['1月','2月','3月','4月','5月','6月',
		'7月','8月','9月','10月','11月','12月'],
		monthNamesShort: ['1月','2月','3月','4月','5月','6月',
		'7月','8月','9月','10月','11月','12月'],
		dayNames: ['日曜日','月曜日','火曜日','水曜日','木曜日','金曜日','土曜日'],
		dayNamesShort: ['日','月','火','水','木','金','土'],
		dayNamesMin: ['日','月','火','水','木','金','土'],
		dateFormat: 'yyyy/mm/dd',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ja'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ja'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
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
﻿/* http://keith-wood.name/calendars.html
   Khmer initialisation for Gregorian/Julian calendars for jQuery.
   Written by Sovichet Tep (sovichet.tep@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['km'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['ខែ​មករា','ខែ​កុម្ភៈ','ខែ​មិនា','ខែ​មេសា','ខែ​ឧសភា','ខែ​មិថុនា',
		'ខែ​កក្កដា','ខែ​សីហា','ខែ​កញ្ញា','ខែ​តុលា','ខែ​វិច្ឆិកា','ខែ​ធ្នូ'],
		monthNamesShort: ['មក', 'កុ', 'មិនា', 'មេ', 'ឧស', 'មិថុ',
		'កក្ក', 'សី', 'កញ្ញា', 'តុលា', 'វិច្ឆិ', 'ធ្នូ'],
		dayNames: ['ថ្ងៃ​អាទិត្យ', 'ថ្ងៃ​ចន្ទ', 'ថ្ងៃ​អង្គារ', 'ថ្ងៃ​ពុធ', 'ថ្ងៃ​ព្រហស្បត្តិ៍', 'ថ្ងៃ​សុក្រ', 'ថ្ងៃ​សៅរ៍'],
		dayNamesShort: ['អា', 'ចន្ទ', 'អង្គ', 'ពុធ', 'ព្រហ', 'សុ', 'សៅរ៍'],
		dayNamesMin: ['អា','ច','អ','ពុ','ព្រ','សុ','ស'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['km'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['km'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Korean localisation for Gregorian/Julian calendars for jQuery.
   Written by DaeKwon Kang (ncrash.dk@gmail.com), Edited by Genie. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ko'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['1월','2월','3월','4월','5월','6월',
		'7월','8월','9월','10월','11월','12월'],
		monthNamesShort: ['1월','2월','3월','4월','5월','6월',
		'7월','8월','9월','10월','11월','12월'],
		dayNames: ['일요일','월요일','화요일','수요일','목요일','금요일','토요일'],
		dayNamesShort: ['일','월','화','수','목','금','토'],
		dayNamesMin: ['일','월','화','수','목','금','토'],
		dateFormat: 'yyyy-mm-dd',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ko'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ko'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Lithuanian localisation for Gregorian/Julian calendars for jQuery.
   Arturas Paleicikas <arturas@avalon.lt>. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['lt'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Sausis','Vasaris','Kovas','Balandis','Gegužė','Birželis',
		'Liepa','Rugpjūtis','Rugsėjis','Spalis','Lapkritis','Gruodis'],
		monthNamesShort: ['Sau','Vas','Kov','Bal','Geg','Bir',
		'Lie','Rugp','Rugs','Spa','Lap','Gru'],
		dayNames: ['sekmadienis','pirmadienis','antradienis','trečiadienis','ketvirtadienis','penktadienis','šeštadienis'],
		dayNamesShort: ['sek','pir','ant','tre','ket','pen','šeš'],
		dayNamesMin: ['Se','Pr','An','Tr','Ke','Pe','Še'],
		dateFormat: 'yyyy-mm-dd',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['lt'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['lt'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Latvian localisation for Gregorian/Julian calendars for jQuery.
   Arturas Paleicikas <arturas.paleicikas@metasite.net>. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['lv'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Janvāris','Februāris','Marts','Aprīlis','Maijs','Jūnijs',
		'Jūlijs','Augusts','Septembris','Oktobris','Novembris','Decembris'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','Mai','Jūn',
		'Jūl','Aug','Sep','Okt','Nov','Dec'],
		dayNames: ['svētdiena','pirmdiena','otrdiena','trešdiena','ceturtdiena','piektdiena','sestdiena'],
		dayNamesShort: ['svt','prm','otr','tre','ctr','pkt','sst'],
		dayNamesMin: ['Sv','Pr','Ot','Tr','Ct','Pk','Ss'],
		dateFormat: 'dd-mm-yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['lv'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['lv'];
	}
})(jQuery);
/* http://keith-wood.name/calendars.html
   Montenegrin localisation for Gregorian/Julian calendars for jQuery.
   By Miloš Milošević - fleka d.o.o. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['me-ME'] = {
		name: 'Gregorijanski',
		epochs: ['pne', 'ne'],
		monthNames: ['Januar','Februar','Mart','April','Maj','Jun',
		'Jul','Avgust','Septembar','Oktobar','Novembar','Decembar'],
		monthNamesShort: ['Jan', 'Feb', 'Mar', 'Apr', 'Maj', 'Jun',
		'Jul', 'Avg', 'Sep', 'Okt', 'Nov', 'Dec'],
		dayNames: ['Neđelja', 'Poneđeljak', 'Utorak', 'Srijeda', 'Četvrtak', 'Petak', 'Subota'],
		dayNamesShort: ['Neđ', 'Pon', 'Uto', 'Sri', 'Čet', 'Pet', 'Sub'],
		dayNamesMin: ['Ne','Po','Ut','Sr','Če','Pe','Su'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['me-ME'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['me-ME'];
	}
})(jQuery);
/* http://keith-wood.name/calendars.html
   Montenegrin localisation for Gregorian/Julian calendars for jQuery.
   By Miloš Milošević - fleka d.o.o. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['me'] = {
		name: 'Грегоријански',
		epochs: ['пне', 'не'],
		monthNames: ['Јануар','Фебруар','Март','Април','Мај','Јун',
		'Јул','Август','Септембар','Октобар','Новембар','Децембар'],
		monthNamesShort: ['Јан', 'Феб', 'Мар', 'Апр', 'Мај', 'Јун',
		'Јул', 'Авг', 'Сеп', 'Окт', 'Нов', 'Дец'],
		dayNames: ['Неђеља', 'Понеђељак', 'Уторак', 'Сриједа', 'Четвртак', 'Петак', 'Субота'],
		dayNamesShort: ['Неђ', 'Пон', 'Уто', 'Сри', 'Чет', 'Пет', 'Суб'],
		dayNamesMin: ['Не','По','Ут','Ср','Че','Пе','Су'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['me'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['me'];
	}
})(jQuery);
/* http://keith-wood.name/calendars.html
   Македонски MK localisation for Gregorian/Julian calendars for jQuery.
   Hajan Selmani (hajan [at] live [dot] com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['mk'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Јануари','Февруари','Март','Април','Мај','Јуни',
		'Јули','Август','Септември','Октомври','Ноември','Декември'],
		monthNamesShort: ['Јан', 'Фев', 'Мар', 'Апр', 'Мај', 'Јун',
		'Јул', 'Авг', 'Сеп', 'Окт', 'Нов', 'Дек'],
		dayNames: ['Недела', 'Понеделник', 'Вторник', 'Среда', 'Четврток', 'Петок', 'Сабота'],
		dayNamesShort: ['Нед', 'Пон', 'Вто', 'Сре', 'Чет', 'Пет', 'Саб'],
		dayNamesMin: ['Не','По','Вт','Ср','Че','Пе','Са'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['mk'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['mk'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Malayalam localisation for Gregorian/Julian calendars for jQuery.
   Saji Nediyanchath (saji89@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ml'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['ജനുവരി','ഫെബ്രുവരി','മാര്‍ച്ച്','ഏപ്രില്‍','മേയ്','ജൂണ്‍',
		'ജൂലൈ','ആഗസ്റ്റ്','സെപ്റ്റംബര്‍','ഒക്ടോബര്‍','നവംബര്‍','ഡിസംബര്‍'],
		monthNamesShort: ['ജനു', 'ഫെബ്', 'മാര്‍', 'ഏപ്രി', 'മേയ്', 'ജൂണ്‍',
		'ജൂലാ', 'ആഗ', 'സെപ്', 'ഒക്ടോ', 'നവം', 'ഡിസ'],
		dayNames: ['ഞായര്‍', 'തിങ്കള്‍', 'ചൊവ്വ', 'ബുധന്‍', 'വ്യാഴം', 'വെള്ളി', 'ശനി'],
		dayNamesShort: ['ഞായ', 'തിങ്ക', 'ചൊവ്വ', 'ബുധ', 'വ്യാഴം', 'വെള്ളി', 'ശനി'],
		dayNamesMin: ['ഞാ','തി','ചൊ','ബു','വ്യാ','വെ','ശ'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ml'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ml'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Malaysian localisation for Gregorian/Julian calendars for jQuery.
   Written by Mohd Nawawi Mohamad Jamili (nawawi@ronggeng.net). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ms'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Januari','Februari','Mac','April','Mei','Jun',
		'Julai','Ogos','September','Oktober','November','Disember'],
		monthNamesShort: ['Jan','Feb','Mac','Apr','Mei','Jun',
		'Jul','Ogo','Sep','Okt','Nov','Dis'],
		dayNames: ['Ahad','Isnin','Selasa','Rabu','Khamis','Jumaat','Sabtu'],
		dayNamesShort: ['Aha','Isn','Sel','Rab','Kha','Jum','Sab'],
		dayNamesMin: ['Ah','Is','Se','Ra','Kh','Ju','Sa'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ms'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ms'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
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
﻿/* http://keith-wood.name/calendars.html
   Dutch/Belgian localisation for Gregorian/Julian calendars for jQuery.
   Written by Mathias Bynens <http://mathiasbynens.be/>. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['nl-BE'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['januari', 'februari', 'maart', 'april', 'mei', 'juni',
		'juli', 'augustus', 'september', 'oktober', 'november', 'december'],
		monthNamesShort: ['jan', 'feb', 'maa', 'apr', 'mei', 'jun',
		'jul', 'aug', 'sep', 'okt', 'nov', 'dec'],
		dayNames: ['zondag', 'maandag', 'dinsdag', 'woensdag', 'donderdag', 'vrijdag', 'zaterdag'],
		dayNamesShort: ['zon', 'maa', 'din', 'woe', 'don', 'vri', 'zat'],
		dayNamesMin: ['zo', 'ma', 'di', 'wo', 'do', 'vr', 'za'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['nl-BE'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['nl-BE'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Dutch localisation for Gregorian/Julian calendars for jQuery.
   Written by Mathias Bynens <http://mathiasbynens.be/>. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['nl'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['januari', 'februari', 'maart', 'april', 'mei', 'juni',
		'juli', 'augustus', 'september', 'oktober', 'november', 'december'],
		monthNamesShort: ['jan', 'feb', 'maa', 'apr', 'mei', 'jun',
		'jul', 'aug', 'sep', 'okt', 'nov', 'dec'],
		dayNames: ['zondag', 'maandag', 'dinsdag', 'woensdag', 'donderdag', 'vrijdag', 'zaterdag'],
		dayNamesShort: ['zon', 'maa', 'din', 'woe', 'don', 'vri', 'zat'],
		dayNamesMin: ['zo', 'ma', 'di', 'wo', 'do', 'vr', 'za'],
		dateFormat: 'dd-mm-yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['nl'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['nl'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Norwegian localisation for Gregorian/Julian calendars for jQuery.
   Written by Naimdjon Takhirov (naimdjon@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['no'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Januar','Februar','Mars','April','Mai','Juni',
		'Juli','August','September','Oktober','November','Desember'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','Mai','Jun',
		'Jul','Aug','Sep','Okt','Nov','Des'],
		dayNamesShort: ['Søn','Man','Tir','Ons','Tor','Fre','Lør'],
		dayNames: ['Søndag','Mandag','Tirsdag','Onsdag','Torsdag','Fredag','Lørdag'],
		dayNamesMin: ['Sø','Ma','Ti','On','To','Fr','Lø'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['no'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['no'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Polish localisation for Gregorian/Julian calendars for jQuery.
   Written by Jacek Wysocki (jacek.wysocki@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['pl'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Styczeń','Luty','Marzec','Kwiecień','Maj','Czerwiec',
		'Lipiec','Sierpień','Wrzesień','Październik','Listopad','Grudzień'],
		monthNamesShort: ['Sty','Lu','Mar','Kw','Maj','Cze',
		'Lip','Sie','Wrz','Pa','Lis','Gru'],
		dayNames: ['Niedziela','Poniedzialek','Wtorek','Środa','Czwartek','Piątek','Sobota'],
		dayNamesShort: ['Nie','Pn','Wt','Śr','Czw','Pt','So'],
		dayNamesMin: ['N','Pn','Wt','Śr','Cz','Pt','So'],
		dateFormat: 'yyyy-mm-dd',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['pl'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['pl'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
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
﻿/* http://keith-wood.name/calendars.html
   Romansh localisation for Gregorian/Julian calendars for jQuery.
   Yvonne Gienal (yvonne.gienal@educa.ch). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['rm'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Schaner','Favrer','Mars','Avrigl','Matg','Zercladur',
		'Fanadur','Avust','Settember','October','November','December'],
		monthNamesShort: ['Scha','Fev','Mar','Avr','Matg','Zer',
		'Fan','Avu','Sett','Oct','Nov','Dec'],
		dayNames: ['Dumengia','Glindesdi','Mardi','Mesemna','Gievgia','Venderdi','Sonda'],
		dayNamesShort: ['Dum','Gli','Mar','Mes','Gie','Ven','Som'],
		dayNamesMin: ['Du','Gl','Ma','Me','Gi','Ve','So'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['rm'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['rm'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Romanian localisation for Gregorian/Julian calendars for jQuery.
   Written by Edmond L. (ll_edmond@walla.com) and Ionut G. Stan (ionut.g.stan@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ro'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Ianuarie','Februarie','Martie','Aprilie','Mai','Iunie',
		'Iulie','August','Septembrie','Octombrie','Noiembrie','Decembrie'],
		monthNamesShort: ['Ian', 'Feb', 'Mar', 'Apr', 'Mai', 'Iun',
		'Iul', 'Aug', 'Sep', 'Oct', 'Noi', 'Dec'],
		dayNames: ['Duminică', 'Luni', 'Marti', 'Miercuri', 'Joi', 'Vineri', 'Sâmbătă'],
		dayNamesShort: ['Dum', 'Lun', 'Mar', 'Mie', 'Joi', 'Vin', 'Sâm'],
		dayNamesMin: ['Du','Lu','Ma','Mi','Jo','Vi','Sâ'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ro'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ro'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Russian localisation for Gregorian/Julian calendars for jQuery.
   Written by Andrew Stromnov (stromnov@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ru'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Январь','Февраль','Март','Апрель','Май','Июнь',
		'Июль','Август','Сентябрь','Октябрь','Ноябрь','Декабрь'],
		monthNamesShort: ['Янв','Фев','Мар','Апр','Май','Июн',
		'Июл','Авг','Сен','Окт','Ноя','Дек'],
		dayNames: ['воскресенье','понедельник','вторник','среда','четверг','пятница','суббота'],
		dayNamesShort: ['вск','пнд','втр','срд','чтв','птн','сбт'],
		dayNamesMin: ['Вс','Пн','Вт','Ср','Чт','Пт','Сб'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ru'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ru'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
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
﻿/* http://keith-wood.name/calendars.html
   Slovenian localisation for Gregorian/Julian calendars for jQuery.
   Written by Jaka Jancar (jaka@kubje.org). */
/* c = &#x10D;, s = &#x161; z = &#x17E; C = &#x10C; S = &#x160; Z = &#x17D; */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['sl'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Januar','Februar','Marec','April','Maj','Junij',
		'Julij','Avgust','September','Oktober','November','December'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','Maj','Jun',
		'Jul','Avg','Sep','Okt','Nov','Dec'],
		dayNames: ['Nedelja','Ponedeljek','Torek','Sreda','&#x10C;etrtek','Petek','Sobota'],
		dayNamesShort: ['Ned','Pon','Tor','Sre','&#x10C;et','Pet','Sob'],
		dayNamesMin: ['Ne','Po','To','Sr','&#x10C;e','Pe','So'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['sl'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['sl'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Albanian localisation for Gregorian/Julian calendars for jQuery.
   Written by Flakron Bytyqi (flakron@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['sq'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Janar','Shkurt','Mars','Prill','Maj','Qershor',
		'Korrik','Gusht','Shtator','Tetor','Nëntor','Dhjetor'],
		monthNamesShort: ['Jan','Shk','Mar','Pri','Maj','Qer',
		'Kor','Gus','Sht','Tet','Nën','Dhj'],
		dayNames: ['E Diel','E Hënë','E Martë','E Mërkurë','E Enjte','E Premte','E Shtune'],
		dayNamesShort: ['Di','Hë','Ma','Më','En','Pr','Sh'],
		dayNamesMin: ['Di','Hë','Ma','Më','En','Pr','Sh'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['sq'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['sq'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Serbian localisation for Gregorian/Julian calendars for jQuery.
   Written by Dejan Dimić. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['sr-SR'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Januar','Februar','Mart','April','Maj','Jun',
		'Jul','Avgust','Septembar','Oktobar','Novembar','Decembar'],
		monthNamesShort: ['Jan','Feb','Mar','Apr','Maj','Jun','Jul','Avg','Sep','Okt','Nov','Dec'],
		dayNames: ['Nedelja','Ponedeljak','Utorak','Sreda','Četvrtak','Petak','Subota'],
		dayNamesShort: ['Ned','Pon','Uto','Sre','Čet','Pet','Sub'],
		dayNamesMin: ['Ne','Po','Ut','Sr','Če','Pe','Su'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['sr-SR'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['sr-SR'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Serbian localisation for Gregorian/Julian calendars for jQuery.
   Written by Dejan Dimić. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['sr'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Јануар','Фебруар','Март','Април','Мај','Јун',
		'Јул','Август','Септембар','Октобар','Новембар','Децембар'],
		monthNamesShort: ['Јан','Феб','Мар','Апр','Мај','Јун','Јул','Авг','Сеп','Окт','Нов','Дец'],
		dayNames: ['Недеља','Понедељак','Уторак','Среда','Четвртак','Петак','Субота'],
		dayNamesShort: ['Нед','Пон','Уто','Сре','Чет','Пет','Суб'],
		dayNamesMin: ['Не','По','Ут','Ср','Че','Пе','Су'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['sr'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['sr'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Swedish localisation for Gregorian/Julian calendars for jQuery.
   Written by Anders Ekdahl (anders@nomadiz.se). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['sv'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
        monthNames: ['Januari','Februari','Mars','April','Maj','Juni',
        'Juli','Augusti','September','Oktober','November','December'],
        monthNamesShort: ['Jan','Feb','Mar','Apr','Maj','Jun',
        'Jul','Aug','Sep','Okt','Nov','Dec'],
		dayNames: ['Söndag','Måndag','Tisdag','Onsdag','Torsdag','Fredag','Lördag'],
		dayNamesShort: ['Sön','Mån','Tis','Ons','Tor','Fre','Lör'],
		dayNamesMin: ['Sö','Må','Ti','On','To','Fr','Lö'],
        dateFormat: 'yyyy-mm-dd',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['sv'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['sv'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Tamil (UTF-8) localisation for Gregorian/Julian calendars for jQuery.
   Written by S A Sureshkumar (saskumar@live.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ta'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['தை','மாசி','பங்குனி','சித்திரை','வைகாசி','ஆனி',
		'ஆடி','ஆவணி','புரட்டாசி','ஐப்பசி','கார்த்திகை','மார்கழி'],
		monthNamesShort: ['தை','மாசி','பங்','சித்','வைகா','ஆனி',
		'ஆடி','ஆவ','புர','ஐப்','கார்','மார்'],
		dayNames: ['ஞாயிற்றுக்கிழமை','திங்கட்கிழமை','செவ்வாய்க்கிழமை','புதன்கிழமை','வியாழக்கிழமை','வெள்ளிக்கிழமை','சனிக்கிழமை'],
		dayNamesShort: ['ஞாயிறு','திங்கள்','செவ்வாய்','புதன்','வியாழன்','வெள்ளி','சனி'],
		dayNamesMin: ['ஞா','தி','செ','பு','வி','வெ','ச'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ta'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ta'];
	}
})(jQuery);
/* http://keith-wood.name/calendars.html
   Thai localisation for Gregorian/Julian calendars for jQuery.
   Written by pipo (pipo@sixhead.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['th'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['มกราคม','กุมภาพันธ์','มีนาคม','เมษายน','พฤษภาคม','มิถุนายน',
		'กรกฎาคม','สิงหาคม','กันยายน','ตุลาคม','พฤศจิกายน','ธันวาคม'],
		monthNamesShort: ['ม.ค.','ก.พ.','มี.ค.','เม.ย.','พ.ค.','มิ.ย.',
		'ก.ค.','ส.ค.','ก.ย.','ต.ค.','พ.ย.','ธ.ค.'],
		dayNames: ['อาทิตย์','จันทร์','อังคาร','พุธ','พฤหัสบดี','ศุกร์','เสาร์'],
		dayNamesShort: ['อา.','จ.','อ.','พ.','พฤ.','ศ.','ส.'],
		dayNamesMin: ['อา.','จ.','อ.','พ.','พฤ.','ศ.','ส.'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['th'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['th'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Turkish localisation for Gregorian/Julian calendars for jQuery.
   Written by Izzet Emre Erkan (kara@karalamalar.net). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['tr'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Ocak','Şubat','Mart','Nisan','Mayıs','Haziran',
		'Temmuz','Ağustos','Eylül','Ekim','Kasım','Aralık'],
		monthNamesShort: ['Oca','Şub','Mar','Nis','May','Haz',
		'Tem','Ağu','Eyl','Eki','Kas','Ara'],
		dayNames: ['Pazar','Pazartesi','Salı','Çarşamba','Perşembe','Cuma','Cumartesi'],
		dayNamesShort: ['Pz','Pt','Sa','Ça','Pe','Cu','Ct'],
		dayNamesMin: ['Pz','Pt','Sa','Ça','Pe','Cu','Ct'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['tr'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['tr'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Tatar localisation for Gregorian/Julian calendars for jQuery.
   Written by Ирек Хаҗиев (khazirek@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['tt'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Гынвар','Февраль','Март','Апрель','Май','Июнь',
		'Июль','Август','Сентябрь','Октябрь','Ноябрь','Декабрь'],
		monthNamesShort: ['Гыйн','Фев','Мар','Апр','Май','Июн',
		'Июл','Авг','Сен','Окт','Ноя','Дек'],
		dayNames: ['якшәмбе','дүшәмбе','сишәмбе','чәршәмбе','пәнҗешәмбе','җомга','шимбә'],
		dayNamesShort: ['якш','дүш','сиш','чәр','пән','җом','шим'],
		dayNamesMin: ['Як','Дү','Си','Чә','Пә','Җо','Ши'],
		dateFormat: 'dd.mm.yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['tt'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['tt'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Ukrainian localisation for Gregorian/Julian calendars for jQuery.
   Written by Maxim Drogobitskiy (maxdao@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['uk'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Січень','Лютий','Березень','Квітень','Травень','Червень',
		'Липень','Серпень','Вересень','Жовтень','Листопад','Грудень'],
		monthNamesShort: ['Січ','Лют','Бер','Кві','Тра','Чер',
		'Лип','Сер','Вер','Жов','Лис','Гру'],
		dayNames: ['неділя','понеділок','вівторок','середа','четвер','п\'ятниця','субота'],
		dayNamesShort: ['нед','пнд','вів','срд','чтв','птн','сбт'],
		dayNamesMin: ['Нд','Пн','Вт','Ср','Чт','Пт','Сб'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['uk'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['uk'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Urdu localisation for Gregorian/Julian calendars for jQuery.
   Mansoor Munib -- mansoormunib@gmail.com <http://www.mansoor.co.nr/mansoor.html>
   Thanks to Habib Ahmed, ObaidUllah Anwar. */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['ur'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['جنوری','فروری','مارچ','اپریل','مئی','جون',
		'جولائی','اگست','ستمبر','اکتوبر','نومبر','دسمبر'],
		monthNamesShort: ['1','2','3','4','5','6',
		'7','8','9','10','11','12'],
		dayNames: ['اتوار','پير','منگل','بدھ','جمعرات','جمعہ','ہفتہ'],
		dayNamesShort: ['اتوار','پير','منگل','بدھ','جمعرات','جمعہ','ہفتہ'],
		dayNamesMin: ['اتوار','پير','منگل','بدھ','جمعرات','جمعہ','ہفتہ'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 0,
		firstDay: 1,
		isRTL: true
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['ur'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['ur'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Vietnamese localisation for Gregorian/Julian calendars for jQuery.
   Translated by Le Thanh Huy (lthanhhuy@cit.ctu.edu.vn). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['vi'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['Tháng Một', 'Tháng Hai', 'Tháng Ba', 'Tháng Tư', 'Tháng Năm', 'Tháng Sáu',
		'Tháng Bảy', 'Tháng Tám', 'Tháng Chín', 'Tháng Mười', 'Tháng Mười Một', 'Tháng Mười Hai'],
		monthNamesShort: ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6',
		'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12'],
		dayNames: ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'],
		dayNamesShort: ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'],
		dayNamesMin: ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'],
		dateFormat: 'dd/mm/yyyy',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['vi'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['vi'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Simplified Chinese localisation for Gregorian/Julian calendars for jQuery.
   Written by Cloudream (cloudream@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['zh-CN'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['一月','二月','三月','四月','五月','六月',
		'七月','八月','九月','十月','十一月','十二月'],
		monthNamesShort: ['一','二','三','四','五','六',
		'七','八','九','十','十一','十二'],
		dayNames: ['星期日','星期一','星期二','星期三','星期四','星期五','星期六'],
		dayNamesShort: ['周日','周一','周二','周三','周四','周五','周六'],
		dayNamesMin: ['日','一','二','三','四','五','六'],
		dateFormat: 'yyyy-mm-dd',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['zh-CN'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['zh-CN'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Hong Kong  Chinese localisation for Gregorian/Julian calendars for jQuery.
   Written by SCCY (samuelcychan@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['zh-HK'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['一月','二月','三月','四月','五月','六月',
		'七月','八月','九月','十月','十一月','十二月'],
		monthNamesShort: ['一','二','三','四','五','六',
		'七','八','九','十','十一','十二'],
		dayNames: ['星期日','星期一','星期二','星期三','星期四','星期五','星期六'],
		dayNamesShort: ['周日','周一','周二','周三','周四','周五','周六'],
		dayNamesMin: ['日','一','二','三','四','五','六'],
		dateFormat: 'dd-mm-yyyy',
		firstDay: 0,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['zh-HK'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['zh-HK'];
	}
})(jQuery);
﻿/* http://keith-wood.name/calendars.html
   Traditional Chinese localisation for Gregorian/Julian calendars for jQuery.
   Written by Ressol (ressol@gmail.com). */
(function($) {
	$.calendars.calendars.gregorian.prototype.regionalOptions['zh-TW'] = {
		name: 'Gregorian',
		epochs: ['BCE', 'CE'],
		monthNames: ['一月','二月','三月','四月','五月','六月',
		'七月','八月','九月','十月','十一月','十二月'],
		monthNamesShort: ['一','二','三','四','五','六',
		'七','八','九','十','十一','十二'],
		dayNames: ['星期日','星期一','星期二','星期三','星期四','星期五','星期六'],
		dayNamesShort: ['周日','周一','周二','周三','周四','周五','周六'],
		dayNamesMin: ['日','一','二','三','四','五','六'],
		dateFormat: 'yyyy/mm/dd',
		firstDay: 1,
		isRTL: false
	};
	if ($.calendars.calendars.julian) {
		$.calendars.calendars.julian.prototype.regionalOptions['zh-TW'] =
			$.calendars.calendars.gregorian.prototype.regionalOptions['zh-TW'];
	}
})(jQuery);
