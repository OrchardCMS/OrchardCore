import path from "path";
import fs from "fs-extra";
import JSON5 from "json5";
import { Parcel } from "@parcel/core";
import { fileURLToPath } from "url";
import chalk from "chalk";
import _ from "lodash";
import buildConfig from "./config.mjs";
import { Buffer } from "buffer";
import process from "node:process";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const parcelConfig = path.join(__dirname, ".parcelrc");
const action = process.argv[2];
const config = JSON5.parse(Buffer.from(process.argv[3], "base64").toString("utf-8"));
const isWatching = action === "watch";
const isHosting = action === "host";

const hashCode = (str) => {
    let hash = 0,
        i,
        chr;

    if (str.length === 0) return hash;

    for (i = 0; i < str.length; i++) {
        chr = str.charCodeAt(i);
        hash = (hash << 5) - hash + chr;
        hash |= 0; // Convert to 32bit integer
    }

    return hash < 0 ? -hash : hash; // Convert to positive number
};

/**
 * Runs parcel with the given command and assetConfig.
 * @param {string} command - The command to run (e.g. build, watch, host)
 * @param {object} assetConfig - The asset configuration to use
 * @returns {Promise<void>}
 */
async function runParcel(command, assetConfig) {
    //console.log(`parcel ${command}`, assetConfig);

    if (assetConfig.source.length === 0) {
        console.error("No source provided for", assetConfig);
        return;
    }

    let options = buildParcelOptions(command, assetConfig);

    let parcel = new Parcel({
        entries: assetConfig.source,
        config: parcelConfig,
        shouldPatchConsole: true,
        ...options,
    });

    await fs.remove(assetConfig.dest); // clean the destination folder

    if (isWatching || isHosting) {
        const parcelCacheFolder = options.cacheDir;
        fs.rmSync(parcelCacheFolder, { recursive: true, force: true });

        console.log(chalk.green("Deleted folder:"), chalk.gray(parcelCacheFolder));

        const { unsubscribe } = await parcel.watch((err) => {
            if (err) {
                throw err;
            }
            // if (event.type === "buildSuccess") {
            //     console.log(
            //         `✨ Built ${assetConfig.source} in ${event.buildTime}ms!`
            //     );
            // }
        });

        process.on("SIGINT", () => {
            console.log(chalk.bold.yellow("Parcel is shutting down... "));
            unsubscribe().finally(() => process.exit(130));
        });
        process.on("SIGTERM", () => {
            console.log(chalk.bold.yellow("Parcel is shutting down... "));
            unsubscribe().finally(() => process.exit(143));
        });
    } else {
        try {
            await parcel.run();
        } catch (err) {
            console.log(err.diagnostics);
            process.exit(1);
        }
    }
}

/**
 * Builds and returns the Parcel options object based on the provided command and asset configuration.
 *
 * @param {string} command - The command to execute, which can be either "build" or "watch".
 * @param {Object} assetConfig - The asset configuration object that contains settings for building or watching assets.
 * @returns {Object} - The Parcel options object tailored for the specified command and asset configuration.
 */

function buildParcelOptions(command, assetConfig) {
    let nodeEnv;
    if (command === "build") {
        nodeEnv = process.env.NODE_ENV || "production";
    } else {
        // watch
        nodeEnv = process.env.NODE_ENV || "development";
    }

    // Set process.env.NODE_ENV to a default if undefined so that it is
    // available in JS configs and plugins.
    process.env.NODE_ENV = nodeEnv;

    let mode = command === "build" ? "production" : "development";
    let defaultOptions = {
        shouldDisableCache: mode === "production",
        cacheDir: path.join(".parcel-cache", assetConfig.name + "-" + hashCode(JSON.stringify(assetConfig))),
        mode,
        shouldContentHash: mode === "production",
        serveOptions: isHosting ? { port: 3000 } : false,
        hmrOptions: isHosting ? { port: 3000 } : false,
        shouldAutoInstall: false,
        logLevel: null,
        shouldProfile: false,
        shouldBuildLazily: false,
        detailedReport: null,
        targets: {
            default: {
                context: "browser",
                scopeHoist: true,
                optimize: mode === "production",
                distDir: assetConfig.dest,
                engines: {
                    browsers: "> 1%, last 2 versions, not dead",
                },
                outputFormat: "global",
                sourceMap: mode === "production",
            },
        },
        env: {
            NODE_ENV: nodeEnv,
        },
        additionalReporters: [
            {
                packageName: "@parcel/reporter-cli",
                resolveFrom: __filename,
            },
        ],
    };

    return _.merge(defaultOptions, buildConfig("parcel")(action, assetConfig, defaultOptions), assetConfig.options);
}

/**
 * Processes JavaScript files in the specified asset configuration destination,
 * removing source mapping URL comments and creating corresponding production
 * JavaScript files with the ".prod.js" extension.
 *
 * @param {Object} assetConfig - The asset configuration object.
 * @param {string} assetConfig.dest - The directory containing JavaScript files to process.
 */

const createProdJsFile = async (assetConfig) => {
    const files = await fs.readdir(assetConfig.dest);

    for (const file of files) {
        const filePath = path.join(assetConfig.dest, file);
        const stats = await fs.stat(filePath);

        if (stats.isFile()) {
            const jsFile = filePath;

            if (path.extname(jsFile) === ".js") {
                const fileContent = await fs.readFile(filePath, "utf8");
                const lines = fileContent.split(/\r?\n/);

                lines.forEach((line, index) => {
                    if (line.startsWith("//# sourceMappingURL=")) {
                        lines.splice(index, 1);
                    }
                });

                const newContent = lines.join("\n");
                const prodFilePath = filePath.replace(/\.js$/, ".prod.js");
                await fs.writeFile(prodFilePath, newContent);
            }
        }
    }
};

/**
 * Normalizes line endings in source map files within the specified asset configuration destination.
 *
 * This function reads all files in the provided destination directory and checks for files
 * with a ".map" extension. It then reads the content of each source map file and replaces
 * any line ending characters (such as "\r\n" or "\n") with a consistent "\n" newline character
 * to ensure uniformity.
 *
 * @param {Object} assetConfig - The asset configuration object.
 * @param {string} assetConfig.dest - The directory containing files to process.
 */

const normalizeSourceMap = async (assetConfig) => {
    const files = await fs.readdir(assetConfig.dest);

    for (const file of files) {
        const filePath = path.join(assetConfig.dest, file);
        const stats = await fs.stat(filePath);

        if (stats.isFile()) {
            if (path.extname(filePath) === ".map") {
                const fileContent = await fs.readFile(filePath, "utf8");
                await fs.writeFile(filePath, fileContent.replace(/(?:\\[rn])+/g, "\\n"));
            }
        }
    }
};

await runParcel(action, config);

if (action === "build") {
    await createProdJsFile(config);
    await normalizeSourceMap(config);
    process.exit(0);
}
