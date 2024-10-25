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

// For compat with older versions of Node.js.
require("es6-promise").polyfill();

// To suppress memory leak warning from gulp.watch().
require("events").EventEmitter.prototype._maxListeners = 100;

/*
** GULP TASKS
*/

// Incremental build (each asset group is built only if one or more inputs are newer than the output).
gulp.task("build-assets", function () {
    var assetGroupTasks = getAssetGroups().map(function (assetGroup) {
        var doRebuild = false;
        return createAssetGroupTask(assetGroup, doRebuild);
    });
    return merge(assetGroupTasks);
});

// Full rebuild (all assets groups are built regardless of timestamps).
gulp.task("rebuild-assets", function () {
    var assetGroupTasks = getAssetGroups().map(function (assetGroup) {
        var doRebuild = true;
        return createAssetGroupTask(assetGroup, doRebuild);
    });
    return merge(assetGroupTasks);
});

// Continuous watch (each asset group is built whenever one of its inputs changes).
gulp.task("watch", function () {
    getAssetGroups().forEach(function (assetGroup) {
        var watchPaths = assetGroup.inputPaths.concat(assetGroup.watchPaths);
        var inputWatcher;
        function createWatcher() {
            inputWatcher = gulp.watch(watchPaths);
            inputWatcher.on('change', function (watchedPath) {
                var isConcat = path.basename(assetGroup.outputFileName, path.extname(assetGroup.outputFileName)) !== "@";
                if (isConcat)
                    console.log("Asset file '" + watchedPath + "' was changed, rebuilding asset group with output '" + assetGroup.outputPath + "'.");
                else
                    console.log("Asset file '" + watchedPath + "' was changed, rebuilding asset group.");
                var doRebuild = true;
                createAssetGroupTask(assetGroup, doRebuild);
            });
        }

        createWatcher();

        gulp.watch(assetGroup.manifestPath).on('change', function (watchedPath) {
            console.log("Asset manifest file '" + watchedPath + "' was changed, restarting watcher.");
            inputWatcher.close();
            createWatcher();
        });
    });
});

gulp.task('help', function () {
    util.log(`
  Usage: gulp [TASK]
  Tasks:
      build     Incremental build (each asset group is built only if one or more inputs are newer than the output).
      rebuild   Full rebuild (all assets groups are built regardless of timestamps).
      watch     Continuous watch (each asset group is built whenever one of its inputs changes).
    `);
});

gulp.task('build', gulp.series(['build-assets']));
gulp.task('rebuild', gulp.series(['rebuild-assets']));
gulp.task('default', gulp.series(['build']));

/*
** ASSET GROUPS
*/

function getAssetGroups() {
    var assetManifestPaths = glob.sync("./src/OrchardCore.{Modules,Themes}/*/Assets.json", {});
    var assetGroups = [];
    assetManifestPaths.forEach(function (assetManifestPath) {
        var assetManifest = require("./" + assetManifestPath);
        assetManifest.forEach(function (assetGroup) {
            resolveAssetGroupPaths(assetGroup, assetManifestPath);
            assetGroups.push(assetGroup);
        });
    });
    return assetGroups;
}

function resolveAssetGroupPaths(assetGroup, assetManifestPath) {
    assetGroup.manifestPath = assetManifestPath.replace(/\\/g, '/');
    assetGroup.basePath = path.dirname(assetGroup.manifestPath);

    var inputPaths = [];

    // The inputPaths can contain either a physical path to a file or a path with a wildcard.
    // It's crucial to maintain the order of each file based on its position in the assets.json file.
    // When a path contains a wildcard, we need to convert the wildcard to physical paths
    // and sort them independently of the previous paths to ensure consistent concatenation.
    assetGroup.inputs.forEach(inputPath => {

        var resolvedPath = path.resolve(path.join(assetGroup.basePath, inputPath)).replace(/\\/g, '/');

        if (resolvedPath.includes('*')) {
            var sortedPaths = glob.sync(resolvedPath, {});

            sortedPaths.sort();

            sortedPaths.forEach(sortedPath => {
                inputPaths.push(sortedPath.replace(/\\/g, '/'));
            });
        } else {
            inputPaths.push(resolvedPath);
        }
    });

    assetGroup.inputPaths = inputPaths;

    assetGroup.watchPaths = [];
    if (!!assetGroup.watch) {
        assetGroup.watchPaths = assetGroup.watch.map(function (watchPath) {
            return path.resolve(path.join(assetGroup.basePath, watchPath)).replace(/\\/g, '/');
        });
    }
    assetGroup.outputPath = path.resolve(path.join(assetGroup.basePath, assetGroup.output)).replace(/\\/g, '/');
    assetGroup.outputDir = path.dirname(assetGroup.outputPath);
    assetGroup.outputFileName = path.basename(assetGroup.output);
    // Uncomment to copy assets to wwwroot
    //assetGroup.webroot = path.join("./src/OrchardCore.Cms.Web/wwwroot/", path.basename(assetGroup.basePath), path.dirname(assetGroup.output));
}

