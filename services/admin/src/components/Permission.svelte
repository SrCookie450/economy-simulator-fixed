<script lang="ts">
	export let p: string;
	let hidden = true;
	import { hasPermission } from "../stores/rank";
	if (hasPermission(p)) {
		console.log("has", p);
		hidden = false;
	}
</script>

{#if hidden !== true}
	<slot />
{/if}
