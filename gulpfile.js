/// <binding />
var fs = require("fs"),
    glob = require("glob"),
    path = require("path-posix"),
    merge = require("merge-stream"),
    gulp = require("gulp"),
    gulpif = require("gulp-if"),
    print = require("gulp-print"),
    debug = require("gulp-debug"),
    newer = require("gulp-newer"),
    plumber = require("gulp-plumber"),
    sourcemaps = require("gulp-sourcemaps"),
    less = require("gulp-less"),
    webpack = require("webpack"),
    cssnano = require("gulp-cssnano"),
    typescript = require("gulp-typescript"),
    uglify = require("gulp-uglify"),
    rename = require("gulp-rename"),
    concat = require("gulp-concat"),
    header = require("gulp-header"),
    eol = require("gulp-eol"),
    babel = require("gulp-babel"),
    browserSync = require("browser-sync"),
    HtmlWebpackPlugin = require("html-webpack-plugin"),
    ExtractTextPlugin = require("extract-text-webpack-plugin"),
    eslint = require("gulp-eslint"),
    react = require("react");

// For compat with older versions of Node.js.
require("es6-promise").polyfill();

// To suppress memory leak warning from gulp.watch().
require("events").EventEmitter.prototype._maxListeners = 100;

/*
** GULP TASKS
*/

// lo mio
var jsFiles = {
    vendor: [],
    source: [
        "./src/OrchardCore.DistribWebAPI/modules/**/*.js",
        "./src/OrchardCore.DistribWebAPI/modules/**/*.jsx"
    ]
};
// Lint JS/JSX files
gulp.task("eslint", function() {
    return gulp
        .src(jsFiles.source)
        .pipe(
            eslint({
                baseConfig: {
                    ecmaFeatures: {
                        jsx: true
                    }
                }
            })
        )
        .pipe(eslint.format())
        .pipe(eslint.failAfterError());
});
// Copy react.js and react-dom.js to assets/js/src/vendor
// only if the copy in node_modules is "newer"
gulp.task("copy-react", function() {
    return gulp
        .src("node_modules/react/dist/react.js")
        .pipe(newer("assets/js/src/vendor/react.js"))
        .pipe(gulp.dest("assets/js/src/vendor"));
});
gulp.task("copy-react-dom", function() {
    return gulp
        .src("node_modules/react-dom/dist/react-dom.js")
        .pipe(newer("assets/js/src/vendor/react-dom.js"))
        .pipe(gulp.dest("assets/js/src/vendor"));
});

// Copy assets/js/vendor/* to assets/js
gulp.task("copy-js-vendor", function() {
    return gulp
        .src([
            "assets/js/src/vendor/react.js",
            "assets/js/src/vendor/react-dom.js"
        ])
        .pipe(gulp.dest("assets/js"));
});

// Concatenate jsFiles.vendor and jsFiles.source into one JS file.
// Run copy-react and eslint before concatenating
// gulp.task("concat", ["copy-react", "copy-react-dom", "eslint"], function() {
//     return gulp
//         .src(jsFiles.vendor.concat(jsFiles.source))
//         .pipe(sourcemaps.init())
//         .pipe(
//             babel({
//                 only: ["./src/OrchardCore/**/*.jsx"],
//                 compact: false
//             })
//         )
//         .pipe(concat("app.js"))
//         .pipe(sourcemaps.write("./"))
//         .pipe(gulp.dest("assets/js"));
// });
// Watch JS/JSX files
gulp.task("watch", function() {
    gulp.watch("./src/OrchardCore.DistribWebAPI/modules/**/*.{js,jsx}", [
        "concat"
    ]);
});

// BrowserSync
gulp.task("browsersync", function() {
    browserSync({
        server: {
            baseDir: "./"
        },
        open: false,
        online: false,
        notify: false
    });
});

