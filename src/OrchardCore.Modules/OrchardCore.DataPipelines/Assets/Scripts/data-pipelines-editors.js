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
// - Searchable lists: a <select data-dp-select> becomes a bootstrap-select, configured by its data-* attributes, such
//   as data-live-search and data-show-selected-tags. It's created when the designer injects the editor, and destroyed
//   when the designer removes it. It raises the select's "change" event, so the designer applies the selection.
// - Filter rows: a <div data-dp-filter-row> shows only the comparisons of its [data-dp-filter-operator] whose
//   data-dp-types has the data-dp-type of the field chosen in its [data-dp-filter-field]. It shows the inputs of the
//   values the comparison takes, typed for the field: a number, a date, a date and time, or a yes/no select. A value the
//   typed input can't hold, such as a placeholder, is written as text, which [data-dp-filter-text] switches. The
//   comparisons that take a list, "is one of" and "isn't one of", take values separated by commas, as text.
// - Choosing a data set: [data-dp-dataset-open] opens the Bootstrap dialog of the <template data-dp-dataset-dialog> of
//   its [data-dp-dataset-picker] over the page. Its [data-dp-dataset-filter] and [data-dp-dataset-category] buttons
//   filter its [data-dp-dataset-card] cards, and [data-dp-dataset-choose="value"] sets the [data-dp-dataset-input] of
//   the picker, raises its "change" event, so the designer applies it, and closes the dialog.
(function () {
    const lastFormulas = new WeakMap();
    const selectSelector = "select[data-dp-select]";
    const filterRowSelector = "[data-dp-filter-row]";

    const initSelects = (node) => {
        if (!window.Selectpicker) {
            return;
        }

        const selects = node.matches(selectSelector) ? [node] : node.querySelectorAll(selectSelector);

        selects.forEach((select) => window.Selectpicker.getOrCreateInstance(select));
    };

    // The patterns of the values each typed input holds, as the steps store them.
    const valuePatterns = {
        days: /^\d+$/,
        number: /^[-+]?(\d+(\.\d*)?|\.\d+)$/,
        date: /^\d{4}-\d{2}-\d{2}$/,
        "datetime-local": /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}(:\d{2}(\.\d{1,3})?)?$/,
        boolean: /^(true|false)$/i,
    };

    const fits = (kind, value) => !value || !valuePatterns[kind] || valuePatterns[kind].test(value.trim());

    const getFilterState = (row) => {
        const fieldType = row.querySelector("[data-dp-filter-field]")?.selectedOptions[0]?.dataset.dpType ?? "";
        const operator = row.querySelector("[data-dp-filter-operator]")?.value ?? "";
        const count = operator === "IsEmpty" || operator === "IsNotEmpty" ? 0 : operator === "Between" ? 2 : 1;
        let kind = "text";

        if (operator === "In" || operator === "NotIn") {
            kind = "list";
        } else if (operator === "InLastDays" || operator === "InNextDays") {
            kind = "days";
        } else if (fieldType === "Integer" || fieldType === "Decimal") {
            kind = "number";
        } else if (fieldType === "Date") {
            kind = "date";
        } else if (fieldType === "DateTime") {
            kind = "datetime-local";
        } else if (fieldType === "Boolean") {
            kind = "boolean";
        }

        return { fieldType, operator, count, kind };
    };

    // Shows the comparisons that apply to the field. A field that changes keeps its comparison when it applies, and
    // takes the first one that applies otherwise. When the editor is loaded, a saved comparison that doesn't apply stays,
    // as the step reports it as an error.
    const updateOperators = (row, fieldType, initial) => {
        const select = row.querySelector("[data-dp-filter-operator]");

        Array.from(select.options).forEach((option) => {
            const applies = !fieldType || (option.dataset.dpTypes ?? "").split(" ").includes(fieldType);
            const visible = applies || (initial && option.selected);

            option.hidden = !visible;
            option.disabled = !visible;
        });

        if (select.selectedOptions[0]?.disabled) {
            const first = Array.from(select.options).find((option) => !option.disabled);

            if (first) {
                select.value = first.value;
            }
        }
    };

    const updateFilterRow = (row, initial) => {
        updateOperators(row, getFilterState(row).fieldType, initial);

        const { fieldType, count, kind } = getFilterState(row);
        const value = row.querySelector("[data-dp-filter-value]");
        const booleanValue = row.querySelector("[data-dp-filter-boolean]");
        const second = row.querySelector("[data-dp-filter-second]");
        const current = booleanValue.disabled ? value.value : booleanValue.value;
        const typed = kind !== "text" && kind !== "list";

        // A value the typed input can't hold, such as {Parameter:Since}, stays as text.
        if (typed && count > 0 && (!fits(kind, current) || (count === 2 && !fits(kind, second.value)))) {
            row.dataset.dpTextMode = "true";
        }

        const textMode = typed && row.dataset.dpTextMode === "true";
        const useBoolean = kind === "boolean" && !textMode && count > 0;

        if (useBoolean) {
            booleanValue.value = current.trim().toLowerCase();
        } else {
            value.value = current;
        }

        booleanValue.hidden = !useBoolean;
        booleanValue.disabled = !useBoolean;
        value.hidden = count === 0 || useBoolean;
        value.disabled = useBoolean;
        second.hidden = count !== 2;

        const type = !typed || textMode || kind === "boolean" ? "text" : kind === "days" ? "number" : kind;

        value.placeholder = (kind === "list" ? value.dataset.dpListPlaceholder : value.dataset.dpValuePlaceholder) ?? value.placeholder;

        [value, second].forEach((input) => {
            input.type = type;

            if (type === "number") {
                input.step = kind === "days" || fieldType === "Integer" ? "1" : "any";
            } else {
                input.removeAttribute("step");
            }

            if (type === "number" && kind === "days") {
                input.min = "1";
            } else {
                input.removeAttribute("min");
            }
        });

        row.querySelector("[data-dp-filter-and]").hidden = count !== 2;
        row.querySelector("[data-dp-filter-days]").hidden = kind !== "days" || count === 0;

        const toggle = row.querySelector("[data-dp-filter-text]");

        toggle.hidden = count === 0 || !typed;
        toggle.classList.toggle("active", textMode);
        toggle.setAttribute("aria-pressed", String(textMode));
    };

    // Switches the values of a row between text and their typed inputs. A value the typed input can't hold is cleared.
    const toggleTextMode = (row) => {
        const { kind } = getFilterState(row);
        const textMode = row.dataset.dpTextMode !== "true";

        row.dataset.dpTextMode = String(textMode);

        if (!textMode) {
            row.querySelectorAll("[data-dp-filter-value], [data-dp-filter-second]").forEach((input) => {
                if (!fits(kind, input.value)) {
                    input.value = "";
                }
            });
        }

        updateFilterRow(row, false);
    };

    // The dialog that chooses a data set, by its picker.
    const dataSetDialogs = new WeakMap();

    // Shows the cards of the chosen source that have every term of the filter, and counts the cards of each source
    // that have them.
    const filterDataSets = (dialog) => {
        const terms = (dialog.querySelector("[data-dp-dataset-filter]")?.value ?? "").trim().toLowerCase().split(/\s+/).filter(Boolean);
        const category = dialog.querySelector("[data-dp-dataset-category].active")?.dataset.dpDatasetCategory ?? "";
        const counts = new Map();
        let visible = 0;

        dialog.querySelectorAll("[data-dp-dataset-card]").forEach((card) => {
            const text = card.dataset.dpSearchText ?? "";
            const match = terms.every((term) => text.includes(term));
            const source = card.dataset.dpCategory ?? "";

            if (match) {
                counts.set("", (counts.get("") ?? 0) + 1);
                counts.set(source, (counts.get(source) ?? 0) + 1);
            }

            card.hidden = !match || (category !== "" && source !== category);
            visible += card.hidden ? 0 : 1;
        });

        dialog.querySelectorAll("[data-dp-dataset-category]").forEach((button) => {
            const count = button.querySelector("[data-dp-dataset-count]");
            const active = button.classList.contains("active");

            if (count) {
                count.textContent = String(counts.get(button.dataset.dpDatasetCategory) ?? 0);
                count.classList.toggle("text-bg-light", active);
                count.classList.toggle("text-bg-secondary", !active);
            }
        });

        const empty = dialog.querySelector("[data-dp-dataset-empty]");

        if (empty) {
            empty.hidden = visible > 0;
        }
    };

    // Opens the dialog over the page, outside the editor, so that its events don't change the form.
    const openDataSetDialog = (picker) => {
        const template = picker.querySelector(":scope > template[data-dp-dataset-dialog]");
        const dialog = template?.content.firstElementChild?.cloneNode(true);

        if (!dialog) {
            return;
        }

        dataSetDialogs.set(dialog, picker);
        document.body.appendChild(dialog);
        filterDataSets(dialog);

        const modal = window.bootstrap?.Modal.getOrCreateInstance(dialog);

        dialog.addEventListener("shown.bs.modal", () => dialog.querySelector("[data-dp-dataset-filter]")?.focus());
        dialog.addEventListener("hide.bs.modal", () => {
            if (dialog.contains(document.activeElement)) {
                document.activeElement.blur();
            }
        });
        dialog.addEventListener("hidden.bs.modal", () => {
            modal?.dispose();
            dialog.remove();
        });
        modal?.show();
    };

    // Sets the data set, which the designer applies, and closes the dialog.
    const chooseDataSet = (dialog, value) => {
        const picker = dataSetDialogs.get(dialog);
        const input = picker?.querySelector("[data-dp-dataset-input]");

        if (input && picker.isConnected) {
            input.value = value;
            input.dispatchEvent(new Event("change", { bubbles: true }));
        }

        const modal = window.bootstrap?.Modal.getInstance(dialog);

        if (modal) {
            modal.hide();
        } else {
            dialog.remove();
        }
    };

    document.addEventListener("input", (event) => {
        const filter = event.target.closest?.("[data-dp-dataset-filter]");

        if (filter) {
            filterDataSets(filter.closest(".modal"));
        }
    });

    const initFilterRows = (node) => {
        const rows = node.matches(filterRowSelector) ? [node] : node.querySelectorAll(filterRowSelector);

        rows.forEach((row) => updateFilterRow(row, true));
    };

    const initEditors = (node) => {
        if (!(node instanceof Element)) {
            return;
        }

        initSelects(node);
        initFilterRows(node);
    };

    new MutationObserver((mutations) => {
        mutations.forEach((mutation) => mutation.addedNodes.forEach(initEditors));
    }).observe(document.body ?? document.documentElement, { childList: true, subtree: true });

    initEditors(document.body ?? document.documentElement);

    // The designer applies the form a moment after a change, so the row is up to date when it reads the form.
    document.addEventListener("change", (event) => {
        const row = event.target.closest?.("[data-dp-filter-field], [data-dp-filter-operator]")?.closest(filterRowSelector);

        if (row) {
            updateFilterRow(row, false);
        }
    });

    document.addEventListener("oc:editor-unmounting", (event) => {
        if (!window.Selectpicker || !(event.target instanceof Element)) {
            return;
        }

        event.target.querySelectorAll(selectSelector).forEach((select) => window.Selectpicker.getInstance(select)?.destroy());
    });

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

    // Inserts text at the caret, replacing the selection, and leaves the caret after "before" and before "after". Only
    // an "input" event is raised: the formula is usually incomplete, such as "MONTH()", and a "change" would apply it
    // at once, and redraw the editor with an error while it is being written. It's applied when it is committed.
    const insertText = (target, before, after) => {
        const start = target.selectionStart ?? target.value.length;
        const end = target.selectionEnd ?? target.value.length;

        target.value = target.value.slice(0, start) + before + after + target.value.slice(end);
        target.focus();
        target.setSelectionRange(start + before.length, start + before.length);
        target.dispatchEvent(new Event("input", { bubbles: true }));
    };

    document.addEventListener("focusin", (event) => {
        const formula = event.target.closest?.("[data-dp-formula]");
        const scope = formula?.closest("[data-dp-formula-scope]");

        if (scope) {
            lastFormulas.set(scope, formula);
        }
    });

    const filterFunctions = (search) => {
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
    };

    // The search of the functions isn't a setting: its events stop here, in the capture phase, so that the editor
    // doesn't apply the form, and redraw it, when the search loses the focus.
    ["input", "change"].forEach((type) => {
        document.addEventListener(type, (event) => {
            const search = event.target.closest?.("[data-dp-function-search]");

            if (!search) {
                return;
            }

            event.stopPropagation();

            if (type === "input") {
                filterFunctions(search);
            }
        }, true);
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

        const openDataSet = event.target.closest("[data-dp-dataset-open]");

        if (openDataSet) {
            event.preventDefault();
            openDataSetDialog(openDataSet.closest("[data-dp-dataset-picker]"));

            return;
        }

        const category = event.target.closest("[data-dp-dataset-category]");

        if (category) {
            const dialog = category.closest(".modal");

            dialog.querySelectorAll("[data-dp-dataset-category]").forEach((button) => {
                button.classList.toggle("active", button === category);
                button.setAttribute("aria-pressed", String(button === category));
            });
            filterDataSets(dialog);

            return;
        }

        const choose = event.target.closest("[data-dp-dataset-choose]");

        if (choose) {
            chooseDataSet(choose.closest(".modal"), choose.dataset.dpDatasetChoose);

            return;
        }

        const textToggle = event.target.closest("[data-dp-filter-text]");

        if (textToggle) {
            event.preventDefault();

            const row = textToggle.closest(filterRowSelector);

            toggleTextMode(row);
            row.querySelector("[data-dp-filter-value]:not([disabled]), [data-dp-filter-boolean]:not([disabled])")?.focus();
            notifyChange(row);

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
