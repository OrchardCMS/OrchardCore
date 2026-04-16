export {};

import { openMediaPicker } from 'orchardcore-media-app/src/picker-api';

type MediaPickerConfig = {
    translations?: string;
    basePath?: string;
    uploadFilesUrl?: string;
    allowedExtensions?: string;
    maxUploadChunkSize?: number;
};

type MediaPickerFile = {
    filePath: string;
};

type TrumbowygEditable = {
    0?: HTMLElement;
};

type TrumbowygRange = {
    deleteContents: () => void;
    insertNode: (node: Node) => void;
};

type TrumbowygInstance = {
    saveRange: () => void;
    restoreRange: () => void;
    range: TrumbowygRange;
    syncCode: () => void;
    addBtnDef: (name: string, definition: { fn: () => Promise<boolean> }) => void;
    $c?: TrumbowygEditable;
};

type TrumbowygStatic = {
    langs?: Record<string, { insertImage?: string }>;
    plugins?: Record<string, { init: (trumbowyg: TrumbowygInstance) => void }>;
};

interface MediaPickerWindow extends Window {
    jQuery?: {
        trumbowyg?: TrumbowygStatic;
    };
}

async function openMediaPickerDialog(): Promise<MediaPickerFile[]> {
    const el = document.querySelector<HTMLElement>('[data-media-picker-config]');
    const config: MediaPickerConfig = {
        translations: el?.dataset.translations ?? '{}',
        basePath: el?.dataset.basePath ?? '/',
        uploadFilesUrl: el?.dataset.uploadFilesUrl ?? '/api/media/Upload',
    };

    return openMediaPicker({
        translations: config.translations ?? '{}',
        basePath: config.basePath ?? '/',
        uploadFilesUrl: config.uploadFilesUrl ?? '/api/media/Upload',
        allowedExtensions: config.allowedExtensions ?? '',
        allowMultiple: true,
        maxUploadChunkSize: config.maxUploadChunkSize ?? 0,
    });
}

function triggerEditorChange(trumbowyg: TrumbowygInstance): void {
    const editable = trumbowyg.$c?.[0];
    if (!editable) {
        return;
    }

    editable.dispatchEvent(new Event('tbwchange', { bubbles: true }));
    editable.focus();
}

const trumbowyg = (window as MediaPickerWindow).jQuery?.trumbowyg;

if (trumbowyg) {
    trumbowyg.langs ??= {};
    trumbowyg.langs.en = {
        ...trumbowyg.langs.en,
        insertImage: 'Insert Media',
    };

    trumbowyg.plugins ??= {};
    trumbowyg.plugins.insertImage = {
        init(editor: TrumbowygInstance): void {
            editor.addBtnDef('insertImage', {
                fn: async (): Promise<boolean> => {
                    editor.saveRange();

                    const selectedFiles = await openMediaPickerDialog();
                    if (!selectedFiles?.length) {
                        return false;
                    }

                    editor.restoreRange();
                    editor.range.deleteContents();

                    for (const selectedFile of selectedFiles) {
                        const mediaBodyContent = ` [image]${selectedFile.filePath}[/image]`;
                        editor.range.insertNode(document.createTextNode(mediaBodyContent));
                    }

                    editor.syncCode();
                    triggerEditorChange(editor);

                    return true;
                },
            });
        },
    };
}
