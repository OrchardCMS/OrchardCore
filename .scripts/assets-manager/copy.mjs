import fs from "fs-extra";
import { glob } from "glob";
import JSON5 from "json5";
import chalk from "chalk";
import path from "path";

let action = process.argv[2];
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

// console.log(`copy ${action}`, config);

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
            fs.stat(file).then((stat) => {
                if (!stat.isDirectory()) {
                    fs.copy(file, target)
                        .then(() => console.log(`Copied (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(file), chalk.cyan(target)))
                        .catch((err) => {
                            console.log(`${chalk.red("Error copying")} (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(file), chalk.cyan(target), chalk.red(err));
                            throw err;
                        });
                }
            });
        }
    });
});
