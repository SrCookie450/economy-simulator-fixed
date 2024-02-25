<script lang="ts">
	export let key: string;
	export let value: string;
	export let cardClasses = "bg-dark";
	export let onClick = null;
</script>

<div class="col-12 col-md-6 col-lg-3 mt-4" on:click={onClick}>
	<div class={`card ${cardClasses} text-white`}>
		<div class="card-body p-0 pb-3 pl-3">
			{#if key}
				<h1 class="m-0 pt-2 pb-2 text-truncate" style="font-size: 4rem;">{key}</h1>
			{:else}
				<p style="margin-top: 1rem;" />
			{/if}
			<p class="m-0 text-truncate" style="font-size: 0.8rem;">{value}</p>
		</div>
	</div>
</div>
