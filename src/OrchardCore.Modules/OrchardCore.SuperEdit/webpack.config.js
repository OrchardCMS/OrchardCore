const path = require('path');
const CheckerPlugin = require('awesome-typescript-loader').CheckerPlugin;
const CleanWebpackPlugin = require('clean-webpack-plugin');

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);

    const outputDir = (env && env.publishDir)
        ? env.publishDir
        : __dirname;

    return [{
        mode: isDevBuild ? 'development' : 'production',

        devtool: 'inline-source-map',

        stats: { modules: false },

        entry: {
            'app': './Assets/js/App.tsx',
            'main': './Assets/js/Main.tsx',
        },

        watch: true,

        watchOptions: {
            ignored: /node_modules/
        },

        output: {
            filename: "Scripts/[name].js",
            path: path.join(outputDir, 'wwwroot'),
            publicPath: '/'
        },

        resolve: {
            // Add '.ts' and '.tsx' as resolvable extensions.
            extensions: [".ts", ".tsx", ".js", ".jsx"],
            alias: {
                'react-dom': '@hot-loader/react-dom'
            }
        },

        devServer: {
            hot: true
        },

        module: {
            rules: [
                // All files with a '.ts' or '.tsx' extension will be handled by 'awesome-typescript-loader'.
                {
                    test: /\.tsx?$/,
                    include: /js/,
                    exclude: /node_modules/,
                    loader: [
                        {
                            loader: 'awesome-typescript-loader',
                            options: {
                                useCache: true,
                                useBabel: true,
                                babelOptions: {
                                    babelrc: false,
                                    plugins: ['react-hot-loader/babel'],
                                }
                            }
                        }
                    ]
                },
                {
                    test: /\.scss$/,
                    use: [
                        {
                            loader: 'file-loader',
                            options: { name: 'Styles/[name].css', }
                        },
                        { loader: 'extract-loader' },
                        { loader: 'css-loader?-url' },
                        { loader: 'sass-loader' }
                    ]
                }
            ]
        },

        plugins: [
            new CleanWebpackPlugin([path.join(outputDir, 'wwwroot', 'Scripts'), path.join(outputDir, 'wwwroot', 'Styles')]),
            new CheckerPlugin()
        ]
    }];
};
