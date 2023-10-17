// https://github.com/spatie/font-awesome-filetypes

const fa_icons = {
    image: 'fa-regular fa-image',
    pdf: 'fa-regular fa-file-pdf',
    word: 'fa-regular fa-file-word',
    powerpoint: 'fa-regular fa-file-powerpoint',
    excel: 'fa-regular fa-file-excel',
    csv: 'fa-regular fa-file',
    audio: 'fa-regular fa-file-audio',
    video: 'fa-regular fa-file-video',
    archive: 'fa-regular fa-file-zipper',
    code: 'fa-regular fa-file-code',
    text: 'fa-regular fa-file-lines',
    file: 'fa-regular fa-file'
};

const fa_thumbnails = {
    gif: fa_icons.image,
    jpeg: fa_icons.image,
    jpg: fa_icons.image,
    png: fa_icons.image,

    pdf: fa_icons.pdf,

    doc: fa_icons.word,
    docx: fa_icons.word,

    ppt: fa_icons.powerpoint,
    pptx: fa_icons.powerpoint,

    xls: fa_icons.excel,
    xlsx: fa_icons.excel,

    csv: fa_icons.csv,

    aac: fa_icons.audio,
    mp3: fa_icons.audio,
    ogg: fa_icons.audio,

    avi: fa_icons.video,
    flv: fa_icons.video,
    mkv: fa_icons.video,
    mp4: fa_icons.video,
    webm: fa_icons.video,

    gz: fa_icons.archive,
    zip: fa_icons.archive,

    css: fa_icons.code,
    html: fa_icons.code,
    js: fa_icons.code,

    txt: fa_icons.text
};

function getClassNameForExtension(extension) {
    return fa_thumbnails[extension.toLowerCase()] || fa_icons.file
}

function getExtensionForFilename(filename) {
    return filename.slice((filename.lastIndexOf('.') - 1 >>> 0) + 2)
}


function getClassNameForFilename(filename) {
    return getClassNameForExtension(getExtensionForFilename(filename))
}