gulp.task("manufacturingPlanner-prod", function() {
    var basePath =
        __dirname +
        "/src/OrchardCore.DistribWebAPI/Modules/PCCom.Distrib.WebAPI.ManufacturingPlanner";
    console.log(
        "awesome-typescript-loader?tsconfig=" +
            path.join(basePath, "tsconfig.json")
    );
    webpack(
        {
            context: path.join(basePath, "/Assets/ManufacturingPlanner"),
            resolve: {
                extensions: [".js", ".ts", ".tsx"]
            },

            entry: ["./main.tsx", "./index.css", "./tables.css"],
            output: {
                path: path.join(basePath, "/Content/Scripts"),
                filename: "ManufacturingPlanner.min.js"
            },

            devtool: "source-map",

            /*devServer: {
            contentBase: "./dist", // Content base
            inline: true, // Enable watch and live reload
            host: "localhost",
            port: 8080,
            stats: "errors-only"
        },*/

            module: {
                rules: [
                    {
                        test: /\.(ts|tsx)$/,
                        exclude: /node_modules/,
                        use: {
                            loader: "awesome-typescript-loader",
                            options: {
                                useBabel: true,
                                configFileName: path.join(
                                    basePath,
                                    "tsconfig.json"
                                )
                            }
                        }
                    },
                    {
                        test: /\.css$/,
                        include: /node_modules/,
                        loader: ExtractTextPlugin.extract({
                            fallback: "style-loader",
                            use: {
                                loader: "css-loader"
                            }
                        })
                    },
                    {
                        // Transform our own .css files with PostCSS and CSS-modules
                        test: /\.css$/,
                        exclude: /node_modules/,
                        use: ["style-loader", "css-loader"]
                    },
                    {
                        // Do not transform vendor's CSS with CSS-modules
                        // The point is that they remain in global scope.
                        // Since we require these CSS files in our JS or CSS files,
                        // they will be a part of our compilation either way.
                        // So, no need for ExtractTextPlugin here.
                        test: /\.css$/,
                        include: /node_modules/,
                        use: ["style-loader", "css-loader"]
                    },
                    {
                        test: /\.scss$/,
                        loaders: ["style-loader", "css-loader", "sass-loader"]
                    },

                    // Using here url-loader and file-loader
                    {
                        test: /\.(woff|woff2)(\?v=\d+\.\d+\.\d+)?$/,
                        loader:
                            "url-loader?limit=10000&mimetype=application/font-woff"
                    },
                    {
                        test: /\.ttf(\?v=\d+\.\d+\.\d+)?$/,
                        loader:
                            "url-loader?limit=10000&mimetype=application/octet-stream"
                    },
                    {
                        test: /\.svg(\?v=\d+\.\d+\.\d+)?$/,
                        loader: "url-loader?limit=10000&mimetype=image/svg+xml"
                    },
                    {
                        test: /\.eot(\?v=\d+\.\d+\.\d+)?$/,
                        loader: "file-loader"
                    }
                ]
            },
            plugins: [
                // Generate index.html in /dist => https://github.com/ampedandwired/html-webpack-plugin
                new HtmlWebpackPlugin({
                    filename: "index.html", // Name of file in ./dist/
                    template: "index.html", // Name of template in ./src
                    hash: true
                }),
                new ExtractTextPlugin({
                    filename: "[chunkhash].[name].css",
                    disable: false,
                    allChunks: true
                })
            ]
        },
        function(err, stats) {
            console.log(stats);
        }
    );
});
//FIN LO MIO

// Incremental build (each asset group is built only if one or more inputs are newer than the output).
gulp.task("build", ["manufacturingPlanner-prod"], function() {
    var assetGroupTasks = getAssetGroups().map(function(assetGroup) {
        var doRebuild = false;
        return createAssetGroupTask(assetGroup, doRebuild);
    });
    return merge(assetGroupTasks);
});

// Full rebuild (all assets groups are built regardless of timestamps).
gulp.task("rebuild", ["manufacturingPlanner-prod"], function() {
    var assetGroupTasks = getAssetGroups().map(function(assetGroup) {
        var doRebuild = true;
        return createAssetGroupTask(assetGroup, doRebuild);
    });
    return merge(assetGroupTasks);
});

// Continuous watch (each asset group is built whenever one of its inputs changes).
gulp.task("watch", function() {
    var pathWin32 = require("path");
    getAssetGroups().forEach(function(assetGroup) {
        var watchPaths = assetGroup.inputPaths.concat(assetGroup.watchPaths);
        var inputWatcher;
        function createWatcher() {
            inputWatcher = gulp.watch(watchPaths, function(event) {
                var isConcat =
                    path.basename(
                        assetGroup.outputFileName,
                        path.extname(assetGroup.outputFileName)
                    ) !== "@";
                if (isConcat)
                    console.log(
                        "Asset file '" +
                            event.path +
                            "' was " +
                            event.type +
                            ", rebuilding asset group with output '" +
                            assetGroup.outputPath +
                            "'."
                    );
                else
                    console.log(
                        "Asset file '" +
                            event.path +
                            "' was " +
                            event.type +
                            ", rebuilding asset group."
                    );
                var doRebuild = true;
                var task = createAssetGroupTask(assetGroup, doRebuild);
            });
        }
        createWatcher();
        gulp.watch(assetGroup.manifestPath, function(event) {
            console.log(
                "Asset manifest file '" +
                    event.path +
                    "' was " +
                    event.type +
                    ", restarting watcher."
            );
            inputWatcher.remove();
            inputWatcher.end();
            createWatcher();
        });
    });
});

