document.addEventListener('DOMContentLoaded', function () {
    // Browsers don't execute <script> tags inserted via innerHTML, so scripts are extracted
    // server-side into a separate markup fragment and re-created here as real <script> elements,
    // which do execute when appended to the document (mirrors jQuery's $.globalEval trick).
    function evalScripts(html) {
        var container = document.createElement('div');
        container.innerHTML = html;
        container.querySelectorAll('script').forEach(function (oldScript) {
            var newScript = document.createElement('script');
            for (var i = 0; i < oldScript.attributes.length; i++) {
                newScript.setAttribute(oldScript.attributes[i].name, oldScript.attributes[i].value);
            }
            newScript.textContent = oldScript.textContent;
            document.body.appendChild(newScript);
        });
    }

    function getIndexes(targetId, htmlFieldPrefix) {
        // Retrieve all index values knowing that some elements may have been moved / removed.
        return Array.from(document.getElementById(targetId).closest("form").querySelectorAll("input[name*='Prefixes']"))
            .filter(function (e) {
                return e.value.substring(0, e.value.lastIndexOf('-')) === htmlFieldPrefix;
            })
            .map(function (e) {
                return parseInt(e.value.substring(e.value.lastIndexOf('-') + 1)) || 0;
            });
    }

    document.addEventListener('click', function (event) {
        var target = event.target.closest('.add-list-widget');
        if (!target) {
            return;
        }

        var type = target.dataset.widgetType;
        var targetId = target.dataset.targetId;
        var htmlFieldPrefix = target.dataset.htmlFieldPrefix;
        var createEditorUrl = document.getElementById(targetId).dataset.buildeditorurl;
        var prefixesName = target.dataset.prefixesName;
        var parentContentType = target.dataset.parentContentType;
        var partName = target.dataset.partName;
        var zonesName = target.dataset.zonesName;
        var zone = target.dataset.zone;

        var indexes = getIndexes(targetId, htmlFieldPrefix);

        // Use a prefix based on the items count (not a guid) so that the browser autofill still works.
        var index = indexes.length ? Math.max(...indexes) + 1 : 0;
        var prefix = htmlFieldPrefix + '-' + index.toString();

        var contentTypesName = target.dataset.contenttypesName;
        var contentItemsName = target.dataset.contentitemsName;
        fetch(createEditorUrl + "?id=" + type + "&prefix=" + prefix + "&prefixesName=" + prefixesName + "&contentTypesName=" + contentTypesName + "&contentItemsName=" + contentItemsName + "&zonesName=" + zonesName + "&zone=" + zone + "&targetId=" + targetId + "&parentContentType=" + parentContentType + "&partName=" + partName)
            .then(function (response) { return response.text(); })
            .then(function (data) {
                var result = JSON.parse(data);
                document.getElementById(targetId).insertAdjacentHTML('beforeend', result.Content);
                evalScripts(result.Scripts);
            });
    });

    document.addEventListener('click', function (event) {
        var target = event.target.closest('.insert-list-widget');
        if (!target) {
            return;
        }

        var type = target.dataset.widgetType;
        var widgetTemplate = target.closest('.widget-template');
        var targetId = target.dataset.targetId;
        var htmlFieldPrefix = target.dataset.htmlFieldPrefix;
        var createEditorUrl = document.getElementById(targetId).dataset.buildeditorurl;
        var prefixesName = target.dataset.prefixesName;
        var parentContentType = target.dataset.parentContentType;
        var partName = target.dataset.partName;
        var zonesName = target.dataset.zonesName;
        var zone = target.dataset.zone;

        var indexes = getIndexes(targetId, htmlFieldPrefix);

        // Use a prefix based on the items count (not a guid) so that the browser autofill still works.
        var index = indexes.length ? Math.max(...indexes) + 1 : 0;
        var prefix = htmlFieldPrefix + '-' + index.toString();

        var contentTypesName = target.dataset.contenttypesName;
        var contentItemsName = target.dataset.contentitemsName;
        fetch(createEditorUrl + "?id=" + type + "&prefix=" + prefix + "&prefixesName=" + prefixesName + "&contentTypesName=" + contentTypesName + "&contentItemsName=" + contentItemsName + "&zonesName=" + zonesName + "&zone=" + zone + "&targetId=" + targetId + "&parentContentType=" + parentContentType + "&partName=" + partName)
            .then(function (response) { return response.text(); })
            .then(function (data) {
                var result = JSON.parse(data);
                widgetTemplate.insertAdjacentHTML('beforebegin', result.Content);
                evalScripts(result.Scripts);
            });
    });

    document.addEventListener('click', function (event) {
        var target = event.target.closest('.widget-list-delete');
        if (!target) {
            return;
        }

        confirmDialog({
            ...target.dataset,
            callback: function callback(r) {
                if (r) {
                    target.closest('.widget-template').remove();
                    document.dispatchEvent(new CustomEvent('contentpreview:render'));
                }
            }
        });
    });

    document.addEventListener('change', function (event) {
        if (event.target.closest('.widget-editor-footer label')) {
            document.dispatchEvent(new CustomEvent('contentpreview:render'));
        }
    });

    document.addEventListener('click', function (event) {
        var target = event.target.closest('.widget-list-editor-btn-toggle');
        if (target) {
            target.closest('.widget-editor').classList.toggle('collapsed');
        }
    });

});
