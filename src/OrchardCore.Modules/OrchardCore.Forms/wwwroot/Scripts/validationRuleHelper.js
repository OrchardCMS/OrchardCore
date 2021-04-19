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

function validationTypeChange(e, option = '', message = '') {
    var selectedValidationOptionItem = e.options[e.options.selectedIndex];
    var isShowOption = selectedValidationOptionItem.getAttribute('is-show-option');
    var isShowValidationMessage = selectedValidationOptionItem.getAttribute('is-show-validationMessage');
    var validationMessage = $(e.closest('.edit-item-parts')).find('.validation-message');
    var validationOption = $(e.closest('.edit-item-parts')).find('.validation-option');

    if (isShowOption === "0") {
        validationOption.css('display', 'none');
    } else {
        validationOption.css('display', 'block');
        var validationOptionInput = validationOption.find('input');
        var dataValidationOption = selectedValidationOptionItem.getAttribute('data-validation-option');
        validationOptionInput.attr('placeholder', dataValidationOption);
        if (option === '') {
            validationOptionInput.val('');
        } else {
            validationOptionInput.html(option);
        }
    }

    if (isShowValidationMessage === '0') {
        validationMessage.css('display', 'none');
    } else {
        validationMessage.css('display', 'block');
        var validationMessageInput = validationMessage.find('input');
        if (message === '') {
            validationMessageInput.val('');
        } else {
            validationMessageInput.html(message);
        }
   
    }
}

