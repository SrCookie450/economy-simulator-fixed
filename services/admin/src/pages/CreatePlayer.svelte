<script lang="ts">
	import { navigate } from "svelte-routing";
	import Main from "../components/templates/Main.svelte";
	import request from "../lib/request";
	let disabled = false;
	let errorMessage: string | undefined;
	import * as rank from "../stores/rank";
	rank.promise.then(() => {
		if (!rank.is("admin")) {
			errorMessage = "You must have the administrator permission to create a player.";
		}
	});
</script>

<svelte:head>
	<title>Create Player</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			<h1>Create Player</h1>
			{#if errorMessage}
				<p class="err">{errorMessage}</p>
			{/if}
		</div>
		<div class="col-12">
			<label for="user-id">User ID (Must not already be taken; leave blank for any id)</label>
			<input type="text" class="form-control" id="user-id" {disabled} />
		</div>
		<div class="col-12">
			<label for="user-id">Username (Must not already be taken)</label>
			<input type="text" class="form-control" id="username" {disabled} />
		</div>
		<div class="col-12">
			<label for="user-id">Password</label>
			<input type="password" class="form-control" id="password" {disabled} />
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
						.post("/create-user", {
							// @ts-ignore
							userId: document.getElementById("user-id").value || null,
							// @ts-ignore
							username: document.getElementById("username").value,
							// @ts-ignore
							password: document.getElementById("password").value,
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
				}}>Create Player</button
			>
		</div>
	</div>
</Main>

<style>
	p.err {
		color: red;
	}
</style>
