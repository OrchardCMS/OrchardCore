import fs from "fs-extra";
import { glob } from "glob";
import JSON5 from "json5";
import chalk from "chalk";
import path from "path";
import swc from "@swc/core";

let action = process.argv[2];
const config = JSON5.parse(
    Buffer.from(process.argv[3], "base64").toString("utf-8")
);

let dest = config.dest;

if (config.dest == undefined) {
    if (config.tags.includes("js")) {
        dest = config.basePath + "/wwwroot/Scripts/";
    } else if (config.tags.includes("css")) {
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

                    if (fileInfo.ext === ".js" || fileInfo.ext === ".css") {
                        let reader = await fs.readFile(file, "utf8");

                        swc.minify(reader, {
                            compress: true,
                            sourceMap: true,
                        }).then((output) => {
                            const minifiedTarget = path.join(
                                dest,
                                path.parse(target).name + ".min.js"
                            );
                            fs.outputFile(minifiedTarget, output.code);
                            console.log(
                                `Minified (${chalk.gray("from")}, ${chalk.cyan(
                                    "to"
                                )})`,
                                chalk.gray(file),
                                chalk.cyan(minifiedTarget)
                            );

                            const mappedTarget = path.join(
                                dest,
                                path.parse(target).name + ".map"
                            );
                            fs.outputFile(mappedTarget, (output.map.replace(/(?:\\[rn])+/g, "\\n")) + "\n");
                            console.log(
                                `Mapped (${chalk.gray("from")}, ${chalk.cyan(
                                    "to"
                                )})`,
                                chalk.gray(file),
                                chalk.cyan(mappedTarget)
                            );
                        });

                        fs.copy(file, target)
                            .then(() =>
                                console.log(
                                    `Copied (${chalk.gray(
                                        "from"
                                    )}, ${chalk.cyan("to")})`,
                                    chalk.gray(file),
                                    chalk.cyan(target)
                                )
                            )
                            .catch((err) => {
                                console.log(
                                    `${chalk.red(
                                        "Error copying"
                                    )} (${chalk.gray("from")}, ${chalk.cyan(
                                        "to"
                                    )})`,
                                    chalk.gray(file),
                                    chalk.cyan(target),
                                    chalk.red(err)
                                );
                                throw err;
                            });
                    } else {
                        console.log(
                            "Trying to minify a file with an extension that is not allowed."
                        );
                    }
                }
            });
        }
    });
});
