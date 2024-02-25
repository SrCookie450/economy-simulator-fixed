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
	let newUrl: string = '';
	let newId: string = '';
</script>

<svelte:head>
	<title>Create Item Asset</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			<h1>Create Item Asset</h1>
			<p>game:GetService("InsertService"):LoadAsset(123).Parent = game.Workspace;</p>
			{#if errorMessage}
				<p class="err">{errorMessage}</p>
			{/if}
		</div>
		<div class="col-12">
			<label for="user-id">Roblox URL</label>
			<input type="text" class="form-control" id="url" {disabled} />
		</div>
		<div class="col-6">
			<label for="user-id">New URL (Click to Copy)</label>
			<input
				on:click={(e) => {
					/** @type {HTMLInputElement} */
					let i = document.getElementById("new-url");
					i.focus();
					// @ts-ignore
					i.select();
					document.execCommand('copy');
				}}
				type="text"
				class="form-control"
				id="new-url"
				value={newUrl}
			/>
		</div>
		<div class="col-6">
			<label>New ID (Click to Copy)</label>
			<input
				on:click={(e) => {
					/** @type {HTMLInputElement} */
					let i = document.getElementById("new-id");
					i.focus();
					// @ts-ignore
					i.select();
					document.execCommand('copy');
				}}
				type="text"
				class="form-control"
				id="new-id"
				value={newId}
			/>
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
					newUrl = '';
					newId = '';
					request
						.post("/asset/create/from-roblox", {
							// @ts-ignore
							url: document.getElementById("url").value,
						})
						.then((d) => {
							newUrl = `http://www.roblox.com/asset/?id=${d.data.assetId}`;
							newId = d.data.assetId.toString();
							errorMessage = undefined;
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
	input#new-url {
		cursor: pointer;
	}
</style>
