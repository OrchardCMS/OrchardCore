// Shared by Media/MediaProfiles/Create.cshtml and Edit.cshtml - both bind the same
// MediaProfileViewModel, so element ids are identical between the two views. The only
// structural difference (Create's wrapper uses ".form-g", Edit's uses ".ocat-limited-wrapper")
// is resolved by both views tagging their BackgroundColor row with a shared marker class instead.
const initMediaProfileEditor = () => {
    const selectedMode = document.getElementById("SelectedMode") as HTMLSelectElement | null;
    const backgroundColor = document.getElementById("BackgroundColor") as HTMLInputElement | null;
    const selectedWidth = document.getElementById("SelectedWidth") as HTMLSelectElement | null;
    const customWidth = document.getElementById("CustomWidth") as HTMLInputElement | null;
    const selectedHeight = document.getElementById("SelectedHeight") as HTMLSelectElement | null;
    const customHeight = document.getElementById("CustomHeight") as HTMLInputElement | null;

    if (!selectedMode || !backgroundColor || !selectedWidth || !customWidth || !selectedHeight || !customHeight) {
        return;
    }

    const padValue = selectedMode.dataset.padValue;
    const boxPadValue = selectedMode.dataset.boxPadValue;
    const backgroundColorWrapper = backgroundColor.closest<HTMLElement>(".media-profile-background-color-wrapper");

    selectedMode.addEventListener("change", () => {
        if (!backgroundColorWrapper) {
            return;
        }

        if (selectedMode.value === padValue || selectedMode.value === boxPadValue) {
            backgroundColorWrapper.style.display = "";
        } else {
            backgroundColorWrapper.style.display = "none";
            backgroundColor.value = "";
        }
    });

    selectedWidth.addEventListener("change", () => {
        if (selectedWidth.value === "-1") {
            customWidth.style.display = "";
        } else {
            customWidth.style.display = "none";
            customWidth.value = "0";
        }
    });

    selectedHeight.addEventListener("change", () => {
        if (selectedHeight.value === "-1") {
            customHeight.style.display = "";
        } else {
            customHeight.style.display = "none";
            customHeight.value = "0";
        }
    });
};

export default initMediaProfileEditor;
