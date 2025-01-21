import fs from "fs-extra";
import { glob } from "glob";
import JSON5 from "json5";
import chalk from "chalk";
import path from "path";
import { transform } from "lightningcss";
import * as sass from "sass";
import postcss from "postcss";
import postcssRTLCSS from "postcss-rtlcss";
import { Mode, Source } from "postcss-rtlcss/options";

let action = process.argv[2];
let mode = action === "build" ? "production" : "development";
const config = JSON5.parse(
    Buffer.from(process.argv[3], "base64").toString("utf-8")
);
const dest = config.dest ?? config.basePath + "/wwwroot/Styles/";

if (config.dryRun) {
    action = "dry-run";
}

// console.log(`sass ${action}`, config);

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
        console.log(
            chalk.yellow(
                `Destination ${dest} already exists, files may be overwritten`
            )
        );
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
            console.log(
                `Dry run (${chalk.gray("from")}, ${chalk.cyan("to")})`,
                chalk.gray(file),
                chalk.cyan(target)
            );
        } else {
            fs.stat(file).then(async (stat) => {
                if (!stat.isDirectory()) {
                    let fileInfo = path.parse(file);

                    if (fileInfo.ext === ".scss") {
                        const scssResult = await sass.compileAsync(file, {
                            sourceMap: mode === "development",
                            sourceMapIncludeSources: false,
                        });

                        if (scssResult.sourceMap) {
                            const mappedTarget = path.join(
                                dest,
                                path.parse(target).name + ".scss.map"
                            );
                            fs.outputFile(
                                mappedTarget,
                                JSON5.stringify(scssResult.sourceMap)
                            );
                            console.log(
                                `Mapped (${chalk.gray("from")}, ${chalk.cyan(
                                    "to"
                                )})`,
                                chalk.gray(file),
                                chalk.cyan(mappedTarget)
                            );
                        }

                        if (scssResult.css) {
                            const normalTarget = path.join(
                                dest,
                                path.parse(target).name + ".css"
                            );
                            await fs.outputFile(normalTarget, scssResult.css);
                            console.log(
                                `Tranpiled (${chalk.gray("from")}, ${chalk.cyan(
                                    "to"
                                )})`,
                                chalk.gray(file),
                                chalk.cyan(normalTarget)
                            );

                            if (config.generateRTL) {
                                const options = {
                                    mode: Mode.combined,
                                };

                                const result = await postcss([
                                    postcssRTLCSS(options),
                                ]).process(scssResult.css, { from: file });

                                await fs.outputFile(normalTarget, result.css);
                                scssResult.css = result.css;
                                console.log(
                                    `RTL (${chalk.gray("from")}, ${chalk.cyan(
                                        "to"
                                    )})`,
                                    chalk.gray(normalTarget),
                                    chalk.cyan(normalTarget)
                                );
                            }

                            let { code, map } = transform({
                                code: Buffer.from(scssResult.css),
                                minify: true,
                                sourceMap: true,
                            });

                            if (code) {
                                const minifiedTarget = path.join(
                                    dest,
                                    path.parse(target).name + ".min.css"
                                );
                                fs.outputFile(minifiedTarget, code);
                                console.log(
                                    `Minified (${chalk.gray(
                                        "from"
                                    )}, ${chalk.cyan("to")})`,
                                    chalk.gray(normalTarget),
                                    chalk.cyan(minifiedTarget)
                                );
                            }

                            if (map) {
                                const mappedTarget = path.join(
                                    dest,
                                    path.parse(target).name + ".css.map"
                                );
                                fs.outputFile(mappedTarget, map);
                                console.log(
                                    `Mapped (${chalk.gray(
                                        "from"
                                    )}, ${chalk.cyan("to")})`,
                                    chalk.gray(normalTarget),
                                    chalk.cyan(mappedTarget)
                                );
                            }
                        }
                    } else {
                        console.log(
                            "Trying to transpile a SASS file with an extension that is not allowed."
                        );
                    }
                }
            });
        }
    });
});
