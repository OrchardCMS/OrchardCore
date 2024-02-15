export function optionsChanged(element, jsonValue, defaultValue) {
    const evt = new CustomEvent("blazor:optionsChanged", {
        bubbles: true,
        detail: {
            options: jsonValue,
            defaultValue: defaultValue
        }
    });
    element.dispatchEvent(evt);
}
