import chalk from "chalk";
import path from "path";
import fs from "fs-extra";
import process from "node:process";

export default async function clean(groups) {
    console.log(chalk.redBright("Clean task called. This wipes all folders referenced as destinations of all groups. Waiting 3 seconds before starting."));
    await new Promise((resolve) => setTimeout(resolve, 3000));
    try {
        const promises = [];
        for (const g of groups) {
            if (g.dest) {
                if (typeof g.dest !== "string" || !(g.dest.includes("wwwroot") || g.dest.includes("dist"))) {
                    console.log(chalk.red("We only support deleting folders that contain wwwroot or dist"), chalk.gray(g.dest));
                    throw chalk.red("Error cleaning: ") + chalk.gray(g.dest);
                }

                promises.push(
                    await fs
                        .rm(g.dest, { recursive: true, force: true })
                        .then(() => console.log(chalk.green("Deleted folder:"), chalk.gray(g.dest)))
                        .catch((err) => {
                            console.log(chalk.red("Error deleting folder:"), chalk.gray(g.dest));
                            throw err;
                        }),
                );
            } else {
                const scriptFolder = path.resolve(g.basePath + "\\wwwroot\\Scripts");
                const styleFolder = path.resolve(g.basePath + "\\wwwroot\\Styles");

                console.log(scriptFolder, styleFolder);
                if (fs.exists(scriptFolder)) {
                    await fs
                        .rm(scriptFolder, { recursive: true, force: true })
                        .then(() => console.log(chalk.green("Deleted folder:"), chalk.gray(scriptFolder)))
                        .catch((err) => {
                            console.log(chalk.red("Error deleting folder:"), chalk.gray(scriptFolder));
                            throw err;
                        });
                }

                if (fs.exists(styleFolder)) {
                    await fs
                        .rm(styleFolder, { recursive: true, force: true })
                        .then(() => console.log(chalk.green("Deleted folder:"), chalk.gray(styleFolder)))
                        .catch((err) => {
                            console.log(chalk.red("Error deleting folder:"), chalk.gray(styleFolder));
                            throw err;
                        });
                }
            }
        }
        const parcelCache = path.join(process.cwd(), ".parcel-cache");
        promises.push(
            await fs
                .rm(parcelCache, { recursive: true, force: true })
                .then(() => console.log(chalk.green("Deleted folder:"), chalk.gray(parcelCache)))
                .catch((err) => {
                    console.log(chalk.red("Error deleting folder:"), chalk.gray(parcelCache));
                    throw err;
                }),
        );
        await Promise.all(promises);
        console.log(chalk.green("Cleaned successfully! exiting..."));
        return 0;
    } catch (e) {
        console.log(chalk.red("Error: "), e);
        return 1;
    }
}