function createAssetGroupTask(assetGroup, doRebuild) {
    var outputExt = path.extname(assetGroup.output).toLowerCase();
    var doConcat = path.basename(assetGroup.outputFileName, outputExt) !== "@";
    if (doConcat && !doRebuild) {
        // Force a rebuild of this asset group is the asset manifest file itself is newer than the output.
        var assetManifestStats = fs.statSync(assetGroup.manifestPath);
        var outputStats = fs.existsSync(assetGroup.outputPath) ? fs.statSync(assetGroup.outputPath) : null;
        doRebuild = !outputStats || assetManifestStats.mtime > outputStats.mtime;
    }

    if (assetGroup.copy === true) {
        return buildCopyPipeline(assetGroup, doRebuild);
    }
    else {
        switch (outputExt) {
            case ".css":
                return buildCssPipeline(assetGroup, doConcat, doRebuild);
            case ".js":
                return buildJsPipeline(assetGroup, doConcat, doRebuild);
        }
    }
}

/*
** PROCESSING PIPELINES
*/

function buildCssPipeline(assetGroup, doConcat, doRebuild) {
    assetGroup.inputPaths.forEach(function (inputPath) {
        var ext = path.extname(inputPath).toLowerCase();
        if (ext !== ".scss" && ext !== ".less" && ext !== ".css")
            throw "Input file '" + inputPath + "' is not of a valid type for output file '" + assetGroup.outputPath + "'.";
    });
    var generateSourceMaps = assetGroup.hasOwnProperty("generateSourceMaps") ? assetGroup.generateSourceMaps : false;
    var generateRTL = assetGroup.hasOwnProperty("generateRTL") ? assetGroup.generateRTL : false;
    var containsLessOrScss = assetGroup.inputPaths.some(function (inputPath) {
        var ext = path.extname(inputPath).toLowerCase();
        return ext === ".less" || ext === ".scss";
    });
    // Source maps are useless if neither concatenating nor transforming.
    if ((!doConcat || assetGroup.inputPaths.length < 2) && !containsLessOrScss)
        generateSourceMaps = false;

    var minifiedStream = gulp.src(assetGroup.inputPaths) // Minified output, source mapping completely disabled.
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
    var devStream = gulp.src(assetGroup.inputPaths) // Non-minified output, with source mapping
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
}

function buildJsPipeline(assetGroup, doConcat, doRebuild) {
    assetGroup.inputPaths.forEach(function (inputPath) {
        var ext = path.extname(inputPath).toLowerCase();
        if (ext !== ".ts" && ext !== ".js")
            throw "Input file '" + inputPath + "' is not of a valid type for output file '" + assetGroup.outputPath + "'.";
    });
    var generateSourceMaps = assetGroup.hasOwnProperty("generateSourceMaps") ? assetGroup.generateSourceMaps : false;
    // Source maps are useless if neither concatenating nor transforming.
    if ((!doConcat || assetGroup.inputPaths.length < 2) && !assetGroup.inputPaths.some(function (inputPath) { return path.extname(inputPath).toLowerCase() === ".ts"; }))
        generateSourceMaps = false;

    var tsCompilerOptions = assetGroup.hasOwnProperty("tsCompilerOptions") ? assetGroup.tsCompilerOptions : {
        declaration: false,
        noImplicitAny: true,
        noEmitOnError: true,
        lib: [
            "dom",
            "es5",
            "scripthost",
            "es2015.iterable"
        ],
        target: "es5",
    };

    return gulp.src(assetGroup.inputPaths)
        .pipe(gulpif(!doRebuild,
            gulpif(doConcat,
                newer(assetGroup.outputPath),
                newer({
                    dest: assetGroup.outputDir,
                    ext: ".js"
                }))))
        .pipe(plumber())
        .pipe(gulpif(generateSourceMaps, sourcemaps.init()))
        .pipe(gulpif("*.ts", typescript(tsCompilerOptions)))
        .pipe(babel({
            "presets": [
                [
                    "@babel/preset-env",
                    {
                        "modules": false
                    },
                    "@babel/preset-flow"
                ]
            ]
        }))
        .pipe(gulpif(doConcat, concat(assetGroup.outputFileName)))
        .pipe(header(
            "/*\n" +
            "** NOTE: This file is generated by Gulp and should not be edited directly!\n" +
            "** Any changes made directly to this file will be overwritten next time its asset group is processed by Gulp.\n" +
            "*/\n\n"))
        .pipe(gulpif(generateSourceMaps, sourcemaps.write()))
        .pipe(gulp.dest(assetGroup.outputDir))
        // Uncomment to copy assets to wwwroot
        //.pipe(gulp.dest(assetGroup.webroot))
        .pipe(terser())
        .pipe(rename({
            suffix: ".min"
        }))
        .pipe(eol('\n'))
        .pipe(gulp.dest(assetGroup.outputDir))
    // Uncomment to copy assets to wwwroot
    //.pipe(gulp.dest(assetGroup.webroot));
}

function buildCopyPipeline(assetGroup, doRebuild) {
    var stream = gulp.src(assetGroup.inputPaths);

    if (!doRebuild) {
        stream = stream.pipe(newer(assetGroup.outputDir))
    }

    var renameFile = assetGroup.outputFileName != "@";

    stream = stream
        .pipe(gulpif(renameFile, rename(assetGroup.outputFileName)))
        .pipe(gulp.dest(assetGroup.outputDir));
    // Uncomment to copy assets to wwwroot
    //.pipe(gulp.dest(assetGroup.webroot));

    return stream;
}
