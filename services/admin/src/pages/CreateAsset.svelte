<script lang="ts">
	import dayjs from "dayjs";

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
	let assetTypeId;
</script>

<svelte:head>
	<title>Create Item</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			<h1>Create Item</h1>
			{#if errorMessage}
				<p class="err">{errorMessage}</p>
			{/if}
		</div>
		<div class="col-6">
			<label for="name">Name</label>
			<input type="text" class="form-control" id="name" {disabled} />
		</div>
		<div class="col-6">
			<label for="description">Description (Optional)</label>
			<input type="text" class="form-control" id="description" />
		</div>
		<div class="col-6">
			<label for="assettype">Type</label>
			<select class="form-control" bind:value={assetTypeId}>
				{#await request.get("/asset/types") then data}
					{#each Object.getOwnPropertyNames(data.data) as element}
						{#if !isNaN(parseInt(element))}
							<option value={element}>{data.data[element]}</option>
						{/if}
					{/each}
				{/await}
			</select>
		</div>
		<div class="col-6">
			<label for="assetgenre">Genre</label>
			<select class="form-control" id="assetgenre">
				{#await request.get("/asset/genres") then data}
					{#each Object.getOwnPropertyNames(data.data) as element}
						{#if !isNaN(parseInt(element))}
							<option value={element}>{data.data[element]}</option>
						{/if}
					{/each}
				{/await}
			</select>
		</div>
		{#if assetTypeId === "32"}
			<div class="col-12 mt-4 mb-4">
				<label for="rbxm">CSV of Package Asset IDs (e.g. "1,2,3")</label>
				<input type="text" class="form-control" id="assetIdsForPackage" />
			</div>
		{:else}
			<div class="col-12 mt-4 mb-4">
				<label for="rbxm">.RBXM File</label>
				<input type="file" class="form-control" id="rbxm" />
			</div>
		{/if}
		<div class="col-12">
			<h3>Economy</h3>
			<div class="row">
				<div class="col-4">
					<label for="name">Price (Optional)</label>
					<input type="text" class="form-control" id="price" {disabled} />
				</div>
				<div class="col-2 mt-4">
					<label for="is_for_sale">For Sale: </label>
					<input type="checkbox" class="form-check-input" id="is_for_sale" />
				</div>
				<div class="col-6">
					<label for="description">Limited Status</label>
					<select class="form-control" id="limited-status">
						<option value="false">Not Limited</option>
						<option value="limited">Limited</option>
						<option value="limited_u">Limited Unique</option>
					</select>
				</div>
				<div class="col-6">
					<label for="description">Max Copy Count (optional)</label>
					<input type="text" class="form-control" id="max-copies" />
				</div>
				<div class="col-6">
					<label for="description">Offsale Time (EST) (optional)</label>
					<input type="text" class="form-control" id="offsale-time" placeholder="Format: YYYY-MM-DD HH:MM:SS" />
				</div>
			</div>
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
					bodyFormData.append("name", document.getElementById("name").value);
					bodyFormData.append("description", document.getElementById("description").value);
					bodyFormData.append("assetTypeId", assetTypeId);
					bodyFormData.append("genre", document.getElementById("assetgenre").value);
					bodyFormData.append("price", document.getElementById("price").value);
					let offsaleTime = document.getElementById("offsale-time").value;
					if (offsaleTime) {
						const v = dayjs(offsaleTime, "YYYY-MM-DD HH:MM:SS");
						if (!v.isValid()) {
							errorMessage = `The offsale time specified is not valid. The format is "YYYY-MM-DD HH:MM:SS"`;
							return;
						}
						bodyFormData.append("offsaleDeadline", v.format());
					}
					let limStatus = document.getElementById("limited-status").value;
					if (limStatus === "limited" || limStatus === "limited_u") {
						bodyFormData.append("isLimited", "true");
					}
					if (limStatus === "limited_u") {
						bodyFormData.append("isLimitedUnique", "true");
					}
					let maxCopies = document.getElementById("max-copies").value;
					if (maxCopies !== "") {
						bodyFormData.append("maxCopies", maxCopies);
					}
					bodyFormData.append("isForSale", document.getElementById("is_for_sale").checked ? "true" : "false");
					if (document.getElementById("rbxm")) {
						bodyFormData.append("rbxm", document.getElementById("rbxm").files[0]);
					}
					if (assetTypeId === "32") {
						bodyFormData.append("packageAssetIds", document.getElementById('assetIdsForPackage').value);
					}
					disabled = true;
					request
						.post("/asset/create", bodyFormData)
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
