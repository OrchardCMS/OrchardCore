var TechnicalNameGenerator = (function () {

    function toLowercaseHyphenated(value) {
        return (value || '')
            .trim()
            .split(/[^A-Za-z0-9]+/)
            .filter(function (w) { return w.length > 0; })
            .map(function (w) { return w.toLowerCase(); })
            .join('-');
    }

    function initialize(displayId, nameId) {
        var displayEl = document.getElementById(displayId);
        var nameEl = document.getElementById(nameId);
        if (!displayEl || !nameEl) return;

        var userEdited = nameEl.value.trim() !== '';

        nameEl.addEventListener('input', function () {
            userEdited = nameEl.value.trim() !== '';
        });

        displayEl.addEventListener('input', function () {
            if (userEdited) return;
            nameEl.value = toLowercaseHyphenated(displayEl.value);
        });
    }

    return {
        initialize: initialize
    };
})();
