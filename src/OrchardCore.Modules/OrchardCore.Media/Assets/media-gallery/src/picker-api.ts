export type MediaPickerConfig = {
    translations: string;
    basePath: string;
    uploadFilesUrl: string;
    allowedExtensions?: string;
    allowMultiple?: boolean;
    maxUploadChunkSize?: number;
};

export type MediaPickerFile = {
    name?: string;
    url?: string;
    filePath?: string;
};

type OpenMediaPickerFn = (
    mediaAppTranslations: string,
    basePath: string,
    uploadFilesUrl: string,
    allowedExtensions?: string,
    allowMultiple?: boolean,
) => Promise<{ mediaPath?: string; name?: string; url?: string }[]>;

type MediaPickerConfigElement = HTMLElement & {
    __openMediaPicker?: OpenMediaPickerFn;
};

export async function openMediaPicker(config: MediaPickerConfig): Promise<MediaPickerFile[]> {
    const el = document.querySelector<MediaPickerConfigElement>('[data-media-picker-config]');

    // The media-picker module (media-picker2.js) registers openMediaPicker on this
    // element when it loads, so we can call it directly without a dynamic import.
    if (typeof el?.__openMediaPicker !== 'function') {
        return [];
    }

    const items = await el.__openMediaPicker(
        config.translations,
        config.basePath,
        config.uploadFilesUrl,
        config.allowedExtensions ?? '',
        config.allowMultiple ?? true,
    );

    return items.map((item) => ({
        name: item.name,
        url: item.url,
        filePath: item.mediaPath,
    }));
}
