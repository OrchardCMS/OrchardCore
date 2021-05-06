
'use strict';
var validationSet = new Set();
var validationRuleApi = "/api/validationApi/ValidateFormByRule";

function checkIE() {
    if (/MSIE (\d+\.\d+);/.test(navigator.userAgent)) {
        var ieversion = new Number(RegExp.$1);
        if (ieversion <= 8) return true;
        return false;
    }
}


function validationEvent(contentItemId, validationElementId, elementId) {
    var validationElement = document.getElementById(validationElementId);
    var elementName = $(elementId).attr('name');
    var elementValue = $(elementId).val();
    $.ajax({
        url: validationRuleApi + "?contentItemId=" + contentItemId + "&formName=" + elementName + "&formValue=" + elementValue
    })
        .done(function (data) {
            if (validationElement) {
                validationElement.remove();
                validationSet.delete(validationElementId);
                if (validationSet.size === 0) {
                    $(elementId).closest('form').find(':submit').removeAttr('disabled');
                }
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            if (!validationElement) {
                var errorMessage = jqXHR.responseJSON.value;
                var dangerText = '<div id="' + validationElementId + '" class="text-danger">' + errorMessage + '</div>';
                $(elementId).after(dangerText);
                validationSet.add(validationElementId);
                $(elementId).closest('form').find(':submit').attr('disabled', 'true');
            }
        });
}

function validationElementFunction(contentItemId, originElementId) {
    var isIE = checkIE();
    var elementId = '#' + originElementId;
    var timestamp = new Date().getTime();
    var validationElementId = originElementId + timestamp;

    //if (window.validatorObject) {
    //    var obj = { elementId: originElementId};
    //    window.validatorObject.push(obj);
    //}
    //else {
    //    window.validatorObject = new Array();
    //    var obj = { elementId: originElementId };
    //    window.validatorObject.push(obj);
    //}

    $(elementId).bind('change', function (e) {
        validationEvent(contentItemId, validationElementId, elementId);
    })
    var submitBtn = $(elementId).closest('form').find(':submit')[0];

    if (submitBtn) {
        if (isIE) {
            submitBtn.attachEvent('click', function () {
                validationEvent(contentItemId, validationElementId, elementId);
            })
        }
        else {
            submitBtn.addEventListener('click', function (e) {
                validationEvent(contentItemId, validationElementId, elementId);
            })
        }
    }
}

function validationTypeChange(e, option = '', message = '') {
    var selectedValidationOptionItem = e.options[e.options.selectedIndex];
    var isShowOption = selectedValidationOptionItem.getAttribute('is-show-option');
    var isShowValidationMessage = selectedValidationOptionItem.getAttribute('is-show-validationMessage');
    var validationErrorMessage = $(e.closest('.edit-item-parts')).find('.validation-error-message');
    var validationOption = $(e.closest('.edit-item-parts')).find('.validation-option');

    if (isShowOption === "False") {
        validationOption.css('display', 'none');
    }
    else {
        validationOption.css('display', 'block');
        var validationOptionInput = validationOption.find('input');
        var dataValidationOption = selectedValidationOptionItem.getAttribute('data-validation-option');
        validationOptionInput.attr('placeholder', dataValidationOption);

        if (option === '') {
            validationOptionInput.val('');
        }
        else {
            validationOptionInput.html(option);
        }
    }

    if (isShowValidationMessage === 'False') {
        validationErrorMessage.css('display', 'none');
    }
    else {
        validationErrorMessage.css('display', 'block');
        var validationMessageInput = validationErrorMessage.find('input');

        if (message === '') {
            validationMessageInput.val('');
        }
        else {
            validationMessageInput.html(message);
        }
    }
}
