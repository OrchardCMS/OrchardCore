// Webpack API see: https://webpack.js.org/api/node/
import webpack from "webpack";
import JSON5 from "json5";
import WebpackDevServer from "webpack-dev-server";
import process from "node:process";
import { Buffer } from "buffer";

const action = process.argv[2];
const assetConfig = JSON5.parse(Buffer.from(process.argv[3], "base64").toString("utf-8"));
const webpackConfig = await import("../../" + assetConfig.basePath + assetConfig.config);
const compiler = webpack(webpackConfig.default);

if (action === "build") {
    // run webpack
    compiler.run(() => {
        compiler.close(() => {});
    });
} else if (action === "watch") {
    // watch webpack
    compiler.watch(
        {
            // Example
            aggregateTimeout: 300,
            poll: undefined,
        },
        (err, stats) => {
            // Print watch/build result here...
            console.log(
                stats.toString({
                    colors: true,
                    chunks: false,
                    modules: false,
                    children: false,
                    entrypoints: false,
                }),
            );
        },
    );
} else if (action === "host") {
    const devServerOptions = { ...webpackConfig.default.devServer, open: true };
    const server = new WebpackDevServer(devServerOptions, compiler);

    const runServer = async () => {
        console.log("Starting server...");
        await server.start();
    };

    runServer();
}
