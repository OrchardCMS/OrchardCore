interface SortableEndEvent {
    item: HTMLElement;
}

const undoForm = document.getElementById("undo-form");
const updatePositionUrl = undoForm?.dataset.updatePositionUrl;

document.querySelectorAll<HTMLElement>(".list-group.zones").forEach((list) => {
    Sortable.create(list, {
        handle: ".properties",
        group: "layers-widgets",
        animation: 150,
        onEnd: (evt: SortableEndEvent) => {
            if (!updatePositionUrl) {
                return;
            }

            const item = evt.item;
            const newZone = item.closest<HTMLElement>(".layer-zone")?.dataset.zone;
            const metadata = item.querySelector<HTMLElement>(".layer-metadata");

            if (!metadata) {
                return;
            }

            const currentPosition = Number(metadata.dataset.position);
            const previous = item.previousElementSibling?.querySelector<HTMLElement>(".layer-metadata") ?? null;
            const next = item.nextElementSibling?.querySelector<HTMLElement>(".layer-metadata") ?? null;
            const contentItemId = metadata.dataset.contentItemId ?? "";

            let newPosition = currentPosition;

            if (!previous && next) {
                newPosition = Number(next.dataset.position || 0) - 1;
            } else if (previous && !next) {
                newPosition = Number(previous.dataset.position || 0) + 1;
            } else if (previous && next) {
                newPosition = (Number(previous.dataset.position || 0) + Number(next.dataset.position || 0)) / 2;
            }

            const currentZone = metadata.dataset.zone ?? "";

            metadata.dataset.zone = newZone;
            metadata.dataset.position = String(newPosition);

            const url = `${updatePositionUrl}?contentItemId=${encodeURIComponent(contentItemId)}&position=${encodeURIComponent(newPosition)}&zone=${encodeURIComponent(newZone ?? "")}`;
            const antiforgeryToken = document.querySelector<HTMLInputElement>("[name='__RequestVerificationToken']")
                ?.value;

            fetch(url, {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: `__RequestVerificationToken=${encodeURIComponent(antiforgeryToken ?? "")}`,
            })
                .then((response) => {
                    if (!response.ok) {
                        throw new Error("Request failed");
                    }

                    document.getElementById("layer-undo-message")?.classList.remove("d-none");
                    (document.getElementById("layer-undo-message-contentItemId") as HTMLInputElement).value =
                        contentItemId;
                    (document.getElementById("layer-undo-message-position") as HTMLInputElement).value =
                        String(currentPosition);
                    (document.getElementById("layer-undo-message-zone") as HTMLInputElement).value = currentZone;
                })
                .catch(() => {
                    document.getElementById("layer-undo-message")?.classList.add("d-none");
                });
        },
    });
});

document.addEventListener("click", (event) => {
    if (!(event.target instanceof Element) || !event.target.closest(".layer-check")) {
        return;
    }

    document.querySelectorAll(".list-group-item").forEach((item) => {
        item.classList.remove("list-group-item-success");
    });

    document.querySelectorAll<HTMLInputElement>(".layer-check:checked").forEach((checkbox) => {
        const layer = checkbox.value;

        document.querySelectorAll(`[data-layer="${layer}"]`).forEach((el) => {
            el.closest(".list-group-item")?.classList.add("list-group-item-success");
        });
    });
});
