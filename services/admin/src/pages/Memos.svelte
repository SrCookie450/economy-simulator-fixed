<script lang="ts">
	import * as rank from "../stores/rank";
	import Main from "../components/templates/Main.svelte";
	interface IMemo {
		group: string;
		title: string;
		message: string;
	}
	const memos: IMemo[] = [
		{
			group: "mod",
			title: "Hello World",
			message: "Test 1234",
		},
	];
	let validMemos: any[] = null;
	rank.promise.then(() => {
		validMemos = memos
			.filter((v) => {
				return rank.is(v.group as any);
			})
			.map((v: any) => {
				v.isForGroup = rank.get().toLowerCase() === v.group.toLowerCase();
				return v;
			});
	});
</script>

<style>
	div.sp {
		margin: 0 auto;
		display: block;
	}
</style>

<svelte:head>
	<title>Staff Memos</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			<h1>Memos</h1>
		</div>
		<div class="col-12 mt-2">
			{#if validMemos}
				{#if validMemos.length === 0}
					<p>There are currently <span class="fw-bold">0</span> memos.</p>
				{:else}
					<p>There {validMemos.length === 1 ? "is" : "are"} currently <span class="fw-bold">{validMemos.length.toLocaleString()}</span> memo{validMemos.length > 1 ? "s" : ""}. <span class="fw-bold">{validMemos.filter((v) => v.isForGroup).length.toLocaleString()}</span> apply to your group specifically.</p>
				{/if}
			{:else}
				<div class="spinner-border sp" />
			{/if}
		</div>
	</div>
</Main>
