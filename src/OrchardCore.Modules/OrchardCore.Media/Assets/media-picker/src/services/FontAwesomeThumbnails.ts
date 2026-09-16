// Extension → FontAwesome icon mapping
// Based on https://github.com/spatie/font-awesome-filetypes

const faIcons = {
  image: "fa-regular fa-image",
  pdf: "fa-regular fa-file-pdf",
  word: "fa-regular fa-file-word",
  powerpoint: "fa-regular fa-file-powerpoint",
  excel: "fa-regular fa-file-excel",
  csv: "fa-regular fa-file",
  audio: "fa-regular fa-file-audio",
  video: "fa-regular fa-file-video",
  archive: "fa-regular fa-file-zipper",
  code: "fa-regular fa-file-code",
  text: "fa-regular fa-file-lines",
  file: "fa-regular fa-file",
} as const;

const faThumbnails: Record<string, string> = {
  gif: faIcons.image,
  jpeg: faIcons.image,
  jpg: faIcons.image,
  png: faIcons.image,
  webp: faIcons.image,
  svg: faIcons.image,
  bmp: faIcons.image,
  ico: faIcons.image,
  pdf: faIcons.pdf,
  doc: faIcons.word,
  docx: faIcons.word,
  ppt: faIcons.powerpoint,
  pptx: faIcons.powerpoint,
  xls: faIcons.excel,
  xlsx: faIcons.excel,
  csv: faIcons.csv,
  aac: faIcons.audio,
  mp3: faIcons.audio,
  ogg: faIcons.audio,
  avi: faIcons.video,
  flv: faIcons.video,
  mkv: faIcons.video,
  mp4: faIcons.video,
  webm: faIcons.video,
  gz: faIcons.archive,
  zip: faIcons.archive,
  css: faIcons.code,
  html: faIcons.code,
  js: faIcons.code,
  txt: faIcons.text,
};

function getExtension(filename: string): string {
  return filename.slice((filename.lastIndexOf(".") - 1 >>> 0) + 2);
}

export function getIconClassForFilename(filename: string, sizeClass = ""): string {
  const ext = getExtension(filename).toLowerCase();
  const icon = faThumbnails[ext] || faIcons.file;
  return sizeClass ? `${icon} ${sizeClass}` : icon;
}

export function isImageExtension(ext: string): boolean {
  const lower = ext.replace(/^\./, "").toLowerCase();
  return ["gif", "jpeg", "jpg", "png", "webp", "svg", "bmp", "ico"].includes(lower);
}
