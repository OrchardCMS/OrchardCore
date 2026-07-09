const submitFilterButton = document.querySelector<HTMLElement>("[name='submit.Filter']");

document.querySelectorAll(".selectpicker").forEach((element) => {
    element.addEventListener("changed.bs.select", () => submitFilterButton?.click());
});

document.querySelectorAll(".filter-options select, .filter-options input").forEach((element) => {
    element.addEventListener("change", () => submitFilterButton?.click());
});

document.querySelectorAll<HTMLElement>(".filter").forEach((element) => {
    element.style.display = "";
});
