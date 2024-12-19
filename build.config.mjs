export const assetsLookupGlob = "src/{OrchardCore.Themes,OrchardCore.Modules}/**/Assets.json";
export const parcelBundleOutput = "src/OrchardCore.Modules/OrchardCore.Resources/wwwroot/Scripts/bundle"

export function parcel() {
  return {
    targets: {
      default: {
        engines: {
          browsers: "> 1%, last 4 versions, not dead",
        },
      },
    },
  };
}