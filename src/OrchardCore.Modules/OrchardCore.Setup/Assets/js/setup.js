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

        $("#connectionStringHint").text($(this).data("connection-string-sample"));
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
    $("#recipeButton").focus();
}

function setLocalizationUrl() {
    var culturesList = document.getElementById('culturesList');
    window.location = culturesList.options[culturesList.selectedIndex].dataset.url;
}

function togglePasswordVisibility(passwordCtl, togglePasswordCtl)
{
    // toggle the type attribute
    type = passwordCtl.getAttribute('type') === 'password' ? 'text' : 'password';
    passwordCtl.setAttribute('type', type);

    // toggle the eye slash icon
    icon = togglePasswordCtl.getElementsByClassName('icon')[0];
    if(icon.getAttribute('data-icon')){ // if the icon is rendered as a svg
        type === 'password' ? icon.setAttribute('data-icon', 'eye') : icon.setAttribute('data-icon', 'eye-slash');
    }
    else{ // if the icon is still a <i> element
        type === 'password' ? icon.classList.remove('fa-eye-slash') : icon.classList.remove('fa-eye');
        type === 'password' ? icon.classList.add('fa-eye') : icon.classList.add('fa-eye-slash');
    }
}
