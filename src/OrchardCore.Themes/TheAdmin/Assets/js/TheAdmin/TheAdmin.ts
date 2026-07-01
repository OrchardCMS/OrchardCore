import "./menu";
import "./resizeDetector";

// Bootstrap is loaded as a shared global script resource (see ResourceManagementOptionsConfiguration.cs),
// not bundled here — this brings in its types only, with no runtime import/bundling.
declare const bootstrap: typeof import("bootstrap");

function confirmDialog({ callback, ...options }: { callback: (response: boolean) => void; [key: string]: any }) {
    const defaultOptions = document.getElementById("confirmRemoveModalMetadata")?.dataset ?? {};
    const { title, message, okText, cancelText, okClass, cancelClass } = { ...defaultOptions, ...options };

    const wrapper = document.createElement("div");
    wrapper.innerHTML = `<div id="confirmRemoveModal" class="modal" tabindex="-1" role="dialog">
        <div class="modal-dialog modal-dialog-centered" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">${title}</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <p>${message}</p>
                </div>
                <div class="modal-footer">
                    <button id="modalOkButton" type="button" class="btn ${okClass}">${okText}</button>
                    <button id="modalCancelButton" type="button" class="btn ${cancelClass}" data-bs-dismiss="modal">${cancelText}</button>
                </div>
            </div>
        </div>
    </div>`;
    document.body.appendChild(wrapper.firstElementChild as HTMLElement);

    const modalElement = document.getElementById("confirmRemoveModal");

    if (modalElement) {
        const confirmModal = new bootstrap.Modal(modalElement, {
            backdrop: "static",
            keyboard: false,
        });

        confirmModal.show();

        document.getElementById("confirmRemoveModal")?.addEventListener("hidden.bs.modal", function () {
            document.getElementById("confirmRemoveModal")?.remove();
            confirmModal.dispose();
        });

        document.getElementById("modalOkButton")?.addEventListener("click", function () {
            callback(true);
            confirmModal.hide();
        });

        document.getElementById("modalCancelButton")?.addEventListener("click", function () {
            callback(false);
            confirmModal.hide();
        });
    }
}

(function () {
    // Prevents page flickering while downloading css
    document.addEventListener("DOMContentLoaded", () => {
        document.body.classList.remove("preload");
    });
})();

document.addEventListener("DOMContentLoaded", () => {
    document.body.addEventListener("click", (event) => {
        const _this = (event.target as Element)?.closest<HTMLElement>('[data-url-af~="RemoveUrl"], a[itemprop~="RemoveUrl"]');
        if (!_this) {
            return;
        }

        if (_this.matches('a[itemprop~="UnsafeUrl"]')) {
            console.warn("Please use data-url-af instead of itemprop attribute for confirm modals. Using itemprop will eventually become deprecated.");
        }
        // don't show the confirm dialog if the link is also UnsafeUrl, as it will already be handled below.
        if (_this.matches('[data-url-af~="UnsafeUrl"], a[itemprop~="UnsafeUrl"]')) {
            event.preventDefault();
            return;
        }

        event.preventDefault();
        confirmDialog({
            ..._this.dataset,
            callback: function (resp: boolean) {
                if (resp) {
                    const url = _this.getAttribute("href");
                    if (url == null) {
                        const form = _this.closest("form");
                        if (form) {
                            // This line is reuired in case we used the FormValueRequiredAttribute
                            const input = document.createElement("input");
                            input.type = "hidden";
                            input.name = _this.getAttribute("name") ?? "";
                            input.value = _this.getAttribute("value") ?? "";
                            form.appendChild(input);
                            form.submit();
                        }
                    } else {
                        window.location.href = url;
                    }
                }
            },
        });
    });
});

document.addEventListener("DOMContentLoaded", () => {
    const magicToken = document.querySelector<HTMLInputElement>("input[name=__RequestVerificationToken]");
    if (magicToken) {
        document.body.addEventListener("click", (event) => {
            const _this = (event.target as Element)?.closest<HTMLElement>('a[data-url-af~="UnsafeUrl"], a[itemprop~="UnsafeUrl"]');
            if (!_this) {
                return;
            }

            if (_this.matches('a[itemprop~="UnsafeUrl"]')) {
                console.warn("Please use data-url-af instead of itemprop attribute for confirm modals. Using itemprop will eventually become deprecated.");
            }
            const hrefParts = _this.getAttribute("href")?.split("?");

            if (hrefParts == undefined) {
                event.preventDefault();
                return;
            }

            event.preventDefault();

            const form = document.createElement("form");
            form.action = hrefParts[0];
            form.method = "POST";
            form.appendChild(magicToken.cloneNode(true) as HTMLInputElement);
            if (hrefParts.length > 1) {
                const queryParts = hrefParts[1].split("&");
                for (let i = 0; i < queryParts.length; i++) {
                    const queryPartKVP = queryParts[i].split("=");
                    //trusting hrefs in the page here
                    const input = document.createElement("input");
                    input.type = "hidden";
                    input.name = decodeURIComponent(queryPartKVP[0]);
                    input.value = decodeURIComponent(queryPartKVP[1]);
                    form.appendChild(input);
                }
            }

            form.style.position = "absolute";
            form.style.left = "-9999em";
            document.body.appendChild(form);

            const unsafeUrlPrompt = _this.dataset.unsafeUrl;

            if (unsafeUrlPrompt && unsafeUrlPrompt.length > 0) {
                confirmDialog({
                    ..._this.dataset,
                    callback: function (resp: boolean) {
                        if (resp) {
                            form.submit();
                        }
                    },
                });

                return;
            }

            if (_this.matches('[data-url-af~="RemoveUrl"], a[itemprop~="RemoveUrl"]')) {
                confirmDialog({
                    ..._this.dataset,
                    callback: function (resp: boolean) {
                        if (resp) {
                            form.submit();
                        }
                    },
                });

                return;
            }

            form.submit();
        });
    }
});

(function () {
    // Tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
})();

//Prevent multi submissions on forms
document.body.addEventListener("submit", (event) => {
    const form = (event.target as Element)?.closest<HTMLFormElement>("form.no-multisubmit");
    if (!form) {
        return;
    }

    const submittingClass = "submitting";

    if (form.classList.contains(submittingClass)) {
        event.preventDefault();
        return;
    }

    form.classList.add(submittingClass);

    // safety-nest in case the form didn't refresh the page
    setTimeout(function () {
        form.classList.remove(submittingClass);
    }, 5000);
});

declare global {
    interface Window {
        confirmDialog: typeof confirmDialog;
    }
}

window.confirmDialog = confirmDialog;

export { confirmDialog };
