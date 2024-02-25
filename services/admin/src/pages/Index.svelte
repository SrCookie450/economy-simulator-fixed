<script lang="ts">
	import {navigate, link} from 'svelte-routing';
	import Main from "../components/templates/Main.svelte";
	import request from "../lib/request";
	import DashStatCard from "../components/DashStatCard.svelte";
	let usersJoinedPastHour: number;
	let usersJoinedPastDay: number;
	let usersOnline: number;
	let usersInGame: {user_id: number; username: string; asset_id: number; asset_name: string;}[] | undefined;
	let numPendingAssets: number|undefined;
	let numPendingText: number|undefined;

	Promise.all([
		request.get(`/groups/pending-icons`),
		request.get(`/assets/pending-assets`),
		request.get(`/icons/pending-assets`),
	]).then(d => {
		let count = 0;
		for (const result of d) {
			count += result.data.length;
		}
		numPendingAssets = count;
	})

	let showInGame = false;
	if (rank.hasPermission("GetUserJoinCount")) {
        request.get<{ total: number }>("/user-joins?period=past-hour").then((data) => {
            usersJoinedPastHour = data.data.total;
        });
		request.get<{ total: number }>("/user-joins?period=past-day").then((data) => {
			usersJoinedPastDay = data.data.total;
		});
	}
	if (rank.hasPermission('GetUsersInGame')) {
		request.get('/players/in-game').then(data => {
			usersInGame = data.data;
		})
	}
	if (rank.hasPermission('GetUsersOnline')) {
		request.get('/players/online-count').then(t => {
			usersOnline = t.data.total;
		})
	}
	import * as rank from "../stores/rank";
	import Permission from "../components/Permission.svelte";
import { get } from 'svelte/store';
import dayjs from 'dayjs';
import { now } from 'svelte/internal';
</script>

<svelte:head>
	<title>Dashboard</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			<h1>Dashboard</h1>
		</div>
	</div>

	<div class="row">
		<Permission p="GetUserJoinCount">
			<DashStatCard key={((typeof usersJoinedPastHour === "number" && usersJoinedPastHour.toLocaleString()) || "-") + '/' + ((typeof usersJoinedPastDay === "number" && usersJoinedPastDay.toLocaleString()) || "-")} value="Users Signed Up, Past Hour and Day" cardClasses="bg-success bg-gradient" />
			<DashStatCard onClick={() => {
				showInGame = !showInGame;
			}} key={((typeof usersOnline === "number" && usersOnline.toLocaleString()) || "-") + '/' + ((usersInGame && usersInGame.length.toLocaleString()) || "-")} value="Users Online and In-Game" cardClasses="bg-primary bg-gradient pointer" />
		</Permission>
		{#if numPendingAssets != undefined}
			<DashStatCard onClick={() => {
				// window.location.href = '/admin/asset/approval';
				navigate('/admin/asset/approval');
			}} value='Num Pending Assets' key={numPendingAssets.toLocaleString()} cardClasses="bg-danger bg-gradient pointer" />
		{/if}
	</div>
	{#if showInGame}
		<div class="row mt-4">
			<div class="col-12">
				<hr />
				<h3 class="mt-4 mb-4">Users In-Game</h3>
			</div>
			{#if usersInGame.length === 0}
				<div class="col-12">
					<p>No users in game :(</p>
				</div>
			{/if}
			{#each usersInGame as data}
				<div class="col-12 col-md-6 col-lg-4">
					<div class="card card-body">
						<div class="row">
							<div class="col-6 col-md-3">
								<img class="avatar-thumb" src={`/Thumbs/Avatar.ashx?height=420&width=420&userid=${data.user_id}&v=0`} alt='User avatar' />
							</div>
							<div class="col-6 col-md-9">
								<p><a href={`/users/${data.user_id}/profile`}>{data.username}</a></p>
								<p>Game: <a href={`/games/${data.asset_id}/--`}>{data.asset_name}</a></p>
							</div>
						</div>
					</div>
				</div>
			{/each}
		</div>
	{/if}
	<div class="row">
		<Permission p="SetAlert">
			<div class="col-12 mt-4">
				<div class="card">
					<div class="card-body">
						<h3>Manage Site-Wide Alert</h3>
						<p>To clear the alert, empty the text box and press submit.</p>
						{#await request.get("/alert") then alertInfo}
							<input placeholder="Alert Text" class="form-control" type="text" id="sitewide-alert-text" value={alertInfo.data.Text} />
							<input placeholder="Alert URL" class="form-control" type="text" id="sitewide-alert-url" value={alertInfo.data.LinkUrl} />
							<button
								class="btn btn-success mt-2"
								on:click={(e) => {
									e.preventDefault();
									let msg = document.getElementById("sitewide-alert-text");
									let url = document.getElementById("sitewide-alert-url");
									request
										.post("/alert", {
											// @ts-ignore
											text: msg.value,
											// @ts-ignore
											url: url.value,
										})
										.then(() => {
											alert("Site-wide updated!");
										})
										.finally(() => {});
								}}>Submit</button
							>
						{/await}
					</div>
				</div>
			</div>
		</Permission>
	</div>
</Main>

