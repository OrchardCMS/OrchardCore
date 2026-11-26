import { onEditorUnmounting } from "../helpers/editorLifecycle";
import { getSelectpicker, notifyContentPreview, SelectpickerInstance, SelectpickerOptionData, SelectpickerSourceCallback } from "./select-picker";

// A picker of items searched on the server, such as the content items of a content picker field or the users of a
// user picker field. The selected items are listed above a bootstrap-select whose options are loaded from the search
// URL, so they can be reordered by dragging them (when several items can be picked), opened when they link to an
// editor, and removed. The ids of the selected items are written, separated by commas, in a hidden input.
//
// The markup is a container with the settings in data-* attributes, the hidden input, and the <select> of the
// search, configured with the data-* attributes of bootstrap-select (placeholder, data-none-results-text, ...):
//
// <div data-remote-picker data-search-url="..." data-multiple="true" data-selected-items="[...]"
//      data-values-input="ContentItemIds" data-status-field="hasPublished" data-status-text="Not published"
//      data-edit-url="/Admin/Contents/ContentItems/contentItemId/Edit" data-remove-text="Remove">
//     <input type="hidden" id="ContentItemIds" name="ContentItemIds" value="..." />
//     <select data-remote-picker-search placeholder="Type to search" data-none-results-text="..."></select>
// </div>

// An item returned by the search URL.
export interface RemotePickerItem {
    id: string;
    displayText: string;
    // When true, the item links to the edit URL.
    isClickable?: boolean;
    [key: string]: unknown;
}

const SEARCH_DELAY = 250;

const parseItems = (json: string | undefined): RemotePickerItem[] => {
    if (!json) {
        return [];
    }

    try {
        const items = JSON.parse(json);

        return Array.isArray(items) ? items : [];
    } catch {
        return [];
    }
};

