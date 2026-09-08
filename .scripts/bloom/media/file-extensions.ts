/**
 * In OrchardCore, file extension validation is handled server-side.
 * These functions provide a basic client-side fallback.
 */

/**
 * Returns allowed file extensions. In OrchardCore all extensions are allowed client-side
 * (server validates against MediaOptions.AllowedFileExtensions).
 */
export const getAllowedFileExtensions = (_directory: string): string[] => {
  return ["*.*"];
};

/**
 * Always valid client-side. Server validates extensions.
 */
export const isValidFileExtension = (_directory: string, fileName: string): boolean => {
  if (fileName == null || fileName == "") {
    return false;
  }

  const fileExtension = fileName.split(".")?.pop()?.toLowerCase();
  if (fileName == fileExtension) {
    return false; // No extension
  }

  return true;
};
