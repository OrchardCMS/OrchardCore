var glob = require("glob"),
    path = require("path-posix"),
    merge = require("merge-stream"),
    gulpif = require("gulp-if"),
    gulp = require("gulp"),
    newer = require("gulp-newer"),
    plumber = require("gulp-plumber"),
    sourcemaps = require("gulp-sourcemaps"),
    less = require("gulp-less"),
    scss = require("gulp-sass"),
    autoprefixer = require("gulp-autoprefixer"),
    minify = require("gulp-minify-css"),
    typescript = require("gulp-typescript"),
    uglify = require("gulp-uglify"),
    rename = require("gulp-rename"),
    concat = require("gulp-concat"),
    header = require("gulp-header"),
    debug = require("gulp-debug"),
    fs = require("fs");

/*
** GULP TASKS
*/

// Incremental build (each asset group is built only if one or more inputs are newer than the output).
gulp.task("build", function () {
    var assetGroupTasks = getAssetGroups().map(function (assetGroup) {
        var doRebuild = false;
        return createAssetGroupTask(assetGroup, doRebuild);
    });
    return merge(assetGroupTasks);
});

// Full rebuild (all assets groups are built regardless of timestamps).
gulp.task("rebuild", function () {
    var assetGroupTasks = getAssetGroups().map(function (assetGroup) {
        var doRebuild = true;
        return createAssetGroupTask(assetGroup, doRebuild);
    });
    return merge(assetGroupTasks);
});

gulp.task("publish", function () {
    var assetGroupTasks = getAssetGroups().map(function (assetGroup) {
        console.log(assetGroup.outputDir);
        console.log(assetGroup.webroot);
        return gulp.src(path.join(assetGroup.outputDir, "**")).pipe(gulp.dest(assetGroup.webroot));
    });
    return assetGroupTasks;
});

// Set "Watchers" as sub-processes in order to restart the task when Assets.json changes.
gulp.task("watch", function () {
    var watchers;
    function restart() {
        if (watchers) {
            watchers.forEach(function (w) {
                w.remove();
                w.end();
            });
        }
        watchers = [];
        // Continuous watch (each asset group is built whenever one of its inputs changes).
        getAssetGroups().forEach(function (assetGroup) {
            var watchPaths = assetGroup.inputPaths.concat(assetGroup.watchPaths);
            var watcher = gulp.watch(watchPaths, function (event) {
                var isConcat = path.basename(assetGroup.outputFileName, path.extname(assetGroup.outputFileName)) !== "@";
                if (isConcat)
                    console.log("Asset file '" + event.path + "' was " + event.type + ", rebuilding asset group with output '" + assetGroup.outputPath + "'.");
                else
                    console.log("Asset file '" + event.path + "' was " + event.type + ", rebuilding asset group.");
                var doRebuild = true;
                var task = createAssetGroupTask(assetGroup, doRebuild);
            });
            watchers.push(watcher);
        });
    }
    var p;
    if (p) { p.exit(); }
    p = gulp.watch("./src/Orchard.Web/{Core,Modules,Themes}/*/Assets.json", function (event) {
        console.log("Asset file '" + event.path + "' was " + event.type + ", resetting asset watchers.");
        restart();
    });
    restart();
});


/*
** ASSET GROUPS
*/
function getAssetGroups() {
    var assetManifestPaths = glob.sync("./src/Orchard.Web/{Core,Modules,Themes}/*/Assets.json", {});
    var assetGroups = [];
    assetManifestPaths.forEach(function (assetManifestPath) {
        var file = './' + assetManifestPath;
        var json = fs.readFileSync(file, 'utf8');
        assetManifest = eval(json);
        assetManifest.forEach(function (assetGroup) {
            resolveAssetGroupPaths(assetGroup, assetManifestPath);
            assetGroups.push(assetGroup);
        });
    });
    return assetGroups;
}

