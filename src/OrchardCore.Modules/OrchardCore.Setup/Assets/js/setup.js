$(function () {
    toggleConnectionString();

    // Show hide the connection string when a provider is selected
    $("#DatabaseProvider").change(function () {
        toggleConnectionString();
    });

    // Refresh the description hide the connection string when a provider is selected
    $("#recipes div a").on('click', function () {
        refreshDescription($(this));
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

// Show the recipe description
function refreshDescription(target) {
    var recipeName = $(target).data("recipe-name");
    var recipeDisplayName = $(target).data("recipe-display-name");
    var recipeDescription = $(target).data("recipe-description");
    $("#recipeButton").text(recipeDisplayName);
    $("#RecipeName").val(recipeName);
    $("#recipeButton").attr("title", recipeDescription);
}