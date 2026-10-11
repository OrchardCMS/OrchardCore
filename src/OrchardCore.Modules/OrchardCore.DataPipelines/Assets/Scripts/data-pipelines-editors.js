// Behaviors of the server-rendered editors of data pipeline steps. They work through event delegation, so they apply
// to editors the designer injects at any time.
//
// - A list of rows: <div data-dp-list data-dp-prefix="Fields"> holds a <template data-dp-template> and a
//   <div data-dp-rows> container of <div data-dp-row> elements. Inputs are named like "Fields[0].Name"; the template
//   uses "__index__" for the index. [data-dp-add] adds a row, [data-dp-remove] removes its row, [data-dp-up] and
//   [data-dp-down] move it. Rows are renumbered after each change, and a bubbling change event lets the designer save.
// - Inserting a field reference: [data-dp-insert="Field name"] inserts "[Field name]" into the element whose id is in
//   data-dp-target, at the caret.
(function () {
    const escapeRegExp = (value) => value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");

    const renumber = (list) => {
        const prefix = list.dataset.dpPrefix;
        const pattern = new RegExp("^" + escapeRegExp(prefix) + "\\[(\\d+|__index__)\\]");
        const rows = list.querySelectorAll(":scope > [data-dp-rows] > [data-dp-row]");

        rows.forEach((row, index) => {
            row.querySelectorAll("[name]").forEach((element) => {
                element.name = element.name.replace(pattern, prefix + "[" + index + "]");
            });

            row.querySelectorAll("[id]").forEach((element) => {
                element.id = element.id.replace(/__index__|_\d+__/, (match) => (match === "__index__" ? String(index) : "_" + index + "__"));
            });
        });

        const empty = list.querySelector(":scope > [data-dp-empty]");

        if (empty) {
            empty.hidden = rows.length > 0;
        }
    };

    const notifyChange = (element) => {
        const target = element.closest("form") ?? element;
        target.dispatchEvent(new Event("change", { bubbles: true }));
    };

    document.addEventListener("click", (event) => {
        const add = event.target.closest("[data-dp-add]");

        if (add) {
            event.preventDefault();

            const list = add.closest("[data-dp-list]");
            const template = list.querySelector(":scope > template[data-dp-template]");
            const rows = list.querySelector(":scope > [data-dp-rows]");
            const fragment = template.content.cloneNode(true);

            rows.appendChild(fragment);
            renumber(list);

            const added = rows.lastElementChild;
            added?.querySelector("input, select, textarea")?.focus();
            notifyChange(list);

            return;
        }

        const action = event.target.closest("[data-dp-remove], [data-dp-up], [data-dp-down]");

        if (action) {
            event.preventDefault();

            const row = action.closest("[data-dp-row]");
            const list = row.closest("[data-dp-list]");

            if (action.hasAttribute("data-dp-remove")) {
                row.remove();
            } else if (action.hasAttribute("data-dp-up") && row.previousElementSibling) {
                row.parentElement.insertBefore(row, row.previousElementSibling);
            } else if (action.hasAttribute("data-dp-down") && row.nextElementSibling) {
                row.parentElement.insertBefore(row.nextElementSibling, row);
            }

            renumber(list);
            notifyChange(list);

            return;
        }

        const insert = event.target.closest("[data-dp-insert]");

        if (insert) {
            event.preventDefault();

            const target = document.getElementById(insert.dataset.dpTarget);

            if (!target) {
                return;
            }

            const text = "[" + insert.dataset.dpInsert + "]";
            const start = target.selectionStart ?? target.value.length;
            const end = target.selectionEnd ?? target.value.length;

            target.value = target.value.slice(0, start) + text + target.value.slice(end);
            target.focus();
            target.setSelectionRange(start + text.length, start + text.length);
            target.dispatchEvent(new Event("input", { bubbles: true }));
            notifyChange(target);
        }
    });
})();
