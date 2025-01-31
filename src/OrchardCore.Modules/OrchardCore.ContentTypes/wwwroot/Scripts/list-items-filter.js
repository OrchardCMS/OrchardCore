document.addEventListener('DOMContentLoaded', function () {
    const searchBox = document.getElementById('search-box');
    const allRows = Array.from(document.querySelectorAll("#list-items > li[data-filter-value]"));
    const listAlert = document.getElementById('list-alert');
    const createButton = document.getElementById("btnCreate");

    let visibleRows = allRows;

    // On each keypress filter the list of types
    searchBox.addEventListener('keyup', function (e) {
        visibleRows = [];
        var search = e.target.value.toLowerCase().trim();

        // On ESC, clear the search box and display all types
        if (e.key === "Escape" || search == '') {
            searchBox.value = '';

            allRows.forEach(row => {
                row.classList.remove("d-none");
                row.classList.remove("first-child-visible");
                row.classList.remove("last-child-visible");

                visibleRows.push(row);
            });
        } else {
            allRows.forEach(row => {
                let text = row.getAttribute('data-filter-value').toLowerCase();
                let found = text.indexOf(search) > -1;

                if (found) {
                    row.classList.remove("d-none");
                    row.classList.remove("first-child-visible");
                    row.classList.remove("last-child-visible");
                    row.classList.remove("border-top");
                    row.classList.remove("rounded-top");

                    visibleRows.push(row);
                }
                else {
                    row.classList.add("d-none");
                }
            });

            if (visibleRows.length > 0) {
                visibleRows[0].classList.add('first-child-visible');
                visibleRows[0].classList.add('border-top');
                visibleRows[0].classList.add("rounded-top");

                visibleRows[visibleRows.length - 1].classList.add('last-child-visible');
            }
        }

        if (visibleRows.length == 0) {
            listAlert.classList.remove("d-none");
        }
        else {
            listAlert.classList.add("d-none");
        }

        // On Enter, redirect to the edit page if the type exists or the create page with a suggestion
        if (e.key === "Enter" && search != '') {

            if (visibleRows.length > 0) {
                let link = visibleRows[0].querySelector('a.edit-button');

                let hrefValue = link.getAttribute("href");

                if (hrefValue) {
                    location.href = hrefValue;
                }
            } else {
                location.href = createButton.getAttribute("href") + "?suggestion=" + search;
            }
            return;
        }
    });
});
