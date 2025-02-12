// modules are defined as an array
// [ module function, map of requires ]
//
// map of requires is short require name -> numeric require
//
// anything defined in a previous bundle is accessed via the
// orig method which is the require for previous bundles

(function (modules, entry, mainEntry, parcelRequireName, globalName) {
  /* eslint-disable no-undef */
  var globalObject =
    typeof globalThis !== 'undefined'
      ? globalThis
      : typeof self !== 'undefined'
      ? self
      : typeof window !== 'undefined'
      ? window
      : typeof global !== 'undefined'
      ? global
      : {};
  /* eslint-enable no-undef */

  // Save the require from previous bundle to this closure if any
  var previousRequire =
    typeof globalObject[parcelRequireName] === 'function' &&
    globalObject[parcelRequireName];

  var cache = previousRequire.cache || {};
  // Do not use `require` to prevent Webpack from trying to bundle this call
  var nodeRequire =
    typeof module !== 'undefined' &&
    typeof module.require === 'function' &&
    module.require.bind(module);

  function newRequire(name, jumped) {
    if (!cache[name]) {
      if (!modules[name]) {
        // if we cannot find the module within our internal map or
        // cache jump to the current global require ie. the last bundle
        // that was added to the page.
        var currentRequire =
          typeof globalObject[parcelRequireName] === 'function' &&
          globalObject[parcelRequireName];
        if (!jumped && currentRequire) {
          return currentRequire(name, true);
        }

        // If there are other bundles on this page the require from the
        // previous one is saved to 'previousRequire'. Repeat this as
        // many times as there are bundles until the module is found or
        // we exhaust the require chain.
        if (previousRequire) {
          return previousRequire(name, true);
        }

        // Try the node require function if it exists.
        if (nodeRequire && typeof name === 'string') {
          return nodeRequire(name);
        }

        var err = new Error("Cannot find module '" + name + "'");
        err.code = 'MODULE_NOT_FOUND';
        throw err;
      }

      localRequire.resolve = resolve;
      localRequire.cache = {};

      var module = (cache[name] = new newRequire.Module(name));

      modules[name][0].call(
        module.exports,
        localRequire,
        module,
        module.exports,
        globalObject
      );
    }

    return cache[name].exports;

    function localRequire(x) {
      var res = localRequire.resolve(x);
      return res === false ? {} : newRequire(res);
    }

    function resolve(x) {
      var id = modules[name][1][x];
      return id != null ? id : x;
    }
  }

  function Module(moduleName) {
    this.id = moduleName;
    this.bundle = newRequire;
    this.exports = {};
  }

  newRequire.isParcelRequire = true;
  newRequire.Module = Module;
  newRequire.modules = modules;
  newRequire.cache = cache;
  newRequire.parent = previousRequire;
  newRequire.register = function (id, exports) {
    modules[id] = [
      function (require, module) {
        module.exports = exports;
      },
      {},
    ];
  };

  Object.defineProperty(newRequire, 'root', {
    get: function () {
      return globalObject[parcelRequireName];
    },
  });

  globalObject[parcelRequireName] = newRequire;

  for (var i = 0; i < entry.length; i++) {
    newRequire(entry[i]);
  }

  if (mainEntry) {
    // Expose entry point to Node, AMD or browser globals
    // Based on https://github.com/ForbesLindesay/umd/blob/master/template.js
    var mainExports = newRequire(mainEntry);

    // CommonJS
    if (typeof exports === 'object' && typeof module !== 'undefined') {
      module.exports = mainExports;

      // RequireJS
    } else if (typeof define === 'function' && define.amd) {
      define(function () {
        return mainExports;
      });

      // <script>
    } else if (globalName) {
      this[globalName] = mainExports;
    }
  }
})({"iwLq6":[function(require,module,exports,__globalThis) {
var parcelHelpers = require("@parcel/transformer-js/src/esmodule-helpers.js");
var _passwordStrength = require("@orchardcore/frontend/components/password-strength");
var _passwordStrengthDefault = parcelHelpers.interopDefault(_passwordStrength);
// Show or hide the connection string or table prefix section when the database provider is selected
const toggleConnectionStringAndPrefix = ()=>{
    const selectedOption = document.querySelector("#DatabaseProvider option:checked");
    if (selectedOption) {
        const connectionString = selectedOption.getAttribute("data-connection-string")?.toLowerCase() === "true";
        const tablePrefix = selectedOption.getAttribute("data-table-prefix")?.toLowerCase() === "true";
        const connectionStringElements = document.querySelectorAll(".connectionString");
        connectionStringElements.forEach((el)=>el.style.display = connectionString ? "block" : "none");
        const tablePrefixElements = document.querySelectorAll(".tablePrefix");
        tablePrefixElements.forEach((el)=>el.style.display = tablePrefix ? "block" : "none");
        document.querySelectorAll(".pwd").forEach((el)=>{
            if (connectionString) el.setAttribute("required", "required");
            else el.removeAttribute("required");
        });
        const connectionStringHint = document.getElementById("connectionStringHint");
        if (connectionStringHint) connectionStringHint.textContent = selectedOption.getAttribute("data-connection-string-sample") || "";
    }
};
const refreshDescription = (target)=>{
    const recipeName = target.getAttribute("data-recipe-name");
    const recipeDisplayName = target.getAttribute("data-recipe-display-name");
    const recipeDescription = target.getAttribute("data-recipe-description");
    const recipeButton = document.getElementById("recipeButton");
    const recipeNameInput = document.getElementById("RecipeName");
    if (recipeButton && recipeNameInput) {
        recipeButton.textContent = recipeDisplayName || "";
        recipeNameInput.value = recipeName || "";
        recipeButton.setAttribute("title", recipeDescription || "");
        recipeButton.focus();
    }
};
const setLocalizationUrl = ()=>{
    const culturesList = document.getElementById("culturesList");
    culturesList.addEventListener("change", ()=>{
        const selectedOption = culturesList.options[culturesList.selectedIndex];
        if (selectedOption) window.location.href = selectedOption.dataset.url || "";
    });
};
const togglePasswordVisibility = (passwordCtl, togglePasswordCtl)=>{
    if (!passwordCtl || !togglePasswordCtl) return;
    // toggle the type attribute
    const type = passwordCtl.getAttribute("type") === "password" ? "text" : "password";
    passwordCtl.setAttribute("type", type);
    // toggle the eye slash icon
    const icon = togglePasswordCtl.getElementsByClassName("icon")[0];
    if (icon) {
        if (icon.getAttribute("data-icon")) // if the icon is rendered as a svg
        icon.setAttribute("data-icon", type === "password" ? "eye" : "eye-slash");
        else {
            // if the icon is still a <i> element
            icon.classList.toggle("fa-eye", type === "password");
            icon.classList.toggle("fa-eye-slash", type !== "password");
        }
    }
};
const init = ()=>{
    toggleConnectionStringAndPrefix();
    // Show hide the connection string when a provider is selected
    document.getElementById("DatabaseProvider")?.addEventListener("change", function() {
        toggleConnectionStringAndPrefix();
    });
    // Refresh the recipe description
    document.querySelectorAll("#recipes div a").forEach(function(element) {
        element.addEventListener("click", function() {
            refreshDescription(this);
        });
    });
    const passwordElement = document.getElementById("Password");
    const options = JSON.parse(passwordElement?.dataset.strength ?? "") ?? {
        requiredLength: 6,
        requiredUniqueChars: 1,
        requireNonAlphanumeric: true,
        requireLowercase: true,
        requireUppercase: true,
        requireDigit: true
    };
    if (passwordElement) (0, _passwordStrengthDefault.default)(passwordElement, options);
    if (passwordElement) passwordElement.addEventListener("focus", function() {
        const popover = document.createElement("div");
        popover.className = "popover bs-popover-top";
        popover.role = "tooltip";
        popover.innerHTML = `<div class="popover-arrow" style="position: absolute; left: 0px; transform: translate(119px, 0px);"></div><div class="popover-header">Password requirements: </div><div class="popover-body"><ul><li>Minimum length: ${options.requiredLength}</li><li>Unique Chars: ${options.requiredUniqueChars}</li><li>Uppercase: ${options.requireUppercase ? "required" : "not required"}</li><li>Lowercase: ${options.requireLowercase ? "required" : "not required"}</li><li>Digit: ${options.requireDigit ? "required" : "not required"}</li><li>Non alphanumeric: ${options.requireNonAlphanumeric ? "required" : "not required"}</li></ul></div>`;
        const rect = passwordElement.getBoundingClientRect();
        popover.style.position = "absolute";
        popover.style.top = `${rect.top - 228}px`;
        popover.style.left = `${rect.left}px`;
        document.body.appendChild(popover);
        function removePopover() {
            popover.remove();
            passwordElement?.removeEventListener("blur", removePopover);
        }
        passwordElement.addEventListener("blur", removePopover);
    });
    const toggleConnectionString = document.querySelector("#toggleConnectionString");
    if (toggleConnectionString) toggleConnectionString.addEventListener("click", function(e) {
        togglePasswordVisibility(document.querySelector("#ConnectionString"), document.querySelector("#toggleConnectionString"));
    });
    const togglePassword = document.querySelector("#togglePassword");
    togglePassword?.addEventListener("click", function(e) {
        togglePasswordVisibility(document.querySelector("#Password"), document.querySelector("#togglePassword"));
    });
    const togglePasswordConfirmation = document.querySelector("#togglePasswordConfirmation");
    togglePasswordConfirmation?.addEventListener("click", function(e) {
        togglePasswordVisibility(document.querySelector("#PasswordConfirmation"), document.querySelector("#togglePasswordConfirmation"));
    });
    setLocalizationUrl();
};
init();

},{"@orchardcore/frontend/components/password-strength":"02Ujf","@parcel/transformer-js/src/esmodule-helpers.js":"dzhKE"}],"02Ujf":[function(require,module,exports,__globalThis) {
/**
 * This function initializes a password strength checker on a given input element.
 * It evaluates the password based on specified requirements such as minimum length,
 * presence of uppercase, lowercase, digits, and special characters.
 * A visual progress bar is displayed to indicate the strength level of the password.
 *
 * @param {HTMLElement} element - The input element to which the strength checker is applied.
 * @param {Object} options - Configuration options for password requirements and display settings.
 * @param {number} options.requiredLength - Minimum required length of the password.
 * @param {boolean} options.requireUppercase - Whether an uppercase letter is required.
 * @param {boolean} options.requireLowercase - Whether a lowercase letter is required.
 * @param {boolean} options.requireDigit - Whether a digit is required.
 * @param {boolean} options.requireNonAlphanumeric - Whether a special character is required.
 * @param {string} options.target - CSS selector for the element where the strength progress bar is displayed.
 * @param {string} options.style - CSS style string for the progress bar.
 */ var parcelHelpers = require("@parcel/transformer-js/src/esmodule-helpers.js");
parcelHelpers.defineInteropFlag(exports);
exports.default = (element, options)=>{
    const settings = Object.assign({
        requiredLength: 8,
        requireUppercase: false,
        requireLowercase: false,
        requireDigit: false,
        requireNonAlphanumeric: false,
        target: '#passwordStrength',
        style: "margin-top: 7px; height: 7px; border-radius: 5px"
    }, options);
    let capitalletters = 0;
    let lowerletters = 0;
    let numbers = 0;
    let specialchars = 0;
    const upperCase = /[A-Z]/;
    const lowerCase = /[a-z]/;
    const number = /[0-9]/;
    const specialchar = /[^\da-zA-Z]/;
    let valid = false;
    const getPercentage = (a, b)=>b / a * 100;
    const getLevel = (value)=>{
        if (value >= 100) return "bg-success";
        if (value >= 50) return "bg-warning";
        if (value == 0) return ''; // grayed
        return "bg-danger";
    };
    const checkStrength = (value)=>{
        const minLength = value.length >= settings.requiredLength ? 1 : 0;
        capitalletters = !settings.requireUppercase || value.match(upperCase) ? 1 : 0;
        lowerletters = !settings.requireLowercase || value.match(lowerCase) ? 1 : 0;
        numbers = !settings.requireDigit || value.match(number) ? 1 : 0;
        specialchars = !settings.requireNonAlphanumeric || value.match(specialchar) ? 1 : 0;
        const total = minLength + capitalletters + lowerletters + numbers + specialchars;
        const percentage = getPercentage(5, total);
        valid = percentage >= 100;
        createProgressBar(percentage, getLevel(percentage));
    };
    const createProgressBar = (percentage, level)=>{
        const el = document.createElement("div");
        el.className = "progress";
        el.setAttribute("value", percentage.toString());
        el.setAttribute("style", settings.style);
        el.setAttribute("max", "100");
        el.setAttribute("aria-describedby", "");
        const bar = document.createElement("div");
        bar.className = "progress-bar " + level;
        bar.style.width = percentage + "%";
        el.appendChild(bar);
        const target = document.querySelector(settings.target);
        target.innerHTML = "";
        target.appendChild(el);
    };
    element.addEventListener("keyup", ()=>checkStrength(element.value));
    element.addEventListener("keydown", ()=>checkStrength(element.value));
    element.addEventListener("change", ()=>checkStrength(element.value));
    element.addEventListener("drop", (event)=>{
        event.preventDefault();
        checkStrength(event.dataTransfer?.getData("text") ?? "");
    });
    element.form?.addEventListener("submit", (event)=>{
        checkStrength(element.value);
        if (!valid) event.preventDefault();
    });
};

},{"@parcel/transformer-js/src/esmodule-helpers.js":"dzhKE"}],"dzhKE":[function(require,module,exports,__globalThis) {
exports.interopDefault = function(a) {
    return a && a.__esModule ? a : {
        default: a
    };
};
exports.defineInteropFlag = function(a) {
    Object.defineProperty(a, '__esModule', {
        value: true
    });
};
exports.exportAll = function(source, dest) {
    Object.keys(source).forEach(function(key) {
        if (key === 'default' || key === '__esModule' || Object.prototype.hasOwnProperty.call(dest, key)) return;
        Object.defineProperty(dest, key, {
            enumerable: true,
            get: function() {
                return source[key];
            }
        });
    });
    return dest;
};
exports.export = function(dest, destName, get) {
    Object.defineProperty(dest, destName, {
        enumerable: true,
        get: get
    });
};

},{}]},["iwLq6"], "iwLq6", "parcelRequire94c2")

//# sourceMappingURL=setup.js.map
