import fs from "fs-extra";
import path from "path";
import { Buffer } from "buffer";

const textOutputExtensions = new Set([
    ".css",
    ".js",
    ".json",
    ".map",
]);

function normalizeLineEndings(content) {
    return content.replace(/\r\n/g, "\n").replace(/\r/g, "\n");
}

function normalizeSourceMapContent(content) {
    const sourceMap = JSON.parse(content);

    if (Array.isArray(sourceMap.sourcesContent)) {
        sourceMap.sourcesContent = sourceMap.sourcesContent.map((sourceContent) =>
            typeof sourceContent === "string"
                ? normalizeLineEndings(sourceContent)
                : sourceContent);
    }

    return JSON.stringify(sourceMap) + "\n";
}

function isTextOutput(filePath) {
    const extension = path.extname(filePath).toLowerCase();

    return textOutputExtensions.has(extension);
}

async function writeIfChanged(filePath, content, encoding = undefined) {
    await fs.ensureDir(path.dirname(filePath));

    if (await fs.pathExists(filePath)) {
        const existingContent = await fs.readFile(filePath, encoding);

        if (existingContent === content) {
            return false;
        }
    }

    await fs.writeFile(filePath, content, encoding);

    return true;
}

export async function writeAssetFile(filePath, content) {
    if (isTextOutput(filePath)) {
        let normalizedContent = normalizeLineEndings(
            typeof content === "string"
                ? content
                : content.toString("utf8"));

        if (path.extname(filePath).toLowerCase() === ".map") {
            normalizedContent = normalizeSourceMapContent(normalizedContent);
        }

        return writeIfChanged(filePath, normalizedContent, "utf8");
    }

    const buffer = Buffer.isBuffer(content)
        ? content
        : Buffer.from(content);

    await fs.ensureDir(path.dirname(filePath));

    if (await fs.pathExists(filePath)) {
        const existingContent = await fs.readFile(filePath);

        if (existingContent.equals(buffer)) {
            return false;
        }
    }

    await fs.writeFile(filePath, buffer);

    return true;
}

export async function copyAssetFile(sourceFilePath, targetFilePath) {
    if (isTextOutput(targetFilePath)) {
        const content = await fs.readFile(sourceFilePath, "utf8");

        return writeAssetFile(targetFilePath, content);
    }

    await fs.ensureDir(path.dirname(targetFilePath));
    await fs.copy(sourceFilePath, targetFilePath, { overwrite: true });

    return true;
}

export { normalizeLineEndings };
