const list = document.getElementById("Methods");

const getMouseOffset = (evt: DragEvent) => {
    const targetRect = (evt.target as HTMLElement).getBoundingClientRect();
    return {
        x: evt.pageX - targetRect.left,
        y: evt.pageY - targetRect.top,
    };
};

const getElementVerticalCenter = (el: HTMLElement) => {
    const rect = el.getBoundingClientRect();
    return (rect.bottom - rect.top) / 2;
};

// Pre-existing bug preserved as-is: sortable() is always called with no second argument, so
// onUpdate is always undefined here, and onDragEnd's unconditional call to it throws a
// TypeError on every drag-end (the notification never actually fires). Not fixed here since
// this extraction is a mechanical move, not a behavior change.
function sortable(rootEl: HTMLElement, onUpdate?: (dragEl: HTMLElement) => void) {
    let dragEl: HTMLElement | undefined;

    // Making all siblings movable
    Array.prototype.slice.call(rootEl.children).forEach((itemEl: HTMLElement) => {
        itemEl.draggable = true;
    });

    // Function responsible for sorting
    function onDragOver(evt: DragEvent) {
        evt.preventDefault();
        if (evt.dataTransfer) {
            evt.dataTransfer.dropEffect = "move";
        }

        const target = evt.target as HTMLElement;
        if (target && target !== dragEl && dragEl) {
            const offset = getMouseOffset(evt);
            const middleY = getElementVerticalCenter(target);

            if (offset.y > middleY) {
                rootEl.insertBefore(dragEl, target.nextSibling);
            } else {
                rootEl.insertBefore(dragEl, target);
            }
        }
    }

    // End of sorting
    function onDragEnd(evt: DragEvent) {
        evt.preventDefault();

        dragEl?.classList.remove("ghost");
        rootEl.removeEventListener("dragover", onDragOver, false);
        rootEl.removeEventListener("dragend", onDragEnd, false);

        // Notification about the end of sorting
        onUpdate!(dragEl!);
    }

    // Sorting starts
    rootEl.addEventListener(
        "dragstart",
        (evt: DragEvent) => {
            dragEl = evt.target as HTMLElement; // Remembering an element that will be moved

            if (evt.dataTransfer) {
                evt.dataTransfer.effectAllowed = "move";
                evt.dataTransfer.setData("Text", dragEl.textContent ?? "");
            }

            rootEl.addEventListener("dragover", onDragOver, false);
            rootEl.addEventListener("dragend", onDragEnd, false);
            setTimeout(() => {
                // If this action is performed without setTimeout, then
                // the moved object will be of this class.
                dragEl?.classList.add("ghost");
            }, 0);
        },
        false,
    );
}

if (list) {
    sortable(list);
}

export {};
