import path from 'path';
import { fileURLToPath } from "url";
import webpack from 'webpack';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

export default {
    mode: "production",
    entry: path.resolve(__dirname, "./js/App.tsx"),
    output: {
        path: path.resolve(__dirname, "../wwwroot/Scripts"),
        filename: "graphiql-orchard.js"
    },
    resolve: {
        // Add `.ts` and `.tsx` as a resolvable extension.
        extensions: [".ts", ".tsx", ".js", ".mjs"]
    },
    performance: {
        hints: false,
    },
    plugins: [
        new webpack.optimize.LimitChunkCountPlugin({
          maxChunks: 1,
        }),
    ],
    module: {
        rules: [
            {
                test: /\.(ts|tsx)$/,
                exclude: /node_modules/,
                use: {
                    loader: "ts-loader"
                }
            },
            {
                test: /\.css$/,
                use: ['style-loader', 'css-loader']
            },
            {
                test: /\.mjs$/,
                include: /node_modules/,
                type: "javascript/auto"
            },
            {
                test: /\.flow$/,
                use: 'null-loader'
            }
        ]
    }
}
