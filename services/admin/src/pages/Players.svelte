<script lang="ts">
	import { link } from "svelte-routing";
	import Confirm from '../components/modal/Confirm.svelte';
	import {chunk} from 'lodash';

	import Main from "../components/templates/Main.svelte";
	import request from "../lib/request";
	import moment from "../lib/moment";
	let playersData = [];
	let sortColumn = "user.id";
	let sortMode = "asc";
	let offset = 0;
	let limit = 10;
	let disabled = false;
	let reason = '';
	let internalReason = '';

    let modalBody: string|undefined;
    let modalCb: (didClickyes: boolean) => void|undefined;
    let modalVisible: boolean = false;

	const resetAllUsernames = async () => {
		let groups = chunk(playersData.filter(v => v.checked), 100);
								for (const items of groups) {
									let promises = [];
									for (const item of items) {
										promises.push(request.request({
											method: 'POST',
											url: `users/${item.id}/reset-username`,
										}));
									}
										await Promise.all(promises);
		}
	}

	const banAllUsers = async () => {
		let groups = chunk(playersData.filter(v => v.checked), 100);
		for (const items of groups) {
			let promises = [];
			for (const item of items) {
				promises.push(request.request({
					method: 'POST',
					url: `ban`,
					data: {
						userId: item.id,
						reason: reason,
						internalReason,
					}
				}))
			}
			await Promise.all(promises);
		}
	}

	const searchUsers = () => {
		disabled = true;
		request
			.get("/users?orderByColumn=" + encodeURIComponent(sortColumn) + "&orderByMode=" + encodeURIComponent(sortMode) + "&limit=" + limit + "&offset=" + offset + "&query=" + encodeURIComponent(searchQuery))
			.then((res) => {
				playersData = res.data.data;
			})
			.finally(() => {
				disabled = false;
			});
	}

	$: {
		searchUsers();
	}
	let searchQuery = "";
</script>

<style>
	.description {
		max-width: 100px;
		max-height: 100px;
		overflow-y: scroll;
	}
	a {
		text-decoration: none;
	}
</style>

<svelte:head>
	<title>Players</title>
</svelte:head>

