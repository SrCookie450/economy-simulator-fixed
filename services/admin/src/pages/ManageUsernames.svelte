<script lang="ts">
	import Main from "../components/templates/Main.svelte";
	import request from "../lib/request";
	let usernames: string[] = null;
	$: {
		if (!usernames) {
			request
				.get("/user/usernames?userId=" + userId)
				.then((d) => {
					usernames = d.data;
				})
				.finally(() => {});
		}
	}
	export let userId: string;

	let errorMessage: string | undefined;

	import * as rank from "../stores/rank";
	rank.promise.then(() => {
		if (!rank.is("admin")) {
			errorMessage = "You must have the administrator permission to perform this action.";
		}
	});
</script>

<style>
	p.err {
		color: red;
	}
</style>

<svelte:head>
	<title>Manage Usernames</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			{#if errorMessage}
				<p class="err">{errorMessage}</p>
			{/if}
		</div>
		<div class="col-12">
			<h2>Delete Username</h2>
			{#if usernames && usernames.length != 0}
				<select class="form-control" id="usernameToDelete">
					{#each usernames as username}
						<option value={username}>{username}</option>
					{/each}
				</select>
				<button
					class="btn btn-danger mt-2"
					on:click={(e) => {
						request
							.post("/user/usernames/delete", {
								userId: userId,
								username: document.getElementById("usernameToDelete").value,
							})
							.then(() => {
								alert("Username has been deleted");
							})
							.catch((e) => {
								errorMessage = (e && e.message) || "Could not delete username";
							});
					}}>Delete</button
				>
			{:else if usernames && usernames.length === 0}
				<p>No usernames available</p>
			{:else}
				<p>Loading Names...</p>
			{/if}
		</div>
	</div>
</Main>
