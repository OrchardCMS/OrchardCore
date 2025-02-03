// Webpack API see: https://webpack.js.org/api/node/
import webpack from "webpack";
import JSON5 from "json5";

const action = process.argv[2];
const assetConfig = JSON5.parse(Buffer.from(process.argv[3], "base64").toString("utf-8"));

if (action === "build") {
  const webpackConfig = await import("../../" + assetConfig.basePath + assetConfig.config);
  const compiler = webpack(webpackConfig.default);

  // run webpack
  compiler.run((err, stats) => {
    compiler.close((closeErr) => {});
  });
}

// TODO: add watcher

// watch webpack
/* const watching = compiler.watch(
  {
    // Example
    aggregateTimeout: 300,
    poll: undefined,
  },
  (err, stats) => {
    // Print watch/build result here...
    console.log(stats);
  },
);

watching.close((closeErr) => {
  console.log("Watching Ended.");
});
 */
