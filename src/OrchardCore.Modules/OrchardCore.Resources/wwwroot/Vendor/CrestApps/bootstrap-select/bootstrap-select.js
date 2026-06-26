/*!
 * Bootstrap-select v1.2.0 (https://github.com/CrestApps/bootstrap-select)
 *
 * CrestApps fork (vanilla JavaScript, Bootstrap 5+) of snapappointments/bootstrap-select
 * Copyright 2012-2018 SnapAppointments, LLC (original work)
 * Fork modifications Copyright 2024-2026 CrestApps
 * Licensed under MIT (https://github.com/CrestApps/bootstrap-select/blob/main/LICENSE)
 */
(function (factory) {
  if (typeof define === 'function' && define.amd) {
    // AMD. Register as an anonymous module.
    define(['bootstrap'], factory);
  } else if (typeof module === 'object' && module.exports) {
    // CommonJS-like environments (Node, bundlers).
    var bootstrap;
    try {
      bootstrap = require('bootstrap');
    } catch (e) {
      bootstrap = undefined;
    }
    module.exports = factory(bootstrap);
  } else {
    // Browser globals.
    factory(typeof window !== 'undefined' ? window.bootstrap : undefined);
  }
}(function (bootstrap) {
  var __SELECTPICKER_EXPOSE_GLOBAL__ = true;

/* eslint-disable no-unused-vars */
// Shared ordered source fragment consumed by the Grunt JS build.

'use strict';

// Resolve Bootstrap's Dropdown component (Bootstrap 5+). It may be provided
// by the UMD factory (`bootstrap`), or available as a global.
function getDropdown () {
  var bs = bootstrap || (typeof window !== 'undefined' ? window.bootstrap : undefined);
  return (bs && bs.Dropdown) || (typeof window !== 'undefined' ? window.Dropdown : undefined);
}

// <editor-fold desc="DOM/Event helpers">
function createFromHTML (html) {
  var wrapper = document.createElement('div');
  wrapper.innerHTML = html.trim();
  return wrapper.firstChild;
}

function toInteger (value) {
  return parseInt(value, 10) || 0;
}

function offset (el) {
  var rect = el.getBoundingClientRect();
  return {
    top: rect.top + window.pageYOffset,
    left: rect.left + window.pageXOffset
  };
}

// Resolves a container option (selector string or element) to an element.
function resolveContainer (container) {
  if (!container) return null;
  return typeof container === 'string' ? document.querySelector(container) : container;
}

function outerHeight (el, includeMargin) {
  var height = el.offsetHeight;
  if (includeMargin) {
    var style = window.getComputedStyle(el);
    height += toInteger(style.marginTop) + toInteger(style.marginBottom);
  }
  return height;
}

function setStyles (el, styles) {
  for (var prop in styles) {
    if (Object.prototype.hasOwnProperty.call(styles, prop)) {
      el.style[prop] = styles[prop];
    }
  }
}

function triggerNative (el, eventName) {
  el.dispatchEvent(new Event(eventName, { bubbles: true }));
}

// shallow array comparison
function isEqual (array1, array2) {
  return array1.length === array2.length && array1.every(function (element, index) {
    return element === array2[index];
  });
}

function toKebabCase (str) {
  return str.replace(/[A-Z]+(?![a-z])|[A-Z]/g, function ($, ofs) {
    return (ofs ? '-' : '') + $.toLowerCase();
  });
}

function toCamelCase (str) {
  return str.replace(/-([a-z])/g, function (m, letter) {
    return letter.toUpperCase();
  });
}

// Read options from data-* attributes using native values where possible.
function convertDataValue (value) {
  if (value === 'true') return true;
  if (value === 'false') return false;
  if (value === 'null') return null;
  if (value === +value + '') return +value;
  if (/^(?:\{[\w\W]*\}|\[[\w\W]*\])$/.test(value)) {
    try {
      return JSON.parse(value);
    } catch (e) {
      return value;
    }
  }
  return value;
}

function getDataset (el) {
  var dataset = {},
      attributes = el.attributes;

  for (var i = 0; i < attributes.length; i++) {
    var name = attributes[i].name;
    if (name.indexOf('data-') === 0) {
      dataset[toCamelCase(name.slice(5))] = convertDataValue(attributes[i].value);
    }
  }

  return dataset;
}
// </editor-fold>

// <editor-fold desc="Sanitizer">
var DISALLOWED_ATTRIBUTES = ['sanitize', 'whiteList', 'sanitizeFn'];

var uriAttrs = [
  'background',
  'cite',
  'href',
  'itemtype',
  'longdesc',
  'poster',
  'src',
  'xlink:href'
];

var ARIA_ATTRIBUTE_PATTERN = /^aria-[\w-]*$/i;

var DefaultWhitelist = {
  // Global attributes allowed on any supplied element below.
  '*': ['class', 'dir', 'id', 'lang', 'role', 'tabindex', 'style', ARIA_ATTRIBUTE_PATTERN],
  a: ['target', 'href', 'title', 'rel'],
  area: [],
  b: [],
  br: [],
  col: [],
  code: [],
  div: [],
  em: [],
  hr: [],
  h1: [],
  h2: [],
  h3: [],
  h4: [],
  h5: [],
  h6: [],
  i: [],
  img: ['src', 'alt', 'title', 'width', 'height'],
  li: [],
  ol: [],
  p: [],
  pre: [],
  s: [],
  small: [],
  span: [],
  sub: [],
  sup: [],
  strong: [],
  u: [],
  ul: []
};

// A pattern that recognizes a commonly useful subset of URLs that are safe.
var SAFE_URL_PATTERN = /^(?:(?:https?|mailto|ftp|tel|file):|[^&:/?#]*(?:[/?#]|$))/gi;

// A pattern that matches safe data URLs. Only matches image, video and audio types.
var DATA_URL_PATTERN = /^data:(?:image\/(?:bmp|gif|jpeg|jpg|png|tiff|webp)|video\/(?:mpeg|mp4|ogg|webm)|audio\/(?:mp3|oga|ogg|opus));base64,[a-z0-9+/]+=*$/i;

var ParseableAttributes = ['placeholder']; // attributes to use as settings, can add others in the future

function applyLegacyOptions (element, config) {
  if (!config.placeholder) {
    var title = element.getAttribute('title');
    if (title) config.placeholder = title;
  }

  return config;
}

function allowedAttribute (attr, allowedAttributeList) {
  var attrName = attr.nodeName.toLowerCase();

  if (allowedAttributeList.indexOf(attrName) !== -1) {
    if (uriAttrs.indexOf(attrName) !== -1) {
      return Boolean(attr.nodeValue.match(SAFE_URL_PATTERN) || attr.nodeValue.match(DATA_URL_PATTERN));
    }

    return true;
  }

  var regExp = allowedAttributeList.filter(function (value) {
    return value instanceof RegExp;
  });

  // Check if a regular expression validates the attribute.
  for (var i = 0, l = regExp.length; i < l; i++) {
    if (attrName.match(regExp[i])) {
      return true;
    }
  }

  return false;
}

function sanitizeHtml (unsafeElements, whiteList, sanitizeFn) {
  if (sanitizeFn && typeof sanitizeFn === 'function') {
    return sanitizeFn(unsafeElements);
  }

  var whitelistKeys = Object.keys(whiteList);

  for (var i = 0, len = unsafeElements.length; i < len; i++) {
    var elements = unsafeElements[i].querySelectorAll('*');

    for (var j = 0, len2 = elements.length; j < len2; j++) {
      var el = elements[j];
      var elName = el.nodeName.toLowerCase();

      if (whitelistKeys.indexOf(elName) === -1) {
        el.parentNode.removeChild(el);

        continue;
      }

      var attributeList = [].slice.call(el.attributes);
      var whitelistedAttributes = [].concat(whiteList['*'] || [], whiteList[elName] || []);

      for (var k = 0, len3 = attributeList.length; k < len3; k++) {
        var attr = attributeList[k];

        if (!allowedAttribute(attr, whitelistedAttributes)) {
          el.removeAttribute(attr.nodeName);
        }
      }
    }
  }
}
// </editor-fold>

function getAttributesObject (element) {
  var attributesObject = {},
      attrVal;

  ParseableAttributes.forEach(function (item) {
    attrVal = element.getAttribute(item);
    if (attrVal) attributesObject[item] = attrVal;
  });

  return attributesObject;
}


/* eslint-disable no-unused-vars */
// Shared ordered source fragment consumed by the Grunt JS build.

// <editor-fold desc="Search / string utilities">
function stringSearch (li, searchString, method, normalize) {
  var stringTypes = [
        'display',
        'subtext',
        'tokens'
      ],
      searchSuccess = false;

  for (var i = 0; i < stringTypes.length; i++) {
    var stringType = stringTypes[i],
        string = li[stringType];

    if (string) {
      string = string.toString();

      // Strip HTML tags. This isn't perfect, but it's much faster than any other method
      if (stringType === 'display') {
        string = string.replace(/<[^>]+>/g, '');
      }

      if (normalize) string = normalizeToBase(string);
      string = string.toUpperCase();

      if (typeof method === 'function') {
        searchSuccess = method(string, searchString);
      } else if (method === 'contains') {
        searchSuccess = string.indexOf(searchString) >= 0;
      } else {
        searchSuccess = string.startsWith(searchString);
      }

      if (searchSuccess) break;
    }
  }

  return searchSuccess;
}

function normalizeSearchInput (value, normalize) {
  if (value === undefined || value === null) value = '';
  value = value.toString().trim();

  if (normalize && value) value = normalizeToBase(value);

  return value.toUpperCase();
}

function getOptionLabelText (option) {
  if (!option) return '';

  return option.title || option.text || option.value || '';
}

// Borrowed from Lodash (_.deburr)
/** Used to map Latin Unicode letters to basic Latin letters. */
var deburredLetters = {
  // Latin-1 Supplement block.
  '\xc0': 'A',  '\xc1': 'A', '\xc2': 'A', '\xc3': 'A', '\xc4': 'A', '\xc5': 'A',
  '\xe0': 'a',  '\xe1': 'a', '\xe2': 'a', '\xe3': 'a', '\xe4': 'a', '\xe5': 'a',
  '\xc7': 'C',  '\xe7': 'c',
  '\xd0': 'D',  '\xf0': 'd',
  '\xc8': 'E',  '\xc9': 'E', '\xca': 'E', '\xcb': 'E',
  '\xe8': 'e',  '\xe9': 'e', '\xea': 'e', '\xeb': 'e',
  '\xcc': 'I',  '\xcd': 'I', '\xce': 'I', '\xcf': 'I',
  '\xec': 'i',  '\xed': 'i', '\xee': 'i', '\xef': 'i',
  '\xd1': 'N',  '\xf1': 'n',
  '\xd2': 'O',  '\xd3': 'O', '\xd4': 'O', '\xd5': 'O', '\xd6': 'O', '\xd8': 'O',
  '\xf2': 'o',  '\xf3': 'o', '\xf4': 'o', '\xf5': 'o', '\xf6': 'o', '\xf8': 'o',
  '\xd9': 'U',  '\xda': 'U', '\xdb': 'U', '\xdc': 'U',
  '\xf9': 'u',  '\xfa': 'u', '\xfb': 'u', '\xfc': 'u',
  '\xdd': 'Y',  '\xfd': 'y', '\xff': 'y',
  '\xc6': 'Ae', '\xe6': 'ae',
  '\xde': 'Th', '\xfe': 'th',
  '\xdf': 'ss',
  // Latin Extended-A block.
  '\u0100': 'A',  '\u0102': 'A', '\u0104': 'A',
  '\u0101': 'a',  '\u0103': 'a', '\u0105': 'a',
  '\u0106': 'C',  '\u0108': 'C', '\u010a': 'C', '\u010c': 'C',
  '\u0107': 'c',  '\u0109': 'c', '\u010b': 'c', '\u010d': 'c',
  '\u010e': 'D',  '\u0110': 'D', '\u010f': 'd', '\u0111': 'd',
  '\u0112': 'E',  '\u0114': 'E', '\u0116': 'E', '\u0118': 'E', '\u011a': 'E',
  '\u0113': 'e',  '\u0115': 'e', '\u0117': 'e', '\u0119': 'e', '\u011b': 'e',
  '\u011c': 'G',  '\u011e': 'G', '\u0120': 'G', '\u0122': 'G',
  '\u011d': 'g',  '\u011f': 'g', '\u0121': 'g', '\u0123': 'g',
  '\u0124': 'H',  '\u0126': 'H', '\u0125': 'h', '\u0127': 'h',
  '\u0128': 'I',  '\u012a': 'I', '\u012c': 'I', '\u012e': 'I', '\u0130': 'I',
  '\u0129': 'i',  '\u012b': 'i', '\u012d': 'i', '\u012f': 'i', '\u0131': 'i',
  '\u0134': 'J',  '\u0135': 'j',
  '\u0136': 'K',  '\u0137': 'k', '\u0138': 'k',
  '\u0139': 'L',  '\u013b': 'L', '\u013d': 'L', '\u013f': 'L', '\u0141': 'L',
  '\u013a': 'l',  '\u013c': 'l', '\u013e': 'l', '\u0140': 'l', '\u0142': 'l',
  '\u0143': 'N',  '\u0145': 'N', '\u0147': 'N', '\u014a': 'N',
  '\u0144': 'n',  '\u0146': 'n', '\u0148': 'n', '\u014b': 'n',
  '\u014c': 'O',  '\u014e': 'O', '\u0150': 'O',
  '\u014d': 'o',  '\u014f': 'o', '\u0151': 'o',
  '\u0154': 'R',  '\u0156': 'R', '\u0158': 'R',
  '\u0155': 'r',  '\u0157': 'r', '\u0159': 'r',
  '\u015a': 'S',  '\u015c': 'S', '\u015e': 'S', '\u0160': 'S',
  '\u015b': 's',  '\u015d': 's', '\u015f': 's', '\u0161': 's',
  '\u0162': 'T',  '\u0164': 'T', '\u0166': 'T',
  '\u0163': 't',  '\u0165': 't', '\u0167': 't',
  '\u0168': 'U',  '\u016a': 'U', '\u016c': 'U', '\u016e': 'U', '\u0170': 'U', '\u0172': 'U',
  '\u0169': 'u',  '\u016b': 'u', '\u016d': 'u', '\u016f': 'u', '\u0171': 'u', '\u0173': 'u',
  '\u0174': 'W',  '\u0175': 'w',
  '\u0176': 'Y',  '\u0177': 'y', '\u0178': 'Y',
  '\u0179': 'Z',  '\u017b': 'Z', '\u017d': 'Z',
  '\u017a': 'z',  '\u017c': 'z', '\u017e': 'z',
  '\u0132': 'IJ', '\u0133': 'ij',
  '\u0152': 'Oe', '\u0153': 'oe',
  '\u0149': "'n", '\u017f': 's'
};

/** Used to match Latin Unicode letters (excluding mathematical operators). */
var reLatin = /[\xc0-\xd6\xd8-\xf6\xf8-\xff\u0100-\u017f]/g;

/** Used to compose unicode character classes. */
var rsComboMarksRange = '\\u0300-\\u036f',
    reComboHalfMarksRange = '\\ufe20-\\ufe2f',
    rsComboSymbolsRange = '\\u20d0-\\u20ff',
    rsComboMarksExtendedRange = '\\u1ab0-\\u1aff',
    rsComboMarksSupplementRange = '\\u1dc0-\\u1dff',
    rsComboRange = rsComboMarksRange + reComboHalfMarksRange + rsComboSymbolsRange + rsComboMarksExtendedRange + rsComboMarksSupplementRange;

/** Used to compose unicode capture groups. */
var rsCombo = '[' + rsComboRange + ']';

var reComboMark = RegExp(rsCombo, 'g');

function deburrLetter (key) {
  return deburredLetters[key];
}

function normalizeToBase (string) {
  string = string.toString();
  return string && string.replace(reLatin, deburrLetter).replace(reComboMark, '');
}

// List of HTML entities for escaping.
var escapeMap = {
  '&': '&amp;',
  '<': '&lt;',
  '>': '&gt;',
  '"': '&quot;',
  "'": '&#x27;',
  '`': '&#x60;'
};

var createEscaper = function (map) {
  var escaper = function (match) {
    return map[match];
  };
  var source = '(?:' + Object.keys(map).join('|') + ')';
  var testRegexp = RegExp(source);
  var replaceRegexp = RegExp(source, 'g');
  return function (string) {
    string = string == null ? '' : '' + string;
    return testRegexp.test(string) ? string.replace(replaceRegexp, escaper) : string;
  };
};

var htmlEscape = createEscaper(escapeMap);
// </editor-fold>


/* eslint-disable no-undef, no-unused-vars */
// Shared ordered source fragment consumed by the Grunt JS build.

// <editor-fold desc="Constants">
var keyCodeMap = {
  32: ' ', 48: '0', 49: '1', 50: '2', 51: '3', 52: '4', 53: '5', 54: '6',
  55: '7', 56: '8', 57: '9', 59: ';',
  65: 'A', 66: 'B', 67: 'C', 68: 'D', 69: 'E', 70: 'F', 71: 'G', 72: 'H',
  73: 'I', 74: 'J', 75: 'K', 76: 'L', 77: 'M', 78: 'N', 79: 'O', 80: 'P',
  81: 'Q', 82: 'R', 83: 'S', 84: 'T', 85: 'U', 86: 'V', 87: 'W', 88: 'X',
  89: 'Y', 90: 'Z',
  96: '0', 97: '1', 98: '2', 99: '3', 100: '4', 101: '5', 102: '6',
  103: '7', 104: '8', 105: '9'
};

var keyCodes = {
  ESCAPE: 27,
  ENTER: 13,
  SPACE: 32,
  TAB: 9,
  ARROW_UP: 38,
  ARROW_DOWN: 40
};

var selectId = 0;

var EVENT_KEY = '.bs.select';

// Bootstrap 5 class names.
var classNames = {
  DISABLED: 'disabled',
  DIVIDER: 'dropdown-divider',
  SHOW: 'show',
  DROPUP: 'dropup',
  MENU: 'dropdown-menu',
  MENUEND: 'dropdown-menu-end',
  BUTTONCLASS: 'btn-light',
  POPOVERHEADER: 'popover-header',
  ICONBASE: '',
  TICKICON: 'bs-ok-default'
};

var Selector = {
  MENU: '.' + classNames.MENU,
  DATA_TOGGLE: 'data-bs-toggle="dropdown"'
};

var elementTemplates = {
  div: document.createElement('div'),
  span: document.createElement('span'),
  i: document.createElement('i'),
  subtext: document.createElement('small'),
  a: document.createElement('a'),
  li: document.createElement('li'),
  whitespace: document.createTextNode('\u00A0'),
  fragment: document.createDocumentFragment(),
  option: document.createElement('option')
};

elementTemplates.selectedOption = elementTemplates.option.cloneNode(false);
elementTemplates.selectedOption.setAttribute('selected', true);

elementTemplates.noResults = elementTemplates.li.cloneNode(false);
elementTemplates.noResults.className = 'no-results';

elementTemplates.a.setAttribute('role', 'option');
elementTemplates.a.className = 'dropdown-item';

elementTemplates.subtext.className = 'text-muted';

elementTemplates.text = elementTemplates.span.cloneNode(false);
elementTemplates.text.className = 'text';

elementTemplates.checkMark = elementTemplates.span.cloneNode(false);

var REGEXP_ARROW = new RegExp(keyCodes.ARROW_UP + '|' + keyCodes.ARROW_DOWN);
var REGEXP_TAB_OR_ESCAPE = new RegExp('^' + keyCodes.TAB + '$|' + keyCodes.ESCAPE);

var generateOption = {
  li: function (content, classes, optgroup) {
    var li = elementTemplates.li.cloneNode(false);

    if (content) {
      if (content.nodeType === 1 || content.nodeType === 11) {
        li.appendChild(content);
      } else {
        li.innerHTML = content;
      }
    }

    if (typeof classes !== 'undefined' && classes !== '') li.className = classes;
    if (typeof optgroup !== 'undefined' && optgroup !== null) li.classList.add('optgroup-' + optgroup);

    return li;
  },

  a: function (text, classes, inline) {
    var a = elementTemplates.a.cloneNode(true);

    if (text) {
      if (text.nodeType === 11) {
        a.appendChild(text);
      } else {
        a.insertAdjacentHTML('beforeend', text);
      }
    }

    if (typeof classes !== 'undefined' && classes !== '') a.classList.add.apply(a.classList, classes.split(/\s+/));
    if (inline) a.setAttribute('style', inline);

    return a;
  },

  text: function (options, useFragment) {
    var textElement = elementTemplates.text.cloneNode(false),
        subtextElement,
        iconElement;

    if (options.content) {
      textElement.innerHTML = options.content;
    } else {
      textElement.textContent = options.text;

      if (options.icon) {
        var whitespace = elementTemplates.whitespace.cloneNode(false);

        // need to use <i> for icons in the button to prevent a breaking change
        iconElement = (useFragment === true ? elementTemplates.i : elementTemplates.span).cloneNode(false);
        iconElement.className = this.options.iconBase + ' ' + options.icon;

        elementTemplates.fragment.appendChild(iconElement);
        elementTemplates.fragment.appendChild(whitespace);
      }

      if (options.subtext) {
        subtextElement = elementTemplates.subtext.cloneNode(false);
        subtextElement.textContent = options.subtext;
        textElement.appendChild(subtextElement);
      }
    }

    if (useFragment === true) {
      while (textElement.childNodes.length > 0) {
        elementTemplates.fragment.appendChild(textElement.childNodes[0]);
      }
    } else {
      elementTemplates.fragment.appendChild(textElement);
    }

    return elementTemplates.fragment;
  },

  label: function (options) {
    var textElement = elementTemplates.text.cloneNode(false),
        subtextElement,
        iconElement;

    textElement.innerHTML = options.display;

    if (options.icon) {
      var whitespace = elementTemplates.whitespace.cloneNode(false);

      iconElement = elementTemplates.span.cloneNode(false);
      iconElement.className = this.options.iconBase + ' ' + options.icon;

      elementTemplates.fragment.appendChild(iconElement);
      elementTemplates.fragment.appendChild(whitespace);
    }

    if (options.subtext) {
      subtextElement = elementTemplates.subtext.cloneNode(false);
      subtextElement.textContent = options.subtext;
      textElement.appendChild(subtextElement);
    }

    elementTemplates.fragment.appendChild(textElement);

    return elementTemplates.fragment;
  }
};

var getOptionData = {
  fromOption: function (option, type) {
    var value;

    switch (type) {
      case 'divider':
        value = option.getAttribute('data-divider') === 'true';
        break;

      case 'text':
        value = option.textContent;
        break;

      case 'label':
        value = option.label;
        break;

      case 'style':
        value = option.style.cssText;
        break;

      case 'title':
        value = option.title;
        break;

      default:
        value = option.getAttribute('data-' + toKebabCase(type));
        break;
    }

    return value;
  },
  fromDataSource: function (option, type) {
    var value;

    switch (type) {
      case 'text':
      case 'label':
        value = option.text || option.value || '';
        break;

      default:
        value = option[type];
        break;
    }

    return value;
  }
};

function showNoResults (searchMatch, searchValue) {
  if (!searchMatch.length) {
    elementTemplates.noResults.innerHTML = this.options.noneResultsText.replace('{0}', '"' + htmlEscape(searchValue) + '"');
    this.menuInner.firstChild.appendChild(elementTemplates.noResults);
  }
}

function filterHidden (item) {
  return !(item.hidden || this.options.hideDisabled && item.disabled);
}

function getSelectedOptions () {
  var options = this.selectpicker.main.data;

  if (this.options.source.data || this.options.source.search) {
    options = Object.values(this.selectpicker.optionValuesDataMap);
  }

  var selectedOptions = options.filter(function (item) {
    if (item.selected) {
      if (this.options.hideDisabled && item.disabled) return false;
      return true;
    }

    return false;
  }, this);

  // ensure only 1 option is selected if multiple are set in the data source
  if (this.options.source.data && !this.multiple && selectedOptions.length > 1) {
    for (var i = 0; i < selectedOptions.length - 1; i++) {
      selectedOptions[i].selected = false;
    }

    selectedOptions = [ selectedOptions[selectedOptions.length - 1] ];
  }

  return selectedOptions;
}

function getSelectValues (selectedOptions) {
  var value = [],
      options = selectedOptions || getSelectedOptions.call(this),
      opt;

  for (var i = 0, len = options.length; i < len; i++) {
    opt = options[i];

    if (!opt.disabled) {
      value.push(opt.value === undefined ? opt.text : opt.value);
    }
  }

  if (!this.multiple) {
    return !value.length ? null : value[0];
  }

  return value;
}
// </editor-fold>

var changedArguments = null;

// shared flag for spacebar selection handling (mirrors original document data flag)
var spaceSelectFlag = false;

var REMOVED_OPTIONS = ['container', 'display', 'mobile', 'styleBase', 'windowPadding'];

function stripRemovedOptions (source) {
  if (!source || typeof source !== 'object') return source;

  var result = Object.assign({}, source);

  for (var i = 0; i < REMOVED_OPTIONS.length; i++) {
    delete result[REMOVED_OPTIONS[i]];
  }

  return result;
}


/* eslint-disable no-undef */
// Shared ordered source fragment consumed by the Grunt JS build.

class Selectpicker {
  constructor (element, options) {
    if (typeof element === 'string') {
      element = document.querySelector(element);
    }

    if (!element || element.tagName !== 'SELECT') {
      throw new TypeError('Selectpicker requires a select element or selector.');
    }

    this.element = element;
    this.newElement = null;
    this.button = null;
    this.menu = null;
    this.options = Selectpicker._buildConfig(element, options || {});

    // tracked event listeners for clean teardown
    this._listeners = [];
    this._named = {};

    this.selectpicker = {
      main: {
        data: [],
        optionQueue: elementTemplates.fragment.cloneNode(false),
        hasMore: false
      },
      search: {
        data: [],
        hasMore: false
      },
      current: {}, // current is either equal to main or search depending on if a search is in progress
      view: {},
      // map of option values and their respective data (only used in conjunction with options.source)
      optionValuesDataMap: {},
      createdOptions: [],
      openOption: {
        isCreating: false
      },
      isSearching: false,
      keydown: {
        keyHistory: '',
        resetKeyHistory: {
          start: () => {
            return setTimeout(() => {
              this.selectpicker.keydown.keyHistory = '';
            }, 800);
          }
        }
      }
    };

    this.sizeInfo = {};

    this.init();

    instanceMap.set(element, this);
  }

  // <editor-fold desc="Event helpers">
  _on (el, type, handler, options) {
    el.addEventListener(type, handler, options);
    this._listeners.push({ el: el, type: type, handler: handler, options: options });
    return handler;
  }

  _delegate (el, type, selector, handler, options) {
    var listener = function (e) {
      var target = e.target.closest(selector);
      if (target && el.contains(target)) {
        handler.call(target, e);
      }
    };
    return this._on(el, type, listener, options);
  }

  _emit (name, detail) {
    var event = new CustomEvent(name + EVENT_KEY, {
      bubbles: true,
      cancelable: true,
      detail: detail || null
    });
    this.element.dispatchEvent(event);
    return event;
  }

  // adds an event listener that replaces any previously-registered listener under `key`
  _replace (key, el, type, handler, options) {
    this._removeNamed(key);
    el.addEventListener(type, handler, options);
    this._named[key] = { el: el, type: type, handler: handler, options: options };
  }

  _removeNamed (key) {
    var prev = this._named[key];
    if (prev) {
      prev.el.removeEventListener(prev.type, prev.handler, prev.options);
      delete this._named[key];
    }
  }
  // </editor-fold>

  init () {
    var that = this,
        id = this.element.getAttribute('id'),
        element = this.element,
        form = element.form;

    selectId++;
    this.selectId = 'bs-select-' + selectId;

    element.classList.add('bs-select-hidden');

    this.multiple = this.element.multiple;
    this.autofocus = this.element.autofocus;

    if (element.classList.contains('show-tick')) {
      this.options.showTick = true;
    }

    this.newElement = this.createDropdown();

    // insert newElement after element, then move element to be the first child of newElement
    element.parentNode.insertBefore(this.newElement, element.nextSibling);
    this.newElement.insertBefore(element, this.newElement.firstChild);

    // ensure select is associated with form element if it got unlinked after moving it inside newElement
    if (form && element.form === null) {
      if (!form.id) form.id = 'form-' + this.selectId;
      element.setAttribute('form', form.id);
    }

    this.button = this.newElement.querySelector(':scope > button');
    if (this.options.allowClear) this.clearButton = this.button.querySelector('.bs-select-clear-selected');
    this.menu = this.newElement.querySelector(':scope > ' + Selector.MENU);
    this.menuInner = this.menu.querySelector('.inner');
    this.searchbox = this.menu.querySelector('input');
    this.selectedItems = this.newElement.querySelector(':scope > .bs-selected-items-external') || this.menu.querySelector('.bs-selected-items');
    this.createOptionButton = this.menu.querySelector('.bs-create-option');

    element.classList.remove('bs-select-hidden');

    this.fetchData(function () {
      that.render(true);
      that.buildList();

      requestAnimationFrame(function () {
        that._emit('loaded');
      });
    });

    if (this.options.dropdownAlignRight === true) this.menu.classList.add(classNames.MENUEND);

    if (typeof id !== 'undefined' && id !== null) {
      this.button.setAttribute('data-id', id);
    }

    this.checkDisabled();
    this.clickListener();

    var Dropdown = getDropdown();
    this.dropdown = new Dropdown(this.button);

    // store a reference to the instance for delegated handlers
    this.newElement.bootstrapSelectInstance = this;
    this.menu.bootstrapSelectInstance = this;

    if (this.options.liveSearch) {
      this.liveSearchListener();
      this.focusedParent = this.searchbox;
    } else {
      this.focusedParent = this.menuInner;
    }

    this.setStyle();
    this.setWidth();
    this._on(this.element, 'hide' + EVENT_KEY, function () {
      if (that.isVirtual()) {
        // empty menu on close
        var menuInner = that.menuInner,
            emptyMenu = menuInner.firstChild.cloneNode(false);

        // replace the existing UL with an empty one - this is faster than emptying it
        menuInner.replaceChild(emptyMenu, menuInner.firstChild);
        menuInner.scrollTop = 0;
      }
    });

    // re-emit Bootstrap dropdown events as bootstrap-select events
    this._on(this.newElement, 'hide.bs.dropdown', function (e) {
      that._emit('hide', { bsEvent: e });
    });
    this._on(this.newElement, 'hidden.bs.dropdown', function (e) {
      that._emit('hidden', { bsEvent: e });
    });
    this._on(this.newElement, 'show.bs.dropdown', function (e) {
      that.onShow(e);
      that._emit('show', { bsEvent: e });
    });
    this._on(this.newElement, 'shown.bs.dropdown', function (e) {
      that._emit('shown', { bsEvent: e });
    });

    if (element.hasAttribute('required')) {
      this._on(this.element, 'invalid', function () {
        that.button.classList.add('bs-invalid');

        var onShownInvalid = function () {
          // set the value to hide the validation message in Chrome when menu is opened
          triggerNative(that.element, 'change');
          that.element.removeEventListener('shown' + EVENT_KEY, onShownInvalid);
        };
        that._on(that.element, 'shown' + EVENT_KEY, onShownInvalid);

        var onRendered = function () {
          // if select is no longer invalid, remove the bs-invalid class
          if (that.element.validity.valid) that.button.classList.remove('bs-invalid');
          that.element.removeEventListener('rendered' + EVENT_KEY, onRendered);
        };
        that._on(that.element, 'rendered' + EVENT_KEY, onRendered);

        var onBlur = function () {
          that.element.focus();
          that.element.blur();
          that.button.removeEventListener('blur' + EVENT_KEY, onBlur);
        };
        that._on(that.button, 'blur' + EVENT_KEY, onBlur);
      });
    }

    if (form) {
      this._on(form, 'reset', function () {
        requestAnimationFrame(function () {
          that.render();
        });
      });
    }
  }

  createDropdown () {
    // Multiple selects always show an indicator. Single selects also need the
    // indicator column when selectionIndicator is enabled.
    var usesSelectionIndicator = this.options.selectionIndicator === 'checkbox',
        showTick = (this.multiple || this.options.showTick || usesSelectionIndicator) ? ' show-tick' : '',
        showSelectedTags = this.options.showSelectedTags ? ' show-selected-tags' : '',
        selectedItemsStyle = this.options.selectedItemsStyle === 'list' ? ' selected-items-style-list' : '',
        selectionIndicator = usesSelectionIndicator
          ? (this.multiple ? ' selection-indicator-checkbox' : ' selection-indicator-radio')
          : '',
        multiselectable = this.multiple ? ' aria-multiselectable="true"' : '',
        autofocus = this.autofocus ? ' autofocus' : '',
        liveSearchPlaceholder = this.options.liveSearchPlaceholder;

    if (liveSearchPlaceholder === null && (this.options.showSelectedTags || this.options.openOptions)) {
      liveSearchPlaceholder = this.options.placeholder || 'Search';
    }

    // Elements
    var drop,
        header = '',
        searchbox = '',
        actionsbox = '',
        donebutton = '',
        clearButton = '';

    if (this.options.header) {
      header =
          '<div class="' + classNames.POPOVERHEADER + '">' +
            '<span class="popover-header-text">' + this.options.header + '</span>' +
            '<button type="button" class="btn-close" aria-label="Close"></button>' +
          '</div>';
    }

    if (this.options.liveSearch) {
      searchbox =
          '<div class="bs-searchbox">' +
            '<input type="search" class="form-control" autocomplete="off"' +
              (
                liveSearchPlaceholder === null ? ''
                :
                ' placeholder="' + htmlEscape(liveSearchPlaceholder) + '"'
              ) +
              ' role="combobox" aria-label="Search" aria-controls="' + this.selectId + '" aria-autocomplete="list">' +
            (this.options.openOptions
              ? '<button type="button" class="bs-create-option dropdown-item" hidden></button>'
              : '') +
          '</div>';
    }

    if (this.multiple && this.options.actionsBox) {
      actionsbox =
          '<div class="bs-actionsbox">' +
            '<div class="btn-group btn-group-sm">' +
              '<button type="button" class="actions-btn bs-select-all btn ' + classNames.BUTTONCLASS + '">' +
                this.options.selectAllText +
              '</button>' +
              '<button type="button" class="actions-btn bs-deselect-all btn ' + classNames.BUTTONCLASS + '">' +
                this.options.deselectAllText +
              '</button>' +
            '</div>' +
          '</div>';
    }

    if (this.multiple && this.options.doneButton) {
      donebutton =
          '<div class="bs-donebutton">' +
            '<div class="btn-group">' +
              '<button type="button" class="btn btn-sm ' + classNames.BUTTONCLASS + '">' +
                this.options.doneButtonText +
              '</button>' +
            '</div>' +
          '</div>';
    }

    if (this.options.allowClear) {
      clearButton = '<span class="bs-select-clear-selected" title="' + this.options.deselectAllText + '"><span>&times;</span>';
    }

    drop =
        '<div class="dropdown bootstrap-select' + showTick + showSelectedTags + selectedItemsStyle + selectionIndicator + '">' +
          '<button type="button" tabindex="-1" class="' +
            'btn dropdown-toggle" ' +
            Selector.DATA_TOGGLE +
            autofocus +
            ' role="combobox" aria-owns="' +
            this.selectId +
            '" aria-haspopup="listbox" aria-expanded="false">' +
            '<div class="filter-option">' +
              '<div class="filter-option-inner">' +
                '<div class="filter-option-inner-inner">&nbsp;</div>' +
              '</div> ' +
            '</div>' +
            clearButton +
            '</span>' +
          '</button>' +
          '<div class="' + classNames.MENU + '">' +
            header +
            searchbox +
            actionsbox +
            '<div class="inner ' + classNames.SHOW + '" role="listbox" id="' + this.selectId + '" tabindex="-1" ' + multiselectable + '>' +
                '<ul class="' + classNames.MENU + ' inner ' + classNames.SHOW + '" role="presentation">' +
                '</ul>' +
            '</div>' +
            donebutton +
          '</div>' +
          (this.multiple && this.options.showSelectedTags
            ? '<div class="bs-selected-items bs-selected-items-external" hidden></div>'
            : '') +
        '</div>';

    return createFromHTML(drop);
  }


/* eslint-disable no-undef */
// Shared ordered source fragment consumed by the Grunt JS build.
  // runs when the dropdown is about to be shown
  onShow () {
    if (this.options.liveSearch && this.searchbox.value) {
      this.searchbox.value = '';
      this.selectpicker.search.previousValue = undefined;
    }

    if (!this.newElement.classList.contains(classNames.SHOW)) {
      this.setSize();
    }
  }

  setPositionData () {
    this.selectpicker.view.canHighlight = [];
    this.selectpicker.view.size = 0;
    this.selectpicker.view.firstHighlightIndex = false;

    for (var i = 0; i < this.selectpicker.current.data.length; i++) {
      var li = this.selectpicker.current.data[i],
          canHighlight = true;

      if (li.type === 'divider') {
        canHighlight = false;
        li.height = this.sizeInfo.dividerHeight;
      } else if (li.type === 'optgroup-label') {
        canHighlight = false;
        li.height = this.sizeInfo.dropdownHeaderHeight;
      } else {
        li.height = this.sizeInfo.liHeight;
      }

      if (li.disabled) canHighlight = false;

      this.selectpicker.view.canHighlight.push(canHighlight);

      if (canHighlight) {
        this.selectpicker.view.size++;
        li.posinset = this.selectpicker.view.size;
        if (this.selectpicker.view.firstHighlightIndex === false) this.selectpicker.view.firstHighlightIndex = i;
      }

      li.position = (i === 0 ? 0 : this.selectpicker.current.data[i - 1].position) + li.height;
    }
  }

  isVirtual () {
    return (this.options.virtualScroll !== false) && (this.selectpicker.main.data.length >= this.options.virtualScroll) || this.options.virtualScroll === true;
  }

  createView (isSearching, setSize, refresh) {
    var that = this,
        scrollTop = 0;

    this.selectpicker.isSearching = isSearching;
    this.selectpicker.current = isSearching ? this.selectpicker.search : this.selectpicker.main;

    this.setPositionData();

    if (setSize) {
      if (refresh) {
        scrollTop = this.menuInner.scrollTop;
      } else if (!that.multiple) {
        var element = that.element,
            selectedIndex = (element.options[element.selectedIndex] || {}).liIndex;

        if (typeof selectedIndex === 'number' && that.options.size !== false) {
          var selectedData = that.selectpicker.main.data[selectedIndex],
              position = selectedData && selectedData.position;

          if (position) {
            scrollTop = position - ((that.sizeInfo.menuInnerHeight + that.sizeInfo.liHeight) / 2);
          }
        }
      }
    }

    scroll(scrollTop, true);

    this._replace('createViewScroll', this.menuInner, 'scroll', function () {
      if (!that.noScroll) scroll(that.menuInner.scrollTop);
      that.noScroll = false;
    });

    function scroll (scrollTop, init) {
      var size = that.selectpicker.current.data.length,
          chunks = [],
          chunkSize,
          chunkCount,
          firstChunk,
          lastChunk,
          currentChunk,
          prevPositions,
          positionIsDifferent,
          previousElements,
          menuIsDifferent = true,
          isVirtual = that.isVirtual();

      that.selectpicker.view.scrollTop = scrollTop;

      chunkSize = that.options.chunkSize; // number of options in a chunk
      chunkCount = Math.ceil(size / chunkSize) || 1; // number of chunks

      for (var i = 0; i < chunkCount; i++) {
        var endOfChunk = (i + 1) * chunkSize;

        if (i === chunkCount - 1) {
          endOfChunk = size;
        }

        chunks[i] = [
          (i) * chunkSize + (!i ? 0 : 1),
          endOfChunk
        ];

        if (!size) break;

        if (currentChunk === undefined && scrollTop - 1 <= that.selectpicker.current.data[endOfChunk - 1].position - that.sizeInfo.menuInnerHeight) {
          currentChunk = i;
        }
      }

      if (currentChunk === undefined) currentChunk = 0;

      prevPositions = [that.selectpicker.view.position0, that.selectpicker.view.position1];

      // always display previous, current, and next chunks
      firstChunk = Math.max(0, currentChunk - 1);
      lastChunk = Math.min(chunkCount - 1, currentChunk + 1);

      that.selectpicker.view.position0 = isVirtual === false ? 0 : (Math.max(0, chunks[firstChunk][0]) || 0);
      that.selectpicker.view.position1 = isVirtual === false ? size : (Math.min(size, chunks[lastChunk][1]) || 0);

      positionIsDifferent = prevPositions[0] !== that.selectpicker.view.position0 || prevPositions[1] !== that.selectpicker.view.position1;

      if (that.activeElement !== undefined) {
        if (init) {
          if (that.activeElement !== that.selectedElement) {
            that.defocusItem(that.activeElement);
          }
          that.activeElement = undefined;
        }

        if (that.activeElement !== that.selectedElement) {
          that.defocusItem(that.selectedElement);
        }
      }

      if (that.prevActiveElement !== undefined && that.prevActiveElement !== that.activeElement && that.prevActiveElement !== that.selectedElement) {
        that.defocusItem(that.prevActiveElement);
      }

      if (init || positionIsDifferent || that.selectpicker.current.hasMore) {
        previousElements = that.selectpicker.view.visibleElements ? that.selectpicker.view.visibleElements.slice() : [];

        if (isVirtual === false) {
          that.selectpicker.view.visibleElements = that.selectpicker.current.elements;
        } else {
          that.selectpicker.view.visibleElements = that.selectpicker.current.elements.slice(that.selectpicker.view.position0, that.selectpicker.view.position1);
        }

        that.setOptionStatus();

        // if searching, check to make sure the list has actually been updated before updating DOM
        // this prevents unnecessary repaints
        if (isSearching || (isVirtual === false && init)) menuIsDifferent = !isEqual(previousElements, that.selectpicker.view.visibleElements);

        // if virtual scroll is disabled and not searching,
        // menu should never need to be updated more than once
        if ((init || isVirtual === true) && menuIsDifferent) {
          var menuInner = that.menuInner,
              menuFragment = document.createDocumentFragment(),
              emptyMenu = menuInner.firstChild.cloneNode(false),
              marginTop,
              marginBottom,
              elements = that.selectpicker.view.visibleElements,
              toSanitize = [];

          // replace the existing UL with an empty one - this is faster than emptying it
          menuInner.replaceChild(emptyMenu, menuInner.firstChild);

          for (var i = 0, visibleElementsLen = elements.length; i < visibleElementsLen; i++) {
            var element = elements[i],
                elText,
                elementData;

            if (that.options.sanitize) {
              elText = element.lastChild;

              if (elText) {
                elementData = that.selectpicker.current.data[i + that.selectpicker.view.position0];

                if (elementData && elementData.content && !elementData.sanitized) {
                  toSanitize.push(elText);
                  elementData.sanitized = true;
                }
              }
            }

            menuFragment.appendChild(element);
          }

          if (that.options.sanitize && toSanitize.length) {
            sanitizeHtml(toSanitize, that.options.whiteList, that.options.sanitizeFn);
          }

          if (isVirtual === true) {
            marginTop = (that.selectpicker.view.position0 === 0 ? 0 : that.selectpicker.current.data[that.selectpicker.view.position0 - 1].position);
            marginBottom = (that.selectpicker.view.position1 > size - 1 ? 0 : that.selectpicker.current.data[size - 1].position - that.selectpicker.current.data[that.selectpicker.view.position1 - 1].position);

            menuInner.firstChild.style.marginTop = marginTop + 'px';
            menuInner.firstChild.style.marginBottom = marginBottom + 'px';
          } else {
            menuInner.firstChild.style.marginTop = 0;
            menuInner.firstChild.style.marginBottom = 0;
          }

          menuInner.firstChild.appendChild(menuFragment);

          // if an option is encountered that is wider than the current menu width, update the menu width accordingly
          if (isVirtual === true && that.sizeInfo.hasScrollBar) {
            var menuInnerInnerWidth = menuInner.firstChild.offsetWidth;

            if (init && menuInnerInnerWidth < that.sizeInfo.menuInnerInnerWidth && that.sizeInfo.totalMenuWidth > that.sizeInfo.selectWidth) {
              menuInner.firstChild.style.minWidth = that.sizeInfo.menuInnerInnerWidth + 'px';
            } else if (menuInnerInnerWidth > that.sizeInfo.menuInnerInnerWidth) {
              // set to 0 to get actual width of menu
              that.menu.style.minWidth = 0;

              var actualMenuWidth = menuInner.firstChild.offsetWidth;

              if (actualMenuWidth > that.sizeInfo.menuInnerInnerWidth) {
                that.sizeInfo.menuInnerInnerWidth = actualMenuWidth;
                menuInner.firstChild.style.minWidth = that.sizeInfo.menuInnerInnerWidth + 'px';
              }

              // reset to default CSS styling
              that.menu.style.minWidth = '';
            }
          }
        }

        if ((!isSearching && that.options.source.data || isSearching && that.options.source.search) && that.selectpicker.current.hasMore && currentChunk === chunkCount - 1) {
          // Don't load the next chunk until scrolling has started
          // This prevents unnecessary requests while the user is typing if pageSize is <= chunkSize
          if (scrollTop > 0) {
            // Chunks use 0-based indexing, but pages use 1-based. Add 1 to convert and add 1 again to get next page
            var page = Math.floor((currentChunk * that.options.chunkSize) / that.options.source.pageSize) + 2;

            that.fetchData(function () {
              that.render();
              that.buildList(size, isSearching);
              that.setPositionData();
              scroll(scrollTop);
            }, isSearching ? 'search' : 'data', page, isSearching ? that.selectpicker.search.previousValue : undefined);
          }
        }
      }

      that.prevActiveElement = that.activeElement;

      if (!that.options.liveSearch) {
        that.menuInner.focus();
      } else if (isSearching && init) {
        var index = 0,
            newActive;

        if (!that.selectpicker.view.canHighlight[index]) {
          index = 1 + that.selectpicker.view.canHighlight.slice(1).indexOf(true);
        }

        newActive = that.selectpicker.view.visibleElements[index];

        that.defocusItem(that.selectpicker.view.currentActive);

        that.activeElement = (that.selectpicker.current.data[index] || {}).element;

        that.focusItem(newActive);
      }
    }

    this._replace('createViewResize', window, 'resize', function () {
      var isActive = that.newElement.classList.contains(classNames.SHOW);

      if (isActive) scroll(that.menuInner.scrollTop);
    });
  }

  focusItem (li, liData, noStyle) {
    if (li) {
      liData = liData || this.selectpicker.current.data[this.selectpicker.current.elements.indexOf(this.activeElement)];
      var a = li.firstChild;

      if (a) {
        a.setAttribute('aria-setsize', this.selectpicker.view.size);
        a.setAttribute('aria-posinset', liData.posinset);

        if (noStyle !== true) {
          this.focusedParent.setAttribute('aria-activedescendant', a.id);
          li.classList.add('active');
          a.classList.add('active');
        }
      }
    }
  }

  defocusItem (li) {
    if (li) {
      li.classList.remove('active');
      if (li.firstChild) li.firstChild.classList.remove('active');
    }
  }

  setPlaceholder () {
    var that = this,
        updateIndex = false;

    if ((this.options.placeholder || this.options.allowClear) && !this.multiple) {
      if (!this.selectpicker.view.titleOption) this.selectpicker.view.titleOption = document.createElement('option');

      // this option doesn't create a new <li> element, but does add a new option at the start,
      // so startIndex should increase to prevent having to check every option for the bs-title-option class
      updateIndex = true;

      var element = this.element,
          selectTitleOption = false,
          titleNotAppended = !this.selectpicker.view.titleOption.parentNode,
          selectedIndex = element.selectedIndex,
          selectedOption = element.options[selectedIndex],
          firstSelectable = element.querySelector('select > *:not(:disabled)'),
          firstSelectableIndex = firstSelectable ? firstSelectable.index : 0,
          navigation = window.performance && window.performance.getEntriesByType('navigation'),
          // Safari doesn't support getEntriesByType('navigation') - fall back to performance.navigation
          isNotBackForward = (navigation && navigation.length) ? navigation[0].type !== 'back_forward' : window.performance.navigation.type !== 2;

      if (titleNotAppended) {
        // Use native JS to prepend option (faster)
        this.selectpicker.view.titleOption.className = 'bs-title-option';
        this.selectpicker.view.titleOption.value = '';

        // Check if selected or data-selected attribute is already set on an option. If not, select the titleOption option.
        selectTitleOption = !selectedOption || (selectedIndex === firstSelectableIndex && selectedOption.defaultSelected === false);
      }

      if (titleNotAppended || this.selectpicker.view.titleOption.index !== 0) {
        element.insertBefore(this.selectpicker.view.titleOption, element.firstChild);
      }

      // Set selected *after* appending to select
      if (selectTitleOption && isNotBackForward) {
        element.selectedIndex = 0;
      } else if (document.readyState !== 'complete') {
        // if navigation type is back_forward, there's a chance the select will have its value set by BFCache
        // wait for that value to be set, then run render again
        window.addEventListener('pageshow', function () {
          if (that.selectpicker.view.displayedValue !== element.value) that.render();
        });
      }
    }

    return updateIndex;
  }



/* eslint-disable no-undef */
// Shared ordered source fragment consumed by the Grunt JS build.
  fetchData (callback, type, page, searchValue) {
    page = page || 1;
    type = type || 'data';

    var that = this,
        data = this.options.source[type],
        builtData;

    if (data) {
      this.options.virtualScroll = true;

      if (typeof data === 'function') {
        data.call(
          this,
          function (data, more, totalItems) {
            var current = that.selectpicker[type === 'search' ? 'search' : 'main'];
            current.hasMore = more;
            current.totalItems = totalItems;
            builtData = that.buildData(data, type);
            callback.call(that, builtData);
            that._emit('fetched');
          },
          page,
          searchValue
        );
      } else if (Array.isArray(data)) {
        builtData = that.buildData(data, type);
        callback.call(that, builtData);
      }
    } else {
      builtData = this.buildData(false, type);
      callback.call(that, builtData);
    }
  }

  buildData (data, type) {
    var that = this;
    var dataGetter = data === false ? getOptionData.fromOption : getOptionData.fromDataSource;

    var optionSelector = ':not([hidden]):not([data-hidden="true"]):not([style*="display: none"])',
        mainData = [],
        startLen = this.selectpicker.main.data ? this.selectpicker.main.data.length : 0,
        optID = 0,
        startIndex = this.setPlaceholder() && !data ? 1 : 0; // append the titleOption if necessary and skip the first option in the loop

    if (type === 'search') {
      startLen = this.selectpicker.search.data.length;
    }

    if (this.options.hideDisabled) optionSelector += ':not(:disabled)';

    var selectOptions = data ? data.filter(filterHidden, this) : this.element.querySelectorAll('select > *' + optionSelector);

    function addDivider (config) {
      var previousData = mainData[mainData.length - 1];

      // ensure optgroup doesn't create back-to-back dividers
      if (
        previousData &&
          previousData.type === 'divider' &&
          (previousData.optID || config.optID)
      ) {
        return;
      }

      config = config || {};
      config.type = 'divider';

      mainData.push(config);
    }

    function addOption (item, config) {
      config = config || {};

      config.divider = dataGetter(item, 'divider');

      if (config.divider === true) {
        addDivider({
          optID: config.optID
        });
      } else {
        var liIndex = mainData.length + startLen,
            cssText = dataGetter(item, 'style'),
            inlineStyle = cssText ? htmlEscape(cssText) : '',
            optionClass = (item.className || '') + (config.optgroupClass || '');

        if (config.optID) optionClass = 'opt ' + optionClass;

        config.optionClass = optionClass.trim();
        config.inlineStyle = inlineStyle;

        config.text = dataGetter(item, 'text');
        config.title = dataGetter(item, 'title');
        config.content = dataGetter(item, 'content');
        config.tokens = dataGetter(item, 'tokens');
        config.subtext = dataGetter(item, 'subtext');
        config.icon = dataGetter(item, 'icon');

        config.display = config.content || config.text;
        config.value = item.value === undefined ? item.text : item.value;
        config.type = 'option';
        config.index = liIndex;

        config.option = !item.option ? item : item.option; // reference option element if it exists
        config.option.liIndex = liIndex;
        config.selected = !!item.selected;
        config.disabled = config.disabled || !!item.disabled;

        if (data !== false) {
          if (that.selectpicker.optionValuesDataMap[config.value]) {
            config = Object.assign(that.selectpicker.optionValuesDataMap[config.value], config);
          } else {
            that.selectpicker.optionValuesDataMap[config.value] = config;
          }
        }

        mainData.push(config);
      }
    }

    function addOptgroup (index, selectOptions) {
      var optgroup = selectOptions[index],
          // skip placeholder option
          previous = index - 1 < startIndex ? false : selectOptions[index - 1],
          next = selectOptions[index + 1],
          options = data ? optgroup.children.filter(filterHidden, this) : optgroup.querySelectorAll('option' + optionSelector);

      if (!options.length) return;

      var config = {
            display: htmlEscape(dataGetter(item, 'label')),
            subtext: dataGetter(optgroup, 'subtext'),
            icon: dataGetter(optgroup, 'icon'),
            type: 'optgroup-label',
            optgroupClass: ' ' + (optgroup.className || ''),
            optgroup: optgroup
          },
          headerIndex,
          lastIndex;

      optID++;

      if (previous) {
        addDivider({ optID: optID });
      }

      config.optID = optID;

      mainData.push(config);

      for (var j = 0, len = options.length; j < len; j++) {
        var option = options[j];

        if (j === 0) {
          headerIndex = mainData.length - 1;
          lastIndex = headerIndex + len;
        }

        addOption(option, {
          headerIndex: headerIndex,
          lastIndex: lastIndex,
          optID: config.optID,
          optgroupClass: config.optgroupClass,
          disabled: optgroup.disabled
        });
      }

      if (next) {
        addDivider({ optID: optID });
      }
    }

    var item;

    for (var len = selectOptions.length, i = startIndex; i < len; i++) {
      item = selectOptions[i];
      var children = item.children;

      if (children && children.length) {
        addOptgroup.call(this, i, selectOptions);
      } else {
        addOption.call(this, item, {});
      }
    }

    switch (type) {
      case 'data': {
        if (!this.selectpicker.main.data) {
          this.selectpicker.main.data = [];
        }
        Array.prototype.push.apply(this.selectpicker.main.data, mainData);
        this.selectpicker.current.data = this.selectpicker.main.data;
        break;
      }
      case 'search': {
        Array.prototype.push.apply(this.selectpicker.search.data, mainData);
        break;
      }
    }

    return mainData;
  }

  buildList (size, searching) {
    var that = this,
        selectData = searching ? this.selectpicker.search.data : this.selectpicker.main.data,
        mainElements = [],
        widestOptionLength = 0;

    if (that.options.showTick || that.multiple || that.options.selectionIndicator === 'checkbox') {
      elementTemplates.checkMark.className = this.options.selectionIndicator === 'checkbox'
        ? 'check-mark bs-selection-indicator'
        : this.options.iconBase + ' ' + that.options.tickIcon + ' check-mark';

      if (!elementTemplates.checkMark.parentNode) {
        elementTemplates.a.appendChild(elementTemplates.checkMark);
      }
    }

    function buildElement (mainElements, item) {
      var liElement,
          combinedLength = 0;

      switch (item.type) {
        case 'divider':
          liElement = generateOption.li(
            false,
            classNames.DIVIDER,
            (item.optID ? item.optID + 'div' : undefined)
          );

          break;

        case 'option':
          liElement = generateOption.li(
            generateOption.a(
              generateOption.text.call(that, item),
              item.optionClass,
              item.inlineStyle
            ),
            '',
            item.optID
          );

          if (liElement.firstChild) {
            liElement.firstChild.id = that.selectId + '-' + item.index;
          }

          break;

        case 'optgroup-label':
          liElement = generateOption.li(
            generateOption.label.call(that, item),
            'dropdown-header' + item.optgroupClass,
            item.optID
          );

          break;
      }

      if (item.content) item.sanitized = false;

      if (!item.element) {
        item.element = liElement;
      } else {
        item.element.innerHTML = liElement.innerHTML;
      }
      mainElements.push(item.element);

      // count the number of characters in the option - not perfect, but should work in most cases
      if (item.display) combinedLength += item.display.length;
      if (item.subtext) combinedLength += item.subtext.length;
      // if there is an icon, ensure this option's width is checked
      if (item.icon) combinedLength += 1;

      if (combinedLength > widestOptionLength) {
        widestOptionLength = combinedLength;

        // guess which option is the widest
        that.selectpicker.view.widestOption = mainElements[mainElements.length - 1];
      }
    }

    var startIndex = size || 0;

    for (var len = selectData.length, i = startIndex; i < len; i++) {
      var item = selectData[i];

      buildElement(mainElements, item);
    }

    if (size) {
      if (searching) {
        Array.prototype.push.apply(this.selectpicker.search.elements, mainElements);
      } else {
        Array.prototype.push.apply(this.selectpicker.main.elements, mainElements);
        this.selectpicker.current.elements = this.selectpicker.main.elements;
      }
    } else {
      if (searching) {
        this.selectpicker.search.elements = mainElements;
      } else {
        this.selectpicker.main.elements = this.selectpicker.current.elements = mainElements;
      }
    }
  }


/* eslint-disable no-undef */
// Shared ordered source fragment consumed by the Grunt JS build.
  findLis () {
    return this.menuInner.querySelectorAll('.inner > li');
  }

  render (init) {
    var that = this,
        element = this.element,
        // ensure titleOption is appended and selected (if necessary) before getting selectedOptions
        placeholderSelected = this.setPlaceholder() && element.selectedIndex === 0,
        selectedOptions = getSelectedOptions.call(this),
        selectedCount = selectedOptions.length,
        selectedValues = getSelectValues.call(this, selectedOptions),
        button = this.button,
        buttonInner = button.querySelector('.filter-option-inner-inner'),
        multipleSeparator = document.createTextNode(this.options.multipleSeparator),
        titleFragment = elementTemplates.fragment.cloneNode(false),
        forceCount = this.multiple && this.options.showSelectedTags && selectedCount > 0,
        showCount,
        countMax,
        hasContent = false;

    function createSelected (item) {
      if (item.selected) {
        that.createOption(item, true);
      } else if (item.children && item.children.length) {
        item.children.map(createSelected);
      }
    }

    // create selected option elements to ensure select value is correct
    if (this.options.source.data && init) {
      selectedOptions.map(createSelected);
      element.appendChild(this.selectpicker.main.optionQueue);

      if (placeholderSelected) placeholderSelected = element.selectedIndex === 0;
    }

    button.classList.toggle('bs-placeholder', that.multiple ? !selectedCount : !selectedValues && selectedValues !== 0);

    if (!that.multiple && selectedOptions.length === 1) {
      that.selectpicker.view.displayedValue = selectedValues;
    }

    if (this.options.selectedTextFormat === 'static') {
      titleFragment = generateOption.text.call(this, { text: this.options.placeholder }, true);
    } else {
      showCount = forceCount || this.multiple && this.options.selectedTextFormat.indexOf('count') !== -1 && selectedCount > 0;

      // determine if the number of selected options will be shown (showCount === true)
      if (showCount && !forceCount) {
        countMax = this.options.selectedTextFormat.split('>');
        showCount = (countMax.length > 1 && selectedCount > countMax[1]) || (countMax.length === 1 && selectedCount >= 2);
      }

      // only loop through all selected options if the count won't be shown
      if (showCount === false) {
        if (!placeholderSelected) {
          for (var selectedIndex = 0; selectedIndex < selectedCount; selectedIndex++) {
            if (selectedIndex < 50) {
              var option = selectedOptions[selectedIndex],
                  titleOptions = {};

              if (option) {
                if (this.multiple && selectedIndex > 0) {
                  titleFragment.appendChild(multipleSeparator.cloneNode(false));
                }

                if (option.title) {
                  titleOptions.text = option.title;
                } else if (option.content && that.options.showContent) {
                  titleOptions.content = option.content.toString();
                  hasContent = true;
                } else {
                  if (that.options.showIcon) {
                    titleOptions.icon = option.icon;
                  }
                  if (that.options.showSubtext && !that.multiple && option.subtext) titleOptions.subtext = ' ' + option.subtext;
                  titleOptions.text = option.text.trim();
                }

                titleFragment.appendChild(generateOption.text.call(this, titleOptions, true));
              }
            } else {
              break;
            }
          }

          // add ellipsis
          if (selectedCount > 49) {
            titleFragment.appendChild(document.createTextNode('...'));
          }
        }
      } else {
        var optionSelector = ':not([hidden]):not([data-hidden="true"]):not([data-divider="true"]):not([style*="display: none"])';
        if (this.options.hideDisabled) optionSelector += ':not(:disabled)';

        // If this is a multiselect, and selectedTextFormat is count, then show 1 of 2 selected, etc.
        var totalCount = this.element.querySelectorAll('select > option' + optionSelector + ', optgroup' + optionSelector + ' option' + optionSelector).length,
            tr8nText = (typeof this.options.countSelectedText === 'function') ? this.options.countSelectedText(selectedCount, totalCount) : this.options.countSelectedText;

        titleFragment = generateOption.text.call(this, {
          text: tr8nText.replace('{0}', selectedCount.toString()).replace('{1}', totalCount.toString())
        }, true);
      }
    }

    // If the select doesn't have a title, then use the default, or if nothing is set at all, use noneSelectedText
    if (!titleFragment.childNodes.length) {
      titleFragment = generateOption.text.call(this, {
        text: this.options.placeholder ? this.options.placeholder : this.options.noneSelectedText
      }, true);
    }

    // if the select has a title, apply it to the button, and if not, apply titleFragment text
    button.title = titleFragment.textContent.replace(/<[^>]*>?/g, '').trim();

    if (this.options.sanitize && hasContent) {
      sanitizeHtml([titleFragment], that.options.whiteList, that.options.sanitizeFn);
    }

    buttonInner.innerHTML = '';
    buttonInner.appendChild(titleFragment);

    this.syncTagEditor();

    this._emit('rendered');
  }

  usesTagEditor () {
    return this.options.liveSearch && (this.options.showSelectedTags || this.options.openOptions);
  }

  syncTagEditor () {
    if (!this.usesTagEditor()) return;

    if (this.selectedItems) {
      var selectedOptions = getSelectedOptions.call(this),
          useListStyle = this.options.selectedItemsStyle === 'list';

      this.selectedItems.innerHTML = '';
      this.selectedItems.hidden = !selectedOptions.length;
      this.selectedItems.classList.toggle('list-group', useListStyle);

      for (var i = 0; i < selectedOptions.length; i++) {
        var item = selectedOptions[i],
            selectedTag = document.createElement('button'),
            removeText = this.options.selectedTagRemoveLabel + ' ' + getOptionLabelText(item),
            content = document.createElement('span'),
            label = document.createElement('span'),
            remove = document.createElement('span'),
            icon;

        selectedTag.type = 'button';
        selectedTag.className = useListStyle
          ? 'bs-selected-item list-group-item list-group-item-action'
          : 'bs-selected-item';
        selectedTag.setAttribute('data-option-value', item.value);
        selectedTag.setAttribute('aria-label', removeText);
        selectedTag.title = removeText;

        content.className = 'bs-selected-item-content';

        if (item.icon && this.options.showIcon) {
          icon = document.createElement('span');
          icon.className = 'bs-selected-item-icon ' + this.options.iconBase + ' ' + item.icon;
          icon.setAttribute('aria-hidden', 'true');
          content.appendChild(icon);
        }

        label.className = 'bs-selected-item-label';
        label.textContent = getOptionLabelText(item);
        content.appendChild(label);

        remove.className = 'bs-selected-item-remove';
        remove.setAttribute('aria-hidden', 'true');
        remove.textContent = '\u00d7';

        selectedTag.appendChild(content);
        selectedTag.appendChild(remove);
        this.selectedItems.appendChild(selectedTag);
      }
    }

    this.syncOpenOptionButton();

    if (this.newElement && this.newElement.classList.contains(classNames.SHOW)) {
      this.setSize(true);
    }
  }

  syncOpenOptionButton () {
    if (!this.createOptionButton) return;

    var searchValue = this.searchbox ? this.searchbox.value : '',
        normalizedValue = searchValue.toString().trim(),
        shouldShow = !!normalizedValue &&
          !this.selectpicker.openOption.isCreating &&
          !this.findOptionBySearchValue(normalizedValue);

    this.createOptionButton.hidden = !shouldShow;
    this.createOptionButton.disabled = this.selectpicker.openOption.isCreating;

    if (shouldShow) {
      this.createOptionButton.textContent = this.options.openOptionsText.replace('{0}', normalizedValue);
      this.createOptionButton.setAttribute('data-search-value', normalizedValue);
    } else {
      this.createOptionButton.textContent = '';
      this.createOptionButton.removeAttribute('data-search-value');
    }
  }

  findOptionByValue (value, dataSet) {
    var options = dataSet || this.selectpicker.main.data,
        stringValue = String(value);

    for (var i = 0; i < options.length; i++) {
      var option = options[i];

      if (option.type === 'option' && String(option.value) === stringValue) {
        return option;
      }
    }

    return null;
  }

  findOptionBySearchValue (searchValue) {
    var options = this.options.source.data || this.options.source.search
          ? Object.values(this.selectpicker.optionValuesDataMap)
          : this.selectpicker.main.data,
        normalizedSearch = normalizeSearchInput(searchValue, this.options.liveSearchNormalize);

    for (var i = 0; i < options.length; i++) {
      var option = options[i];

      if (option.type !== 'option') continue;

      if (
        normalizeSearchInput(option.text, this.options.liveSearchNormalize) === normalizedSearch ||
        normalizeSearchInput(option.value, this.options.liveSearchNormalize) === normalizedSearch ||
        normalizeSearchInput(option.title, this.options.liveSearchNormalize) === normalizedSearch
      ) {
        return option;
      }
    }

    return null;
  }

  createOptionElement (optionData) {
    var option = document.createElement('option');

    option.value = optionData.value === undefined ? optionData.text : optionData.value;
    option.textContent = optionData.text === undefined ? option.value : optionData.text;

    if (optionData.className) option.className = optionData.className;
    if (optionData.title) option.title = optionData.title;
    if (optionData.content) option.setAttribute('data-content', optionData.content);
    if (optionData.tokens) option.setAttribute('data-tokens', optionData.tokens);
    if (optionData.subtext) option.setAttribute('data-subtext', optionData.subtext);
    if (optionData.icon) option.setAttribute('data-icon', optionData.icon);
    if (optionData.disabled) option.disabled = true;
    if (optionData.hidden) option.hidden = true;

    return option;
  }

  appendCreatedSearchResults (searchValue) {
    if (!this.selectpicker.createdOptions.length) return;

    var matches = [];

    for (var i = 0; i < this.selectpicker.createdOptions.length; i++) {
      var option = this.selectpicker.createdOptions[i];

      if (
        stringSearch(option, normalizeSearchInput(searchValue, this.options.liveSearchNormalize), this._searchStyle(), this.options.liveSearchNormalize) &&
        !this.findOptionByValue(option.value, this.selectpicker.search.data)
      ) {
        matches.push(option);
      }
    }

    if (matches.length) this.buildData(matches, 'search');
  }

  addCreatedOption (optionData) {
    optionData = Object.assign({}, optionData);
    optionData.value = optionData.value === undefined ? optionData.text : optionData.value;
    optionData.text = optionData.text === undefined ? optionData.value : optionData.text;

    var size = this.selectpicker.main.elements ? this.selectpicker.main.elements.length : 0,
        option = this.createOptionElement(optionData);
    optionData.option = option;

    this.element.appendChild(option);
    var builtOptions = this.buildData([optionData], 'data'),
        builtOption = builtOptions[0];

    this.buildList(size);
    this.selectpicker.createdOptions.push(builtOption);

    return builtOption;
  }

  removeSelectedTag (value) {
    var option = this.findOptionByValue(value);

    if (!option || !option.selected) return;

    var prevValue = getSelectValues.call(this);

    this.setSelected(option, false);
    changedArguments = [option.index, false, prevValue];
    triggerNative(this.element, 'change');

    if (this.options.liveSearch) this.searchbox.focus();
  }

  createOpenOption (searchValue) {
    searchValue = searchValue === undefined || searchValue === null ? '' : searchValue.toString().trim();

    if (!searchValue || this.selectpicker.openOption.isCreating) return;

    var existingOption = this.findOptionBySearchValue(searchValue);

    if (existingOption) {
      if (!existingOption.selected) {
        var prevSelectedValue = getSelectValues.call(this);

        this.setSelected(existingOption, true);
        changedArguments = [existingOption.index, true, prevSelectedValue];
        triggerNative(this.element, 'change');
      }

      if (this.options.liveSearch) this.searchbox.focus();
      return;
    }

    var that = this,
        prevValue = getSelectValues.call(this),
        createHandler = this.options.source.create;

    this.selectpicker.openOption.isCreating = true;
    this.syncOpenOptionButton();

    function finalize (createdOption) {
      that.selectpicker.openOption.isCreating = false;

      if (createdOption === undefined || createdOption === null || createdOption === false) {
        that.syncOpenOptionButton();
        return;
      }

      if (Array.isArray(createdOption)) createdOption = createdOption[0];
      if (typeof createdOption !== 'object') {
        createdOption = {
          text: createdOption,
          value: createdOption
        };
      }

      if (!createdOption.text && !createdOption.value) {
        createdOption.text = searchValue;
      }

      if (createdOption.value === undefined) createdOption.value = createdOption.text;
      if (createdOption.text === undefined) createdOption.text = createdOption.value;

      var option = that.findOptionByValue(createdOption.value) || that.findOptionBySearchValue(createdOption.text);

      if (!option) {
        option = that.addCreatedOption(createdOption);
      }

      that.setSelected(option, true);

      if (that.options.source.data) that.element.appendChild(that.selectpicker.main.optionQueue);

      if (that.searchbox) {
        that.searchbox.value = '';
      }

      that.selectpicker.search.previousValue = '';
      that.selectpicker.search.data = [];
      that.selectpicker.search.elements = [];
      that.createView(false);

      changedArguments = [option.index, true, prevValue];
      triggerNative(that.element, 'change');

      if (that.options.liveSearch) that.searchbox.focus();
    }

    if (typeof createHandler === 'function') {
      var returnedOption = createHandler.call(this, finalize, searchValue);

      if (returnedOption && typeof returnedOption.then === 'function') {
        returnedOption.then(finalize);
      } else if (returnedOption !== undefined) {
        finalize(returnedOption);
      }
    } else {
      finalize({
        text: searchValue,
        value: searchValue
      });
    }
  }

  /**
     * @param [newStyle]
     * @param [status]
     */
  setStyle (newStyle, status) {
    var button = this.button,
        newElement = this.newElement,
        style = this.options.style.trim(),
        buttonClass;

    if (this.element.getAttribute('class')) {
      var extra = this.element.getAttribute('class').replace(/selectpicker|mobile-device|bs-select-hidden|validate\[.*\]/gi, '').trim();
      if (extra) newElement.classList.add.apply(newElement.classList, extra.split(/\s+/));
    }

    if (newStyle) {
      buttonClass = newStyle.trim();
    } else {
      buttonClass = style;
    }

    if (status === 'add') {
      if (buttonClass) button.classList.add.apply(button.classList, buttonClass.split(' '));
    } else if (status === 'remove') {
      if (buttonClass) button.classList.remove.apply(button.classList, buttonClass.split(' '));
    } else {
      if (style) button.classList.remove.apply(button.classList, style.split(' '));
      if (buttonClass) button.classList.add.apply(button.classList, buttonClass.split(' '));
    }
  }



/* eslint-disable no-undef */
// Shared ordered source fragment consumed by the Grunt JS build.
  liHeight (refresh) {
    if (!refresh && (this.options.size === false || Object.keys(this.sizeInfo).length)) return;

    var newElement = elementTemplates.div.cloneNode(false),
        menu = elementTemplates.div.cloneNode(false),
        menuInner = elementTemplates.div.cloneNode(false),
        menuInnerInner = document.createElement('ul'),
        divider = elementTemplates.li.cloneNode(false),
        dropdownHeader = elementTemplates.li.cloneNode(false),
        li,
        a = elementTemplates.a.cloneNode(false),
        text = elementTemplates.span.cloneNode(false),
        header = this.options.header && this.menu.querySelectorAll('.' + classNames.POPOVERHEADER).length > 0 ? this.menu.querySelector('.' + classNames.POPOVERHEADER).cloneNode(true) : null,
        search = this.options.liveSearch && this.menu.querySelector('.bs-searchbox')
          ? this.menu.querySelector('.bs-searchbox').cloneNode(true)
          : null,
        actions = this.options.actionsBox && this.multiple && this.menu.querySelectorAll('.bs-actionsbox').length > 0 ? this.menu.querySelector('.bs-actionsbox').cloneNode(true) : null,
        doneButton = this.options.doneButton && this.multiple && this.menu.querySelectorAll('.bs-donebutton').length > 0 ? this.menu.querySelector('.bs-donebutton').cloneNode(true) : null,
        firstOption = this.element.options[0];

    this.sizeInfo.selectWidth = this.newElement.offsetWidth;

    text.className = 'text';
    a.className = 'dropdown-item ' + (firstOption ? firstOption.className : '');
    newElement.className = this.menu.parentNode.className + ' ' + classNames.SHOW;
    newElement.style.width = 0; // ensure button width doesn't affect natural width of menu when calculating
    menu.className = classNames.MENU + ' ' + classNames.SHOW;
    menuInner.className = 'inner ' + classNames.SHOW;
    menuInnerInner.className = classNames.MENU + ' inner ' + classNames.SHOW;
    divider.className = classNames.DIVIDER;
    dropdownHeader.className = 'dropdown-header';

    text.appendChild(document.createTextNode('\u200b'));

    if (this.selectpicker.current.data.length) {
      for (var i = 0; i < this.selectpicker.current.data.length; i++) {
        var data = this.selectpicker.current.data[i];
        if (data.type === 'option' && window.getComputedStyle(data.element.firstChild).display !== 'none') {
          li = data.element;
          break;
        }
      }
    } else {
      li = elementTemplates.li.cloneNode(false);
      a.appendChild(text);
      li.appendChild(a);
    }

    dropdownHeader.appendChild(text.cloneNode(true));

    if (this.selectpicker.view.widestOption) {
      menuInnerInner.appendChild(this.selectpicker.view.widestOption.cloneNode(true));
    }

    menuInnerInner.appendChild(li);
    menuInnerInner.appendChild(divider);
    menuInnerInner.appendChild(dropdownHeader);
    if (header) menu.appendChild(header);
    if (search) menu.appendChild(search);
    if (actions) menu.appendChild(actions);
    menuInner.appendChild(menuInnerInner);
    menu.appendChild(menuInner);
    if (doneButton) menu.appendChild(doneButton);
    newElement.appendChild(menu);

    document.body.appendChild(newElement);

    var liHeight = li.offsetHeight,
        dropdownHeaderHeight = dropdownHeader ? dropdownHeader.offsetHeight : 0,
        headerHeight = header ? header.offsetHeight : 0,
        searchHeight = search ? search.offsetHeight : 0,
        actionsHeight = actions ? actions.offsetHeight : 0,
        doneButtonHeight = doneButton ? doneButton.offsetHeight : 0,
        dividerHeight = outerHeight(divider, true),
        menuStyle = window.getComputedStyle(menu),
        menuWidth = menu.offsetWidth,
        menuPadding = {
          vert: toInteger(menuStyle.paddingTop) +
                  toInteger(menuStyle.paddingBottom) +
                  toInteger(menuStyle.borderTopWidth) +
                  toInteger(menuStyle.borderBottomWidth),
          horiz: toInteger(menuStyle.paddingLeft) +
                  toInteger(menuStyle.paddingRight) +
                  toInteger(menuStyle.borderLeftWidth) +
                  toInteger(menuStyle.borderRightWidth)
        },
        menuExtras = {
          vert: menuPadding.vert +
                  toInteger(menuStyle.marginTop) +
                  toInteger(menuStyle.marginBottom) + 2,
          horiz: menuPadding.horiz +
                  toInteger(menuStyle.marginLeft) +
                  toInteger(menuStyle.marginRight) + 2
        },
        scrollBarWidth;

    menuInner.style.overflowY = 'scroll';

    scrollBarWidth = menu.offsetWidth - menuWidth;

    document.body.removeChild(newElement);

    this.sizeInfo.liHeight = liHeight;
    this.sizeInfo.dropdownHeaderHeight = dropdownHeaderHeight;
    this.sizeInfo.headerHeight = headerHeight;
    this.sizeInfo.searchHeight = searchHeight;
    this.sizeInfo.actionsHeight = actionsHeight;
    this.sizeInfo.doneButtonHeight = doneButtonHeight;
    this.sizeInfo.dividerHeight = dividerHeight;
    this.sizeInfo.menuPadding = menuPadding;
    this.sizeInfo.menuExtras = menuExtras;
    this.sizeInfo.menuWidth = menuWidth;
    this.sizeInfo.menuInnerInnerWidth = menuWidth - menuPadding.horiz;
    this.sizeInfo.totalMenuWidth = this.sizeInfo.menuWidth;
    this.sizeInfo.scrollBarWidth = scrollBarWidth;
    this.sizeInfo.selectHeight = this.newElement.offsetHeight;

    this.setPositionData();
  }

  getSelectPosition () {
    var that = this,
        winScrollTop = window.pageYOffset,
        winScrollLeft = window.pageXOffset,
        winHeight = document.documentElement.clientHeight,
        winWidth = document.documentElement.clientWidth,
        pos = offset(that.newElement);

    this.sizeInfo.selectOffsetTop = pos.top - winScrollTop;
    this.sizeInfo.selectOffsetBot = winHeight - this.sizeInfo.selectOffsetTop - this.sizeInfo.selectHeight;
    this.sizeInfo.selectOffsetLeft = pos.left - winScrollLeft;
    this.sizeInfo.selectOffsetRight = winWidth - this.sizeInfo.selectOffsetLeft - this.sizeInfo.selectWidth;
  }

  setMenuSize (isAuto) {
    this.getSelectPosition();

    var selectWidth = this.sizeInfo.selectWidth,
        liHeight = this.sizeInfo.liHeight,
        headerHeight = this.sizeInfo.headerHeight,
        searchHeight = this.sizeInfo.searchHeight,
        actionsHeight = this.sizeInfo.actionsHeight,
        doneButtonHeight = this.sizeInfo.doneButtonHeight,
        divHeight = this.sizeInfo.dividerHeight,
        menuPadding = this.sizeInfo.menuPadding,
        menuInnerHeight,
        menuHeight,
        divLength = 0,
        minHeight,
        _minHeight,
        maxHeight,
        menuInnerMinHeight,
        estimate,
        isDropup;

    if (this.options.dropupAuto) {
      // Get the estimated height of the menu without scrollbars.
      estimate = liHeight * this.selectpicker.current.data.length + menuPadding.vert;

      isDropup = this.sizeInfo.selectOffsetTop - this.sizeInfo.selectOffsetBot > this.sizeInfo.menuExtras.vert && estimate + this.sizeInfo.menuExtras.vert + 50 > this.sizeInfo.selectOffsetBot;

      // ensure dropup doesn't change while searching (so menu doesn't bounce back and forth)
      if (this.selectpicker.isSearching === true) {
        isDropup = this.selectpicker.dropup;
      }

      this.newElement.classList.toggle(classNames.DROPUP, isDropup);
      this.selectpicker.dropup = isDropup;
    }

    if (this.options.size === 'auto') {
      _minHeight = this.selectpicker.current.data.length > 3 ? this.sizeInfo.liHeight * 3 + this.sizeInfo.menuExtras.vert - 2 : 0;
      menuHeight = this.sizeInfo.selectOffsetBot - this.sizeInfo.menuExtras.vert;
      minHeight = _minHeight + headerHeight + searchHeight + actionsHeight + doneButtonHeight;
      menuInnerMinHeight = Math.max(_minHeight - menuPadding.vert, 0);

      if (this.newElement.classList.contains(classNames.DROPUP)) {
        menuHeight = this.sizeInfo.selectOffsetTop - this.sizeInfo.menuExtras.vert;
      }

      maxHeight = menuHeight;
      menuInnerHeight = menuHeight - headerHeight - searchHeight - actionsHeight - doneButtonHeight - menuPadding.vert;
    } else if (this.options.size && this.options.size !== 'auto' && this.selectpicker.current.elements.length > this.options.size) {
      for (var i = 0; i < this.options.size; i++) {
        if (this.selectpicker.current.data[i].type === 'divider') divLength++;
      }

      menuHeight = liHeight * this.options.size + divLength * divHeight + menuPadding.vert;
      menuInnerHeight = menuHeight - menuPadding.vert;
      maxHeight = menuHeight + headerHeight + searchHeight + actionsHeight + doneButtonHeight;
      minHeight = menuInnerMinHeight = '';
    }

    setStyles(this.menu, {
      maxHeight: maxHeight + 'px',
      overflow: 'hidden',
      minHeight: minHeight + 'px'
    });

    setStyles(this.menuInner, {
      maxHeight: menuInnerHeight + 'px',
      overflow: 'hidden auto',
      minHeight: menuInnerMinHeight + 'px'
    });

    // ensure menuInnerHeight is always a positive number to prevent issues calculating chunkSize in createView
    this.sizeInfo.menuInnerHeight = Math.max(menuInnerHeight, 1);

    if (this.selectpicker.current.data.length && this.selectpicker.current.data[this.selectpicker.current.data.length - 1].position > this.sizeInfo.menuInnerHeight) {
      this.sizeInfo.hasScrollBar = true;
      this.sizeInfo.totalMenuWidth = this.sizeInfo.menuWidth + this.sizeInfo.scrollBarWidth;
    }

    if (this.options.dropdownAlignRight === 'auto') {
      this.menu.classList.toggle(classNames.MENUEND, this.sizeInfo.selectOffsetLeft > this.sizeInfo.selectOffsetRight && this.sizeInfo.selectOffsetRight < (this.sizeInfo.totalMenuWidth - selectWidth));
    }

    if (this.dropdown && this.dropdown._popper) this.dropdown._popper.update();
  }

  setSize (refresh) {
    this.liHeight(refresh);

    if (this.options.header) this.menu.style.paddingTop = 0;

    if (this.options.size !== false) {
      var that = this;

      this.setMenuSize();

      if (this.options.liveSearch) {
        this._replace('setMenuSizeInput', this.searchbox, 'input', function () {
          return that.setMenuSize();
        });
      }

      if (this.options.size === 'auto') {
        var windowSizeHandler = function () {
          return that.setMenuSize();
        };
        this._replace('setMenuSizeResize', window, 'resize', windowSizeHandler);
        this._replace('setMenuSizeScroll', window, 'scroll', windowSizeHandler);
      } else if (this.options.size && this.options.size !== 'auto' && this.selectpicker.current.elements.length > this.options.size) {
        this._removeNamed('setMenuSizeResize');
        this._removeNamed('setMenuSizeScroll');
      }
    }

    this.createView(false, true, refresh);
  }

  setWidth () {
    this.menu.style.minWidth = '';
    this.newElement.style.width = '';
    this.newElement.classList.remove('fit-width');

    if (this.options.width === 'fit') {
      this.newElement.classList.add('fit-width');
      return;
    }

    if (this.options.width && this.options.width !== 'auto') {
      this.newElement.style.width = this.options.width;
    }
  }

  selectPosition () {
    this.bsContainer = createFromHTML('<div class="bs-container" />');

    var that = this,
        container = resolveContainer(this.options.container),
        pos,
        containerPos,
        actualHeight,
        getPlacement = function (element) {
          var Dropdown = getDropdown(),
              containerPosition = {},
              // fall back to dropdown's default display setting if display is not manually set
              display = that.options.display || (Dropdown.Default ? Dropdown.Default.display : false);

          var extraClass = element.getAttribute('class').replace(/form-control|fit-width/gi, '').trim();
          if (extraClass) that.bsContainer.classList.add.apply(that.bsContainer.classList, extraClass.split(/\s+/));
          that.bsContainer.classList.toggle(classNames.DROPUP, element.classList.contains(classNames.DROPUP));
          pos = offset(element);

          if (container !== document.body) {
            containerPos = offset(container);
            var containerStyle = window.getComputedStyle(container);
            containerPos.top += toInteger(containerStyle.borderTopWidth) - container.scrollTop;
            containerPos.left += toInteger(containerStyle.borderLeftWidth) - container.scrollLeft;
          } else {
            containerPos = { top: 0, left: 0 };
          }

          actualHeight = element.classList.contains(classNames.DROPUP) ? 0 : element.offsetHeight;

          // Bootstrap 5 uses Popper for menu positioning
          if (display === 'static') {
            containerPosition.top = pos.top - containerPos.top + actualHeight;
            containerPosition.left = pos.left - containerPos.left;
          }

          containerPosition.width = element.offsetWidth;

          setStyles(that.bsContainer, {
            top: containerPosition.top !== undefined ? containerPosition.top + 'px' : '',
            left: containerPosition.left !== undefined ? containerPosition.left + 'px' : '',
            width: containerPosition.width + 'px'
          });
        };

    this._on(this.button, 'click', function () {
      if (that.isDisabled()) {
        return;
      }

      getPlacement(that.newElement);

      container.appendChild(that.bsContainer);
      that.bsContainer.classList.toggle(classNames.SHOW, !that.button.classList.contains(classNames.SHOW));
      that.bsContainer.appendChild(that.menu);
    });

    var windowHandler = function () {
      var isActive = that.newElement.classList.contains(classNames.SHOW);

      if (isActive) getPlacement(that.newElement);
    };
    this._replace('selectPositionResize', window, 'resize', windowHandler);
    this._replace('selectPositionScroll', window, 'scroll', windowHandler);

    this._on(this.element, 'hide' + EVENT_KEY, function () {
      that._menuHeight = outerHeight(that.menu);
      if (that.bsContainer.parentNode) that.bsContainer.parentNode.removeChild(that.bsContainer);
    });
  }

  createOption (data, init) {
    var optionData = !data.option ? data : data.option;

    if (optionData && optionData.nodeType !== 1) {
      var option = (init ? elementTemplates.selectedOption : elementTemplates.option).cloneNode(true);
      if (optionData.value !== undefined) option.value = optionData.value;
      option.textContent = optionData.text;

      option.selected = true;

      if (optionData.liIndex !== undefined) {
        option.liIndex = optionData.liIndex;
      } else if (!init) {
        option.liIndex = data.index;
      }

      data.option = option;

      this.selectpicker.main.optionQueue.appendChild(option);
    }
  }

  setOptionStatus (selectedOnly) {
    var that = this;

    that.noScroll = false;

    if (that.selectpicker.view.visibleElements && that.selectpicker.view.visibleElements.length) {
      for (var i = 0; i < that.selectpicker.view.visibleElements.length; i++) {
        var liData = that.selectpicker.current.data[i + that.selectpicker.view.position0],
            option = liData.option;

        if (option) {
          if (selectedOnly !== true) {
            that.setDisabled(liData);
          }

          that.setSelected(liData);
        }
      }

      // append optionQueue (documentFragment with option elements for select options)
      if (this.options.source.data) this.element.appendChild(this.selectpicker.main.optionQueue);
    }
  }

  /**
     * @param {Object} liData - the option object that is being changed
     * @param {boolean} selected - true if the option is being selected, false if being deselected
     */
  setSelected (liData, selected) {
    selected = selected === undefined ? liData.selected : selected;

    var li = liData.element,
        activeElementIsSet = this.activeElement !== undefined,
        thisIsActive = this.activeElement === li,
        prevActive,
        a,
        keepActive = thisIsActive || (selected && !this.multiple && !activeElementIsSet);

    if (selected !== undefined) {
      liData.selected = selected;
      if (liData.option) liData.option.selected = selected;
    }

    if (selected && this.options.source.data) {
      this.createOption(liData, false);
    }

    if (!li) return;

    a = li.firstChild;

    if (selected) {
      this.selectedElement = li;
    }

    li.classList.toggle('selected', selected);

    if (keepActive) {
      this.focusItem(li, liData);
      this.selectpicker.view.currentActive = li;
      this.activeElement = li;
    } else {
      this.defocusItem(li);
    }

    if (a) {
      a.classList.toggle('selected', selected);

      if (selected) {
        a.setAttribute('aria-selected', true);
      } else {
        if (this.multiple) {
          a.setAttribute('aria-selected', false);
        } else {
          a.removeAttribute('aria-selected');
        }
      }
    }

    if (!keepActive && !activeElementIsSet && selected && this.prevActiveElement !== undefined) {
      prevActive = this.prevActiveElement;

      this.defocusItem(prevActive);
    }
  }

  /**
     * @param {Object} liData - the option that is being disabled
     */
  setDisabled (liData) {
    var disabled = liData.disabled,
        li = liData.element,
        a;

    if (!li) return;

    a = li.firstChild;

    li.classList.toggle(classNames.DISABLED, disabled);

    if (a) {
      a.classList.toggle(classNames.DISABLED, disabled);

      if (disabled) {
        a.setAttribute('aria-disabled', disabled);
        a.setAttribute('tabindex', -1);
      } else {
        a.removeAttribute('aria-disabled');
        a.setAttribute('tabindex', 0);
      }
    }
  }

  isDisabled () {
    return this.element.disabled;
  }

  checkDisabled () {
    if (this.isDisabled()) {
      this.newElement.classList.add(classNames.DISABLED);
      this.button.classList.add(classNames.DISABLED);
      this.button.setAttribute('aria-disabled', true);
    } else {
      if (this.button.classList.contains(classNames.DISABLED)) {
        this.newElement.classList.remove(classNames.DISABLED);
        this.button.classList.remove(classNames.DISABLED);
        this.button.setAttribute('aria-disabled', false);
      }
    }
  }



/* eslint-disable no-undef */
// Shared ordered source fragment consumed by the Grunt JS build.
  clickListener () {
    var that = this;

    spaceSelectFlag = false;

    this._on(this.button, 'keyup', function (e) {
      if (/(32)/.test(e.keyCode.toString(10)) && spaceSelectFlag) {
        e.preventDefault();
        spaceSelectFlag = false;
      }
    });

    function clearSelection (e) {
      if (that.multiple) {
        that.deselectAll();
      } else {
        var element = that.element,
            prevValue = element.value,
            prevIndex = element.selectedIndex,
            prevOption = element.options[prevIndex],
            prevData = prevOption ? that.selectpicker.main.data[prevOption.liIndex] : false;

        if (prevData) {
          that.setSelected(prevData, false);
        }

        element.selectedIndex = 0;

        changedArguments = [prevIndex, false, prevValue];
        triggerNative(that.element, 'change');
      }

      // remove selected styling if menu is open
      if (that.newElement.classList.contains(classNames.SHOW)) {
        if (that.options.liveSearch) {
          that.searchbox.focus();
        }

        that.createView(false);
      }
    }

    if (this.options.allowClear) {
      this._on(this.button, 'click', function (e) {
        var target = e.target,
            clearButton = that.clearButton;

        if (target === clearButton || target.parentElement === clearButton) {
          e.stopImmediatePropagation();
          clearSelection(e);
        }
      });
    }

    function setFocus () {
      if (that.options.liveSearch) {
        that.searchbox.focus();
      } else {
        that.menuInner.focus();
      }
    }

    function checkPopperExists () {
      if (that.dropdown && that.dropdown._popper && that.dropdown._popper.state) {
        setFocus();
      } else {
        requestAnimationFrame(checkPopperExists);
      }
    }

    this._on(this.element, 'shown' + EVENT_KEY, function () {
      if (that.menuInner.scrollTop !== that.selectpicker.view.scrollTop) {
        that.menuInner.scrollTop = that.selectpicker.view.scrollTop;
      }

      requestAnimationFrame(checkPopperExists);
    });

    // ensure posinset and setsize are correct before selecting an option via a click
    this._delegate(this.menuInner, 'mouseover', 'li a', function () {
      var hoverLi = this.parentElement,
          position0 = that.isVirtual() ? that.selectpicker.view.position0 : 0,
          index = Array.prototype.indexOf.call(hoverLi.parentElement.children, hoverLi),
          hoverData = that.selectpicker.current.data[index + position0];

      that.focusItem(hoverLi, hoverData, true);
    });

    this._delegate(this.menuInner, 'click', 'li a', function (e) {
      that.onOptionClick(this, e);
    });

    this._delegate(this.menu, 'click', 'li.' + classNames.DISABLED + ' a, .' + classNames.POPOVERHEADER + ', .' + classNames.POPOVERHEADER + ' :not(.btn-close):not(.close)', function (e) {
      if (e.currentTarget === this || e.target === this) {
        e.preventDefault();
        e.stopPropagation();
        if (that.options.liveSearch && !e.target.classList.contains('btn-close') && !e.target.classList.contains('close')) {
          that.searchbox.focus();
        } else {
          that.button.focus();
        }
      }
    });

    this._delegate(this.menuInner, 'click', '.divider, .dropdown-header', function (e) {
      e.preventDefault();
      e.stopPropagation();
      if (that.options.liveSearch) {
        that.searchbox.focus();
      } else {
        that.button.focus();
      }
    });

    this._delegate(this.menu, 'click', '.' + classNames.POPOVERHEADER + ' .btn-close, .' + classNames.POPOVERHEADER + ' .close', function () {
      that.dropdown.hide();
    });

    this._delegate(this.newElement, 'click', '.bs-selected-item', function (e) {
      e.preventDefault();
      e.stopPropagation();
      that.removeSelectedTag(this.getAttribute('data-option-value'));
    });

    this._delegate(this.menu, 'click', '.bs-create-option', function (e) {
      e.preventDefault();
      e.stopPropagation();
      that.createOpenOption(this.getAttribute('data-search-value'));
    });

    if (this.searchbox) {
      this._on(this.searchbox, 'click', function (e) {
        e.stopPropagation();
      });
    }

    this._delegate(this.menu, 'click', '.actions-btn', function (e) {
      if (that.options.liveSearch) {
        that.searchbox.focus();
      } else {
        that.button.focus();
      }

      e.preventDefault();
      e.stopPropagation();

      if (this.classList.contains('bs-select-all')) {
        that.selectAll();
      } else {
        that.deselectAll();
      }
    });

    this._on(this.button, 'focus', function (e) {
      var tabindex = that.element.getAttribute('tabindex');

      // only change when button is actually focused
      if (tabindex !== undefined && tabindex !== null && e.isTrusted) {
        // apply select element's tabindex to ensure correct order is followed when tabbing to the next element
        this.setAttribute('tabindex', tabindex);
        // set element's tabindex to -1 to allow for reverse tabbing
        that.element.setAttribute('tabindex', -1);
        that.selectpicker.view.tabindex = tabindex;
      }
    });

    this._on(this.button, 'blur', function (e) {
      // revert everything to original tabindex
      if (that.selectpicker.view.tabindex !== undefined && e.isTrusted) {
        that.element.setAttribute('tabindex', that.selectpicker.view.tabindex);
        this.setAttribute('tabindex', -1);
        that.selectpicker.view.tabindex = undefined;
      }
    });

    this._on(this.element, 'change', function () {
      that.render();
      that._emit('changed', changedArguments ? {
        clickedIndex: changedArguments[0],
        isSelected: changedArguments[1],
        previousValue: changedArguments[2]
      } : null);
      changedArguments = null;
    });

    this._on(this.element, 'focus', function () {
      if (!that.options.mobile) that.button.focus();
    });
  }

  onOptionClick (clickedAnchor, e, retainActive) {
    var that = this,
        element = that.element,
        li = clickedAnchor.parentElement,
        position0 = that.isVirtual() ? that.selectpicker.view.position0 : 0,
        clickedData = that.selectpicker.current.data[Array.prototype.indexOf.call(li.parentElement.children, li) + position0],
        clickedElement = clickedData.element,
        prevValue = getSelectValues.call(that),
        prevIndex = element.selectedIndex,
        prevOption = element.options[prevIndex],
        prevData = prevOption ? that.selectpicker.main.data[prevOption.liIndex] : false,
        triggerChange = true;

    // Don't close on multi choice menu
    if (that.multiple && that.options.maxOptions !== 1) {
      e.stopPropagation();
    }

    e.preventDefault();

    // Don't run if the select is disabled
    if (!that.isDisabled() && !li.classList.contains(classNames.DISABLED)) {
      var option = clickedData.option,
          state = option.selected,
          optgroupData = that.selectpicker.current.data.find(function (datum) {
            return datum.optID === clickedData.optID && datum.type === 'optgroup-label';
          }),
          optgroup = optgroupData ? optgroupData.optgroup : undefined,
          dataGetter = optgroup instanceof Element ? getOptionData.fromOption : getOptionData.fromDataSource,
          optgroupOptions = optgroup && optgroup.children,
          maxOptions = parseInt(that.options.maxOptions),
          maxOptionsGrp = optgroup && parseInt(dataGetter(optgroup, 'maxOptions')) || false;

      if (clickedElement === that.activeElement) retainActive = true;

      if (!retainActive) {
        that.prevActiveElement = that.activeElement;
        that.activeElement = undefined;
      }

      if (!that.multiple || maxOptions === 1) { // Deselect previous option if not multi select
        if (prevData) that.setSelected(prevData, false);
        that.setSelected(clickedData, true);
      } else { // Toggle the clicked option if multi select.
        that.setSelected(clickedData, !state);
        that.focusedParent.focus();

        if (maxOptions !== false || maxOptionsGrp !== false) {
          var maxReached = maxOptions < getSelectedOptions.call(that).length,
              selectedGroupOptions = 0;

          if (optgroup && optgroup.children) {
            for (var i = 0; i < optgroup.children.length; i++) {
              if (optgroup.children[i].selected) selectedGroupOptions++;
            }
          }

          var maxReachedGrp = maxOptionsGrp < selectedGroupOptions;

          if ((maxOptions && maxReached) || (maxOptionsGrp && maxReachedGrp)) {
            if (maxOptions && maxOptions === 1) {
              element.selectedIndex = -1;
              that.setOptionStatus(true);
            } else if (maxOptionsGrp && maxOptionsGrp === 1) {
              for (var j = 0; j < optgroupOptions.length; j++) {
                var _option = optgroupOptions[j];
                that.setSelected(that.selectpicker.current.data[_option.liIndex], false);
              }

              that.setSelected(clickedData, true);
            } else {
              var maxOptionsText = typeof that.options.maxOptionsText === 'string' ? [that.options.maxOptionsText, that.options.maxOptionsText] : that.options.maxOptionsText,
                  maxOptionsArr = typeof maxOptionsText === 'function' ? maxOptionsText(maxOptions, maxOptionsGrp) : maxOptionsText,
                  maxTxt = maxOptionsArr[0].replace('{n}', maxOptions),
                  maxTxtGrp = maxOptionsArr[1].replace('{n}', maxOptionsGrp),
                  notify = createFromHTML('<div class="notify"></div>');

              that.menu.appendChild(notify);

              if (maxOptions && maxReached) {
                notify.appendChild(createFromHTML('<div>' + maxTxt + '</div>'));
                triggerChange = false;
                that._emit('maxReached');
              }

              if (maxOptionsGrp && maxReachedGrp) {
                notify.appendChild(createFromHTML('<div>' + maxTxtGrp + '</div>'));
                triggerChange = false;
                that._emit('maxReachedGrp');
              }

              setTimeout(function () {
                that.setSelected(clickedData, false);
              }, 10);

              notify.classList.add('fadeOut');

              setTimeout(function () {
                notify.remove();
              }, 1050);
            }
          }
        }
      }

      if (that.options.source.data) that.element.appendChild(that.selectpicker.main.optionQueue);

      if (!that.multiple || (that.multiple && that.options.maxOptions === 1)) {
        that.button.focus();
      } else if (that.options.liveSearch) {
        that.searchbox.focus();
      }

      // Trigger select 'change'
      if (triggerChange) {
        if (that.multiple || prevIndex !== element.selectedIndex) {
          changedArguments = [option.index, option.selected, prevValue];
          triggerNative(that.element, 'change');
        }
      }
    }
  }

  liveSearchListener () {
    var that = this;

    this._on(this.searchbox, 'click', function (e) {
      e.stopPropagation();
    });
    this._on(this.searchbox, 'focus', function (e) {
      e.stopPropagation();
    });
    this._on(this.searchbox, 'touchend', function (e) {
      e.stopPropagation();
    });
    this._on(this.searchbox, 'keydown', function (e) {
      if (e.key === 'Enter' && that.createOptionButton && !that.createOptionButton.hidden && !that.selectpicker.current.data.length) {
        e.preventDefault();
        e.stopPropagation();
        that.createOpenOption(that.searchbox.value);
      }
    });

    this._on(this.searchbox, 'input', function () {
      var searchValue = that.searchbox.value;

      that.selectpicker.search.elements = [];
      that.selectpicker.search.data = [];

      if (searchValue) {
        that.selectpicker.search.previousValue = searchValue;

        if (that.options.source.search) {
          that.fetchData(function () {
            that.appendCreatedSearchResults(searchValue);
            that.render();
            that.buildList(undefined, true);
            that.noScroll = true;
            that.menuInner.scrollTop = 0;
            that.createView(true);
            showNoResults.call(that, that.selectpicker.search.data, searchValue);
          }, 'search', 0, searchValue);
        } else {
          var searchMatch = [],
              q = searchValue.toUpperCase(),
              cache = {},
              cacheArr = [],
              searchStyle = that._searchStyle(),
              normalizeSearch = that.options.liveSearchNormalize;

          if (normalizeSearch) q = normalizeToBase(q);

          for (var i = 0; i < that.selectpicker.main.data.length; i++) {
            var li = that.selectpicker.main.data[i];

            if (!cache[i]) {
              cache[i] = stringSearch(li, q, searchStyle, normalizeSearch);
            }

            if (cache[i] && li.headerIndex !== undefined && cacheArr.indexOf(li.headerIndex) === -1) {
              if (li.headerIndex > 0) {
                cache[li.headerIndex - 1] = true;
                cacheArr.push(li.headerIndex - 1);
              }

              cache[li.headerIndex] = true;
              cacheArr.push(li.headerIndex);

              cache[li.lastIndex + 1] = true;
            }

            if (cache[i] && li.type !== 'optgroup-label') cacheArr.push(i);
          }

          for (var j = 0, cacheLen = cacheArr.length; j < cacheLen; j++) {
            var index = cacheArr[j],
                prevIndex = cacheArr[j - 1],
                liData = that.selectpicker.main.data[index],
                liPrev = that.selectpicker.main.data[prevIndex];

            if (liData.type !== 'divider' || (liData.type === 'divider' && liPrev && liPrev.type !== 'divider' && cacheLen - 1 !== j)) {
              that.selectpicker.search.data.push(liData);
              searchMatch.push(that.selectpicker.main.elements[index]);
            }
          }

          that.activeElement = undefined;
          that.noScroll = true;
          that.menuInner.scrollTop = 0;
          that.selectpicker.search.elements = searchMatch;
          that.createView(true);
          showNoResults.call(that, searchMatch, searchValue);
        }
      } else if (that.selectpicker.search.previousValue) {
        that.menuInner.scrollTop = 0;
        that.createView(false);
      }

      that.syncOpenOptionButton();
    });
  }

  _searchStyle () {
    return this.options.liveSearchStyle || 'contains';
  }

  getValue () {
    var element = this.element;

    if (this.multiple) {
      var values = [];
      for (var i = 0; i < element.options.length; i++) {
        if (element.options[i].selected) values.push(element.options[i].value);
      }
      return values;
    }

    return element.value;
  }



/* eslint-disable no-undef */
// Shared ordered source fragment consumed by the Grunt JS build.
  val (value) {
    var element = this.element;

    if (typeof value !== 'undefined') {
      var selectedOptions = getSelectedOptions.call(this),
          prevValue = getSelectValues.call(this, selectedOptions);

      changedArguments = [null, null, prevValue];

      if (!Array.isArray(value)) value = [ value ];

      value.map(String);

      for (var i = 0; i < selectedOptions.length; i++) {
        var item = selectedOptions[i];

        if (item && value.indexOf(String(item.value)) === -1) {
          this.setSelected(item, false);
        }
      }

      // only update selected value if it matches an existing option
      this.selectpicker.main.data.filter(function (item) {
        if (value.indexOf(String(item.value)) !== -1) {
          this.setSelected(item, true);
          return true;
        }

        return false;
      }, this);

      if (this.options.source.data) element.appendChild(this.selectpicker.main.optionQueue);

      this._emit('changed', changedArguments ? {
        clickedIndex: changedArguments[0],
        isSelected: changedArguments[1],
        previousValue: changedArguments[2]
      } : null);

      if (this.newElement.classList.contains(classNames.SHOW)) {
        if (this.multiple) {
          this.setOptionStatus(true);
        } else {
          var liSelectedIndex = (element.options[element.selectedIndex] || {}).liIndex;

          if (typeof liSelectedIndex === 'number') {
            this.setSelected(this.selectpicker.current.data[liSelectedIndex], true);
          }
        }
      }

      this.render();

      changedArguments = null;

      return this.element;
    } else {
      return this.getValue();
    }
  }

  changeAll (status) {
    if (!this.multiple) return;
    if (typeof status === 'undefined') status = true;

    var element = this.element,
        previousSelected = 0,
        currentSelected = 0,
        prevValue = getSelectValues.call(this);

    element.classList.add('bs-select-hidden');

    for (var i = 0, data = this.selectpicker.current.data, len = data.length; i < len; i++) {
      var liData = data[i],
          option = liData.option;

      if (option && !liData.disabled && liData.type !== 'divider') {
        if (liData.selected) previousSelected++;
        option.selected = status;
        liData.selected = status;
        if (status === true) currentSelected++;
      }
    }

    element.classList.remove('bs-select-hidden');

    if (previousSelected === currentSelected) return;

    this.setOptionStatus();

    changedArguments = [null, null, prevValue];

    triggerNative(this.element, 'change');
  }

  selectAll () {
    return this.changeAll(true);
  }

  deselectAll () {
    return this.changeAll(false);
  }

  toggle (e, state) {
    var isActive,
        triggerToggle = state === undefined;

    if (e && e.stopPropagation) e.stopPropagation();

    if (triggerToggle === false) {
      isActive = this.newElement.classList.contains(classNames.SHOW);
      triggerToggle = (state === true && isActive === false) || (state === false && isActive === true);
    }

    if (triggerToggle) this.dropdown.toggle();
  }

  open (e) {
    this.toggle(e, true);
  }

  close (e) {
    this.toggle(e, false);
  }

  _keydown (e, el) {
    var that = this,
        which = e.which || e.keyCode,
        isToggle = el.classList.contains('dropdown-toggle'),
        items = that.findLis(),
        index,
        isActive,
        liActive,
        activeLi,
        offsetVal,
        updateScroll = false,
        downOnTab = which === keyCodes.TAB && !isToggle && !that.options.selectOnTab,
        isArrowKey = REGEXP_ARROW.test(which) || downOnTab,
        scrollTop = that.menuInner.scrollTop,
        isVirtual = that.isVirtual(),
        position0 = isVirtual === true ? that.selectpicker.view.position0 : 0;

    // do nothing if a function key is pressed
    if (which >= 112 && which <= 123) return;

    isActive = that.menu.classList.contains(classNames.SHOW);

    if (
      !isActive &&
        (
          isArrowKey ||
          (which >= 48 && which <= 57) ||
          (which >= 96 && which <= 105) ||
          (which >= 65 && which <= 90)
        )
    ) {
      that.dropdown.show();

      if (that.options.liveSearch) {
        that.searchbox.focus();
        return;
      }
    }

    if (which === keyCodes.ESCAPE && isActive) {
      e.preventDefault();
      that.dropdown.hide();
      that.button.focus();
    }

    if (isArrowKey) { // if up or down
      if (!items.length) return;

      liActive = that.activeElement;
      index = liActive ? Array.prototype.indexOf.call(liActive.parentElement.children, liActive) : -1;

      if (index !== -1) {
        that.defocusItem(liActive);
      }

      if (which === keyCodes.ARROW_UP) { // up
        if (index !== -1) index--;
        if (index + position0 < 0) index += items.length;

        if (!that.selectpicker.view.canHighlight[index + position0]) {
          index = that.selectpicker.view.canHighlight.slice(0, index + position0).lastIndexOf(true) - position0;
          if (index === -1) index = items.length - 1;
        }
      } else if (which === keyCodes.ARROW_DOWN || downOnTab) { // down
        index++;
        if (index + position0 >= that.selectpicker.view.canHighlight.length) index = that.selectpicker.view.firstHighlightIndex;

        if (!that.selectpicker.view.canHighlight[index + position0]) {
          index = index + 1 + that.selectpicker.view.canHighlight.slice(index + position0 + 1).indexOf(true);
        }
      }

      e.preventDefault();

      var liActiveIndex = position0 + index;

      if (which === keyCodes.ARROW_UP) { // up
        // scroll to bottom and highlight last option
        if (position0 === 0 && index === items.length - 1) {
          that.menuInner.scrollTop = that.menuInner.scrollHeight;

          liActiveIndex = that.selectpicker.current.elements.length - 1;
        } else {
          activeLi = that.selectpicker.current.data[liActiveIndex];

          // could be undefined if no results exist
          if (activeLi) {
            offsetVal = activeLi.position - activeLi.height;

            updateScroll = offsetVal < scrollTop;
          }
        }
      } else if (which === keyCodes.ARROW_DOWN || downOnTab) { // down
        // scroll to top and highlight first option
        if (index === that.selectpicker.view.firstHighlightIndex) {
          that.menuInner.scrollTop = 0;

          liActiveIndex = that.selectpicker.view.firstHighlightIndex;
        } else {
          activeLi = that.selectpicker.current.data[liActiveIndex];

          // could be undefined if no results exist
          if (activeLi) {
            offsetVal = activeLi.position - that.sizeInfo.menuInnerHeight;

            updateScroll = offsetVal > scrollTop;
          }
        }
      }

      liActive = that.selectpicker.current.elements[liActiveIndex];

      that.activeElement = (that.selectpicker.current.data[liActiveIndex] || {}).element;

      that.focusItem(liActive);

      that.selectpicker.view.currentActive = liActive;

      if (updateScroll) that.menuInner.scrollTop = offsetVal;

      if (that.options.liveSearch) {
        that.searchbox.focus();
      } else {
        el.focus();
      }
    } else if (
      (!el.matches('input') && !REGEXP_TAB_OR_ESCAPE.test(which)) ||
        (which === keyCodes.SPACE && that.selectpicker.keydown.keyHistory)
    ) {
      var matches = [],
          keyHistory;

      e.preventDefault();

      that.selectpicker.keydown.keyHistory += keyCodeMap[which];

      if (that.selectpicker.keydown.resetKeyHistory.cancel) clearTimeout(that.selectpicker.keydown.resetKeyHistory.cancel);
      that.selectpicker.keydown.resetKeyHistory.cancel = that.selectpicker.keydown.resetKeyHistory.start();

      keyHistory = that.selectpicker.keydown.keyHistory;

      // if all letters are the same, set keyHistory to just the first character when searching
      if (/^(.)\1+$/.test(keyHistory)) {
        keyHistory = keyHistory.charAt(0);
      }

      // find matches
      for (var i = 0; i < that.selectpicker.current.data.length; i++) {
        var li = that.selectpicker.current.data[i],
            hasMatch;

        hasMatch = stringSearch(li, keyHistory, 'startsWith', true);

        if (hasMatch && that.selectpicker.view.canHighlight[i]) {
          matches.push(li.element);
        }
      }

      if (matches.length) {
        var matchIndex = 0;

        Array.prototype.forEach.call(items, function (item) {
          item.classList.remove('active');
          if (item.firstChild) item.firstChild.classList.remove('active');
        });

        // either only one key has been pressed or they are all the same key
        if (keyHistory.length === 1) {
          matchIndex = matches.indexOf(that.activeElement);

          if (matchIndex === -1 || matchIndex === matches.length - 1) {
            matchIndex = 0;
          } else {
            matchIndex++;
          }
        }

        activeLi = that.selectpicker.main.data[that.selectpicker.main.elements.indexOf(matches[matchIndex])];

        if (activeLi) {
          if (scrollTop - activeLi.position > 0) {
            offsetVal = activeLi.position - activeLi.height;
            updateScroll = true;
          } else {
            offsetVal = activeLi.position - that.sizeInfo.menuInnerHeight;
            // if the option is already visible at the current scroll position, just keep it the same
            updateScroll = activeLi.position > scrollTop + that.sizeInfo.menuInnerHeight;
          }
        }

        liActive = matches[matchIndex];

        that.activeElement = liActive;

        that.focusItem(liActive);

        if (liActive) liActive.firstChild.focus();

        if (updateScroll) that.menuInner.scrollTop = offsetVal;

        el.focus();
      }
    }

    // Select focused option if "Enter", "Spacebar" or "Tab" (when selectOnTab is true) are pressed inside the menu.
    if (
      isActive &&
        (
          (which === keyCodes.SPACE && !that.selectpicker.keydown.keyHistory) ||
          which === keyCodes.ENTER ||
          (which === keyCodes.TAB && that.options.selectOnTab)
        )
    ) {
      if (which !== keyCodes.SPACE) e.preventDefault();

      if (!that.options.liveSearch || which !== keyCodes.SPACE) {
        var activeAnchor = that.menuInner.querySelector('.active a');
        if (activeAnchor) that.onOptionClick(activeAnchor, e, true); // retain active class
        el.focus();

        if (!that.options.liveSearch) {
          // Prevent screen from scrolling if the user hits the spacebar
          e.preventDefault();
          // Fixes spacebar selection of dropdown items in FF & IE
          spaceSelectFlag = true;
        }
      }
    }
  }

  mobile () {
    // ensure mobile is set to true if mobile function is called after init
    this.options.mobile = true;
    this.element.classList.add('mobile-device');
  }

  resetMenuData () {
    this.selectpicker.main.data = [];
    this.selectpicker.main.elements = [];
    this.selectpicker.main.hasMore = false;
    this.selectpicker.search.data = [];
    this.selectpicker.search.elements = [];
    this.selectpicker.search.hasMore = false;
    this.selectpicker.current.data = this.selectpicker.main.data;
    this.selectpicker.current.elements = this.selectpicker.main.elements;
    this.selectpicker.current.hasMore = false;
    this.selectpicker.isSearching = false;
  }

  refresh () {
    var that = this;
    // update options if data attributes have been changed
    var config = stripRemovedOptions(Object.assign({}, this.options, getAttributesObject(this.element), getDataset(this.element)));
    this.options = config;

    if (this.options.source.data) {
      this.render();
      this.buildList();
    } else {
      this.resetMenuData();
      this.fetchData(function () {
        that.render();
        that.buildList();
      });
    }

    this.checkDisabled();
    this.setStyle();
    this.setWidth();

    this.setSize(true);

    this._emit('refreshed');
  }

  hide () {
    this.newElement.style.display = 'none';
  }

  show () {
    this.newElement.style.display = '';
  }

  remove () {
    if (this.newElement.parentNode) this.newElement.parentNode.removeChild(this.newElement);
    instanceMap.delete(this.element);
  }

  destroy () {
    // move the select back out of newElement, then remove newElement
    if (this.newElement.parentNode) {
      this.newElement.parentNode.insertBefore(this.element, this.newElement);
      this.newElement.parentNode.removeChild(this.newElement);
    }

    if (this.bsContainer) {
      if (this.bsContainer.parentNode) this.bsContainer.parentNode.removeChild(this.bsContainer);
    } else if (this.menu && this.menu.parentNode) {
      this.menu.parentNode.removeChild(this.menu);
    }

    if (this.selectpicker.view.titleOption && this.selectpicker.view.titleOption.parentNode) {
      this.selectpicker.view.titleOption.parentNode.removeChild(this.selectpicker.view.titleOption);
    }

    // remove all tracked event listeners
    for (var i = 0; i < this._listeners.length; i++) {
      var l = this._listeners[i];
      l.el.removeEventListener(l.type, l.handler, l.options);
    }
    this._listeners = [];

    for (var key in this._named) {
      if (Object.prototype.hasOwnProperty.call(this._named, key)) {
        this._removeNamed(key);
      }
    }

    if (this.dropdown && typeof this.dropdown.dispose === 'function') {
      this.dropdown.dispose();
    }

    this.element.classList.remove('bs-select-hidden', 'selectpicker', 'mobile-device');

    instanceMap.delete(this.element);
  }
}

// stores element -> Selectpicker instance
var instanceMap = new WeakMap();

Selectpicker.NAME = 'selectpicker';
Selectpicker.VERSION = '1.2.0';

// user-provided global defaults (set via Selectpicker.setDefaults, used by i18n files)
Selectpicker.defaults = null;

// part of this is duplicated in i18n/defaults-en_US.js. Make sure to update both.
Selectpicker.DEFAULTS = {
  noneSelectedText: 'Nothing selected',
  noneResultsText: 'No results matched {0}',
  countSelectedText: function (numSelected) {
    return (numSelected == 1) ? '{0} item selected' : '{0} items selected';
  },
  maxOptionsText: function (numAll, numGroup) {
    return [
      (numAll == 1) ? 'Limit reached ({n} item max)' : 'Limit reached ({n} items max)',
      (numGroup == 1) ? 'Group limit reached ({n} item max)' : 'Group limit reached ({n} items max)'
    ];
  },
  selectAllText: 'Select All',
  deselectAllText: 'Deselect All',
  source: {
    pageSize: 40,
    create: null
  },
  chunkSize: 40,
  doneButton: false,
  doneButtonText: 'Close',
  multipleSeparator: ', ',
  style: classNames.BUTTONCLASS,
  size: 'auto',
  placeholder: null,
  allowClear: false,
  selectedTextFormat: 'values',
  width: false,
  hideDisabled: false,
  showSubtext: false,
  showIcon: true,
  showContent: true,
  dropupAuto: true,
  header: false,
  liveSearch: false,
  liveSearchPlaceholder: null,
  liveSearchNormalize: false,
  liveSearchStyle: 'contains',
  openOptions: false,
  openOptionsText: 'Create "{0}"',
  selectionIndicator: 'checkmark',
  actionsBox: false,
  iconBase: classNames.ICONBASE,
  tickIcon: classNames.TICKICON,
  showTick: false,
  showSelectedTags: false,
  selectedItemsStyle: 'tags',
  selectedTagRemoveLabel: 'Remove',
  template: {
    caret: '<span class="caret"></span>'
  },
  maxOptions: false,
  selectOnTab: true,
  dropdownAlignRight: false,
  virtualScroll: 600,
  sanitize: true,
  sanitizeFn: null,
  whiteList: DefaultWhitelist
};

Selectpicker._buildConfig = function (element, options) {
  options = stripRemovedOptions(options || {});

  var dataAttributes = stripRemovedOptions(getDataset(element));

  for (var dataAttr in dataAttributes) {
    if (Object.prototype.hasOwnProperty.call(dataAttributes, dataAttr) && DISALLOWED_ATTRIBUTES.indexOf(dataAttr) !== -1) {
      delete dataAttributes[dataAttr];
    }
  }

  var userDefaults = stripRemovedOptions(Selectpicker.defaults || {});

  var config = Object.assign({}, Selectpicker.DEFAULTS, userDefaults, getAttributesObject(element), dataAttributes, options);
  config.template = Object.assign({}, Selectpicker.DEFAULTS.template, userDefaults.template || {}, dataAttributes.template, options.template);
  config.source = Object.assign({}, Selectpicker.DEFAULTS.source, userDefaults.source || {}, options.source);

  return applyLegacyOptions(element, config);
};

Selectpicker.setDefaults = function (newDefaults) {
  Selectpicker.defaults = stripRemovedOptions(Object.assign({}, Selectpicker.defaults, newDefaults));
};

Selectpicker.getInstance = function (element) {
  if (typeof element === 'string') element = document.querySelector(element);
  return instanceMap.get(element) || null;
};

Selectpicker.getOrCreateInstance = function (element, options) {
  if (typeof element === 'string') element = document.querySelector(element);
  if (!element || element.tagName !== 'SELECT') return null;

  var instance = instanceMap.get(element);

  if (instance) {
    options = stripRemovedOptions(options);

    if (options && typeof options === 'object') {
      for (var i in options) {
        if (Object.prototype.hasOwnProperty.call(options, i)) {
          instance.options[i] = options[i];
        }
      }
    }

    return instance;
  }

  return new Selectpicker(element, typeof options === 'object' ? options : {});
};

// Runtime wiring lives in js/bootstrap-select.runtime.js so each distribution
// can choose whether it should expose a browser global or stay module-scoped.


/* global Selector, __SELECTPICKER_EXPOSE_GLOBAL__ */

// <editor-fold desc="Keyboard handling and auto-initialization">
var KEYDOWN_SELECTOR = '.bootstrap-select [' + Selector.DATA_TOGGLE + '], .bootstrap-select [role="listbox"], .bootstrap-select .bs-searchbox input';

function initSelectpickerRuntime (Selectpicker, exposeGlobal) {
  if (typeof window === 'undefined' || typeof document === 'undefined') return;

  // Handle keyboard navigation ourselves. This listener runs in the capture
  // phase on `window` so it executes before Bootstrap's `document`-level
  // (capture-phase, delegated) dropdown keydown handler and prevents it from
  // processing bootstrap-select's custom menu (which would otherwise error on
  // relocated/container menus and conflict with our own navigation). This
  // replaces the upstream approach of unbinding Bootstrap's global handler.
  window.addEventListener('keydown', function (e) {
    var target = e.target;
    if (!target || !target.closest) return;

    // Any keydown originating inside a bootstrap-select widget (or its
    // relocated menu container) must not reach Bootstrap's dropdown keydown
    // handler.
    var widget = target.closest('.bootstrap-select, .bs-container');
    if (!widget) return;

    e.stopImmediatePropagation();

    var trigger = target.closest(KEYDOWN_SELECTOR);
    if (!trigger) return;

    var instance;
    for (var node = trigger; node; node = node.parentElement) {
      if (node.bootstrapSelectInstance) {
        instance = node.bootstrapSelectInstance;
        break;
      }
    }

    if (instance) instance._keydown(e, trigger);
  }, true);

  document.addEventListener('focusin', function (e) {
    var target = e.target;
    if (target && target.closest && target.closest(KEYDOWN_SELECTOR)) {
      e.stopPropagation();
    }
  });

  function initAll () {
    var selects = document.querySelectorAll('.selectpicker');
    Array.prototype.forEach.call(selects, function (select) {
      Selectpicker.getOrCreateInstance(select);
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initAll);
  } else {
    initAll();
  }

  if (exposeGlobal) {
    window.Selectpicker = Selectpicker;
  }
}

initSelectpickerRuntime(
  Selectpicker,
  typeof __SELECTPICKER_EXPOSE_GLOBAL__ === 'undefined' ? true : __SELECTPICKER_EXPOSE_GLOBAL__
);
// </editor-fold>

return Selectpicker;


}));
//# sourceMappingURL=bootstrap-select.js.map
