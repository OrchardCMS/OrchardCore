/*
  jQuery utils - 0.8.5
  http://code.google.com/p/jquery-utils/

  (c) Maxime Haineault <haineault@gmail.com> 
  http://haineault.com

  MIT License (http://www.opensource.org/licenses/mit-license.php

*/

(function($){
     $.extend($.expr[':'], {
        // case insensitive version of :contains
        icontains: function(a,i,m){return (a.textContent||a.innerText||jQuery(a).text()||"").toLowerCase().indexOf(m[3].toLowerCase())>=0;}
    });

    $.iterators = {
        getText:  function() { return $(this).text(); },
        parseInt: function(v){ return parseInt(v, 10); }
    };

	$.extend({ 

        // Returns a range object
        // Author: Matthias Miller
        // Site:   http://blog.outofhanwell.com/2006/03/29/javascript-range-function/
        range:  function() {
            if (!arguments.length) { return []; }
            var min, max, step;
            if (arguments.length == 1) {
                min  = 0;
                max  = arguments[0]-1;
                step = 1;
            }
            else {
                // default step to 1 if it's zero or undefined
                min  = arguments[0];
                max  = arguments[1]-1;
                step = arguments[2] || 1;
            }
            // convert negative steps to positive and reverse min/max
            if (step < 0 && min >= max) {
                step *= -1;
                var tmp = min;
                min = max;
                max = tmp;
                min += ((max-min) % step);
            }
            var a = [];
            for (var i = min; i <= max; i += step) { a.push(i); }
            return a;
        },

        // Taken from ui.core.js. 
        // Why are you keeping this gem for yourself guys ? :|
        keyCode: {
            BACKSPACE: 8, CAPS_LOCK: 20, COMMA: 188, CONTROL: 17, DELETE: 46, DOWN: 40,
            END: 35, ENTER: 13, ESCAPE: 27, HOME: 36, INSERT:  45, LEFT: 37,
            NUMPAD_ADD: 107, NUMPAD_DECIMAL: 110, NUMPAD_DIVIDE: 111, NUMPAD_ENTER: 108, 
            NUMPAD_MULTIPLY: 106, NUMPAD_SUBTRACT: 109, PAGE_DOWN: 34, PAGE_UP: 33, 
            PERIOD: 190, RIGHT: 39, SHIFT: 16, SPACE: 32, TAB: 9, UP: 38
        },
        
        // Takes a keyboard event and return true if the keycode match the specified keycode
        keyIs: function(k, e) {
            return parseInt($.keyCode[k.toUpperCase()], 10) == parseInt((typeof(e) == 'number' )? e: e.keyCode, 10);
        },
        
        // Returns the key of an array
        keys: function(arr) {
            var o = [];
            for (k in arr) { o.push(k); }
            return o;
        },

        // Redirect to a specified url
        redirect: function(url) {
            window.location.href = url;
            return url;
        },

        // Stop event shorthand
        stop: function(e, preventDefault, stopPropagation) {
            if (preventDefault)  { e.preventDefault(); }
            if (stopPropagation) { e.stopPropagation(); }
            return preventDefault && false || true;
        },

        // Returns the basename of a path
        basename: function(path) {
            var t = path.split('/');
            return t[t.length] === '' && s || t.slice(0, t.length).join('/');
        },

        // Returns the filename of a path
        filename: function(path) {
            return path.split('/').pop();
        }, 

        // Returns a formated file size
        filesizeformat: function(bytes, suffixes){
            var b = parseInt(bytes, 10);
            var s = suffixes || ['byte', 'bytes', 'KB', 'MB', 'GB'];
            if (isNaN(b) || b === 0) { return '0 ' + s[0]; }
            if (b == 1)              { return '1 ' + s[0]; }
            if (b < 1024)            { return  b.toFixed(2) + ' ' + s[1]; }
            if (b < 1048576)         { return (b / 1024).toFixed(2) + ' ' + s[2]; }
            if (b < 1073741824)      { return (b / 1048576).toFixed(2) + ' '+ s[3]; }
            else                     { return (b / 1073741824).toFixed(2) + ' '+ s[4]; }
        },

        fileExtension: function(s) {
            var tokens = s.split('.');
            return tokens[tokens.length-1] || false;
        },
        
        // Returns true if an object is a String
        isString: function(o) {
            return typeof(o) == 'string' && true || false;
        },
        
        // Returns true if an object is a RegExp
		isRegExp: function(o) {
			return o && o.constructor.toString().indexOf('RegExp()') != -1 || false;
		},

        isObject: function(o) {
            return (typeof(o) == 'object');
        },
        
        // Convert input to currency (two decimal fixed number)
		toCurrency: function(i) {
			i = parseFloat(i, 10).toFixed(2);
			return (i=='NaN') ? '0.00' : i;
		},

        /*-------------------------------------------------------------------- 
         * javascript method: "pxToEm"
         * by:
           Scott Jehl (scott@filamentgroup.com) 
           Maggie Wachs (maggie@filamentgroup.com)
           http://www.filamentgroup.com
         *
         * Copyright (c) 2008 Filament Group
         * Dual licensed under the MIT (filamentgroup.com/examples/mit-license.txt) and GPL (filamentgroup.com/examples/gpl-license.txt) licenses.
         *
         * Description: pxToEm converts a pixel value to ems depending on inherited font size.  
         * Article: http://www.filamentgroup.com/lab/retaining_scalable_interfaces_with_pixel_to_em_conversion/
         * Demo: http://www.filamentgroup.com/examples/pxToEm/	 	
         *							
         * Options:  	 								
                scope: string or jQuery selector for font-size scoping
                reverse: Boolean, true reverses the conversion to em-px
         * Dependencies: jQuery library						  
         * Usage Example: myPixelValue.pxToEm(); or myPixelValue.pxToEm({'scope':'#navigation', reverse: true});
         *
         * Version: 2.1, 18.12.2008
         * Changelog:
         *		08.02.2007 initial Version 1.0
         *		08.01.2008 - fixed font-size calculation for IE
         *		18.12.2008 - removed native object prototyping to stay in jQuery's spirit, jsLinted (Maxime Haineault <haineault@gmail.com>)
        --------------------------------------------------------------------*/

        pxToEm: function(i, settings){
            //set defaults
            settings = jQuery.extend({
                scope: 'body',
                reverse: false
            }, settings);
            
            var pxVal = (i === '') ? 0 : parseFloat(i);
            var scopeVal;
            var getWindowWidth = function(){
                var de = document.documentElement;
                return self.innerWidth || (de && de.clientWidth) || document.body.clientWidth;
            };	
            
            /* When a percentage-based font-size is set on the body, IE returns that percent of the window width as the font-size. 
                For example, if the body font-size is 62.5% and the window width is 1000px, IE will return 625px as the font-size. 	
                When this happens, we calculate the correct body font-size (%) and multiply it by 16 (the standard browser font size) 
                to get an accurate em value. */
                        
            if (settings.scope == 'body' && $.browser.msie && (parseFloat($('body').css('font-size')) / getWindowWidth()).toFixed(1) > 0.0) {
                var calcFontSize = function(){		
                    return (parseFloat($('body').css('font-size'))/getWindowWidth()).toFixed(3) * 16;
                };
                scopeVal = calcFontSize();
            }
            else { scopeVal = parseFloat(jQuery(settings.scope).css("font-size")); }
                    
            var result = (settings.reverse === true) ? (pxVal * scopeVal).toFixed(2) + 'px' : (pxVal / scopeVal).toFixed(2) + 'em';
            return result;
        }
	});

	$.extend($.fn, { 
        type: function() {
            try { return $(this).get(0).nodeName.toLowerCase(); }
            catch(e) { return false; }
        },
        // Select a text range in a textarea
        selectRange: function(start, end){
            // use only the first one since only one input can be focused
            if ($(this).get(0).createTextRange) {
                var range = $(this).get(0).createTextRange();
                range.collapse(true);
                range.moveEnd('character',   end);
                range.moveStart('character', start);
                range.select();
            }
            else if ($(this).get(0).setSelectionRange) {
                $(this).bind('focus', function(e){
                    e.preventDefault();
                }).get(0).setSelectionRange(start, end);
            }
            return $(this);
        },

        /*-------------------------------------------------------------------- 
         * JQuery Plugin: "EqualHeights"
         * by:	Scott Jehl, Todd Parker, Maggie Costello Wachs (http://www.filamentgroup.com)
         *
         * Copyright (c) 2008 Filament Group
         * Licensed under GPL (http://www.opensource.org/licenses/gpl-license.php)
         *
         * Description: Compares the heights or widths of the top-level children of a provided element 
                and sets their min-height to the tallest height (or width to widest width). Sets in em units 
                by default if pxToEm() method is available.
         * Dependencies: jQuery library, pxToEm method	(article: 
                http://www.filamentgroup.com/lab/retaining_scalable_interfaces_with_pixel_to_em_conversion/)							  
         * Usage Example: $(element).equalHeights();
                Optional: to set min-height in px, pass a true argument: $(element).equalHeights(true);
         * Version: 2.1, 18.12.2008
         *
         * Note: Changed pxToEm call to call $.pxToEm instead, jsLinted (Maxime Haineault <haineault@gmail.com>)
        --------------------------------------------------------------------*/

        equalHeights: function(px){
            $(this).each(function(){
                var currentTallest = 0;
                $(this).children().each(function(i){
                    if ($(this).height() > currentTallest) { currentTallest = $(this).height(); }
                });
                if (!px || !$.pxToEm) { currentTallest = $.pxToEm(currentTallest); } //use ems unless px is specified
                // for ie6, set height since min-height isn't supported
                if ($.browser.msie && $.browser.version == 6.0) { $(this).children().css({'height': currentTallest}); }
                $(this).children().css({'min-height': currentTallest}); 
            });
            return this;
        },

        // Copyright (c) 2009 James Padolsey
        // http://james.padolsey.com/javascript/jquery-delay-plugin/
        delay: function(time, callback){
            jQuery.fx.step.delay = function(){};
            return this.animate({delay:1}, time, callback);
        }        
	});
})(jQuery);
/*
  jQuery strings - 0.3
  http://code.google.com/p/jquery-utils/
  
  (c) Maxime Haineault <haineault@gmail.com>
  http://haineault.com   

  MIT License (http://www.opensource.org/licenses/mit-license.php)

  Implementation of Python3K advanced string formatting
  http://www.python.org/dev/peps/pep-3101/

  Documentation: http://code.google.com/p/jquery-utils/wiki/StringFormat
  
*/
(function($){
    var strings = {
        strConversion: {
            // tries to translate any objects type into string gracefully
            __repr: function(i){
                switch(this.__getType(i)) {
                    case 'array':case 'date':case 'number':
                        return i.toString();
                    case 'object': 
                        var o = [];
                        for (x=0; x<i.length; i++) { o.push(i+': '+ this.__repr(i[x])); }
                        return o.join(', ');
                    case 'string': 
                        return i;
                    default: 
                        return i;
                }
            },
            // like typeof but less vague
            __getType: function(i) {
                if (!i || !i.constructor) { return typeof(i); }
                var match = i.constructor.toString().match(/Array|Number|String|Object|Date/);
                return match && match[0].toLowerCase() || typeof(i);
            },
            //+ Jonas Raoni Soares Silva
            // the @ sign next to "//" is interpreted by IE when using cc<underscore>on! Inserted a space.
            //  @ http://jsfromhell.com/string/pad [v1.0]
            __pad: function(str, l, s, t){
                var p = s || ' ';
                var o = str;
                if (l - str.length > 0) {
                    o = new Array(Math.ceil(l / p.length)).join(p).substr(0, t = !t ? l : t == 1 ? 0 : Math.ceil(l / 2)) + str + p.substr(0, l - t);
                }
                return o;
            },
            __getInput: function(arg, args) {
                 var key = arg.getKey();
                switch(this.__getType(args)){
                    case 'object': // Thanks to Jonathan Works for the patch
                        var keys = key.split('.');
                        var obj = args;
                        for(var subkey = 0; subkey < keys.length; subkey++){
                            obj = obj[keys[subkey]];
                        }
                        if (typeof(obj) != 'undefined') {
                            if (strings.strConversion.__getType(obj) == 'array') {
                                return arg.getFormat().match(/\.\*/) && obj[1] || obj;
                            }
                            return obj;
                        }
                        else {
                            // TODO: try by numerical index                    
                        }
                    break;
                    case 'array': 
                        key = parseInt(key, 10);
                        if (arg.getFormat().match(/\.\*/) && typeof args[key+1] != 'undefined') { return args[key+1]; }
                        else if (typeof args[key] != 'undefined') { return args[key]; }
                        else { return key; }
                    break;
                }
                return '{'+key+'}';
            },
            __formatToken: function(token, args) {
                var arg   = new Argument(token, args);
                return strings.strConversion[arg.getFormat().slice(-1)](this.__getInput(arg, args), arg);
            },

            // Signed integer decimal.
            d: function(input, arg){
                var o = parseInt(input, 10); // enforce base 10
                var p = arg.getPaddingLength();
                if (p) { return this.__pad(o.toString(), p, arg.getPaddingString(), 0); }
                else   { return o; }
            },
            // Signed integer decimal.
            i: function(input, args){ 
                return this.d(input, args);
            },
            // Unsigned octal
            o: function(input, arg){ 
                var o = input.toString(8);
                if (arg.isAlternate()) { o = this.__pad(o, o.length+1, '0', 0); }
                return this.__pad(o, arg.getPaddingLength(), arg.getPaddingString(), 0);
            },
            // Unsigned decimal
            u: function(input, args) {
                return Math.abs(this.d(input, args));
            },
            // Unsigned hexadecimal (lowercase)
            x: function(input, arg){
                var o = parseInt(input, 10).toString(16);
                o = this.__pad(o, arg.getPaddingLength(), arg.getPaddingString(),0);
                return arg.isAlternate() ? '0x'+o : o;
            },
            // Unsigned hexadecimal (uppercase)
            X: function(input, arg){
                return this.x(input, arg).toUpperCase();
            },
            // Floating point exponential format (lowercase)
            e: function(input, arg){
                return parseFloat(input, 10).toExponential(arg.getPrecision());
            },
            // Floating point exponential format (uppercase)
            E: function(input, arg){
                return this.e(input, arg).toUpperCase();
            },
            // Floating point decimal format
            f: function(input, arg){
                return this.__pad(parseFloat(input, 10).toFixed(arg.getPrecision()), arg.getPaddingLength(), arg.getPaddingString(),0);
            },
            // Floating point decimal format (alias)
            F: function(input, args){
                return this.f(input, args);
            },
            // Floating point format. Uses exponential format if exponent is greater than -4 or less than precision, decimal format otherwise
            g: function(input, arg){
                var o = parseFloat(input, 10);
                return (o.toString().length > 6) ? Math.round(o.toExponential(arg.getPrecision())): o;
            },
            // Floating point format. Uses exponential format if exponent is greater than -4 or less than precision, decimal format otherwise
            G: function(input, args){
                return this.g(input, args);
            },
            // Single character (accepts integer or single character string). 	
            c: function(input, args) {
                var match = input.match(/\w|\d/);
                return match && match[0] || '';
            },
            // String (converts any JavaScript object to anotated format)
            r: function(input, args) {
                return this.__repr(input);
            },
            // String (converts any JavaScript object using object.toString())
            s: function(input, args) {
                return input.toString && input.toString() || ''+input;
            }
        },

        format: function(str, args) {
            var end    = 0;
            var start  = 0;
            var match  = false;
            var buffer = [];
            var token  = '';
            var tmp    = (str||'').split('');
            for(start=0; start < tmp.length; start++) {
                if (tmp[start] == '{' && tmp[start+1] !='{') {
                    end   = str.indexOf('}', start);
                    token = tmp.slice(start+1, end).join('');
                    if (tmp[start-1] != '{' && tmp[end+1] != '}') {
                        var tokenArgs = (typeof arguments[1] != 'object')? arguments2Array(arguments, 2): args || [];
                        buffer.push(strings.strConversion.__formatToken(token, tokenArgs));
                    }
                    else {
                        buffer.push(token);
                    }
                }
                else if (start > end || buffer.length < 1) { buffer.push(tmp[start]); }
            }
            return (buffer.length > 1)? buffer.join(''): buffer[0];
        },

        calc: function(str, args) {
            return eval(format(str, args));
        },

        repeat: function(s, n) { 
            return new Array(n+1).join(s); 
        },

        UTF8encode: function(s) { 
            return unescape(encodeURIComponent(s)); 
        },

        UTF8decode: function(s) { 
            return decodeURIComponent(escape(s)); 
        },

        tpl: function() {
            var out = '';
            var render = true;
            // Set
            // $.tpl('ui.test', ['<span>', helloWorld ,'</span>']);
            if (arguments.length == 2 && $.isArray(arguments[1])) {
                this[arguments[0]] = arguments[1].join('');
                return $(this[arguments[0]]);
            }
            // $.tpl('ui.test', '<span>hello world</span>');
            if (arguments.length == 2 && $.isString(arguments[1])) {
                this[arguments[0]] = arguments[1];
                return $(this[arguments[0]]);
            }
            // Call
            // $.tpl('ui.test');
            if (arguments.length == 1) {
                return $(this[arguments[0]]);
            }
            // $.tpl('ui.test', false);
            if (arguments.length == 2 && arguments[1] == false) {
                return this[arguments[0]];
            }
            // $.tpl('ui.test', {value:blah});
            if (arguments.length == 2 && $.isObject(arguments[1])) {
                return $($.format(this[arguments[0]], arguments[1]));
            }
            // $.tpl('ui.test', {value:blah}, false);
            if (arguments.length == 3 && $.isObject(arguments[1])) {
                return (arguments[2] == true) 
                    ? $.format(this[arguments[0]], arguments[1])
                    : $($.format(this[arguments[0]], arguments[1]));
            }
        }
    };

    var Argument = function(arg, args) {
        this.__arg  = arg;
        this.__args = args;
        this.__max_precision = parseFloat('1.'+ (new Array(32)).join('1'), 10).toString().length-3;
        this.__def_precision = 6;
        this.getString = function(){
            return this.__arg;
        };
        this.getKey = function(){
            return this.__arg.split(':')[0];
        };
        this.getFormat = function(){
            var match = this.getString().split(':');
            return (match && match[1])? match[1]: 's';
        };
        this.getPrecision = function(){
            var match = this.getFormat().match(/\.(\d+|\*)/g);
            if (!match) { return this.__def_precision; }
            else {
                match = match[0].slice(1);
                if (match != '*') { return parseInt(match, 10); }
                else if(strings.strConversion.__getType(this.__args) == 'array') {
                    return this.__args[1] && this.__args[0] || this.__def_precision;
                }
                else if(strings.strConversion.__getType(this.__args) == 'object') {
                    return this.__args[this.getKey()] && this.__args[this.getKey()][0] || this.__def_precision;
                }
                else { return this.__def_precision; }
            }
        };
        this.getPaddingLength = function(){
            var match = false;
            if (this.isAlternate()) {
                match = this.getString().match(/0?#0?(\d+)/);
                if (match && match[1]) { return parseInt(match[1], 10); }
            }
            match = this.getString().match(/(0|\.)(\d+|\*)/g);
            return match && parseInt(match[0].slice(1), 10) || 0;
        };
        this.getPaddingString = function(){
            var o = '';
            if (this.isAlternate()) { o = ' '; }
            // 0 take precedence on alternate format
            if (this.getFormat().match(/#0|0#|^0|\.\d+/)) { o = '0'; }
            return o;
        };
        this.getFlags = function(){
            var match = this.getString().matc(/^(0|\#|\-|\+|\s)+/);
            return match && match[0].split('') || [];
        };
        this.isAlternate = function() {
            return !!this.getFormat().match(/^0?#/);
        };
    };

    var arguments2Array = function(args, shift) {
        var o = [];
        for (l=args.length, x=(shift || 0)-1; x<l;x++) { o.push(args[x]); }
        return o;
    };
    $.extend(strings);
})(jQuery);
/*
  jQuery anchor handler - 0.5
  http://code.google.com/p/jquery-utils/

  (c) Maxime Haineault <haineault@gmail.com>
  http://haineault.com   

  MIT License (http://www.opensource.org/licenses/mit-license.php)

*/

(function($){
    var hash = window.location.hash;
    var handlers  = [];
    var opt = {};

	$.extend({
		anchorHandler: {
            apply: function() {
                $.map(handlers, function(handler){
                    var match = hash.match(handler.r) && hash.match(handler.r)[0] || false;
                    if (match)  { handler.cb.apply($('a[href*='+match+']').get(0), [handler.r, hash || '']); }
                });
                return $.anchorHandler;
            },
			add: function(regexp, callback, options) {
                var opt  = $.extend({handleClick: true, preserveHash: true}, options);
                if (opt.handleClick) { 
                    $('a[href*=#]').each(function(i, a){
                        if (a.href.match(regexp)) {
                            $(a).bind('click.anchorHandler', function(){
                                if (opt.preserveHash) { window.location.hash = a.hash; }
                                return callback.apply(this, [regexp, a.href]);
                                });
                        }
                    }); 
                }
				handlers.push({r: regexp, cb: callback});
                $($.anchorHandler.apply);
				return $.anchorHandler;
			}
		}
	});
})(jQuery);
/**
 * Cookie plugin
 *
 * Copyright (c) 2006 Klaus Hartl (stilbuero.de)
 * Dual licensed under the MIT and GPL licenses:
 * http://www.opensource.org/licenses/mit-license.php
 * http://www.gnu.org/licenses/gpl.html
 *
 */

/**
 * Create a cookie with the given name and value and other optional parameters.
 *
 * @example $.cookie('the_cookie', 'the_value');
 * @desc Set the value of a cookie.
 * @example $.cookie('the_cookie', 'the_value', { expires: 7, path: '/', domain: 'jquery.com', secure: true });
 * @desc Create a cookie with all available options.
 * @example $.cookie('the_cookie', 'the_value');
 * @desc Create a session cookie.
 * @example $.cookie('the_cookie', null);
 * @desc Delete a cookie by passing null as value. Keep in mind that you have to use the same path and domain
 *       used when the cookie was set.
 *
 * @param String name The name of the cookie.
 * @param String value The value of the cookie.
 * @param Object options An object literal containing key/value pairs to provide optional cookie attributes.
 * @option Number|Date expires Either an integer specifying the expiration date from now on in days or a Date object.
 *                             If a negative value is specified (e.g. a date in the past), the cookie will be deleted.
 *                             If set to null or omitted, the cookie will be a session cookie and will not be retained
 *                             when the the browser exits.
 * @option String path The value of the path atribute of the cookie (default: path of page that created the cookie).
 * @option String domain The value of the domain attribute of the cookie (default: domain of page that created the cookie).
 * @option Boolean secure If true, the secure attribute of the cookie will be set and the cookie transmission will
 *                        require a secure protocol (like HTTPS).
 * @type undefined
 *
 * @name $.cookie
 * @cat Plugins/Cookie
 * @author Klaus Hartl/klaus.hartl@stilbuero.de
 */

/**
 * Get the value of a cookie with the given name.
 *
 * @example $.cookie('the_cookie');
 * @desc Get the value of a cookie.
 *
 * @param String name The name of the cookie.
 * @return The value of the cookie.
 * @type String
 *
 * @name $.cookie
 * @cat Plugins/Cookie
 * @author Klaus Hartl/klaus.hartl@stilbuero.de
 */
jQuery.cookie = function(name, value, options) {
    if (typeof value != 'undefined') { // name and value given, set cookie
        options = options || {};
        if (value === null) {
            value = '';
            options.expires = -1;
        }
        var expires = '';
        if (options.expires && (typeof options.expires == 'number' || options.expires.toUTCString)) {
            var date;
            if (typeof options.expires == 'number') {
                date = new Date();
                date.setTime(date.getTime() + (options.expires * 24 * 60 * 60 * 1000));
            } else {
                date = options.expires;
            }
            expires = '; expires=' + date.toUTCString(); // use expires attribute, max-age is not supported by IE
        }
        // CAUTION: Needed to parenthesize options.path and options.domain
        // in the following expressions, otherwise they evaluate to undefined
        // in the packed version for some reason...
        var path = options.path ? '; path=' + (options.path) : '';
        var domain = options.domain ? '; domain=' + (options.domain) : '';
        var secure = options.secure ? '; secure' : '';
        document.cookie = [name, '=', encodeURIComponent(value), expires, path, domain, secure].join('');
    } else { // only name given, get cookie
        var cookieValue = null;
        if (document.cookie && document.cookie != '') {
            var cookies = document.cookie.split(';');
            for (var i = 0; i < cookies.length; i++) {
                var cookie = jQuery.trim(cookies[i]);
                // Does this cookie string begin with the name we want?
                if (cookie.substring(0, name.length + 1) == (name + '=')) {
                    cookieValue = decodeURIComponent(cookie.substring(name.length + 1));
                    break;
                }
            }
        }
        return cookieValue;
    }
};
/*
  jQuery countdown - 0.2
  http://code.google.com/p/jquery-utils/

  (c) Maxime Haineault <haineault@gmail.com>
  http://haineault.com   

  MIT License (http://www.opensource.org/licenses/mit-license.php)
  
*/

(function($) {
    function countdown(el, options) {
        var calc = function (target, current) {
            /* Return true if the target date has arrived,
             * an object of the time left otherwise.
             */
            var current = current || new Date();
            if (current >= target) { return true; }

            var o = {};
            var remain = Math.floor((target.getTime() - current.getTime()) / 1000);

            o.days = Math.floor(remain / 86400);
            remain %= 86400;
            o.hours = Math.floor(remain / 3600);
            remain %= 3600;
            o.minutes = Math.floor(remain / 60);
            remain %= 60;
            o.seconds = remain;
            o.years = Math.floor(o.days / 365);
            o.months = Math.floor(o.days / 30);
            o.weeks = Math.floor(o.days / 7);

            return o;
        };

        var getWeek = function(date) { 
            var onejan = new Date(date.getFullYear(),0,1);
            return Math.ceil((((date - onejan) / 86400000) + onejan.getDay())/7);
        };

        var options = $.extend({
            date: new Date(),
            modifiers: [],
            interval: 1000,
            msgFormat: '%d [day|days] %hh %mm %ss',
            msgNow: 'Now !'
        }, options);

        var tokens = {
            y: new RegExp ('\\%y(.+?)\\[(\\w+)\\|(\\w+)\\]', 'g'), // years 
            M: new RegExp ('\\%M(.+?)\\[(\\w+)\\|(\\w+)\\]', 'g'), // months 
            w: new RegExp ('\\%w(.+?)\\[(\\w+)\\|(\\w+)\\]', 'g'), // weeks
            d: new RegExp ('\\%d(.+?)\\[(\\w+)\\|(\\w+)\\]', 'g'), // days
            h: new RegExp ('\\%h(.+?)\\[(\\w+)\\|(\\w+)\\]', 'g'), // hours
            m: new RegExp ('\\%m(.+?)\\[(\\w+)\\|(\\w+)\\]', 'g'), // minutes
            s: new RegExp ('\\%s(.+?)\\[(\\w+)\\|(\\w+)\\]', 'g')  // seconds
        };

        var formatToken = function(str, token, val) {
            return (!tokens[token])? '': str.match(/\[|\]/g) 
                    && (str.replace(tokens[token], val+'$1'+ ((parseInt(val, 10)<2)?'$2':'$3')) || '')
                    || str.replace('%'+token, val);
        };

        var format = function(str, obj) {
            var o = str;
            o = formatToken(o, 'y', obj.years);
            o = formatToken(o, 'M', obj.months);
            o = formatToken(o, 'w', obj.weeks);
            o = formatToken(o, 'd', obj.days);
            o = formatToken(o, 'h', obj.hours);
            o = formatToken(o, 'm', obj.minutes);
            o = formatToken(o, 's', obj.seconds);
            return o;
        };

        var update = function() {
            var date_obj = calc(cd.date);
            if (date_obj === true) {
                cd.stop(); clearInterval(cd.id);
                $(cd.el).html(options.msgNow);
                return true;
            }
            else {
                $(cd.el).text(format(options.msgFormat, date_obj));
            }
        };

        var apply_modifiers = function (modifiers, date) {
            if (modifiers.length === 0) {
                return date;
            }

            var modifier_re = /^([+-]\d+)([yMdhms])$/;
            var conversions = {
                s: 1000,
                m: 60 * 1000,
                h: 60 * 60 * 1000,
                d: 24 * 60 * 60 * 1000,
                M: 30 * 24 * 60 * 60 * 1000,
                y: 365 * 24 * 60 * 60 * 1000
            };

            var displacement = 0;
            for (var i = 0, n = modifiers.length; i < n; ++i) {
                var match = modifiers[i].match(modifier_re);
                if (match !== null) {
                    displacement += parseInt(match[1], 10) * conversions[match[2]];
                }
            }
            return new Date(date.getTime() + displacement);
        };

        var cd = {
            id    : setInterval(update, options.interval),
            el    : el,
            start : function(){ return new countdown($(this.el), options); },
            stop  : function(){ return clearInterval(this.id); },
            date  : apply_modifiers(options.modifiers, options.date)
        };
        $(el).data('countdown', cd);
        update();
        return $(el).data('countdown');
    }
    $.fn.countdown = function(args) { if(this.get(0)) return new countdown(this.get(0), args); };
})(jQuery);
/*
 * jQuery Cycle Plugin for light-weight slideshows
 * Examples and documentation at: http://malsup.com/jquery/cycle/
 * Copyright (c) 2007-2008 M. Alsup
 * Version: 2.24 (07/30/2008)
 * Dual licensed under the MIT and GPL licenses:
 * http://www.opensource.org/licenses/mit-license.php
 * http://www.gnu.org/licenses/gpl.html
 * Requires: jQuery v1.2.3 or later
 *
 * Based on the work of:
 *  1) Matt Oakes (http://portfolio.gizone.co.uk/applications/slideshow/)
 *  2) Torsten Baldes (http://medienfreunde.com/lab/innerfade/)
 *  3) Benjamin Sterling (http://www.benjaminsterling.com/experiments/jqShuffle/)
 */
;(function($) {

var ver = '2.24';
var ie6 = $.browser.msie && /MSIE 6.0/.test(navigator.userAgent);

function log() {
    if (window.console && window.console.log)
        window.console.log('[cycle] ' + Array.prototype.join.call(arguments,''));
};

$.fn.cycle = function(options) {
    return this.each(function() {
        if (options === undefined || options === null)
            options = {};
        if (options.constructor == String) {
            switch(options) {
            case 'stop':
                if (this.cycleTimeout) clearTimeout(this.cycleTimeout);
                this.cycleTimeout = 0;
                $(this).data('cycle.opts', '');
                return;
            case 'pause':
                this.cyclePause = 1;
                return;
            case 'resume':
                this.cyclePause = 0;
                return;
            default:
                options = { fx: options };
            };
        }
        else if (options.constructor == Number) {
            // go to the requested slide slide
            var num = options;
            options = $(this).data('cycle.opts');
            if (!options) {
                log('options not found, can not advance slide');
                return;
            }
            if (num < 0 || num >= options.elements.length) {
                log('invalid slide index: ' + num);
                return;
            }
            options.nextSlide = num;
            if (this.cycleTimeout) {
                clearTimeout(this.cycleTimeout);
                this.cycleTimeout = 0;
            }            
            go(options.elements, options, 1, 1);
            return;
        }

        // stop existing slideshow for this container (if there is one)
        if (this.cycleTimeout) clearTimeout(this.cycleTimeout);
        this.cycleTimeout = 0;
        this.cyclePause = 0;
        
        var $cont = $(this);
        var $slides = options.slideExpr ? $(options.slideExpr, this) : $cont.children();
        var els = $slides.get();
        if (els.length < 2) {
            log('terminating; too few slides: ' + els.length);
            return; // don't bother
        }

        // support metadata plugin (v1.0 and v2.0)
        var opts = $.extend({}, $.fn.cycle.defaults, options || {}, $.metadata ? $cont.metadata() : $.meta ? $cont.data() : {});
        if (opts.autostop) 
            opts.countdown = opts.autostopCount || els.length;

        $cont.data('cycle.opts', opts);
        opts.container = this;

        opts.elements = els;
        opts.before = opts.before ? [opts.before] : [];
        opts.after = opts.after ? [opts.after] : [];
        opts.after.unshift(function(){ opts.busy=0; });
        if (opts.continuous)
            opts.after.push(function() { go(els,opts,0,!opts.rev); });
            
        // clearType corrections
        if (ie6 && opts.cleartype && !opts.cleartypeNoBg)
            clearTypeFix($slides);

        // allow shorthand overrides of width, height and timeout
        var cls = this.className;
        opts.width = parseInt((cls.match(/w:(\d+)/)||[])[1]) || opts.width;
        opts.height = parseInt((cls.match(/h:(\d+)/)||[])[1]) || opts.height;
        opts.timeout = parseInt((cls.match(/t:(\d+)/)||[])[1]) || opts.timeout;

        if ($cont.css('position') == 'static') 
            $cont.css('position', 'relative');
        if (opts.width) 
            $cont.width(opts.width);
        if (opts.height && opts.height != 'auto') 
            $cont.height(opts.height);

        if (opts.random) {
            opts.randomMap = [];
            for (var i = 0; i < els.length; i++) 
                opts.randomMap.push(i);
            opts.randomMap.sort(function(a,b) {return Math.random() - 0.5;});
            opts.randomIndex = 0;
            opts.startingSlide = opts.randomMap[0];
        }
        else if (opts.startingSlide >= els.length)
            opts.startingSlide = 0; // catch bogus input
        var first = opts.startingSlide || 0;
        $slides.css({position: 'absolute', top:0, left:0}).hide().each(function(i) { 
            var z = first ? i >= first ? els.length - (i-first) : first-i : els.length-i;
            $(this).css('z-index', z) 
        });
        
        $(els[first]).css('opacity',1).show(); // opacity bit needed to handle reinit case
        if ($.browser.msie) els[first].style.removeAttribute('filter');

        if (opts.fit && opts.width) 
            $slides.width(opts.width);
        if (opts.fit && opts.height && opts.height != 'auto') 
            $slides.height(opts.height);
        if (opts.pause) 
            $cont.hover(function(){this.cyclePause=1;},function(){this.cyclePause=0;});

        // run transition init fn
        var init = $.fn.cycle.transitions[opts.fx];
        if ($.isFunction(init))
            init($cont, $slides, opts);
        else if (opts.fx != 'custom')
            log('unknown transition: ' + opts.fx);
        
        $slides.each(function() {
            var $el = $(this);
            this.cycleH = (opts.fit && opts.height) ? opts.height : $el.height();
            this.cycleW = (opts.fit && opts.width) ? opts.width : $el.width();
        });

        opts.cssBefore = opts.cssBefore || {};
        opts.animIn = opts.animIn || {};
        opts.animOut = opts.animOut || {};

        $slides.not(':eq('+first+')').css(opts.cssBefore);
        if (opts.cssFirst)
            $($slides[first]).css(opts.cssFirst);

        if (opts.timeout) {
            // ensure that timeout and speed settings are sane
            if (opts.speed.constructor == String)
                opts.speed = {slow: 600, fast: 200}[opts.speed] || 400;
            if (!opts.sync)
                opts.speed = opts.speed / 2;
            while((opts.timeout - opts.speed) < 250)
                opts.timeout += opts.speed;
        }
        if (opts.easing) 
            opts.easeIn = opts.easeOut = opts.easing;
        if (!opts.speedIn) 
            opts.speedIn = opts.speed;
        if (!opts.speedOut) 
            opts.speedOut = opts.speed;

 		opts.slideCount = els.length;
        opts.currSlide = first;
        if (opts.random) {
            opts.nextSlide = opts.currSlide;
            if (++opts.randomIndex == els.length) 
                opts.randomIndex = 0;
            opts.nextSlide = opts.randomMap[opts.randomIndex];
        }
        else
            opts.nextSlide = opts.startingSlide >= (els.length-1) ? 0 : opts.startingSlide+1;

        // fire artificial events
        var e0 = $slides[first];
        if (opts.before.length)
            opts.before[0].apply(e0, [e0, e0, opts, true]);
        if (opts.after.length > 1)
            opts.after[1].apply(e0, [e0, e0, opts, true]);
        
        if (opts.click && !opts.next)
            opts.next = opts.click;
        if (opts.next)
            $(opts.next).bind('click', function(){return advance(els,opts,opts.rev?-1:1)});
        if (opts.prev)
            $(opts.prev).bind('click', function(){return advance(els,opts,opts.rev?1:-1)});
        if (opts.pager)
            buildPager(els,opts);

        // expose fn for adding slides after the show has started
        opts.addSlide = function(newSlide) {
            var $s = $(newSlide), s = $s[0];
            if (!opts.autostopCount)
                opts.countdown++;
            els.push(s);
            if (opts.els) 
                opts.els.push(s); // shuffle needs this
            opts.slideCount = els.length;
            
            $s.css('position','absolute').appendTo($cont);
            
            if (ie6 && opts.cleartype && !opts.cleartypeNoBg)
                clearTypeFix($s);

            if (opts.fit && opts.width) 
                $s.width(opts.width);
            if (opts.fit && opts.height && opts.height != 'auto') 
                $slides.height(opts.height);
            s.cycleH = (opts.fit && opts.height) ? opts.height : $s.height();
            s.cycleW = (opts.fit && opts.width) ? opts.width : $s.width();

            $s.css(opts.cssBefore);

            if (opts.pager)
                $.fn.cycle.createPagerAnchor(els.length-1, s, $(opts.pager), els, opts);
            
            if (typeof opts.onAddSlide == 'function')
                opts.onAddSlide($s);
        };

        if (opts.timeout || opts.continuous)
            this.cycleTimeout = setTimeout(
                function(){go(els,opts,0,!opts.rev)}, 
                opts.continuous ? 10 : opts.timeout + (opts.delay||0));
    });
};

function go(els, opts, manual, fwd) {
    if (opts.busy) return;
    var p = opts.container, curr = els[opts.currSlide], next = els[opts.nextSlide];
    if (p.cycleTimeout === 0 && !manual) 
        return;

    if (!manual && !p.cyclePause && 
        ((opts.autostop && (--opts.countdown <= 0)) ||
        (opts.nowrap && !opts.random && opts.nextSlide < opts.currSlide))) {
        if (opts.end)
            opts.end(opts);
        return;
    }

    if (manual || !p.cyclePause) {
        if (opts.before.length)
            $.each(opts.before, function(i,o) { o.apply(next, [curr, next, opts, fwd]); });
        var after = function() {
            if ($.browser.msie && opts.cleartype)
                this.style.removeAttribute('filter');
            $.each(opts.after, function(i,o) { o.apply(next, [curr, next, opts, fwd]); });
        };

        if (opts.nextSlide != opts.currSlide) {
            opts.busy = 1;
            if (opts.fxFn)
                opts.fxFn(curr, next, opts, after, fwd);
            else if ($.isFunction($.fn.cycle[opts.fx]))
                $.fn.cycle[opts.fx](curr, next, opts, after);
            else
                $.fn.cycle.custom(curr, next, opts, after);
        }
        if (opts.random) {
            opts.currSlide = opts.nextSlide;
            if (++opts.randomIndex == els.length) 
                opts.randomIndex = 0;
            opts.nextSlide = opts.randomMap[opts.randomIndex];
        }
        else { // sequence
            var roll = (opts.nextSlide + 1) == els.length;
            opts.nextSlide = roll ? 0 : opts.nextSlide+1;
            opts.currSlide = roll ? els.length-1 : opts.nextSlide-1;
        }
        if (opts.pager)
            $.fn.cycle.updateActivePagerLink(opts.pager, opts.currSlide);
    }
    if (opts.timeout && !opts.continuous)
        p.cycleTimeout = setTimeout(function() { go(els,opts,0,!opts.rev) }, opts.timeout);
    else if (opts.continuous && p.cyclePause) 
        p.cycleTimeout = setTimeout(function() { go(els,opts,0,!opts.rev) }, 10);
};

$.fn.cycle.updateActivePagerLink = function(pager, currSlide) {
    $(pager).find('a').removeClass('activeSlide').filter('a:eq('+currSlide+')').addClass('activeSlide');
};

// advance slide forward or back
function advance(els, opts, val) {
    var p = opts.container, timeout = p.cycleTimeout;
    if (timeout) {
        clearTimeout(timeout);
        p.cycleTimeout = 0;
    }
    if (opts.random && val < 0) {
        // move back to the previously display slide
        opts.randomIndex--;
        if (--opts.randomIndex == -2)
            opts.randomIndex = els.length-2;
        else if (opts.randomIndex == -1)
            opts.randomIndex = els.length-1;
        opts.nextSlide = opts.randomMap[opts.randomIndex];
    }
    else if (opts.random) {
        if (++opts.randomIndex == els.length) 
            opts.randomIndex = 0;
        opts.nextSlide = opts.randomMap[opts.randomIndex];
    }
    else {
        opts.nextSlide = opts.currSlide + val;
        if (opts.nextSlide < 0) {
            if (opts.nowrap) return false;
            opts.nextSlide = els.length - 1;
        }
        else if (opts.nextSlide >= els.length) {
            if (opts.nowrap) return false;
            opts.nextSlide = 0;
        }
    }
    
log('nextSlide: ' + opts.nextSlide + '; randomIndex: ' + opts.randomIndex);    
    if (opts.prevNextClick && typeof opts.prevNextClick == 'function')
        opts.prevNextClick(val > 0, opts.nextSlide, els[opts.nextSlide]);
    go(els, opts, 1, val>=0);
    return false;
};

function buildPager(els, opts) {
    var $p = $(opts.pager);
    $.each(els, function(i,o) {
        $.fn.cycle.createPagerAnchor(i,o,$p,els,opts);
    });
   $.fn.cycle.updateActivePagerLink(opts.pager, opts.startingSlide);
};

$.fn.cycle.createPagerAnchor = function(i, el, $p, els, opts) {
    var $a = (typeof opts.pagerAnchorBuilder == 'function')
        ? $(opts.pagerAnchorBuilder(i,el))
        : $('<a href="#">'+(i+1)+'</a>');
    
    // don't reparent if anchor is in the dom
    if ($a.parents('body').length == 0)
        $a.appendTo($p);
        
    $a.bind(opts.pagerEvent, function() {
        opts.nextSlide = i;
        var p = opts.container, timeout = p.cycleTimeout;
        if (timeout) {
            clearTimeout(timeout);
            p.cycleTimeout = 0;
        }            
        if (typeof opts.pagerClick == 'function')
            opts.pagerClick(opts.nextSlide, els[opts.nextSlide]);
        go(els,opts,1,opts.currSlide < i);
        return false;
    });
};


// this fixes clearType problems in ie6 by setting an explicit bg color
function clearTypeFix($slides) {
    function hex(s) {
        var s = parseInt(s).toString(16);
        return s.length < 2 ? '0'+s : s;
    };
    function getBg(e) {
        for ( ; e && e.nodeName.toLowerCase() != 'html'; e = e.parentNode) {
            var v = $.css(e,'background-color');
            if (v.indexOf('rgb') >= 0 ) { 
                var rgb = v.match(/\d+/g); 
                return '#'+ hex(rgb[0]) + hex(rgb[1]) + hex(rgb[2]);
            }
            if (v && v != 'transparent')
                return v;
        }
        return '#ffffff';
    };
    $slides.each(function() { $(this).css('background-color', getBg(this)); });
};


$.fn.cycle.custom = function(curr, next, opts, cb) {
    var $l = $(curr), $n = $(next);
    $n.css(opts.cssBefore);
    var fn = function() {$n.animate(opts.animIn, opts.speedIn, opts.easeIn, cb)};
    $l.animate(opts.animOut, opts.speedOut, opts.easeOut, function() {
        if (opts.cssAfter) $l.css(opts.cssAfter);
        if (!opts.sync) fn();
    });
    if (opts.sync) fn();
};

$.fn.cycle.transitions = {
    fade: function($cont, $slides, opts) {
        $slides.not(':eq('+opts.startingSlide+')').css('opacity',0);
        opts.before.push(function() { $(this).show() });
        opts.animIn    = { opacity: 1 };
        opts.animOut   = { opacity: 0 };
        opts.cssBefore = { opacity: 0 };
        opts.cssAfter  = { display: 'none' };
    }
};

$.fn.cycle.ver = function() { return ver; };

// override these globally if you like (they are all optional)
$.fn.cycle.defaults = {
    fx:           'fade', // one of: fade, shuffle, zoom, scrollLeft, etc
    timeout:       4000,  // milliseconds between slide transitions (0 to disable auto advance)
    continuous:    0,     // true to start next transition immediately after current one completes
    speed:         1000,  // speed of the transition (any valid fx speed value)
    speedIn:       null,  // speed of the 'in' transition
    speedOut:      null,  // speed of the 'out' transition
    next:          null,  // id of element to use as click trigger for next slide
    prev:          null,  // id of element to use as click trigger for previous slide
    prevNextClick: null,  // callback fn for prev/next clicks:  function(isNext, zeroBasedSlideIndex, slideElement)
    pager:         null,  // id of element to use as pager container
    pagerClick:    null,  // callback fn for pager clicks:  function(zeroBasedSlideIndex, slideElement)
    pagerEvent:   'click', // event which drives the pager navigation
    pagerAnchorBuilder: null, // callback fn for building anchor links
    before:        null,  // transition callback (scope set to element to be shown)
    after:         null,  // transition callback (scope set to element that was shown)
    end:           null,  // callback invoked when the slideshow terminates (use with autostop or nowrap options)
    easing:        null,  // easing method for both in and out transitions
    easeIn:        null,  // easing for "in" transition
    easeOut:       null,  // easing for "out" transition
    shuffle:       null,  // coords for shuffle animation, ex: { top:15, left: 200 }
    animIn:        null,  // properties that define how the slide animates in
    animOut:       null,  // properties that define how the slide animates out
    cssBefore:     null,  // properties that define the initial state of the slide before transitioning in
    cssAfter:      null,  // properties that defined the state of the slide after transitioning out
    fxFn:          null,  // function used to control the transition
    height:       'auto', // container height
    startingSlide: 0,     // zero-based index of the first slide to be displayed
    sync:          1,     // true if in/out transitions should occur simultaneously
    random:        0,     // true for random, false for sequence (not applicable to shuffle fx)
    fit:           0,     // force slides to fit container
    pause:         0,     // true to enable "pause on hover"
    autostop:      0,     // true to end slideshow after X transitions (where X == slide count)
    autostopCount: 0,     // number of transitions (optionally used with autostop to define X)
    delay:         0,     // additional delay (in ms) for first transition (hint: can be negative)
    slideExpr:     null,  // expression for selecting slides (if something other than all children is required)
    cleartype:     0,     // true if clearType corrections should be applied (for IE)
    nowrap:        0      // true to prevent slideshow from wrapping
};

})(jQuery);


/*
 * jQuery Cycle Plugin Transition Definitions
 * This script is a plugin for the jQuery Cycle Plugin
 * Examples and documentation at: http://malsup.com/jquery/cycle/
 * Copyright (c) 2007-2008 M. Alsup
 * Version:  2.22
 * Dual licensed under the MIT and GPL licenses:
 * http://www.opensource.org/licenses/mit-license.php
 * http://www.gnu.org/licenses/gpl.html
 */
(function($) {

//
// These functions define one-time slide initialization for the named
// transitions. To save file size feel free to remove any of these that you 
// don't need.
//

// scrollUp/Down/Left/Right
$.fn.cycle.transitions.scrollUp = function($cont, $slides, opts) {
    $cont.css('overflow','hidden');
    opts.before.push(function(curr, next, opts) {
        $(this).show();
        opts.cssBefore.top = next.offsetHeight;
        opts.animOut.top = 0-curr.offsetHeight;
    });
    opts.cssFirst = { top: 0 };
    opts.animIn   = { top: 0 };
    opts.cssAfter = { display: 'none' };
};
$.fn.cycle.transitions.scrollDown = function($cont, $slides, opts) {
    $cont.css('overflow','hidden');
    opts.before.push(function(curr, next, opts) {
        $(this).show();
        opts.cssBefore.top = 0-next.offsetHeight;
        opts.animOut.top = curr.offsetHeight;
    });
    opts.cssFirst = { top: 0 };
    opts.animIn   = { top: 0 };
    opts.cssAfter = { display: 'none' };
};
$.fn.cycle.transitions.scrollLeft = function($cont, $slides, opts) {
    $cont.css('overflow','hidden');
    opts.before.push(function(curr, next, opts) {
        $(this).show();
        opts.cssBefore.left = next.offsetWidth;
        opts.animOut.left = 0-curr.offsetWidth;
    });
    opts.cssFirst = { left: 0 };
    opts.animIn   = { left: 0 };
};
$.fn.cycle.transitions.scrollRight = function($cont, $slides, opts) {
    $cont.css('overflow','hidden');
    opts.before.push(function(curr, next, opts) {
        $(this).show();
        opts.cssBefore.left = 0-next.offsetWidth;
        opts.animOut.left = curr.offsetWidth;
    });
    opts.cssFirst = { left: 0 };
    opts.animIn   = { left: 0 };
};
$.fn.cycle.transitions.scrollHorz = function($cont, $slides, opts) {
    $cont.css('overflow','hidden').width();
//    $slides.show();
    opts.before.push(function(curr, next, opts, fwd) {
        $(this).show();
        var currW = curr.offsetWidth, nextW = next.offsetWidth;
        opts.cssBefore = fwd ? { left: nextW } : { left: -nextW };
        opts.animIn.left = 0;
        opts.animOut.left = fwd ? -currW : currW;
        $slides.not(curr).css(opts.cssBefore);
    });
    opts.cssFirst = { left: 0 };
    opts.cssAfter = { display: 'none' }
};
$.fn.cycle.transitions.scrollVert = function($cont, $slides, opts) {
    $cont.css('overflow','hidden');
//    $slides.show();
    opts.before.push(function(curr, next, opts, fwd) {
        $(this).show();
        var currH = curr.offsetHeight, nextH = next.offsetHeight;
        opts.cssBefore = fwd ? { top: -nextH } : { top: nextH };
        opts.animIn.top = 0;
        opts.animOut.top = fwd ? currH : -currH;
        $slides.not(curr).css(opts.cssBefore);
    });
    opts.cssFirst = { top: 0 };
    opts.cssAfter = { display: 'none' }
};

// slideX/slideY
$.fn.cycle.transitions.slideX = function($cont, $slides, opts) {
    opts.before.push(function(curr, next, opts) {
        $(curr).css('zIndex',1);
    });    
    opts.onAddSlide = function($s) { $s.hide(); };
    opts.cssBefore = { zIndex: 2 };
    opts.animIn  = { width: 'show' };
    opts.animOut = { width: 'hide' };
};
$.fn.cycle.transitions.slideY = function($cont, $slides, opts) {
    opts.before.push(function(curr, next, opts) {
        $(curr).css('zIndex',1);
    });    
    opts.onAddSlide = function($s) { $s.hide(); };
    opts.cssBefore = { zIndex: 2 };
    opts.animIn  = { height: 'show' };
    opts.animOut = { height: 'hide' };
};

// shuffle
$.fn.cycle.transitions.shuffle = function($cont, $slides, opts) {
    var w = $cont.css('overflow', 'visible').width();
    $slides.css({left: 0, top: 0});
    opts.before.push(function() { $(this).show() });
    opts.speed = opts.speed / 2; // shuffle has 2 transitions        
    opts.random = 0;
    opts.shuffle = opts.shuffle || {left:-w, top:15};
    opts.els = [];
    for (var i=0; i < $slides.length; i++)
        opts.els.push($slides[i]);

    for (var i=0; i < opts.startingSlide; i++)
        opts.els.push(opts.els.shift());

    // custom transition fn (hat tip to Benjamin Sterling for this bit of sweetness!)
    opts.fxFn = function(curr, next, opts, cb, fwd) {
        var $el = fwd ? $(curr) : $(next);
        $el.animate(opts.shuffle, opts.speedIn, opts.easeIn, function() {
            fwd ? opts.els.push(opts.els.shift()) : opts.els.unshift(opts.els.pop());
            if (fwd) 
                for (var i=0, len=opts.els.length; i < len; i++)
                    $(opts.els[i]).css('z-index', len-i);
            else {
                var z = $(curr).css('z-index');
                $el.css('z-index', parseInt(z)+1);
            }
            $el.animate({left:0, top:0}, opts.speedOut, opts.easeOut, function() {
                $(fwd ? this : curr).hide();
                if (cb) cb();
            });
        });
    };
    opts.onAddSlide = function($s) { $s.hide(); };
};

// turnUp/Down/Left/Right
$.fn.cycle.transitions.turnUp = function($cont, $slides, opts) {
    opts.before.push(function(curr, next, opts) {
        $(this).show();
        opts.cssBefore.top = next.cycleH;
        opts.animIn.height = next.cycleH;
    });
    opts.onAddSlide = function($s) { $s.hide(); };
    opts.cssFirst  = { top: 0 };
    opts.cssBefore = { height: 0 };
    opts.animIn    = { top: 0 };
    opts.animOut   = { height: 0 };
    opts.cssAfter  = { display: 'none' };
};
$.fn.cycle.transitions.turnDown = function($cont, $slides, opts) {
    opts.before.push(function(curr, next, opts) {
        $(this).show();
        opts.animIn.height = next.cycleH;
        opts.animOut.top   = curr.cycleH;
    });
    opts.onAddSlide = function($s) { $s.hide(); };
    opts.cssFirst  = { top: 0 };
    opts.cssBefore = { top: 0, height: 0 };
    opts.animOut   = { height: 0 };
    opts.cssAfter  = { display: 'none' };
};
$.fn.cycle.transitions.turnLeft = function($cont, $slides, opts) {
    opts.before.push(function(curr, next, opts) {
        $(this).show();
        opts.cssBefore.left = next.cycleW;
        opts.animIn.width = next.cycleW;
    });
    opts.onAddSlide = function($s) { $s.hide(); };
    opts.cssBefore = { width: 0 };
    opts.animIn    = { left: 0 };
    opts.animOut   = { width: 0 };
    opts.cssAfter  = { display: 'none' };
};
$.fn.cycle.transitions.turnRight = function($cont, $slides, opts) {
    opts.before.push(function(curr, next, opts) {
        $(this).show();
        opts.animIn.width = next.cycleW;
        opts.animOut.left = curr.cycleW;
    });
    opts.onAddSlide = function($s) { $s.hide(); };
    opts.cssBefore = { left: 0, width: 0 };
    opts.animIn    = { left: 0 };
    opts.animOut   = { width: 0 };
    opts.cssAfter  = { display: 'none' };
};

// zoom
$.fn.cycle.transitions.zoom = function($cont, $slides, opts) {
    opts.cssFirst = { top:0, left: 0 }; 
    opts.cssAfter = { display: 'none' };
    
    opts.before.push(function(curr, next, opts) {
        $(this).show();
        opts.cssBefore = { width: 0, height: 0, top: next.cycleH/2, left: next.cycleW/2 };
        opts.cssAfter  = { display: 'none' };
        opts.animIn    = { top: 0, left: 0, width: next.cycleW, height: next.cycleH };
        opts.animOut   = { width: 0, height: 0, top: curr.cycleH/2, left: curr.cycleW/2 };
        $(curr).css('zIndex',2);
        $(next).css('zIndex',1);
    });    
    opts.onAddSlide = function($s) { $s.hide(); };
};

// fadeZoom
$.fn.cycle.transitions.fadeZoom = function($cont, $slides, opts) {
    opts.before.push(function(curr, next, opts) {
        opts.cssBefore = { width: 0, height: 0, opacity: 1, left: next.cycleW/2, top: next.cycleH/2, zIndex: 1 };
        opts.animIn    = { top: 0, left: 0, width: next.cycleW, height: next.cycleH };
    });    
    opts.animOut  = { opacity: 0 };
    opts.cssAfter = { zIndex: 0 };
};

// blindX
$.fn.cycle.transitions.blindX = function($cont, $slides, opts) {
    var w = $cont.css('overflow','hidden').width();
    $slides.show();
    opts.before.push(function(curr, next, opts) {
        $(curr).css('zIndex',1);
    });    
    opts.cssBefore = { left: w, zIndex: 2 };
    opts.cssAfter = { zIndex: 1 };
    opts.animIn = { left: 0 };
    opts.animOut  = { left: w };
};
// blindY
$.fn.cycle.transitions.blindY = function($cont, $slides, opts) {
    var h = $cont.css('overflow','hidden').height();
    $slides.show();
    opts.before.push(function(curr, next, opts) {
        $(curr).css('zIndex',1);
    });    
    opts.cssBefore = { top: h, zIndex: 2 };
    opts.cssAfter = { zIndex: 1 };
    opts.animIn = { top: 0 };
    opts.animOut  = { top: h };
};
// blindZ
$.fn.cycle.transitions.blindZ = function($cont, $slides, opts) {
    var h = $cont.css('overflow','hidden').height();
    var w = $cont.width();
    $slides.show();
    opts.before.push(function(curr, next, opts) {
        $(curr).css('zIndex',1);
    });    
    opts.cssBefore = { top: h, left: w, zIndex: 2 };
    opts.cssAfter = { zIndex: 1 };
    opts.animIn = { top: 0, left: 0 };
    opts.animOut  = { top: h, left: w };
};

// growX - grow horizontally from centered 0 width
$.fn.cycle.transitions.growX = function($cont, $slides, opts) {
    opts.before.push(function(curr, next, opts) {
        opts.cssBefore = { left: this.cycleW/2, width: 0, zIndex: 2 };
        opts.animIn = { left: 0, width: this.cycleW };
        opts.animOut = { left: 0 };
        $(curr).css('zIndex',1);
    });    
    opts.onAddSlide = function($s) { $s.hide().css('zIndex',1); };
};
// growY - grow vertically from centered 0 height
$.fn.cycle.transitions.growY = function($cont, $slides, opts) {
    opts.before.push(function(curr, next, opts) {
        opts.cssBefore = { top: this.cycleH/2, height: 0, zIndex: 2 };
        opts.animIn = { top: 0, height: this.cycleH };
        opts.animOut = { top: 0 };
        $(curr).css('zIndex',1);
    });    
    opts.onAddSlide = function($s) { $s.hide().css('zIndex',1); };
};

// curtainX - squeeze in both edges horizontally
$.fn.cycle.transitions.curtainX = function($cont, $slides, opts) {
    opts.before.push(function(curr, next, opts) {
        opts.cssBefore = { left: next.cycleW/2, width: 0, zIndex: 1, display: 'block' };
        opts.animIn = { left: 0, width: this.cycleW };
        opts.animOut = { left: curr.cycleW/2, width: 0 };
        $(curr).css('zIndex',2);
    });    
    opts.onAddSlide = function($s) { $s.hide(); };
    opts.cssAfter = { zIndex: 1, display: 'none' };
};
// curtainY - squeeze in both edges vertically
$.fn.cycle.transitions.curtainY = function($cont, $slides, opts) {
    opts.before.push(function(curr, next, opts) {
        opts.cssBefore = { top: next.cycleH/2, height: 0, zIndex: 1, display: 'block' };
        opts.animIn = { top: 0, height: this.cycleH };
        opts.animOut = { top: curr.cycleH/2, height: 0 };
        $(curr).css('zIndex',2);
    });    
    opts.onAddSlide = function($s) { $s.hide(); };
    opts.cssAfter = { zIndex: 1, display: 'none' };
};

// cover - curr slide covered by next slide
$.fn.cycle.transitions.cover = function($cont, $slides, opts) {
    var d = opts.direction || 'left';
    var w = $cont.css('overflow','hidden').width();
    var h = $cont.height();
    opts.before.push(function(curr, next, opts) {
        opts.cssBefore = opts.cssBefore || {};
        opts.cssBefore.zIndex = 2;
        opts.cssBefore.display = 'block';
        
        if (d == 'right') 
            opts.cssBefore.left = -w;
        else if (d == 'up')    
            opts.cssBefore.top = h;
        else if (d == 'down')  
            opts.cssBefore.top = -h;
        else
            opts.cssBefore.left = w;
        $(curr).css('zIndex',1);
    });    
    if (!opts.animIn)  opts.animIn = { left: 0, top: 0 };
    if (!opts.animOut) opts.animOut = { left: 0, top: 0 };
    opts.cssAfter = opts.cssAfter || {};
    opts.cssAfter.zIndex = 2;
    opts.cssAfter.display = 'none';
};

// uncover - curr slide moves off next slide
$.fn.cycle.transitions.uncover = function($cont, $slides, opts) {
    var d = opts.direction || 'left';
    var w = $cont.css('overflow','hidden').width();
    var h = $cont.height();
    opts.before.push(function(curr, next, opts) {
        opts.cssBefore.display = 'block';
        if (d == 'right') 
            opts.animOut.left = w;
        else if (d == 'up')    
            opts.animOut.top = -h;
        else if (d == 'down')  
            opts.animOut.top = h;
        else
            opts.animOut.left = -w;
        $(curr).css('zIndex',2);
        $(next).css('zIndex',1);
    });    
    opts.onAddSlide = function($s) { $s.hide(); };
    if (!opts.animIn)  opts.animIn = { left: 0, top: 0 };
    opts.cssBefore = opts.cssBefore || {};
    opts.cssBefore.top = 0;
    opts.cssBefore.left = 0;
    
    opts.cssAfter = opts.cssAfter || {};
    opts.cssAfter.zIndex = 1;
    opts.cssAfter.display = 'none';
};

// toss - move top slide and fade away
$.fn.cycle.transitions.toss = function($cont, $slides, opts) {
    var w = $cont.css('overflow','visible').width();
    var h = $cont.height();
    opts.before.push(function(curr, next, opts) {
        $(curr).css('zIndex',2);
        opts.cssBefore.display = 'block'; 
        // provide default toss settings if animOut not provided
        if (!opts.animOut.left && !opts.animOut.top)
            opts.animOut = { left: w*2, top: -h/2, opacity: 0 };
        else
            opts.animOut.opacity = 0;
    });    
    opts.onAddSlide = function($s) { $s.hide(); };
    opts.cssBefore = { left: 0, top: 0, zIndex: 1, opacity: 1 };
    opts.animIn = { left: 0 };
    opts.cssAfter = { zIndex: 2, display: 'none' };
};

// wipe - clip animation
$.fn.cycle.transitions.wipe = function($cont, $slides, opts) {
    var w = $cont.css('overflow','hidden').width();
    var h = $cont.height();
    opts.cssBefore = opts.cssBefore || {};
    var clip;
    if (opts.clip) {
        if (/l2r/.test(opts.clip))
            clip = 'rect(0px 0px '+h+'px 0px)';
        else if (/r2l/.test(opts.clip))
            clip = 'rect(0px '+w+'px '+h+'px '+w+'px)';
        else if (/t2b/.test(opts.clip))
            clip = 'rect(0px '+w+'px 0px 0px)';
        else if (/b2t/.test(opts.clip))
            clip = 'rect('+h+'px '+w+'px '+h+'px 0px)';
        else if (/zoom/.test(opts.clip)) {
            var t = parseInt(h/2);
            var l = parseInt(w/2);
            clip = 'rect('+t+'px '+l+'px '+t+'px '+l+'px)';
        }
    }
    
    opts.cssBefore.clip = opts.cssBefore.clip || clip || 'rect(0px 0px 0px 0px)';
    
    var d = opts.cssBefore.clip.match(/(\d+)/g);
    var t = parseInt(d[0]), r = parseInt(d[1]), b = parseInt(d[2]), l = parseInt(d[3]);
    
    opts.before.push(function(curr, next, opts) {
        if (curr == next) return;
        var $curr = $(curr).css('zIndex',2);
        var $next = $(next).css({
            zIndex:  3,
            display: 'block'
        });
        
        var step = 1, count = parseInt((opts.speedIn / 13)) - 1;
        function f() {
            var tt = t ? t - parseInt(step * (t/count)) : 0;
            var ll = l ? l - parseInt(step * (l/count)) : 0;
            var bb = b < h ? b + parseInt(step * ((h-b)/count || 1)) : h;
            var rr = r < w ? r + parseInt(step * ((w-r)/count || 1)) : w;
            $next.css({ clip: 'rect('+tt+'px '+rr+'px '+bb+'px '+ll+'px)' });
            (step++ <= count) ? setTimeout(f, 13) : $curr.css('display', 'none');
        }
        f();
    });    
    opts.cssAfter  = { };
    opts.animIn    = { left: 0 };
    opts.animOut   = { left: 0 };
};

})(jQuery);
/*
 jQuery delayed observer - 0.8
 http://code.google.com/p/jquery-utils/

 (c) Maxime Haineault <haineault@gmail.com>
 http://haineault.com
 
 MIT License (http://www.opensource.org/licenses/mit-license.php)
 
*/

(function($){
    $.extend($.fn, {
        delayedObserver: function(callback, delay, options){
            return this.each(function(){
                var el = $(this);
                var op = options || {};
                el.data('oldval', el.val())
                    .data('delay', delay || 0.5)
                    .data('condition', op.condition || function() { return ($(this).data('oldval') == $(this).val()); })
                    .data('callback', callback)
                    [(op.event||'keyup')](function(){
                        if (el.data('condition').apply(el)) { return; }
                        else {
                            if (el.data('timer')) { clearTimeout(el.data('timer')); }
                            el.data('timer', setTimeout(function(){
                                el.data('callback').apply(el);
                            }, el.data('delay') * 1000));
                            el.data('oldval', el.val());
                        }
                    });
            });
        }
    });
})(jQuery);
/**
 * Flash (http://jquery.lukelutman.com/plugins/flash)
 * A jQuery plugin for embedding Flash movies.
 * 
 * Version 1.0
 * November 9th, 2006
 *
 * Copyright (c) 2006 Luke Lutman (http://www.lukelutman.com)
 * Dual licensed under the MIT and GPL licenses.
 * http://www.opensource.org/licenses/mit-license.php
 * http://www.opensource.org/licenses/gpl-license.php
 * 
 * Inspired by:
 * SWFObject (http://blog.deconcept.com/swfobject/)
 * UFO (http://www.bobbyvandersluis.com/ufo/)
 * sIFR (http://www.mikeindustries.com/sifr/)
 * 
 * IMPORTANT: 
 * The packed version of jQuery breaks ActiveX control
 * activation in Internet Explorer. Use JSMin to minifiy
 * jQuery (see: http://jquery.lukelutman.com/plugins/flash#activex).
 *
 **/ 
;(function(){
	
var $$;

/**
 * 
 * @desc Replace matching elements with a flash movie.
 * @author Luke Lutman
 * @version 1.0.1
 *
 * @name flash
 * @param Hash htmlOptions Options for the embed/object tag.
 * @param Hash pluginOptions Options for detecting/updating the Flash plugin (optional).
 * @param Function replace Custom block called for each matched element if flash is installed (optional).
 * @param Function update Custom block called for each matched if flash isn't installed (optional).
 * @type jQuery
 *
 * @cat plugins/flash
 * 
 * @example $('#hello').flash({ src: 'hello.swf' });
 * @desc Embed a Flash movie.
 *
 * @example $('#hello').flash({ src: 'hello.swf' }, { version: 8 });
 * @desc Embed a Flash 8 movie.
 *
 * @example $('#hello').flash({ src: 'hello.swf' }, { expressInstall: true });
 * @desc Embed a Flash movie using Express Install if flash isn't installed.
 *
 * @example $('#hello').flash({ src: 'hello.swf' }, { update: false });
 * @desc Embed a Flash movie, don't show an update message if Flash isn't installed.
 *
**/
$$ = jQuery.fn.flash = function(htmlOptions, pluginOptions, replace, update) {
	
	// Set the default block.
	var block = replace || $$.replace;
	
	// Merge the default and passed plugin options.
	pluginOptions = $$.copy($$.pluginOptions, pluginOptions);
	
	// Detect Flash.
	if(!$$.hasFlash(pluginOptions.version)) {
		// Use Express Install (if specified and Flash plugin 6,0,65 or higher is installed).
		if(pluginOptions.expressInstall && $$.hasFlash(6,0,65)) {
			// Add the necessary flashvars (merged later).
			var expressInstallOptions = {
				flashvars: {  	
					MMredirectURL: location,
					MMplayerType: 'PlugIn',
					MMdoctitle: jQuery('title').text() 
				}					
			};
		// Ask the user to update (if specified).
		} else if (pluginOptions.update) {
			// Change the block to insert the update message instead of the flash movie.
			block = update || $$.update;
		// Fail
		} else {
			// The required version of flash isn't installed.
			// Express Install is turned off, or flash 6,0,65 isn't installed.
			// Update is turned off.
			// Return without doing anything.
			return this;
		}
	}
	
	// Merge the default, express install and passed html options.
	htmlOptions = $$.copy($$.htmlOptions, expressInstallOptions, htmlOptions);
	
	// Invoke $block (with a copy of the merged html options) for each element.
	return this.each(function(){
		block.call(this, $$.copy(htmlOptions));
	});
	
};
/**
 *
 * @name flash.copy
 * @desc Copy an arbitrary number of objects into a new object.
 * @type Object
 * 
 * @example $$.copy({ foo: 1 }, { bar: 2 });
 * @result { foo: 1, bar: 2 };
 *
**/
$$.copy = function() {
	var options = {}, flashvars = {};
	for(var i = 0; i < arguments.length; i++) {
		var arg = arguments[i];
		if(arg == undefined) continue;
		jQuery.extend(options, arg);
		// don't clobber one flash vars object with another
		// merge them instead
		if(arg.flashvars == undefined) continue;
		jQuery.extend(flashvars, arg.flashvars);
	}
	options.flashvars = flashvars;
	return options;
};
/*
 * @name flash.hasFlash
 * @desc Check if a specific version of the Flash plugin is installed
 * @type Boolean
 *
**/
$$.hasFlash = function() {
	// look for a flag in the query string to bypass flash detection
	if(/hasFlash\=true/.test(location)) return true;
	if(/hasFlash\=false/.test(location)) return false;
	var pv = $$.hasFlash.playerVersion().match(/\d+/g);
	var rv = String([arguments[0], arguments[1], arguments[2]]).match(/\d+/g) || String($$.pluginOptions.version).match(/\d+/g);
	for(var i = 0; i < 3; i++) {
		pv[i] = parseInt(pv[i] || 0);
		rv[i] = parseInt(rv[i] || 0);
		// player is less than required
		if(pv[i] < rv[i]) return false;
		// player is greater than required
		if(pv[i] > rv[i]) return true;
	}
	// major version, minor version and revision match exactly
	return true;
};
/**
 *
 * @name flash.hasFlash.playerVersion
 * @desc Get the version of the installed Flash plugin.
 * @type String
 *
**/
$$.hasFlash.playerVersion = function() {
	// ie
	try {
		try {
			// avoid fp6 minor version lookup issues
			// see: http://blog.deconcept.com/2006/01/11/getvariable-setvariable-crash-internet-explorer-flash-6/
			var axo = new ActiveXObject('ShockwaveFlash.ShockwaveFlash.6');
			try { axo.AllowScriptAccess = 'always';	} 
			catch(e) { return '6,0,0'; }				
		} catch(e) {}
		return new ActiveXObject('ShockwaveFlash.ShockwaveFlash').GetVariable('$version').replace(/\D+/g, ',').match(/^,?(.+),?$/)[1];
	// other browsers
	} catch(e) {
		try {
			if(navigator.mimeTypes["application/x-shockwave-flash"].enabledPlugin){
				return (navigator.plugins["Shockwave Flash 2.0"] || navigator.plugins["Shockwave Flash"]).description.replace(/\D+/g, ",").match(/^,?(.+),?$/)[1];
			}
		} catch(e) {}		
	}
	return '0,0,0';
};
/**
 *
 * @name flash.htmlOptions
 * @desc The default set of options for the object or embed tag.
 *
**/
$$.htmlOptions = {
	height: 240,
	flashvars: {},
	pluginspage: 'http://www.adobe.com/go/getflashplayer',
	src: '#',
	type: 'application/x-shockwave-flash',
	width: 320		
};
/**
 *
 * @name flash.pluginOptions
 * @desc The default set of options for checking/updating the flash Plugin.
 *
**/
$$.pluginOptions = {
	expressInstall: false,
	update: true,
	version: '6.0.65'
};
/**
 *
 * @name flash.replace
 * @desc The default method for replacing an element with a Flash movie.
 *
**/
$$.replace = function(htmlOptions) {
	this.innerHTML = '<div class="alt">'+this.innerHTML+'</div>';
	jQuery(this)
		.addClass('flash-replaced')
		.prepend($$.transform(htmlOptions));
};
/**
 *
 * @name flash.update
 * @desc The default method for replacing an element with an update message.
 *
**/
$$.update = function(htmlOptions) {
	var url = String(location).split('?');
	url.splice(1,0,'?hasFlash=true&');
	url = url.join('');
	var msg = '<p>This content requires the Flash Player. <a href="http://www.adobe.com/go/getflashplayer">Download Flash Player</a>. Already have Flash Player? <a href="'+url+'">Click here.</a></p>';
	this.innerHTML = '<span class="alt">'+this.innerHTML+'</span>';
	jQuery(this)
		.addClass('flash-update')
		.prepend(msg);
};
/**
 *
 * @desc Convert a hash of html options to a string of attributes, using Function.apply(). 
 * @example toAttributeString.apply(htmlOptions)
 * @result foo="bar" foo="bar"
 *
**/
function toAttributeString() {
	var s = '';
	for(var key in this)
		if(typeof this[key] != 'function')
			s += key+'="'+this[key]+'" ';
	return s;		
};
/**
 *
 * @desc Convert a hash of flashvars to a url-encoded string, using Function.apply(). 
 * @example toFlashvarsString.apply(flashvarsObject)
 * @result foo=bar&foo=bar
 *
**/
function toFlashvarsString() {
	var s = '';
	for(var key in this)
		if(typeof this[key] != 'function')
			s += key+'='+encodeURIComponent(this[key])+'&';
	return s.replace(/&$/, '');		
};
/**
 *
 * @name flash.transform
 * @desc Transform a set of html options into an embed tag.
 * @type String 
 *
 * @example $$.transform(htmlOptions)
 * @result <embed src="foo.swf" ... />
 *
 * Note: The embed tag is NOT standards-compliant, but it 
 * works in all current browsers. flash.transform can be
 * overwritten with a custom function to generate more 
 * standards-compliant markup.
 *
**/
$$.transform = function(htmlOptions) {
	htmlOptions.toString = toAttributeString;
	if(htmlOptions.flashvars) htmlOptions.flashvars.toString = toFlashvarsString;
	return '<embed ' + String(htmlOptions) + '/>';		
};

/**
 *
 * Flash Player 9 Fix (http://blog.deconcept.com/2006/07/28/swfobject-143-released/)
 *
**/
if (window.attachEvent) {
	window.attachEvent("onbeforeunload", function(){
		__flash_unloadHandler = function() {};
		__flash_savedUnloadHandler = function() {};
	});
}
	
})();
(function($){
    $._i18n = { trans: {}, 'default':  'en', language: 'en' };
    $.i18n = function() {
        var getTrans = function(ns, str) {
            var trans = false;
            // check if string exists in translation
            if ($._i18n.trans[$._i18n.language] 
                && $._i18n.trans[$._i18n.language][ns]
                && $._i18n.trans[$._i18n.language][ns][str]) {
                trans = $._i18n.trans[$._i18n.language][ns][str];
            }
            // or exists in default
            else if ($._i18n.trans[$._i18n['default']] 
                     && $._i18n.trans[$._i18n['default']][ns]
                     && $._i18n.trans[$._i18n['default']][ns][str]) {
                trans = $._i18n.trans[$._i18n['default']][ns][str];
            }
            // return trans or original string
            return trans || str;
        };
        // Set language (accepted formats: en or en-US)
        if (arguments.length < 2) {
            $._i18n.language = arguments[0]; 
            return $._i18n.language;
        }
        else {
            // get translation
            if (typeof(arguments[1]) == 'string') {
                var trans = getTrans(arguments[0], arguments[1]);
                // has variables for string formating
                if (arguments[2] && typeof(arguments[2]) == 'object') {
                    return $.format(trans, arguments[2]);
                }
                else {
                    return trans;
                }
            }
            // set translation
            else {
                var tmp  = arguments[0].split('.');
                var lang = tmp[0];
                var ns   = tmp[1] || 'jQuery';
                if (!$._i18n.trans[lang]) {
                    $._i18n.trans[lang] = {};
                    $._i18n.trans[lang][ns] = arguments[1];
                }
                else {
                    $.extend($._i18n.trans[lang][ns], arguments[1]);
                }
            }
        }
    };
})(jQuery);
/*
 * Copyright (c) 2007-2008 Josh Bush (digitalbush.com)
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE. 
 */
 
/*
 * Version: 1.1.3
 * Release: 2008-04-16
 */ 
(function($) {

	//Helper Function for Caret positioning
	$.fn.caret=function(begin,end){	
		if(this.length==0) return;
		if (typeof begin == 'number') {
            end = (typeof end == 'number')?end:begin;  
			return this.each(function(){
				if(this.setSelectionRange){
					this.focus();
					this.setSelectionRange(begin,end);
				}else if (this.createTextRange){
					var range = this.createTextRange();
					range.collapse(true);
					range.moveEnd('character', end);
					range.moveStart('character', begin);
					range.select();
				}
			});
        } else {
            if (this[0].setSelectionRange){
				begin = this[0].selectionStart;
				end = this[0].selectionEnd;
			}else if (document.selection && document.selection.createRange){
				var range = document.selection.createRange();			
				begin = 0 - range.duplicate().moveStart('character', -100000);
				end = begin + range.text.length;
			}
			return {begin:begin,end:end};
        }       
	};

	//Predefined character definitions
	var charMap={
		'9':"[0-9]",
		'a':"[A-Za-z]",
		'*':"[A-Za-z0-9]"
	};
	
	//Helper method to inject character definitions
	$.mask={
		addPlaceholder : function(c,r){
			charMap[c]=r;
		}
	};
	
	$.fn.unmask=function(){
		return this.trigger("unmask");
	};
	
	//Main Method
	$.fn.mask = function(mask,settings) {	
		settings = $.extend({
			placeholder: "_",			
			completed: null
		}, settings);		
		
		//Build Regex for format validation
		var re = new RegExp("^"+	
		$.map( mask.split(""), function(c,i){		  		  
		  return charMap[c]||((/[A-Za-z0-9]/.test(c)?"":"\\")+c);
		}).join('')+				
		"$");		

		return this.each(function(){		
			var input=$(this);
			var buffer=new Array(mask.length);
			var locked=new Array(mask.length);
			var valid=false;   
			var ignore=false;  			//Variable for ignoring control keys
			var firstNonMaskPos=null; 
			
			//Build buffer layout from mask & determine the first non masked character			
			$.each( mask.split(""), function(i,c){				
				locked[i]=(charMap[c]==null);				
				buffer[i]=locked[i]?c:settings.placeholder;									
				if(!locked[i] && firstNonMaskPos==null)
					firstNonMaskPos=i;
			});		
			
			function focusEvent(){					
				checkVal();
				writeBuffer();
				setTimeout(function(){
					$(input[0]).caret(valid?mask.length:firstNonMaskPos);					
				},0);
			};
			
			function keydownEvent(e){				
				var pos=$(this).caret();
				var k = e.keyCode;
				ignore=(k < 16 || (k > 16 && k < 32 ) || (k > 32 && k < 41));
				
				//delete selection before proceeding
				if((pos.begin-pos.end)!=0 && (!ignore || k==8 || k==46)){
					clearBuffer(pos.begin,pos.end);
				}	
				//backspace and delete get special treatment
				if(k==8){//backspace					
					while(pos.begin-->=0){
						if(!locked[pos.begin]){								
							buffer[pos.begin]=settings.placeholder;
							if($.browser.opera){
								//Opera won't let you cancel the backspace, so we'll let it backspace over a dummy character.								
								s=writeBuffer();
								input.val(s.substring(0,pos.begin)+" "+s.substring(pos.begin));
								$(this).caret(pos.begin+1);								
							}else{
								writeBuffer();
								$(this).caret(Math.max(firstNonMaskPos,pos.begin));								
							}									
							return false;								
						}
					}						
				}else if(k==46){//delete
					clearBuffer(pos.begin,pos.begin+1);
					writeBuffer();
					$(this).caret(Math.max(firstNonMaskPos,pos.begin));					
					return false;
				}else if (k==27){//escape
					clearBuffer(0,mask.length);
					writeBuffer();
					$(this).caret(firstNonMaskPos);					
					return false;
				}									
			};
			
			function keypressEvent(e){					
				if(ignore){
					ignore=false;
					//Fixes Mac FF bug on backspace
					return (e.keyCode == 8)? false: null;
				}
				e=e||window.event;
				var k=e.charCode||e.keyCode||e.which;						
				var pos=$(this).caret();
								
				if(e.ctrlKey || e.altKey){//Ignore
					return true;
				}else if ((k>=41 && k<=122) ||k==32 || k>186){//typeable characters
					var p=seekNext(pos.begin-1);					
					if(p<mask.length){
						if(new RegExp(charMap[mask.charAt(p)]).test(String.fromCharCode(k))){
							buffer[p]=String.fromCharCode(k);									
							writeBuffer();
							var next=seekNext(p);
							$(this).caret(next);
							if(settings.completed && next == mask.length)
								settings.completed.call(input);
						}				
					}
				}				
				return false;				
			};
			
			function clearBuffer(start,end){
				for(var i=start;i<end&&i<mask.length;i++){
					if(!locked[i])
						buffer[i]=settings.placeholder;
				}				
			};
			
			function writeBuffer(){				
				return input.val(buffer.join('')).val();				
			};
			
			function checkVal(){	
				//try to place charcters where they belong
				var test=input.val();
				var pos=0;
				for(var i=0;i<mask.length;i++){					
					if(!locked[i]){
						buffer[i]=settings.placeholder;
						while(pos++<test.length){
							//Regex Test each char here.
							var reChar=new RegExp(charMap[mask.charAt(i)]);
							if(test.charAt(pos-1).match(reChar)){
								buffer[i]=test.charAt(pos-1);								
								break;
							}									
						}
					}
				}
				var s=writeBuffer();
				if(!s.match(re)){							
					input.val("");	
					clearBuffer(0,mask.length);
					valid=false;
				}else
					valid=true;
			};
			
			function seekNext(pos){				
				while(++pos<mask.length){					
					if(!locked[pos])
						return pos;
				}
				return mask.length;
			};
			
			input.one("unmask",function(){
				input.unbind("focus",focusEvent);
				input.unbind("blur",checkVal);
				input.unbind("keydown",keydownEvent);
				input.unbind("keypress",keypressEvent);
				if ($.browser.msie) 
					this.onpaste= null;                     
				else if ($.browser.mozilla)
					this.removeEventListener('input',checkVal,false);
			});
			input.bind("focus",focusEvent);
			input.bind("blur",checkVal);
			input.bind("keydown",keydownEvent);
			input.bind("keypress",keypressEvent);
			//Paste events for IE and Mozilla thanks to Kristinn Sigmundsson
			if ($.browser.msie) 
				this.onpaste= function(){setTimeout(checkVal,0);};                     
			else if ($.browser.mozilla)
				this.addEventListener('input',checkVal,false);
				
			checkVal();//Perform initial check for existing values
		});
	};
})(jQuery);
/* Copyright (c) 2006 Brandon Aaron (brandon.aaron@gmail.com || http://brandonaaron.net)
 * Dual licensed under the MIT (http://www.opensource.org/licenses/mit-license.php)
 * and GPL (http://www.opensource.org/licenses/gpl-license.php) licenses.
 * Thanks to: http://adomas.org/javascript-mouse-wheel/ for some pointers.
 * Thanks to: Mathias Bank(http://www.mathias-bank.de) for a scope bug fix.
 *
 * $LastChangedDate: 2007-12-20 09:02:08 -0600 (Thu, 20 Dec 2007) $
 * $Rev: 4265 $
 *
 * Version: 3.0
 * 
 * Requires: $ 1.2.2+
 */

(function($) {

$.event.special.mousewheel = {
	setup: function() {
		var handler = $.event.special.mousewheel.handler;
		
		// Fix pageX, pageY, clientX and clientY for mozilla
		if ( $.browser.mozilla )
			$(this).bind('mousemove.mousewheel', function(event) {
				$.data(this, 'mwcursorposdata', {
					pageX: event.pageX,
					pageY: event.pageY,
					clientX: event.clientX,
					clientY: event.clientY
				});
			});
	
		if ( this.addEventListener )
			this.addEventListener( ($.browser.mozilla ? 'DOMMouseScroll' : 'mousewheel'), handler, false);
		else
			this.onmousewheel = handler;
	},
	
	teardown: function() {
		var handler = $.event.special.mousewheel.handler;
		
		$(this).unbind('mousemove.mousewheel');
		
		if ( this.removeEventListener )
			this.removeEventListener( ($.browser.mozilla ? 'DOMMouseScroll' : 'mousewheel'), handler, false);
		else
			this.onmousewheel = function(){};
		
		$.removeData(this, 'mwcursorposdata');
	},
	
	handler: function(event) {
		var args = Array.prototype.slice.call( arguments, 1 );
		
		event = $.event.fix(event || window.event);
		// Get correct pageX, pageY, clientX and clientY for mozilla
		$.extend( event, $.data(this, 'mwcursorposdata') || {} );
		var delta = 0, returnValue = true;
		
		if ( event.wheelDelta ) delta = event.wheelDelta/120;
		if ( event.detail     ) delta = -event.detail/3;
		if ( $.browser.opera  ) delta = -event.wheelDelta;
		
		event.data  = event.data || {};
		event.type  = "mousewheel";
		
		// Add delta to the front of the arguments
		args.unshift(delta);
		// Add event to the front of the arguments
		args.unshift(event);

		return $.event.handle.apply(this, args);
	}
};

$.fn.extend({
	mousewheel: function(fn) {
		return fn ? this.bind("mousewheel", fn) : this.trigger("mousewheel");
	},
	
	unmousewheel: function(fn) {
		return this.unbind("mousewheel", fn);
	}
});

})(jQuery);/*!
        Slimbox v2.02 - The ultimate lightweight Lightbox clone for jQuery
        (c) 2007-2009 Christophe Beyls <http://www.digitalia.be>
        MIT-style license.
*/

(function($) {

        // Global variables, accessible to Slimbox only
        var win = $(window), options, images, activeImage = -1, activeURL, prevImage, nextImage, compatibleOverlay, middle, centerWidth, centerHeight, ie6 = !window.XMLHttpRequest,
                operaFix = window.opera && (document.compatMode == "CSS1Compat") && ($.browser.version >= 9.3), documentElement = document.documentElement,

        // Preload images
        preload = {}, preloadPrev = new Image(), preloadNext = new Image(),

        // DOM elements
        overlay, center, image, sizer, prevLink, nextLink, bottomContainer, bottom, caption, number;

        /*
                Initialization
        */

        $(function() {
                // Append the Slimbox HTML code at the bottom of the document
                $("body").append(
                        $([
                                overlay = $('<div id="lbOverlay" />')[0],
                                center = $('<div id="lbCenter" />')[0],
                                bottomContainer = $('<div id="lbBottomContainer" />')[0]
                        ]).css("display", "none")
                );

                image = $('<div id="lbImage" />').appendTo(center).append(
                        sizer = $('<div style="position: relative;" />').append([
                                prevLink = $('<a id="lbPrevLink" href="#" />').click(previous)[0],
                                nextLink = $('<a id="lbNextLink" href="#" />').click(next)[0]
                        ])[0]
                )[0];

                bottom = $('<div id="lbBottom" />').appendTo(bottomContainer).append([
                        $('<a id="lbCloseLink" href="#" />').add(overlay).click(close)[0],
                        caption = $('<div id="lbCaption" />')[0],
                        number = $('<div id="lbNumber" />')[0],
                        $('<div style="clear: both;" />')[0]
                ])[0];
        });


        /*
                API
        */

        // Open Slimbox with the specified parameters
        $.slimbox = function(_images, startImage, _options) {
                options = $.extend({
                        loop: false,                            // Allows to navigate between first and last images
                        overlayOpacity: 0.8,                    // 1 is opaque, 0 is completely transparent (change the color in the CSS file)
                        overlayFadeDuration: 400,               // Duration of the overlay fade-in and fade-out animations (in milliseconds)
                        resizeDuration: 400,                    // Duration of each of the box resize animations (in milliseconds)
                        resizeEasing: "swing",                  // "swing" is jQuery's default easing
                        initialWidth: 250,                      // Initial width of the box (in pixels)
                        initialHeight: 250,                     // Initial height of the box (in pixels)
                        imageFadeDuration: 400,                 // Duration of the image fade-in animation (in milliseconds)
                        captionAnimationDuration: 400,          // Duration of the caption animation (in milliseconds)
                        counterText: "Image {x} of {y}",        // Translate or change as you wish, or set it to false to disable counter text for image groups
                        closeKeys: [27, 88, 67],                // Array of keycodes to close Slimbox, default: Esc (27), 'x' (88), 'c' (67)
                        previousKeys: [37, 80],                 // Array of keycodes to navigate to the previous image, default: Left arrow (37), 'p' (80)
                        nextKeys: [39, 78]                      // Array of keycodes to navigate to the next image, default: Right arrow (39), 'n' (78)
                }, _options);

                // The function is called for a single image, with URL and Title as first two arguments
                if (typeof _images == "string") {
                        _images = [[_images, startImage]];
                        startImage = 0;
                }

                middle = win.scrollTop() + ((operaFix ? documentElement.clientHeight : win.height()) / 2);
                centerWidth = options.initialWidth;
                centerHeight = options.initialHeight;
                $(center).css({top: Math.max(0, middle - (centerHeight / 2)), width: centerWidth, height: centerHeight, marginLeft: -centerWidth/2}).show();
                compatibleOverlay = ie6 || (overlay.currentStyle && (overlay.currentStyle.position != "fixed"));
                if (compatibleOverlay) overlay.style.position = "absolute";
                $(overlay).css("opacity", options.overlayOpacity).fadeIn(options.overlayFadeDuration);
                position();
                setup(1);

                images = _images;
                options.loop = options.loop && (images.length > 1);
                return changeImage(startImage);
        };

        /*
                options:        Optional options object, see jQuery.slimbox()
                linkMapper:     Optional function taking a link DOM element and an index as arguments and returning an array containing 2 elements:
                                the image URL and the image caption (may contain HTML)
                linksFilter:    Optional function taking a link DOM element and an index as arguments and returning true if the element is part of
                                the image collection that will be shown on click, false if not. "this" refers to the element that was clicked.
                                This function must always return true when the DOM element argument is "this".
        */
        $.fn.slimbox = function(_options, linkMapper, linksFilter) {
                linkMapper = linkMapper || function(el) {
                        return [el.href, el.title];
                };

                linksFilter = linksFilter || function() {
                        return true;
                };

                var links = this;

                return links.unbind("click").click(function() {
                        // Build the list of images that will be displayed
                        var link = this, startIndex = 0, filteredLinks, i = 0, length;
                        filteredLinks = $.grep(links, function(el, i) {
                                return linksFilter.call(link, el, i);
                        });

                        // We cannot use jQuery.map() because it flattens the returned array
                        for (length = filteredLinks.length; i < length; ++i) {
                                if (filteredLinks[i] == link) startIndex = i;
                                filteredLinks[i] = linkMapper(filteredLinks[i], i);
                        }

                        return $.slimbox(filteredLinks, startIndex, _options);
                });
        };


        /*
                Internal functions
        */

        function position() {
                var l = win.scrollLeft(), w = operaFix ? documentElement.clientWidth : win.width();
                $([center, bottomContainer]).css("left", l + (w / 2));
                if (compatibleOverlay) $(overlay).css({left: l, top: win.scrollTop(), width: w, height: win.height()});
        }

        function setup(open) {
                $("object").add(ie6 ? "select" : "embed").each(function(index, el) {
                        if (open) $.data(el, "slimbox", el.style.visibility);
                        el.style.visibility = open ? "hidden" : $.data(el, "slimbox");
                });
                var fn = open ? "bind" : "unbind";
                win[fn]("scroll resize", position);
                $(document)[fn]("keydown", keyDown);
        }

        function keyDown(event) {
                var code = event.keyCode, fn = $.inArray;
                // Prevent default keyboard action (like navigating inside the page)
                return (fn(code, options.closeKeys) >= 0) ? close()
                        : (fn(code, options.nextKeys) >= 0) ? next()
                        : (fn(code, options.previousKeys) >= 0) ? previous()
                        : false;
        }

        function previous() {
                return changeImage(prevImage);
        }

        function next() {
                return changeImage(nextImage);
        }

        function changeImage(imageIndex) {
                if (imageIndex >= 0) {
                        activeImage = imageIndex;
                        activeURL = images[activeImage][0];
                        prevImage = (activeImage || (options.loop ? images.length : 0)) - 1;
                        nextImage = ((activeImage + 1) % images.length) || (options.loop ? 0 : -1);

                        stop();
                        center.className = "lbLoading";

                        preload = new Image();
                        preload.onload = animateBox;
                        preload.src = activeURL;
                }

                return false;
        }

        function animateBox() {
                center.className = "";
                $(image).css({backgroundImage: "url(" + activeURL + ")", visibility: "hidden", display: ""});
                $(sizer).width(preload.width);
                $([sizer, prevLink, nextLink]).height(preload.height);

                $(caption).html(images[activeImage][1] || "");
                $(number).html((((images.length > 1) && options.counterText) || "").replace(/{x}/, activeImage + 1).replace(/{y}/, images.length));

                if (prevImage >= 0) preloadPrev.src = images[prevImage][0];
                if (nextImage >= 0) preloadNext.src = images[nextImage][0];

                centerWidth = image.offsetWidth;
                centerHeight = image.offsetHeight;
                var top = Math.max(0, middle - (centerHeight / 2));
                if (center.offsetHeight != centerHeight) {
                        $(center).animate({height: centerHeight, top: top}, options.resizeDuration, options.resizeEasing);
                }
                if (center.offsetWidth != centerWidth) {
                        $(center).animate({width: centerWidth, marginLeft: -centerWidth/2}, options.resizeDuration, options.resizeEasing);
                }
                $(center).queue(function() {
                        $(bottomContainer).css({width: centerWidth, top: top + centerHeight, marginLeft: -centerWidth/2, visibility: "hidden", display: ""});
                        $(image).css({display: "none", visibility: "", opacity: ""}).fadeIn(options.imageFadeDuration, animateCaption);
                });
        }


        function animateCaption() {
                if (prevImage >= 0) $(prevLink).show();
                if (nextImage >= 0) $(nextLink).show();
                $(bottom).css("marginTop", -bottom.offsetHeight).animate({marginTop: 0}, options.captionAnimationDuration);
                bottomContainer.style.visibility = "";
        }

        function stop() {
                preload.onload = null;
                preload.src = preloadPrev.src = preloadNext.src = activeURL;
                $([center, image, bottom]).stop(true);
                $([prevLink, nextLink, image, bottomContainer]).hide();
        }

        function close() {
                if (activeImage >= 0) {
                        stop();
                        activeImage = prevImage = nextImage = -1;
                        $(center).hide();
                        $(overlay).stop().fadeOut(options.overlayFadeDuration, setup);
                }

                return false;
        }

})(jQuery);
/*
 * timeago: a jQuery plugin, version: 0.5.1 (08/20/2008)
 * @requires jQuery v1.2 or later
 *
 * Timeago is a jQuery plugin that makes it easy to support automatically
 * updating fuzzy timestamps (e.g. "4 minutes ago" or "about 1 day ago").
 *
 * For usage and examples, visit:
 * http://timeago.yarp.com/
 *
 * Licensed under the MIT:
 * http://www.opensource.org/licenses/mit-license.php
 *
 * Copyright (c) 2008, Ryan McGeary (ryanonjavascript -[at]- mcgeary [*dot*] org)
 */
(function($) {
  $.timeago = function(timestamp) {
    if (timestamp instanceof Date) return inWords(timestamp);
    else if (typeof timestamp == "string") return inWords($.timeago.parse(timestamp));
    else return inWords($.timeago.parse($(timestamp).attr("title")));
  };
  var $t = $.timeago;

  $.extend($.timeago, {
    settings: {
      refreshMillis: 60000,
      allowFuture: false,
      strings: {
        ago: "ago",
        fromNow: "from now",
        seconds: "less than a minute",
        minute: "about a minute",
        minutes: "%d minutes",
        hour: "about an hour",
        hours: "about %d hours",
        day: "a day",
        days: "%d days",
        month: "about a month",
        months: "%d months",
        year: "about a year",
        years: "%d years"
      }
    },
    inWords: function(distanceMillis) {
      var $l = this.settings.strings;
      var suffix = $l.ago;
      if (this.settings.allowFuture) {
        if (distanceMillis < 0) suffix = $l.fromNow;
        distanceMillis = Math.abs(distanceMillis);
      }

      var seconds = distanceMillis / 1000;
      var minutes = seconds / 60;
      var hours = minutes / 60;
      var days = hours / 24;
      var years = days / 365;

      var words = seconds < 45 && sprintf($l.seconds, Math.round(seconds)) ||
        seconds < 90 && $l.minute ||
        minutes < 45 && sprintf($l.minutes, Math.round(minutes)) ||
        minutes < 90 && $l.hour ||
        hours < 24 && sprintf($l.hours, Math.round(hours)) ||
        hours < 48 && $l.day ||
        days < 30 && sprintf($l.days, Math.floor(days)) ||
        days < 60 && $l.month ||
        days < 365 && sprintf($l.months, Math.floor(days / 30)) ||
        years < 2 && $l.year ||
        sprintf($l.years, Math.floor(years));

      return words + " " + suffix;
    },
    parse: function(iso8601) {
      var s = $.trim(iso8601);
      s = s.replace(/-/,"/").replace(/-/,"/");
      s = s.replace(/T/," ").replace(/Z/," UTC");
      s = s.replace(/([\+-]\d\d)\:?(\d\d)/," $1$2"); // -04:00 -> -0400
      return new Date(s);
    }
  });

  $.fn.timeago = function() {
    var self = this;
    self.each(refresh);

    var $s = $t.settings;
    if ($s.refreshMillis > 0) {
      setInterval(function() { self.each(refresh); }, $s.refreshMillis);
    }
    return self;
  };

  function refresh() {
    var date = $t.parse(this.title);
    if (!isNaN(date)) {
      $(this).text(inWords(date));
    }
    return this;
  }

  function inWords(date) {
    return $t.inWords(distance(date));
  }

  function distance(date) {
    return (new Date().getTime() - date.getTime());
  }

  // lame sprintf implementation
  function sprintf(string, value) {
    return string.replace(/%d/i, value);
  }

  // fix for IE6 suckage
  if ($.browser.msie && $.browser.version < 7.0) {
    document.createElement('abbr');
  }
})(jQuery);