/*
** ASSET GROUPS
*/

function getAssetGroups() {
    var assetManifestPaths = glob.sync(
        "./src/OrchardCore.{Modules,Themes}/*/Assets.json",
        {}
    );
    var ownVendors = glob.sync(
        "./src/OrchardCore.DistribWebAPI/Modules/*/Assets.json",
        {}
    );
    assetManifestPaths = assetManifestPaths.concat(ownVendors);
    var assetGroups = [];
    assetManifestPaths.forEach(function(assetManifestPath) {
        var assetManifest = require("./" + assetManifestPath);
        assetManifest.forEach(function(assetGroup) {
            resolveAssetGroupPaths(assetGroup, assetManifestPath);
            assetGroups.push(assetGroup);
        });
    });
    return assetGroups;
}

function resolveAssetGroupPaths(assetGroup, assetManifestPath) {
    assetGroup.manifestPath = assetManifestPath;
    assetGroup.basePath = path.dirname(assetManifestPath);
    assetGroup.inputPaths = assetGroup.inputs.map(function(inputPath) {
        return path.resolve(path.join(assetGroup.basePath, inputPath));
    });
    assetGroup.watchPaths = [];
    if (!!assetGroup.watch) {
        assetGroup.watchPaths = assetGroup.watch.map(function(watchPath) {
            return path.resolve(path.join(assetGroup.basePath, watchPath));
        });
    }
    assetGroup.outputPath = path.resolve(
        path.join(assetGroup.basePath, assetGroup.output)
    );
    assetGroup.outputDir = path.dirname(assetGroup.outputPath);
    assetGroup.outputFileName = path.basename(assetGroup.output);
    // Uncomment to copy assets to wwwroot
    //assetGroup.webroot = path.join("./src/Orchard.Cms.Web/wwwroot/", path.basename(assetGroup.basePath), path.dirname(assetGroup.output));
}

function createAssetGroupTask(assetGroup, doRebuild) {
    var outputExt = path.extname(assetGroup.output).toLowerCase();
    var doConcat = path.basename(assetGroup.outputFileName, outputExt) !== "@";
    if (doConcat && !doRebuild) {
        // Force a rebuild of this asset group is the asset manifest file itself is newer than the output.
        var assetManifestStats = fs.statSync(assetGroup.manifestPath);
        var outputStats = fs.existsSync(assetGroup.outputPath)
            ? fs.statSync(assetGroup.outputPath)
            : null;
        doRebuild =
            !outputStats || assetManifestStats.mtime > outputStats.mtime;
    }
    switch (outputExt) {
        case ".css":
            return buildCssPipeline(assetGroup, doConcat, doRebuild);
        case ".js":
            return buildJsPipeline(assetGroup, doConcat, doRebuild);
    }
}

/*
** PROCESSING PIPELINES
*/

