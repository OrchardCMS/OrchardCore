import path from "path";
import fs from "fs-extra";
import JSON5 from "json5";
import { Parcel } from "@parcel/core";
import { fileURLToPath } from "url";
import chalk from "chalk";
import _ from "lodash";
import buildConfig from "./config.mjs";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const parcelConfig = path.join(__dirname, ".parcelrc");
const action = process.argv[2];
const config = JSON5.parse(Buffer.from(process.argv[3], "base64").toString("utf-8"));
const isWatching = action === "watch";
const isHosting = action === "host";

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

    fs.remove(assetConfig.dest); // clean the destination folder

    if (isWatching || isHosting) {
        const parcelCacheFolder = options.cacheDir;
        fs.rmSync(parcelCacheFolder, { recursive: true, force: true });

        console.log(chalk.green("Deleted folder:"), chalk.gray(parcelCacheFolder));

        const { unsubscribe } = await parcel.watch((err, event) => {
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
        process.exit(0);
    }
}

// Builds the options to pass to the parcel constructor.
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
                sourceMap: mode === "development",
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

// run the process
await runParcel(action, config);

function hashCode(str) {
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
}
