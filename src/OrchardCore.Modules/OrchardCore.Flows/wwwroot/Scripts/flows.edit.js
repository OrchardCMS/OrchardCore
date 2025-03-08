//variables used in FlowPart.Edit sortable
var widgetDragItem, lastContainer, widgetItemSourceId, widgetItemDestId;

document.addEventListener('DOMContentLoaded', function () {
    document.addEventListener('click', function (event) {
        if (event.target.matches('.add-widget')) {
            var type = event.target.dataset.widgetType;
            var targetId = event.target.dataset.targetId;
            var htmlFieldPrefix = event.target.dataset.htmlFieldPrefix;
            var createEditorUrl = document.getElementById(targetId).dataset.buildeditorurl;
            var prefixesName = event.target.dataset.prefixesName;
            var flowmetadata = event.target.dataset.flowmetadata;
            var parentContentType = event.target.dataset.parentContentType;
            var partName = event.target.dataset.partName;

            var indexes = Array.from(document.querySelectorAll(`#${targetId}`).closest("form").querySelectorAll("input[name*='Prefixes']"))
                .filter(e => e.value.substring(0, e.value.lastIndexOf('-')) === htmlFieldPrefix)
                .map(e => parseInt(e.value.substring(e.value.lastIndexOf('-') + 1)) || 0);

            var index = indexes.length ? Math.max(...indexes) + 1 : 0;
            var prefix = htmlFieldPrefix + '-' + index.toString();

            var contentTypesName = event.target.dataset.contenttypesName;
            var contentItemsName = event.target.dataset.contentitemsName;

            fetch(`${createEditorUrl}?id=${type}&prefix=${prefix}&prefixesName=${prefixesName}&contentTypesName=${contentTypesName}&contentItemsName=${contentItemsName}&targetId=${targetId}&flowmetadata=${flowmetadata}&parentContentType=${parentContentType}&partName=${partName}`)
                .then(response => response.json())
                .then(data => {
                    document.getElementById(targetId).insertAdjacentHTML('beforeend', data.Content);

                    var dom = new DOMParser().parseFromString(data.Scripts, 'text/html');
                    Array.from(dom.querySelectorAll('script')).forEach(script => {
                        eval(script.textContent || script.innerHTML);
                    });
                });
        }

        if (event.target.matches('.insert-widget')) {
            var type = event.target.dataset.widgetType;
            var target = event.target.closest('.widget-template');
            var targetId = event.target.dataset.targetId;
            var htmlFieldPrefix = event.target.dataset.htmlFieldPrefix;
            var createEditorUrl = document.getElementById(targetId).dataset.buildeditorurl;
            var flowmetadata = event.target.dataset.flowmetadata;
            var prefixesName = event.target.dataset.prefixesName;
            var parentContentType = event.target.dataset.parentContentType;
            var partName = event.target.dataset.partName;

            var indexes = Array.from(document.querySelectorAll(`#${targetId}`).closest("form").querySelectorAll("input[name*='Prefixes']"))
                .filter(e => e.value.substring(0, e.value.lastIndexOf('-')) === htmlFieldPrefix)
                .map(e => parseInt(e.value.substring(e.value.lastIndexOf('-') + 1)) || 0);

            var index = indexes.length ? Math.max(...indexes) + 1 : 0;
            var prefix = htmlFieldPrefix + '-' + index.toString();

            var contentTypesName = event.target.dataset.contenttypesName;
            var contentItemsName = event.target.dataset.contentitemsName;

            fetch(`${createEditorUrl}?id=${type}&prefix=${prefix}&prefixesName=${prefixesName}&contentTypesName=${contentTypesName}&contentItemsName=${contentItemsName}&targetId=${targetId}&flowmetadata=${flowmetadata}&parentContentType=${parentContentType}&partName=${partName}`)
                .then(response => response.json())
                .then(data => {
                    target.insertAdjacentHTML('beforebegin', data.Content);

                    var dom = new DOMParser().parseFromString(data.Scripts, 'text/html');
                    Array.from(dom.querySelectorAll('script')).forEach(script => {
                        eval(script.textContent || script.innerHTML);
                    });
                });
        }

        if (event.target.matches('.widget-delete')) {
            var $this = event.target;
            confirmDialog({
                ...$this.dataset, callback: function (r) {
                    if (r) {
                        $this.closest('.widget-template').remove();
                        document.dispatchEvent(new Event('contentpreview:render'));
                    }
                }
            });
        }

        if (event.target.matches('.widget-editor-btn-toggle')) {
            event.target.closest('.widget-editor').classList.toggle('collapsed');
        }
    });

    document.addEventListener('change', function (event) {
        if (event.target.matches('.widget-editor-footer label, .widget-editor-header label')) {
            var $tmpl = event.target.closest('.widget-template');
            var $radio = event.target.querySelector("input:first-child");
            if ($radio && $radio.id.includes('Size')) {
                var $radioSize = $radio.value;
                var classList = $tmpl.className.split(' ');
                classList.forEach(item => {
                    if (item.startsWith('col-md-')) $tmpl.classList.remove(item);
                });
                var colSize = Math.round($radioSize / 100 * 12);
                $tmpl.classList.add('col-md-' + colSize);

                var dropdown = event.target.closest('.dropdown-menu');
                dropdown.previousElementSibling.textContent = $radioSize + '%';
            } else if ($radio && $radio.id.includes('Alignment')) {
                var svg = event.target.querySelector('svg').outerHTML;
                var alignDropdown = event.target.closest('.dropdown-menu');
                var $btn = alignDropdown.previousElementSibling;
                $btn.innerHTML = svg;
            }

            Array.from(event.target.parentNode.querySelectorAll('.dropdown-item')).forEach(item => item.classList.remove('active'));
            event.target.classList.toggle('active');
            document.dispatchEvent(new Event('contentpreview:render'));
        }
    });

    document.addEventListener('keyup', function (event) {
        if (event.target.matches('.widget-editor-body .form-group input.content-caption-text')) {
            var headerTextLabel = event.target.closest('.widget-editor').querySelector('.widget-editor-header:first-child .widget-editor-header-text');
            var contentTypeDisplayText = headerTextLabel.dataset.contentTypeDisplayText;
            var title = event.target.value;
            var newDisplayText = title + ' ' + contentTypeDisplayText;

            headerTextLabel.textContent = newDisplayText;
        }
    });
});
