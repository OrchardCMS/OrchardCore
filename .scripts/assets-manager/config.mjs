import path from "path";
import chalk from "chalk";

let userProvidedConfig = await import(path.join("file://", process.cwd(), "build.config.mjs"));

export default function getConfig(key) {
    switch (key) {
        case "vite":
            if (typeof userProvidedConfig?.vite !== "function") {
                console.log(chalk.yellow("build.config.mjs did not provide the viteConfig function. Using defaults."));
                return () => ({});
            }
            return userProvidedConfig.vite;
        case "parcel":
            if (typeof userProvidedConfig?.parcel !== "function") {
                console.log(chalk.yellow("build.config.mjs did not provide the parcelConfig function. Using defaults."));
                return () => ({});
            }
            return userProvidedConfig.parcel;
        case "assetsLookupGlob":
            if (!userProvidedConfig?.assetsLookupGlob) {
                console.log(chalk.yellow("build.config.mjs did not provide the assetsLookupGlob string. Using default of 'src/{Modules,Themes}/*/Assets.json'"));
                return "src/{Modules,Themes}/*/Assets.json";
            }
            return userProvidedConfig.assetsLookupGlob;
        case "parcelBundleOutput":
            if (!userProvidedConfig?.parcelBundleOutput) {
                throw "build.config.mjs did not provide the parcelDistDirForBundle but parcel bundles are present. Please add the parcelBundleOutput constant in build.config.mjs";
            }
            return userProvidedConfig?.parcelBundleOutput;
    }
    console.log(chalk.yellow("Key not found in build.config.mjs"), key);
    return;
}
