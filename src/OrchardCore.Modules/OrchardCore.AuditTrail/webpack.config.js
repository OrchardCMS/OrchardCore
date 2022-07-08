var path = require('path');

module.exports = {
    entry: "./Assets/js/App.tsx",
    output: {
        path: path.resolve(__dirname, "wwwroot/Scripts"),
        filename: "diffviewer.js"
    },
    resolve: {
        // Add `.ts` and `.tsx` as a resolvable extension.
        extensions: [".ts", ".tsx", ".js", ".mjs"]
    },
    performance: {
      hints: false,
    },
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
            test: /\.s?css$/,
            use:['style-loader','css-loader', 'sass-loader']
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
  };