const indexPreviewUrl = document.getElementById("indexPreviewUrl")?.dataset.value ?? "";
let contentPreviewUrl = document.getElementById("contentPreviewUrl")?.dataset.value ?? "";
const renderPreviewUrl = document.getElementById("renderPreviewUrl")?.dataset.value ?? "";
const iframe = document.getElementById("iframe") as HTMLIFrameElement | null;
const notConnectedWarning = document.getElementById("notConnectedWarning");
const serverErrorWarning = document.getElementById("serverErrorWarning");

let previewEventTimer: ReturnType<typeof setTimeout> | undefined;
let previewRenderTimer: ReturnType<typeof setTimeout> | undefined;
let previewRendering = false;
let previewRenderData: string | undefined;
let initialized = false;

const toQueryString = (data: Record<string, string>) =>
    Object.keys(data)
        .map((key) => `${encodeURIComponent(key)}=${encodeURIComponent(data[key])}`)
        .join("&");

function renderPreview(value: string) {
    if (previewRendering) {
        clearTimeout(previewRenderTimer);
        previewRenderTimer = setTimeout(() => renderPreview(value), 100);
    }

    previewRendering = true;
    clearTimeout(previewRenderTimer);

    try {
        let formData = JSON.parse(value);
        if (!formData) {
            previewRendering = false;
            return;
        }
        formData += `&${toQueryString({ Handle: contentPreviewUrl || "" })}`;

        fetch(renderPreviewUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8",
            },
            body: formData,
        })
            .then((response) => {
                if (!response.ok) {
                    throw response;
                }

                return response.text();
            })
            .then((data) => {
                if (iframe?.contentWindow) {
                    iframe.contentWindow.document.open();
                    iframe.contentWindow.document.write(data);
                    iframe.contentWindow.document.close();
                    previewRenderData = data;
                }
                if (serverErrorWarning) {
                    serverErrorWarning.style.display = "none";
                }
            })
            .catch((data: { status?: number }) => {
                if (data.status) {
                    const ul = document.querySelector("#serverErrorWarning ul");
                    if (ul) {
                        ul.innerHTML = "";
                        const error = `Status code ${data.status}`;
                        console.error(error);
                        const li = document.createElement("li");
                        li.textContent = error;
                        ul.appendChild(li);
                    }
                    if (serverErrorWarning) {
                        serverErrorWarning.style.display = "block";
                    }
                }
                previewRendering = false;
            });
    } catch (e) {
        previewRendering = false;
        console.log(`Error while previewing: ${e}`);
    }
}

window.addEventListener("storage", (ev) => {
    if (ev.key === "OrchardCore.templates:not-connected") {
        if (notConnectedWarning) {
            notConnectedWarning.style.display = "block";
        }
    } else if (ev.key === "OrchardCore.templates") {
        if (ev.newValue != null) {
            // Smooth event cascading
            clearTimeout(previewEventTimer);
            previewEventTimer = setTimeout(() => renderPreview(ev.newValue as string), 150);
            if (notConnectedWarning) {
                notConnectedWarning.style.display = "none";
            }
        }
    }
});

// override default behavior of Bootstrap's. We only hide, not remove the alert.
document.getElementById("close-warning")?.addEventListener("click", () => {
    if (notConnectedWarning) {
        notConnectedWarning.style.display = "none";
    }
});

document.getElementById("close-server-warning")?.addEventListener("click", () => {
    if (serverErrorWarning) {
        serverErrorWarning.style.display = "none";
    }
});

const preview = localStorage.getItem("OrchardCore.templates");

if (preview == null) {
    // notify the editor to render the preview
    localStorage.setItem("OrchardCore.templates:ready", "");
    localStorage.removeItem("OrchardCore.templates:ready");
} else {
    renderPreview(preview);
}

if (iframe) {
    iframe.onload = () => {
        // The iframe may not be well setup after the first loading and then on the first editor update
        // we may lose the scrolling position. A workaround is to refrech the page once after or to add
        // here a delay that really block the thread before exiting. Here, we just rewrite the document.
        if (!initialized) {
            if (iframe.contentWindow) {
                // Pre-existing: previewRenderData may still be undefined here if the initial
                // render's fetch hasn't resolved yet - preserved as-is (document.write coerces
                // it to the literal string "undefined" in that race, same as the original).
                iframe.contentWindow.document.open();
                iframe.contentWindow.document.write(previewRenderData as string);
                iframe.contentWindow.document.close();
            }
            initialized = true;
        }
        previewRendering = false;

        if (iframe.contentWindow && iframe.contentWindow.location.pathname !== indexPreviewUrl) {
            contentPreviewUrl = iframe.contentWindow.location.pathname;
        }
    };
}
