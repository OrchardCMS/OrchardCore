document.addEventListener("DOMContentLoaded", () => {
    const databaseProvider = document.getElementById("DatabaseProvider");

    toggleConnectionStringAndPrefix();

    if (databaseProvider) {
        databaseProvider.addEventListener("change", toggleConnectionStringAndPrefix);
    }
});

function toggleConnectionStringAndPrefix() {
    const databaseProvider = document.getElementById("DatabaseProvider");
    const tablePrefix = document.getElementById("TablePrefix");
    const connectionString = document.getElementById("ConnectionString");
    const connectionStringHint = document.getElementById("connectionStringHint");

    if (!tablePrefix) {
        return;
    }

    const selectedOption = databaseProvider?.selectedOptions?.[0] ?? null;
    const hasProviderSelector = selectedOption !== null;
    const requireTablePrefix = tablePrefix.dataset.requireTablePrefix === "true";
    const hasTablePrefix = hasProviderSelector
        ? selectedOption.dataset.tablePrefix === "true"
        : tablePrefix.dataset.providerHasTablePrefix === "true";
    const hasConnectionString = hasProviderSelector
        ? selectedOption.dataset.connectionString === "true"
        : connectionString?.dataset.providerHasConnectionString === "true";

    toggleElements(".connectionString", hasConnectionString);
    toggleElements(".tablePrefixField", hasTablePrefix);
    toggleElements(".schemaField", hasTablePrefix);

    tablePrefix.required = requireTablePrefix && hasTablePrefix;
    if (connectionString) {
        // Connection string is always optional at tenant create/edit time.
        // It becomes required during setup if not provided at create.
        connectionString.required = false;
    }

    if (connectionStringHint) {
        connectionStringHint.textContent = hasProviderSelector
            ? (selectedOption?.dataset.connectionStringSample ?? "")
            : (connectionString?.dataset.connectionStringSample ?? "");
    }
}

function toggleElements(selector, isVisible) {
    document.querySelectorAll(selector).forEach((element) => {
        element.hidden = !isVisible;
    });
}
