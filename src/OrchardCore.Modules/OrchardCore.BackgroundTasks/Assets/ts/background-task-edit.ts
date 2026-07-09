const resetButton = document.getElementById("use-default-schedule");
const scheduleInput = document.querySelector<HTMLInputElement>(".background-task-schedule");
const defaultScheduleInput = document.querySelector<HTMLInputElement>(".background-task-default-schedule");

if (resetButton && scheduleInput && defaultScheduleInput) {
    resetButton.addEventListener("click", () => {
        scheduleInput.value = defaultScheduleInput.value;
    });
}
