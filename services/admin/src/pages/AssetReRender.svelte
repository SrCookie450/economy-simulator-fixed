<script lang="ts">
	import { navigate } from "svelte-routing";
	import Main from "../components/templates/Main.svelte";
	import request from "../lib/request";
	let disabled = false;
	let errorMessage: string | undefined;
	import * as rank from "../stores/rank";
	rank.promise.then(() => {
		if (!rank.is("admin")) {
			errorMessage = "You must have the administrator permission to manage re-renders.";
		}
	});
</script>

<svelte:head>
	<title>Request Asset Re-Render</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			<h1>Request Asset Re-Render</h1>
			{#if errorMessage}
				<p class="err">{errorMessage}</p>
			{/if}
		</div>
		<div class="col-6">
			<label for="name">Asset ID</label>
			<input type="text" class="form-control" id="assetid" {disabled} />
		</div>

		<div class="col-12 mt-4">
			<button
				class="btn btn-success"
				{disabled}
				on:click={(e) => {
					e.preventDefault();
					if (disabled) {
						return;
					}
					disabled = true;
					request
						.post("/asset/re-render", { assetId: document.getElementById("assetid").value })
						.then((d) => {
							alert("Render requested");
						})
						.catch((e) => {
							errorMessage = e.message;
						})
						.finally(() => {
							disabled = false;
						});
				}}>Submit</button
			>
		</div>
	</div>
</Main>

<style>
	p.err {
		color: red;
	}
</style>
