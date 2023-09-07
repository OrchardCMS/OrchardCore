/**
    Represents a Media element; a file with its metadata.
*/
export interface IMedia {
    mime: string;
    name: string;
    url: string;
    lastModify: string;
    size: number;
}

/**
    Represents a Media of Folder UI element that can be selected.
*/
export interface IMediaElement {
    name: string;
    path: string;
    folder: string;
    isDirectory: boolean;
}
