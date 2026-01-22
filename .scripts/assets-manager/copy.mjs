import fs from "fs-extra";
import { glob } from "glob";
import JSON5 from "json5";
import chalk from "chalk";
import path from "path";
import { Buffer } from "buffer";
import process from "node:process";

let action = process.argv[2];
const encodedGroup = process.argv[3] ?? process.env.ASSETS_MANAGER_ENCODED_GROUP;
if (!encodedGroup) {
    console.error("Missing encoded group config.");
    process.exit(1);
}
const config = JSON5.parse(Buffer.from(encodedGroup, "base64").toString("utf-8"));

// Normalize source to always be an array for processing
const sources = Array.isArray(config.source) ? config.source : [config.source];

// Determine default destination if not specified
if (config.dest == undefined) {
    // Use the first source to determine file extension
    const firstSource = sources[0];
    const fileExtension = firstSource.split(".").pop();
    
    if (config.tags.includes("js") || fileExtension == "js") {
        config.dest = config.basePath + "/wwwroot/Scripts/";
    } else if (config.tags.includes("css") || fileExtension == "css") {
        config.dest = config.basePath + "/wwwroot/Styles/";
    }
}

if (config.dryRun) {
    action = "dry-run";
}

// Process each source pattern
async function processSources() {
    let allFilesProcessed = 0;
    
    for (const sourcePattern of sources) {
        const files = await glob(sourcePattern);
        
        if (files.length == 0) {
            console.log(chalk.yellow("No files to copy for pattern:", sourcePattern));
            continue;
        }

        const destExists = fs.existsSync(config.dest);

        if (destExists) {
            const stats = fs.lstatSync(config.dest);
            if (!stats.isDirectory()) {
                console.log(chalk.red("Destination is not a directory"));
                console.log("Pattern:", sourcePattern);
                console.log("Files:", files);
                console.log("Destination:", config.dest);
                continue;
            }
        }

        if (allFilesProcessed === 0 && destExists) {
            console.log(chalk.yellow(`Destination ${config.dest} already exists, files may be overwritten`));
        }

        let baseFolder;

        if (sourcePattern.indexOf("**") > 0) {
            baseFolder = sourcePattern.substring(0, sourcePattern.indexOf("**"));
        }

        for (const file of files) {
            const normalizedFile = file.replace(/\\/g, "/");

            let relativePath;
            if (baseFolder) {
                relativePath = normalizedFile.replace(baseFolder, "");
            } else {
                relativePath = path.basename(normalizedFile);
            }

            const target = path.join(config.dest, relativePath);

            if (action === "dry-run") {
                console.log(`Dry run (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(normalizedFile), chalk.cyan(target));
            } else {
                try {
                    const stat = await fs.stat(normalizedFile);
                    if (!stat.isDirectory()) {
                        await fs.copy(normalizedFile, target);
                        console.log(`Copied (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(normalizedFile), chalk.cyan(target));
                        allFilesProcessed++;
                    }
                } catch (err) {
                    console.log(`${chalk.red("Error copying")} (${chalk.gray("from")}, ${chalk.cyan("to")})`, chalk.gray(normalizedFile), chalk.cyan(target), chalk.red(err));
                    throw err;
                }
            }
        }
    }
    
    if (allFilesProcessed > 0 || action === "dry-run") {
        console.log(chalk.green(`Processed ${allFilesProcessed} file(s) from ${sources.length} source pattern(s)`));
    }
}

processSources().catch(err => {
    console.error(chalk.red("Copy failed:"), err);
    process.exit(1);
});
