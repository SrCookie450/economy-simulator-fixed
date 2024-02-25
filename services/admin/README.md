# Admin

This is the management frontend created with [svelte](https://svelte.dev/), [svelte-routing](https://www.npmjs.com/package/svelte-routing), and [bootstrap v5.0.0-beta1](https://getbootstrap.com/docs/5.0/getting-started/introduction/). Backend-related tasks can be found in the `services/api/src/controllers/admin-api.ts` controller.

## Get Started

Run `npm run dev` to start the development webpack builder. The service doesn't host itself though - you have to be running the development API server, and then navigate to [localhost/admin](http://localhost/admin/) to see the service.

## Deploy

Deploying is simple. Just run `npm run build`, then commit everything. There will be merge conflicts inevitably with this method, however, it works for now (until S3 deployment is setup).
