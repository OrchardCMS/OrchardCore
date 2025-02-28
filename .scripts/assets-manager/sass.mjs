import fs from "fs-extra";
import { glob } from "glob";
import JSON5 from "json5";
import chalk from "chalk";
import path from "path";
import { transform } from "lightningcss";
import * as sass from "sass";
import postcss from "postcss";
import postcssRTLCSS from "postcss-rtlcss";
import { Mode } from "postcss-rtlcss/options";
import chokidar from "chokidar";
import process from "node:process";
import { Buffer } from "buffer";

let action = process.argv[2];
let mode = action === "build" ? "production" : "development";
const config = JSON5.parse(Buffer.from(process.argv[3], "base64").toString("utf-8"));
const dest = config.dest ?? config.basePath + "/wwwroot/Styles/";

if (config.dryRun) {
    action = "dry-run";
}

const isWatching = action === "watch";

/**
 * Recursively resolves all the @import statements in a given file.
 * @param {string} filePath - The path to the SCSS file.
 * @param {string} fileContent - The content of the SCSS file.
 * @param {Set<string>} [resolvedFiles] - The set of resolved files.
 * @returns {Set<string>} The set of resolved files.
 */
const resolveImports = (filePath, fileContent, resolvedFiles = new Set()) => {
    const importRegex = /@import\s+['"](.+?)['"]/g;
    let match;
    while ((match = importRegex.exec(fileContent)) !== null) {
        let importPath = path.resolve(path.dirname(filePath), match[1]);
        if (!importPath.endsWith(".scss")) {
            importPath += ".scss";
        }
        if (!resolvedFiles.has(importPath)) {
            resolvedFiles.add(importPath);

            let content;
            try {
                content = fs.readFileSync(importPath, "utf8");
            } catch {
                // Try with file name starting with '_'
                const altImportPath = path.join(path.dirname(importPath), "_" + path.basename(importPath));
                try {
                    content = fs.readFileSync(altImportPath, "utf8");
                    importPath = altImportPath; // Update to the underscore-prefixed path
                } catch (altErr) {
                    console.error(`Failed to resolve import at ${importPath} or ${altImportPath}:`, altErr);
                    continue;
                }
            }
            resolveImports(importPath, content, resolvedFiles);
        }
    }
    return resolvedFiles;
};

if (isWatching) {
    glob(config.source).then((files) => {
        const watchFiles = new Set();
        watchFiles.add(path.dirname(config.source));

        files.forEach((file) => {
            const content = fs.readFileSync(file, "utf8");
            const resolvedFiles = resolveImports(file, content);
            resolvedFiles.forEach((resolvedFile) => watchFiles.add(resolvedFile));
        });

        chokidar
            .watch([...watchFiles], {
                ignored: (path, stats) => stats?.isFile() && !path.endsWith(".scss"),
                persistent: true,
            })
            .on("change", () => {
                runSass(config);
            });
    });
} else {
    runSass(config);
}

/**
 * Transpiles SCSS files to CSS, including optional RTL transformation and minification.
 *
 * @param {Object} config - Configuration object for the Sass transpilation process.
 * @param {string} config.source - Glob pattern or path to the source SCSS files.
 * @param {string} [config.dest] - Destination directory for the output CSS files. Defaults to a path derived from config.basePath.
 * @param {boolean} [config.dryRun] - If true, performs a dry run without writing files.
 * @param {boolean} [config.generateRTL] - If true, generates RTL-compatible CSS using postcss-rtlcss.
 *
 * The function processes SCSS files by:
 * - Resolving the source files matching the given glob pattern.
 * - Checking if the destination directory exists and is valid.
 * - Transpiling SCSS to CSS, optionally generating source maps.
 * - Generating RTL CSS if specified.
 * - Minifying the resulting CSS.
 * - Logging the progress and results to the console.
 */

function runSass(config) {
    glob(config.source).then((files) => {
        if (files.length == 0) {
            console.log(chalk.yellow("No files to copy", config.source));
            return;
        }

        const destExists = fs.existsSync(dest);

        if (destExists) {
            const stats = fs.lstatSync(dest);
            if (!stats.isDirectory()) {
                console.log(chalk.red("Destination is not a directory"));
                console.log("Files:", files);
                console.log("Destination:", dest);
                return;
            }
            console.log(chalk.yellow(`Destination ${dest} already exists, files may be overwritten`));
        }

        let baseFolder;

        if (config.source.indexOf("**") > 0) {
            baseFolder = config.source.substring(0, config.source.indexOf("**"));
        }

        files.forEach((file) => {
            file = file.replace(/\\/g, "/");
            let relativePath;

            if (baseFolder) {
                relativePath = file.replace(baseFolder, "");
            } else {
                relativePath = path.basename(file);
            }

            const target = path.join(dest, relativePath);

            if (action === "dry-run") {
                console.log(`Dry run (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(file), chalk.cyan(target));
            } else {
                fs.stat(file).then(async (stat) => {
                    if (!stat.isDirectory()) {
                        let fileInfo = path.parse(file);

                        if (fileInfo.ext === ".scss") {
                            const scssResult = await sass.compileAsync(file, {
                                sourceMap: mode === "production",
                                sourceMapIncludeSources: false,
                            });

                            /*                             if (mode === "production" && scssResult.sourceMap) {
                                const mappedTarget = path.join(dest, path.parse(target).name + ".scss.map");
                                fs.outputFile(mappedTarget, JSON5.stringify(scssResult.sourceMap));
                                console.log(`Mapped (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(file), chalk.cyan(mappedTarget));
                            } */

                            if (scssResult.css) {
                                const normalTarget = path.join(dest, path.parse(target).name + ".css");
                                await fs.outputFile(normalTarget, scssResult.css);
                                console.log(`Tranpiled (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(file), chalk.cyan(normalTarget));

                                if (config.generateRTL) {
                                    const options = {
                                        mode: Mode.combined,
                                    };

                                    const result = await postcss([postcssRTLCSS(options)]).process(scssResult.css, { from: file });

                                    await fs.outputFile(normalTarget, result.css);
                                    scssResult.css = result.css;
                                    console.log(`RTL (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(normalTarget), chalk.cyan(normalTarget));
                                }

                                let { code, map } = transform({
                                    code: Buffer.from(scssResult.css),
                                    minify: true,
                                    sourceMap: true,
                                });

                                if (code) {
                                    const minifiedTarget = path.join(dest, path.parse(target).name + ".min.css");
                                    fs.outputFile(minifiedTarget, code);
                                    console.log(`Minified (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(normalTarget), chalk.cyan(minifiedTarget));
                                }

/*                                 if (mode === "production" && map) {
                                    const mappedTarget = path.join(dest, path.parse(target).name + ".css.map");
                                    fs.outputFile(mappedTarget, map);
                                    console.log(`Mapped (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(normalTarget), chalk.cyan(mappedTarget));
                                } */
                            }
                        } else {
                            console.log("Trying to transpile a SASS file with an extension that is not allowed.");
                        }
                    }
                });
            }
        });
    });
}