function resolveAssetGroupPaths(assetGroup, assetManifestPath) {
    assetGroup.basePath = path.dirname(assetManifestPath);
    assetGroup.inputPaths = assetGroup.inputs.map(function (inputPath) {
        return path.resolve(path.join(assetGroup.basePath, inputPath));
    });
    assetGroup.watchPaths = [];
    if (!!assetGroup.watch) {
        assetGroup.watchPaths = assetGroup.watch.map(function (watchPath) {
            return path.resolve(path.join(assetGroup.basePath, watchPath));
        });
    }  
    assetGroup.outputPath = path.resolve(path.join(assetGroup.basePath, assetGroup.output));
    assetGroup.outputDir = path.dirname(assetGroup.outputPath);
    assetGroup.outputFileName = path.basename(assetGroup.output);
    assetGroup.webroot = path.join("./src/Orchard.Web/wwwroot/", path.basename(assetGroup.basePath), path.dirname(assetGroup.output));
}

function createAssetGroupTask(assetGroup, doRebuild) {
    var outputExt = path.extname(assetGroup.output).toLowerCase();
    switch (outputExt) {
        case ".css":
            return buildCssPipeline(assetGroup, doRebuild);
        case ".js":
            return buildJsPipeline(assetGroup, doRebuild);
    }
}

/*
** PROCESSING PIPELINES
*/

function buildCssPipeline(assetGroup, doRebuild) {
    assetGroup.inputPaths.forEach(function (inputPath) {
        var ext = path.extname(inputPath).toLowerCase();
        if (ext !== ".scss" && ext !== ".less" && ext !== ".css")
            throw "Input file '" + inputPath + "' is not of a valid type for output file '" + assetGroup.outputPath + "'.";
    });
    var doConcat = path.basename(assetGroup.outputFileName, ".css") !== "@";
    var generateSourceMaps = assetGroup.hasOwnProperty("generateSourceMaps") ? assetGroup.generateSourceMaps : true;
    return gulp.src(assetGroup.inputPaths)
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
        .pipe(gulpif("*.scss", scss()))
        .pipe(gulpif(doConcat, concat(assetGroup.outputFileName)))
        .pipe(autoprefixer({ browsers: ["last 2 versions"] }))
        // TODO: Start using below whenever gulp-header supports sourcemaps.
        //.pipe(header(
        //    "/*\n" +
        //    "** NOTE: This file is generated by Gulp compilation and should not be edited directly!\n" +
        //    "** Any changes made directly to this file will be overwritten next time the Gulp compilation runs.\n" +
        //    "** For more information, see the Readme.txt file in the Gulp solution folder.\n" +
        //    "*/\n\n"))
        .pipe(gulpif(generateSourceMaps, sourcemaps.write()))
        .pipe(gulp.dest(assetGroup.outputDir))
        .pipe(gulp.dest(assetGroup.webroot))
        .pipe(minify())
        .pipe(rename({
            suffix: ".min"
        }))
        .pipe(gulp.dest(assetGroup.outputDir))
        .pipe(gulp.dest(assetGroup.webroot))
        ;
    ;
}

function buildJsPipeline(assetGroup, doRebuild) {
    assetGroup.inputPaths.forEach(function (inputPath) {
        var ext = path.extname(inputPath).toLowerCase();
        if (ext !== ".ts" && ext !== ".js")
            throw "Input file '" + inputPath + "' is not of a valid type for output file '" + assetGroup.outputPath + "'.";
    });
    var doConcat = path.basename(assetGroup.outputFileName, ".js") !== "@";
    var generateSourceMaps = assetGroup.hasOwnProperty("generateSourceMaps") ? assetGroup.generateSourceMaps : true;
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
        .pipe(gulpif("*.ts", typescript({
            declaration: false,
            //noImplicitAny: true,
            noEmitOnError: true,
            sortOutput: true,
        }).js))
	.pipe(gulpif(doConcat, concat(assetGroup.outputFileName)))
        // TODO: Start using below whenever gulp-header supports sourcemaps.
        //.pipe(header(
        //    "/*\n" +
        //    "** NOTE: This file is generated by Gulp compilation and should not be edited directly!\n" +
        //    "** Any changes made directly to this file will be overwritten next time the Gulp compilation runs.\n" +
        //    "** For more information, see the Readme.txt file in the Gulp solution folder.\n" +
        //    "*/\n\n"))
        .pipe(gulpif(generateSourceMaps, sourcemaps.write()))
        .pipe(gulp.dest(assetGroup.outputDir))
        .pipe(gulp.dest(assetGroup.webroot))
	.pipe(uglify())
	.pipe(rename({
	    suffix: ".min"
	}))
	.pipe(gulp.dest(assetGroup.outputDir))
    .pipe(gulp.dest(assetGroup.webroot));
}
