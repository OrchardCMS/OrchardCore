import { glob } from 'glob'
import JSON5 from "json5";
import fs from "fs-extra";
import path from "path";

import buildConfig from "./config.mjs";

// Gets an object representation of all Assets.json in the solution.
export default function getAllAssetGroups() {
  var assetGroups = [];
  getAssetsJsonPaths().forEach(function (assetManifestPath) {
    var assetManifest = JSON5.parse(fs.readFileSync(assetManifestPath, "utf-8"));
    assetManifest.forEach(function (assetGroup) {
      resolveAssetGroupPaths(assetGroup, assetManifestPath);
      assetGroups.push(assetGroup);
    });
  });
  return assetGroups;
}

// Expands the paths to full paths.
function resolveAssetGroupPaths(assetGroup, assetManifestPath) {
  assetGroup.manifestPath = assetManifestPath;
  assetGroup.basePath = path.dirname(assetManifestPath);
  if (assetGroup.source) {
    assetGroup.originalSource = assetGroup.source;
    // since node_modules all resolve at the root (because of yarn)
    if (assetGroup.source.startsWith("node_modules")) {
      assetGroup.source = path.resolve(path.join(process.cwd(), assetGroup.source)).replace(/\\/g, "/");
    } else {
      assetGroup.source = path.resolve(path.join(assetGroup.basePath, assetGroup.source)).replace(/\\/g, "/");
    }
    // The source path relative to the root of the solution.
    assetGroup.relativeSource = assetGroup.source.substring(process.cwd().length);

  }
  if (assetGroup.dest) {
    assetGroup.dest = path.resolve(path.join(assetGroup.basePath, assetGroup.dest)).replace(/\\/g, "/");
  }
}

function getAssetsJsonPaths() {
  return glob.sync(buildConfig("assetsLookupGlob"), {});
}
