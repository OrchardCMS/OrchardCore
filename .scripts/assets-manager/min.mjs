import fs from "fs-extra";
import { glob } from "glob";
import JSON5 from "json5";
import chalk from "chalk";
import path from "path";
import swc from "@swc/core";
import { transform } from "lightningcss";
import postcss from "postcss";
import postcssRTLCSS from "postcss-rtlcss";
import { Mode, Source } from "postcss-rtlcss/options";
import { Buffer } from "buffer";
import process from "node:process";

let action = process.argv[2];
let mode = action === "build" ? "production" : "development";

const config = JSON5.parse(Buffer.from(process.argv[3], "base64").toString("utf-8"));

let dest = config.dest;
let fileExtension = config.source.split(".").pop();

if (config.dest == undefined) {
    if (config.tags.includes("js") || fileExtension == "js") {
        dest = config.basePath + "/wwwroot/Scripts/";
    } else if (config.tags.includes("css") || fileExtension == "css") {
        dest = config.basePath + "/wwwroot/Styles/";
    }
}

if (config.dryRun) {
    action = "dry-run";
}

// console.log(`minify ${action}`, config);

glob(config.source).then((files) => {
    if (files.length == 0) {
        console.log(chalk.yellow("No files to minify", config.source));
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

                    if (fileInfo.ext === ".js") {
                        let reader = await fs.readFile(file, "utf8");

                        await swc
                            .minify(reader.replace(/(\r?\n|\r)/gm, "\n"), {
                                compress: true,
                                sourceMap: mode === "production",
                            })
                            .then((output) => {
                                const minifiedTarget = path.join(dest, path.parse(target).name + ".min.js");

                                fs.outputFile(minifiedTarget, output.code);
                                console.log(`Minified (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(file), chalk.cyan(minifiedTarget));

                                if (mode === "production" && output.map) {
                                    const mappedTarget = path.join(dest, path.parse(target).name + ".map");
                                    let normalized = output.map.replace(/(?:\\[rn])+/g, "\\n");
                                    fs.outputFile(mappedTarget, normalized + "\n");
                                    console.log(`Mapped (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(file), chalk.cyan(mappedTarget));
                                }
                            });

                        let sourceFile = reader.toString().replace(/(?:\\[rn])+/g, "\\n");
                        
                        // Remove last line from sourceFile if it is a newline
                        let sourceLines = sourceFile.split("\n");
                        if (sourceLines[sourceLines.length - 1] === "") {
                            sourceLines.pop();
                        }
                        sourceFile = sourceLines.join("\n");
                        sourceFile = sourceFile + "\n";

                        await fs
                            .outputFile(target, sourceFile)
                            .then(() => console.log(`Copied (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(file), chalk.cyan(target)))
                            .catch((err) => {
                                console.log(`${chalk.red("Error copying")} (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(file), chalk.cyan(target), chalk.red(err));
                                throw err;
                            });
                    } else if (fileInfo.ext === ".css") {
                        let reader = await fs.readFile(file, "utf8");

                        if (config.generateRTL) {
                            const rtlTarget = path.join(dest, path.parse(target).name + ".css");

                            const options = {
                                mode: Mode.combined,
                                from: Source.css,
                            };

                            const result = await postcss([postcssRTLCSS(options)]).process(reader);
                            reader = result.css;

                            console.log(`RTL (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(rtlTarget), chalk.cyan(rtlTarget));
                        }

                        const copyTarget = path.join(dest, path.parse(target).base);

                        await fs
                            .outputFile(copyTarget, reader)
                            .then(() => {
                                console.log(`Copied (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(file), chalk.cyan(target));
                            })
                            .catch((err) => {
                                console.log(`${chalk.red("Error copying")} (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(file), chalk.cyan(target), chalk.red(err));
                                throw err;
                            });

                        let { code } = transform({
                            code: Buffer.from(reader),
                            minify: true
                        });

                        if (code) {
                            const minifiedTarget = path.join(dest, path.parse(target).name + ".min.css");
                            await fs.outputFile(minifiedTarget, code.toString());
                            console.log(`Minified (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(file), chalk.cyan(minifiedTarget));
                        }
                    } else {
                        console.log("Trying to minify a file with an extension that is not allowed.");
                    }
                }
            });
        }
    });
});
