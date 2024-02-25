<script lang="ts">
	import Main from "../components/templates/Main.svelte";
	import request from "../lib/request";
	import { link } from "svelte-routing";

	export let userAssetId = 11;
	let history = [];
	$: {
		history = null;
		request.get("/trackitem?userAssetId=" + encodeURIComponent(userAssetId)).then((data) => {
			history = data.data;
		});
	}
	import moment from "moment";

	let errorMessage: string | undefined;
</script>

<style>
	p.err {
		color: red;
	}
</style>

<svelte:head>
	<title>Track User Asset</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			{#if errorMessage}
				<p class="err">{errorMessage}</p>
			{/if}
		</div>
		<div class="col-12">
			<h2>Track Userasset #{userAssetId}</h2>
		</div>
		<div class="col-12">
			{#if history !== null}
				<table class="table">
					<thead>
						<tr>
							<td>Date</td>
							<td>Description</td>
							<td>From</td>
							<td>To</td>
						</tr>
					</thead>
					<tbody>
						{#each history as entry}
							<tr>
								<td>{moment(entry.created_at).fromNow()} <br />{moment(entry.created_at).format("dddd, MMMM Do, YYYY h:mma")}</td>
								<td>
									{#if entry.track_type === "Sale"}
										<p>User "{entry.user_one_username}" purchased from "{entry.user_two_username}" for {entry.amount.toLocaleString()} {entry.currency_type === 1 ? "Robux" : "Tix"}</p>
									{:else if entry.track_type === "Trade"}
										<p>User "{entry.user_one_username}" traded the item to "{entry.user_two_username}" - TradeID #{entry.id}</p>
									{:else if entry.track_type === "ModerationGive"}
										{#if entry.user_id_from === null}
											<p>Moderator "{entry.author_username}" created the item and gave it to "{entry.to_username}"</p>
										{:else}
											<p>Moderator "{entry.author_username}" transferred the item from "{entry.from_username}" to "{entry.to_username}"</p>
										{/if}
									{:else}
										<p>Unknown Type "{entry.track_type}"</p>
									{/if}
								</td>

								{#if entry.user_id_two || entry.user_id_from}
									<td><a use:link href={`/admin/manage-user/${entry.user_id_two || entry.user_id_from}`}>{entry.user_two_username || entry.from_username || "-"}</a></td>
									<td><a use:link href={`/admin/manage-user/${entry.user_id_one || entry.user_id_to}`}>{entry.user_one_username || entry.to_username || "-"}</a></td>
								{:else}
									<td />
									<td />
								{/if}
							</tr>
						{/each}
					</tbody>
				</table>
			{:else}
				<div class="row">
					<div class="col-12 text-center">
						<div class="spinner-border sp" />
					</div>
				</div>
			{/if}
		</div>
	</div>
</Main>
