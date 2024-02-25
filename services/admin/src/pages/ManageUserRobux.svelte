<script lang="ts">
	import { navigate } from "svelte-routing";
	import Main from "../components/templates/Main.svelte";
	import request from "../lib/request";
	export let userId: string;
	let disabled = false;
	let errorMessage: string | undefined;
</script>

<style>
	p.err {
		color: red;
	}
</style>

<svelte:head>
	<title>Manage Currency</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			<h1>Manage Currency</h1>
			{#if errorMessage}
				<p class="err">{errorMessage}</p>
			{/if}
		</div>
		<div class="col-12">
			<label for="message-subject">Robux Amount</label>
			<input type="text" class="form-control" id="robux-amount" {disabled} />
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
					// @ts-ignore
					let robux = document.getElementById("robux-amount").value;
					disabled = true;
					request
						.post("/giverobux", {
							userId,
							robux,
						})
						.then((d) => {
							navigate("/admin/manage-user/" + userId);
						})
						.catch((e) => {
							errorMessage = e.message;
						})
						.finally(() => {
							disabled = false;
						});
				}}>Give Robux</button
			>
		</div>
		<div class="col-12 mt-4">
			<button
				class="btn btn-danger"
				{disabled}
				on:click={(e) => {
					e.preventDefault();
					if (disabled) {
						return;
					}
					// @ts-ignore
					let robux = document.getElementById("robux-amount").value;
					disabled = true;
					request
						.post("/removerobux", {
							userId,
							robux,
						})
						.then((d) => {
							navigate("/admin/manage-user/" + userId);
						})
						.catch((e) => {
							errorMessage = e.message;
						})
						.finally(() => {
							disabled = false;
						});
				}}>Remove Robux</button>
		</div>
		<div class="col-12">
			<label for="message-subject">Tickets Amount</label>
			<input type="text" class="form-control" id="tickets-amount" {disabled} />
		</div>
		<div class="col-12 mt-4">
			<button
				class="btn btn-warning"
				{disabled}
				on:click={(e) => {
					e.preventDefault();
					if (disabled) {
						return;
					}
					// @ts-ignore
					let amt = document.getElementById("tickets-amount").value;
					disabled = true;
					request
						.post("/givetickets", {
							userId,
							tickets: amt,
						})
						.then((d) => {
							navigate("/admin/manage-user/" + userId);
						})
						.catch((e) => {
							errorMessage = e.message;
						})
						.finally(() => {
							disabled = false;
						});
				}}>Give Tickets</button
			>
		</div>
		<div class="col-12 mt-4">
			<button
				class="btn btn-danger"
				{disabled}
				on:click={(e) => {
					e.preventDefault();
					if (disabled) {
						return;
					}
					// @ts-ignore
					let amt = document.getElementById("tickets-amount").value;
					disabled = true;
					request
						.post("/removetickets", {
							userId,
							tickets: amt,
						})
						.then((d) => {
							navigate("/admin/manage-user/" + userId);
						})
						.catch((e) => {
							errorMessage = e.message;
						})
						.finally(() => {
							disabled = false;
						});
				}}>Remove Tickets</button>
		</div>
	</div>
</Main>
