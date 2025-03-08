function getPrefixIndex(prefix, container) {
    return Math.max(...container.querySelectorAll(`input[name*='${prefix}-']`).values().map(input => parseInt(input.value.substring(input.value.lastIndexOf('-') + 1)) || 0)) + 1;
}

(function () {
    document.addEventListener('click', function (event) {
        if (event.target.classList.contains('add-list-widget')) {
            var type = event.target.dataset.widgetType;
            var targetId = event.target.dataset.targetId;
            var htmlFieldPrefix = event.target.dataset.htmlFieldPrefix;
            var createEditorUrl = document.getElementById(targetId).dataset.buildeditorurl;
            var prefixesName = event.target.dataset.prefixesName;
            var parentContentType = event.target.dataset.parentContentType;
            var partName = event.target.dataset.partName;
            var zonesName = event.target.dataset.zonesName;
            var zone = event.target.dataset.zone;

            var index = getPrefixIndex(htmlFieldPrefix, document.getElementById(targetId).closest("form"));
            var prefix = htmlFieldPrefix + '-' + index.toString();

            var contentTypesName = event.target.dataset.contenttypesName;
            var contentItemsName = event.target.dataset.contentitemsName;
            fetch(`${createEditorUrl}?id=${type}&prefix=${prefix}&prefixesName=${prefixesName}&contentTypesName=${contentTypesName}&contentItemsName=${contentItemsName}&zonesName=${zonesName}&zone=${zone}&targetId=${targetId}&parentContentType=${parentContentType}&partName=${partName}`)
                .then(response => response.json())
                .then(data => {
                    document.getElementById(targetId).insertAdjacentHTML('beforeend', data.Content);

                    var dom = new DOMParser().parseFromString(data.Scripts, 'text/html');
                    Array.from(dom.querySelectorAll('script')).forEach(script => {
                        eval(script.textContent || script.innerHTML);
                    });
                });
        } else if (event.target.classList.contains('insert-list-widget')) {
            var type = event.target.dataset.widgetType;
            var target = event.target.closest('.widget-template');
            var targetId = event.target.dataset.targetId;
            var htmlFieldPrefix = event.target.dataset.htmlFieldPrefix;
            var createEditorUrl = document.getElementById(targetId).dataset.buildeditorurl;
            var prefixesName = event.target.dataset.prefixesName;
            var parentContentType = event.target.dataset.parentContentType;
            var partName = event.target.dataset.partName;
            var zonesName = event.target.dataset.zonesName;
            var zone = event.target.dataset.zone;

            var index = getPrefixIndex(htmlFieldPrefix, document.getElementById(targetId).closest("form"));
            var prefix = htmlFieldPrefix + '-' + index.toString();

            var contentTypesName = event.target.dataset.contenttypesName;
            var contentItemsName = event.target.dataset.contentitemsName;
            fetch(`${createEditorUrl}?id=${type}&prefix=${prefix}&prefixesName=${prefixesName}&contentTypesName=${contentTypesName}&contentItemsName=${contentItemsName}&zonesName=${zonesName}&zone=${zone}&targetId=${targetId}&parentContentType=${parentContentType}&partName=${partName}`)
                .then(response => response.json())
                .then(data => {
                    target.insertAdjacentHTML('beforebegin', data.Content);

                    var dom = new DOMParser().parseFromString(data.Scripts, 'text/html');
                    Array.from(dom.querySelectorAll('script')).forEach(script => {
                        eval(script.textContent || script.innerHTML);
                    });
                });
        }
    });

    document.addEventListener('click', function (event) {
        if (event.target.classList.contains('widget-list-delete')) {
            var $this = event.target;
            confirmDialog(_objectSpread({}, $this.dataset, {
                callback: function callback(r) {
                    if (r) {
                        $this.closest('.widget-template').remove();
                        document.dispatchEvent(new CustomEvent('contentpreview:render'));
                    }
                }
            }));
        }
    });

    document.addEventListener('change', function (event) {
        if (event.target.closest('.widget-editor-footer')) {
            document.dispatchEvent(new CustomEvent('contentpreview:render'));
        }
    });

    document.addEventListener('click', function (event) {
        if (event.target.classList.contains('widget-list-editor-btn-toggle')) {
            event.target.closest('.widget-editor').classList.toggle('collapsed');
        }
    });
})();

