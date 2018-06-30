$(function () {
    toggleConnectionStringAndPrefix();

    // Show hide the connection string when a provider is selected
    $("#DatabaseProvider").change(function () {
        toggleConnectionStringAndPrefix();
    });

    // Refresh the recipe description
    $("#recipes div a").on('click', function () {
        refreshDescription($(this));
    });

});

// Show or hide the connection string or table prefix section when the database provider is selected
function toggleConnectionStringAndPrefix() {
    $("#DatabaseProvider option:selected").each(function () {
        $(this).data("connection-string").toLowerCase() === "true"
            ? $(".connectionString").show()
            : $(".connectionString").hide();

        $(this).data("table-prefix").toLowerCase() === "true"
            ? $(".tablePrefix").show()
            : $(".tablePrefix").hide();
        
        $(this).data("connection-string").toLowerCase() === "true"
            ? $(".pwd").attr('required', 'required')
            : $(".pwd").removeAttr('required');
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