// Behaviors of the server-rendered editors of data pipeline steps. They work through event delegation, so they apply
// to editors the designer injects at any time.
//
// - A list of rows: <div data-dp-list data-dp-prefix="Fields"> holds a <template data-dp-template> and a
//   <div data-dp-rows> container of <div data-dp-row> elements. Inputs are named like "Fields[0].Name"; the template
//   uses "__index__" for the index. [data-dp-add] adds a row, [data-dp-remove] removes its row, [data-dp-up] and
//   [data-dp-down] move it. Rows are renumbered after each change, and a bubbling change event lets the designer save.
// - Inserting a field reference: [data-dp-insert="Field name"] inserts "[Field name]" at the caret of the formula whose
//   id is in data-dp-target. Without a target, it goes to the [data-dp-formula] element of the closest
//   [data-dp-formula-scope] that last had the focus, or to its first one.
// - Inserting a function: [data-dp-insert-function="NAME"] inserts "NAME()" the same way, with the caret between the
//   parentheses. A [data-dp-function-search] input filters the [data-dp-search-text] items of its [data-dp-functions].
(function () {
    const lastFormulas = new WeakMap();

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

    const findFormula = (button) => {
        if (button.dataset.dpTarget) {
            return document.getElementById(button.dataset.dpTarget);
        }

        const scope = button.closest("[data-dp-formula-scope]");

        if (!scope) {
            return null;
        }

        const last = lastFormulas.get(scope);

        return last && scope.contains(last) ? last : scope.querySelector("[data-dp-formula]");
    };

    // Inserts text at the caret, replacing the selection, and leaves the caret after "before" and before "after".
    const insertText = (target, before, after) => {
        const start = target.selectionStart ?? target.value.length;
        const end = target.selectionEnd ?? target.value.length;

        target.value = target.value.slice(0, start) + before + after + target.value.slice(end);
        target.focus();
        target.setSelectionRange(start + before.length, start + before.length);
        target.dispatchEvent(new Event("input", { bubbles: true }));
        notifyChange(target);
    };

    document.addEventListener("focusin", (event) => {
        const formula = event.target.closest?.("[data-dp-formula]");
        const scope = formula?.closest("[data-dp-formula-scope]");

        if (scope) {
            lastFormulas.set(scope, formula);
        }
    });

    document.addEventListener("input", (event) => {
        const search = event.target.closest?.("[data-dp-function-search]");

        if (!search) {
            return;
        }

        const container = search.closest("[data-dp-functions]");
        const terms = search.value.trim().toLowerCase().split(/\s+/).filter(Boolean);
        let matches = 0;

        container.querySelectorAll("[data-dp-function-category]").forEach((category) => {
            let visible = 0;

            category.querySelectorAll("[data-dp-search-text]").forEach((item) => {
                const text = item.dataset.dpSearchText;
                const match = terms.every((term) => text.includes(term));

                item.hidden = !match;
                visible += match ? 1 : 0;
            });

            category.hidden = visible === 0;
            matches += visible;
        });

        const empty = container.querySelector("[data-dp-function-empty]");

        if (empty) {
            empty.hidden = matches > 0;
        }
    });

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

        const insert = event.target.closest("[data-dp-insert], [data-dp-insert-function]");

        if (insert) {
            event.preventDefault();

            const target = findFormula(insert);

            if (!target) {
                return;
            }

            if (insert.hasAttribute("data-dp-insert-function")) {
                insertText(target, insert.dataset.dpInsertFunction + "(", ")");
            } else {
                insertText(target, "[" + insert.dataset.dpInsert + "]", "");
            }
        }
    });
})();
