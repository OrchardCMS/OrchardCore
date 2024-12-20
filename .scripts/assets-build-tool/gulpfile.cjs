var fs = require("graceful-fs"),
    glob = require("glob"),
    path = require("path-posix"),
    merge = require("merge-stream"),
    gulp = require("gulp"),
    gulpif = require("gulp-if"),
    newer = require("gulp-newer"),
    plumber = require("gulp-plumber"),
    sourcemaps = require("gulp-sourcemaps"),
    less = require("gulp-less"),
    scss = require("gulp-dart-sass"),
    minify = require("gulp-minifier"),
    typescript = require("gulp-typescript"),
    terser = require("gulp-terser"),
    rename = require("gulp-rename"),
    concat = require("gulp-concat"),
    header = require("gulp-header"),
    eol = require("gulp-eol"),
    util = require('gulp-util'),
    postcss = require('gulp-postcss'),
    rtl = require('postcss-rtl'),
    babel = require('gulp-babel');
    JSON5 = require("json5");

// For compat with older versions of Node.js.
require("es6-promise").polyfill();

// To suppress memory leak warning from gulp.watch().
require("events").EventEmitter.prototype._maxListeners = 100;

let action = process.argv[2];
const assetGroup = JSON5.parse(Buffer.from(process.argv[3], "base64").toString("utf-8"));
const doRebuild = false;
const doConcat = true;

assetGroup.inputs.forEach(function (inputPath) {
    var ext = path.extname(inputPath).toLowerCase();
    if (ext !== ".scss" && ext !== ".less" && ext !== ".css")
        throw "Input file '" + inputPath + "' is not of a valid type for output file '" + assetGroup.outputPath + "'.";
});
var generateSourceMaps = assetGroup.hasOwnProperty("generateSourceMaps") ? assetGroup.generateSourceMaps : false;
var generateRTL = assetGroup.hasOwnProperty("generateRTL") ? assetGroup.generateRTL : false;
var containsLessOrScss = assetGroup.inputs.some(function (inputPath) {
    var ext = path.extname(inputPath).toLowerCase();
    return ext === ".less" || ext === ".scss";
});
// Source maps are useless if neither concatenating nor transforming.
if ((assetGroup.inputs.length < 2) && !containsLessOrScss)
    generateSourceMaps = false;

var minifiedStream = gulp.src(assetGroup.inputs) // Minified output, source mapping completely disabled.
    .pipe(gulpif(!doRebuild,
        gulpif(doConcat,
            newer(assetGroup.outputPath),
            newer({
                dest: assetGroup.outputDir,
                ext: ".css"
            }))))
    .pipe(plumber())
    .pipe(gulpif("*.less", less()))
    .pipe(gulpif("*.scss", scss({
        precision: 10,

    })))
    .pipe(gulpif(doConcat, concat(assetGroup.outputFileName)))
    .pipe(gulpif(generateRTL, postcss([rtl()])))
    .pipe(minify({
        minify: true,
        minifyHTML: {
            collapseWhitespace: true,
            conservativeCollapse: true,
        },
        minifyJS: {
            sourceMap: true
        },
        minifyCSS: true
    }))
    .pipe(rename({
        suffix: ".min"
    }))
    .pipe(eol('\n'))
    .pipe(gulp.dest(assetGroup.outputDir));
// Uncomment to copy assets to wwwroot
//.pipe(gulp.dest(assetGroup.webroot));
var devStream = gulp.src(assetGroup.inputs) // Non-minified output, with source mapping
    .pipe(gulpif(!doRebuild,
        gulpif(doConcat,
            newer(assetGroup.outputPath),
            newer({
                dest: assetGroup.outputDir,
                ext: ".css"
            }))))
    .pipe(plumber())
    .pipe(gulpif(generateSourceMaps, sourcemaps.init()))
    .pipe(gulpif("*.less", less()))
    .pipe(gulpif("*.scss", scss({
        precision: 10
    })))
    .pipe(gulpif(doConcat, concat(assetGroup.outputFileName)))
    .pipe(gulpif(generateRTL, postcss([rtl()])))
    .pipe(gulpif(generateSourceMaps, sourcemaps.write()))
    .pipe(eol('\n'))
    .pipe(gulp.dest(assetGroup.outputDir));
// Uncomment to copy assets to wwwroot
//.pipe(gulp.dest(assetGroup.webroot));
return merge([minifiedStream, devStream]);


