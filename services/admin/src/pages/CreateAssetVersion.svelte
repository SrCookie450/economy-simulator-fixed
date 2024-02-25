<script lang="ts">
	import { navigate } from "svelte-routing";
	import Main from "../components/templates/Main.svelte";
	import request from "../lib/request";
	let disabled = false;
	let errorMessage: string | undefined;
	import * as rank from "../stores/rank";
	rank.promise.then(() => {
		if (!rank.is("admin")) {
			errorMessage = "You must have the administrator permission to do this.";
		}
	});
</script>

<svelte:head>
	<title>Create Asset Version</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			<h1>Create Asset Version</h1>
			{#if errorMessage}
				<p class="err">{errorMessage}</p>
			{/if}
		</div>
		<div class="col-6">
			<label for="name">Asset ID</label>
			<input type="text" class="form-control" id="assetid" {disabled} />
		</div>
		<div class="col-12 mt-4 mb-4">
			<label for="rbxm">.RBXM File</label>
			<input type="file" class="form-control" id="rbxm" />
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
					let bodyFormData = new FormData();
					bodyFormData.append("assetId", document.getElementById("assetid").value);
					bodyFormData.append("rbxm", document.getElementById("rbxm").files[0]);
					disabled = true;
					request
						.post("/asset/version/create", bodyFormData)
						.then((d) => {
							window.location.href = `/catalog/${d.data.assetId}/--`;
						})
						.catch((e) => {
							errorMessage = e.message;
						})
						.finally(() => {
							disabled = false;
						});
				}}>Create Asset</button
			>
		</div>
	</div>
</Main>

<style>
	p.err {
		color: red;
	}
</style>
