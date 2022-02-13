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

            const linkedTextarea = document.getElementById(el.dataset.textareaId);

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

            // if language is liquid and the ConfigureLiquidIntellisense function exists, call it.
            if(editorSettings.language == "liquid" && typeof ConfigureLiquidIntellisense === 'function') {
                ConfigureLiquidIntellisense(monaco);
            }

            // automaticLayout no longer required as we manage the height / width automatically below
            settings.automaticLayout = false;

            // if a schema is provided, we also expect a schemaName
            if(el.dataset.schema) {
                const schemaName = el.dataset.schemaName;
                const modelUri = monaco.Uri.parse(`x://${schemaName}.json`);
                const schema = JSON.parse(el.dataset.schema)
                settings.model = monaco.editor.createModel(linkedTextarea.value, settings.language, modelUri);
    
                monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
                    validate: true,
                    schemas: [{
                        uri: `x://${schemaName}.schema.json`,
                        fileMatch: [modelUri.toString()],
                        schema: schema
                    }]
                });
            }

            const editor = monaco.editor.create(el, settings);
            
            // set the initial value of the monaco editor based on the value of the textarea
            editor.getModel().setValue(linkedTextarea.value);

            window.addEventListener("submit", function () {
                linkedTextarea.value = editor.getValue();
            });

            // only trigger the preview if the element has the preview data-attribute
            if(el.dataset.preview)
            {
                // debounced change handler that will trigger the preview
                const debouncedOnChange = debounce(function(event) {
                    linkedTextarea.value = editor.getValue();
                    $document.trigger('contentpreview:render');
                }, 500);
                // trigger preview when content changes (debounced)
                editor.getModel().onDidChangeContent(debouncedOnChange);
            }
            

            let maxHeight = 1000;
            if(el.dataset.maxHeight) {
                //setting max-height to 0 means infinite
                maxHeight = el.dataset.maxHeight;
            }
            // dynamically update the height of the monaco editor to a max of 650px 
            const updateHeight = () => {
                let width = el.parentElement.clientWidth;
                const contentHeight = maxHeight == 0 ? editor.getContentHeight() : Math.min(maxHeight, editor.getContentHeight());
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
            if(el.dataset.shortcodes) {
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

            // template preview
            if(el.dataset.templatePreview) {
               
                var nameInput =  document.getElementById(el.dataset.nameId);
                const antiforgerytoken = $("[name='__RequestVerificationToken']").val();
        
                function sendFormData () {
                    const formData = {
                        'Name': nameInput.value,
                        'Content': editor.getValue(),
                        '__RequestVerificationToken': antiforgerytoken
                    };
                    // store the form data to pass it in the event handler
                    localStorage.setItem('OrchardCore.templates', JSON.stringify($.param(formData)));
                }
    

                // debounced change handler that will trigger the preview
                const debouncedOnChangeTemplate = debounce(function(event) {
                    sendFormData()
                }, 500);
                // trigger preview when content changes (debounced)
                editor.getModel().onDidChangeContent(debouncedOnChangeTemplate);
    
                window.addEventListener('storage', function (ev) {
                    if (ev.key != 'OrchardCore.templates:ready') return; // ignore other keys
                    // triggered by the preview window the first time it is loaded in order
                    // to pre-render the view even if no contentpreview:render is already sent
                    debouncedOnChangeTemplate();
                }, false);
    
                window.addEventListener('unload', function () {
                    localStorage.removeItem('OrchardCore.templates');
                    // this will raise an event in the preview window to notify that the live preview is no longer active.
                    localStorage.setItem('OrchardCore.templates:not-connected', '');
                    localStorage.removeItem('OrchardCore.templates:not-connected');
                });
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
