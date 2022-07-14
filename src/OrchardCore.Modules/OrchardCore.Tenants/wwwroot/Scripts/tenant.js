$(function () {
    toggleConnectionStringAndPrefix();

    // Show hide the connection string when a provider is selected
    $("#DatabaseProvider").change(function () {
        toggleConnectionStringAndPrefix();
    });
});

// Show or hide the connection string section and table prefix depending on the database provider
function toggleConnectionStringAndPrefix() {
    $("#DatabaseProvider option:selected").each(function () {
        $(this).data("connection-string") === true
            ? $(".connectionString").show()
            : $(".connectionString").hide();

        $(this).data("table-prefix") === true
            ? $(".tablePrefix").show()
            : $(".tablePrefix").hide();

        $("#connectionStringHint").text($(this).data("connection-string-sample"));
    });
}