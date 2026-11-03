const submitFilterButton = document.querySelector<HTMLElement>("[name='submit.Filter']");

document.querySelectorAll<HTMLElement>(".selectpicker").forEach((element) => {
    element.addEventListener("changed.bs.select", () => submitFilterButton?.click());
});

export {};