function buildCssPipeline(assetGroup, doConcat, doRebuild) {
    assetGroup.inputPaths.forEach(function(inputPath) {
        var ext = path.extname(inputPath).toLowerCase();
        if (ext !== ".scss" && ext !== ".less" && ext !== ".css")
            throw "Input file '" +
                inputPath +
                "' is not of a valid type for output file '" +
                assetGroup.outputPath +
                "'.";
    });
    var generateSourceMaps = assetGroup.hasOwnProperty("generateSourceMaps")
        ? assetGroup.generateSourceMaps
        : true;
    var containsLessOrScss = assetGroup.inputPaths.some(function(inputPath) {
        var ext = path.extname(inputPath).toLowerCase();
        return ext === ".less" || ext === ".scss";
    });
    // Source maps are useless if neither concatenating nor transforming.
    if ((!doConcat || assetGroup.inputPaths.length < 2) && !containsLessOrScss)
        generateSourceMaps = false;
    var minifiedStream = gulp
        .src(assetGroup.inputPaths) // Minified output, source mapping completely disabled.
        .pipe(
            gulpif(
                !doRebuild,
                gulpif(
                    doConcat,
                    newer(assetGroup.outputPath),
                    newer({
                        dest: assetGroup.outputDir,
                        ext: ".css"
                    })
                )
            )
        )
        .pipe(plumber())
        .pipe(gulpif("*.less", less()))
        .pipe(
            gulpif(
                "*.scss",
                scss({
                    precision: 10
                })
            )
        )
        .pipe(gulpif(doConcat, concat(assetGroup.outputFileName)))
        .pipe(
            cssnano({
                autoprefixer: { browsers: ["last 2 versions"] },
                discardComments: { removeAll: true },
                discardUnused: false,
                mergeIdents: false,
                reduceIdents: false,
                zindex: false
            })
        )
        .pipe(
            rename({
                suffix: ".min"
            })
        )
        .pipe(eol())
        .pipe(gulp.dest(assetGroup.outputDir));
    // Uncomment to copy assets to wwwroot
    //.pipe(gulp.dest(assetGroup.webroot));
    var devStream = gulp
        .src(assetGroup.inputPaths) // Non-minified output, with source mapping
        .pipe(
            gulpif(
                !doRebuild,
                gulpif(
                    doConcat,
                    newer(assetGroup.outputPath),
                    newer({
                        dest: assetGroup.outputDir,
                        ext: ".css"
                    })
                )
            )
        )
        .pipe(plumber())
        .pipe(gulpif(generateSourceMaps, sourcemaps.init()))
        .pipe(gulpif("*.less", less()))
        .pipe(
            gulpif(
                "*.scss",
                scss({
                    precision: 10
                })
            )
        )
        .pipe(gulpif(doConcat, concat(assetGroup.outputFileName)))
        .pipe(
            header(
                "/*\n" +
                    "** NOTE: This file is generated by Gulp and should not be edited directly!\n" +
                    "** Any changes made directly to this file will be overwritten next time its asset group is processed by Gulp.\n" +
                    "*/\n\n"
            )
        )
        .pipe(gulpif(generateSourceMaps, sourcemaps.write()))
        .pipe(eol())
        .pipe(gulp.dest(assetGroup.outputDir));
    // Uncomment to copy assets to wwwroot
    //.pipe(gulp.dest(assetGroup.webroot));
    return merge([minifiedStream, devStream]);
}

function buildJsPipeline(assetGroup, doConcat, doRebuild) {
    assetGroup.inputPaths.forEach(function(inputPath) {
        var ext = path.extname(inputPath).toLowerCase();
        if (ext !== ".ts" && ext !== ".js")
            throw "Input file '" +
                inputPath +
                "' is not of a valid type for output file '" +
                assetGroup.outputPath +
                "'.";
    });
    var generateSourceMaps = assetGroup.hasOwnProperty("generateSourceMaps")
        ? assetGroup.generateSourceMaps
        : true;
    // Source maps are useless if neither concatenating nor transforming.
    if (
        (!doConcat || assetGroup.inputPaths.length < 2) &&
        !assetGroup.inputPaths.some(function(inputPath) {
            return path.extname(inputPath).toLowerCase() === ".ts";
        })
    )
        generateSourceMaps = false;
    return (
        gulp
            .src(assetGroup.inputPaths)
            .pipe(
                gulpif(
                    !doRebuild,
                    gulpif(
                        doConcat,
                        newer(assetGroup.outputPath),
                        newer({
                            dest: assetGroup.outputDir,
                            ext: ".js"
                        })
                    )
                )
            )
            .pipe(plumber())
            .pipe(gulpif(generateSourceMaps, sourcemaps.init()))
            .pipe(
                gulpif(
                    "*.ts",
                    typescript({
                        declaration: false,
                        noImplicitAny: true,
                        noEmitOnError: true,
                        sortOutput: true
                    }).js
                )
            )
            .pipe(gulpif(doConcat, concat(assetGroup.outputFileName)))
            .pipe(
                header(
                    "/*\n" +
                        "** NOTE: This file is generated by Gulp and should not be edited directly!\n" +
                        "** Any changes made directly to this file will be overwritten next time its asset group is processed by Gulp.\n" +
                        "*/\n\n"
                )
            )
            .pipe(gulpif(generateSourceMaps, sourcemaps.write()))
            .pipe(gulp.dest(assetGroup.outputDir))
            // Uncomment to copy assets to wwwroot
            //.pipe(gulp.dest(assetGroup.webroot))
            .pipe(uglify())
            .pipe(
                rename({
                    suffix: ".min"
                })
            )
            .pipe(eol())
            .pipe(gulp.dest(assetGroup.outputDir))
    );
    // Uncomment to copy assets to wwwroot
    //.pipe(gulp.dest(assetGroup.webroot));
}