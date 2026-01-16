import { glob } from "glob";
import JSON5 from "json5";
import fs from "fs-extra";
import path from "path";
import process from "node:process";

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
        
        // Handle both string and array sources
        const sources = Array.isArray(assetGroup.source) ? assetGroup.source : [assetGroup.source];
        const resolvedSources = sources.map(src => {
            // since node_modules all resolve at the root (because of yarn)
            if (src.startsWith("node_modules")) {
                return path.resolve(path.join(process.cwd(), src)).replace(/\\/g, "/");
            } else {
                return path.resolve(path.join(assetGroup.basePath, src)).replace(/\\/g, "/");
            }
        });
        
        // Keep as array if it was an array, otherwise convert back to string
        assetGroup.source = Array.isArray(assetGroup.originalSource) ? resolvedSources : resolvedSources[0];
        
        // The source path relative to the root of the solution.
        if (Array.isArray(assetGroup.source)) {
            assetGroup.relativeSource = assetGroup.source.map(src => src.substring(process.cwd().length));
        } else {
            assetGroup.relativeSource = assetGroup.source.substring(process.cwd().length);
        }
    }
    if (assetGroup.dest) {
        assetGroup.dest = path.resolve(path.join(assetGroup.basePath, assetGroup.dest)).replace(/\\/g, "/");
    }
}

function getAssetsJsonPaths() {
    return glob.sync(buildConfig("assetsLookupGlob"), {});
}
