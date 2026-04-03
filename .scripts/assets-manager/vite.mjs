import { build, createServer } from "vite";
import JSON5 from "json5";
import { Buffer } from "buffer";
import process from "node:process";
import { minifyPlugin } from "./plugins/vite-plugin-minify.mjs";

async function runVite(command, assetConfig) {
    if (command === "build") {
        await build({
            root: assetConfig.source,
            plugins: [minifyPlugin()],
        });
    } else if (command === "watch") {
        await build({
            root: assetConfig.source,
            plugins: [minifyPlugin()],
            build: { watch: {} },
        });
    } else if (command === "host") {
        // Could be changed to "serve" command
        const server = await createServer({
            root: assetConfig.source,
        });

        await server.listen();

        server.printUrls();
        server.bindCLIShortcuts({ print: true });
    }
}

// run the process
const action = process.argv[2];
const encodedGroup = process.argv[3] ?? process.env.ASSETS_MANAGER_ENCODED_GROUP;
if (!encodedGroup) {
    console.error("Missing encoded group config.");
    process.exit(1);
}
const config = JSON5.parse(Buffer.from(encodedGroup, "base64").toString("utf-8"));

await runVite(action, config);
