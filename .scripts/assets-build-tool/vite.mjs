import path from "node:path";
import { fileURLToPath } from "node:url";
import * as vite from "vite";
import JSON5 from "json5";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

async function runVite(command, assetConfig) {
    await vite.build({
        build: {
            lib: {
                entry: [assetConfig.source],
                name: "MyLib",
                // the proper extensions will be added
                fileName: "my-lib",
            },
            rollupOptions: {
                // make sure to externalize deps that shouldn't be bundled
                // into your library
                external: ["vue"],
                output: {
                    // Provide global variables to use in the UMD build
                    // for externalized deps
                    globals: {
                        vue: "Vue",
                    },
                },
            },
        },
    });
}

// run the process
const action = process.argv[2];
const config = JSON5.parse(
    Buffer.from(process.argv[3], "base64").toString("utf-8")
);

await runVite(action, config);
