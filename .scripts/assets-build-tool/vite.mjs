import { build, createServer } from "vite";
import JSON5 from "json5";

async function runVite(command, assetConfig) {
  if (command === "build") {
    await build({
      root: assetConfig.source,
    });
  } else if (command === "watch") {
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
const config = JSON5.parse(Buffer.from(process.argv[3], "base64").toString("utf-8"));

await runVite(action, config);
