const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const PurgecssPlugin = require('purgecss-webpack-plugin');
const path = require('path');
const glob = require('glob')
const sveltePreprocess = require('svelte-preprocess');

const mode = process.env.NODE_ENV;
const prod = mode === 'production';
const distPath = path.join(__dirname, '/public');
console.log('[info] running in mode:', mode);
console.log('[info] prod', prod);

const conf = {
	entry: {
		'build/bundle': ['./src/main.ts']
	},
	resolve: {
		alias: {
			svelte: path.dirname(require.resolve('svelte/package.json'))
		},
		extensions: ['.mjs', '.js', '.ts', '.svelte'],
		mainFields: ['svelte', 'browser', 'module', 'main']
	},
	output: {
		path: distPath,
		filename: '[name].js',
		chunkFilename: '[name].[id].js'
	},
	module: {
		rules: [
			{
				test: /\.ts$/,
				loader: 'ts-loader',
				exclude: /node_modules/
			},
			{
				test: /\.svelte$/,
				use: {
					loader: 'svelte-loader',
					options: {
						compilerOptions: {
							dev: !prod
						},
						emitCss: prod,
						hotReload: !prod,
						preprocess: sveltePreprocess()
					}
				}
			},
			{
				test: /\.css$/,
				use: [
					MiniCssExtractPlugin.loader,
					'css-loader'
				]
			},
			{
				// required to prevent errors from Svelte on Webpack 5+
				test: /node_modules\/svelte\/.*\.mjs$/,
				resolve: {
					fullySpecified: false
				}
			},
		]
	},
	mode,
	optimization: {
		usedExports: true,
	},
	plugins: [
		new MiniCssExtractPlugin({
			filename: '[name].css'
		}),
		new PurgecssPlugin({
			paths: glob.sync(`${distPath}/**/*`, { nodir: true }),
			safelist: [
				'mod-card-black',
				'mod-card-white',
				'mod-card-default',
			],
		}),
	],
	devtool: prod ? false : 'source-map',
	devServer: {
		hot: true
	}
};

if (process.env.ANALYZE) {
	const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;
	conf.plugins.push(new BundleAnalyzerPlugin());
}

module.exports = conf;