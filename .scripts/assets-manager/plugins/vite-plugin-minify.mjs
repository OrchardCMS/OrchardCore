import { transform as esbuildTransform } from "esbuild";
import { transform as lightningTransform } from "lightningcss";
import { Buffer } from "buffer";
import fs from "fs";
import path from "path";

/**
 * Vite plugin that minifies output and generates files following the
 * Orchard Core asset-manager convention:
 *
 *   file.js      — minified WITH sourceMappingURL reference
 *   file.min.js  — minified WITHOUT sourceMappingURL reference
 *   file.map     — source map
 *
 *   file.css     — minified WITH sourceMappingURL reference
 *   file.min.css — minified WITHOUT sourceMappingURL reference
 *   file.css.map — source map
 */
export function minifyPlugin() {
    let outDir = "";

    return {
        name: "orchard-minify",
        apply: "build",
        configResolved(config) {
            outDir = config.build.outDir;
        },
        async writeBundle(_options, bundle) {
            for (const [fileName, chunk] of Object.entries(bundle)) {
                const filePath = path.resolve(outDir, fileName);

                if (fileName.endsWith(".js") && chunk.type === "chunk") {
                    const parsed = path.parse(filePath);

                    const result = await esbuildTransform(chunk.code, {
                        minify: true,
                        sourcemap: true,
                        sourcefile: path.basename(filePath),
                    });

                    const mapFileName = `${parsed.name}.map`;
                    const minPath = path.join(parsed.dir, `${parsed.name}.min.js`);

                    // .js — minified with sourcemap reference
                    fs.writeFileSync(filePath, `${result.code}//# sourceMappingURL=${mapFileName}\n`);
                    // .min.js — minified without sourcemap reference
                    fs.writeFileSync(minPath, result.code);
                    // .map — source map
                    fs.writeFileSync(path.join(parsed.dir, mapFileName), result.map);
                }

                if (fileName.endsWith(".css") && chunk.type === "asset" && typeof chunk.source === "string") {
                    const parsed = path.parse(filePath);

                    const { code, map } = lightningTransform({
                        code: Buffer.from(chunk.source, "utf-8"),
                        minify: true,
                        sourceMap: true,
                        filename: path.basename(filePath),
                    });

                    const mapFileName = `${parsed.name}.css.map`;
                    const minPath = path.join(parsed.dir, `${parsed.name}.min.css`);
                    const minified = code.toString();

                    // .css — minified with sourcemap reference
                    fs.writeFileSync(filePath, `${minified}\n/*# sourceMappingURL=${mapFileName} */\n`);
                    // .min.css — minified without sourcemap reference
                    fs.writeFileSync(minPath, minified);
                    // .css.map — source map
                    fs.writeFileSync(path.join(parsed.dir, mapFileName), JSON.stringify(map));
                }
            }
        },
    };
}
