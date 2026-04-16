export {};

import { openMediaPicker } from 'orchardcore-media-app/src/picker-api';

type MediaPickerFile = {
    filePath?: string;
};

type MediaPickerConfig = {
    translations?: string;
    basePath?: string;
    uploadFilesUrl?: string;
    allowedExtensions?: string;
    maxUploadChunkSize?: number;
};

type EasyMDEToolbarButton = {
    name: string;
    action?: string | ((editor: EasyMDEInstance) => void) | ((editor: EasyMDEInstance) => Promise<void>);
    className?: string;
    title?: string;
    default?: boolean;
};

type EasyMDECodeMirror = {
    replaceSelection: (text: string) => void;
};

type EasyMDEInstance = {
    codemirror: EasyMDECodeMirror;
    gui: {
        toolbar: HTMLElement;
    };
};

type EasyMDEStatic = {
    undo: (editor: EasyMDEInstance) => void;
    redo: (editor: EasyMDEInstance) => void;
    toggleBold: (editor: EasyMDEInstance) => void;
    toggleItalic: (editor: EasyMDEInstance) => void;
    toggleStrikethrough: (editor: EasyMDEInstance) => void;
    toggleHeadingSmaller: (editor: EasyMDEInstance) => void;
    toggleCodeBlock: (editor: EasyMDEInstance) => void;
    toggleBlockquote: (editor: EasyMDEInstance) => void;
    drawLink: (editor: EasyMDEInstance) => void;
    toggleUnorderedList: (editor: EasyMDEInstance) => void;
    toggleOrderedList: (editor: EasyMDEInstance) => void;
    drawTable: (editor: EasyMDEInstance) => void;
    drawHorizontalRule: (editor: EasyMDEInstance) => void;
    togglePreview: (editor: EasyMDEInstance) => void;
    toggleSideBySide: (editor: EasyMDEInstance) => void;
    toggleFullScreen: (editor: EasyMDEInstance) => void;
};

type ShortcodesApp = {
    init: (callback: (defaultValue: string) => void) => void;
};

interface MediaPickerWindow extends Window {
    shortcodesApp?: ShortcodesApp;
    EasyMDE?: EasyMDEStatic;
    mdeToolbar?: (EasyMDEToolbarButton | '|')[];
    initializeMdeShortcodeWrapper?: (mde: EasyMDEInstance) => void;
}

const w = window as MediaPickerWindow;
const EasyMDE = w.EasyMDE;

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

w.mdeToolbar = [
    {
        name: 'guide',
        action: 'https://www.markdownguide.org/basic-syntax/',
        className: 'fab fa-markdown fa-sm',
        title: 'Markdown Guide',
    },
    '|',
    {
        name: 'undo',
        action: EasyMDE?.undo,
        className: 'fas fa-undo no-disable fa-sm',
        title: 'Undo',
    },
    {
        name: 'redo',
        action: EasyMDE?.redo,
        className: 'fas fa-redo no-disable fa-sm',
        title: 'Redo',
    },
    '|',
    {
        name: 'bold',
        action: EasyMDE?.toggleBold,
        className: 'fas fa-bold fa-sm',
        title: 'Bold',
    },
    {
        name: 'italic',
        action: EasyMDE?.toggleItalic,
        className: 'fas fa-italic fa-sm',
        title: 'Italic',
    },
    {
        name: 'strikethrough',
        action: EasyMDE?.toggleStrikethrough,
        className: 'fas fa-strikethrough fa-sm',
        title: 'Strikethrough',
    },
    '|',
    {
        name: 'heading',
        action: EasyMDE?.toggleHeadingSmaller,
        className: 'fas fa-heading fa-sm',
        title: 'Heading',
    },
    '|',
    {
        name: 'code',
        action: EasyMDE?.toggleCodeBlock,
        className: 'fas fa-code fa-sm',
        title: 'Code',
    },
    {
        name: 'quote',
        action: EasyMDE?.toggleBlockquote,
        className: 'fas fa-quote-left fa-sm',
        title: 'Quote',
    },
    '|',
    {
        name: 'link',
        action: EasyMDE?.drawLink,
        className: 'fas fa-link fa-sm',
        title: 'Create Link',
    },
    '|',
    {
        name: 'shortcode',
        className: 'icon-shortcode',
        title: 'Insert Shortcode',
        default: true,
        action: function (editor: EasyMDEInstance) {
            w.shortcodesApp?.init(function (defaultValue: string) {
                editor.codemirror.replaceSelection(defaultValue);
            });
        },
    },
    '|',
    {
        name: 'image',
        action: async function (editor: EasyMDEInstance) {
            const selectedFiles = await openMediaPickerDialog();
            if (!selectedFiles.length) {
                return;
            }

            let mediaMarkdownContent = '';
            for (let i = 0; i < selectedFiles.length; i++) {
                mediaMarkdownContent += ' [image]' + selectedFiles[i].filePath + '[/image]';
            }

            editor.codemirror.replaceSelection(mediaMarkdownContent);
        },
        className: 'far fa-image fa-sm',
        title: 'Insert Image',
        default: true,
    },
    '|',
    {
        name: 'unordered-list',
        action: EasyMDE?.toggleUnorderedList,
        className: 'fa fa-list-ul fa-sm',
        title: 'Generic List',
    },
    {
        name: 'ordered-list',
        action: EasyMDE?.toggleOrderedList,
        className: 'fa fa-list-ol fa-sm',
        title: 'Numbered List',
    },
    {
        name: 'mdtable',
        action: EasyMDE?.drawTable,
        className: 'fas fa-table fa-sm',
        title: 'Insert Table',
    },
    '|',
    {
        name: 'horizontal-rule',
        action: EasyMDE?.drawHorizontalRule,
        className: 'fas fa-minus fa-sm',
        title: 'Insert Horizontal Line',
    },
    '|',
    {
        name: 'preview',
        action: EasyMDE?.togglePreview,
        className: 'fas fa-eye no-disable fa-sm',
        title: 'Toggle Preview',
    },
    {
        name: 'side-by-side',
        action: EasyMDE?.toggleSideBySide,
        className: 'fas fa-columns no-disable no-mobile fa-sm',
        title: 'Toggle Side by Side',
    },
    {
        name: 'fullscreen',
        action: EasyMDE?.toggleFullScreen,
        className: 'fas fa-arrows-alt no-disable no-mobile fa-sm',
        title: 'Toggle Fullscreen',
    },
];

w.initializeMdeShortcodeWrapper = function (mde: EasyMDEInstance): void {
    const toolbar = mde.gui.toolbar;
    const wrapper = document.createElement('div');
    wrapper.className = 'shortcode-modal-wrapper';
    toolbar.parentNode?.insertBefore(wrapper, toolbar);
    wrapper.appendChild(toolbar);
};
