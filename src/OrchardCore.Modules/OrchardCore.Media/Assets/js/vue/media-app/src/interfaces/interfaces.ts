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
    selected: boolean;
}

/**
 * Represent a notification severity level
 */
export enum SeverityLevel {
    Success = "Success",
    Info = "Info",
    Warn = "Warn",
    Error = "Error",
}
