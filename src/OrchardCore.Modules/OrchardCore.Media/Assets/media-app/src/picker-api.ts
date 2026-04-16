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

type MediaPickerRuntimeModule = {
    openMediaPicker?: (config: MediaPickerConfig) => Promise<MediaPickerFile[]>;
};

type MediaPickerRuntimeWindow = Window & {
    OrchardCoreMedia?: MediaPickerRuntimeModule;
};

export async function openMediaPicker(config: MediaPickerConfig): Promise<MediaPickerFile[]> {
    const runtimeModule = (window as MediaPickerRuntimeWindow).OrchardCoreMedia;

    if (typeof runtimeModule?.openMediaPicker !== 'function') {
        return [];
    }

    return runtimeModule.openMediaPicker(config);
}
