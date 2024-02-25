<script lang="ts">
	import { link, navigate, Router } from "svelte-routing";
	import Main from "../components/templates/Main.svelte";
	import request from "../lib/request";
	import * as rank from "../stores/rank";
	let data;
	let offset = 0;
	let limit = 10;
	let disabled = false;
	$: {
		disabled = true;
		request
			.get(`/logs?logType=${logType}&offset=${offset}&limit=${limit}`)
			.then((res) => {
				data = res.data;
			})
			.finally(() => {
				disabled = false;
			});
	}

	let logType = "ban";
</script>

<svelte:head>
	<title>Mod Logs</title>
</svelte:head>

{#await rank.promise then _}
	<Main>
		<div class="row">
			<div class="col-12 col-md-6">
				<h1>Moderation Logs</h1>
			</div>
			<div class="col-12 col-md-6">
				<label for="sort-selection">LOG TYPE</label>
				<select
					{disabled}
					class="form-control"
					id="sort-selection"
					on:change={(e) => {
						logType = e.currentTarget.value;
						offset = 0;
					}}
				>
					<option value="ban">BAN</option>
					<option value="unban">UNBAN</option>
					<option value="robux">ROBUX</option>
					<option value="tickets">TICKETS</option>
					<option value="item">ITEMS</option>
					<option value="alert">ALERT</option>
					<option value='applications'>APPLICATIONS</option>
					<option value='asset'>ASSET APPROVAL</option>
					<option value='message'>ADMIN MESSAGE</option>
					<option value='product'>PRODUCT UPDATE</option>
					<option value='refund'>REFUND</option>
				</select>
			</div>
			{#if data && data.data}
				<div class="col-12">
					<table class="table">
						<thead>
							<tr>
								{#each data.columns as col}
									<th>{col}</th>
								{/each}
							</tr>
						</thead>
						<tbody>
							{#each data.data as i}
								<tr>
									{#each Object.getOwnPropertyNames(i) as col}
										{#if col === "user_id" || col === "author_user_id" || col === "actor_id" || col === "actor_user_id"}
											<td>
												<a use:link href={`/admin/manage-user/${i[col]}`}>
													{i[col]}
												</a>
											</td>
										{:else if col === "asset_id"}
											<td>
												<a target='_blank' href={`/admin/product/update?assetId=${i[col]}`} use:link>
													{i[col]}
												</a>
											</td>
										{:else}
											<td>{i[col]}</td>
										{/if}
									{/each}
								</tr>
							{/each}
						</tbody>
					</table>
				</div>
				<div class="col-12">
					<nav aria-label="Page navigation example">
						<ul class="pagination">
							<li class={`page-item${disabled || !offset ? " disabled" : ""}`}>
								<a
									class="page-link"
									href="#!"
									on:click={(e) => {
										e.preventDefault();
										if (offset >= limit) {
											offset -= limit;
										}
									}}>Previous</a
								>
							</li>
							<li class="page-item active">
								<a
									class="page-link"
									href="#!"
									on:click={(e) => {
										e.preventDefault();
									}}>{(offset / limit + 1).toLocaleString()}</a
								>
							</li>
							<li class={`page-item${disabled || (data && data.data.length < limit) ? " disabled" : ""}`}>
								<a
									class="page-link"
									href="#!"
									on:click={(e) => {
										e.preventDefault();
										offset += limit;
									}}>Next</a
								>
							</li>
						</ul>
					</nav>
				</div>
			{/if}
		</div>
	</Main>
{/await}
