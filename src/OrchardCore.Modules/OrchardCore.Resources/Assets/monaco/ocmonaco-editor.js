require.config({ paths: { 'vs': document.currentScript.dataset.tenantPrefix + '/OrchardCore.Resources/Scripts/monaco/vs' } });

const $document = $(document);

// initialize all monaco editors
$document.ready(function () {
    function debounce(func, wait, immediate) {
        var timeout;
        return function() {
            var context = this, args = arguments;
            var later = function() {
                timeout = null;
                if (!immediate) func.apply(context, args);
            };
            var callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(context, args);
        };
    };
    require(['vs/editor/editor.main'], function () {

        // set the theme from the html element
        const html = document.getElementsByTagName("html")[0];
        const mutationObserver = new MutationObserver(setTheme);
        mutationObserver.observe(html, { attributes: true });

        function setTheme() {
            let theme = html.dataset.theme;
            if (theme === "darkmode") {
                monaco.editor.setTheme('vs-dark')
            } else {
                monaco.editor.setTheme('vs')
            }
        }
        setTheme();

        function initMonacoEditor(el) {
            const $el = $(el);
            if($el.data("monaco") != undefined)
            {
                // this dom node has already been initialized, skip initialization
                return;
            }

            // sensible defaults for all monaco editors
            const defaultSettings = {
                wordWrap: "on",
                "scrollBeyondLastLine": false,
                "scrollbar": {
                    // allow the page to scroll when bottom of editor is  scrolling
                    "alwaysConsumeMouseWheel": false,
                }
            }

            // set diagnoticsOptions 
            if(el.dataset.diagnosticsOptions) {
                const json =atob(el.dataset.diagnosticsOptions);
                monaco.languages.json.jsonDefaults.setDiagnosticsOptions(JSON.parse(json));
            }

            let editorSettings = {};
            if(el.dataset.language)
            {
                editorSettings.language = el.dataset.language;
            }
            // settings override provided language
            if(el.dataset.settings) {
               
                editorSettings = JSON.parse(atob(el.dataset.settings));
            }
            const settings = Object.assign({}, defaultSettings, editorSettings);
            // automaticLayout no longer required as we manage the height / width automatically below
            settings.automaticLayout = false;

            const editor = monaco.editor.create(el, settings);
            const linkedElement = document.getElementById(el.dataset.linkedId);
            
            // set the initial value of the monaco editor based on the value of the textarea
            editor.getModel().setValue(linkedElement.value);

            window.addEventListener("submit", function () {
                linkedElement.value = editor.getValue();
            });

            // only trigger the preview if the element has the preview data-attribute
            if(el.dataset.preview)
            {
                // debounced change handler that will trigger the preview
                const debouncedOnChange = debounce(function(event) {
                    linkedElement.value = editor.getValue();
                    $document.trigger('contentpreview:render');
                }, 500);
                // trigger preview when content changes (debounced)
                editor.getModel().onDidChangeContent(debouncedOnChange);
            }

            // dynamically update the height of the monaco editor to a max of 650px 
            const updateHeight = () => {
                let width = el.parentElement.clientWidth;
                const contentHeight = Math.min(650, editor.getContentHeight());
                el.style.width = `${width}px`;
                el.style.height = `${contentHeight + 20}px`;
                editor.layout({ width: width - 20,  height: contentHeight });
            };
            editor.onDidContentSizeChange(updateHeight);
            updateHeight();

            // dynamically set the width when the browser is resized.
            const debouncedUpdateHeight = debounce(updateHeight,150);
            window.addEventListener('resize', debouncedUpdateHeight);
            
            // include the shortcodes action for html editors
            if(el.dataset.shortcodes)
            {
                const shortcodesAction = {
                    id: "shortcodes",
                    label: "Add Shortcode",
                    run: function (editor) {
                        shortcodesApp.init(function (value) {
                            if (value) {
                                var selection = editor.getSelection();
                                var text = value;
                                var op = { range: selection, text: text, forceMoveMarkers: true };
                                editor.executeEdits("shortcodes", [op]);
                            }
                            editor.focus();
                        })
                    },
                    contextMenuGroupId: 'orchardcore',
                    contextMenuOrder: 0,
                    keybindings: [
                        monaco.KeyMod.Alt | monaco.KeyCode.KEY_S, 
                    ]
                }
                editor.addAction(shortcodesAction);
            }

            // set the jQuery monaco data to the dom node. 
            // The data object is used to allow other components to use the editor functions or updateHeight function
            $el.data("monaco", { editor, updateHeight });
        }

        // Initially loop through and initialize all monaco editors on the page
        document.querySelectorAll("[data-monaco-oc]").forEach(initMonacoEditor);


        // initialize or update monaco editors. This is triggerred when widgets are dynamically added to the page,
        // when widgets are expanded or collapsed, when tabs are clicked or acoordion expanded.

        document.addEventListener('init-editors', function (e) {
            // wait until next tick, this is useful when showing / hiding tabs or widgets
            window.requestAnimationFrame(()=>{
                document.querySelectorAll("[data-monaco-oc]").forEach(function(el) {
                    let monacoData = $(el).data("monaco");
                    if(monacoData) {
                        // update the height if the editor has already been initialized
                        monacoData.updateHeight();
                    } else {
                         // Initialize new editors.
                        initMonacoEditor(el);
                    }
                });
            });
        }, false);
    });
});
