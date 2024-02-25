module.exports = {
	env: {
		es2021: true,
		node: true
	},
	ignorePatterns: '*.js',
	extends: [
		'eslint:recommended',
		'plugin:@typescript-eslint/recommended'
	],
	parser: '@typescript-eslint/parser',
	parserOptions: {
		ecmaVersion: 12,
		sourceType: 'module'
	},
	plugins: [
		'@typescript-eslint'
	],
	rules: {
		indent: [
			'error',
			'tab'
		],
		'linebreak-style': [
			'error',
			'windows'
		],
		quotes: [
			'error',
			'single'
		],
		semi: [
			'error',
			'always'
		],
		'quote-props': ['warn', 'as-needed'],
		'explicit-module-boundary-types': 'off',
		'@typescript-eslint/explicit-module-boundary-types': 'off',
		'@typescript-eslint/no-explicit-any': 'off',
		'@typescript-eslint/ban-ts-comment': 'off',
		'@typescript-eslint/no-unused-vars': 'off',
		'no-unused-vars': 'off'
	}
};
