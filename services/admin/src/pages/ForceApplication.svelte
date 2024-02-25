<script lang="ts">
	import { navigate } from "svelte-routing";
	import Main from "../components/templates/Main.svelte";
	import request from "../lib/request";
	let disabled = false;
	let errorMessage: string | undefined;
	import * as rank from "../stores/rank";
</script>

<svelte:head>
	<title>Force Application</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			<h1>Force Application</h1>
			{#if errorMessage}
				<p class="err">{errorMessage}</p>
			{/if}
		</div>
		<div class="col-12">
			<label for="user-id">User ID (To force the application on)</label>
			<input type="text" class="form-control" id="user-id" {disabled} />
		</div>
		<div class="col-12">
			<label for="user-id">Social URL (The Signup URL)</label>
			<input type="text" class="form-control" id="social-url" {disabled} />
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
						.post("/force-application", {
							// @ts-ignore
							userId: document.getElementById("user-id").value,
							// @ts-ignore
							socialURL: document.getElementById("social-url").value,
							// @ts-ignore
						})
						.then((d) => {
							let id = document.getElementById("user-id").value
							navigate(`/admin/manage-user/${id}`);
						})
						.catch((e) => {
							errorMessage = e.message;
						})
						.finally(() => {
							disabled = false;
						});
				}}>Force Application</button
			>
		</div>
	</div>
</Main>

<style>
	p.err {
		color: red;
	}
</style>
