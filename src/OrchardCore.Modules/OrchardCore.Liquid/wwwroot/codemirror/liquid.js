CodeMirror.defineMode("liquid", function (config) {
    return CodeMirror.multiplexingMode(
        CodeMirror.getMode(config, "text/html"),
        {
            open: "{{",
            close: "}}",
            mode: CodeMirror.getMode(config, "text/x-liquid"),
            delimStyle: "liquid variable variable-2",
            innerStyle: "liquid variable variable-2"
        },
        {
            open: "{%",
            close: "%}",
            mode: CodeMirror.getMode(config, "text/x-liquid"),
            delimStyle: "liquid variable-2 special keyword",
            innerStyle: "liquid variable-2 special keyword"
        }
    );
});