export const initRemotePicker = (container: HTMLElement): void => {
    const Selectpicker = getSelectpicker();
    const select = container.querySelector<HTMLSelectElement>("select[data-remote-picker-search]");

    if (!Selectpicker || !select) {
        return;
    }

    const searchUrl = container.dataset.searchUrl ?? "";
    const multiple = container.dataset.multiple === "true";
    const statusField = container.dataset.statusField;
    const statusText = container.dataset.statusText ?? "";
    const editUrl = container.dataset.editUrl;
    const removeText = container.dataset.removeText ?? "Remove";
    const valuesInputId = container.dataset.valuesInput;
    const valuesInput = valuesInputId ? document.getElementById(valuesInputId) : null;

    const items = parseItems(container.dataset.selectedItems);

    // The items returned by the searches, by id, to find the item of a selected option.
    const knownItems = new Map<string, RemotePickerItem>();

    const list = document.createElement("ul");
    list.className = "list-group mb-1 remote-picker-list";
    container.insertBefore(list, container.firstChild);

    let picker: SelectpickerInstance | null = null;
    let searchTimeout: ReturnType<typeof setTimeout> | undefined;
    let searchVersion = 0;

    const getStatus = (item: RemotePickerItem) => (statusField && !item[statusField] ? statusText : "");

    const writeValues = (notify = true) => {
        if (valuesInput instanceof HTMLInputElement) {
            valuesInput.value = items.map((item) => item.id).join(",");
        }

        if (notify) {
            notifyContentPreview();
        }
    };

    const renderList = () => {
        list.replaceChildren(
            ...items.map((item) => {
                const listItem = document.createElement("li");
                listItem.className = "list-group-item d-flex align-items-center justify-content-between gap-2 remote-picker-item";
                listItem.dataset.id = item.id;

                if (multiple) {
                    listItem.classList.add("cursor-move");
                }

                const label = document.createElement("div");
                label.className = "remote-picker-item-label";

                if (multiple) {
                    // The items are dragged by the whole row, the handle shows that they can be.
                    const handle = document.createElement("i");
                    handle.className = "fa-solid fa-grip-vertical text-muted me-2 remote-picker-item-handle";
                    handle.setAttribute("aria-hidden", "true");
                    label.appendChild(handle);
                }

                const text = document.createElement(item.isClickable && editUrl ? "a" : "span");
                text.textContent = item.displayText;

                if (text instanceof HTMLAnchorElement && editUrl) {
                    text.href = editUrl.replace("contentItemId", encodeURIComponent(item.id));
                    text.target = "_blank";
                }

                label.appendChild(text);

                const status = getStatus(item);

                if (status) {
                    const statusElement = document.createElement("span");
                    statusElement.className = "text-muted small ms-1";
                    statusElement.textContent = `(${status})`;
                    label.appendChild(statusElement);
                }

                const removeButton = document.createElement("button");
                removeButton.type = "button";
                removeButton.className = "btn btn-secondary btn-sm remote-picker-item-remove";
                removeButton.title = removeText;
                removeButton.setAttribute("aria-label", `${removeText} ${item.displayText}`);
                removeButton.innerHTML = '<i class="fa-solid fa-trash fa-sm" aria-hidden="true"></i>';
                removeButton.addEventListener("click", () => {
                    items.splice(items.indexOf(item), 1);
                    renderList();
                    writeValues();
                    // Shows the removed item in the options again.
                    resetPicker(true);
                });

                listItem.append(label, removeButton);

                return listItem;
            }),
        );

        list.hidden = items.length === 0;
    };

    // Loads the items matching the query, without the selected ones, as options.
    const fetchOptions = async (query: string): Promise<SelectpickerOptionData[]> => {
        const url = new URL(searchUrl, window.location.href);

        if (query) {
            url.searchParams.set("query", query);
        }

        try {
            const response = await fetch(url, { headers: { Accept: "application/json" } });

            if (!response.ok) {
                return [];
            }

            const results: RemotePickerItem[] = await response.json();

            return results
                .filter((result) => {
                    knownItems.set(result.id, result);

                    return !items.some((item) => item.id === result.id);
                })
                .map((result) => ({ value: result.id, text: result.displayText, subtext: getStatus(result) || undefined }));
        } catch {
            return [];
        }
    };

    const loadOptions = (callback: SelectpickerSourceCallback) => {
        fetchOptions("").then((options) => callback(options, false));
    };

    // Searches once the user stops typing, and drops the results of the searches that were replaced by a new one.
    const searchOptions = (callback: SelectpickerSourceCallback, _page: number, searchValue: string) => {
        const version = ++searchVersion;

        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            fetchOptions(searchValue).then((options) => {
                if (version === searchVersion) {
                    callback(options, false);
                }
            });
        }, SEARCH_DELAY);
    };

    const createPicker = () => {
        // Removes the options of the previous search.
        select.replaceChildren();

        picker = Selectpicker.getOrCreateInstance(select, {
            liveSearch: true,
            width: "100%",
            source: { data: loadOptions, search: searchOptions },
        });

        // A single item is replaced by removing it first.
        if (!multiple && items.length > 0) {
            picker?.hide();
        } else {
            picker?.show();
        }
    };

    // Starts over with the options of an empty search, without the selected items.
    const resetPicker = (focus: boolean) => {
        picker?.destroy();
        createPicker();

        if (focus && picker && picker.newElement.style.display !== "none") {
            picker.button.focus();
        }
    };

    select.addEventListener("change", () => {
        const item = knownItems.get(select.value);

        if (!item || items.some((selected) => selected.id === item.id)) {
            return;
        }

        items.push(item);
        renderList();
        writeValues();

        // Lets bootstrap-select finish handling the click before it's recreated.
        setTimeout(() => resetPicker(multiple), 0);
    });

    if (multiple && typeof Sortable !== "undefined") {
        Sortable.create(list, {
            animation: 150,
            filter: ".remote-picker-item-remove",
            preventOnFilter: false,
            onEnd: () => {
                const order = Array.from(list.children).map((element) => (element as HTMLElement).dataset.id);

                items.sort((a, b) => order.indexOf(a.id) - order.indexOf(b.id));
                writeValues();
            },
        });
    }

    renderList();
    writeValues(false);
    createPicker();

    onEditorUnmounting(container, () => {
        clearTimeout(searchTimeout);
        picker?.destroy();
    });
};

export default initRemotePicker;
