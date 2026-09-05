const tenant = document.querySelector<HTMLInputElement>(".openid-validation-tenant");
const authority = document.querySelector<HTMLInputElement>(".openid-validation-authority");
const audience = document.querySelector<HTMLInputElement>(".openid-validation-audience");
const disableTokenTypeValidation = document.querySelector<HTMLInputElement>(".openid-validation-disable-token-type");
const authorityField = document.getElementById("AuthorityField");
const audienceField = document.getElementById("AudienceField");
const disableTokenTypeValidationField = document.getElementById("DisableTokenTypeValidationField");

if (tenant && authority && audience && disableTokenTypeValidation && authorityField && audienceField && disableTokenTypeValidationField) {
    const refresh = () => {
        if (tenant.value !== "") {
            authority.value = "";
            authorityField.style.display = "none";
            audience.value = "";
            audienceField.style.display = "none";
            // Pre-existing: sets the checkbox's value attribute, not its checked state -
            // preserved as-is, unrelated to this extraction.
            disableTokenTypeValidation.value = "false";
            disableTokenTypeValidationField.style.display = "none";
        } else {
            authorityField.style.display = "";
            audienceField.style.display = "";
            disableTokenTypeValidationField.style.display = "";
        }
    };

    tenant.addEventListener("change", refresh);
    refresh();
}
