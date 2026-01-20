import fs from "fs-extra";
import { glob } from "glob";
import JSON5 from "json5";
import chalk from "chalk";
import path from "path";
import swc from "@swc/core";
import { Buffer } from "buffer";
import process from "node:process";

let action = process.argv[2];
let mode = action === "build" ? "production" : "development";

const encodedGroup = process.env.ASSETS_MANAGER_ENCODED_GROUP;
if (!encodedGroup) {
    console.error("Missing encoded group config (ASSETS_MANAGER_ENCODED_GROUP).");
    process.exit(1);
}
const config = JSON5.parse(Buffer.from(encodedGroup, "base64").toString("utf-8"));

let dest = config.dest;
let output = config.output;

if (!dest && !output) {
    if (config.tags.includes("js")) {
        output = config.basePath + "/wwwroot/Scripts/" + config.name + ".js";
    } else if (config.tags.includes("css")) {
        output = config.basePath + "/wwwroot/Styles/" + config.name + ".css";
    }
} else if (!output) {
    output = path.join(dest, config.name + ".js");
}

if (config.dryRun) {
    action = "dry-run";
}

// Ensure sources is an array
const sources = Array.isArray(config.source) ? config.source : [config.source];

// Resolve all glob patterns and expand files
async function resolveFiles(patterns, basePath) {
    const allFiles = [];
    
    for (const pattern of patterns) {
        const fullPattern = path.isAbsolute(pattern) ? pattern : path.join(basePath, pattern);
        const files = await glob(fullPattern, { windowsPathsNoEscape: true });
        allFiles.push(...files);
    }
    
    return allFiles;
}

resolveFiles(sources, config.basePath).then(async (files) => {
    if (files.length === 0) {
        console.log(chalk.yellow("No files to concatenate", sources));
        return;
    }

    if (action === "dry-run") {
        console.log(`Dry run - would concatenate ${files.length} file(s) to ${chalk.cyan(output)}`);
        files.forEach(file => {
            console.log(`  ${chalk.gray(file)}`);
        });
        return;
    }

    // Read and concatenate all files
    let concatenated = "";
    for (const file of files) {
        try {
            const content = await fs.readFile(file, "utf8");
            concatenated += content + "\n\n";
            console.log(`Concatenated ${chalk.gray(file)}`);
        } catch (err) {
            console.log(chalk.red(`Error reading file ${file}:`), err.message);
            throw err;
        }
    }

    // Ensure output directory exists
    await fs.ensureDir(path.dirname(output));

    // Write non-minified version
    await fs.outputFile(output, concatenated);
    console.log(`Concatenated ${files.length} file(s) to ${chalk.cyan(output)}`);

    // Minify for production
    try {
        const minified = await swc.minify(concatenated.replace(/(\r?\n|\r)/gm, "\n"), {
            compress: true,
            mangle: true,
            sourceMap: mode === "production",
        });

        const minOutput = output.replace(/\.js$/, ".min.js");
        await fs.outputFile(minOutput, minified.code + "\n");
        console.log(`Minified to ${chalk.cyan(minOutput)}`);

        if (mode === "production" && minified.map) {
            const mapOutput = output.replace(/\.js$/, ".map");
            let normalized = minified.map.replace(/(?:\\r\\n)/g, "\\n");
            await fs.outputFile(mapOutput, normalized + "\n");
            console.log(`Source map to ${chalk.cyan(mapOutput)}`);
        }
    } catch (err) {
        console.log(chalk.red("Error minifying:"), err.message);
        throw err;
    }
}).catch(err => {
    console.error(chalk.red("Fatal error:"), err);
    process.exit(1);
});