<Main>
	{#if modalVisible}
		<Confirm
			title="Confirm"
			message={modalBody}
			cb={(e) => {
				modalVisible = false;
				modalCb(e);
			}}
		/>
	{/if}
	<div class="row">
		<div class="col-12 col-md-2">
			<h1>Players</h1>
		</div>
		<div class="col-12 col-md-6 col-lg-2">
			<label for="sort-selection">SORT</label>
			<select
				{disabled}
				class="form-control"
				id="sort-selection"
				on:change={(e) => {
					sortMode = e.currentTarget.value;
				}}
			>
				<option value="asc">ASC</option>
				<option value="desc">DESC</option>
			</select>
		</div>
		<div class="col-12 col-md-6 col-lg-2">
			<label for="sort-column">SORT COLUMN</label>
			<select
				{disabled}
				id="sort-column"
				class="form-control"
				on:change={(e) => {
					sortColumn = e.currentTarget.value;
				}}
			>
				<option value="user.id">ID</option>
				<option value="user_economy.balance_robux">RBX</option>
				<option value="user_economy.balance_tickets">TIX</option>
				<option value="user.online_at">Online Time</option>
			</select>
		</div>
		<div class="col-12 col-md-6 col-lg-2">
			<label for="limit">LIMIT</label>
			<select {disabled} id="limit" class="form-control" on:change={(e) => {
				limit = parseInt(e.currentTarget.value, 10);
			}}>
				<option value="10">10</option>
				<option value="50">50</option>
				<option value="100">100</option>
				<option value="1000">1K</option>
				<option value="10000">10K</option>
			</select>
		</div>
		<div class="col-12 col-md-12 col-lg-2">
			<label for="search-username">SEARCH USERNAME</label>
			<input
				{disabled}
				class="form-control"
				type="text"
				bind:value={searchQuery}
				maxlength={32}
				id="search-username"
			/>
		</div>
		<div class="col-12 col-md-12 col-lg-2">
			<p class="mb-0 mt-0">&emsp;</p>
			<button class="btn btn-primary w-100" {disabled} on:click={() => {
				searchUsers();
			}}>Search</button>
		</div>
		{#if playersData && playersData.filter(v => v.checked).length !== 0}
			<div class="col-12">
				<p class="mb-0 fw-bold">Mass Action ({playersData.filter(v => v.checked).length})</p>
				<input bind:value={reason} class="form-control" placeholder="Ban Reason" />
				<textarea bind:value={internalReason} class="form-control" rows={2} placeholder="Internal Reason"></textarea>
				<div class="btn-group">
					<button disabled={!playersData.find(v => v.checked)} class="btn btn-sm btn-outline-danger"
						on:click={() => {
							modalBody = 'Confirm that you want to mass ban these users: ' + ( playersData.filter(v => v.checked).map(v => v.username + ' (ID = ' + v.id + ')').join(', '));
							modalCb = async (t) => {
								if (t) {
									if (reason === '' || reason=== null || !reason) {
										return alert('Please specify a reason.');
									}
									if (disabled) return;
									disabled = true;
									try {
										await banAllUsers();
									}catch(e) {
										alert('Error mass-banning users. Try again. Message = ' + e.message);
									}
									window.location.reload();
								}
							};
							modalVisible = true;
						}}>Ban</button>
					<button disabled={!playersData.find(v => v.checked)} class="btn btn-sm btn-outline-danger" 
					on:click={() => {
						modalBody = 'Confirm that you want to mass reset these names: ' + ( playersData.filter(v => v.checked).map(v => v.username + ' (ID = ' + v.id + ')').join(', '));
                        modalCb = async (t) => {
                            if (t) {
								if (disabled) return;
								disabled = true;
								try {
									await resetAllUsernames();
								}catch(e) {
									alert('Error mass-resetting usernames. Try again. Message = ' + e.message);
								}
								window.location.reload();
                            }
                        };
                        modalVisible = true;
					}}>Name Reset</button>
					<button disabled={!playersData.find(v => v.checked)} class="btn btn-sm btn-outline-danger" on:click={() => {
						modalBody = 'Confirm that you want to mass ban and reset the names of these users: ' + ( playersData.filter(v => v.checked).map(v => v.username + ' (ID = ' + v.id + ')').join(', '));
						modalCb = async (t) => {
							if (t) {
								if (reason === '' || reason === null || !reason) {
									return alert('Please specify a reason.');
								}
								if (internalReason === '' || internalReason === null || !internalReason) {
									return alert('Please specify an internal reason.');
								}
								if (disabled) return;
								disabled = true;
								try {
									await resetAllUsernames();
									await banAllUsers();
								}catch(e) {
									alert('Error mass updating usernames. Try again. Message = ' + e.message);
								}
								window.location.reload();
							}
						};
						modalVisible = true;
					}}>Ban + Name Reset</button>
				</div>
			</div>
		{/if}
		<div class="col-12">
			<table class="table">
				<thead>
					<tr>
						<th><input on:change={(e) => {
							playersData = playersData.map(v => {
								v.checked = e.currentTarget.checked;
								return v;
							})
						}} class="form-check-input" type="checkbox" value="" /></th>
						<th>#</th>
						<th>Name</th>
						<th>Created</th>
						<th>Online</th>
						<th>Status</th>
						<th>Join App</th>
						<th>RBX</th>
						<th>TX</th>
						<th>18+</th>
					</tr>
				</thead>
				<tbody>
					{#each playersData as i}
						<tr>
							<td>
								<input checked={i.checked === true} class="form-check-input" type="checkbox" on:change={(e) => {
									playersData = playersData.map(v => {
										if (v.id === i.id) {
											v.checked = e.currentTarget.checked;
										}
										return v;
									})
								}} />
							</td>
							<td><a use:link href={`/admin/manage-user/${i.id}`}>{i.id}</a></td>
							<td><a use:link href={`/admin/manage-user/${i.id}`}>{i.username}</a></td>
							<td>{moment(i.created_at).format("MMM DD YYYY, h:mm A")}</td>
							<td>{moment(i.online_at).format("MMM DD YYYY, h:mm A")}</td>
							<td>
								{#if i.status === "Ok"}
									<span class="badge bg-success">OK</span>
								{:else if (i.status === 'Deleted' || i.status === 'Forgotten')}
									<span class="badge bg-danger">{i.status}</span>
								{:else}
									<span class="badge bg-warning">{i.status}</span>
								{/if}
							</td>
							<td>
								{#if i.invite_id !== null}
									<span class="badge bg-primary">
										<a class="text-white" href={`/admin/manage-user/${i.invite_author_id}`}>
											Invited
										</a>
									</span>
								{:else if i.join_application_id === null}
									<span class="badge bg-danger">No Join App</span>
								{:else}
									<span class="badge bg-primary">
										<a class="text-white" href={`/admin/applications/${i.join_application_id}`}>
											{i.join_application_status}
										</a>
									</span>
								{/if}
							</td>
							<td>
								R${typeof i.balance_robux === 'number' ? i.balance_robux.toLocaleString() : "0"}
							</td>
							<td>
								T${typeof i.balance_tickets === 'number' ? i.balance_tickets.toLocaleString() : "0"}
							</td>
							<td>
								{#if i.is_18_plus}
								<span class="badge bg-success">Yes</span>
								{:else}
								<span>No</span>
								{/if}
							</td>
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
									searchUsers();
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
					<li class={`page-item${disabled || (playersData && playersData.length < limit) ? " disabled" : ""}`}>
						<a
							class="page-link"
							href="#!"
							on:click={(e) => {
								e.preventDefault();
								offset += limit;
								searchUsers();
							}}>Next</a
						>
					</li>
				</ul>
			</nav>
		</div>
	</div>
</Main>
