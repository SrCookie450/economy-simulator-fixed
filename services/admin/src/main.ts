import 'bootstrap/dist/css/bootstrap.css';
import './global.css';
import './bs5.css';

import App from './App.svelte';
import rank from './stores/rank';

const app = new App({
	target: document.body,
	props: {},
});

export default app;
