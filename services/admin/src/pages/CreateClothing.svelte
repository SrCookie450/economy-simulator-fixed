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
</script>

<svelte:head>
	<title>Create Clothing</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			<h1>Create Clothing</h1>
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
			<select class="form-control" id="assettype">
				<option value="11">Shirt</option>
				<option value="12">Pants</option>
				<option value="2">TeeShirt</option>
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
		<div class="col-12 mt-4 mb-4">
			<label for="rbxm">Clothing Texture</label>
			<input type="file" class="form-control" id="texture" />
		</div>
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
					bodyFormData.append("type", document.getElementById("assettype").value);
					bodyFormData.append("genre", document.getElementById("assetgenre").value);
					bodyFormData.append("price", document.getElementById("price").value);
					bodyFormData.append("isForSale", document.getElementById("is_for_sale").checked ? "true" : "false");
					bodyFormData.append("texture", document.getElementById("texture").files[0]);
					disabled = true;
					request
						.post("/asset/create/clothing", bodyFormData)
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
