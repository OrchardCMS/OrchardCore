
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


function validationEvent(contentItemId, validationElementId, elementId, e) {
    var validationElement = document.getElementById(validationElementId);
    var elementName = $(elementId).attr('name');
    var elementValue = $(elementId).val();

    $.ajax({
        url: validationRuleApi + "?contentItemId=" + contentItemId + "&formName=" + elementName + "&formValue=" + elementValue
    })
        .done(function (data) {
            if (data) {
                if (!validationElement) {
                    var errorMessage = data.errorMessage.value;
                    var dangerText = '<div id="' + validationElementId + '" class="text-danger">' + errorMessage + '</div>';
                    validationSet.add(validationElementId);
                    if ($(elementId).next().attr("class") != 'text-danger') {
                        $(elementId).after(dangerText);
                    }
                    $(elementId).closest('form').find(':submit').attr('disabled', 'true');
                }
            } else {
                if (validationElement) {
                    validationElement.remove();
                    validationSet.delete(validationElementId);
                }
                if (validationSet.size === 0) {
                    var formDom = $(elementId).closest('form');
                    formDom.find(':submit').removeAttr('disabled');
                    if (e) {
                        if (!formDom.find('input[name="contentItemId"]')[0]) {
                            var input = $("<input>")
                                .attr("type", "hidden")
                                .attr("name", "contentItemId").val(contentItemId);
                            formDom.append(input);
                        }
                        $(elementId).closest('form').submit();
                    }
                }
            }

        });
}

function validationElementFunction(contentItemId, originElementId) {
    var isIE = checkIE();
    var elementId = '#' + originElementId;
    var timestamp = new Date().getTime();
    var validationElementId = originElementId + timestamp;
    var submitBtn = $(elementId).closest('form').find(':submit')[0];

    $(elementId).bind('keyup change', function (e) {
        validationEvent(contentItemId, validationElementId, elementId);
    })

    if (submitBtn) {
        if (isIE) {
            submitBtn.attachEvent('click', function (e) {
                e.preventDefault();
                validationEvent(contentItemId, validationElementId, elementId, e);
            })
        }
        else {
            submitBtn.addEventListener('click', function (e) {
                e.preventDefault();
                validationEvent(contentItemId, validationElementId, elementId, e);
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
