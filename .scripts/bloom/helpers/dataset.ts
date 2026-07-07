const getDatasetBoolean = (element: HTMLElement, key: string) => element.dataset[key] === "true";

const getDatasetNumber = (element: HTMLElement, key: string) => Number(element.dataset[key]);

const getDatasetJson = <T>(element: HTMLElement, key: string): T | undefined => {
    const raw = element.dataset[key];

    return raw ? (JSON.parse(raw) as T) : undefined;
};

export { getDatasetBoolean, getDatasetNumber, getDatasetJson };
