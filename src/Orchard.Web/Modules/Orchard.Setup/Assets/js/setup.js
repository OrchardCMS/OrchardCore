$(function () {
    toggleConnectionString();

    // Show hide the connection string when a provider is selected
    $("#DatabaseProvider").change(function () {
        toggleConnectionString();
    });

});

// Show or hide the connection string section when the database provider
function toggleConnectionString() {
    $("#DatabaseProvider option:selected").each(function () {
        $(this).data("connection-string").toLowerCase() === "true"
        ? $(".connectionString").show()
        : $(".connectionString").hide();
        ;
    });
}
