<script lang="ts">
	import {link} from 'svelte-routing';
	import Main from "../components/templates/Main.svelte";
	import request from "../lib/request";
	let disabled = false;
	let errorMessage: string | undefined;
	import * as rank from "../stores/rank";
	let createdAssetId: number|undefined;
	let force = 'false';
	let didError = false;
</script>

<svelte:head>
	<title>Copy Roblox Asset</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			<h1>Copy Roblox Asset</h1>
			{#if errorMessage}
				<p class="err">{errorMessage}</p>
			{/if}
			{#if createdAssetId !== undefined}
				<p>Link: <a href={`/catalog/${createdAssetId}/--`}>View on site</a></p>
				<p>Product: <a use:link href={`/admin/product/update?assetId=${createdAssetId}`}>Update Product</a></p>
			{/if}
		</div>
		<div class="col-6">
			<label for="url">Roblox URL</label>
			<input type="text" class="form-control" id="url" {disabled} placeholder="Example: https://www.roblox.com/catalog/17238615/Burro-Pinata" />
		</div>
		<div class="col-6">
			{#if didError}
				<label for="force">Force Upload</label>
				<select class="form-control" bind:value={force}>
					<option value="false">No</option>
					<option value="true">Yes</option>
				</select>
			{/if}
		</div>
		<div class="col-6 mt-4">
			<button
				class="btn btn-success"
				{disabled}
				on:click={(e) => {
					e.preventDefault();
					if (disabled) {
						return;
					}
					disabled = true;
					createdAssetId = undefined;
					request
						.post("/asset/copy-from-roblox", {
							// @ts-ignore
							assetId: parseInt(document.getElementById("url").value.match(/[0-9]+/)[0], 10),
							force: force ==='true' ? true : false,
						})
						.then((d) => {
							// window.location.href = `/catalog/${d.data.assetId}/--`;
							createdAssetId = d.data.assetId;
							// reset incase user is going to upload more items
							didError = false;
							force = 'false';
						})
						.catch((e) => {
							errorMessage = e.message;
							didError = true;
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
	input#new-url {
		cursor: pointer;
	}
</style>
