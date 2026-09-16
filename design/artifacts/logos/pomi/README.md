# Pomi terminal logo

Approved direction: terminal prompt `>_` with a leaf, paired with the lowercase Pomi wordmark.

## Choose an asset

| Filename suffix | Intended background | Symbol | Wordmark |
| --- | --- | --- | --- |
| `light` | White or light | Forest green `#007A45` | Charcoal `#101719` |
| `dark` | Dark | Mint green `#63D99B` | White `#FFFFFF` |
| `black` | Light, single-color reproduction | Black | Black |
| `white` | Dark, single-color reproduction | White | White |

`light` and `dark` describe the background the asset is intended to sit on. All files in `svg/` and `png/` have transparent backgrounds.

- `svg/`: eight scalable vector assets, four logo versions and four icon versions. Shapes and lettering are paths; no external fonts or embedded raster images are required.
- `png/`: matching transparent PNG assets. Logos are 1600 pixels wide; icons are 1024 × 1024 pixels. The light and dark icons also have 32, 64, 128, and 256 pixel square exports.
- `preview.png`: visual comparison. Rows are light, dark, black, and white; columns are logo and icon.
- `pomi-terminal-logo.png` and `pomi-terminal-icon.png`: original white-background images retained for quick sharing.
- `source/`: transparent raster master and generation prompt.
- `export-vectors.cjs`: reproducible tracing and export script. It uses Node.js with the `sharp` package. Set `SHARP_MODULE` to the package path if it is not installed on the usual module search path.

Use SVG for websites, documentation, slides, and other scalable layouts. Use PNG when an application does not accept SVG. Use the icon alone at small sizes, where the wordmark would be difficult to read. Preserve aspect ratio and the supplied clear space. The white variant can appear empty in viewers that display transparency on white.

## Production notes

The built-in image generation tool removed the background from the approved raster logo. Its silhouette was traced to vector paths and normalized to flat brand colors. The wordmark and emblem use the same traced geometry in every color variant. The icon is derived from that same emblem for consistency. This is a vector tracing of the approved artwork, not original typographic source artwork.

Every transparent PNG was checked for both fully transparent and fully opaque pixels, and the exports were visually checked on light and dark backgrounds.
