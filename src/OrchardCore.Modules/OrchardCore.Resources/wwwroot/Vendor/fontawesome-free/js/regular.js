/*!
 * Font Awesome Free 7.0.0 by @fontawesome - https://fontawesome.com
 * License - https://fontawesome.com/license/free (Icons: CC BY 4.0, Fonts: SIL OFL 1.1, Code: MIT License)
 * Copyright 2025 Fonticons, Inc.
 */
(function () {
  'use strict';

  var _WINDOW = {};
  var _DOCUMENT = {};
  try {
    if (typeof window !== 'undefined') _WINDOW = window;
    if (typeof document !== 'undefined') _DOCUMENT = document;
  } catch (e) {} // eslint-disable-line no-empty

  var _ref = _WINDOW.navigator || {},
    _ref$userAgent = _ref.userAgent,
    userAgent = _ref$userAgent === void 0 ? '' : _ref$userAgent;
  var WINDOW = _WINDOW;
  var DOCUMENT = _DOCUMENT;
  var IS_BROWSER = !!WINDOW.document;
  var IS_DOM = !!DOCUMENT.documentElement && !!DOCUMENT.head && typeof DOCUMENT.addEventListener === 'function' && typeof DOCUMENT.createElement === 'function';
  var IS_IE = ~userAgent.indexOf('MSIE') || ~userAgent.indexOf('Trident/');

  function _arrayLikeToArray(r, a) {
    (null == a || a > r.length) && (a = r.length);
    for (var e = 0, n = Array(a); e < a; e++) n[e] = r[e];
    return n;
  }
  function _arrayWithoutHoles(r) {
    if (Array.isArray(r)) return _arrayLikeToArray(r);
  }
  function _defineProperty(e, r, t) {
    return (r = _toPropertyKey(r)) in e ? Object.defineProperty(e, r, {
      value: t,
      enumerable: !0,
      configurable: !0,
      writable: !0
    }) : e[r] = t, e;
  }
  function _iterableToArray(r) {
    if ("undefined" != typeof Symbol && null != r[Symbol.iterator] || null != r["@@iterator"]) return Array.from(r);
  }
  function _nonIterableSpread() {
    throw new TypeError("Invalid attempt to spread non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method.");
  }
  function ownKeys(e, r) {
    var t = Object.keys(e);
    if (Object.getOwnPropertySymbols) {
      var o = Object.getOwnPropertySymbols(e);
      r && (o = o.filter(function (r) {
        return Object.getOwnPropertyDescriptor(e, r).enumerable;
      })), t.push.apply(t, o);
    }
    return t;
  }
  function _objectSpread2(e) {
    for (var r = 1; r < arguments.length; r++) {
      var t = null != arguments[r] ? arguments[r] : {};
      r % 2 ? ownKeys(Object(t), !0).forEach(function (r) {
        _defineProperty(e, r, t[r]);
      }) : Object.getOwnPropertyDescriptors ? Object.defineProperties(e, Object.getOwnPropertyDescriptors(t)) : ownKeys(Object(t)).forEach(function (r) {
        Object.defineProperty(e, r, Object.getOwnPropertyDescriptor(t, r));
      });
    }
    return e;
  }
  function _toConsumableArray(r) {
    return _arrayWithoutHoles(r) || _iterableToArray(r) || _unsupportedIterableToArray(r) || _nonIterableSpread();
  }
  function _toPrimitive(t, r) {
    if ("object" != typeof t || !t) return t;
    var e = t[Symbol.toPrimitive];
    if (void 0 !== e) {
      var i = e.call(t, r || "default");
      if ("object" != typeof i) return i;
      throw new TypeError("@@toPrimitive must return a primitive value.");
    }
    return ("string" === r ? String : Number)(t);
  }
  function _toPropertyKey(t) {
    var i = _toPrimitive(t, "string");
    return "symbol" == typeof i ? i : i + "";
  }
  function _unsupportedIterableToArray(r, a) {
    if (r) {
      if ("string" == typeof r) return _arrayLikeToArray(r, a);
      var t = {}.toString.call(r).slice(8, -1);
      return "Object" === t && r.constructor && (t = r.constructor.name), "Map" === t || "Set" === t ? Array.from(r) : "Arguments" === t || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(t) ? _arrayLikeToArray(r, a) : void 0;
    }
  }

  var _so;
  var z = {
      classic: {
        fa: "solid",
        fas: "solid",
        "fa-solid": "solid",
        far: "regular",
        "fa-regular": "regular",
        fal: "light",
        "fa-light": "light",
        fat: "thin",
        "fa-thin": "thin",
        fab: "brands",
        "fa-brands": "brands"
      },
      duotone: {
        fa: "solid",
        fad: "solid",
        "fa-solid": "solid",
        "fa-duotone": "solid",
        fadr: "regular",
        "fa-regular": "regular",
        fadl: "light",
        "fa-light": "light",
        fadt: "thin",
        "fa-thin": "thin"
      },
      sharp: {
        fa: "solid",
        fass: "solid",
        "fa-solid": "solid",
        fasr: "regular",
        "fa-regular": "regular",
        fasl: "light",
        "fa-light": "light",
        fast: "thin",
        "fa-thin": "thin"
      },
      "sharp-duotone": {
        fa: "solid",
        fasds: "solid",
        "fa-solid": "solid",
        fasdr: "regular",
        "fa-regular": "regular",
        fasdl: "light",
        "fa-light": "light",
        fasdt: "thin",
        "fa-thin": "thin"
      },
      slab: {
        "fa-regular": "regular",
        faslr: "regular"
      },
      "slab-press": {
        "fa-regular": "regular",
        faslpr: "regular"
      },
      thumbprint: {
        "fa-light": "light",
        fatl: "light"
      },
      whiteboard: {
        "fa-semibold": "semibold",
        fawsb: "semibold"
      },
      notdog: {
        "fa-solid": "solid",
        fans: "solid"
      },
      "notdog-duo": {
        "fa-solid": "solid",
        fands: "solid"
      },
      etch: {
        "fa-solid": "solid",
        faes: "solid"
      },
      jelly: {
        "fa-regular": "regular",
        fajr: "regular"
      },
      "jelly-fill": {
        "fa-regular": "regular",
        fajfr: "regular"
      },
      "jelly-duo": {
        "fa-regular": "regular",
        fajdr: "regular"
      },
      chisel: {
        "fa-regular": "regular",
        facr: "regular"
      }
    };
  var a = "classic",
    o = "duotone",
    d = "sharp",
    t = "sharp-duotone",
    i = "chisel",
    n = "etch",
    h = "jelly",
    s = "jelly-duo",
    f = "jelly-fill",
    g = "notdog",
    l = "notdog-duo",
    u = "slab",
    p = "slab-press",
    e = "thumbprint",
    w = "whiteboard",
    m = "Classic",
    y = "Duotone",
    x = "Sharp",
    c = "Sharp Duotone",
    I = "Chisel",
    b = "Etch",
    F = "Jelly",
    v = "Jelly Duo",
    S = "Jelly Fill",
    A = "Notdog",
    P = "Notdog Duo",
    j = "Slab",
    B = "Slab Press",
    N = "Thumbprint",
    k = "Whiteboard",
    so = (_so = {}, _defineProperty(_defineProperty(_defineProperty(_defineProperty(_defineProperty(_defineProperty(_defineProperty(_defineProperty(_defineProperty(_defineProperty(_so, a, m), o, y), d, x), t, c), i, I), n, b), h, F), s, v), f, S), g, A), _defineProperty(_defineProperty(_defineProperty(_defineProperty(_defineProperty(_so, l, P), u, j), p, B), e, N), w, k));
  var io = {
      classic: {
        900: "fas",
        400: "far",
        normal: "far",
        300: "fal",
        100: "fat"
      },
      duotone: {
        900: "fad",
        400: "fadr",
        300: "fadl",
        100: "fadt"
      },
      sharp: {
        900: "fass",
        400: "fasr",
        300: "fasl",
        100: "fast"
      },
      "sharp-duotone": {
        900: "fasds",
        400: "fasdr",
        300: "fasdl",
        100: "fasdt"
      },
      slab: {
        400: "faslr"
      },
      "slab-press": {
        400: "faslpr"
      },
      whiteboard: {
        600: "fawsb"
      },
      thumbprint: {
        300: "fatl"
      },
      notdog: {
        900: "fans"
      },
      "notdog-duo": {
        900: "fands"
      },
      etch: {
        900: "faes"
      },
      chisel: {
        400: "facr"
      },
      jelly: {
        400: "fajr"
      },
      "jelly-fill": {
        400: "fajfr"
      },
      "jelly-duo": {
        400: "fajdr"
      }
    };
  var Ro = {
      chisel: {
        regular: "facr"
      },
      classic: {
        brands: "fab",
        light: "fal",
        regular: "far",
        solid: "fas",
        thin: "fat"
      },
      duotone: {
        light: "fadl",
        regular: "fadr",
        solid: "fad",
        thin: "fadt"
      },
      etch: {
        solid: "faes"
      },
      jelly: {
        regular: "fajr"
      },
      "jelly-duo": {
        regular: "fajdr"
      },
      "jelly-fill": {
        regular: "fajfr"
      },
      notdog: {
        solid: "fans"
      },
      "notdog-duo": {
        solid: "fands"
      },
      sharp: {
        light: "fasl",
        regular: "fasr",
        solid: "fass",
        thin: "fast"
      },
      "sharp-duotone": {
        light: "fasdl",
        regular: "fasdr",
        solid: "fasds",
        thin: "fasdt"
      },
      slab: {
        regular: "faslr"
      },
      "slab-press": {
        regular: "faslpr"
      },
      thumbprint: {
        light: "fatl"
      },
      whiteboard: {
        semibold: "fawsb"
      }
    };
  var Oo = {
      kit: {
        fak: "kit",
        "fa-kit": "kit"
      },
      "kit-duotone": {
        fakd: "kit-duotone",
        "fa-kit-duotone": "kit-duotone"
      }
    },
    Go = ["kit"];
  var D = "kit",
    r = "kit-duotone",
    T = "Kit",
    C = "Kit Duotone",
    qo = _defineProperty(_defineProperty({}, D, T), r, C);
  var Xo = {
    kit: {
      "fa-kit": "fak"
    },
    "kit-duotone": {
      "fa-kit-duotone": "fakd"
    }
  };
  var et = {
      kit: {
        fak: "fa-kit"
      },
      "kit-duotone": {
        fakd: "fa-kit-duotone"
      }
    };
  var dt = {
      kit: {
        kit: "fak"
      },
      "kit-duotone": {
        "kit-duotone": "fakd"
      }
    };

  var _fl;
  var l$1 = {
      GROUP: "duotone-group",
      SWAP_OPACITY: "swap-opacity",
      PRIMARY: "primary",
      SECONDARY: "secondary"
    };
  var f$1 = "classic",
    a$1 = "duotone",
    n$1 = "sharp",
    t$1 = "sharp-duotone",
    h$1 = "chisel",
    g$1 = "etch",
    u$1 = "jelly",
    s$1 = "jelly-duo",
    p$1 = "jelly-fill",
    y$1 = "notdog",
    e$1 = "notdog-duo",
    m$1 = "slab",
    c$1 = "slab-press",
    r$1 = "thumbprint",
    w$1 = "whiteboard",
    x$1 = "Classic",
    I$1 = "Duotone",
    b$1 = "Sharp",
    F$1 = "Sharp Duotone",
    v$1 = "Chisel",
    S$1 = "Etch",
    A$1 = "Jelly",
    j$1 = "Jelly Duo",
    P$1 = "Jelly Fill",
    B$1 = "Notdog",
    k$1 = "Notdog Duo",
    N$1 = "Slab",
    D$1 = "Slab Press",
    C$1 = "Thumbprint",
    T$1 = "Whiteboard",
    fl = (_fl = {}, _defineProperty(_defineProperty(_defineProperty(_defineProperty(_defineProperty(_defineProperty(_defineProperty(_defineProperty(_defineProperty(_defineProperty(_fl, f$1, x$1), a$1, I$1), n$1, b$1), t$1, F$1), h$1, v$1), g$1, S$1), u$1, A$1), s$1, j$1), p$1, P$1), y$1, B$1), _defineProperty(_defineProperty(_defineProperty(_defineProperty(_defineProperty(_fl, e$1, k$1), m$1, N$1), c$1, D$1), r$1, C$1), w$1, T$1));
  var L = "kit",
    d$1 = "kit-duotone",
    R$1 = "Kit",
    W$1 = "Kit Duotone",
    lo$1 = _defineProperty(_defineProperty({}, L, R$1), d$1, W$1);
  var zo$1 = {
      classic: {
        "fa-brands": "fab",
        "fa-duotone": "fad",
        "fa-light": "fal",
        "fa-regular": "far",
        "fa-solid": "fas",
        "fa-thin": "fat"
      },
      duotone: {
        "fa-regular": "fadr",
        "fa-light": "fadl",
        "fa-thin": "fadt"
      },
      sharp: {
        "fa-solid": "fass",
        "fa-regular": "fasr",
        "fa-light": "fasl",
        "fa-thin": "fast"
      },
      "sharp-duotone": {
        "fa-solid": "fasds",
        "fa-regular": "fasdr",
        "fa-light": "fasdl",
        "fa-thin": "fasdt"
      },
      slab: {
        "fa-regular": "faslr"
      },
      "slab-press": {
        "fa-regular": "faslpr"
      },
      whiteboard: {
        "fa-semibold": "fawsb"
      },
      thumbprint: {
        "fa-light": "fatl"
      },
      notdog: {
        "fa-solid": "fans"
      },
      "notdog-duo": {
        "fa-solid": "fands"
      },
      etch: {
        "fa-solid": "faes"
      },
      jelly: {
        "fa-regular": "fajr"
      },
      "jelly-fill": {
        "fa-regular": "fajfr"
      },
      "jelly-duo": {
        "fa-regular": "fajdr"
      },
      chisel: {
        "fa-regular": "facr"
      }
    },
    J$1 = {
      classic: ["fas", "far", "fal", "fat", "fad"],
      duotone: ["fadr", "fadl", "fadt"],
      sharp: ["fass", "fasr", "fasl", "fast"],
      "sharp-duotone": ["fasds", "fasdr", "fasdl", "fasdt"],
      slab: ["faslr"],
      "slab-press": ["faslpr"],
      whiteboard: ["fawsb"],
      thumbprint: ["fatl"],
      notdog: ["fans"],
      "notdog-duo": ["fands"],
      etch: ["faes"],
      jelly: ["fajr"],
      "jelly-fill": ["fajfr"],
      "jelly-duo": ["fajdr"],
      chisel: ["facr"]
    },
    Go$1 = {
      classic: {
        fab: "fa-brands",
        fad: "fa-duotone",
        fal: "fa-light",
        far: "fa-regular",
        fas: "fa-solid",
        fat: "fa-thin"
      },
      duotone: {
        fadr: "fa-regular",
        fadl: "fa-light",
        fadt: "fa-thin"
      },
      sharp: {
        fass: "fa-solid",
        fasr: "fa-regular",
        fasl: "fa-light",
        fast: "fa-thin"
      },
      "sharp-duotone": {
        fasds: "fa-solid",
        fasdr: "fa-regular",
        fasdl: "fa-light",
        fasdt: "fa-thin"
      },
      slab: {
        faslr: "fa-regular"
      },
      "slab-press": {
        faslpr: "fa-regular"
      },
      whiteboard: {
        fawsb: "fa-semibold"
      },
      thumbprint: {
        fatl: "fa-light"
      },
      notdog: {
        fans: "fa-solid"
      },
      "notdog-duo": {
        fands: "fa-solid"
      },
      etch: {
        faes: "fa-solid"
      },
      jelly: {
        fajr: "fa-regular"
      },
      "jelly-fill": {
        fajfr: "fa-regular"
      },
      "jelly-duo": {
        fajdr: "fa-regular"
      },
      chisel: {
        facr: "fa-regular"
      }
    },
    _$1 = ["solid", "regular", "light", "thin", "duotone", "brands", "semibold"],
    K$1 = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
    M$1 = K$1.concat([11, 12, 13, 14, 15, 16, 17, 18, 19, 20]),
    O$1 = ["aw", "fw", "pull-left", "pull-right"],
    Ho$1 = [].concat(_toConsumableArray(Object.keys(J$1)), _$1, O$1, ["2xs", "xs", "sm", "lg", "xl", "2xl", "beat", "border", "fade", "beat-fade", "bounce", "flip-both", "flip-horizontal", "flip-vertical", "flip", "inverse", "layers", "layers-bottom-left", "layers-bottom-right", "layers-counter", "layers-text", "layers-top-left", "layers-top-right", "li", "pull-end", "pull-start", "pulse", "rotate-180", "rotate-270", "rotate-90", "rotate-by", "shake", "spin-pulse", "spin-reverse", "spin", "stack-1x", "stack-2x", "stack", "ul", "width-auto", "width-fixed", l$1.GROUP, l$1.SWAP_OPACITY, l$1.PRIMARY, l$1.SECONDARY]).concat(K$1.map(function (o) {
      return "".concat(o, "x");
    })).concat(M$1.map(function (o) {
      return "w-".concat(o);
    }));

  var NAMESPACE_IDENTIFIER = '___FONT_AWESOME___';
  var PRODUCTION = function () {
    try {
      return "production" === 'production';
    } catch (e$$1) {
      return false;
    }
  }();
  function familyProxy(obj) {
    // Defaults to the classic family if family is not available
    return new Proxy(obj, {
      get: function get(target, prop) {
        return prop in target ? target[prop] : target[a];
      }
    });
  }
  var _PREFIX_TO_STYLE = _objectSpread2({}, z);

  // We changed FACSSClassesToStyleId in the icons repo to be canonical and as such, "classic" family does not have any
  // duotone styles.  But we do still need duotone in _PREFIX_TO_STYLE below, so we are manually adding
  // {'fa-duotone': 'duotone'}
  _PREFIX_TO_STYLE[a] = _objectSpread2(_objectSpread2(_objectSpread2(_objectSpread2({}, {
    'fa-duotone': 'duotone'
  }), z[a]), Oo['kit']), Oo['kit-duotone']);
  var PREFIX_TO_STYLE = familyProxy(_PREFIX_TO_STYLE);
  var _STYLE_TO_PREFIX = _objectSpread2({}, Ro);

  // We changed FAStyleIdToShortPrefixId in the icons repo to be canonical and as such, "classic" family does not have any
  // duotone styles.  But we do still need duotone in _STYLE_TO_PREFIX below, so we are manually adding {duotone: 'fad'}
  _STYLE_TO_PREFIX[a] = _objectSpread2(_objectSpread2(_objectSpread2(_objectSpread2({}, {
    duotone: 'fad'
  }), _STYLE_TO_PREFIX[a]), dt['kit']), dt['kit-duotone']);
  var STYLE_TO_PREFIX = familyProxy(_STYLE_TO_PREFIX);
  var _PREFIX_TO_LONG_STYLE = _objectSpread2({}, Go$1);
  _PREFIX_TO_LONG_STYLE[a] = _objectSpread2(_objectSpread2({}, _PREFIX_TO_LONG_STYLE[a]), et['kit']);
  var PREFIX_TO_LONG_STYLE = familyProxy(_PREFIX_TO_LONG_STYLE);
  var _LONG_STYLE_TO_PREFIX = _objectSpread2({}, zo$1);
  _LONG_STYLE_TO_PREFIX[a] = _objectSpread2(_objectSpread2({}, _LONG_STYLE_TO_PREFIX[a]), Xo['kit']);
  var LONG_STYLE_TO_PREFIX = familyProxy(_LONG_STYLE_TO_PREFIX);
  var _FONT_WEIGHT_TO_PREFIX = _objectSpread2({}, io);
  var FONT_WEIGHT_TO_PREFIX = familyProxy(_FONT_WEIGHT_TO_PREFIX);
  var RESERVED_CLASSES = [].concat(_toConsumableArray(Go), _toConsumableArray(Ho$1));

  function bunker(fn) {
    try {
      for (var _len = arguments.length, args = new Array(_len > 1 ? _len - 1 : 0), _key = 1; _key < _len; _key++) {
        args[_key - 1] = arguments[_key];
      }
      fn.apply(void 0, args);
    } catch (e) {
      if (!PRODUCTION) {
        throw e;
      }
    }
  }

  var w$2 = WINDOW || {};
  if (!w$2[NAMESPACE_IDENTIFIER]) w$2[NAMESPACE_IDENTIFIER] = {};
  if (!w$2[NAMESPACE_IDENTIFIER].styles) w$2[NAMESPACE_IDENTIFIER].styles = {};
  if (!w$2[NAMESPACE_IDENTIFIER].hooks) w$2[NAMESPACE_IDENTIFIER].hooks = {};
  if (!w$2[NAMESPACE_IDENTIFIER].shims) w$2[NAMESPACE_IDENTIFIER].shims = [];
  var namespace = w$2[NAMESPACE_IDENTIFIER];

  function normalizeIcons(icons) {
    return Object.keys(icons).reduce(function (acc, iconName) {
      var icon = icons[iconName];
      var expanded = !!icon.icon;
      if (expanded) {
        acc[icon.iconName] = icon.icon;
      } else {
        acc[iconName] = icon;
      }
      return acc;
    }, {});
  }
  function defineIcons(prefix, icons) {
    var params = arguments.length > 2 && arguments[2] !== undefined ? arguments[2] : {};
    var _params$skipHooks = params.skipHooks,
      skipHooks = _params$skipHooks === void 0 ? false : _params$skipHooks;
    var normalized = normalizeIcons(icons);
    if (typeof namespace.hooks.addPack === 'function' && !skipHooks) {
      namespace.hooks.addPack(prefix, normalizeIcons(icons));
    } else {
      namespace.styles[prefix] = _objectSpread2(_objectSpread2({}, namespace.styles[prefix] || {}), normalized);
    }

    /**
     * Font Awesome 4 used the prefix of `fa` for all icons. With the introduction
     * of new styles we needed to differentiate between them. Prefix `fa` is now an alias
     * for `fas` so we'll ease the upgrade process for our users by automatically defining
     * this as well.
     */
    if (prefix === 'fas') {
      defineIcons('fa', icons);
    }
  }

  var icons = {
    "square-minus": [448, 512, [61767, "minus-square"], "f146", "M64 80c-8.8 0-16 7.2-16 16l0 320c0 8.8 7.2 16 16 16l320 0c8.8 0 16-7.2 16-16l0-320c0-8.8-7.2-16-16-16L64 80zM0 96C0 60.7 28.7 32 64 32l320 0c35.3 0 64 28.7 64 64l0 320c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 96zM136 232l176 0c13.3 0 24 10.7 24 24s-10.7 24-24 24l-176 0c-13.3 0-24-10.7-24-24s10.7-24 24-24z"],
    "calendar-check": [448, 512, [], "f274", "M120 0c13.3 0 24 10.7 24 24l0 40 160 0 0-40c0-13.3 10.7-24 24-24s24 10.7 24 24l0 40 32 0c35.3 0 64 28.7 64 64l0 288c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 128C0 92.7 28.7 64 64 64l32 0 0-40c0-13.3 10.7-24 24-24zm0 112l-56 0c-8.8 0-16 7.2-16 16l0 288c0 8.8 7.2 16 16 16l320 0c8.8 0 16-7.2 16-16l0-288c0-8.8-7.2-16-16-16l-264 0zM308.4 228.7l-80 128c-4.2 6.7-11.4 10.9-19.3 11.3s-15.5-3.2-20.2-9.6l-48-64c-8-10.6-5.8-25.6 4.8-33.6s25.6-5.8 33.6 4.8l27 36 61.4-98.3c7-11.2 21.8-14.7 33.1-7.6s14.7 21.8 7.6 33.1z"],
    "face-kiss": [512, 512, [128535, "kiss"], "f596", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm240 0l32 0c26.5 0 48 21.5 48 48 0 12.3-4.6 23.5-12.2 32 7.6 8.5 12.2 19.7 12.2 32 0 26.5-21.5 48-48 48l-32 0c-8.8 0-16-7.2-16-16s7.2-16 16-16l16 0c8.8 0 16-7.2 16-16s-7.2-16-16-16l-16 0c-8.8 0-16-7.2-16-16s7.2-16 16-16l16 0c8.8 0 16-7.2 16-16s-7.2-16-16-16l-16 0c-8.8 0-16-7.2-16-16s7.2-16 16-16zm-96-48a32 32 0 1 1 64 0 32 32 0 1 1 -64 0zm192-32a32 32 0 1 1 0 64 32 32 0 1 1 0-64z"],
    "paste": [512, 512, ["file-clipboard"], "f0ea", "M64 48l224 0c8.8 0 16 7.2 16 16l0 48 48 0 0-48c0-35.3-28.7-64-64-64L64 0C28.7 0 0 28.7 0 64L0 384c0 35.3 28.7 64 64 64l112 0 0-48-112 0c-8.8 0-16-7.2-16-16L48 64c0-8.8 7.2-16 16-16zm176 72c0-13.3-10.7-24-24-24L104 96c-13.3 0-24 10.7-24 24s10.7 24 24 24l105.6 0c8.8-8.6 19-15.8 30.2-21.1 .1-.9 .2-1.9 .2-2.9zM448 464l-160 0c-8.8 0-16-7.2-16-16l0-224c0-8.8 7.2-16 16-16l101.5 0c4.2 0 8.3 1.7 11.3 4.7l58.5 58.5c3 3 4.7 7.1 4.7 11.3L464 448c0 8.8-7.2 16-16 16zM224 224l0 224c0 35.3 28.7 64 64 64l160 0c35.3 0 64-28.7 64-64l0-165.5c0-17-6.7-33.3-18.7-45.3l-58.5-58.5c-12-12-28.3-18.7-45.3-18.7L288 160c-35.3 0-64 28.7-64 64z"],
    "hand-point-left": [512, 512, [], "f0a5", "M64 128l177.6 0c-1 5.2-1.6 10.5-1.6 16l0 16-176 0c-8.8 0-16-7.2-16-16s7.2-16 16-16zm224 16c0-17.7 14.3-32 32-32l24 0c66.3 0 120 53.7 120 120l0 48c0 52.5-33.7 97.1-80.7 113.4 .5-3.1 .7-6.2 .7-9.4 0-20-9.2-37.9-23.6-49.7 4.9-9 7.6-19.4 7.6-30.3 0-15.1-5.3-29-14-40 8.8-11 14-24.9 14-40l0-40c0-13.3-10.7-24-24-24s-24 10.7-24 24l0 40c0 8.8-7.2 16-16 16s-16-7.2-16-16l0-80zm32-80l0 0c-18 0-34.6 6-48 16L64 80C28.7 80 0 108.7 0 144s28.7 64 64 64l82 0c-1.3 5.1-2 10.5-2 16 0 25.3 14.7 47.2 36 57.6-2.6 7-4 14.5-4 22.4 0 20 9.2 37.9 23.6 49.7-4.9 9-7.6 19.4-7.6 30.3 0 35.3 28.7 64 64 64l88 0c92.8 0 168-75.2 168-168l0-48c0-92.8-75.2-168-168-168l-24 0zM256 400c-8.8 0-16-7.2-16-16s7.2-16 16-16l64 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-64 0zM240 224c0 5.5 .7 10.9 2 16l-34 0c-8.8 0-16-7.2-16-16s7.2-16 16-16l32 0 0 16zm24 64l40 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-64 0c-8.8 0-16-7.2-16-16s7.2-16 16-16l24 0z"],
    "file-excel": [384, 512, [], "f1c3", "M64 48l112 0 0 88c0 39.8 32.2 72 72 72l88 0 0 240c0 8.8-7.2 16-16 16L64 464c-8.8 0-16-7.2-16-16L48 64c0-8.8 7.2-16 16-16zM224 67.9l92.1 92.1-68.1 0c-13.3 0-24-10.7-24-24l0-68.1zM64 0C28.7 0 0 28.7 0 64L0 448c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-261.5c0-17-6.7-33.3-18.7-45.3L242.7 18.7C230.7 6.7 214.5 0 197.5 0L64 0zm99.2 265.6c-8-10.6-23-12.8-33.6-4.8s-12.8 23-4.8 33.6L162 344 124.8 393.6c-8 10.6-5.8 25.6 4.8 33.6s25.6 5.8 33.6-4.8L192 384 220.8 422.4c8 10.6 23 12.8 33.6 4.8s12.8-23 4.8-33.6L222 344 259.2 294.4c8-10.6 5.8-25.6-4.8-33.6s-25.6-5.8-33.6 4.8L192 304 163.2 265.6z"],
    "envelope": [512, 512, [128386, 9993, 61443], "f0e0", "M61.4 64C27.5 64 0 91.5 0 125.4 0 126.3 0 127.1 .1 128L0 128 0 384c0 35.3 28.7 64 64 64l384 0c35.3 0 64-28.7 64-64l0-256-.1 0c0-.9 .1-1.7 .1-2.6 0-33.9-27.5-61.4-61.4-61.4L61.4 64zM464 192.3L464 384c0 8.8-7.2 16-16 16L64 400c-8.8 0-16-7.2-16-16l0-191.7 154.8 117.4c31.4 23.9 74.9 23.9 106.4 0L464 192.3zM48 125.4C48 118 54 112 61.4 112l389.2 0c7.4 0 13.4 6 13.4 13.4 0 4.2-2 8.2-5.3 10.7L280.2 271.5c-14.3 10.8-34.1 10.8-48.4 0L53.3 136.1c-3.3-2.5-5.3-6.5-5.3-10.7z"],
    "square-caret-down": [448, 512, ["caret-square-down"], "f150", "M384 432c8.8 0 16-7.2 16-16l0-320c0-8.8-7.2-16-16-16L64 80c-8.8 0-16 7.2-16 16l0 320c0 8.8 7.2 16 16 16l320 0zm64-16c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 96C0 60.7 28.7 32 64 32l320 0c35.3 0 64 28.7 64 64l0 320zM224 352c-6.7 0-13-2.8-17.6-7.7l-104-112c-6.5-7-8.2-17.2-4.4-25.9S110.5 192 120 192l208 0c9.5 0 18.2 5.7 22 14.4s2.1 18.9-4.4 25.9l-104 112c-4.5 4.9-10.9 7.7-17.6 7.7z"],
    "truck": [576, 512, [128666, 9951], "f0d1", "M64 80c-8.8 0-16 7.2-16 16l0 288c0 8.8 7.2 16 16 16l3.3 0c10.4-36.9 44.4-64 84.7-64s74.2 27.1 84.7 64l102.6 0c4.9-17.4 15.1-32.7 28.7-43.9L368 96c0-8.8-7.2-16-16-16L64 80zm3.3 368L64 448c-35.3 0-64-28.7-64-64L0 96C0 60.7 28.7 32 64 32l288 0c35.3 0 64 28.7 64 64l0 32 55.4 0c17 0 33.3 6.7 45.3 18.7l40.6 40.6c12 12 18.7 28.3 18.7 45.3L576 384c0 35.3-28.7 64-64 64l-3.3 0c-10.4 36.9-44.4 64-84.7 64s-74.2-27.1-84.7-64l-102.6 0c-10.4 36.9-44.4 64-84.7 64s-74.2-27.1-84.7-64zM416 256l112 0 0-23.4c0-4.2-1.7-8.3-4.7-11.3l-40.6-40.6c-3-3-7.1-4.7-11.3-4.7l-55.4 0 0 80zm0 48l0 32.4c2.6-.2 5.3-.4 8-.4 40.3 0 74.2 27.1 84.7 64l3.3 0c8.8 0 16-7.2 16-16l0-80-112 0zM152 464a40 40 0 1 0 0-80 40 40 0 1 0 0 80zm272 0a40 40 0 1 0 0-80 40 40 0 1 0 0 80z"],
    "bell": [448, 512, [128276, 61602], "f0f3", "M224 0c-13.3 0-24 10.7-24 24l0 9.7C118.6 45.3 56 115.4 56 200l0 14.5c0 37.7-10 74.7-29 107.3L5.1 359.2C1.8 365 0 371.5 0 378.2 0 399.1 16.9 416 37.8 416l372.4 0c20.9 0 37.8-16.9 37.8-37.8 0-6.7-1.8-13.3-5.1-19L421 321.7c-19-32.6-29-69.6-29-107.3l0-14.5c0-84.6-62.6-154.7-144-166.3l0-9.7c0-13.3-10.7-24-24-24zM392.4 368l-336.9 0 12.9-22.1C91.7 306 104 260.6 104 214.5l0-14.5c0-66.3 53.7-120 120-120s120 53.7 120 120l0 14.5c0 46.2 12.3 91.5 35.5 131.4L392.4 368zM156.1 464c9.9 28 36.6 48 67.9 48s58-20 67.9-48l-135.8 0z"],
    "message": [512, 512, ["comment-alt"], "f27a", "M203.7 512.9s0 0 0 0l-37.8 26.7c-7.3 5.2-16.9 5.8-24.9 1.7S128 529 128 520l0-72-32 0c-53 0-96-43-96-96L0 128C0 75 43 32 96 32l320 0c53 0 96 43 96 96l0 224c0 53-43 96-96 96l-120.4 0-91.9 64.9zm64.3-104.1c8.1-5.7 17.8-8.8 27.7-8.8L416 400c26.5 0 48-21.5 48-48l0-224c0-26.5-21.5-48-48-48L96 80c-26.5 0-48 21.5-48 48l0 224c0 26.5 21.5 48 48 48l56 0c10.4 0 19.3 6.6 22.6 15.9 .9 2.5 1.4 5.2 1.4 8.1l0 49.7c32.7-23.1 63.3-44.7 91.9-64.9z"],
    "face-dizzy": [512, 512, ["dizzy"], "f567", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zM134.1 153.9l25.9 25.9 25.9-25.9c7.8-7.8 20.5-7.8 28.3 0s7.8 20.5 0 28.3l-25.9 25.9 25.9 25.9c7.8 7.8 7.8 20.5 0 28.3s-20.5 7.8-28.3 0l-25.9-25.9-25.9 25.9c-7.8 7.8-20.5 7.8-28.3 0s-7.8-20.5 0-28.3l25.9-25.9-25.9-25.9c-7.8-7.8-7.8-20.5 0-28.3s20.5-7.8 28.3 0zm192 0l25.9 25.9 25.9-25.9c7.8-7.8 20.5-7.8 28.3 0s7.8 20.5 0 28.3l-25.9 25.9 25.9 25.9c7.8 7.8 7.8 20.5 0 28.3s-20.5 7.8-28.3 0l-25.9-25.9-25.9 25.9c-7.8 7.8-20.5 7.8-28.3 0s-7.8-20.5 0-28.3l25.9-25.9-25.9-25.9c-7.8-7.8-7.8-20.5 0-28.3s20.5-7.8 28.3 0zM256 288a64 64 0 1 1 0 128 64 64 0 1 1 0-128z"],
    "calendar-days": [448, 512, ["calendar-alt"], "f073", "M120 0c13.3 0 24 10.7 24 24l0 40 160 0 0-40c0-13.3 10.7-24 24-24s24 10.7 24 24l0 40 32 0c35.3 0 64 28.7 64 64l0 288c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 128C0 92.7 28.7 64 64 64l32 0 0-40c0-13.3 10.7-24 24-24zM384 432c8.8 0 16-7.2 16-16l0-64-88 0 0 80 72 0zm16-128l0-80-88 0 0 80 88 0zm-136 0l0-80-80 0 0 80 80 0zm-128 0l0-80-88 0 0 80 88 0zM48 352l0 64c0 8.8 7.2 16 16 16l72 0 0-80-88 0zm136 0l0 80 80 0 0-80-80 0zM120 112l-56 0c-8.8 0-16 7.2-16 16l0 48 352 0 0-48c0-8.8-7.2-16-16-16l-264 0z"],
    "hand-point-up": [384, 512, [9757], "f0a6", "M64 64l0 177.6c5.2-1 10.5-1.6 16-1.6l16 0 0-176c0-8.8-7.2-16-16-16S64 55.2 64 64zM80 288c-17.7 0-32 14.3-32 32l0 24c0 66.3 53.7 120 120 120l48 0c52.5 0 97.1-33.7 113.4-80.7-3.1 .5-6.2 .7-9.4 .7-20 0-37.9-9.2-49.7-23.6-9 4.9-19.4 7.6-30.3 7.6-15.1 0-29-5.3-40-14-11 8.8-24.9 14-40 14l-40 0c-13.3 0-24-10.7-24-24s10.7-24 24-24l40 0c8.8 0 16-7.2 16-16s-7.2-16-16-16l-80 0zM0 320l0 0c0-18 6-34.6 16-48L16 64C16 28.7 44.7 0 80 0s64 28.7 64 64l0 82c5.1-1.3 10.5-2 16-2 25.3 0 47.2 14.7 57.6 36 7-2.6 14.5-4 22.4-4 20 0 37.9 9.2 49.7 23.6 9-4.9 19.4-7.6 30.3-7.6 35.3 0 64 28.7 64 64l0 88c0 92.8-75.2 168-168 168l-48 0C75.2 512 0 436.8 0 344l0-24zm336-64c0-8.8-7.2-16-16-16s-16 7.2-16 16l0 64c0 8.8 7.2 16 16 16s16-7.2 16-16l0-64zM160 240c5.5 0 10.9 .7 16 2l0-34c0-8.8-7.2-16-16-16s-16 7.2-16 16l0 32 16 0zm64 24l0 40c0 8.8 7.2 16 16 16s16-7.2 16-16l0-64c0-8.8-7.2-16-16-16s-16 7.2-16 16l0 24z"],
    "hand-lizard": [512, 512, [], "f258", "M72 112c-13.3 0-24 10.7-24 24s10.7 24 24 24l168 0c35.3 0 64 28.7 64 64s-28.7 64-64 64l-104 0c-13.3 0-24 10.7-24 24s10.7 24 24 24l152 0c4.5 0 8.9 1.3 12.7 3.6l64 40c7 4.4 11.3 12.1 11.3 20.4l0 24c0 13.3-10.7 24-24 24s-24-10.7-24-24l0-10.7-46.9-29.3-145.1 0c-39.8 0-72-32.2-72-72s32.2-72 72-72l104 0c8.8 0 16-7.2 16-16s-7.2-16-16-16L72 208c-39.8 0-72-32.2-72-72S32.2 64 72 64l209.6 0c46.7 0 90.9 21.5 119.7 58.3l78.4 100.1c20.9 26.7 32.3 59.7 32.3 93.7L512 424c0 13.3-10.7 24-24 24s-24-10.7-24-24l0-107.9c0-23.2-7.8-45.8-22.1-64.1L363.5 151.9c-19.7-25.2-49.9-39.9-81.9-39.9L72 112z"],
    "square-full": [512, 512, [128997, 128998, 128999, 129000, 129001, 129002, 129003, 11035, 11036], "f45c", "M448 48c8.8 0 16 7.2 16 16l0 384c0 8.8-7.2 16-16 16L64 464c-8.8 0-16-7.2-16-16L48 64c0-8.8 7.2-16 16-16l384 0zM64 0C28.7 0 0 28.7 0 64L0 448c0 35.3 28.7 64 64 64l384 0c35.3 0 64-28.7 64-64l0-384c0-35.3-28.7-64-64-64L64 0z"],
    "circle-pause": [512, 512, [62092, "pause-circle"], "f28b", "M256 48a208 208 0 1 1 0 416 208 208 0 1 1 0-416zm0 464a256 256 0 1 0 0-512 256 256 0 1 0 0 512zM224 184c0-13.3-10.7-24-24-24s-24 10.7-24 24l0 144c0 13.3 10.7 24 24 24s24-10.7 24-24l0-144zm112 0c0-13.3-10.7-24-24-24s-24 10.7-24 24l0 144c0 13.3 10.7 24 24 24s24-10.7 24-24l0-144z"],
    "hard-drive": [448, 512, [128436, "hdd"], "f0a0", "M64 80c-8.8 0-16 7.2-16 16l0 162c5.1-1.3 10.5-2 16-2l320 0c5.5 0 10.9 .7 16 2l0-162c0-8.8-7.2-16-16-16L64 80zM48 320l0 96c0 8.8 7.2 16 16 16l320 0c8.8 0 16-7.2 16-16l0-96c0-8.8-7.2-16-16-16L64 304c-8.8 0-16 7.2-16 16zM0 320L0 96C0 60.7 28.7 32 64 32l320 0c35.3 0 64 28.7 64 64l0 320c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64l0-96zm216 48a24 24 0 1 1 48 0 24 24 0 1 1 -48 0zm120-24a24 24 0 1 1 0 48 24 24 0 1 1 0-48z"],
    "file-zipper": [384, 512, ["file-archive"], "f1c6", "M64 48l112 0 0 88c0 39.8 32.2 72 72 72l88 0 0 240c0 8.8-7.2 16-16 16L64 464c-8.8 0-16-7.2-16-16L48 64c0-8.8 7.2-16 16-16zM224 67.9l92.1 92.1-68.1 0c-13.3 0-24-10.7-24-24l0-68.1zM64 0C28.7 0 0 28.7 0 64L0 448c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-261.5c0-17-6.7-33.3-18.7-45.3L242.7 18.7C230.7 6.7 214.5 0 197.5 0L64 0zM80 104c0 13.3 10.7 24 24 24l16 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-16 0c-13.3 0-24 10.7-24 24zm0 80c0 13.3 10.7 24 24 24l32 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-32 0c-13.3 0-24 10.7-24 24zm64 56l-32 0c-17.7 0-32 14.3-32 32l0 48c0 26.5 21.5 48 48 48s48-21.5 48-48l0-48c0-17.7-14.3-32-32-32zm-16 64a16 16 0 1 1 0 32 16 16 0 1 1 0-32z"],
    "floppy-disk": [448, 512, [128190, 128426, "save"], "f0c7", "M64 80c-8.8 0-16 7.2-16 16l0 320c0 8.8 7.2 16 16 16l320 0c8.8 0 16-7.2 16-16l0-242.7c0-4.2-1.7-8.3-4.7-11.3L320 86.6 320 176c0 17.7-14.3 32-32 32l-160 0c-17.7 0-32-14.3-32-32l0-96-32 0zm80 0l0 80 128 0 0-80-128 0zM0 96C0 60.7 28.7 32 64 32l242.7 0c17 0 33.3 6.7 45.3 18.7L429.3 128c12 12 18.7 28.3 18.7 45.3L448 416c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 96zM160 320a64 64 0 1 1 128 0 64 64 0 1 1 -128 0z"],
    "face-grin-tongue-squint": [512, 512, [128541, "grin-tongue-squint"], "f58a", "M464 256c0-114.9-93.1-208-208-208S48 141.1 48 256c0 75.9 40.7 142.4 101.5 178.7-3.6-10.9-5.5-22.6-5.5-34.7l0-37.5c-10.2-12.6-18.3-26.9-23.8-42.4-4.1-11.6 7.8-21.4 19.6-17.8 34.7 10.6 74.2 16.5 116.1 16.5 42 0 81.5-6 116.3-16.6 11.8-3.6 23.7 6.1 19.6 17.8-5.5 15.6-13.6 29.9-23.8 42.5l0 37.5c0 12.1-1.9 23.8-5.5 34.7 60.8-36.3 101.5-102.7 101.5-178.7zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm125.8-75.7c-6.2-5.2-7.6-14.3-3.1-21.1s13.3-9.2 20.6-5.5l79.6 40c5.4 2.7 8.8 8.2 8.8 14.3s-3.4 11.6-8.8 14.3l-79.6 40c-7.3 3.6-16.1 1.3-20.6-5.5s-3.1-15.9 3.1-21.1L159 208 125.8 180.3zm263.6-21.1c4.5 6.8 3.1 15.9-3.1 21.1L353 208 386.2 235.7c6.2 5.2 7.6 14.3 3.1 21.1s-13.3 9.2-20.6 5.5l-79.6-40c-5.4-2.7-8.8-8.2-8.8-14.3s3.4-11.6 8.8-14.3l79.6-40c7.3-3.6 16.1-1.3 20.6 5.5zM320 416l0-37.4c0-14.7-11.9-26.6-26.6-26.6l-2 0c-11.3 0-21.1 7.9-23.6 18.9-2.8 12.6-20.8 12.6-23.6 0-2.5-11.1-12.3-18.9-23.6-18.9l-2 0c-14.7 0-26.6 11.9-26.6 26.6l0 37.4c0 35.3 28.7 64 64 64s64-28.7 64-64z"],
    "camera": [512, 512, [62258, "camera-alt"], "f030", "M193.1 32c-18.7 0-36.2 9.4-46.6 24.9L120.5 96 64 96C28.7 96 0 124.7 0 160L0 416c0 35.3 28.7 64 64 64l384 0c35.3 0 64-28.7 64-64l0-256c0-35.3-28.7-64-64-64l-56.5 0-26-39.1C355.1 41.4 337.6 32 318.9 32L193.1 32zm-6.7 51.6c1.5-2.2 4-3.6 6.7-3.6l125.7 0c2.7 0 5.2 1.3 6.7 3.6l33.2 49.8c4.5 6.7 11.9 10.7 20 10.7l69.3 0c8.8 0 16 7.2 16 16l0 256c0 8.8-7.2 16-16 16L64 432c-8.8 0-16-7.2-16-16l0-256c0-8.8 7.2-16 16-16l69.3 0c8 0 15.5-4 20-10.7l33.2-49.8zM256 384a112 112 0 1 0 0-224 112 112 0 1 0 0 224zM192 272a64 64 0 1 1 128 0 64 64 0 1 1 -128 0z"],
    "face-grin-stars": [512, 512, [129321, "grin-stars"], "f587", "M0 256c0-29.6 5-57.9 14.2-84.4l17.3 16.9-4.6 27c-4.2 24.4 5.6 46.2 22 59.9 9.8 105.8 98.8 188.7 207.1 188.7s197.4-82.8 207.1-188.6c16.4-13.7 26.1-35.4 22-59.9l-4.6-27 17.3-16.9c9.2 26.4 14.2 54.8 14.2 84.4 0 141.4-114.6 256-256 256S0 397.4 0 256zM256 48c-15.2 0-30 1.6-44.3 4.7L201.4 31.8C197 23 191.1 15.8 184.2 10.2 207 3.6 231.1 0 256 0s49 3.6 71.8 10.2C320.9 15.8 315 23 310.6 31.8L300.3 52.7C286 49.6 271.2 48 256 48zM372.2 302.3c11.8-3.6 23.7 6.1 19.6 17.8-19.8 55.9-73.1 96-135.8 96-62.7 0-116-40-135.8-95.9-4.1-11.6 7.8-21.4 19.6-17.8 34.7 10.6 74.2 16.5 116.1 16.5 42 0 81.5-6 116.3-16.6zM353.7 53.1c5.9-11.9 22.8-11.9 28.7 0l23.3 47.2 52 7.6c13.1 1.9 18.4 18 8.9 27.3l-37.7 36.7 8.9 51.8c2.2 13.1-11.5 23-23.2 16.9L368 216 321.5 240.5c-11.7 6.2-25.5-3.8-23.2-16.9l8.9-51.8-37.7-36.7c-9.5-9.3-4.3-25.4 8.9-27.3l52-7.6 23.3-47.2zm-195.3 0l23.3 47.2 52 7.6c13.1 1.9 18.4 18 8.9 27.3l-37.7 36.7 8.9 51.8c2.2 13.1-11.5 23-23.2 16.9L144 216 97.5 240.5c-11.7 6.2-25.5-3.8-23.2-16.9l8.9-51.8-37.7-36.7c-9.5-9.3-4.3-25.4 8.9-27.3l52-7.6 23.3-47.2c5.9-11.9 22.8-11.9 28.7 0z"],
    "eye": [576, 512, [128065], "f06e", "M288 80C222.8 80 169.2 109.6 128.1 147.7 89.6 183.5 63 226 49.4 256 63 286 89.6 328.5 128.1 364.3 169.2 402.4 222.8 432 288 432s118.8-29.6 159.9-67.7C486.4 328.5 513 286 526.6 256 513 226 486.4 183.5 447.9 147.7 406.8 109.6 353.2 80 288 80zM95.4 112.6C142.5 68.8 207.2 32 288 32s145.5 36.8 192.6 80.6c46.8 43.5 78.1 95.4 93 131.1 3.3 7.9 3.3 16.7 0 24.6-14.9 35.7-46.2 87.7-93 131.1-47.1 43.7-111.8 80.6-192.6 80.6S142.5 443.2 95.4 399.4c-46.8-43.5-78.1-95.4-93-131.1-3.3-7.9-3.3-16.7 0-24.6 14.9-35.7 46.2-87.7 93-131.1zM288 336c44.2 0 80-35.8 80-80 0-29.6-16.1-55.5-40-69.3-1.4 59.7-49.6 107.9-109.3 109.3 13.8 23.9 39.7 40 69.3 40zm-79.6-88.4c2.5 .3 5 .4 7.6 .4 35.3 0 64-28.7 64-64 0-2.6-.2-5.1-.4-7.6-37.4 3.9-67.2 33.7-71.1 71.1zm45.6-115c10.8-3 22.2-4.5 33.9-4.5 8.8 0 17.5 .9 25.8 2.6 .3 .1 .5 .1 .8 .2 57.9 12.2 101.4 63.7 101.4 125.2 0 70.7-57.3 128-128 128-61.6 0-113-43.5-125.2-101.4-1.8-8.6-2.8-17.5-2.8-26.6 0-11 1.4-21.8 4-32 .2-.7 .3-1.3 .5-1.9 11.9-43.4 46.1-77.6 89.5-89.5z"],
    "face-sad-tear": [512, 512, [128546, "sad-tear"], "f5b4", "M464 256c0-114.9-93.1-208-208-208S48 141.1 48 256c0 41.8 12.3 80.7 33.6 113.3 8.2 44.7 47.3 78.6 94.3 78.7 24.7 10.3 51.7 16 80.1 16 114.9 0 208-93.1 208-208zM288 352c-5.5 0-10.9 .6-16 1.8 0-.6 0-1.2 0-1.8 0-16.2-4-31.5-11.1-44.9 8.7-2 17.8-3.1 27.1-3.1 40.2 0 75.7 19.8 97.5 50 7.7 10.8 5.3 25.8-5.5 33.5s-25.8 5.3-33.5-5.5c-13.1-18.2-34.4-30-58.5-30zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm176-80a32 32 0 1 1 0 64 32 32 0 1 1 0-64zm128 32a32 32 0 1 1 64 0 32 32 0 1 1 -64 0zM185.4 276.8c6.5 7.8 12.6 16.1 18.3 24.6 9 13.4 20.3 30.2 20.3 47.4 0 28.3-21.5 51.2-48 51.2s-48-22.9-48-51.2c0-17.2 11.2-34 20.3-47.4 5.7-8.5 11.9-16.7 18.3-24.6 2.4-2.9 5.7-4.8 9.4-4.8s7 1.9 9.4 4.8z"],
    "share-from-square": [576, 512, [61509, "share-square"], "f14d", "M425.5 7c-6.9-6.9-17.2-8.9-26.2-5.2S384.5 14.3 384.5 24l0 56-48 0c-88.4 0-160 71.6-160 160 0 46.7 20.7 80.4 43.6 103.4 8.1 8.2 16.5 14.9 24.3 20.4 9.2 6.5 21.7 5.7 30.1-1.9s10.2-20 4.5-29.8c-3.6-6.3-6.5-14.9-6.5-26.7 0-36.2 29.3-65.5 65.5-65.5l46.5 0 0 56c0 9.7 5.8 18.5 14.8 22.2s19.3 1.7 26.2-5.2l136-136c9.4-9.4 9.4-24.6 0-33.9L425.5 7zm7 97l0-22.1 78.1 78.1-78.1 78.1 0-22.1c0-13.3-10.7-24-24-24L338 192c-50.9 0-93.9 33.5-108.3 79.6-3.3-9.4-5.2-19.8-5.2-31.6 0-61.9 50.1-112 112-112l72 0c13.3 0 24-10.7 24-24zm-320-8c-44.2 0-80 35.8-80 80l0 256c0 44.2 35.8 80 80 80l256 0c44.2 0 80-35.8 80-80l0-24c0-13.3-10.7-24-24-24s-24 10.7-24 24l0 24c0 17.7-14.3 32-32 32l-256 0c-17.7 0-32-14.3-32-32l0-256c0-17.7 14.3-32 32-32l24 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-24 0z"],
    "note-sticky": [448, 512, [62026, "sticky-note"], "f249", "M240 432L64 432c-8.8 0-16-7.2-16-16L48 96c0-8.8 7.2-16 16-16l320 0c8.8 0 16 7.2 16 16l0 176-88 0c-39.8 0-72 32.2-72 72l0 88zM380.1 320L288 412.1 288 344c0-13.3 10.7-24 24-24l68.1 0zM0 416c0 35.3 28.7 64 64 64l197.5 0c17 0 33.3-6.7 45.3-18.7L429.3 338.7c12-12 18.7-28.3 18.7-45.3L448 96c0-35.3-28.7-64-64-64L64 32C28.7 32 0 60.7 0 96L0 416z"],
    "hand-back-fist": [384, 512, ["hand-rock"], "f255", "M96 400c-17.7 0-32 14.3-32 32l0 48c0 17.7 14.3 32 32 32l224 0c17.7 0 32-14.3 32-32l0-48c0-17.7-14.3-32-32-32L96 400zM73.2 352l64.6 0-79.5-88.3C51.7 256.3 48 246.8 48 236.9L48 204c0-16.1 11.9-29.5 27.4-31.7 11.8-1.7 20.6-11.8 20.6-23.8L96 72c0-13.3 10.7-24 24-24 7.2 0 13.6 3.1 18 8.1 4.6 5.2 11.1 8.1 18 8.1s13.4-3 18-8.1c4.4-5 10.8-8.1 18-8.1 8.5 0 15.9 4.4 20.2 11.1 6.9 10.7 20.9 14.2 32 8 3.5-1.9 7.4-3.1 11.8-3.1 10.6 0 19.7 6.9 22.8 16.6 3.8 11.7 15.9 18.7 28 16 1.7-.4 3.4-.6 5.2-.6 13.3 0 24 10.7 24 24l0 92.2c0 14.4-3.5 28.5-10.2 41.2l-52.2 98.6 54.3 0 40.3-76.2c10.4-19.6 15.8-41.5 15.8-63.6l0-92.2c0-38.4-30.1-69.8-68.1-71.9-12.9-19.3-34.9-32.1-59.9-32.1-5.7 0-11.2 .7-16.5 1.9-12.7-11.1-29.3-17.9-47.5-17.9-13.1 0-25.4 3.5-36 9.6-10.6-6.1-22.9-9.6-36-9.6-39.8 0-72 32.2-72 72l0 58.7C19.7 143 0 171.2 0 204l0 32.9c0 21.7 8 42.7 22.6 58.9L73.2 352z"],
    "chess-queen": [512, 512, [9819], "f445", "M325.3 90.8c9.1-4.8 20.6-3.3 28.2 4.3l39.8 39.8 3.7 3.3c9.1 7.1 20.9 10 32.4 7.7l46.4-9.3 3.5-.4c8-.4 15.8 3.2 20.6 9.8 5.5 7.6 6.1 17.6 1.6 25.8l-112.6 202.6 51.5 70.9 1.8 2.7c4 6.6 6.2 14.2 6.2 22 0 23.3-18.9 42.1-42.1 42.1l-299.8 0c-21.8 0-39.8-16.6-41.9-37.8l-.2-4.3 .1-3.3c.6-7.7 3.4-15.1 7.9-21.4l51.5-70.9-112.5-202.6c-4.5-8.2-3.9-18.3 1.6-25.8s14.9-11.2 24.1-9.4l46.4 9.3c13.1 2.6 26.7-1.5 36.1-10.9L159.5 95 163 92.2c8.6-5.8 20.1-5.6 28.5 1.1l40 32 2.8 2.1c14.4 9.6 33.5 8.9 47.2-2.1l40-32 3.8-2.5zM164.7 400l-46.6 64 276.7 0-46.6-64-183.6 0zM311.5 162.8c-30.1 24.1-72.1 25.6-103.8 4.5l-6.2-4.5-23.3-18.6-24.6 24.6c-19.8 19.8-47.7 28.9-75.1 24.8l88.1 158.5 179.8 0 88-158.5c-25.7 3.8-51.7-3.9-71.1-21l-4-3.7-24.6-24.6-23.2 18.6zM256.5 72a40 40 0 1 1 0-80 40 40 0 1 1 0 80z"],
    "face-grin-tears": [640, 512, [128514, "grin-tears"], "f588", "M504.1 353C512.9 367.2 525.3 379 539.8 387.2 495.1 462 413.4 512 320 512S144.9 462 100.2 387.2c14.6-8.2 26.9-20 35.8-34.3 34.9 66 104.2 111 184.1 111s149.2-45 184.1-111zm16.4-152.5C496.2 112.6 415.7 48 320 48S143.8 112.6 119.5 200.5c-10.6-4.8-22.7-6.8-35.4-5l-13.4 1.9C97.2 84.3 198.8 0 320 0S542.8 84.3 569.3 197.4l-13.4-1.9c-12.7-1.8-24.8 .2-35.4 5zM455.8 320c-19.8 55.9-73.1 96-135.8 96-62.7 0-116-40-135.8-95.9-4.1-11.6 7.8-21.4 19.6-17.8 34.7 10.6 74.2 16.5 116.1 16.5 42 0 81.5-6 116.3-16.6 11.8-3.6 23.7 6.1 19.6 17.8zM212 208l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-37.6 30.4-68 68-68s68 30.4 68 68l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-15.5-12.5-28-28-28s-28 12.5-28 28zm188-28c-15.5 0-28 12.5-28 28l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-37.6 30.4-68 68-68s68 30.4 68 68l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-15.5-12.5-28-28-28zM640 300.6c0 28.4-23 51.4-51.4 51.4-25.6 0-47.3-18.8-50.9-44.1L531 261.1c-1.5-10.6 7.5-19.6 18.1-18.1l46.7 6.7c25.3 3.6 44.1 25.3 44.1 50.9zm-640 0c0-25.6 18.8-47.3 44.1-50.9L90.9 243c10.6-1.5 19.6 7.5 18.1 18.1l-6.7 46.7C98.7 333.2 77 352 51.4 352 23 352 0 329 0 300.6z"],
    "pen-to-square": [512, 512, ["edit"], "f044", "M441 58.9L453.1 71c9.4 9.4 9.4 24.6 0 33.9L424 134.1 377.9 88 407 58.9c9.4-9.4 24.6-9.4 33.9 0zM209.8 256.2L344 121.9 390.1 168 255.8 302.2c-2.9 2.9-6.5 5-10.4 6.1l-58.5 16.7 16.7-58.5c1.1-3.9 3.2-7.5 6.1-10.4zM373.1 25L175.8 222.2c-8.7 8.7-15 19.4-18.3 31.1l-28.6 100c-2.4 8.4-.1 17.4 6.1 23.6s15.2 8.5 23.6 6.1l100-28.6c11.8-3.4 22.5-9.7 31.1-18.3L487 138.9c28.1-28.1 28.1-73.7 0-101.8L474.9 25C446.8-3.1 401.2-3.1 373.1 25zM88 64C39.4 64 0 103.4 0 152L0 424c0 48.6 39.4 88 88 88l272 0c48.6 0 88-39.4 88-88l0-112c0-13.3-10.7-24-24-24s-24 10.7-24 24l0 112c0 22.1-17.9 40-40 40L88 464c-22.1 0-40-17.9-40-40l0-272c0-22.1 17.9-40 40-40l112 0c13.3 0 24-10.7 24-24s-10.7-24-24-24L88 64z"],
    "face-grin-beam-sweat": [576, 512, [128517, "grin-beam-sweat"], "f583", "M530.2 15.9c-8.8-10.7-18.5-20.9-29-30-3-2.6-7.4-2.6-10.4 0-10.5 9.1-20.1 19.3-29 30-14.7 17.8-29.8 40.1-29.8 64.1 0 36.4 27.6 64 64 64s64-27.6 64-64c0-24-15.2-46.3-29.8-64.1zm-132 8.9C364.8 8.9 327.4 0 288 0 146.6 0 32 114.6 32 256S146.6 512 288 512 544 397.4 544 256c0-24.4-3.4-48-9.8-70.4-11.9 4.2-24.7 6.4-38.2 6.4-3.4 0-6.8-.1-10.2-.4 6.6 20.3 10.2 41.9 10.2 64.4 0 114.9-93.1 208-208 208S80 370.9 80 256 173.1 48 288 48c34.8 0 67.5 8.5 96.3 23.6 1.4-17.4 6.9-33.1 13.8-46.8zM423.8 320c4.1-11.6-7.8-21.4-19.6-17.8-34.8 10.6-74.3 16.6-116.3 16.6-41.9 0-81.4-6-116.1-16.5-11.8-3.6-23.7 6.1-19.6 17.8 19.8 55.9 73.1 95.9 135.8 95.9 62.7 0 116-40.1 135.8-96zM180 208c0-15.5 12.5-28 28-28s28 12.5 28 28l0 8c0 11 9 20 20 20s20-9 20-20l0-8c0-37.6-30.4-68-68-68s-68 30.4-68 68l0 8c0 11 9 20 20 20s20-9 20-20l0-8zm188-28c15.5 0 28 12.5 28 28l0 8c0 11 9 20 20 20s20-9 20-20l0-8c0-37.6-30.4-68-68-68s-68 30.4-68 68l0 8c0 11 9 20 20 20s20-9 20-20l0-8c0-15.5 12.5-28 28-28z"],
    "clock": [512, 512, [128339, "clock-four"], "f017", "M464 256a208 208 0 1 1 -416 0 208 208 0 1 1 416 0zM0 256a256 256 0 1 0 512 0 256 256 0 1 0 -512 0zM232 120l0 136c0 8 4 15.5 10.7 20l96 64c11 7.4 25.9 4.4 33.3-6.7s4.4-25.9-6.7-33.3L280 243.2 280 120c0-13.3-10.7-24-24-24s-24 10.7-24 24z"],
    "face-laugh-wink": [512, 512, ["laugh-wink"], "f59c", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm118.3 58.2c-4.2-13.7 7.1-26.2 21.4-26.2l232.6 0c14.3 0 25.6 12.5 21.4 26.2-18 58.9-72.9 101.8-137.7 101.8S136.3 373.1 118.3 314.2zM144 192a32 32 0 1 1 64 0 32 32 0 1 1 -64 0zm164 8c0 11-9 20-20 20s-20-9-20-20c0-33.1 26.9-60 60-60l16 0c33.1 0 60 26.9 60 60 0 11-9 20-20 20s-20-9-20-20-9-20-20-20l-16 0c-11 0-20 9-20 20z"],
    "paper-plane": [576, 512, [61913], "f1d8", "M290.5 287.7L491.4 86.9 359 456.3 290.5 287.7zM457.4 53L256.6 253.8 88 185.3 457.4 53zM38.1 216.8l205.8 83.6 83.6 205.8c5.3 13.1 18.1 21.7 32.3 21.7 14.7 0 27.8-9.2 32.8-23.1L570.6 8c3.5-9.8 1-20.6-6.3-28s-18.2-9.8-28-6.3L39.4 151.7c-13.9 5-23.1 18.1-23.1 32.8 0 14.2 8.6 27 21.7 32.3z"],
    "heart": [512, 512, [128153, 128154, 128155, 128156, 128420, 129293, 129294, 129505, 9829, 10084, 61578], "f004", "M378.9 80c-27.3 0-53 13.1-69 35.2l-34.4 47.6c-4.5 6.2-11.7 9.9-19.4 9.9s-14.9-3.7-19.4-9.9l-34.4-47.6c-16-22.1-41.7-35.2-69-35.2-47 0-85.1 38.1-85.1 85.1 0 49.9 32 98.4 68.1 142.3 41.1 50 91.4 94 125.9 120.3 3.2 2.4 7.9 4.2 14 4.2s10.8-1.8 14-4.2c34.5-26.3 84.8-70.4 125.9-120.3 36.2-43.9 68.1-92.4 68.1-142.3 0-47-38.1-85.1-85.1-85.1zM271 87.1c25-34.6 65.2-55.1 107.9-55.1 73.5 0 133.1 59.6 133.1 133.1 0 68.6-42.9 128.9-79.1 172.8-44.1 53.6-97.3 100.1-133.8 127.9-12.3 9.4-27.5 14.1-43.1 14.1s-30.8-4.7-43.1-14.1C176.4 438 123.2 391.5 79.1 338 42.9 294.1 0 233.7 0 165.1 0 91.6 59.6 32 133.1 32 175.8 32 216 52.5 241 87.1l15 20.7 15-20.7z"],
    "font-awesome": [512, 512, [62501, 62694, "font-awesome-flag", "font-awesome-logo-full"], "f2b4", "M91.7 96C106.3 86.8 116 70.5 116 52 116 23.3 92.7 0 64 0S12 23.3 12 52c0 16.7 7.8 31.5 20 41l0 419 48 0 0-64 389.6 0c14.6 0 26.4-11.8 26.4-26.4 0-3.7-.8-7.3-2.3-10.7L432 272 493.7 133.1c1.5-3.4 2.3-7 2.3-10.7 0-14.6-11.8-26.4-26.4-26.4L91.7 96zM80 400l0-256 356.4 0-48.2 108.5c-5.5 12.4-5.5 26.6 0 39L436.4 400 80 400z"],
    "clone": [512, 512, [], "f24d", "M288 464L64 464c-8.8 0-16-7.2-16-16l0-224c0-8.8 7.2-16 16-16l48 0 0-48-48 0c-35.3 0-64 28.7-64 64L0 448c0 35.3 28.7 64 64 64l224 0c35.3 0 64-28.7 64-64l0-48-48 0 0 48c0 8.8-7.2 16-16 16zM224 304c-8.8 0-16-7.2-16-16l0-224c0-8.8 7.2-16 16-16l224 0c8.8 0 16 7.2 16 16l0 224c0 8.8-7.2 16-16 16l-224 0zm-64-16c0 35.3 28.7 64 64 64l224 0c35.3 0 64-28.7 64-64l0-224c0-35.3-28.7-64-64-64L224 0c-35.3 0-64 28.7-64 64l0 224z"],
    "folder-open": [576, 512, [128194, 128449, 61717], "f07c", "M97.5 400l50-160 379.4 0-50 160-379.4 0zm190.7 48L477 448c21 0 39.6-13.6 45.8-33.7l50-160c9.7-30.9-13.4-62.3-45.8-62.3l-379.4 0c-21 0-39.6 13.6-45.8 33.7L80.2 294.4 80.2 96c0-8.8 7.2-16 16-16l138.7 0c3.5 0 6.8 1.1 9.6 3.2L282.9 112c13.8 10.4 30.7 16 48 16l117.3 0c8.8 0 16 7.2 16 16l48 0c0-35.3-28.7-64-64-64L330.9 80c-6.9 0-13.7-2.2-19.2-6.4L273.3 44.8C262.2 36.5 248.8 32 234.9 32L96.2 32c-35.3 0-64 28.7-64 64l0 288c0 35.3 28.7 64 64 64l192 0z"],
    "window-minimize": [512, 512, [128469], "f2d1", "M0 424c0-13.3 10.7-24 24-24l464 0c13.3 0 24 10.7 24 24s-10.7 24-24 24L24 448c-13.3 0-24-10.7-24-24z"],
    "star-half": [576, 512, [61731], "f089", "M285.7-15.8c10.8 2.6 18.4 12.2 18.4 23.3l0 387.1c0 9-5.1 17.3-13.1 21.4L143.8 491c-8 4.1-17.7 3.3-25-2s-11-14.2-9.6-23.2L134.4 305.9 20 191.4c-6.4-6.4-8.6-15.8-5.8-24.4s10.1-14.9 19.1-16.3L193.1 125.3 258.8-3.3c5-9.9 16.2-15 27-12.4zM256.1 107.4L230.3 158c-3.5 6.8-10 11.6-17.6 12.8l-125.5 20 89.8 89.9c5.4 5.4 7.9 13.1 6.7 20.7l-19.8 125.5 92.2-46.9 0-272.6z"],
    "alarm-clock": [512, 512, [9200], "f34e", "M402.6 50.2c-5.4 1.7-11.3 1.8-16.2-.9-5.8-3.2-11.8-6.2-17.8-8.9-10.4-4.7-13.7-18.3-4.1-24.6 15-9.9 33-15.7 52.3-15.7 52.6 0 95.2 42.6 95.2 95.2 0 13.2-2.7 25.8-7.6 37.3-4.5 10.5-18.4 9.8-24.9 .4-3.8-5.5-7.8-10.8-12-16-3.5-4.4-4.5-10.2-3.8-15.8 .2-1.9 .4-3.9 .4-5.9 0-26.1-21.2-47.2-47.2-47.2-4.9 0-9.7 .8-14.2 2.2zM32.5 132.9c-6.5 9.4-20.5 10.1-24.9-.4-4.9-11.5-7.6-24.1-7.6-37.3 0-52.6 42.6-95.2 95.2-95.2 19.3 0 37.3 5.8 52.3 15.7 9.6 6.3 6.3 19.9-4.1 24.6-6.1 2.8-12 5.7-17.8 8.9-4.9 2.7-10.9 2.6-16.2 .9-4.5-1.4-9.2-2.2-14.2-2.2-26.1 0-47.2 21.2-47.2 47.2 0 2 .1 4 .4 5.9 .7 5.6-.3 11.4-3.8 15.8-4.2 5.2-8.2 10.5-12 16zM432 288a176 176 0 1 0 -352 0 176 176 0 1 0 352 0zM396.5 462.5C358.1 493.4 309.2 512 256 512s-102.1-18.6-140.5-49.5L73 505c-9.4 9.4-24.6 9.4-33.9 0s-9.4-24.6 0-33.9l42.5-42.5C50.6 390.1 32 341.2 32 288 32 164.3 132.3 64 256 64S480 164.3 480 288c0 53.2-18.6 102.1-49.5 140.5L473 471c9.4 9.4 9.4 24.6 0 33.9s-24.6 9.4-33.9 0l-42.5-42.5zM280 184l0 94.1 41 41c9.4 9.4 9.4 24.6 0 33.9s-24.6 9.4-33.9 0l-48-48c-4.5-4.5-7-10.6-7-17l0-104c0-13.3 10.7-24 24-24s24 10.7 24 24z"],
    "newspaper": [512, 512, [128240], "f1ea", "M168 80c-13.3 0-24 10.7-24 24l0 304c0 8.4-1.4 16.5-4.1 24L440 432c13.3 0 24-10.7 24-24l0-304c0-13.3-10.7-24-24-24L168 80zM72 480c-39.8 0-72-32.2-72-72L0 112C0 98.7 10.7 88 24 88s24 10.7 24 24l0 296c0 13.3 10.7 24 24 24s24-10.7 24-24l0-304c0-39.8 32.2-72 72-72l272 0c39.8 0 72 32.2 72 72l0 304c0 39.8-32.2 72-72 72L72 480zM192 152c0-13.3 10.7-24 24-24l48 0c13.3 0 24 10.7 24 24l0 48c0 13.3-10.7 24-24 24l-48 0c-13.3 0-24-10.7-24-24l0-48zm152 24l48 0c13.3 0 24 10.7 24 24s-10.7 24-24 24l-48 0c-13.3 0-24-10.7-24-24s10.7-24 24-24zM216 256l176 0c13.3 0 24 10.7 24 24s-10.7 24-24 24l-176 0c-13.3 0-24-10.7-24-24s10.7-24 24-24zm0 80l176 0c13.3 0 24 10.7 24 24s-10.7 24-24 24l-176 0c-13.3 0-24-10.7-24-24s10.7-24 24-24z"],
    "hospital": [576, 512, [127973, 62589, "hospital-alt", "hospital-wide"], "f0f8", "M176 0c-35.3 0-64 28.7-64 64l0 48-48 0c-35.3 0-64 28.7-64 64L0 448c0 35.3 28.7 64 64 64l448 0c35.3 0 64-28.7 64-64l0-272c0-35.3-28.7-64-64-64l-48 0 0-48c0-35.3-28.7-64-64-64L176 0zM160 64c0-8.8 7.2-16 16-16l224 0c8.8 0 16 7.2 16 16l0 72c0 13.3 10.7 24 24 24l72 0c8.8 0 16 7.2 16 16l0 272c0 8.8-7.2 16-16 16l-176 0 0-80c0-17.7-14.3-32-32-32l-32 0c-17.7 0-32 14.3-32 32l0 80-176 0c-8.8 0-16-7.2-16-16l0-272c0-8.8 7.2-16 16-16l72 0c13.3 0 24-10.7 24-24l0-72zM112 224c-8.8 0-16 7.2-16 16l0 32c0 8.8 7.2 16 16 16l32 0c8.8 0 16-7.2 16-16l0-32c0-8.8-7.2-16-16-16l-32 0zM96 336l0 32c0 8.8 7.2 16 16 16l32 0c8.8 0 16-7.2 16-16l0-32c0-8.8-7.2-16-16-16l-32 0c-8.8 0-16 7.2-16 16zm320 0l0 32c0 8.8 7.2 16 16 16l32 0c8.8 0 16-7.2 16-16l0-32c0-8.8-7.2-16-16-16l-32 0c-8.8 0-16 7.2-16 16zm16-112c-8.8 0-16 7.2-16 16l0 32c0 8.8 7.2 16 16 16l32 0c8.8 0 16-7.2 16-16l0-32c0-8.8-7.2-16-16-16l-32 0zM264 104l0 32-32 0c-8.8 0-16 7.2-16 16l0 16c0 8.8 7.2 16 16 16l32 0 0 32c0 8.8 7.2 16 16 16l16 0c8.8 0 16-7.2 16-16l0-32 32 0c8.8 0 16-7.2 16-16l0-16c0-8.8-7.2-16-16-16l-32 0 0-32c0-8.8-7.2-16-16-16l-16 0c-8.8 0-16 7.2-16 16z"],
    "circle-stop": [512, 512, [62094, "stop-circle"], "f28d", "M256 48a208 208 0 1 1 0 416 208 208 0 1 1 0-416zm0 464a256 256 0 1 0 0-512 256 256 0 1 0 0 512zM160 192l0 128c0 17.7 14.3 32 32 32l128 0c17.7 0 32-14.3 32-32l0-128c0-17.7-14.3-32-32-32l-128 0c-17.7 0-32 14.3-32 32zm48 112l0-96 96 0 0 96-96 0z"],
    "object-ungroup": [640, 512, [], "f248", "M48.2 66.8c-.1-.8-.2-1.7-.2-2.5l0-.2c0-8.8 7.2-16 16-16 .9 0 1.9 .1 2.8 .2 7.5 1.3 13.2 7.9 13.2 15.8 0 8.8-7.2 16-16 16-7.9 0-14.5-5.7-15.8-13.2zM0 64c0 26.9 16.5 49.9 40 59.3l0 105.3c-23.5 9.5-40 32.5-40 59.3 0 35.3 28.7 64 64 64 26.9 0 49.9-16.5 59.3-40l201.3 0c9.5 23.5 32.5 40 59.3 40 35.3 0 64-28.7 64-64 0-26.9-16.5-49.9-40-59.3l0-105.3c23.5-9.5 40-32.5 40-59.3 0-35.3-28.7-64-64-64-26.9 0-49.9 16.5-59.3 40L123.3 40C113.9 16.5 90.9 0 64 0 28.7 0 0 28.7 0 64zm368 0a16 16 0 1 1 32 0 16 16 0 1 1 -32 0zM324.7 88c6.5 16 19.3 28.9 35.3 35.3l0 105.3c-16 6.5-28.9 19.3-35.3 35.3l-201.3 0c-6.5-16-19.3-28.9-35.3-35.3l0-105.3c16-6.5 28.9-19.3 35.3-35.3l201.3 0zM384 272a16 16 0 1 1 0 32 16 16 0 1 1 0-32zM80 288c0 7.9-5.7 14.5-13.2 15.8-.8 .1-1.7 .2-2.5 .2l-.2 0c-8.8 0-16-7.2-16-16 0-.9 .1-1.9 .2-2.8 1.3-7.5 7.9-13.2 15.8-13.2 8.8 0 16 7.2 16 16zm436.7-40c6.5 16 19.3 28.9 35.3 35.3l0 105.3c-16 6.5-28.9 19.3-35.3 35.3l-201.3 0c-6.5-16-19.3-28.9-35.3-35.3l0-20.7-48 0 0 20.7c-23.5 9.5-40 32.5-40 59.3 0 35.3 28.7 64 64 64 26.9 0 49.9-16.5 59.3-40l201.3 0c9.5 23.5 32.5 40 59.3 40 35.3 0 64-28.7 64-64 0-26.9-16.5-49.9-40-59.3l0-105.3c23.5-9.5 40-32.5 40-59.3 0-35.3-28.7-64-64-64-26.9 0-49.9 16.5-59.3 40l-52.7 0 0 9.6c10.7 10.9 19.1 23.9 24.6 38.4l28 0zm59.3-8a16 16 0 1 1 0-32 16 16 0 1 1 0 32zM271.8 450.7a16 16 0 1 1 -31.5-5.5 16 16 0 1 1 31.5 5.5zm301.5 13c-7.5-1.3-13.2-7.9-13.2-15.8 0-8.8 7.2-16 16-16 7.9 0 14.5 5.7 15.8 13.2l0 .1c.1 .9 .2 1.8 .2 2.7 0 8.8-7.2 16-16 16-.9 0-1.9-.1-2.8-.2z"],
    "comment": [512, 512, [128489, 61669], "f075", "M51.9 384.9C19.3 344.6 0 294.4 0 240 0 107.5 114.6 0 256 0S512 107.5 512 240 397.4 480 256 480c-36.5 0-71.2-7.2-102.6-20L37 509.9c-3.7 1.6-7.5 2.1-11.5 2.1-14.1 0-25.5-11.4-25.5-25.5 0-4.3 1.1-8.5 3.1-12.2l48.8-89.4zm37.3-30.2c12.2 15.1 14.1 36.1 4.8 53.2l-18 33.1 58.5-25.1c11.8-5.1 25.2-5.2 37.1-.3 25.7 10.5 54.2 16.4 84.3 16.4 117.8 0 208-88.8 208-192S373.8 48 256 48 48 136.8 48 240c0 42.8 15.1 82.4 41.2 114.7z"],
    "chess-pawn": [384, 512, [9823], "f443", "M192-32c66.3 0 120 53.7 120 120 0 27.6-9.3 52.9-24.9 73.2 9.8 3 16.9 12.1 16.9 22.8 0 13.3-10.7 24-24 24l-.6 0 24.6 160 53.6 67c6.7 8.4 10.4 18.8 10.4 29.6 0 26.2-21.2 47.4-47.4 47.4L63.4 512c-26.2 0-47.4-21.2-47.4-47.4 0-10.8 3.7-21.2 10.4-29.6l53.6-67 24.6-160-.6 0c-13.3 0-24-10.7-24-24 0-10.8 7.1-19.8 16.9-22.8-15.6-20.3-24.9-45.6-24.9-73.2 0-66.3 53.7-120 120-120zM115.9 400l-51.2 64 254.7 0-51.2-64-152.2 0zm36.2-184.7l-21 136.7 121.9 0-21-136.7-1.1-7.3-77.6 0-1.1 7.3zM192 16a72 72 0 1 0 0 144 72 72 0 1 0 0-144z"],
    "calendar-plus": [448, 512, [], "f271", "M120 0c13.3 0 24 10.7 24 24l0 40 160 0 0-40c0-13.3 10.7-24 24-24s24 10.7 24 24l0 40 32 0c35.3 0 64 28.7 64 64l0 288c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 128C0 92.7 28.7 64 64 64l32 0 0-40c0-13.3 10.7-24 24-24zm0 112l-56 0c-8.8 0-16 7.2-16 16l0 288c0 8.8 7.2 16 16 16l320 0c8.8 0 16-7.2 16-16l0-288c0-8.8-7.2-16-16-16l-264 0zm104 64c13.3 0 24 10.7 24 24l0 48 48 0c13.3 0 24 10.7 24 24s-10.7 24-24 24l-48 0 0 48c0 13.3-10.7 24-24 24s-24-10.7-24-24l0-48-48 0c-13.3 0-24-10.7-24-24s10.7-24 24-24l48 0 0-48c0-13.3 10.7-24 24-24z"],
    "clipboard": [384, 512, [128203], "f328", "M232 96l-80 0c-13.3 0-24-10.7-24-24s10.7-24 24-24l80 0c13.3 0 24 10.7 24 24s-10.7 24-24 24zm0 48c37.1 0 67.6-28 71.6-64L320 80c8.8 0 16 7.2 16 16l0 352c0 8.8-7.2 16-16 16L64 464c-8.8 0-16-7.2-16-16L48 96c0-8.8 7.2-16 16-16l16.4 0c4 36 34.5 64 71.6 64l80 0zM291.9 32C279 12.7 257 0 232 0L152 0c-25 0-47 12.7-59.9 32L64 32C28.7 32 0 60.7 0 96L0 448c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-352c0-35.3-28.7-64-64-64l-28.1 0z"],
    "thumbs-down": [512, 512, [128078, 61576], "f165", "M360 32l7.4 .4c35 3.6 62.5 32.2 64.4 67.7 17.8 11.8 30.1 31.4 32 53.9l.2 6c0 5.7-.7 11.2-2 16.5 10.2 11.5 16.8 26.3 17.8 42.7l.2 4.8c0 13.2-3.6 25.4-9.8 36 4.9 8.4 8.2 17.9 9.3 28l.4 8c0 37.3-28.3 67.9-64.6 71.6l-7.4 .4-109.7 0 14.1 30 3.1 7.6c12.5 35.7-1.8 75.5-34.2 95l-7.2 3.9c-37.5 17.6-81.7 3.6-102.6-31.2l-.6-.9-2.7-5-.6-1.2-30.1-64c-9.4 17.8-28 29.9-49.5 29.9l-32 0c-30.9 0-56-25.1-56-56L0 152c0-30.9 25.1-56 56-56l32 0c12.4 0 23.9 4.1 33.2 11 13.2-21.4 32-39.4 55-51.6l12.2-6.5 .7-.3 6.6-3.2 .7-.3 7.1-3c16.7-6.6 34.5-9.9 52.6-9.9L360 32zM255.9 80c-12 0-23.9 2.3-35.1 6.6l-4.7 2-5.3 2.6 0 0-12.2 6.5c-29.2 15.5-48.3 44.9-50.7 77.6l-.2 8 0 112.9 .1 4.1c.5 8.2 2.5 16.2 6 23.7l56.8 120.9 2.1 3.8c8.4 13.7 26 19.1 40.8 12.2l2.9-1.6c13-7.8 18.7-23.7 13.7-38l-1.2-3-30.2-64.2c-3.5-7.4-2.9-16.1 1.5-23.1s12-11.1 20.2-11.1l147.5 0 2.4-.1c11.3-1.1 20.3-10.1 21.4-21.4l.1-2.5c0-7.1-3.1-13.5-8.2-18-5.2-4.6-8.2-11.1-8.2-18s3-13.4 8.2-18c4.4-3.9 7.4-9.3 8-15.3l.2-2.7c0-8.4-4.4-15.9-11.2-20.2-10.7-6.9-14.2-20.9-8-32 1.5-2.6 2.5-5.6 2.9-8.6l.2-3.2c0-10.6-6.9-19.6-16.6-22.8-11.7-3.8-18.7-15.9-16-28 .2-.9 .3-1.8 .4-2.6l.2-2.6c0-12.4-9.5-22.6-21.6-23.8L360 80 255.9 80zM56 144c-4.4 0-8 3.6-8 8l0 224c0 4.4 3.6 8 8 8l32 0c4.4 0 8-3.6 8-8l0-224c0-4.4-3.6-8-8-8l-32 0z"],
    "id-badge": [384, 512, [], "f2c1", "M256 48l0 16c0 17.7-14.3 32-32 32l-64 0c-17.7 0-32-14.3-32-32l0-16-64 0c-8.8 0-16 7.2-16 16l0 384c0 8.8 7.2 16 16 16l256 0c8.8 0 16-7.2 16-16l0-384c0-8.8-7.2-16-16-16l-64 0zM0 64C0 28.7 28.7 0 64 0L320 0c35.3 0 64 28.7 64 64l0 384c0 35.3-28.7 64-64 64L64 512c-35.3 0-64-28.7-64-64L0 64zM160 320l64 0c44.2 0 80 35.8 80 80 0 8.8-7.2 16-16 16L96 416c-8.8 0-16-7.2-16-16 0-44.2 35.8-80 80-80zm-24-96a56 56 0 1 1 112 0 56 56 0 1 1 -112 0z"],
    "square-check": [448, 512, [9745, 9989, 61510, "check-square"], "f14a", "M64 80c-8.8 0-16 7.2-16 16l0 320c0 8.8 7.2 16 16 16l320 0c8.8 0 16-7.2 16-16l0-320c0-8.8-7.2-16-16-16L64 80zM0 96C0 60.7 28.7 32 64 32l320 0c35.3 0 64 28.7 64 64l0 320c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 96zM308.4 204.7l-80 128c-4.2 6.7-11.4 10.9-19.3 11.3s-15.5-3.2-20.2-9.6l-48-64c-8-10.6-5.8-25.6 4.8-33.6s25.6-5.8 33.6 4.8l27 36 61.4-98.3c7-11.2 21.8-14.7 33.1-7.6s14.7 21.8 7.6 33.1z"],
    "chess-bishop": [320, 512, [9821], "f43a", "M216 0c13.3 0 24 10.7 24 24s-10.7 24-24 24l-16 0 81.8 98.1c24.7 29.6 38.2 67 38.2 105.6 0 43.7-17.4 85.7-48.3 116.6l-8.6 8.6 46.5 58.2c6.7 8.4 10.4 18.8 10.4 29.6 0 26.2-21.2 47.4-47.4 47.4L47.4 512C21.2 512 0 490.8 0 464.6 0 453.9 3.7 443.4 10.4 435l46.5-58.2-8.6-8.6C17.4 337.4 0 295.4 0 251.7 0 213.1 13.5 175.8 38.2 146.1L120 48 104 48C90.7 48 80 37.3 80 24S90.7 0 104 0L216 0zM94.4 406.8l-45.7 57.2 222.7 0-45.7-57.1-5.5-6.9-120.3 0-5.5 6.8zM156.9 78.7L75.1 176.8c-15.3 18.4-24.6 41-26.7 64.7L48 251.7c0 31 12.3 60.7 34.2 82.7l17.7 17.7 120.2 0c6.2-6.2 12.1-12.1 17.8-17.7 21.9-21.9 34.2-51.6 34.2-82.6l-.4-10.2c-1.5-17-6.7-33.3-15.2-48L209 241c-9.4 9.4-24.6 9.4-33.9 0s-9.4-24.6 0-33.9l51.8-51.8-63.7-76.5-3.1-3.8-3.1 3.8z"],
    "envelope-open": [512, 512, [62135], "f2b6", "M512 416c0 35.3-28.5 64-63.9 64L64 480c-35.4 0-64-28.7-64-64L0 164c.1-15.5 7.8-30 20.5-38.8L206-2.7c30.1-20.7 69.8-20.7 99.9 0L491.5 125.2c12.8 8.8 20.4 23.3 20.5 38.8l0 252zM64 432l384.1 0c8.8 0 15.9-7.1 15.9-16l0-191.7-154.8 117.4c-31.4 23.9-74.9 23.9-106.4 0L48 224.3 48 416c0 8.9 7.2 16 16 16zM463.6 164.4L278.7 36.8c-13.7-9.4-31.7-9.4-45.4 0L48.4 164.4 231.8 303.5c14.3 10.8 34.1 10.8 48.4 0L463.6 164.4z"],
    "circle-xmark": [512, 512, [61532, "times-circle", "xmark-circle"], "f057", "M256 48a208 208 0 1 1 0 416 208 208 0 1 1 0-416zm0 464a256 256 0 1 0 0-512 256 256 0 1 0 0 512zM167 167c-9.4 9.4-9.4 24.6 0 33.9l55 55-55 55c-9.4 9.4-9.4 24.6 0 33.9s24.6 9.4 33.9 0l55-55 55 55c9.4 9.4 24.6 9.4 33.9 0s9.4-24.6 0-33.9l-55-55 55-55c9.4-9.4 9.4-24.6 0-33.9s-24.6-9.4-33.9 0l-55 55-55-55c-9.4-9.4-24.6-9.4-33.9 0z"],
    "square-caret-up": [448, 512, ["caret-square-up"], "f151", "M64 80c-8.8 0-16 7.2-16 16l0 320c0 8.8 7.2 16 16 16l320 0c8.8 0 16-7.2 16-16l0-320c0-8.8-7.2-16-16-16L64 80zM0 96C0 60.7 28.7 32 64 32l320 0c35.3 0 64 28.7 64 64l0 320c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 96zm224 64c6.7 0 13 2.8 17.6 7.7l104 112c6.5 7 8.2 17.2 4.4 25.9S337.5 320 328 320l-208 0c-9.5 0-18.2-5.7-22-14.4s-2.1-18.9 4.4-25.9l104-112c4.5-4.9 10.9-7.7 17.6-7.7z"],
    "file-image": [384, 512, [128443], "f1c5", "M176 48L64 48c-8.8 0-16 7.2-16 16l0 384c0 8.8 7.2 16 16 16l256 0c8.8 0 16-7.2 16-16l0-240-88 0c-39.8 0-72-32.2-72-72l0-88zM316.1 160L224 67.9 224 136c0 13.3 10.7 24 24 24l68.1 0zM0 64C0 28.7 28.7 0 64 0L197.5 0c17 0 33.3 6.7 45.3 18.7L365.3 141.3c12 12 18.7 28.3 18.7 45.3L384 448c0 35.3-28.7 64-64 64L64 512c-35.3 0-64-28.7-64-64L0 64zM259.4 432l-134.8 0c-15.8 0-28.6-12.8-28.6-28.6 0-6.4 2.1-12.5 6-17.6l67.6-86.9C175 292 183.3 288 192 288s17 4 22.4 10.9L282 385.9c3.9 5 6 11.2 6 17.6 0 15.8-12.8 28.6-28.6 28.6zM112 224a32 32 0 1 1 0 64 32 32 0 1 1 0-64z"],
    "square-caret-right": [448, 512, ["caret-square-right"], "f152", "M400 96c0-8.8-7.2-16-16-16L64 80c-8.8 0-16 7.2-16 16l0 320c0 8.8 7.2 16 16 16l320 0c8.8 0 16-7.2 16-16l0-320zM384 32c35.3 0 64 28.7 64 64l0 320c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 96C0 60.7 28.7 32 64 32l320 0zM320 256c0 6.7-2.8 13-7.7 17.6l-112 104c-7 6.5-17.2 8.2-25.9 4.4S160 369.5 160 360l0-208c0-9.5 5.7-18.2 14.4-22s18.9-2.1 25.9 4.4l112 104c4.9 4.5 7.7 10.9 7.7 17.6z"],
    "sun": [576, 512, [9728], "f185", "M200.6-7.9c-6.7-4.4-15.1-5.2-22.5-2.2S165.4-.5 163.9 7.3L143 110.6 39.7 131.4c-7.8 1.6-14.4 7-17.4 14.3s-2.2 15.8 2.2 22.5L82.7 256 24.5 343.8c-4.4 6.7-5.2 15.1-2.2 22.5s9.6 12.8 17.4 14.3L143 401.4 163.9 504.7c1.6 7.8 7 14.4 14.3 17.4s15.8 2.2 22.5-2.2l87.8-58.2 87.8 58.2c6.7 4.4 15.1 5.2 22.5 2.2s12.8-9.6 14.3-17.4l20.9-103.2 103.2-20.9c7.8-1.6 14.4-7 17.4-14.3s2.2-15.8-2.2-22.5l-58.2-87.8 58.2-87.8c4.4-6.7 5.2-15.1 2.2-22.5s-9.6-12.8-17.4-14.3L433.8 110.6 413 7.3C411.4-.5 406-7 398.6-10.1s-15.8-2.2-22.5 2.2L288.4 50.3 200.6-7.9zM186.9 135.7l17-83.9 71.3 47.3c8 5.3 18.5 5.3 26.5 0l71.3-47.3 17 83.9c1.9 9.5 9.3 16.8 18.8 18.8l83.9 17-47.3 71.3c-5.3 8-5.3 18.5 0 26.5l47.3 71.3-83.9 17c-9.5 1.9-16.9 9.3-18.8 18.8l-17 83.9-71.3-47.3c-8-5.3-18.5-5.3-26.5 0l-71.3 47.3-17-83.9c-1.9-9.5-9.3-16.9-18.8-18.8l-83.9-17 47.3-71.3c5.3-8 5.3-18.5 0-26.5l-47.3-71.3 83.9-17c9.5-1.9 16.8-9.3 18.8-18.8zM239.6 256a48.4 48.4 0 1 1 96.8 0 48.4 48.4 0 1 1 -96.8 0zm144.8 0a96.4 96.4 0 1 0 -192.8 0 96.4 96.4 0 1 0 192.8 0z"],
    "image": [448, 512, [], "f03e", "M64 80c-8.8 0-16 7.2-16 16l0 320c0 8.8 7.2 16 16 16l320 0c8.8 0 16-7.2 16-16l0-320c0-8.8-7.2-16-16-16L64 80zM0 96C0 60.7 28.7 32 64 32l320 0c35.3 0 64 28.7 64 64l0 320c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 96zm128 32a32 32 0 1 1 0 64 32 32 0 1 1 0-64zm136 72c8.5 0 16.4 4.5 20.7 11.8l80 136c4.4 7.4 4.4 16.6 .1 24.1S352.6 384 344 384l-240 0c-8.9 0-17.2-5-21.3-12.9s-3.5-17.5 1.6-24.8l56-80c4.5-6.4 11.8-10.2 19.7-10.2s15.2 3.8 19.7 10.2l17.2 24.6 46.5-79c4.3-7.3 12.2-11.8 20.7-11.8z"],
    "lightbulb": [384, 512, [128161], "f0eb", "M296.5 291.1C321 265.2 336 230.4 336 192 336 112.5 271.5 48 192 48S48 112.5 48 192c0 38.4 15 73.2 39.5 99.1 21.3 22.4 44.9 54 53.3 92.9l102.4 0c8.4-39 32-70.5 53.3-92.9zm34.8 33C307.7 349 288 379.4 288 413.7l0 18.3c0 44.2-35.8 80-80 80l-32 0c-44.2 0-80-35.8-80-80l0-18.3C96 379.4 76.3 349 52.7 324.1 20 289.7 0 243.2 0 192 0 86 86 0 192 0S384 86 384 192c0 51.2-20 97.7-52.7 132.1zM144 184c0 13.3-10.7 24-24 24s-24-10.7-24-24c0-48.6 39.4-88 88-88 13.3 0 24 10.7 24 24s-10.7 24-24 24c-22.1 0-40 17.9-40 40z"],
    "address-card": [576, 512, [62140, "contact-card", "vcard"], "f2bb", "M512 80c8.8 0 16 7.2 16 16l0 320c0 8.8-7.2 16-16 16L64 432c-8.8 0-16-7.2-16-16L48 96c0-8.8 7.2-16 16-16l448 0zM64 32C28.7 32 0 60.7 0 96L0 416c0 35.3 28.7 64 64 64l448 0c35.3 0 64-28.7 64-64l0-320c0-35.3-28.7-64-64-64L64 32zM208 248a56 56 0 1 0 0-112 56 56 0 1 0 0 112zm-32 40c-44.2 0-80 35.8-80 80 0 8.8 7.2 16 16 16l192 0c8.8 0 16-7.2 16-16 0-44.2-35.8-80-80-80l-64 0zM376 144c-13.3 0-24 10.7-24 24s10.7 24 24 24l80 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-80 0zm0 96c-13.3 0-24 10.7-24 24s10.7 24 24 24l80 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-80 0z"],
    "face-meh": [512, 512, [128528, "meh"], "f11a", "M464 256a208 208 0 1 1 -416 0 208 208 0 1 1 416 0zM256 0a256 256 0 1 0 0 512 256 256 0 1 0 0-512zM176 240a32 32 0 1 0 0-64 32 32 0 1 0 0 64zm192-32a32 32 0 1 0 -64 0 32 32 0 1 0 64 0zM184 320c-13.3 0-24 10.7-24 24s10.7 24 24 24l144 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-144 0z"],
    "map": [512, 512, [128506, 62072], "f279", "M512 48c0-8.3-4.3-16-11.3-20.4s-15.9-4.8-23.3-1.1L352.5 88.1 180 29.4c-13.7-4.7-28.7-3.8-41.9 2.3L13.8 90.3C5.4 94.2 0 102.7 0 112L0 464c0 8.2 4.2 15.9 11.1 20.3s15.6 4.9 23.1 1.4l127.3-59.9 170.7 56.9c13.7 4.6 28.5 3.7 41.6-2.5l124.4-58.5c8.4-4 13.8-12.4 13.8-21.7l0-352zM144 82.1l0 299-96 45.2 0-299 96-45.2zm48 303.3l0-301.1 128 43.5 0 300.3-128-42.7zM368 134l96-47.4 0 298.2-96 45.2 0-296z"],
    "hand-point-down": [384, 512, [], "f0a7", "M64 448l0-177.6c5.2 1 10.5 1.6 16 1.6l16 0 0 176c0 8.8-7.2 16-16 16s-16-7.2-16-16zM80 224c-17.7 0-32-14.3-32-32l0-24c0-66.3 53.7-120 120-120l48 0c52.5 0 97.1 33.7 113.4 80.7-3.1-.5-6.2-.7-9.4-.7-20 0-37.9 9.2-49.7 23.6-9-4.9-19.4-7.6-30.3-7.6-15.1 0-29 5.3-40 14-11-8.8-24.9-14-40-14l-40 0c-13.3 0-24 10.7-24 24s10.7 24 24 24l40 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-80 0zM0 192l0 0c0 18 6 34.6 16 48l0 208c0 35.3 28.7 64 64 64s64-28.7 64-64l0-82c5.1 1.3 10.5 2 16 2 25.3 0 47.2-14.7 57.6-36 7 2.6 14.5 4 22.4 4 20 0 37.9-9.2 49.7-23.6 9 4.9 19.4 7.6 30.3 7.6 35.3 0 64-28.7 64-64l0-88C384 75.2 308.8 0 216 0L168 0C75.2 0 0 75.2 0 168l0 24zm336 64c0 8.8-7.2 16-16 16s-16-7.2-16-16l0-64c0-8.8 7.2-16 16-16s16 7.2 16 16l0 64zM160 272c5.5 0 10.9-.7 16-2l0 34c0 8.8-7.2 16-16 16s-16-7.2-16-16l0-32 16 0zm64-24l0-40c0-8.8 7.2-16 16-16s16 7.2 16 16l0 64c0 8.8-7.2 16-16 16s-16-7.2-16-16l0-24z"],
    "face-meh-blank": [512, 512, [128566, "meh-blank"], "f5a4", "M256 48a208 208 0 1 0 0 416 208 208 0 1 0 0-416zM512 256a256 256 0 1 1 -512 0 256 256 0 1 1 512 0zM144 208a32 32 0 1 1 64 0 32 32 0 1 1 -64 0zm192-32a32 32 0 1 1 0 64 32 32 0 1 1 0-64z"],
    "face-grin-tongue": [512, 512, [128539, "grin-tongue"], "f589", "M464 256c0-114.9-93.1-208-208-208S48 141.1 48 256c0 74.1 38.8 139.2 97.1 176-.7-5.2-1.1-10.6-1.1-16l0-53.5c-10.2-12.6-18.3-26.9-23.8-42.4-4.1-11.6 7.8-21.4 19.6-17.8 34.7 10.6 74.2 16.5 116.1 16.5 42 0 81.5-6 116.3-16.6 11.8-3.6 23.7 6.1 19.6 17.8-5.5 15.6-13.6 29.9-23.8 42.5l0 53.5c0 5.4-.4 10.8-1.1 16 58.4-36.8 97.1-101.9 97.1-176zm48 0c0 116.3-77.6 214.6-183.9 245.7-19.5 16.4-44.6 26.3-72.1 26.3s-52.6-9.9-72.1-26.3C77.6 470.6 0 372.3 0 256 0 114.6 114.6 0 256 0S512 114.6 512 256zM176 176a32 32 0 1 1 0 64 32 32 0 1 1 0-64zm128 32a32 32 0 1 1 64 0 32 32 0 1 1 -64 0zm16 208l0-37.4c0-14.7-11.9-26.6-26.6-26.6l-2 0c-11.3 0-21.1 7.9-23.6 18.9-2.8 12.6-20.8 12.6-23.6 0-2.5-11.1-12.3-18.9-23.6-18.9l-2 0c-14.7 0-26.6 11.9-26.6 26.6l0 37.4c0 35.3 28.7 64 64 64s64-28.7 64-64z"],
    "futbol": [512, 512, [9917, "futbol-ball", "soccer-ball"], "f1e3", "M387 228.3c-4.4-2.8-7.6-7-9.2-11.9s-1.4-10.2 .5-15L411.6 118c-19.9-22.4-44.6-40.5-72.4-52.7l-69.1 57.6c-4 3.3-9 5.1-14.1 5.1s-10.2-1.8-14.1-5.1L172.8 65.3c-27.8 12.2-52.5 30.3-72.4 52.7l33.4 83.4c1.9 4.8 2.1 10.1 .5 15s-4.9 9.1-9.2 11.9L49 276.2c3 30.9 12.7 59.7 27.6 85.2l89.7-6c5.2-.3 10.3 1.1 14.5 4.2s7.2 7.4 8.4 12.5l22 87.2c14.4 3.2 29.4 4.8 44.8 4.8s30.3-1.7 44.8-4.8l22-87.2c1.3-5 4.2-9.4 8.4-12.5s9.3-4.5 14.5-4.2l89.7 6c15-25.4 24.7-54.3 27.6-85.1L387 228.3zM256 0a256 256 0 1 1 0 512 256 256 0 1 1 0-512zm62 221c8.4 6.1 11.9 16.9 8.7 26.8l-18.3 56.3c-3.2 9.9-12.4 16.6-22.8 16.6l-59.2 0c-10.4 0-19.6-6.7-22.8-16.6l-18.3-56.3c-3.2-9.9 .3-20.7 8.7-26.8l47.9-34.8c8.4-6.1 19.8-6.1 28.2 0L318 221z"],
    "face-surprise": [512, 512, [128558, "surprise"], "f5c2", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm176-80a32 32 0 1 1 0 64 32 32 0 1 1 0-64zm128 32a32 32 0 1 1 64 0 32 32 0 1 1 -64 0zm-48 80a64 64 0 1 1 0 128 64 64 0 1 1 0-128z"],
    "folder": [512, 512, [128193, 128447, 61716, "folder-blank"], "f07b", "M64 400l384 0c8.8 0 16-7.2 16-16l0-240c0-8.8-7.2-16-16-16l-149.3 0c-17.3 0-34.2-5.6-48-16L212.3 83.2c-2.8-2.1-6.1-3.2-9.6-3.2L64 80c-8.8 0-16 7.2-16 16l0 288c0 8.8 7.2 16 16 16zm384 48L64 448c-35.3 0-64-28.7-64-64L0 96C0 60.7 28.7 32 64 32l138.7 0c13.8 0 27.3 4.5 38.4 12.8l38.4 28.8c5.5 4.2 12.3 6.4 19.2 6.4L448 80c35.3 0 64 28.7 64 64l0 240c0 35.3-28.7 64-64 64z"],
    "cloud": [576, 512, [9729], "f0c2", "M80 192c0-88.4 71.6-160 160-160 47.1 0 89.4 20.4 118.7 52.7 10.6-3.1 21.8-4.7 33.3-4.7 66.3 0 120 53.7 120 120 0 13.2-2.1 25.9-6.1 37.8 41.6 21.1 70.1 64.3 70.1 114.2 0 70.7-57.3 128-128 128l-304 0c-79.5 0-144-64.5-144-144 0-56.8 32.9-105.9 80.7-129.4-.4-4.8-.7-9.7-.7-14.6zM240 80c-61.9 0-112 50.1-112 112 0 8.4 .9 16.6 2.7 24.5 2.7 12.1-4.3 24.3-16.1 28.1-38.7 12.4-66.6 48.7-66.6 91.4 0 53 43 96 96 96l304 0c44.2 0 80-35.8 80-80 0-37.4-25.7-68.9-60.5-77.6-7.5-1.9-13.6-7.2-16.5-14.3s-2.1-15.2 2-21.7c7-11.1 11-24.2 11-38.3 0-39.8-32.2-72-72-72-11.1 0-21.5 2.5-30.8 6.9-10.5 5-23.1 1.7-29.8-7.8-20.3-28.6-53.7-47.1-91.3-47.1z"],
    "circle": [512, 512, [128308, 128309, 128992, 128993, 128994, 128995, 128996, 9679, 9898, 9899, 11044, 61708, 61915], "f111", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0z"],
    "face-grin-squint": [512, 512, [128518, "grin-squint"], "f585", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm372.2 46.3c11.8-3.6 23.7 6.1 19.6 17.8-19.8 55.9-73.1 96-135.8 96-62.7 0-116-40-135.8-95.9-4.1-11.6 7.8-21.4 19.6-17.8 34.7 10.6 74.2 16.5 116.1 16.5 42 0 81.5-6 116.3-16.6zm-249.6-143c4.5-6.8 13.3-9.2 20.6-5.5l79.6 40c5.4 2.7 8.8 8.2 8.8 14.3s-3.4 11.6-8.8 14.3l-79.6 40c-7.3 3.6-16.1 1.3-20.6-5.5s-3.1-15.9 3.1-21.1L159 208 125.8 180.3c-6.2-5.2-7.6-14.3-3.1-21.1zm263.6 21.1L353 208 386.2 235.7c6.2 5.2 7.6 14.3 3.1 21.1s-13.3 9.2-20.6 5.5l-79.6-40c-5.4-2.7-8.8-8.2-8.8-14.3s3.4-11.6 8.8-14.3l79.6-40c7.3-3.6 16.1-1.3 20.6 5.5s3.1 15.9-3.1 21.1z"],
    "circle-user": [512, 512, [62142, "user-circle"], "f2bd", "M406.5 399.6C387.4 352.9 341.5 320 288 320l-64 0c-53.5 0-99.4 32.9-118.5 79.6-35.6-37.3-57.5-87.9-57.5-143.6 0-114.9 93.1-208 208-208s208 93.1 208 208c0 55.7-21.9 106.2-57.5 143.6zm-40.1 32.7C334.4 452.4 296.6 464 256 464s-78.4-11.6-110.5-31.7c7.3-36.7 39.7-64.3 78.5-64.3l64 0c38.8 0 71.2 27.6 78.5 64.3zM256 512a256 256 0 1 0 0-512 256 256 0 1 0 0 512zm0-272a40 40 0 1 1 0-80 40 40 0 1 1 0 80zm-88-40a88 88 0 1 0 176 0 88 88 0 1 0 -176 0z"],
    "rectangle-list": [512, 512, ["list-alt"], "f022", "M64 112c-8.8 0-16 7.2-16 16l0 256c0 8.8 7.2 16 16 16l384 0c8.8 0 16-7.2 16-16l0-256c0-8.8-7.2-16-16-16L64 112zM0 128C0 92.7 28.7 64 64 64l384 0c35.3 0 64 28.7 64 64l0 256c0 35.3-28.7 64-64 64L64 448c-35.3 0-64-28.7-64-64L0 128zM160 320a32 32 0 1 1 -64 0 32 32 0 1 1 64 0zm-32-96a32 32 0 1 1 0-64 32 32 0 1 1 0 64zm104-56l160 0c13.3 0 24 10.7 24 24s-10.7 24-24 24l-160 0c-13.3 0-24-10.7-24-24s10.7-24 24-24zm0 128l160 0c13.3 0 24 10.7 24 24s-10.7 24-24 24l-160 0c-13.3 0-24-10.7-24-24s10.7-24 24-24z"],
    "hand": [512, 512, [129306, 9995, "hand-paper"], "f256", "M256.5 0c-25.3 0-47.2 14.7-57.6 36-7-2.6-14.5-4-22.4-4-35.3 0-64 28.7-64 64l0 165.5-2.7-2.7c-25-25-65.5-25-90.5 0s-25 65.5 0 90.5L107 437c48 48 113.1 75 181 75l16.5 0c1.5 0 3-.1 4.5-.4 91.7-6.2 165-79.4 171.1-171.1 .3-1.5 .4-3 .4-4.5l0-176c0-35.3-28.7-64-64-64-5.5 0-10.9 .7-16 2l0-2c0-35.3-28.7-64-64-64-7.9 0-15.4 1.4-22.4 4-10.4-21.3-32.3-36-57.6-36zm-16 96.1l0-.1 0-32c0-8.8 7.2-16 16-16s16 7.2 16 16l0 168c0 13.3 10.7 24 24 24s24-10.7 24-24l0-136c0-8.8 7.2-16 16-16s16 7.2 16 16l0 136c0 13.3 10.7 24 24 24s24-10.7 24-24l0-72c0-8.8 7.2-16 16-16s16 7.2 16 16l0 172.9c-.1 .6-.1 1.3-.2 1.9-3.4 69.7-59.3 125.6-129 129-.6 0-1.3 .1-1.9 .2L288 464C232.9 464 180 442.1 141 403.1L53.2 315.3c-6.2-6.2-6.2-16.4 0-22.6s16.4-6.2 22.6 0l43.7 43.7c6.9 6.9 17.2 8.9 26.2 5.2s14.8-12.5 14.8-22.2l0-223.4c0-8.8 7.2-16 16-16 8.8 0 16 7.1 16 15.9l0 136.1c0 13.3 10.7 24 24 24s24-10.7 24-24l0-135.9z"],
    "thumbs-up": [512, 512, [128077, 61575], "f164", "M171.5 38.8C192.3 4 236.5-10 274 7.6l7.2 3.8C316 32.3 330 76.5 312.4 114l0 0-14.1 30 109.7 0 7.4 .4c36.3 3.7 64.6 34.4 64.6 71.6 0 13.2-3.6 25.4-9.8 36 6.1 10.6 9.7 22.8 9.8 36 0 18.3-6.9 34.8-18 47.5 1.3 5.3 2 10.8 2 16.5 0 25.1-12.9 47-32.2 59.9-1.9 35.5-29.4 64.2-64.4 67.7l-7.4 .4-104.1 0c-18 0-35.9-3.4-52.6-9.9l-7.1-3-.7-.3-6.6-3.2-.7-.3-12.2-6.5c-12.3-6.5-23.3-14.7-32.9-24.1-4.1 26.9-27.3 47.4-55.3 47.4l-32 0c-30.9 0-56-25.1-56-56L0 200c0-30.9 25.1-56 56-56l32 0c10.8 0 20.9 3.1 29.5 8.5l50.1-106.5 .6-1.2 2.7-5 .6-.9zM56 192c-4.4 0-8 3.6-8 8l0 224c0 4.4 3.6 8 8 8l32 0c4.4 0 8-3.6 8-8l0-224c0-4.4-3.6-8-8-8l-32 0zM253.6 51c-14.8-6.9-32.3-1.6-40.7 12l-2.2 4-56.8 120.9c-3.5 7.5-5.5 15.5-6 23.7l-.1 4.2 0 112.9 .2 7.9c2.4 32.7 21.4 62.1 50.7 77.7l11.5 6.1 6.3 3.1c12.4 5.6 25.8 8.5 39.4 8.5l104.1 0 2.4-.1c12.1-1.2 21.6-11.5 21.6-23.9l-.2-2.6c-.1-.9-.2-1.7-.4-2.6-2.7-12.1 4.3-24.2 16-28 9.7-3.1 16.6-12.2 16.6-22.8 0-4.3-1.1-8.2-3.1-11.8-6.3-11.1-2.8-25.2 8-32 6.8-4.3 11.2-11.8 11.2-20.2 0-7.1-3.1-13.5-8.2-18-5.2-4.6-8.2-11.1-8.2-18s3-13.4 8.2-18c5.1-4.5 8.2-10.9 8.2-18l-.1-2.4c-1.1-11.3-10.1-20.3-21.4-21.4l-2.4-.1-147.5 0c-8.2 0-15.8-4.2-20.2-11.1-4.4-6.9-5-15.7-1.5-23.1L269 93.6c7-15 1.4-32.7-12.5-41L253.6 51z"],
    "building": [384, 512, [127970, 61687], "f1ad", "M64 48c-8.8 0-16 7.2-16 16l0 384c0 8.8 7.2 16 16 16l80 0 0-80c0-17.7 14.3-32 32-32l32 0c17.7 0 32 14.3 32 32l0 80 80 0c8.8 0 16-7.2 16-16l0-384c0-8.8-7.2-16-16-16L64 48zM0 64C0 28.7 28.7 0 64 0L320 0c35.3 0 64 28.7 64 64l0 384c0 35.3-28.7 64-64 64L64 512c-35.3 0-64-28.7-64-64L0 64zm96 48c0-8.8 7.2-16 16-16l32 0c8.8 0 16 7.2 16 16l0 32c0 8.8-7.2 16-16 16l-32 0c-8.8 0-16-7.2-16-16l0-32zM240 96l32 0c8.8 0 16 7.2 16 16l0 32c0 8.8-7.2 16-16 16l-32 0c-8.8 0-16-7.2-16-16l0-32c0-8.8 7.2-16 16-16zM96 240c0-8.8 7.2-16 16-16l32 0c8.8 0 16 7.2 16 16l0 32c0 8.8-7.2 16-16 16l-32 0c-8.8 0-16-7.2-16-16l0-32zm144-16l32 0c8.8 0 16 7.2 16 16l0 32c0 8.8-7.2 16-16 16l-32 0c-8.8 0-16-7.2-16-16l0-32c0-8.8 7.2-16 16-16z"],
    "chess-rook": [384, 512, [9820], "f447", "M352 0c17.7 0 32 14.3 32 32l0 138.7c0 13.8-4.5 27.3-12.8 38.4l-35.2 46.9 0 112 40.8 68.1c4.7 7.8 7.2 16.7 7.2 25.8 0 27.7-22.4 50.1-50.1 50.1L50.1 512c-27.7 0-50.1-22.4-50.1-50.1 0-9.1 2.5-18 7.2-25.8L48 368 48 256 12.8 209.1C4.5 198 0 184.5 0 170.7L0 32C0 14.3 14.3 0 32 0L352 0zM48.3 460.8l-.3 1.1c0 1.2 1 2.1 2.1 2.1l283.8 0c1.2 0 2.1-1 2.1-2.1l-.3-1.1-36.5-60.8-214.4 0-36.5 60.8zM48 170.7c0 2.6 .6 5.1 1.8 7.4l1.4 2.2 0 0 35.2 46.9 9.6 12.8 0 112 192 0 0-112 9.6-12.8 35.2-46.9 0 0 1.4-2.2c1.2-2.3 1.8-4.8 1.8-7.4l0-122.7-64 0 0 24c0 13.3-10.7 24-24 24s-24-10.7-24-24l0-24-64 0 0 24c0 13.3-10.7 24-24 24s-24-10.7-24-24l0-24-64 0 0 122.7z"],
    "circle-question": [512, 512, [62108, "question-circle"], "f059", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm256-80c-17.7 0-32 14.3-32 32 0 13.3-10.7 24-24 24s-24-10.7-24-24c0-44.2 35.8-80 80-80s80 35.8 80 80c0 47.2-36 67.2-56 74.5l0 3.8c0 13.3-10.7 24-24 24s-24-10.7-24-24l0-8.1c0-20.5 14.8-35.2 30.1-40.2 6.4-2.1 13.2-5.5 18.2-10.3 4.3-4.2 7.7-10 7.7-19.6 0-17.7-14.3-32-32-32zM224 368a32 32 0 1 1 64 0 32 32 0 1 1 -64 0z"],
    "file": [384, 512, [128196, 128459, 61462], "f15b", "M176 48L64 48c-8.8 0-16 7.2-16 16l0 384c0 8.8 7.2 16 16 16l256 0c8.8 0 16-7.2 16-16l0-240-88 0c-39.8 0-72-32.2-72-72l0-88zM316.1 160L224 67.9 224 136c0 13.3 10.7 24 24 24l68.1 0zM0 64C0 28.7 28.7 0 64 0L197.5 0c17 0 33.3 6.7 45.3 18.7L365.3 141.3c12 12 18.7 28.3 18.7 45.3L384 448c0 35.3-28.7 64-64 64L64 512c-35.3 0-64-28.7-64-64L0 64z"],
    "face-sad-cry": [512, 512, [128557, "sad-cry"], "f5b3", "M400 406.1L400 288c0-13.3-10.7-24-24-24s-24 10.7-24 24l0 152.6c-28.7 15-61.4 23.4-96 23.4s-67.3-8.5-96-23.4L160 288c0-13.3-10.7-24-24-24s-24 10.7-24 24l0 118.1C72.6 368.2 48 315 48 256 48 141.1 141.1 48 256 48s208 93.1 208 208c0 59-24.6 112.2-64 150.1zM256 512a256 256 0 1 0 0-512 256 256 0 1 0 0 512zM152 196l16 0c11 0 20 9 20 20s9 20 20 20 20-9 20-20c0-33.1-26.9-60-60-60l-16 0c-33.1 0-60 26.9-60 60 0 11 9 20 20 20s20-9 20-20 9-20 20-20zm172 20c0-11 9-20 20-20l16 0c11 0 20 9 20 20s9 20 20 20 20-9 20-20c0-33.1-26.9-60-60-60l-16 0c-33.1 0-60 26.9-60 60 0 11 9 20 20 20s20-9 20-20zM208 336l0 32c0 26.5 21.5 48 48 48s48-21.5 48-48l0-32c0-26.5-21.5-48-48-48s-48 21.5-48 48z"],
    "calendar-minus": [448, 512, [], "f272", "M120 0c13.3 0 24 10.7 24 24l0 40 160 0 0-40c0-13.3 10.7-24 24-24s24 10.7 24 24l0 40 32 0c35.3 0 64 28.7 64 64l0 288c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 128C0 92.7 28.7 64 64 64l32 0 0-40c0-13.3 10.7-24 24-24zm0 112l-56 0c-8.8 0-16 7.2-16 16l0 288c0 8.8 7.2 16 16 16l320 0c8.8 0 16-7.2 16-16l0-288c0-8.8-7.2-16-16-16l-264 0zm32 136l144 0c13.3 0 24 10.7 24 24s-10.7 24-24 24l-144 0c-13.3 0-24-10.7-24-24s10.7-24 24-24z"],
    "face-tired": [512, 512, [128555, "tired"], "f5c8", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm176.5 64.3C196.1 302.1 223.8 288 256 288s59.9 14.1 79.5 32.3c19 17.8 32.5 41.7 32.5 63.7 0 5.4-2.7 10.4-7.2 13.4s-10.2 3.4-15.2 1.3l-17.2-7.5c-22.8-10-47.5-15.1-72.4-15.1s-49.6 5.2-72.4 15.1l-17.2 7.5c-4.9 2.2-10.7 1.7-15.2-1.3s-7.2-8-7.2-13.4c0-22 13.5-45.9 32.5-63.7zM122.6 159.2c4.5-6.8 13.3-9.2 20.6-5.5l79.6 40c5.4 2.7 8.8 8.2 8.8 14.3s-3.4 11.6-8.8 14.3l-79.6 40c-7.3 3.6-16.1 1.3-20.6-5.5s-3.1-15.9 3.1-21.1L159 208 125.8 180.3c-6.2-5.2-7.6-14.3-3.1-21.1zm263.6 21.1L353 208 386.2 235.7c6.2 5.2 7.6 14.3 3.1 21.1s-13.3 9.2-20.6 5.5l-79.6-40c-5.4-2.7-8.8-8.2-8.8-14.3s3.4-11.6 8.8-14.3l79.6-40c7.3-3.6 16.1-1.3 20.6 5.5s3.1 15.9-3.1 21.1z"],
    "hand-point-right": [512, 512, [], "f0a4", "M448 128l-177.6 0c1 5.2 1.6 10.5 1.6 16l0 16 176 0c8.8 0 16-7.2 16-16s-7.2-16-16-16zM224 144c0-17.7-14.3-32-32-32l-24 0c-66.3 0-120 53.7-120 120l0 48c0 52.5 33.7 97.1 80.7 113.4-.5-3.1-.7-6.2-.7-9.4 0-20 9.2-37.9 23.6-49.7-4.9-9-7.6-19.4-7.6-30.3 0-15.1 5.3-29 14-40-8.8-11-14-24.9-14-40l0-40c0-13.3 10.7-24 24-24s24 10.7 24 24l0 40c0 8.8 7.2 16 16 16s16-7.2 16-16l0-80zM192 64l0 0c18 0 34.6 6 48 16l208 0c35.3 0 64 28.7 64 64s-28.7 64-64 64l-82 0c1.3 5.1 2 10.5 2 16 0 25.3-14.7 47.2-36 57.6 2.6 7 4 14.5 4 22.4 0 20-9.2 37.9-23.6 49.7 4.9 9 7.6 19.4 7.6 30.3 0 35.3-28.7 64-64 64l-88 0C75.2 448 0 372.8 0 280l0-48C0 139.2 75.2 64 168 64l24 0zm64 336c8.8 0 16-7.2 16-16s-7.2-16-16-16l-64 0c-8.8 0-16 7.2-16 16s7.2 16 16 16l64 0zm16-176c0 5.5-.7 10.9-2 16l34 0c8.8 0 16-7.2 16-16s-7.2-16-16-16l-32 0 0 16zm-24 64l-40 0c-8.8 0-16 7.2-16 16s7.2 16 16 16l64 0c8.8 0 16-7.2 16-16s-7.2-16-16-16l-24 0z"],
    "circle-up": [512, 512, [61467, "arrow-alt-circle-up"], "f35b", "M256 48a208 208 0 1 1 0 416 208 208 0 1 1 0-416zm0 464a256 256 0 1 0 0-512 256 256 0 1 0 0 512zm11.3-387.3c-6.2-6.2-16.4-6.2-22.6 0l-104 104c-4.6 4.6-5.9 11.5-3.5 17.4s8.3 9.9 14.8 9.9l72 0 0 104c0 13.3 10.7 24 24 24l16 0c13.3 0 24-10.7 24-24l0-104 72 0c6.5 0 12.3-3.9 14.8-9.9s1.1-12.9-3.5-17.4l-104-104z"],
    "hand-scissors": [512, 512, [], "f257", "M.2 276.3c-1.2-35.3 26.4-65 61.7-66.2l3.3-.1-8.2-1.8C22.5 200.5 .7 166.3 8.3 131.8S50.2 75.5 84.7 83.2l173 38.3c2.3-2.9 4.6-5.7 7.1-8.5l18.4-20.3C299.9 74.5 323.5 64 348.3 64l10.2 0c54.1 0 104.1 28.7 131.3 75.4l1.5 2.6c13.6 23.2 20.7 49.7 20.7 76.6L512 344c0 66.3-53.7 120-120 120l-104 0c-35.3 0-64-28.7-64-64 0-2.8 .2-5.6 .5-8.3-19.4-11-32.5-31.8-32.5-55.7 0-.8 0-1.6 0-2.4L66.4 338c-35.3 1.2-65-26.4-66.2-61.7zm63.4-18.2c-8.8 .3-15.7 7.7-15.4 16.6s7.7 15.7 16.5 15.4l161.5-5.6c9.8-.3 18.7 5.3 22.7 14.2s2.2 19.3-4.5 26.4c-2.8 2.9-4.4 6.7-4.4 11 0 8.8 7.2 16 16 16 9.1 0 17.4 5.1 21.5 13.3s3.2 17.9-2.3 25.1c-2 2.7-3.2 6-3.2 9.6 0 8.8 7.2 16 16 16l104 0c39.8 0 72-32.2 72-72l0-125.4c0-18.4-4.9-36.5-14.2-52.4l-1.5-2.6c-18.6-32-52.8-51.6-89.8-51.6l-10.2 0c-11.3 0-22 4.8-29.6 13.1l0 0-18.4 20.3c-.6 .6-1.1 1.3-1.7 1.9l57 13.2c8.6 2 14 10.6 12 19.2s-10.6 14-19.2 12L262.8 171.8 74.3 130c-8.6-1.9-17.2 3.5-19.1 12.2s3.5 17.2 12.2 19.1l187.5 41.6c10.2 2.3 17.8 10.9 18.7 21.4l.1 1c.6 6.6-1.5 13.1-5.8 18.1s-10.6 7.9-17.2 8.2L63.6 258.1z"],
    "gem": [512, 512, [128142], "f3a5", "M168.5 72l87.5 93 87.5-93-175 0zM383.9 99.1l-72.3 76.9 129 0-56.6-76.9zm50 124.9L78.1 224 256 420.3 433.9 224zM71.5 176l129 0-72.3-76.9-56.6 76.9zm434.3 40.1l-232 256c-4.5 5-11 7.9-17.8 7.9s-13.2-2.9-17.8-7.9l-232-256c-7.7-8.5-8.3-21.2-1.5-30.4l112-152c4.5-6.1 11.7-9.8 19.3-9.8l240 0c7.6 0 14.8 3.6 19.3 9.8l112 152c6.8 9.2 6.1 21.9-1.5 30.4z"],
    "rectangle-xmark": [512, 512, [62164, "rectangle-times", "times-rectangle", "window-close"], "f410", "M64 112c-8.8 0-16 7.2-16 16l0 256c0 8.8 7.2 16 16 16l384 0c8.8 0 16-7.2 16-16l0-256c0-8.8-7.2-16-16-16L64 112zM0 128C0 92.7 28.7 64 64 64l384 0c35.3 0 64 28.7 64 64l0 256c0 35.3-28.7 64-64 64L64 448c-35.3 0-64-28.7-64-64L0 128zm334.1 49.9c9.4 9.4 9.4 24.6 0 33.9l-44.1 44.1 44.1 44.1c9.4 9.4 9.4 24.6 0 33.9s-24.6 9.4-33.9 0l-44.1-44.1-44.1 44.1c-9.4 9.4-24.6 9.4-33.9 0s-9.4-24.6 0-33.9l44.1-44.1-44.1-44.1c-9.4-9.4-9.4-24.6 0-33.9s24.6-9.4 33.9 0l44.1 44.1 44.1-44.1c9.4-9.4 24.6-9.4 33.9 0z"],
    "trash-can": [448, 512, [61460, "trash-alt"], "f2ed", "M166.2-16c-13.3 0-25.3 8.3-30 20.8L120 48 24 48C10.7 48 0 58.7 0 72S10.7 96 24 96l400 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-96 0-16.2-43.2C307.1-7.7 295.2-16 281.8-16L166.2-16zM32 144l0 304c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-304-48 0 0 304c0 8.8-7.2 16-16 16L96 464c-8.8 0-16-7.2-16-16l0-304-48 0zm160 72c0-13.3-10.7-24-24-24s-24 10.7-24 24l0 176c0 13.3 10.7 24 24 24s24-10.7 24-24l0-176zm112 0c0-13.3-10.7-24-24-24s-24 10.7-24 24l0 176c0 13.3 10.7 24 24 24s24-10.7 24-24l0-176z"],
    "life-ring": [512, 512, [], "f1cd", "M385.1 419.1C349.7 447.2 304.8 464 256 464s-93.7-16.8-129.1-44.9l80.4-80.4c14.3 8.4 31 13.3 48.8 13.3s34.5-4.8 48.8-13.3l80.4 80.4zm68.1 .2C489.9 374.9 512 318.1 512 256S489.9 137.1 453.2 92.7L465 81c9.4-9.4 9.4-24.6 0-33.9s-24.6-9.4-33.9 0L419.3 58.8C374.9 22.1 318.1 0 256 0S137.1 22.1 92.7 58.8L81 47c-9.4-9.4-24.6-9.4-33.9 0S37.7 71.6 47 81L58.8 92.7C22.1 137.1 0 193.9 0 256S22.1 374.9 58.8 419.3L47 431c-9.4 9.4-9.4 24.6 0 33.9s24.6 9.4 33.9 0l11.8-11.8C137.1 489.9 193.9 512 256 512s118.9-22.1 163.3-58.8L431 465c9.4 9.4 24.6 9.4 33.9 0s9.4-24.6 0-33.9l-11.8-11.8zm-34.1-34.1l-80.4-80.4c8.4-14.3 13.3-31 13.3-48.8s-4.8-34.5-13.3-48.8l80.4-80.4C447.2 162.3 464 207.2 464 256s-16.8 93.7-44.9 129.1zM385.1 92.9l-80.4 80.4c-14.3-8.4-31-13.3-48.8-13.3s-34.5 4.8-48.8 13.3L126.9 92.9C162.3 64.8 207.2 48 256 48s93.7 16.8 129.1 44.9zM173.3 304.8L92.9 385.1C64.8 349.7 48 304.8 48 256s16.8-93.7 44.9-129.1l80.4 80.4c-8.4 14.3-13.3 31-13.3 48.8s4.8 34.5 13.3 48.8zM208 256a48 48 0 1 1 96 0 48 48 0 1 1 -96 0z"],
    "copyright": [512, 512, [169], "f1f9", "M256 48a208 208 0 1 1 0 416 208 208 0 1 1 0-416zm0 464a256 256 0 1 0 0-512 256 256 0 1 0 0 512zM205.1 306.9c-28.1-28.1-28.1-73.7 0-101.8s73.7-28.1 101.8 0c9.4 9.4 24.6 9.4 33.9 0s9.4-24.6 0-33.9c-46.9-46.9-122.8-46.9-169.7 0s-46.9 122.8 0 169.7 122.8 46.9 169.7 0c9.4-9.4 9.4-24.6 0-33.9s-24.6-9.4-33.9 0c-28.1 28.1-73.7 28.1-101.8 0z"],
    "circle-left": [512, 512, [61840, "arrow-alt-circle-left"], "f359", "M48 256a208 208 0 1 1 416 0 208 208 0 1 1 -416 0zm464 0a256 256 0 1 0 -512 0 256 256 0 1 0 512 0zM124.7 244.7c-6.2 6.2-6.2 16.4 0 22.6l104 104c4.6 4.6 11.5 5.9 17.4 3.5s9.9-8.3 9.9-14.8l0-72 104 0c13.3 0 24-10.7 24-24l0-16c0-13.3-10.7-24-24-24l-104 0 0-72c0-6.5-3.9-12.3-9.9-14.8s-12.9-1.1-17.4 3.5l-104 104z"],
    "calendar": [448, 512, [128197, 128198], "f133", "M120 0c13.3 0 24 10.7 24 24l0 40 160 0 0-40c0-13.3 10.7-24 24-24s24 10.7 24 24l0 40 32 0c35.3 0 64 28.7 64 64l0 288c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 128C0 92.7 28.7 64 64 64l32 0 0-40c0-13.3 10.7-24 24-24zm0 112l-56 0c-8.8 0-16 7.2-16 16l0 48 352 0 0-48c0-8.8-7.2-16-16-16l-264 0zM48 224l0 192c0 8.8 7.2 16 16 16l320 0c8.8 0 16-7.2 16-16l0-192-352 0z"],
    "face-frown-open": [512, 512, [128550, "frown-open"], "f57a", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zM182.4 382.5c-12.4 5.2-26.5-4.1-21.1-16.4 16-36.6 52.4-62.1 94.8-62.1s78.8 25.6 94.8 62.1c5.4 12.3-8.7 21.6-21.1 16.4-22.4-9.5-47.4-14.8-73.7-14.8s-51.3 5.3-73.7 14.8zM144 208a32 32 0 1 1 64 0 32 32 0 1 1 -64 0zm192-32a32 32 0 1 1 0 64 32 32 0 1 1 0-64z"],
    "chart-bar": [512, 512, ["bar-chart"], "f080", "M48 56c0-13.3-10.7-24-24-24S0 42.7 0 56L0 400c0 44.2 35.8 80 80 80l408 0c13.3 0 24-10.7 24-24s-10.7-24-24-24L80 432c-17.7 0-32-14.3-32-32L48 56zm104 72l208 0c13.3 0 24-10.7 24-24s-10.7-24-24-24L152 80c-13.3 0-24 10.7-24 24s10.7 24 24 24zm0 64c-13.3 0-24 10.7-24 24s10.7 24 24 24l144 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-144 0zm0 112c-13.3 0-24 10.7-24 24s10.7 24 24 24l272 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-272 0z"],
    "house": [512, 512, [127968, 63498, 63500, "home", "home-alt", "home-lg-alt"], "f015", "M240 6.1c9.1-8.2 22.9-8.2 32 0l232 208c9.9 8.8 10.7 24 1.8 33.9s-24 10.7-33.9 1.8l-8-7.2 0 205.3c0 35.3-28.7 64-64 64l-288 0c-35.3 0-64-28.7-64-64l0-205.3-8 7.2c-9.9 8.8-25 8-33.9-1.8s-8-25 1.8-33.9L240 6.1zm16 50.1L96 199.7 96 448c0 8.8 7.2 16 16 16l48 0 0-104c0-39.8 32.2-72 72-72l48 0c39.8 0 72 32.2 72 72l0 104 48 0c8.8 0 16-7.2 16-16l0-248.3-160-143.4zM208 464l96 0 0-104c0-13.3-10.7-24-24-24l-48 0c-13.3 0-24 10.7-24 24l0 104z"],
    "face-frown": [512, 512, [9785, "frown"], "f119", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zM334.7 384.6C319.7 369 293.6 352 256 352s-63.7 17-78.7 32.6c-9.2 9.6-24.4 9.9-33.9 .7s-9.9-24.4-.7-33.9c22.1-23 60-47.4 113.3-47.4s91.2 24.4 113.3 47.4c9.2 9.6 8.9 24.8-.7 33.9s-24.8 8.9-33.9-.7zM144 208a32 32 0 1 1 64 0 32 32 0 1 1 -64 0zm192-32a32 32 0 1 1 0 64 32 32 0 1 1 0-64z"],
    "user": [448, 512, [128100, 62144, 62470, "user-alt", "user-large"], "f007", "M144 128a80 80 0 1 1 160 0 80 80 0 1 1 -160 0zm208 0a128 128 0 1 0 -256 0 128 128 0 1 0 256 0zM48 480c0-70.7 57.3-128 128-128l96 0c70.7 0 128 57.3 128 128l0 8c0 13.3 10.7 24 24 24s24-10.7 24-24l0-8c0-97.2-78.8-176-176-176l-96 0C78.8 304 0 382.8 0 480l0 8c0 13.3 10.7 24 24 24s24-10.7 24-24l0-8z"],
    "snowflake": [512, 512, [10052, 10054], "f2dc", "M280.1-8c0-13.3-10.7-24-24-24s-24 10.7-24 24l0 78.1-23-23c-9.4-9.4-24.6-9.4-33.9 0s-9.4 24.6 0 33.9l57 57 0 76.5-66.2-38.2-20.9-77.8c-3.4-12.8-16.6-20.4-29.4-17S95.2 98 98.7 110.8l8.4 31.5-67.6-39C28 96.6 13.3 100.5 6.7 112S4 138.2 15.5 144.8l67.6 39-31.5 8.4c-12.8 3.4-20.4 16.6-17 29.4s16.6 20.4 29.4 17l77.8-20.9 66.2 38.2-66.2 38.2-77.8-20.9c-12.8-3.4-26 4.2-29.4 17s4.2 26 17 29.4l31.5 8.4-67.6 39C4 373.8 .1 388.5 6.7 400s21.3 15.4 32.8 8.8l67.6-39-8.4 31.5c-3.4 12.8 4.2 26 17 29.4s26-4.2 29.4-17l20.9-77.8 66.2-38.2 0 76.5-57 57c-9.4 9.4-9.4 24.6 0 33.9s24.6 9.4 33.9 0l23-23 0 78.1c0 13.3 10.7 24 24 24s24-10.7 24-24l0-78.1 23 23c9.4 9.4 24.6 9.4 33.9 0s9.4-24.6 0-33.9l-57-57 0-76.5 66.2 38.2 20.9 77.8c3.4 12.8 16.6 20.4 29.4 17s20.4-16.6 17-29.4l-8.4-31.5 67.6 39c11.5 6.6 26.2 2.7 32.8-8.8s2.7-26.2-8.8-32.8l-67.6-39 31.5-8.4c12.8-3.4 20.4-16.6 17-29.4s-16.6-20.4-29.4-17l-77.8 20.9-66.2-38.2 66.2-38.2 77.8 20.9c12.8 3.4 26-4.2 29.4-17s-4.2-26-17-29.4l-31.5-8.4 67.6-39c11.5-6.6 15.4-21.3 8.8-32.8s-21.3-15.4-32.8-8.8l-67.6 39 8.4-31.5c3.4-12.8-4.2-26-17-29.4s-26 4.2-29.4 17l-20.9 77.8-66.2 38.2 0-76.5 57-57c9.4-9.4 9.4-24.6 0-33.9s-24.6-9.4-33.9 0l-23 23 0-78.1z"],
    "bookmark": [384, 512, [128278, 61591], "f02e", "M0 64C0 28.7 28.7 0 64 0L320 0c35.3 0 64 28.7 64 64l0 417.1c0 25.6-28.5 40.8-49.8 26.6L192 412.8 49.8 507.7C28.5 521.9 0 506.6 0 481.1L0 64zM64 48c-8.8 0-16 7.2-16 16l0 387.2 117.4-78.2c16.1-10.7 37.1-10.7 53.2 0L336 451.2 336 64c0-8.8-7.2-16-16-16L64 48z"],
    "square-caret-left": [448, 512, ["caret-square-left"], "f191", "M48 416c0 8.8 7.2 16 16 16l320 0c8.8 0 16-7.2 16-16l0-320c0-8.8-7.2-16-16-16L64 80c-8.8 0-16 7.2-16 16l0 320zm16 64c-35.3 0-64-28.7-64-64L0 96C0 60.7 28.7 32 64 32l320 0c35.3 0 64 28.7 64 64l0 320c0 35.3-28.7 64-64 64L64 480zm64-224c0-6.7 2.8-13 7.7-17.6l112-104c7-6.5 17.2-8.2 25.9-4.4S288 142.5 288 152l0 208c0 9.5-5.7 18.2-14.4 22s-18.9 2.1-25.9-4.4l-112-104c-4.9-4.5-7.7-10.9-7.7-17.6z"],
    "handshake": [640, 512, [129309, 62662, "handshake-alt", "handshake-simple"], "f2b5", "M598.1 75.4c10.7-7.8 13.1-22.8 5.3-33.5s-22.8-13.1-33.5-5.3l-74.5 54.2-9.9-6.6C465.8 71 442.6 64 418.9 64l-59.2 0-.4 0-143.6 0c-26.7 0-52.5 8.9-73.4 25.1L70.1 36.6c-10.7-7.8-25.7-5.4-33.5 5.3s-5.4 25.7 5.3 33.5l88 64c9.6 6.9 22.7 5.9 31.1-2.4l3.9-3.9c13.5-13.5 31.8-21.1 50.9-21.1l46.3 0-91.7 91.7c-15.6 15.6-15.6 40.9 0 56.6l.8 .8C218 308 294 308 340.9 261.1l27.1-27.1 97.8 97.8c15.6 15.6 15.6 40.9 0 56.6l-9.8 9.8-31-31c-9.4-9.4-24.6-9.4-33.9 0s-9.4 24.6 0 33.9l28 28c-17.5 10.4-37.2 16.7-57.6 18.5L313 399c-9.4-9.4-24.6-9.4-33.9 0s-9.4 24.6 0 33.9l15 15-3.8 0c-36.1 0-70.7-14.3-96.2-39.8L65 279c-9.4-9.4-24.6-9.4-33.9 0s-9.4 24.6 0 33.9L160.2 442.1c34.5 34.5 81.3 53.9 130.1 53.9l51.8 0 1 1 1-1 5.7 0c48.8 0 95.6-19.4 130.1-53.9l19.9-19.9c1.2-1.2 2.3-2.3 3.4-3.5 .7-.5 1.3-1.1 1.9-1.7L609 313c9.4-9.4 9.4-24.6 0-33.9s-24.6-9.4-33.9 0l-53.8 53.8c-4.2-12.8-11.3-24.9-21.5-35.1L385 183c-9.4-9.4-24.6-9.4-33.9 0l-44.1 44.1c-26.5 26.5-68.5 28-96.7 4.6l98.7-98.7c13.4-13.4 31.6-21 50.6-21.1l8.5 0 .2 0 50.8 0c14.2 0 28.1 4.2 39.9 12.1L482.7 140c8.4 5.6 19.3 5.3 27.4-.6l88-64z"],
    "face-smile-wink": [512, 512, [128521, "smile-wink"], "f4da", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm177.3 63.4C192.3 335 218.4 352 256 352s63.7-17 78.7-32.6c9.2-9.6 24.4-9.9 33.9-.7s9.9 24.4 .7 33.9c-22.1 23-60 47.4-113.3 47.4s-91.2-24.4-113.3-47.4c-9.2-9.6-8.9-24.8 .7-33.9s24.8-8.9 33.9 .7zM144 208a32 32 0 1 1 64 0 32 32 0 1 1 -64 0zm164 8c0 11-9 20-20 20s-20-9-20-20c0-33.1 26.9-60 60-60l16 0c33.1 0 60 26.9 60 60 0 11-9 20-20 20s-20-9-20-20-9-20-20-20l-16 0c-11 0-20 9-20 20z"],
    "face-grin-squint-tears": [512, 512, [129315, "grin-squint-tears"], "f586", "M403.1 403.1c67.2-67.2 78.8-168.9 34.9-248l36.7-5.2c4.5-.6 8.8-1.6 13.1-2.8 44.6 94.9 27.7 211.5-50.7 290s-195.1 95.3-290 50.7c1.2-4.2 2.1-8.6 2.8-13.1l5.2-36.7c79.1 43.9 180.8 32.3 248-34.9zM75 75c78.4-78.4 195.1-95.3 290-50.7-1.2 4.2-2.1 8.6-2.8 13.1l-5.2 36.7c-79.1-43.9-180.8-32.3-248 34.9s-78.8 168.9-34.9 248l-36.7 5.2c-4.5 .6-8.8 1.6-13.1 2.8-44.6-94.9-27.7-211.5 50.7-290zM370.9 206.5c5.8-10.9 21.1-12.4 26.4-1.3 25.6 53.5 16.2 119.6-28.2 163.9-44.3 44.3-110.3 53.7-163.8 28.2-11.1-5.3-9.6-20.6 1.3-26.4 32-17.1 64.2-40.8 93.8-70.4 29.7-29.7 53.4-61.9 70.5-94zM93.3 281.9c-1.7-8 2.9-15.9 10.6-18.4l84.6-28c5.7-1.9 12.1-.4 16.3 3.9s5.8 10.6 3.9 16.3l-28 84.6c-2.6 7.7-10.5 12.3-18.4 10.6s-13.4-9-12.7-17.1l3.9-43.1-43.1 3.9c-8.1 .7-15.5-4.7-17.1-12.7zM294.6 110.4l-3.9 43.1 43.1-3.9c8.1-.7 15.5 4.7 17.1 12.7s-2.9 15.9-10.6 18.4l-84.6 28c-5.7 1.9-12.1 .4-16.3-3.9s-5.8-10.6-3.9-16.3l28-84.6c2.6-7.7 10.5-12.3 18.4-10.6s13.4 9 12.7 17.1zM512 51.4c0 25.6-18.8 47.3-44.1 50.9L421.1 109c-10.6 1.5-19.6-7.5-18.1-18.1l6.7-46.7C413.3 18.8 435 0 460.6 0 489 0 512 23 512 51.4zM44.1 409.7L90.9 403c10.6-1.5 19.6 7.5 18.1 18.1l-6.7 46.7C98.7 493.2 77 512 51.4 512 23 512 0 489 0 460.6 0 435 18.8 413.3 44.1 409.7z"],
    "file-audio": [384, 512, [], "f1c7", "M64 48l112 0 0 88c0 39.8 32.2 72 72 72l88 0 0 240c0 8.8-7.2 16-16 16L64 464c-8.8 0-16-7.2-16-16L48 64c0-8.8 7.2-16 16-16zM224 67.9l92.1 92.1-68.1 0c-13.3 0-24-10.7-24-24l0-68.1zM64 0C28.7 0 0 28.7 0 64L0 448c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-261.5c0-17-6.7-33.3-18.7-45.3L242.7 18.7C230.7 6.7 214.5 0 197.5 0L64 0zM221.9 267.6c-4.7 10-.3 21.9 9.7 26.6 19.2 8.9 32.4 28.3 32.4 50.8s-13.2 41.9-32.4 50.8c-10 4.7-14.4 16.6-9.7 26.6s16.6 14.4 26.6 9.7C281.2 416.8 304 383.6 304 345s-22.8-71.9-55.6-87.1c-10-4.7-21.9-.3-26.6 9.7zM104 305c-13.3 0-24 10.7-24 24l0 32c0 13.3 10.7 24 24 24l16 0 27.2 34c3 3.8 7.6 6 12.5 6l.3 0c8.8 0 16-7.2 16-16l0-128c0-8.8-7.2-16-16-16l-.3 0c-4.9 0-9.5 2.2-12.5 6l-27.2 34-16 0zM223.3 373c9.9-5.4 16.7-16 16.7-28.1s-6.7-22.7-16.7-28.1c-7.8-4.2-15.3 3.3-15.3 12.1l0 32c0 8.8 7.6 16.3 15.3 12.1z"],
    "calendar-xmark": [448, 512, ["calendar-times"], "f273", "M120 0c13.3 0 24 10.7 24 24l0 40 160 0 0-40c0-13.3 10.7-24 24-24s24 10.7 24 24l0 40 32 0c35.3 0 64 28.7 64 64l0 288c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 128C0 92.7 28.7 64 64 64l32 0 0-40c0-13.3 10.7-24 24-24zm0 112l-56 0c-8.8 0-16 7.2-16 16l0 288c0 8.8 7.2 16 16 16l320 0c8.8 0 16-7.2 16-16l0-288c0-8.8-7.2-16-16-16l-264 0zm171.9 92.1c9.4 9.4 9.4 24.6 0 33.9l-33.9 33.9 33.9 33.9c9.4 9.4 9.4 24.6 0 33.9s-24.6 9.4-33.9 0l-33.9-33.9-33.9 33.9c-9.4 9.4-24.6 9.4-33.9 0s-9.4-24.6 0-33.9l33.9-33.9-33.9-33.9c-9.4-9.4-9.4-24.6 0-33.9s24.6-9.4 33.9 0l33.9 33.9 33.9-33.9c9.4-9.4 24.6-9.4 33.9 0z"],
    "circle-down": [512, 512, [61466, "arrow-alt-circle-down"], "f358", "M256 464a208 208 0 1 1 0-416 208 208 0 1 1 0 416zM256 0a256 256 0 1 0 0 512 256 256 0 1 0 0-512zM244.7 387.3c6.2 6.2 16.4 6.2 22.6 0l104-104c4.6-4.6 5.9-11.5 3.5-17.4S366.5 256 360 256l-72 0 0-104c0-13.3-10.7-24-24-24l-16 0c-13.3 0-24 10.7-24 24l0 104-72 0c-6.5 0-12.3 3.9-14.8 9.9s-1.1 12.9 3.5 17.4l104 104z"],
    "file-lines": [384, 512, [128441, 128462, 61686, "file-alt", "file-text"], "f15c", "M64 48l112 0 0 88c0 39.8 32.2 72 72 72l88 0 0 240c0 8.8-7.2 16-16 16L64 464c-8.8 0-16-7.2-16-16L48 64c0-8.8 7.2-16 16-16zM224 67.9l92.1 92.1-68.1 0c-13.3 0-24-10.7-24-24l0-68.1zM64 0C28.7 0 0 28.7 0 64L0 448c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-261.5c0-17-6.7-33.3-18.7-45.3L242.7 18.7C230.7 6.7 214.5 0 197.5 0L64 0zm56 256c-13.3 0-24 10.7-24 24s10.7 24 24 24l144 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-144 0zm0 96c-13.3 0-24 10.7-24 24s10.7 24 24 24l144 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-144 0z"],
    "comments": [576, 512, [128490, 61670], "f086", "M76.2 258.7c6.1-15.2 4-32.6-5.6-45.9-14.5-20.1-22.6-43.7-22.6-68.8 0-66.8 60.5-128 144-128s144 61.2 144 128-60.5 128-144 128c-15.9 0-31.1-2.3-45.3-6.5-10.3-3.1-21.4-2.5-31.4 1.5l-50.4 20.2 11.4-28.5zM0 144c0 35.8 11.6 69.1 31.7 96.8L1.9 315.2c-1.3 3.2-1.9 6.6-1.9 10 0 14.8 12 26.8 26.8 26.8 3.4 0 6.8-.7 10-1.9l96.3-38.5c18.6 5.5 38.4 8.4 58.9 8.4 106 0 192-78.8 192-176S298-32 192-32 0 46.8 0 144zM384 512c20.6 0 40.3-3 58.9-8.4l96.3 38.5c3.2 1.3 6.6 1.9 10 1.9 14.8 0 26.8-12 26.8-26.8 0-3.4-.7-6.8-1.9-10l-29.7-74.4c20-27.8 31.7-61.1 31.7-96.8 0-82.4-61.7-151.5-145-170.7-1.6 16.3-5.1 31.9-10.1 46.9 63.9 14.8 107.2 67.3 107.2 123.9 0 25.1-8.1 48.7-22.6 68.8-9.6 13.3-11.7 30.6-5.6 45.9l11.4 28.5-50.4-20.2c-10-4-21.1-4.5-31.4-1.5-14.2 4.2-29.4 6.5-45.3 6.5-72.2 0-127.1-45.7-140.7-101.2-15.6 3.2-31.7 5-48.1 5.2 16.4 81.9 94.7 144 188.8 144z"],
    "circle-check": [512, 512, [61533, "check-circle"], "f058", "M256 48a208 208 0 1 1 0 416 208 208 0 1 1 0-416zm0 464a256 256 0 1 0 0-512 256 256 0 1 0 0 512zm84.4-299.3c7-11.2 3.6-26-7.6-33.1s-26-3.6-33.1 7.6l-61.4 98.3-27-36c-8-10.6-23-12.8-33.6-4.8s-12.8 23-4.8 33.6l48 64c4.7 6.3 12.3 9.9 20.2 9.6s15.1-4.5 19.3-11.3l80-128z"],
    "moon": [512, 512, [127769, 9214], "f186", "M239.3 48.7c-107.1 8.5-191.3 98.1-191.3 207.3 0 114.9 93.1 208 208 208 33.3 0 64.7-7.8 92.6-21.7-103.4-23.4-180.6-115.8-180.6-226.3 0-65.8 27.4-125.1 71.3-167.3zM0 256c0-141.4 114.6-256 256-256 19.4 0 38.4 2.2 56.7 6.3 9.9 2.2 17.3 10.5 18.5 20.5s-4 19.8-13.1 24.4c-60.6 30.2-102.1 92.7-102.1 164.8 0 101.6 82.4 184 184 184 5 0 9.9-.2 14.8-.6 10.1-.8 19.6 4.8 23.8 14.1s2 20.1-5.3 27.1C387.3 484.8 324.8 512 256 512 114.6 512 0 397.4 0 256z"],
    "closed-captioning": [512, 512, [], "f20a", "M448 112c8.8 0 16 7.2 16 16l0 256c0 8.8-7.2 16-16 16L64 400c-8.8 0-16-7.2-16-16l0-256c0-8.8 7.2-16 16-16l384 0zM64 64C28.7 64 0 92.7 0 128L0 384c0 35.3 28.7 64 64 64l384 0c35.3 0 64-28.7 64-64l0-256c0-35.3-28.7-64-64-64L64 64zm88 144l32 0c4.4 0 8 3.6 8 8 0 13.3 10.7 24 24 24s24-10.7 24-24c0-30.9-25.1-56-56-56l-32 0c-30.9 0-56 25.1-56 56l0 80c0 30.9 25.1 56 56 56l32 0c30.9 0 56-25.1 56-56 0-13.3-10.7-24-24-24s-24 10.7-24 24c0 4.4-3.6 8-8 8l-32 0c-4.4 0-8-3.6-8-8l0-80c0-4.4 3.6-8 8-8zm168 8c0-4.4 3.6-8 8-8l32 0c4.4 0 8 3.6 8 8 0 13.3 10.7 24 24 24s24-10.7 24-24c0-30.9-25.1-56-56-56l-32 0c-30.9 0-56 25.1-56 56l0 80c0 30.9 25.1 56 56 56l32 0c30.9 0 56-25.1 56-56 0-13.3-10.7-24-24-24s-24 10.7-24 24c0 4.4-3.6 8-8 8l-32 0c-4.4 0-8-3.6-8-8l0-80z"],
    "images": [576, 512, [], "f302", "M480 80c8.8 0 16 7.2 16 16l0 256c0 8.8-7.2 16-16 16l-320 0c-8.8 0-16-7.2-16-16l0-256c0-8.8 7.2-16 16-16l320 0zM160 32c-35.3 0-64 28.7-64 64l0 256c0 35.3 28.7 64 64 64l320 0c35.3 0 64-28.7 64-64l0-256c0-35.3-28.7-64-64-64L160 32zm80 112a32 32 0 1 0 -64 0 32 32 0 1 0 64 0zm140.7 3.8c-4.3-7.3-12.2-11.8-20.7-11.8s-16.4 4.5-20.7 11.8l-46.5 79-17.2-24.6c-4.5-6.4-11.8-10.2-19.7-10.2s-15.2 3.8-19.7 10.2l-56 80c-5.1 7.3-5.8 16.9-1.6 24.8S191.1 320 200 320l240 0c8.6 0 16.6-4.6 20.8-12.1s4.2-16.7-.1-24.1l-80-136zM48 152c0-13.3-10.7-24-24-24S0 138.7 0 152L0 448c0 35.3 28.7 64 64 64l360 0c13.3 0 24-10.7 24-24s-10.7-24-24-24L64 464c-8.8 0-16-7.2-16-16l0-296z"],
    "circle-right": [512, 512, [61838, "arrow-alt-circle-right"], "f35a", "M464 256a208 208 0 1 1 -416 0 208 208 0 1 1 416 0zM0 256a256 256 0 1 0 512 0 256 256 0 1 0 -512 0zm387.3 11.3c6.2-6.2 6.2-16.4 0-22.6l-104-104c-4.6-4.6-11.5-5.9-17.4-3.5S256 145.5 256 152l0 72-104 0c-13.3 0-24 10.7-24 24l0 16c0 13.3 10.7 24 24 24l104 0 0 72c0 6.5 3.9 12.3 9.9 14.8s12.9 1.1 17.4-3.5l104-104z"],
    "id-card": [576, 512, [62147, "drivers-license"], "f2c2", "M48 416l0-256 480 0 0 256c0 8.8-7.2 16-16 16l-192 0c0-44.2-35.8-80-80-80l-64 0c-44.2 0-80 35.8-80 80l-32 0c-8.8 0-16-7.2-16-16zM64 32C28.7 32 0 60.7 0 96L0 416c0 35.3 28.7 64 64 64l448 0c35.3 0 64-28.7 64-64l0-320c0-35.3-28.7-64-64-64L64 32zM208 312a56 56 0 1 0 0-112 56 56 0 1 0 0 112zM376 208c-13.3 0-24 10.7-24 24s10.7 24 24 24l80 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-80 0zm0 96c-13.3 0-24 10.7-24 24s10.7 24 24 24l80 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-80 0z"],
    "circle-play": [512, 512, [61469, "play-circle"], "f144", "M256 48a208 208 0 1 1 0 416 208 208 0 1 1 0-416zm0 464a256 256 0 1 0 0-512 256 256 0 1 0 0 512zM212.5 147.5c-7.4-4.5-16.7-4.7-24.3-.5S176 159.3 176 168l0 176c0 8.7 4.7 16.7 12.3 20.9s16.8 4.1 24.3-.5l144-88c7.1-4.4 11.5-12.1 11.5-20.5s-4.4-16.1-11.5-20.5l-144-88zM298 256l-74 45.2 0-90.4 74 45.2z"],
    "face-laugh-beam": [512, 512, [128513, "laugh-beam"], "f59a", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm118.3 58.2c-4.2-13.7 7.1-26.2 21.4-26.2l232.6 0c14.3 0 25.6 12.5 21.4 26.2-18 58.9-72.9 101.8-137.7 101.8S136.3 373.1 118.3 314.2zM176 180c-15.5 0-28 12.5-28 28l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-37.6 30.4-68 68-68s68 30.4 68 68l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-15.5-12.5-28-28-28zm132 28l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-37.6 30.4-68 68-68s68 30.4 68 68l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-15.5-12.5-28-28-28s-28 12.5-28 28z"],
    "address-book": [512, 512, [62138, "contact-book"], "f2b9", "M384 48c8.8 0 16 7.2 16 16l0 384c0 8.8-7.2 16-16 16L96 464c-8.8 0-16-7.2-16-16L80 64c0-8.8 7.2-16 16-16l288 0zM96 0C60.7 0 32 28.7 32 64l0 384c0 35.3 28.7 64 64 64l288 0c35.3 0 64-28.7 64-64l0-384c0-35.3-28.7-64-64-64L96 0zM240 248a56 56 0 1 0 0-112 56 56 0 1 0 0 112zm-32 40c-44.2 0-80 35.8-80 80 0 8.8 7.2 16 16 16l192 0c8.8 0 16-7.2 16-16 0-44.2-35.8-80-80-80l-64 0zM512 80c0-8.8-7.2-16-16-16s-16 7.2-16 16l0 64c0 8.8 7.2 16 16 16s16-7.2 16-16l0-64zM496 192c-8.8 0-16 7.2-16 16l0 64c0 8.8 7.2 16 16 16s16-7.2 16-16l0-64c0-8.8-7.2-16-16-16zm16 144c0-8.8-7.2-16-16-16s-16 7.2-16 16l0 64c0 8.8 7.2 16 16 16s16-7.2 16-16l0-64z"],
    "hourglass": [384, 512, [9203, 62032, "hourglass-empty"], "f254", "M24 0C10.7 0 0 10.7 0 24S10.7 48 24 48l8 0 0 19c0 40.3 16 79 44.5 107.5l81.5 81.5-81.5 81.5C48 366 32 404.7 32 445l0 19-8 0c-13.3 0-24 10.7-24 24s10.7 24 24 24l336 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-8 0 0-19c0-40.3-16-79-44.5-107.5l-81.5-81.5 81.5-81.5C336 146 352 107.3 352 67l0-19 8 0c13.3 0 24-10.7 24-24S373.3 0 360 0L24 0zM192 289.9l81.5 81.5C293 391 304 417.4 304 445l0 19-224 0 0-19c0-27.6 11-54 30.5-73.5L192 289.9zm0-67.9l-81.5-81.5C91 121 80 94.6 80 67l0-19 224 0 0 19c0 27.6-11 54-30.5 73.5L192 222.1z"],
    "headphones": [448, 512, [127911, 62863, "headphones-alt", "headphones-simple"], "f025", "M48 224c0-97.2 78.8-176 176-176s176 78.8 176 176l0 44.8c-14.1-8.2-30.5-12.8-48-12.8l-16 0c-26.5 0-48 21.5-48 48l0 128c0 26.5 21.5 48 48 48l16 0c53 0 96-43 96-96l0-160C448 100.3 347.7 0 224 0S0 100.3 0 224L0 384c0 53 43 96 96 96l16 0c26.5 0 48-21.5 48-48l0-128c0-26.5-21.5-48-48-48l-16 0c-17.5 0-33.9 4.7-48 12.8L48 224zm0 128c0-26.5 21.5-48 48-48l16 0 0 128-16 0c-26.5 0-48-21.5-48-48l0-32zm352 0l0 32c0 26.5-21.5 48-48 48l-16 0 0-128 16 0c26.5 0 48 21.5 48 48z"],
    "file-powerpoint": [384, 512, [], "f1c4", "M64 48l112 0 0 88c0 39.8 32.2 72 72 72l88 0 0 240c0 8.8-7.2 16-16 16L64 464c-8.8 0-16-7.2-16-16L48 64c0-8.8 7.2-16 16-16zM224 67.9l92.1 92.1-68.1 0c-13.3 0-24-10.7-24-24l0-68.1zM64 0C28.7 0 0 28.7 0 64L0 448c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-261.5c0-17-6.7-33.3-18.7-45.3L242.7 18.7C230.7 6.7 214.5 0 197.5 0L64 0zm88 256c-13.3 0-24 10.7-24 24l0 128c0 13.3 10.7 24 24 24s24-10.7 24-24l0-16 28 0c37.6 0 68-30.4 68-68s-30.4-68-68-68l-52 0zm52 88l-28 0 0-40 28 0c11 0 20 9 20 20s-9 20-20 20z"],
    "window-maximize": [512, 512, [128470], "f2d0", "M48 224l0 160c0 8.8 7.2 16 16 16l384 0c8.8 0 16-7.2 16-16l0-160-416 0zM0 128C0 92.7 28.7 64 64 64l384 0c35.3 0 64 28.7 64 64l0 256c0 35.3-28.7 64-64 64L64 448c-35.3 0-64-28.7-64-64L0 128z"],
    "comment-dots": [512, 512, [128172, 62075, "commenting"], "f4ad", "M0 240c0 54.4 19.3 104.6 51.9 144.9L3.1 474.3c-2 3.7-3.1 7.9-3.1 12.2 0 14.1 11.4 25.5 25.5 25.5 4 0 7.8-.6 11.5-2.1L153.4 460c31.4 12.9 66.1 20 102.6 20 141.4 0 256-107.5 256-240S397.4 0 256 0 0 107.5 0 240zM94 407.9c9.3-17.1 7.4-38.1-4.8-53.2-26.1-32.3-41.2-71.9-41.2-114.7 0-103.2 90.2-192 208-192s208 88.8 208 192-90.2 192-208 192c-30.2 0-58.7-5.9-84.3-16.4-11.9-4.9-25.3-4.8-37.1 .3L76 440.9 94 407.9zM144 272a32 32 0 1 0 0-64 32 32 0 1 0 0 64zm144-32a32 32 0 1 0 -64 0 32 32 0 1 0 64 0zm80 32a32 32 0 1 0 0-64 32 32 0 1 0 0 64z"],
    "face-grin-tongue-wink": [512, 512, [128540, "grin-tongue-wink"], "f58b", "M366.9 432c.8-5.2 1.1-10.6 1.1-16l0-53.5c10.2-12.6 18.3-26.9 23.8-42.5 4.1-11.6-7.8-21.4-19.6-17.8-34.8 10.6-74.3 16.6-116.3 16.6-41.9 0-81.4-6-116.1-16.5-11.8-3.6-23.7 6.1-19.6 17.8 5.5 15.5 13.6 29.9 23.8 42.4l0 53.5c0 5.4 .4 10.8 1.1 16-58.4-36.8-97.1-101.9-97.1-176 0-114.9 93.1-208 208-208s208 93.1 208 208c0 74.1-38.8 139.2-97.1 176zm-38.8 69.7C434.4 470.6 512 372.3 512 256 512 114.6 397.4 0 256 0S0 114.6 0 256C0 372.3 77.6 470.6 183.9 501.7 203.4 518.1 228.5 528 256 528s52.6-9.9 72.1-26.3zM320 378.6l0 37.4c0 35.3-28.7 64-64 64s-64-28.7-64-64l0-37.4c0-14.7 11.9-26.6 26.6-26.6l2 0c11.3 0 21.1 7.9 23.6 18.9 2.8 12.6 20.8 12.6 23.6 0 2.5-11.1 12.3-18.9 23.6-18.9l2 0c14.7 0 26.6 11.9 26.6 26.6zM132 232c0-11 9-20 20-20l16 0c11 0 20 9 20 20s9 20 20 20 20-9 20-20c0-33.1-26.9-60-60-60l-16 0c-33.1 0-60 26.9-60 60 0 11 9 20 20 20s20-9 20-20zm228.4-24a24 24 0 1 0 -48 0 24 24 0 1 0 48 0zM288 208a48 48 0 1 1 96 0 48 48 0 1 1 -96 0zm128 0a80 80 0 1 0 -160 0 80 80 0 1 0 160 0z"],
    "hourglass-half": [384, 512, ["hourglass-2"], "f252", "M0 24C0 10.7 10.7 0 24 0L360 0c13.3 0 24 10.7 24 24s-10.7 24-24 24l-8 0 0 19c0 40.3-16 79-44.5 107.5l-81.5 81.5 81.5 81.5C336 366 352 404.7 352 445l0 19 8 0c13.3 0 24 10.7 24 24s-10.7 24-24 24L24 512c-13.3 0-24-10.7-24-24s10.7-24 24-24l8 0 0-19c0-40.3 16-79 44.5-107.5l81.5-81.5-81.5-81.5C48 146 32 107.3 32 67l0-19-8 0C10.7 48 0 37.3 0 24zM110.5 371.5c-3.9 3.9-7.5 8.1-10.7 12.5l184.4 0c-3.2-4.4-6.8-8.6-10.7-12.5l-81.5-81.5-81.5 81.5zM80.8 432c-.5 4.3-.8 8.6-.8 13l0 19 224 0 0-19c0-4.4-.3-8.7-.8-13L80.8 432zM254.1 160l-124.1 0 62.1 62.1 62.1-62.1zm39.7-48C300.4 98.1 304 82.7 304 67l0-19-224 0 0 19c0 15.7 3.6 31.1 10.2 45l203.5 0z"],
    "credit-card": [512, 512, [128179, 62083, "credit-card-alt"], "f09d", "M448 112c8.8 0 16 7.2 16 16l0 32-416 0 0-32c0-8.8 7.2-16 16-16l384 0zm16 112l0 160c0 8.8-7.2 16-16 16L64 400c-8.8 0-16-7.2-16-16l0-160 416 0zM64 64C28.7 64 0 92.7 0 128L0 384c0 35.3 28.7 64 64 64l384 0c35.3 0 64-28.7 64-64l0-256c0-35.3-28.7-64-64-64L64 64zM80 344c0 13.3 10.7 24 24 24l48 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-48 0c-13.3 0-24 10.7-24 24zm144 0c0 13.3 10.7 24 24 24l64 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-64 0c-13.3 0-24 10.7-24 24z"],
    "hand-spock": [512, 512, [128406], "f259", "M138.3 80.8c-9.2-33.8 10.5-68.8 44.3-78.4 34-9.6 69.4 10.2 79 44.2L291.9 153.7 305.1 84c6.6-34.7 40.1-57.5 74.8-50.9 31.4 6 53 33.9 52 64.9 10-2.6 20.8-2.8 31.5-.1 34.3 8.6 55.1 43.3 46.6 77.6L454.7 397.2C437.8 464.7 377.2 512 307.6 512l-33.7 0c-56.9 0-112.2-19-157.2-53.9l-92-71.6c-27.9-21.7-32.9-61.9-11.2-89.8s61.9-32.9 89.8-11.2l17 13.2-51.8-131.2c-13-32.9 3.2-70.1 36-83 11.1-4.4 22.7-5.4 33.7-3.7zm77.1-21.2c-2.4-8.5-11.2-13.4-19.7-11s-13.4 11.2-11 19.7l54.8 182.4c3.5 12.3-3.3 25.2-15.4 29.3s-25.3-2-30-13.9L142.9 138.1c-3.2-8.2-12.5-12.3-20.8-9s-12.3 12.5-9 20.8l73.3 185.6c12 30.3-23.7 57-49.4 37L73.8 323.4c-7-5.4-17-4.2-22.5 2.8s-4.2 17 2.8 22.5l92 71.6c36.5 28.4 81.4 43.8 127.7 43.8l33.7 0c47.5 0 89-32.4 100.5-78.5l55.4-221.6c2.1-8.6-3.1-17.3-11.6-19.4s-17.3 3.1-19.4 11.6l-26 104c-2.9 11.7-13.4 19.9-25.5 19.9-16.5 0-28.9-15-25.8-31.2L383.7 99c1.7-8.7-4-17.1-12.7-18.7S354 84.3 352.3 93L320.5 260c-2.2 11.6-12.4 20-24.2 20-11 0-20.7-7.3-23.7-17.9L215.4 59.6z"],
    "bell-slash": [576, 512, [128277, 61943], "f1f6", "M41-24.9c-9.4-9.4-24.6-9.4-33.9 0S-2.3-.3 7 9.1l528 528c9.4 9.4 24.6 9.4 33.9 0s9.4-24.6 0-33.9l-87.8-87.8c17.5-3.3 30.8-18.7 30.8-37.1 0-6.7-1.8-13.3-5.1-19L485 321.7c-19-32.6-29-69.6-29-107.3l0-14.5c0-84.6-62.6-154.7-144-166.3l0-9.7c0-13.3-10.7-24-24-24s-24 10.7-24 24l0 9.7c-42.2 6-79.4 27.8-105.4 59.1L41-24.9zM192.8 126.9C214.7 98.4 249.2 80 288 80 354.3 80 408 133.7 408 200l0 14.5c0 46.2 12.3 91.5 35.5 131.4l12.9 22.1-22.6 0-241.1-241.1zM132.5 345.9c19.5-33.4 31.3-70.7 34.6-109l-46.7-46.7c-.2 3.3-.3 6.6-.3 9.9l0 14.5c0 37.7-10 74.7-29 107.3L69.1 359.2c-3.4 5.8-5.1 12.3-5.1 19 0 20.9 16.9 37.8 37.8 37.8l244.4 0-48-48-178.6 0 12.9-22.1zM220.1 464c9.9 28 36.6 48 67.9 48s58-20 67.9-48l-135.8 0z"],
    "star": [576, 512, [11088, 61446], "f005", "M288.1-32c9 0 17.3 5.1 21.4 13.1L383 125.3 542.9 150.7c8.9 1.4 16.3 7.7 19.1 16.3s.5 18-5.8 24.4L441.7 305.9 467 465.8c1.4 8.9-2.3 17.9-9.6 23.2s-17 6.1-25 2L288.1 417.6 143.8 491c-8 4.1-17.7 3.3-25-2s-11-14.2-9.6-23.2L134.4 305.9 20 191.4c-6.4-6.4-8.6-15.8-5.8-24.4s10.1-14.9 19.1-16.3l159.9-25.4 73.6-144.2c4.1-8 12.4-13.1 21.4-13.1zm0 76.8L230.3 158c-3.5 6.8-10 11.6-17.6 12.8l-125.5 20 89.8 89.9c5.4 5.4 7.9 13.1 6.7 20.7l-19.8 125.5 113.3-57.6c6.8-3.5 14.9-3.5 21.8 0l113.3 57.6-19.8-125.5c-1.2-7.6 1.3-15.3 6.7-20.7l89.8-89.9-125.5-20c-7.6-1.2-14.1-6-17.6-12.8L288.1 44.8z"],
    "flag": [448, 512, [127988, 61725], "f024", "M48 24C48 10.7 37.3 0 24 0S0 10.7 0 24L0 488c0 13.3 10.7 24 24 24s24-10.7 24-24l0-100 80.3-20.1c41.1-10.3 84.6-5.5 122.5 13.4 44.2 22.1 95.5 24.8 141.7 7.4l34.7-13c12.5-4.7 20.8-16.6 20.8-30l0-279.7c0-23-24.2-38-44.8-27.7l-9.6 4.8c-46.3 23.2-100.8 23.2-147.1 0-35.1-17.6-75.4-22-113.5-12.5L48 52 48 24zm0 77.5l96.6-24.2c27-6.7 55.5-3.6 80.4 8.8 54.9 27.4 118.7 29.7 175 6.8l0 241.8-24.4 9.1c-33.7 12.6-71.2 10.7-103.4-5.4-48.2-24.1-103.3-30.1-155.6-17.1l-68.6 17.2 0-237z"],
    "lemon": [448, 512, [127819], "f094", "M368 80c-3.2 0-6.2 .4-8.9 1.3-19.1 5.5-46.1 10.7-74.3 3.3-57.4-14.9-124.6 7.4-174.7 57.5S37.7 259.4 52.6 316.8c7.3 28.2 2.2 55.2-3.3 74.3-.8 2.8-1.3 5.8-1.3 8.9 0 17.7 14.3 32 32 32 3.2 0 6.2-.4 8.9-1.3 19.1-5.5 46.1-10.7 74.3-3.3 57.4 14.9 124.6-7.4 174.7-57.5s72.4-117.3 57.5-174.7c-7.3-28.2-2.2-55.2 3.3-74.3 .8-2.8 1.3-5.8 1.3-8.9 0-17.7-14.3-32-32-32zm0-48c44.2 0 80 35.8 80 80 0 7.7-1.1 15.2-3.1 22.3-4.6 15.8-7.1 32.9-3 48.9 20.1 77.6-10.9 161.5-70 220.7s-143.1 90.2-220.7 70c-16-4.1-33-1.6-48.9 3-7.1 2-14.6 3.1-22.3 3.1-44.2 0-80-35.8-80-80 0-7.7 1.1-15.2 3.1-22.3 4.6-15.8 7.1-32.9 3-48.9-20.1-77.6 10.9-161.5 70-220.7S219.3 18 296.8 38.1c16 4.1 33 1.6 48.9-3 7.1-2 14.6-3.1 22.3-3.1zM246.7 167c-52 15.2-96.5 59.7-111.7 111.7-3.7 12.7-17.1 20-29.8 16.3S85.2 278 89 265.3c19.8-67.7 76.6-124.5 144.3-144.3 12.7-3.7 26.1 3.6 29.8 16.3s-3.6 26.1-16.3 29.8z"],
    "window-restore": [576, 512, [], "f2d2", "M512 80L224 80c-8.8 0-16 7.2-16 16l0 16-48 0 0-16c0-35.3 28.7-64 64-64l288 0c35.3 0 64 28.7 64 64l0 192c0 35.3-28.7 64-64 64l-48 0 0-48 48 0c8.8 0 16-7.2 16-16l0-192c0-8.8-7.2-16-16-16zM368 288l-320 0 0 128c0 8.8 7.2 16 16 16l288 0c8.8 0 16-7.2 16-16l0-128zM64 160l288 0c35.3 0 64 28.7 64 64l0 192c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 224c0-35.3 28.7-64 64-64z"],
    "face-grin-hearts": [512, 512, [128525, "grin-hearts"], "f584", "M464 256c0 114.9-93.1 208-208 208S48 370.9 48 256c0-3.5 .1-7.1 .3-10.6-14-13.9-29.7-33.1-39.3-56.7-5.8 21.4-8.9 44-8.9 67.3 0 141.4 114.6 256 256 256S512 397.4 512 256c0-23.3-3.1-45.9-8.9-67.3-9.6 23.7-25.4 42.8-39.3 56.7 .2 3.5 .3 7 .3 10.6zM368 58.9c11.7-6 24.5-9.6 37.7-10.6-42.1-30.4-93.8-48.3-149.7-48.3S148.4 17.9 106.3 48.3c13.2 1 26 4.6 37.7 10.6 13.8-7.1 29.3-10.9 45.1-10.9l2.9 0c8.9 0 17.6 1.2 25.8 3.5 12.4-2.3 25.2-3.5 38.2-3.5s25.8 1.2 38.2 3.5c8.2-2.3 16.9-3.5 25.8-3.5l2.9 0c15.8 0 31.3 3.8 45.1 10.9zm4.2 243.4c-34.8 10.6-74.3 16.6-116.3 16.6-41.9 0-81.4-6-116.1-16.5-11.8-3.6-23.7 6.1-19.6 17.8 19.8 55.9 73.1 95.9 135.8 95.9 62.7 0 116-40.1 135.8-96 4.1-11.6-7.8-21.4-19.6-17.8zM322.9 96L320 96c-26.5 0-48 21.5-48 48 0 53.4 66.9 95.7 89 108.2 4.4 2.5 9.6 2.5 14 0 22.1-12.5 89-54.8 89-108.2 0-26.5-21.5-48-48-48l-2.9 0c-13.5 0-26.5 5.4-36 14.9l-9.1 9.1-9.1-9.1c-9.5-9.5-22.5-14.9-36-14.9zm-188 14.9c-9.5-9.5-22.5-14.9-36-14.9L96 96c-26.5 0-48 21.5-48 48 0 53.4 66.9 95.7 89 108.2 4.4 2.5 9.6 2.5 14 0 22.1-12.5 89-54.8 89-108.2 0-26.5-21.5-48-48-48l-2.9 0c-13.5 0-26.5 5.4-36 14.9l-9.1 9.1-9.1-9.1z"],
    "face-kiss-beam": [512, 512, [128537, "kiss-beam"], "f597", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm240 16l32 0c26.5 0 48 21.5 48 48 0 12.3-4.6 23.5-12.2 32 7.6 8.5 12.2 19.7 12.2 32 0 26.5-21.5 48-48 48l-32 0c-8.8 0-16-7.2-16-16s7.2-16 16-16l16 0c8.8 0 16-7.2 16-16s-7.2-16-16-16l-16 0c-8.8 0-16-7.2-16-16s7.2-16 16-16l16 0c8.8 0 16-7.2 16-16s-7.2-16-16-16l-16 0c-8.8 0-16-7.2-16-16s7.2-16 16-16zm-64-92c-15.5 0-28 12.5-28 28l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-37.6 30.4-68 68-68s68 30.4 68 68l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-15.5-12.5-28-28-28zm132 28l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-37.6 30.4-68 68-68s68 30.4 68 68l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-15.5-12.5-28-28-28s-28 12.5-28 28z"],
    "file-pdf": [576, 512, [], "f1c1", "M208 48L96 48c-8.8 0-16 7.2-16 16l0 384c0 8.8 7.2 16 16 16l80 0 0 48-80 0c-35.3 0-64-28.7-64-64L32 64C32 28.7 60.7 0 96 0L229.5 0c17 0 33.3 6.7 45.3 18.7L397.3 141.3c12 12 18.7 28.3 18.7 45.3l0 149.5-48 0 0-128-88 0c-39.8 0-72-32.2-72-72l0-88zM348.1 160L256 67.9 256 136c0 13.3 10.7 24 24 24l68.1 0zM240 380l32 0c33.1 0 60 26.9 60 60s-26.9 60-60 60l-12 0 0 28c0 11-9 20-20 20s-20-9-20-20l0-128c0-11 9-20 20-20zm32 80c11 0 20-9 20-20s-9-20-20-20l-12 0 0 40 12 0zm96-80l32 0c28.7 0 52 23.3 52 52l0 64c0 28.7-23.3 52-52 52l-32 0c-11 0-20-9-20-20l0-128c0-11 9-20 20-20zm32 128c6.6 0 12-5.4 12-12l0-64c0-6.6-5.4-12-12-12l-12 0 0 88 12 0zm76-108c0-11 9-20 20-20l48 0c11 0 20 9 20 20s-9 20-20 20l-28 0 0 24 28 0c11 0 20 9 20 20s-9 20-20 20l-28 0 0 44c0 11-9 20-20 20s-20-9-20-20l0-128z"],
    "face-grin-wide": [512, 512, [128515, "grin-alt"], "f581", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm372.2 46.3c11.8-3.6 23.7 6.1 19.6 17.8-19.8 55.9-73.1 96-135.8 96-62.7 0-116-40-135.8-95.9-4.1-11.6 7.8-21.4 19.6-17.8 34.7 10.6 74.2 16.5 116.1 16.5 42 0 81.5-6 116.3-16.6zM224 192c0 35.3-14.3 64-32 64s-32-28.7-32-64 14.3-64 32-64 32 28.7 32 64zm96 64c-17.7 0-32-28.7-32-64s14.3-64 32-64 32 28.7 32 64-14.3 64-32 64z"],
    "face-laugh-squint": [512, 512, ["laugh-squint"], "f59b", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm125.2 76.4c-6.5-14 5-28.4 20.4-28.4l220.8 0c15.4 0 26.8 14.4 20.4 28.4-22.8 49.4-72.8 83.6-130.8 83.6s-107.9-34.2-130.8-83.6zm-2.6-173.2c4.5-6.8 13.3-9.2 20.6-5.5l79.6 40c5.4 2.7 8.8 8.2 8.8 14.3s-3.4 11.6-8.8 14.3l-79.6 40c-7.3 3.6-16.1 1.3-20.6-5.5s-3.1-15.9 3.1-21.1L159 208 125.8 180.3c-6.2-5.2-7.6-14.3-3.1-21.1zm263.6 21.1L353 208 386.2 235.7c6.2 5.2 7.6 14.3 3.1 21.1s-13.3 9.2-20.6 5.5l-79.6-40c-5.4-2.7-8.8-8.2-8.8-14.3s3.4-11.6 8.8-14.3l79.6-40c7.3-3.6 16.1-1.3 20.6 5.5s3.1 15.9-3.1 21.1z"],
    "face-kiss-wink-heart": [640, 512, [128536, "kiss-wink-heart"], "f598", "M386 439.5c-29.2 15.6-62.5 24.5-98 24.5-114.9 0-208-93.1-208-208S173.2 48 288 48c113.2 0 205.2 90.4 207.9 202.9 14.3 1.5 28.6 6 41.9 13.7 2 1.2 4 2.4 5.9 3.7 .2-4.1 .3-8.2 .3-12.3 0-141.4-114.6-256-256-256S32 114.6 32 256 146.6 512 288 512c41.4 0 80.5-9.8 115.1-27.3-5.8-12.9-12-28.5-17.2-45.2zM256 288c0 8.8 7.2 16 16 16l16 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-16 0c-8.8 0-16 7.2-16 16s7.2 16 16 16l16 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-16 0c-8.8 0-16 7.2-16 16s7.2 16 16 16l32 0c26.5 0 48-21.5 48-48 0-12.3-4.6-23.5-12.2-32 7.6-8.5 12.2-19.7 12.2-32 0-26.5-21.5-48-48-48l-32 0c-8.8 0-16 7.2-16 16zm-48-48a32 32 0 1 0 0-64 32 32 0 1 0 0 64zm152-44l16 0c11 0 20 9 20 20s9 20 20 20 20-9 20-20c0-33.1-26.9-60-60-60l-16 0c-33.1 0-60 26.9-60 60 0 11 9 20 20 20s20-9 20-20 9-20 20-20zM542.8 350c-2.2-18.3-12.9-34.6-28.9-43.8-28.1-16.2-63.9-6.6-80.1 21.5l-2.7 4.6c-24.5 42.5 7.9 117.9 24.4 150.8 5.1 10.1 15.5 16.1 26.8 15.5 36.7-2.2 118.2-11.7 142.8-54.2l2.7-4.6c16.2-28.1 6.6-63.9-21.5-80.1-16-9.2-35.4-10.4-52.4-3.1l-9.8 4.2-1.3-10.6z"],
    "copy": [448, 512, [], "f0c5", "M384 336l-192 0c-8.8 0-16-7.2-16-16l0-256c0-8.8 7.2-16 16-16l133.5 0c4.2 0 8.3 1.7 11.3 4.7l58.5 58.5c3 3 4.7 7.1 4.7 11.3L400 320c0 8.8-7.2 16-16 16zM192 384l192 0c35.3 0 64-28.7 64-64l0-197.5c0-17-6.7-33.3-18.7-45.3L370.7 18.7C358.7 6.7 342.5 0 325.5 0L192 0c-35.3 0-64 28.7-64 64l0 256c0 35.3 28.7 64 64 64zM64 128c-35.3 0-64 28.7-64 64L0 448c0 35.3 28.7 64 64 64l192 0c35.3 0 64-28.7 64-64l0-16-48 0 0 16c0 8.8-7.2 16-16 16L64 464c-8.8 0-16-7.2-16-16l0-256c0-8.8 7.2-16 16-16l16 0 0-48-16 0z"],
    "chess-king": [448, 512, [9818], "f43f", "M224-32c13.3 0 24 10.7 24 24l0 40 48 0c13.3 0 24 10.7 24 24s-10.7 24-24 24l-48 0 0 80 161.8 0c21.1 0 38.2 17.1 38.2 38.2 0 6.4-1.6 12.7-4.7 18.3L357.2 374.5 405.6 435c6.7 8.4 10.4 18.8 10.4 29.6 0 26.2-21.2 47.4-47.4 47.4L79.4 512c-26.2 0-47.4-21.2-47.4-47.4 0-10.8 3.7-21.2 10.4-29.6L90.8 374.5 4.7 216.6C1.6 210.9 0 204.6 0 198.2 0 177.1 17.1 160 38.2 160l161.8 0 0-80-48 0c-13.3 0-24-10.7-24-24s10.7-24 24-24l48 0 0-40c0-13.3 10.7-24 24-24zM131.8 400l-3.6 4.4-47.6 59.6 286.6 0-47.6-59.6-3.6-4.4-184.3 0zm1.1-48.5l.3 .5 181.6 0 .3-.5 78.3-143.5-338.7 0 78.3 143.5z"],
    "square-plus": [448, 512, [61846, "plus-square"], "f0fe", "M64 80c-8.8 0-16 7.2-16 16l0 320c0 8.8 7.2 16 16 16l320 0c8.8 0 16-7.2 16-16l0-320c0-8.8-7.2-16-16-16L64 80zM0 96C0 60.7 28.7 32 64 32l320 0c35.3 0 64 28.7 64 64l0 320c0 35.3-28.7 64-64 64L64 480c-35.3 0-64-28.7-64-64L0 96zM200 344l0-64-64 0c-13.3 0-24-10.7-24-24s10.7-24 24-24l64 0 0-64c0-13.3 10.7-24 24-24s24 10.7 24 24l0 64 64 0c13.3 0 24 10.7 24 24s-10.7 24-24 24l-64 0 0 64c0 13.3-10.7 24-24 24s-24-10.7-24-24z"],
    "file-code": [384, 512, [], "f1c9", "M64 48l112 0 0 88c0 39.8 32.2 72 72 72l88 0 0 240c0 8.8-7.2 16-16 16L64 464c-8.8 0-16-7.2-16-16L48 64c0-8.8 7.2-16 16-16zM224 67.9l92.1 92.1-68.1 0c-13.3 0-24-10.7-24-24l0-68.1zM64 0C28.7 0 0 28.7 0 64L0 448c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-261.5c0-17-6.7-33.3-18.7-45.3L242.7 18.7C230.7 6.7 214.5 0 197.5 0L64 0zM170.2 295.6c8.6-10.1 7.5-25.2-2.6-33.8s-25.2-7.5-33.8 2.6l-48 56c-7.7 9-7.7 22.2 0 31.2l48 56c8.6 10.1 23.8 11.2 33.8 2.6s11.2-23.8 2.6-33.8l-34.6-40.4 34.6-40.4zm80-31.2c-8.6-10.1-23.8-11.2-33.8-2.6s-11.2 23.8-2.6 33.8l34.6 40.4-34.6 40.4c-8.6 10.1-7.5 25.2 2.6 33.8s25.2 7.5 33.8-2.6l48-56c7.7-9 7.7-22.2 0-31.2l-48-56z"],
    "face-grin-wink": [512, 512, ["grin-wink"], "f58c", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm372.2 46.3c11.8-3.6 23.7 6.1 19.6 17.8-19.8 55.9-73.1 96-135.8 96-62.7 0-116-40-135.8-95.9-4.1-11.6 7.8-21.4 19.6-17.8 34.7 10.6 74.2 16.5 116.1 16.5 42 0 81.5-6 116.3-16.6zM144 208a32 32 0 1 1 64 0 32 32 0 1 1 -64 0zm164 8c0 11-9 20-20 20s-20-9-20-20c0-33.1 26.9-60 60-60l16 0c33.1 0 60 26.9 60 60 0 11-9 20-20 20s-20-9-20-20-9-20-20-20l-16 0c-11 0-20 9-20 20z"],
    "money-bill-1": [512, 512, ["money-bill-alt"], "f3d1", "M112 112c0 35.3-28.7 64-64 64l0 160c35.3 0 64 28.7 64 64l288 0c0-35.3 28.7-64 64-64l0-160c-35.3 0-64-28.7-64-64l-288 0zM0 128C0 92.7 28.7 64 64 64l384 0c35.3 0 64 28.7 64 64l0 256c0 35.3-28.7 64-64 64L64 448c-35.3 0-64-28.7-64-64L0 128zm256 16a112 112 0 1 1 0 224 112 112 0 1 1 0-224zm-16 44c-11 0-20 9-20 20 0 9.7 6.9 17.7 16 19.6l0 48.4-4 0c-11 0-20 9-20 20s9 20 20 20l48 0c11 0 20-9 20-20s-9-20-20-20l-4 0 0-68c0-11-9-20-20-20l-16 0z"],
    "eye-slash": [576, 512, [], "f070", "M41-24.9c-9.4-9.4-24.6-9.4-33.9 0S-2.3-.3 7 9.1l528 528c9.4 9.4 24.6 9.4 33.9 0s9.4-24.6 0-33.9l-96.4-96.4c2.7-2.4 5.4-4.8 8-7.2 46.8-43.5 78.1-95.4 93-131.1 3.3-7.9 3.3-16.7 0-24.6-14.9-35.7-46.2-87.7-93-131.1-47.1-43.7-111.8-80.6-192.6-80.6-56.8 0-105.6 18.2-146 44.2L41-24.9zM176.9 111.1c32.1-18.9 69.2-31.1 111.1-31.1 65.2 0 118.8 29.6 159.9 67.7 38.5 35.7 65.1 78.3 78.6 108.3-13.6 30-40.2 72.5-78.6 108.3-3.1 2.8-6.2 5.6-9.4 8.4L393.8 328c14-20.5 22.2-45.3 22.2-72 0-70.7-57.3-128-128-128-26.7 0-51.5 8.2-72 22.2l-39.1-39.1zm182 182l-108-108c11.1-5.8 23.7-9.1 37.1-9.1 44.2 0 80 35.8 80 80 0 13.4-3.3 26-9.1 37.1zM103.4 173.2l-34-34c-32.6 36.8-55 75.8-66.9 104.5-3.3 7.9-3.3 16.7 0 24.6 14.9 35.7 46.2 87.7 93 131.1 47.1 43.7 111.8 80.6 192.6 80.6 37.3 0 71.2-7.9 101.5-20.6L352.2 422c-20 6.4-41.4 10-64.2 10-65.2 0-118.8-29.6-159.9-67.7-38.5-35.7-65.1-78.3-78.6-108.3 10.4-23.1 28.6-53.6 54-82.8z"],
    "file-word": [384, 512, [], "f1c2", "M64 48l112 0 0 88c0 39.8 32.2 72 72 72l88 0 0 240c0 8.8-7.2 16-16 16L64 464c-8.8 0-16-7.2-16-16L48 64c0-8.8 7.2-16 16-16zM224 67.9l92.1 92.1-68.1 0c-13.3 0-24-10.7-24-24l0-68.1zM64 0C28.7 0 0 28.7 0 64L0 448c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-261.5c0-17-6.7-33.3-18.7-45.3L242.7 18.7C230.7 6.7 214.5 0 197.5 0L64 0zm71.3 274.2c-3.2-12.9-16.2-20.7-29.1-17.5S85.5 273 88.7 285.8l32 128c2.5 10.2 11.4 17.5 21.9 18.1s20.1-5.7 23.8-15.5l25.5-68.1 25.5 68.1c3.7 9.8 13.3 16.1 23.8 15.5s19.4-7.9 21.9-18.1l32-128c3.2-12.9-4.6-25.9-17.5-29.1s-25.9 4.6-29.1 17.5l-13.3 53.2-20.9-55.8C211 262.2 202 256 192 256s-19 6.2-22.5 15.6l-20.9 55.8-13.3-53.2z"],
    "face-angry": [512, 512, [128544, "angry"], "f556", "M256 48a208 208 0 1 1 0 416 208 208 0 1 1 0-416zm0 464a256 256 0 1 0 0-512 256 256 0 1 0 0 512zm0-144c24.1 0 45.4 11.8 58.5 30 7.7 10.8 22.7 13.2 33.5 5.5s13.2-22.7 5.5-33.5c-21.7-30.2-57.3-50-97.5-50s-75.7 19.8-97.5 50c-7.7 10.8-5.3 25.8 5.5 33.5s25.8 5.3 33.5-5.5c13.1-18.2 34.4-30 58.5-30zm-80-96c17.7 0 32-14.3 32-32l0-.3 9.7 3.2c10.5 3.5 21.8-2.2 25.3-12.6s-2.2-21.8-12.6-25.3l-96-32c-10.5-3.5-21.8 2.2-25.3 12.6s2.2 21.8 12.6 25.3l28.9 9.6c-4.1 5.4-6.6 12.1-6.6 19.4 0 17.7 14.3 32 32 32zm192-32c0-7.3-2.4-14-6.6-19.4l28.9-9.6c10.5-3.5 16.1-14.8 12.6-25.3s-14.8-16.1-25.3-12.6l-96 32c-10.5 3.5-16.1 14.8-12.6 25.3s14.8 16.1 25.3 12.6l9.7-3.2 0 .3c0 17.7 14.3 32 32 32s32-14.3 32-32z"],
    "chess-knight": [448, 512, [9822], "f441", "M232-32c110.5 0 200 89.5 200 200l0 127.7c0 18.9-6.1 37.1-17.2 52.2l-5.1 6.2-36.3 40.7 32.1 40.2c6.7 8.4 10.4 18.8 10.4 29.6l-.2 4.8c-2.4 23.9-22.6 42.5-47.1 42.5l-289.2 0-4.8-.2c-23.9-2.4-42.5-22.6-42.5-47.1 0-10.8 3.7-21.2 10.4-29.6l37.6-47 0-24.3c0-24.3 10.1-47.6 27.8-64.2l63.5-59.5-17.4 0-.2 .2c-20.3 20.3-49.6 28.2-77.1 21.1l-5.5-1.6c-30.9-10.3-52.3-38-54.9-70.1l-.2-6.4 0-1.4c0-19.7 7.1-38.8 19.9-53.8l76.1-88.8 0-47.1 .1-2.5C113.4-22.6 123.6-32 136-32l96 0zM80.7 464l286.6 0-38.4-48-209.9 0-38.4 48zM160 48c0 5.7-2.1 11.3-5.8 15.6L72.3 159.1C67 165.4 64 173.4 64 181.7l0 1.4 .4 5.2c1.9 11.9 10.3 21.9 21.9 25.8l4.5 1.1c10.5 1.9 21.3-1.4 29-9l7.2-7.2 3.7-3c3.9-2.6 8.5-4 13.3-4l88 0c9.8 0 18.7 6 22.3 15.2s1.3 19.6-5.9 26.3l-107.8 101c-8.1 7.6-12.7 18.1-12.7 29.2l0 4.3 205.2 0 40.7-45.8 2.3-2.8c5.1-6.8 7.8-15.2 7.8-23.7L384 168c0-83.9-68.1-152-152-152l-72 0 0 32zm32 72a24 24 0 1 1 0-48 24 24 0 1 1 0 48z"],
    "face-grin-beam": [512, 512, [128516, "grin-beam"], "f582", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm372.2 46.3c11.8-3.6 23.7 6.1 19.6 17.8-19.8 55.9-73.1 96-135.8 96-62.7 0-116-40-135.8-95.9-4.1-11.6 7.8-21.4 19.6-17.8 34.7 10.6 74.2 16.5 116.1 16.5 42 0 81.5-6 116.3-16.6zM176 180c-15.5 0-28 12.5-28 28l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-37.6 30.4-68 68-68s68 30.4 68 68l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-15.5-12.5-28-28-28zm132 28l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-37.6 30.4-68 68-68s68 30.4 68 68l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-15.5-12.5-28-28-28s-28 12.5-28 28z"],
    "hand-peace": [448, 512, [9996], "f25b", "M219 1.4c-35.2-3.7-66.6 21.8-70.3 57l-6.4 60.6-17.4-49.4C113.2 36.3 76.6 18.8 43.3 30.5S-7.6 78.8 4.1 112.1L56.9 262.2C41.7 276.7 32.2 297.3 32.2 320l0 24c0 92.8 75.2 168 168 168l48 0c92.8 0 168-75.2 168-168l0-120c0-35.3-28.7-64-64-64-7.9 0-15.4 1.4-22.4 4-10.4-21.3-32.3-36-57.6-36-.7 0-1.5 0-2.2 0l5.9-56.3c3.7-35.2-21.8-66.6-57-70.3zm-.2 155.4c-6.6 10.1-10.5 22.2-10.5 35.2l0 48c0 .7 0 1.4 0 2-5.1-1.3-10.5-2-16-2l-7.4 0-5.4-15.3 17-161.3c.9-8.8 8.8-15.2 17.6-14.2s15.2 8.8 14.2 17.6l-9.5 90.1zM79.6 85.6l54.3 154.4-21.7 0c-4 0-8 .3-11.9 .9L49.4 96.2c-2.9-8.3 1.5-17.5 9.8-20.4s17.5 1.5 20.4 9.8zM256.2 192c0-8.8 7.2-16 16-16s16 7.2 16 16l0 48c0 8.8-7.2 16-16 16s-16-7.2-16-16l0-48zm38.4 108c10.4 21.3 32.3 36 57.6 36 5.5 0 10.9-.7 16-2l0 10c0 66.3-53.7 120-120 120l-48 0c-66.3 0-120-53.7-120-120l0-24c0-17.7 14.3-32 32-32l80 0c8.8 0 16 7.2 16 16s-7.2 16-16 16l-40 0c-13.3 0-24 10.7-24 24s10.7 24 24 24l40 0c35.3 0 64-28.7 64-64 0-.7 0-1.4 0-2 5.1 1.3 10.5 2 16 2 7.9 0 15.4-1.4 22.4-4zm73.6-28c0 8.8-7.2 16-16 16s-16-7.2-16-16l0-48c0-8.8 7.2-16 16-16s16 7.2 16 16l0 48z"],
    "compass": [512, 512, [129517], "f14e", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm306.7 69.1L162.4 380.6c-19.4 7.5-38.5-11.6-31-31l55.5-144.3c3.3-8.5 9.9-15.1 18.4-18.4l144.3-55.5c19.4-7.5 38.5 11.6 31 31L325.1 306.7c-3.3 8.5-9.9 15.1-18.4 18.4zM288 256a32 32 0 1 0 -64 0 32 32 0 1 0 64 0z"],
    "square": [448, 512, [9632, 9723, 9724, 61590], "f0c8", "M384 80c8.8 0 16 7.2 16 16l0 320c0 8.8-7.2 16-16 16L64 432c-8.8 0-16-7.2-16-16L48 96c0-8.8 7.2-16 16-16l320 0zM64 32C28.7 32 0 60.7 0 96L0 416c0 35.3 28.7 64 64 64l320 0c35.3 0 64-28.7 64-64l0-320c0-35.3-28.7-64-64-64L64 32z"],
    "face-grin": [512, 512, [128512, "grin"], "f580", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm372.2 46.3c11.8-3.6 23.7 6.1 19.6 17.8-19.8 55.9-73.1 96-135.8 96-62.7 0-116-40-135.8-95.9-4.1-11.6 7.8-21.4 19.6-17.8 34.7 10.6 74.2 16.5 116.1 16.5 42 0 81.5-6 116.3-16.6zM144 208a32 32 0 1 1 64 0 32 32 0 1 1 -64 0zm192-32a32 32 0 1 1 0 64 32 32 0 1 1 0-64z"],
    "face-smile": [512, 512, [128578, "smile"], "f118", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm177.3 63.4C192.3 335 218.4 352 256 352s63.7-17 78.7-32.6c9.2-9.6 24.4-9.9 33.9-.7s9.9 24.4 .7 33.9c-22.1 23-60 47.4-113.3 47.4s-91.2-24.4-113.3-47.4c-9.2-9.6-8.9-24.8 .7-33.9s24.8-8.9 33.9 .7zM144 208a32 32 0 1 1 64 0 32 32 0 1 1 -64 0zm192-32a32 32 0 1 1 0 64 32 32 0 1 1 0-64z"],
    "face-smile-beam": [512, 512, [128522, "smile-beam"], "f5b8", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm177.3 63.4C192.3 335 218.4 352 256 352s63.7-17 78.7-32.6c9.2-9.6 24.4-9.9 33.9-.7s9.9 24.4 .7 33.9c-22.1 23-60 47.4-113.3 47.4s-91.2-24.4-113.3-47.4c-9.2-9.6-8.9-24.8 .7-33.9s24.8-8.9 33.9 .7zM176 180c-15.5 0-28 12.5-28 28l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-37.6 30.4-68 68-68s68 30.4 68 68l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-15.5-12.5-28-28-28zm132 28l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-37.6 30.4-68 68-68s68 30.4 68 68l0 8c0 11-9 20-20 20s-20-9-20-20l0-8c0-15.5-12.5-28-28-28s-28 12.5-28 28z"],
    "folder-closed": [512, 512, [], "e185", "M448 400L64 400c-8.8 0-16-7.2-16-16l0-144 416 0 0 144c0 8.8-7.2 16-16 16zm16-208l-416 0 0-96c0-8.8 7.2-16 16-16l138.7 0c3.5 0 6.8 1.1 9.6 3.2L250.7 112c13.8 10.4 30.7 16 48 16L448 128c8.8 0 16 7.2 16 16l0 48zM64 448l384 0c35.3 0 64-28.7 64-64l0-240c0-35.3-28.7-64-64-64L298.7 80c-6.9 0-13.7-2.2-19.2-6.4L241.1 44.8C230 36.5 216.5 32 202.7 32L64 32C28.7 32 0 60.7 0 96L0 384c0 35.3 28.7 64 64 64z"],
    "keyboard": [576, 512, [9000], "f11c", "M64 112c-8.8 0-16 7.2-16 16l0 256c0 8.8 7.2 16 16 16l448 0c8.8 0 16-7.2 16-16l0-256c0-8.8-7.2-16-16-16L64 112zM0 128C0 92.7 28.7 64 64 64l448 0c35.3 0 64 28.7 64 64l0 256c0 35.3-28.7 64-64 64L64 448c-35.3 0-64-28.7-64-64L0 128zM176 320l224 0c8.8 0 16 7.2 16 16l0 16c0 8.8-7.2 16-16 16l-224 0c-8.8 0-16-7.2-16-16l0-16c0-8.8 7.2-16 16-16zm-72-72c0-8.8 7.2-16 16-16l16 0c8.8 0 16 7.2 16 16l0 16c0 8.8-7.2 16-16 16l-16 0c-8.8 0-16-7.2-16-16l0-16zm16-96l16 0c8.8 0 16 7.2 16 16l0 16c0 8.8-7.2 16-16 16l-16 0c-8.8 0-16-7.2-16-16l0-16c0-8.8 7.2-16 16-16zm64 96c0-8.8 7.2-16 16-16l16 0c8.8 0 16 7.2 16 16l0 16c0 8.8-7.2 16-16 16l-16 0c-8.8 0-16-7.2-16-16l0-16zm16-96l16 0c8.8 0 16 7.2 16 16l0 16c0 8.8-7.2 16-16 16l-16 0c-8.8 0-16-7.2-16-16l0-16c0-8.8 7.2-16 16-16zm64 96c0-8.8 7.2-16 16-16l16 0c8.8 0 16 7.2 16 16l0 16c0 8.8-7.2 16-16 16l-16 0c-8.8 0-16-7.2-16-16l0-16zm16-96l16 0c8.8 0 16 7.2 16 16l0 16c0 8.8-7.2 16-16 16l-16 0c-8.8 0-16-7.2-16-16l0-16c0-8.8 7.2-16 16-16zm64 96c0-8.8 7.2-16 16-16l16 0c8.8 0 16 7.2 16 16l0 16c0 8.8-7.2 16-16 16l-16 0c-8.8 0-16-7.2-16-16l0-16zm16-96l16 0c8.8 0 16 7.2 16 16l0 16c0 8.8-7.2 16-16 16l-16 0c-8.8 0-16-7.2-16-16l0-16c0-8.8 7.2-16 16-16zm64 96c0-8.8 7.2-16 16-16l16 0c8.8 0 16 7.2 16 16l0 16c0 8.8-7.2 16-16 16l-16 0c-8.8 0-16-7.2-16-16l0-16zm16-96l16 0c8.8 0 16 7.2 16 16l0 16c0 8.8-7.2 16-16 16l-16 0c-8.8 0-16-7.2-16-16l0-16c0-8.8 7.2-16 16-16z"],
    "face-rolling-eyes": [512, 512, [128580, "meh-rolling-eyes"], "f5a5", "M256 48a208 208 0 1 1 0 416 208 208 0 1 1 0-416zm0 464a256 256 0 1 0 0-512 256 256 0 1 0 0 512zM176 376c0 13.3 10.7 24 24 24l112 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-112 0c-13.3 0-24 10.7-24 24zM160 264c-22.1 0-40-17.9-40-40 0-9.5 3.3-18.1 8.8-25 3.2 14.3 16 25 31.2 25s28-10.7 31.2-25c5.5 6.8 8.8 15.5 8.8 25 0 22.1-17.9 40-40 40zm0 40a80 80 0 1 0 0-160 80 80 0 1 0 0 160zm192-40c-22.1 0-40-17.9-40-40 0-9.5 3.3-18.1 8.8-25 3.2 14.3 16 25 31.2 25s28-10.7 31.2-25c5.5 6.8 8.8 15.5 8.8 25 0 22.1-17.9 40-40 40zm0 40a80 80 0 1 0 0-160 80 80 0 1 0 0 160z"],
    "face-grimace": [512, 512, [128556, "grimace"], "f57f", "M256 48a208 208 0 1 0 0 416 208 208 0 1 0 0-416zM512 256a256 256 0 1 1 -512 0 256 256 0 1 1 512 0zM152 352c0 11.9 8.6 21.8 20 23.7l0-47.3c-11.4 1.9-20 11.8-20 23.7zm84 24l0-48-24 0 0 48 24 0zm64 0l0-48-24 0 0 48 24 0zm40-.3c11.4-1.9 20-11.8 20-23.7s-8.6-21.8-20-23.7l0 47.3zM176 288l160 0c35.3 0 64 28.7 64 64s-28.7 64-64 64l-160 0c-35.3 0-64-28.7-64-64s28.7-64 64-64zm0-112a32 32 0 1 1 0 64 32 32 0 1 1 0-64zm128 32a32 32 0 1 1 64 0 32 32 0 1 1 -64 0z"],
    "circle-dot": [512, 512, [128280, "dot-circle"], "f192", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm307.2 0a51.2 51.2 0 1 0 -102.4 0 51.2 51.2 0 1 0 102.4 0zM160 256a96 96 0 1 1 192 0 96 96 0 1 1 -192 0z"],
    "object-group": [576, 512, [], "f247", "M40 64a24 24 0 1 1 48 0 24 24 0 1 1 -48 0zm48 59.3c16-6.5 28.9-19.3 35.3-35.3l329.3 0c6.5 16 19.3 28.9 35.3 35.3l0 265.3c-16 6.5-28.9 19.3-35.3 35.3l-329.3 0c-6.5-16-19.3-28.9-35.3-35.3l0-265.3zM512 0c-26.9 0-49.9 16.5-59.3 40L123.3 40C113.9 16.5 90.9 0 64 0 28.7 0 0 28.7 0 64 0 90.9 16.5 113.9 40 123.3l0 265.3c-23.5 9.5-40 32.5-40 59.3 0 35.3 28.7 64 64 64 26.9 0 49.9-16.5 59.3-40l329.3 0c9.5 23.5 32.5 40 59.3 40 35.3 0 64-28.7 64-64 0-26.9-16.5-49.9-40-59.3l0-265.3c23.5-9.5 40-32.5 40-59.3 0-35.3-28.7-64-64-64zM488 64a24 24 0 1 1 48 0 24 24 0 1 1 -48 0zM64 424a24 24 0 1 1 0 48 24 24 0 1 1 0-48zm424 24a24 24 0 1 1 48 0 24 24 0 1 1 -48 0zM192 176l88 0 0 56-88 0 0-56zm-8-40c-17.7 0-32 14.3-32 32l0 72c0 17.7 14.3 32 32 32l104 0c17.7 0 32-14.3 32-32l0-72c0-17.7-14.3-32-32-32l-104 0zm72 184l0 24c0 17.7 14.3 32 32 32l104 0c17.7 0 32-14.3 32-32l0-72c0-17.7-14.3-32-32-32l-24 0c0 14.6-3.9 28.2-10.7 40l26.7 0 0 56-88 0 0-16.4c-2.6 .3-5.3 .4-8 .4l-32 0z"],
    "face-flushed": [512, 512, [128563, "flushed"], "f579", "M464 256a208 208 0 1 1 -416 0 208 208 0 1 1 416 0zM256 0a256 256 0 1 0 0 512 256 256 0 1 0 0-512zM160 248a24 24 0 1 0 0-48 24 24 0 1 0 0 48zm216-24a24 24 0 1 0 -48 0 24 24 0 1 0 48 0zM192 352c-13.3 0-24 10.7-24 24s10.7 24 24 24l128 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-128 0zM160 176a48 48 0 1 1 0 96 48 48 0 1 1 0-96zm0 128a80 80 0 1 0 0-160 80 80 0 1 0 0 160zm144-80a48 48 0 1 1 96 0 48 48 0 1 1 -96 0zm128 0a80 80 0 1 0 -160 0 80 80 0 1 0 160 0z"],
    "star-half-stroke": [576, 512, ["star-half-alt"], "f5c0", "M309.5-18.9c-4.1-8-12.4-13.1-21.4-13.1s-17.3 5.1-21.4 13.1L193.1 125.3 33.2 150.7c-8.9 1.4-16.3 7.7-19.1 16.3s-.5 18 5.8 24.4l114.4 114.5-25.2 159.9c-1.4 8.9 2.3 17.9 9.6 23.2s16.9 6.1 25 2L288.1 417.6 432.4 491c8 4.1 17.7 3.3 25-2s11-14.2 9.6-23.2L441.7 305.9 556.1 191.4c6.4-6.4 8.6-15.8 5.8-24.4s-10.1-14.9-19.1-16.3L383 125.3 309.5-18.9zM264.1 91.8l0 284.1-100.1 50.9 19.8-125.5c1.2-7.6-1.3-15.3-6.7-20.7l-89.8-89.9 125.5-20c7.6-1.2 14.1-6 17.6-12.8l33.8-66.2zm48 284.1l0-284.1 33.8 66.2c3.5 6.8 10 11.6 17.6 12.8l125.5 20-89.8 89.9c-5.4 5.4-7.9 13.1-6.7 20.7l19.8 125.5-100.1-50.9z"],
    "file-video": [384, 512, [], "f1c8", "M64 48l112 0 0 88c0 39.8 32.2 72 72 72l88 0 0 240c0 8.8-7.2 16-16 16L64 464c-8.8 0-16-7.2-16-16L48 64c0-8.8 7.2-16 16-16zM224 67.9l92.1 92.1-68.1 0c-13.3 0-24-10.7-24-24l0-68.1zM64 0C28.7 0 0 28.7 0 64L0 448c0 35.3 28.7 64 64 64l256 0c35.3 0 64-28.7 64-64l0-261.5c0-17-6.7-33.3-18.7-45.3L242.7 18.7C230.7 6.7 214.5 0 197.5 0L64 0zM80 288l0 96c0 17.7 14.3 32 32 32l96 0c17.7 0 32-14.3 32-32l0-24 35 35c3.2 3.2 7.5 5 12 5 9.4 0 17-7.6 17-17l0-94.1c0-9.4-7.6-17-17-17-4.5 0-8.8 1.8-12 5l-35 35 0-24c0-17.7-14.3-32-32-32l-96 0c-17.7 0-32 14.3-32 32z"],
    "face-laugh": [512, 512, ["laugh"], "f599", "M464 256a208 208 0 1 0 -416 0 208 208 0 1 0 416 0zM0 256a256 256 0 1 1 512 0 256 256 0 1 1 -512 0zm118.3 58.2c-4.2-13.7 7.1-26.2 21.4-26.2l232.6 0c14.3 0 25.6 12.5 21.4 26.2-18 58.9-72.9 101.8-137.7 101.8S136.3 373.1 118.3 314.2zM144 192a32 32 0 1 1 64 0 32 32 0 1 1 -64 0zm192-32a32 32 0 1 1 0 64 32 32 0 1 1 0-64z"],
    "hand-pointer": [448, 512, [], "f25a", "M160 64c0-8.8 7.2-16 16-16s16 7.2 16 16l0 136c0 10.3 6.6 19.5 16.4 22.8s20.6-.1 26.8-8.3c3-3.9 7.6-6.4 12.8-6.4 8.8 0 16 7.2 16 16 0 10.3 6.6 19.5 16.4 22.8s20.6-.1 26.8-8.3c3-3.9 7.6-6.4 12.8-6.4 7.8 0 14.3 5.6 15.7 13 1.6 8.2 7.3 15.1 15.1 18s16.7 1.6 23.3-3.6c2.7-2.1 6.1-3.4 9.9-3.4 8.8 0 16 7.2 16 16l0 120c0 39.8-32.2 72-72 72l-116.6 0c-37.4 0-72.4-18.7-93.2-49.9L50.7 312.9c-4.9-7.4-2.9-17.3 4.4-22.2s17.3-2.9 22.2 4.4L116 353.2c5.9 8.8 16.8 12.7 26.9 9.7s17-12.4 17-23L160 64zM176 0c-35.3 0-64 28.7-64 64l0 197.7C91.2 238 55.5 232.8 28.5 250.7-.9 270.4-8.9 310.1 10.8 339.5L78.3 440.8c29.7 44.5 79.6 71.2 133.1 71.2L328 512c66.3 0 120-53.7 120-120l0-120c0-35.3-28.7-64-64-64-4.5 0-8.8 .5-13 1.3-11.7-15.4-30.2-25.3-51-25.3-6.9 0-13.5 1.1-19.7 3.1-11.6-16.4-30.7-27.1-52.3-27.1-2.7 0-5.4 .2-8 .5L240 64c0-35.3-28.7-64-64-64zm48 304c0-8.8-7.2-16-16-16s-16 7.2-16 16l0 96c0 8.8 7.2 16 16 16s16-7.2 16-16l0-96zm48-16c-8.8 0-16 7.2-16 16l0 96c0 8.8 7.2 16 16 16s16-7.2 16-16l0-96c0-8.8-7.2-16-16-16zm80 16c0-8.8-7.2-16-16-16s-16 7.2-16 16l0 96c0 8.8 7.2 16 16 16s16-7.2 16-16l0-96z"],
    "registered": [512, 512, [174], "f25d", "M256 48a208 208 0 1 1 0 416 208 208 0 1 1 0-416zm0 464a256 256 0 1 0 0-512 256 256 0 1 0 0 512zM200 144c-13.3 0-24 10.7-24 24l0 176c0 13.3 10.7 24 24 24s24-10.7 24-24l0-56 34.4 0 41 68.3c6.8 11.4 21.6 15 32.9 8.2s15-21.6 8.2-32.9l-30.2-50.3c24.6-11.5 41.6-36.4 41.6-65.3 0-39.8-32.2-72-72-72l-80 0zm72 96l-48 0 0-48 56 0c13.3 0 24 10.7 24 24s-10.7 24-24 24l-8 0z"]
  };

  bunker(function () {
    defineIcons('far', icons);
    defineIcons('fa-regular', icons);
  });

}());
