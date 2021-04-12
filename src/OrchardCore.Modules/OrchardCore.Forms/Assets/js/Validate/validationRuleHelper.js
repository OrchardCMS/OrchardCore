'use strict';
var validationSet = new Set();

function checkIE() {
    if (/MSIE (\d+\.\d+);/.test(navigator.userAgent)) {
        var ieversion = new Number(RegExp.$1);
        if (ieversion <= 8) return true;
        return false;
    }
}

function validationEvent(type, validationElementId, option, elementId, validationMessage, defaultValidationMessage) {
    var validationResult = false;
    if (window.validator.isEmpty(option)) {
        validationResult = window.validator[type]($(elementId).val());
    }
    else if (window.validator.isJSON(option)) {
        validationResult = window.validator[type]($(elementId).val(), JSON.parse(option));
    }
    else {
        validationResult = window.validator[type]($(elementId).val(), option);
    }
    var validationElement = document.getElementById(validationElementId);
    if (validationResult) {
        if (validationElement) {
            validationElement.remove();
            validationSet.delete(validationElementId);
            if (validationSet.size === 0) {
                $(elementId).closest('form').find(':submit').removeAttr('disabled');
            }
        }
    }
    else {
        if (!validationElement) {
            if ($.isEmptyObject(validationMessage)) {
                validationMessage = defaultValidationMessage;
            }
            var dangerText = '<div id="' + validationElementId + '" class="text-danger">' + validationMessage + '</div>';
            $(elementId).after(dangerText);
            validationSet.add(validationElementId);
            $(elementId).closest('form').find(':submit').attr('disabled', 'true');
        }
    }
    return validationResult;
}
   
function validationElementFunction(type, originElementId, option, validationMessage, defaultValidationMessage) {
    var isIE = checkIE();
    var elementId = '#' + originElementId;
    var timestamp = new Date().getTime();
    var validationElementId = originElementId + timestamp;
    if (window.validatorObject) {
        var obj = { type: type, elementId: originElementId, option: option };
        window.validatorObject.push(obj);
    }
    else {
        window.validatorObject = new Array();
        var obj = { type: type, elementId: originElementId, option: option };
        window.validatorObject.push(obj);
    }
    $(elementId).bind('change keyup', function (e) {
        validationEvent(type, validationElementId, option, elementId, validationMessage, defaultValidationMessage);
    })
    var submitBtn = $(elementId).closest('form').find(':submit')[0];
    if (submitBtn) {
        if (isIE) {
            submitBtn.attachEvent('click', function () {
                validationEvent(type, validationElementId, option, elementId, validationMessage, defaultValidationMessage);
            })
        } else {
            submitBtn.addEventListener('click', function (e) {
                validationEvent(type, validationElementId, option, elementId, validationMessage, defaultValidationMessage);
            })
        }
    }
}


