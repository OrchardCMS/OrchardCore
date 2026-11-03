interface PreviewMessage {
    type: "hello" | "token" | "error" | "disconnected";
    previewUrl?: string;
    errors?: string[];
}

const iframeA = document.getElementById("iframe-a") as HTMLIFrameElement | null;
const iframeB = document.getElementById("iframe-b") as HTMLIFrameElement | null;
const previewId = iframeA?.dataset.previewId;

if (iframeA && iframeB && previewId) {
    const frames = [iframeA, iframeB];
    let activeIdx = 0;
    let navigating = false;
    const channel = new BroadcastChannel(`contentpreview-${previewId}`);

    const getActive = () => frames[activeIdx];
    const getStaging = () => frames[1 - activeIdx];

    // Restore the exact pixel scroll position on the hidden staging frame before bringing it to
    // the front. Because the swap is instantaneous (z-index only, no animation), the user never
    // sees the staging frame scroll - they just see the new content already at the right position
    // after the swap.
    const swap = () => {
        const active = getActive();
        const staging = getStaging();
        const scrollY = active.contentWindow ? active.contentWindow.scrollY : 0;

        if (scrollY > 0 && staging.contentWindow) {
            staging.contentWindow.scrollTo(0, scrollY);
        }

        staging.style.zIndex = "999990";
        active.style.zIndex = "999989";
        activeIdx = 1 - activeIdx;
    };

    const onFrameLoad = function (this: HTMLIFrameElement) {
        if (navigating && this === getStaging()) {
            navigating = false;
            swap();
        }
    };

    frames[0].addEventListener("load", onFrameLoad);
    frames[1].addEventListener("load", onFrameLoad);

    channel.onmessage = (event: MessageEvent<PreviewMessage>) => {
        if (!event.data) {
            return;
        }

        switch (event.data.type) {
            case "hello":
                // The editor (re)connected. Re-announce readiness so it sends the current draft,
                // which restores the preview after an editor reload.
                channel.postMessage({ type: "ready" });
                break;
            case "token":
                navigating = true;
                getStaging().src = event.data.previewUrl ?? "";
                document.getElementById("notConnectedWarning")?.style.setProperty("display", "none");
                break;
            case "error":
                if (event.data.errors) {
                    const list = document.querySelector("#serverErrorWarning ul");

                    if (list) {
                        list.innerHTML = "";
                        event.data.errors.forEach((error) => {
                            console.error(error);

                            const item = document.createElement("li");
                            item.textContent = error;
                            list.appendChild(item);
                        });
                    }

                    document.getElementById("serverErrorWarning")?.style.setProperty("display", "block");
                }
                break;
            case "disconnected":
                document.getElementById("notConnectedWarning")?.style.setProperty("display", "block");
                break;
        }
    };

    document.getElementById("close-connect-warning")?.addEventListener("click", () => {
        document.getElementById("notConnectedWarning")?.style.setProperty("display", "none");
    });

    document.getElementById("close-server-warning")?.addEventListener("click", () => {
        document.getElementById("serverErrorWarning")?.style.setProperty("display", "none");
    });

    // Signal the editor that the preview window is ready to receive updates.
    channel.postMessage({ type: "ready" });
}
