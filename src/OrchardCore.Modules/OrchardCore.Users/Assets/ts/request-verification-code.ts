// Shared by both EmailAuthenticatorValidation.cshtml and SmsAuthenticatorValidation.cshtml -
// their scripts are byte-identical (only the surrounding page text differs).
const box = document.getElementById("RequestCodeFeedback");
const btn = document.getElementById("RequestCode");
const form = document.getElementById("RequestCodeForm") as HTMLFormElement | null;

if (box && btn && form) {
    btn.addEventListener("click", (event) => {
        event.preventDefault();
        box.classList.add("d-none");

        fetch(form.getAttribute("action") ?? "", {
            method: form.getAttribute("method") ?? "post",
            body: new FormData(form),
        })
            .then((response) => response.json())
            .then((result) => {
                if (result.success) {
                    box.classList.add("alert-success");
                    box.classList.remove("alert-danger");
                } else {
                    box.classList.add("alert-danger");
                    box.classList.remove("alert-success");
                }

                box.innerText = result.message;
                box.classList.remove("d-none");
            })
            .catch((error) => console.log(error));
    });
}
